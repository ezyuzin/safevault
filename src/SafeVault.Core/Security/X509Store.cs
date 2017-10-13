using System;
using SafeVault.Cache;
using SafeVault.Exceptions;
using SafeVault.Security.Ciphers;

namespace SafeVault.Security
{
    public class X509Store : IDisposable
    {
        private Cache<string, ICipher> _cache;
        public X509Store()
        {
            _cache = new Cache<string, ICipher>();
            _cache.TTL = TimeSpan.FromMinutes(15);
        }

        public ICipher GetCertificate(string certificatePath)
        {
            var cert = _cache.GetOrAdd(certificatePath, s =>
            {
                if (global::System.IO.File.Exists(certificatePath) == false)
                    return null;

                var privateKeyPath = $"{certificatePath}.key";
                if (global::System.IO.File.Exists(privateKeyPath) == false)
                    privateKeyPath = null;

                return RsaCipher.LoadFromPEM(certificatePath, privateKeyPath);
            });

            if (cert == null)
                throw new FileNotFoundException("Certificate not Found");

            return cert;
        }

        public void Dispose()
        {
            _cache?.Dispose();
            _cache = null;
        }
    }
}