//Luke Ward
using Newtonsoft.Json;
using PrimeGen;
using System.Numerics;

namespace Extensions
{
    public static class Extension
    {
        /* <summary>
         * Method to combine sets of byte arrays.
         * </summary>
         * <param name="arrays">Two dimensional array of byte arrays.</param>
         * <returns>A single byte array made up of the combined arrays.</returns>
         */
        public static byte[] Combine(params byte[][] arrays)
        {
            byte[] rv = new byte[arrays.Sum(a => a.Length)];
            int offset = 0;
            foreach (byte[] array in arrays)
            {
                System.Buffer.BlockCopy(array, 0, rv, offset, array.Length);
                offset += array.Length;
            }
            return rv;
        }

        /* <summary>
         * Method to preform mod inverse.
         * </summary>
         * <param name="a">Multiplicative number.</param>
         * <param name="n">Mod number.</param>
         * <returns>The mod inverse of a and n.</returns>
         */
        public static BigInteger modInverse(BigInteger a, BigInteger n)
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

        /* <summary>
         * Extension method to check if a number is prime, will first run several pretests and then run a Miller-Rabin Primality Test
         * </summary>
         * <param name="value">BigInteger whose primality is to be tested.</param>
         * <param name="k">Witness loop repitions for the Miller-Rabin Primality test.</param>
         * <returns>Boolean value denoting if the number is prime or not.</returns>
         */
        public static Boolean IsProbablyPrime(this BigInteger value, int k = 10)
        {
            // if (value < 0 ) return false;
            if (preTestsOne(value))
                return false;

            if (k <= 0)
                k = 10;

            BigInteger d = value - 1;
            var r = 0;

            // Write n as 2^(r)·d + 1 with d odd (by factoring out powers of 2 from n − 1)
            while (d % 2 == 0)
            {
                d /= 2;
                r++;
            }
            var b = new byte[value.ToByteArray().Length];
            // WitnessLoop: repeat k times:
            for (int i = 0; i < k; i++)
            {
                // Pick a random integer a in the range[2, n − 2]
                var rand = new Random();
                BigInteger a;
                do
                {
                    rand.NextBytes(b);
                    a = new BigInteger(b);
                } while (a < 2 || a > value - 2);

                // x ← a^(d) mod n
                var mod = BigInteger.ModPow(a, d, value);
                // if x = 1 or x = n − 1 then continue WitnessLoop interpreted as 
                if (mod == 1 || mod == value - 1)
                    continue;

                // repeat r − 1 times:
                for (int j = 1; j < r - 1; j++)
                {
                    // x ← x^2 mod n
                    mod = BigInteger.ModPow(mod, 2, value);
                    // if x = n − 1 then
                    if (mod == value - 1)
                        break;
                    if (mod == 1)
                        return false;
                }
                if (mod != value - 1)
                    return false;
            }
            return true;
        }

        private static Boolean preTestsOne(BigInteger value)
        {
            if (value < 0) return true;
            var primeList = new List<int>() { 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41,
                43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89, 97, 101, 103 };
            if (!value.IsEven)
            {
                for (int i = 0; i < primeList.Count; i++)
                    if (value % primeList[i] == 0)
                        return true;
            }
            return false;
        }
    }
}