//Luke Ward
using Newtonsoft.Json;
using System.Numerics;

namespace jsonObjects
{
    internal class keyJsonPriv
    {
        public List<string> email { get; set; } = new List<string>();
        public string? Key { get; set; }

        /* <summary>
         * Method to read the private key from the current directory and save it to the instance of keyJsonPriv called
         * </summary>
         */
        public void readKey()
        {
            keyJsonPriv key = new();
            string name = "private.key";
            using (var sr = new StreamReader(name))
            {
                string? keyString = sr.ReadLine();

                key = JsonConvert.DeserializeObject<keyJsonPriv>(keyString);
                this.email = key.email;
                this.Key = key.Key;
            }
        }

        /* <summary>
        * Method to decode private keys and returns the values
        * </summary>
        * <returns>Array of BigIntegers, index 0 is the D and 2 is N</returns>
        */
        public BigInteger[] decodeKeys()
        {
            var decoded = new BigInteger[2];
            var deserialized = this;

#pragma warning disable CS8604 // Possible null reference argument.
            byte[] decodedKey = Convert.FromBase64String(deserialized.Key);
#pragma warning restore CS8604 // Possible null reference argument.

            var d4 = decodedKey.Take(4).Reverse().ToArray();
            int d = BitConverter.ToInt32(d4);
            var dBytes = decodedKey.Skip(4).Take(d).ToArray();
            decoded[0] = new(dBytes);

            var n4 = decodedKey.Skip(4 + d).Take(4).Reverse().ToArray();
            int n = BitConverter.ToInt32(d4);
            var nBytes = decodedKey.Skip(8 + d).Take(d).ToArray();
            decoded[1] = new(nBytes);

            return decoded;
        }
    }

    internal class keyJsonPub
    {
        public string? email { get; set; }
        public string? Key { get; set; }

        /* <summary>
        * Method to read the a specified public key key from the current directory and save it to the instance of keyJsonPub called
        * </summary>
        * <param name="name"> name of the public key. Either user email or public</param>
        */
        public void readKey(string name)
        {
            keyJsonPub key = new();
            name = $"{name}.key";
            using (var sr = new StreamReader(name))
            {
                string? keyString = sr.ReadLine();
                key = JsonConvert.DeserializeObject<keyJsonPub>(keyString);
                this.email = key.email;
                this.Key = key.Key;
            }
        }

        /* <summary>
        * Method to decode private keys and returns the values
        * </summary>
        * <returns>Array of BigIntegers, index 0 is the D and 2 is N</returns>
        */
        public BigInteger[] decodeKey()
        {
            var decoded = new BigInteger[2];
            var deserialized = this;

#pragma warning disable CS8604 // Possible null reference argument.
            byte[] decodedKey = Convert.FromBase64String(deserialized.Key);
#pragma warning restore CS8604 // Possible null reference argument.

            var e4 = decodedKey.Take(4).Reverse().ToArray();
            int e = BitConverter.ToInt32(e4);
            var eBytes = decodedKey.Skip(4).Take(e).ToArray();
            decoded[0] = new(eBytes);

            var n4 = decodedKey.Skip(4 + e).Take(4).Reverse().ToArray();
            int n = BitConverter.ToInt32(n4);
            var nBytes = decodedKey.Skip(8 + e).Take(n).ToArray();
            decoded[1] = new(nBytes);

            return decoded;
        }
    }

    internal class messageJson
    {
        public string? email { get; set; }
        public string? content { get; set; }

        /* <summary>
        * Fills the current instance of the messageJson
        * </summary>
        * <param name="email">Address where message is going to be sent to</param>
        * <param name="msg">Encrypted base64 message to be sent</param>
        */
        internal void fill(string email, string msg)
        {
            this.email = email;
            this.content = msg;
        }
    }
}