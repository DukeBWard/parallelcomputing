//Luke Ward
using Extensions;
using System.Numerics;
using System.Security.Cryptography;

namespace PrimeGen
{
    public class GeneratePrimes
    {
        /* <summary>
         * Method to generate and check an integer of bytes size
         * </summary>
         * <param name = "size"> Size in bytes of the number to be generated </param>
         * <returns> Prime BigInteger of size bytes </returns>
         */
        public BigInteger genAndCheck(Int32 bytes)
        {
            BigInteger result = new BigInteger(-1);
            Parallel.ForEach(Enumerable.Range(0, int.MaxValue), (i, state) =>
            {
                byte[] data = RandomNumberGenerator.GetBytes(bytes);
                BigInteger temp = new BigInteger(data);
                Boolean prime = temp.IsProbablyPrime();
                if (prime)
                {
                    if (result == -1) result = temp;
                    state.Stop();
                }
            });
            return result;
        }
    }
}