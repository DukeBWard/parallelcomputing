//Luke Ward
using Extensions;
using jsonObjects;
using Newtonsoft.Json;
using PrimeGen;
using System.Numerics;
using System.Text;

namespace secureMessaging
{
    public class secureMessaging
    {
        static async Task Main(String[] args)
        {
            Keys keys = new();
            Messaging msg = new();

            switch (args[0])
            {
                case "keyGen":
                    try
                    {
                        keys.genKey(Int32.Parse(args[1]));
                    }
                    catch
                    {
                        help(2);
                    }
                    break;
                case "sendKey":
                    try
                    {
                        await keys.sendKeyAsync(args[1]);
                    }
                    catch
                    {
                        help(3);
                    }
                    break;
                case "getKey":
                    try
                    {
                        await keys.getKeyAsync(args[1]);
                    }
                    catch
                    {
                        help(4);
                    }
                    break;
                case "sendMsg":
                    try
                    {
                        if (args.Length > 2)
                        {
                            var message = String.Join(' ', args.Skip(2).ToArray());
                            await msg.sendMsgAsync(args[1], message);
                        }
                        else
                        {
                            throw new Exception();
                        }
                    }
                    catch
                    {
                        help(5);
                    }
                    break;
                case "getMsg":
                    try
                    {
                        await msg.getMsgAsync(args[1]);
                    }
                    catch
                    {
                        help(6);
                    }
                    break;
                default:
                    help(-1);
                    break;
            }
        }

        private static void help(int errorCode)
        {
            switch (errorCode)
            {
                case 0:
                    Console.WriteLine("Passed too few arguments for the program");
                    break;
                case 1:
                    Console.WriteLine("Passed invalid arguments");
                    break;
                case 2:
                    Console.WriteLine("Usage: keyGen <size>\n" +
                        "- size - Combined size of the keys to be generated");
                    break;
                case 3:
                    Console.WriteLine("Usage: sendKey <email>\n" +
                        "- email - Email address for key to be sent to");
                    break;
                case 4:
                    Console.WriteLine("Usage: getKey <email>\n" +
                        "- email - Email address for key to be retrieved from");
                    break;
                case 5:
                    Console.WriteLine("Usage: sendMsg <email> <plaintext>\n" +
                        "- email - Email address for message to be sent to\n" +
                        "- plaintext - Message to be sent to email address");
                    break;
                case 6:
                    Console.WriteLine("Usage: sendKey <email>\n" +
                        "- email - Email address for message to be sent retrieved from\n");
                    break;
                default:
                    Console.WriteLine("Unrecognized input");
                    break;
            }
        }
    }

    public class Messaging
    {
        /* <summary>
        *  Sends message to a specified user.
        * </summary>
        * <param name="email">Email to send the message too.</param>
        * <param name="message">Plaintext message to be sent.</param>
        */
        public async Task sendMsgAsync(string email, string message)
        {
            keyJsonPub key = new();
            try
            {
                key.readKey(email);
                try
                {
                    var msgBytes = Encoding.ASCII.GetBytes(message);

                    BigInteger msgInt = new(msgBytes);
                    msgInt = codeMsg(msgInt, key.decodeKey());

                    var codedMsg = msgInt.ToByteArray();
                    var byteMsg = Convert.ToBase64String(codedMsg);
                    messageJson msgJson = new();
                    msgJson.fill(email, byteMsg);

                    var client = new HttpClient();
                    string serialized = JsonConvert.SerializeObject(msgJson);
                    var content = new StringContent(serialized, Encoding.UTF8, "application/json");

                    var response = await client.PutAsync($"http://kayrun.cs.rit.edu:5000/Message/{email}", content);
                    Console.WriteLine("Message Written");
                }
                catch
                {
                    Console.WriteLine("Message cannot be sent");
                }
            }
            catch
            {
                Console.WriteLine($"Key does not exist for {email}");
            }
        }

        /* <summary>
        * Gets message from the server for a specified user.
        * </summary>
        * <param name="email">Email to retrieve messages for.</param>
        */
        public async Task getMsgAsync(string email)
        {
            var client = new HttpClient();
            try
            {
                keyJsonPriv key = new();
                key.readKey();

                if (key.email.Contains(email))
                {
                    var response = await client.GetAsync($"http://kayrun.cs.rit.edu:5000/Message/{email}");
                    var rawContent = await response.Content.ReadAsStringAsync();

                    var deserialized = JsonConvert.DeserializeObject<messageJson>(rawContent);
                    byte[] stringMsg = Convert.FromBase64String(deserialized.content);
                    var bigIntMsg = new BigInteger(stringMsg);

                    var decodedMsg = codeMsg(bigIntMsg, key.decodeKeys());
                    var msgBytes = decodedMsg.ToByteArray();
                    Console.WriteLine(Encoding.UTF8.GetString(msgBytes));
                }
                else
                {
                    throw new Exception();
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("Message cant be decoded");
            }
        }

        private BigInteger codeMsg(BigInteger msgInt, BigInteger[] decoded)
        {
            // decoded 1 = N 
            // decoded 0 = D o E
            BigInteger ED =  new BigInteger(decoded[0].ToByteArray(),true);
            BigInteger N = new BigInteger(decoded[1].ToByteArray(),true);
            return BigInteger.ModPow(msgInt, ED, N);
        }
    }

