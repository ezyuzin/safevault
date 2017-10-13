using System.Linq;
using System.Security.Cryptography;

namespace SafeVault.Security.Ciphers
{
    public class AesCipherKey : CipherKey
    {
        public RijndaelManaged Aes { get; private set; }

        public AesCipherKey(RijndaelManaged aes)
        {
            Aes = aes;
            Aes.Padding = PaddingMode.PKCS7;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                #if NETSTANDARD2_0
                Aes?.Dispose();
                #endif

                #if NETFX
                Aes?.Clear();
                #endif

                Aes = null;
            }
            base.Dispose(disposing);
        }
    }
}