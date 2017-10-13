using System;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using SafeVault.Configuration;
using SafeVault.Exceptions;
using SafeVault.Logger;
using SafeVault.Security;
using SafeVault.Security.Ciphers;
using SafeVault.Transport;
using SafeVault.Transport.Exceptions;

namespace SafeVault.Core.UnitTest
{
    [TestFixture]
    public class ServiceChannelTest
    {
        public static ILog Logger;

        [SetUp]
        public void SetUp()
        {
            var location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var config = new AppConfig($"{location}/data/app.config");

            Log.Instance = new LogManager(config);
            Logger = Log.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        }

        public void TransferData(Stream source, Stream target)
        {
            var pos = target.Position;
            source.Position = 0;

            byte[] buf = new byte[1024];
            while (true)
            {
                int nbytes = source.Read(buf, 0, buf.Length);
                if (nbytes == 0)
                    break;

                target.Write(buf, 0, nbytes);
            }

            source.SetLength(0);
            target.Position = pos;
        }

        [Test]
        public void ServiceChannel000Test()
        {
            var location = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            using(var stream1 = new MemoryStream())
            using(var stream2 = new MemoryStream())
            using(var stream11 = new MemoryStream())
            using(var stream21 = new MemoryStream())
            using (var clientChannel = new ServiceChannel())
            using (var serviceChannel = new ServiceChannel())
            {
                serviceChannel.SetReadStream(stream1, false);
                serviceChannel.CipherLib["rsa-private"] = RsaCipher.LoadFromPEM($"{location}\\data\\server\\server.pem", $"{location}\\data\\server\\server.pem.key");
                
                clientChannel.SetWriteStream(stream2, false);
                clientChannel.CipherLib["rsa-public"] = RsaCipher.LoadFromPEM($"{location}\\data\\server\\server.pem");
                clientChannel.CipherLib["rsa-private"] = RsaCipher.LoadFromPEM($"{location}\\data\\client\\test-user\\cer.pem", $"{location}\\data\\client\\test-user\\cer.pem.key");

                clientChannel.Encrypt();
                clientChannel.WriteObject("HELO");
                clientChannel.Flush();

                clientChannel.SetReadStream(stream21, false);

                Console.WriteLine(stream2.Length);

                TransferData(stream2, stream1);
                var value = serviceChannel.ReadObject<string>();
                Console.WriteLine(value);
                Assert.AreEqual("HELO", value);

                clientChannel.CipherLib["rsa-public"] = RsaCipher.LoadFromPEM($"{location}\\data\\client\\test-user\\cer.pem");

                serviceChannel.SetWriteStream(stream11, false);
                serviceChannel.Encrypt();
                serviceChannel.WriteObject("EHLO");
                serviceChannel.Flush();

                Console.WriteLine(stream11.Length);
                TransferData(stream11, stream21);

                Console.WriteLine(string.Join(" ", stream21.ToArray().Select(m => $"{m:X2}").ToArray()));

                var value1 = clientChannel.ReadObject<string>();
                Console.WriteLine(value1);
                Assert.AreEqual("EHLO", value1);
            }
        }

        [Test]
        public void ServiceChannel020Test()
        {
            var location = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            using(var stream1 = new MemoryStream())
            using(var stream2 = new MemoryStream())
            using(var stream11 = new MemoryStream())
            using(var stream21 = new MemoryStream())
            using (var clientChannel = new ServiceChannel())
            using (var serviceChannel = new ServiceChannel())
            {
                serviceChannel.SetReadStream(stream1, false);
                clientChannel.SetWriteStream(stream2, false);

                clientChannel.Encrypt();
                clientChannel.WriteObject("HELO");
                clientChannel.Flush();

                clientChannel.SetReadStream(stream21, false);

                Console.WriteLine(stream2.Length);

                TransferData(stream2, stream1);

                var value = serviceChannel.ReadObject<string>();
                Console.WriteLine(value);
                Assert.AreEqual("HELO", value);

                serviceChannel.SetWriteStream(stream11, false);
                serviceChannel.Encrypt();
                serviceChannel.WriteObject("EHLO");
                serviceChannel.Flush();

                Console.WriteLine(stream11.Length);
                TransferData(stream11, stream21);

                Console.WriteLine(string.Join(" ", stream21.ToArray().Select(m => $"{m:X2}").ToArray()));

                var value1 = clientChannel.ReadObject<string>();
                Console.WriteLine(value1);
                Assert.AreEqual("EHLO", value1);
            }
        }

