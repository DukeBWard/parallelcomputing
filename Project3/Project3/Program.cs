using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq.Expressions;
using System.Net.Mime;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

// Author: Luke Ward

namespace Project3
{
    class Key
    {
        public string email
        {
            get;
            set;
        }

        public string key
        {
            get;
            set;
        }
    }

    class PublicKey
    {
        public string[] email
        {
            get;
            set;
        }

        public string key
        {
            get;
            set;
        }
    }
    
    class Messaging
    {
        private static HttpClient client = new HttpClient();
        
        /// <summary>
        /// Takes arguments from command line and plugs into proper methods
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            //var t = Task.Run(() => getKey("jsb@cs.rit.edu"));
            //t.Wait();
            //splitKey("jsb@cs.rit.edu");
            keyGen(1024);
        }

        public static void keyGen(int keysize)
        {
            Random rand = new Random();
            double temp = rand.Next(20, 33);
            temp *= 0.01;
            temp = ((keysize/2) + (keysize/2) * temp);
            BigInteger p = (int)temp;
            BigInteger q = keysize - p;
            
            Console.WriteLine(temp);
            Console.WriteLine("p:" +p);
            Console.WriteLine("q:" +q);
            BigInteger[] parray = PrimeGen.runPrime(p);
            BigInteger[] qarray = PrimeGen.runPrime(q);
            BigInteger[] Earray = PrimeGen.runPrime(keysize-2);
            BigInteger P = parray[0];
            BigInteger Q = qarray[0];

            //BigInteger E = Earray[0];
            BigInteger E = 65537;
            BigInteger N = P * Q;
            BigInteger r = (P - 1) * (Q - 1);
            BigInteger D = modInverse(E, r);
            byte[] Ebytes = E.ToByteArray();
            byte[] Nbytes = N.ToByteArray();
            byte[] rbytes = r.ToByteArray();
            byte[] Dbytes = D.ToByteArray();
            BigInteger Esize = Ebytes.Length;
            byte[] Esizebyte = Esize.ToByteArray();

            Console.WriteLine("E: "+E);
            Console.WriteLine("N: "+N);
            Console.WriteLine("r: "+r);
            Console.WriteLine("D: "+D);

            byte[] PublicKey = new byte[keysize];
            byte[] PrivateKey = new byte[keysize];
            
            Array.Copy(Esizebyte,PublicKey,Esizebyte.Length);
            Array.Copy(Ebytes,0,PublicKey,(int)Esize,Ebytes.Length);
            

        }
        
        public static void sendKey(string email)
        {
            return;
        }
        
        static async Task getKey(string email)
        {
            try
            {

                using HttpResponseMessage response = await client.GetAsync("http://kayrun.cs.rit.edu:5000/Key/"+email);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                //Console.WriteLine(responseBody);
                using (StreamWriter sw = File.CreateText(email+".key"))
                {
                    sw.WriteLine(responseBody);
                }
                //Key key = JsonSerializer.Deserialize<Key>(responseBody);
                //Console.WriteLine(key.email);
                //Console.WriteLine(key.key);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            
        }
        
        public static void sendMsg(string email, string plaintext)
        {
            return;
        }
        
        public static void getMsg(string email)
        {
            return;
        }

        public static BigInteger[] splitKey(string email)
        {
            Key? key;
            using (StreamReader r = new StreamReader(email + ".key"))
            {
                string json = r.ReadToEnd();
                key = JsonSerializer.Deserialize<Key>(json);
                //Console.WriteLine(key.email);
                //Console.WriteLine(key.key);
            }
            byte[] keyBytes = Convert.FromBase64String(key.key);
           
            byte[] ebytes = new byte[4];
            Array.Copy(keyBytes, ebytes,4); ;
            Array.Reverse(ebytes);
            int e = BitConverter.ToInt32(ebytes, 0);
            Console.WriteLine("e: "+e);

            byte[] Ebytes = new byte[e];
            Array.Copy(keyBytes,4,Ebytes,0,e);
            BigInteger E = new BigInteger(Ebytes);
            Console.WriteLine("E: "+E);
            
            byte[] nbytes = new byte[4];
            Array.Copy(keyBytes,4+e,nbytes,0,4);
            Array.Reverse(nbytes);
            int n = BitConverter.ToInt32(nbytes, 0);
            Console.WriteLine("n: "+n);

            byte[] Nbytes = new byte[n];
            Array.Copy(keyBytes,4+e+4,Nbytes,0,n);
            BigInteger N = new BigInteger(Nbytes);
            Console.WriteLine("N: "+N);

            return new BigInteger[]{E, N};
        }
        
        static BigInteger modInverse(BigInteger a, BigInteger n)
        {
            BigInteger i = n, v = 0, d = 1;
            while (a > 0)
            {
                BigInteger t = i / a, x = a;
                a = i % x;
                i = x;
                x = d;
                d = v - t * x;
                v = x;
            }

            v %= n;
            if (v < 0) v = (v + n) % n;
            return v;
        }

        public void Help()
        {
            Console.WriteLine("Usage: dotnet run <option> <other arguments>");
        }
    }
    class PrimeGen
    {
        private static int GlobalCount = 1;

