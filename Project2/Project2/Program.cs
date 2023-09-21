using System;
using System.Diagnostics;
using System.IO;
using System.Linq.Expressions;
using System.Numerics;
using System.Security.Cryptography;

// Author: Luke Ward

namespace Project2
{
    class PrimeGen
    {
        private static int GlobalCount = 1;
        /// <summary>
        /// Takes arguments from command line and plugs into proper methods
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            var timer = new Stopwatch();
            int count;
            BigInteger bits;
            try
            {
                if (args.Length < 1 || args.Length > 2 || int.Parse(args[0]) % 8 != 0 || int.Parse(args[0]) < 32)
                {
                    help();
                }
                else if (args.Length == 1)
                {
                    timer.Start();
                    bits = new BigInteger(int.Parse(args[0]));
                    Console.WriteLine("BitLength: " + args[0] + " bits");
                    BigInteger[] temp = runPrime(bits);
                    timer.Stop();
                    Console.WriteLine("Time to Generate: " + timer.Elapsed);
                }
                else
                {
                    timer.Start();
                    bits = new BigInteger(int.Parse(args[0]));
                    count = int.Parse(args[1]);
                    Console.WriteLine("BitLength: " + args[0] + " bits");
                    BigInteger[] temp = runPrime(bits, count);
                    timer.Stop();
                    Console.WriteLine("Time to Generate: " + timer.Elapsed);
                }
            }
            catch
            {
                help();
            }
        }

        /// <summary>
        /// Runs the given prime numbers and checks if they're prime
        /// </summary>
        /// <param name="bits"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        static BigInteger[] runPrime(BigInteger bits, int count = 1)
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
                                        Console.WriteLine(GlobalCount + ": " + temp + "\n");
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

       

        /// <summary>
        /// Static method to display help text
        /// </summary>
        static void help()
        {
            Console.WriteLine("Usage: dotnet run <bits> <count=1>");
            Console.WriteLine("- bits - the number of bits of the prime number, this must be a multiple of 8, and at least 32 bits.");
            Console.WriteLine("- count - the number of prime numbers to generate, defaults to 1.\n");
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
