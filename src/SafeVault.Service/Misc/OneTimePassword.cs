using System;
using System.Security.Cryptography;
using SafeVault.Security;

namespace SafeVault.Misc
{
    public class OneTimePassword
    {
        static public string Get(string phrase, int offset, int timeStep = 30, int digits = 6)
        {
            long time = offset + ((long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds) / timeStep;

            return Calculate(phrase, time, digits);
        }

        public static string Get(byte[] secret, int offset, int timeStep = 30, int digits = 6)
        {
            long time = offset + ((long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds) / timeStep;
            return string.Format("{0}", Calculate(secret, time, digits)).PadLeft(digits, '0');
        }

        public static string Calculate(string phrase, long time, int digits)
        {
            var secret = Base32.Decode(phrase);
            return string.Format("{0}", Calculate(secret, time, digits)).PadLeft(digits, '0');
        }

        static int Calculate(byte[] secret, long tm, int digits)
        {
            byte[] data = new byte[8];
            for (int i = 8; i != 0; --i)
            {
                data[i - 1] = (byte) (tm & 0xFF);
                tm = tm >> 8;
            }

            byte[] hmac;
            using (HMACSHA1 sha1 = new HMACSHA1(secret))
            {
                hmac = sha1.ComputeHash(data);
            }

            int offset = hmac[hmac.Length - 1] & 0xF;
            long truncatedHash = 0;

            for (int i = 0; i < 4; ++i)
            {
                truncatedHash <<= 8;
                truncatedHash |= (long)(hmac[offset + i] & 0xFF);
            }

            long module = (long) Math.Pow(10, digits);
            truncatedHash &= 0x7FFFFFFF;
            truncatedHash %= module;

            return (int) truncatedHash;
        }
    }
}