        [Test]
        public void ServiceChannel010Test()
        {
            var location = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            using(var stream1 = new MemoryStream())
            using(var stream2 = new MemoryStream())
            using(var stream11 = new MemoryStream())
            using(var stream21 = new MemoryStream())
            using (var clientChannel = new ServiceChannel())
            using (var serviceChannel = new ServiceChannel())
            {
                serviceChannel.SetReadStream(stream1, false);
                serviceChannel.CipherLib["rsa-private"] = RsaCipher.LoadFromPEM($"{location}\\data\\server\\server.pem", $"{location}\\data\\server\\server.pem.key");
                
                clientChannel.SetWriteStream(stream2, false);
                clientChannel.CipherLib["rsa-public"] = RsaCipher.LoadFromPEM($"{location}\\data\\server\\server.pem");
                //clientChannel.CipherLib["rsa-private"] = RsaCipher.LoadFromPEM($"{location}\\data\\client\\test-user\\cer.pem", $"{location}\\data\\client\\test-user\\cer.pem.key");

                clientChannel.Encrypt();
                clientChannel.WriteObject("HELO");
                clientChannel.Flush();

                clientChannel.SetReadStream(stream21, false);

                Console.WriteLine(stream2.Length);

                TransferData(stream2, stream1);

                var value = serviceChannel.ReadObject<string>();
                Console.WriteLine(value);
                Assert.AreEqual("HELO", value);

                //clientChannel.CipherLib["rsa-public"] = RsaCipher.LoadFromPEM($"{location}\\data\\client\\test-user\\cer.pem");

                serviceChannel.SetWriteStream(stream11, false);
                serviceChannel.Encrypt();
                serviceChannel.WriteObject("EHLO");
                serviceChannel.Flush();

                Console.WriteLine(stream11.Length);
                TransferData(stream11, stream21);

                Console.WriteLine(string.Join(" ", stream21.ToArray().Select(m => $"{m:X2}").ToArray()));

                var value1 = clientChannel.ReadObject<string>();
                Console.WriteLine(value1);
                Assert.AreEqual("EHLO", value1);
            }
        }



        [Test]
        public void ServiceChannel030Test()
        {
            var location = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            using(var stream1 = new MemoryStream())
            using(var stream2 = new MemoryStream())
            using (var clientChannel = new ServiceChannel())
            using (var serviceChannel = new ServiceChannel())
            {
                serviceChannel.SetReadStream(stream1, false);
                //serviceChannel.CipherLib["rsa-private"] = RsaCipher.LoadFromPEM($"{location}\\data\\server\\server.pem", $"{location}\\data\\server\\server.pem.key");

                clientChannel.SetWriteStream(stream2, false);
                clientChannel.CipherLib["rsa-public"] = RsaCipher.LoadFromPEM($"{location}\\data\\server\\server.pem");

                clientChannel.Encrypt();
                clientChannel.WriteObject("HELO");
                clientChannel.Flush();
                Console.WriteLine(stream2.Length);

                TransferData(stream2, stream1);
                Assert.Catch<SecureChannelException>(() =>
                {
                    var msg = serviceChannel.ReadObject<string>();
                });
            }
        }

        [Test]
        public void ServiceChannel040Test()
        {
            var location = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            using(var stream1 = new MemoryStream())
            using(var stream2 = new MemoryStream())
            using (var clientChannel = new ServiceChannel())
            using (var serviceChannel = new ServiceChannel())
            {
                serviceChannel.SetReadStream(stream1, false);
                serviceChannel.CipherLib["rsa-private"] = RsaCipher.LoadFromPEM($"{location}\\data\\server\\server.pem", $"{location}\\data\\server\\server.pem.key");

                clientChannel.SetWriteStream(stream2, false);
                //clientChannel.CipherLib["rsa-public"] = RsaCipher.LoadFromPEM($"{location}\\data\\server\\server.pem");

                clientChannel.Encrypt();
                clientChannel.WriteObject("HELO");
                clientChannel.Flush();
                Console.WriteLine(stream2.Length);

                TransferData(stream2, stream1);
                var msg = serviceChannel.ReadObject<string>();
                Assert.AreEqual("HELO", msg);
            }
        }