    public class Keys
    {
        /* <summary>
        * Generates key of given size.
        * </summary>
        * <param name="size">Size in bits for the key to be generated.</param>
        */
        public void genKey(int size)
        {
            // Splits the key
            var Rand = new Random();
            var v = Rand.Next(80, 120);
            size = (int)((size / 2) * (v / 100.0));

            // Generate p and q
            GeneratePrimes generator = new();
            var p = generator.genAndCheck(size / 8);
            var bitLength = (int)p.GetBitLength();
            var q = generator.genAndCheck(bitLength / 8);

            //Generate nonce
            var n = p * q;

            // Generate Eueler's totient n
            var r = (p - 1) * (q - 1);

            // Generate a 2^16 number aka 2 bytes
            Int32 E = (int)generator.genAndCheck(2);

            // Create d via modinverse
            BigInteger d = Extension.modInverse(E, r);

            // Encode these keys
            encodeKeys(E, d, n);
        }

        /* <summary>
        * Gets the key for a specified user and saves it.
        * </summary>
        * <param name="email">Email of the user who owns the key.</param>
        */
        public async Task getKeyAsync(string email)
        {
            // http://kayrun.cs.rit.edu:5000/Key/jsb@cs.rit.edu
            var client = new HttpClient();
            try
            {
                var response = await client.GetAsync($"http://kayrun.cs.rit.edu:5000/Key/{email}");
                var rawContent = await response.Content.ReadAsStringAsync();
                writeKeys(rawContent, $"{email}.key");
            }
            catch (HttpRequestException)
            {
                Console.WriteLine("No key could be retrieved");
            }
        }

        /* <summary>
        *  Sends key for a specified user to the server.
        * </summary>
        * <param name="email">Email to current public key to.</param>
        */
        public async Task sendKeyAsync(string email)
        {
            keyJsonPub pubKey = new();
            pubKey.readKey("public");

            pubKey.email = email;

            string serialized = JsonConvert.SerializeObject(pubKey);
            var client = new HttpClient();
            var content = new StringContent(serialized, Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PutAsync($"http://kayrun.cs.rit.edu:5000/Key/{email}", content);

                keyJsonPriv privKey = new();
                privKey.readKey();
                privKey.email.Add(email);

                serialized = JsonConvert.SerializeObject(privKey);
                writeKeys(serialized, "private.key");
                Console.WriteLine("Key Saved");
            }
            catch (HttpRequestException)
            {
                Console.WriteLine("Key could not be sent");
            }
        }

        private void encodeKeys(BigInteger E, BigInteger D, BigInteger N)
        {
            var bigEBytes = E.ToByteArray();
            var bigDBytes = D.ToByteArray();
            var bigNBytes = N.ToByteArray();

            var eBytes = BitConverter.GetBytes(bigEBytes.Length).Reverse().ToArray();
            var dBytes = BitConverter.GetBytes(bigDBytes.Length).Reverse().ToArray();
            var nBytes = BitConverter.GetBytes(bigNBytes.Length).Reverse().ToArray();

            var arrayOfArraysPriv = new[] { dBytes, bigDBytes, nBytes, bigNBytes };
            var arrayOfArraysPub = new[] { eBytes, bigEBytes, nBytes, bigNBytes };

            var combinedPriv = Extension.Combine(arrayOfArraysPriv);
            var combinedPub = Extension.Combine(arrayOfArraysPub);

            var encodedPrivKey = Convert.ToBase64String(combinedPriv);
            var encodedPubKey = Convert.ToBase64String(combinedPub);

            keyJsonPriv privKey = new();
            privKey.Key = encodedPrivKey;

            keyJsonPub pubKey = new();
            pubKey.Key = encodedPubKey;

            string jsonResultPriv = JsonConvert.SerializeObject(privKey);
            string jsonResultPub = JsonConvert.SerializeObject(pubKey);

            writeKeys(jsonResultPriv, "private.key");
            writeKeys(jsonResultPub, "public.key");
        }

        private void writeKeys(string jsonObject, string name)
        {
            using (var sw = new StreamWriter(name))
            {
                sw.WriteLine(jsonObject);
            }
        }
    }
}