        /// <summary>
        /// Runs the given prime numbers and checks if they're prime
        /// </summary>
        /// <param name="bits"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static BigInteger[] runPrime(BigInteger bits, int count = 1)
        {
            BigInteger[] PrimeNums = new BigInteger[count];
            RandomNumberGenerator rng = RandomNumberGenerator.Create();
            byte[] bytes = new byte[(long)bits/8];
            BigInteger temp;

            BigInteger[] lowPrimes = new BigInteger[] {3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61,
                67, 71, 73, 79, 83, 89, 97
                , 101, 103, 107, 109, 113, 127, 131, 137, 139, 149, 151, 157, 163, 167, 173, 179
                , 181, 191, 193, 197, 199, 211, 223, 227, 229, 233, 239, 241, 251, 257, 263, 269
                , 271, 277, 281, 283, 293, 307, 311, 313, 317, 331, 337, 347, 349, 353, 359, 367
                , 373, 379, 383, 389, 397, 401, 409, 419, 421, 431, 433, 439, 443, 449, 457, 461
                , 463, 467, 479, 487, 491, 499, 503, 509, 521, 523, 541, 547, 557, 563, 569, 571
                , 577, 587, 593, 599, 601, 607, 613, 617, 619, 631, 641, 643, 647, 653, 659, 661
                , 673, 677, 683, 691, 701, 709, 719, 727, 733, 739, 743, 751, 757, 761, 769, 773
                , 787, 797, 809, 811, 821, 823, 827, 829, 839, 853, 857, 859, 863, 877, 881, 883
                , 887, 907, 911, 919, 929, 937, 941, 947, 953, 967, 971, 977, 983, 991, 997};
            

            Parallel.For(0, count, i =>
            {
                Parallel.For(0, 4, (index, state) =>
                {
                    Loop:
                    while (true)
                    {
                        lock (PrimeNums)
                        {
                            rng.GetBytes(bytes);
                            temp = new BigInteger(bytes);

                            if (temp >= 3)
                            {
                                if ((temp & 1) != 0)
                                {

                                    foreach (var p in lowPrimes)
                                    {
                                        if (temp == p)
                                            break;
                                        if (temp % p == 0)
                                            goto Loop;
                                    }

                                    if (temp.IsProbablyPrime() && temp > 0 && GlobalCount <= count)
                                    {
                                        PrimeNums[i] = temp;
                                        //Console.WriteLine(GlobalCount + ": " + temp + "\n");
                                        GlobalCount++;
                                        state.Break();
                                        break;
                                    }

                                    if (GlobalCount > count)
                                    {
                                        return;
                                    }

                                }
                            }
                        }
                    }
                });


            });

            GlobalCount = 1;
            return PrimeNums;
        }
    }

    public static class BigIntExtension
    {
        public static Boolean IsProbablyPrime(this BigInteger value, int k = 10)
        {
            if(value == 2 || value == 3)
                return true;
            if(value < 2 || value % 2 == 0)
                return false;

            BigInteger d = value - 1;
            int s = 0;

            while(d % 2 == 0)
            {
                d /= 2;
                s += 1;
            }
            RandomNumberGenerator rng = RandomNumberGenerator.Create();
            byte[] bytes = new byte[value.ToByteArray().LongLength];
            BigInteger a;

            for(int i = 0; i < k; i++)
            {
                do
                {
                    rng.GetBytes(bytes);
                    a = new BigInteger(bytes);
                }
                while(a < 2 || a >= value - 2);

                BigInteger x = BigInteger.ModPow(a, d, value);
                if(x == 1 || x == value - 1)
                    continue;

                for(int r = 1; r < s; r++)
                {
                    x = BigInteger.ModPow(x, 2, value);
                    if(x == 1)
                        return false;
                    if(x == value - 1)
                        break;
                }

                if(x != value - 1)
                    return false;
            }

            return true;
        }
    }
}