        [Test]
        public void ServiceChannel050Test()
        {
            var location = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            using(var stream1 = new MemoryStream())
            using(var stream2 = new MemoryStream())
            using (var clientChannel = new ServiceChannel())
            using (var serviceChannel = new ServiceChannel())
            {
                serviceChannel.SetReadStream(stream1, false);
                serviceChannel.CipherLib["rsa-private"] = RsaCipher.LoadFromPEM($"{location}\\data\\server\\server.pem", $"{location}\\data\\server\\server.pem.key");

                clientChannel.SetWriteStream(stream2, false);
                
                clientChannel.Encrypt();
                clientChannel.WriteObject("HELO");

                clientChannel.CipherLib["rsa-public"] = RsaCipher.LoadFromPEM($"{location}\\data\\server\\server.pem");
                clientChannel.Encrypt();
                clientChannel.WriteObject("HELO2");

                clientChannel.Flush();
                Console.WriteLine(stream2.Length);

                TransferData(stream2, stream1);
                var msg = serviceChannel.ReadObject<string>();
                Assert.AreEqual("HELO", msg);

                var msg2 = serviceChannel.ReadObject<string>();
                Assert.AreEqual("HELO2", msg2);
            }
        }

        [Test]
        public void ServiceChannel060Test()
        {
            var location = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            using(var stream1 = new MemoryStream())
            using(var stream2 = new MemoryStream())
            using (var clientChannel = new ServiceChannel())
            using (var serviceChannel = new ServiceChannel())
            {
                serviceChannel.SetReadStream(stream1, false);
                //serviceChannel.CipherLib["rsa-private"] = RsaCipher.LoadFromPEM($"{location}\\data\\server\\server.pem", $"{location}\\data\\server\\server.pem.key");

                clientChannel.SetWriteStream(stream2, false);
                
                clientChannel.Encrypt();
                clientChannel.WriteObject("HELO");

                clientChannel.CipherLib["rsa-public"] = RsaCipher.LoadFromPEM($"{location}\\data\\server\\server.pem");
                clientChannel.Encrypt();
                clientChannel.WriteObject("HELO2");

                clientChannel.Flush();
                Console.WriteLine(stream2.Length);

                TransferData(stream2, stream1);
                var msg = serviceChannel.ReadObject<string>();
                Assert.AreEqual("HELO", msg);

                Assert.Catch<SecureChannelException>(() =>
                {
                    var msg2 = serviceChannel.ReadObject<string>();
                });
            }
        }

        [Test]
        public void ServiceChannel070Test()
        {
            var location = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            using(var stream1 = new MemoryStream())
            using(var stream2 = new MemoryStream())
            using (var clientChannel = new ServiceChannel())
            using (var serviceChannel = new ServiceChannel())
            {
                serviceChannel.SetReadStream(stream1, false);
                serviceChannel.CipherLib["rsa-private"] = RsaCipher.LoadFromPEM($"{location}\\data\\server\\server.pem", $"{location}\\data\\server\\server.pem.key");

                clientChannel.SetWriteStream(stream2, false);
                clientChannel.CipherLib["rsa-public"] = RsaCipher.LoadFromPEM($"{location}\\data\\server\\server.pem");
                clientChannel.Encrypt();

                var data = Security.Random.Get(128 * 1024 + 1);
                clientChannel.Encrypt();
                clientChannel.Write(data);

                clientChannel.Flush();
                Console.WriteLine(stream2.Length);

                TransferData(stream2, stream1);
                var data1 = serviceChannel.Read();
                Assert.AreEqual(Hash.MD5(data), Hash.MD5(data1));
            }
        }
    }
}