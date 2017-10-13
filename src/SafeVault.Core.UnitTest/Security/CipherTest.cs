using System;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using SafeVault.Security.Ciphers;
using Random = SafeVault.Security.Random;

namespace SafeVault.Core.UnitTest
{
    [TestFixture]
    public class CipherTest
    {
        [Test]
        public void XorCipherTest()
        {
            using (var cipher1 = new XorCipher())
            using (var cipher2 = new XorCipher())
            {
                var content = "password";

                var passwordEnc = cipher1.Encrypt(Encoding.UTF8.GetBytes(content));
                Console.Write(string.Join(";", passwordEnc.Select(@byte => $"{@byte:x2}").ToArray()));

                var password = Encoding.UTF8.GetString(cipher2.Decrypt(passwordEnc));
                Assert.AreEqual(content, password);
            }
        }

        [Test]
        public void AesCipherTest()
        {
            using (var cipher = new Aes256Cipher(Encoding.UTF8.GetBytes("changeit")))
            {
                var content = "password";

                var passwordEnc = cipher.Encrypt(Encoding.UTF8.GetBytes(content));
                var password = Encoding.UTF8.GetString(cipher.Decrypt(passwordEnc));
                Assert.AreEqual(content, password);
            }
        }

        [Test]
        public void RsaCipher030Test()
        {
            var location = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            using (var cipher1 = RsaCipher.LoadFromPEM($"{location}\\data\\server\\server.pem"))
            using (var cipher2 = RsaCipher.LoadFromPEM($"{location}\\data\\server\\server.pem", $"{location}\\data\\server\\server.pem.key"))
            {
                var content = Random.Get(32);
                var contentEnc = cipher1.Encrypt(content);
                var contentDec = cipher2.Decrypt(contentEnc);
                Assert.AreEqual(content, contentDec);
            }
        }

        [Test]
        public void RsaCipher020Test()
        {
            var location = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            using (var cipher1 = RsaCipher.LoadFromPEM($"{location}\\data\\client\\test-user\\cer.pem"))
            using (var cipher2 = RsaCipher.LoadFromPEM($"{location}\\data\\client\\test-user\\cer.pem", $"{location}\\data\\client\\test-user\\cer.pem.key"))
            {
                var content = Random.Get(32);
                var contentEnc = cipher1.Encrypt(content);
                var contentDec = cipher2.Decrypt(contentEnc);
                Assert.AreEqual(content, contentDec);
            }
        }

        [Test]
        public void RsaCipherTest()
        {
            var location = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            using (var cipher1 = RsaCipher.LoadFromPEM($"{location}\\data\\client\\test-user\\cer.pem"))
            using (var cipher2 = RsaCipher.LoadFromPEM($"{location}\\data\\client\\test-user\\cer.pem", $"{location}\\data\\client\\test-user\\cer.pem.key"))
            {
                var content = "password";
                var passwordEnc = cipher1.Encrypt(Encoding.UTF8.GetBytes(content));
                var password = Encoding.UTF8.GetString(cipher2.Decrypt(passwordEnc));
                Assert.AreEqual(content, password);
            }
        }

        [Test]
        public void CipherRefTest()
        {
            var cipher = new Aes256Cipher(Encoding.UTF8.GetBytes("changeit"));
            var content = "password";
            var passwordEnc = cipher.Encrypt(Encoding.UTF8.GetBytes(content));

            var cipher2 = cipher.Clone();
            cipher.Dispose();

            Assert.Catch<ObjectDisposedException>(() => cipher.Decrypt(passwordEnc));

            var password = Encoding.UTF8.GetString(cipher2.Decrypt(passwordEnc));
            Assert.AreEqual(content, password);

            cipher2.Dispose();
            Assert.Catch<ObjectDisposedException>(() => cipher2.Decrypt(passwordEnc));
        }
    }
}