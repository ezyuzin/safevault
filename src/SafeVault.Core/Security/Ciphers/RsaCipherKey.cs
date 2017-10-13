using System.Security.Cryptography;

namespace SafeVault.Security.Ciphers
{
    public class RsaCipherKey : CipherKey
    {
        #if NETSTANDARD2_0
        public RSA Public;
        public RSA Private;
        #endif

        #if NETFX
        public RSACryptoServiceProvider Public;
        public RSACryptoServiceProvider Private;
        #endif

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                #if NETSTANDARD2_0
                Public?.Dispose();
                Private?.Dispose();
                #endif

                #if NETFX
                Public?.Clear();
                Private?.Clear();
                #endif

                Public = null;
                Private = null;
            }
        }
    }
}