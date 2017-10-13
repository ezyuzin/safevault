using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using SafeVault.Configuration;
using SafeVault.Logger;
using SafeVault.Security.Ciphers;
using SafeVault.Transport;
using SafeVault.Transport.Models;
using Random = SafeVault.Security.Random;

namespace SafeVault.Core.UnitTest
{
    [TestFixture]
    [Ignore("require run external service")]
    public class HttpServiceChannelTest
    {
        public static ILog Logger;

        [SetUp]
        public void SetUp()
        {
            var location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Environment.CurrentDirectory = location;

            var config = new AppConfig($"data/app.config");

            Log.Instance = new LogManager(config);
            Logger = Log.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        }

        [Test]
        public void HttpService040Test()
        {
            using (var clientChannel = new HttpServiceChannel(new Uri("http://192.168.0.235:5000/api/safevault")))
            {
                clientChannel.CipherLib["rsa-public"] = RsaCipher.LoadFromPEM($"data\\server\\server.pem");
                clientChannel.CipherLib["rsa-private"] = RsaCipher.LoadFromX509Store("Evgeny Zyuzin SafeVault Client");

                var qm = new QueryMessage {Command = "dbx-Download"};
                qm.Params["username"] = "test-user1";
                qm.Params["uuid"] = "safevault";
                qm.Params["password"] = "1234567890";

                clientChannel.Encrypt();
                clientChannel.WriteObject(qm);
                clientChannel.Post();

                var response = clientChannel.ReadObject<ResponseMessage>();
                Console.WriteLine(response.StatusCode + " " + response.StatusText);
                Assert.AreEqual(200, response.StatusCode);
                Console.WriteLine(response.Header["data"]);

                var data = clientChannel.Read();
                Assert.AreEqual(response.Header["md5"], Security.Hash.MD5(data));
                Console.WriteLine(data.Length);
            }
        }

        [Test]
        public void HttpService050Test()
        {
            using (var clientChannel = new HttpServiceChannel(new Uri("http://192.168.0.235:5000/api/safevault")))
            {
                clientChannel.CipherLib["rsa-public"] = RsaCipher.LoadFromPEM($"data\\server\\server.pem");
                clientChannel.CipherLib["rsa-private"] = RsaCipher.LoadFromPEM($"data\\client\\test-user\\cer.pem", $"data\\client\\test-user\\cer.pem.key");

                var dbxData = Random.Get(3838);

                var qm = new QueryMessage {Command = "dbx-Upload"};
                qm.Params["username"] = "test-user";
                qm.Params["uuid"] = "safevault";
                qm.Params["password"] = "1234567890";
                qm.Params["md5"] = Security.Hash.MD5(dbxData);
                qm.Params["last-modified"] = "2017-01-01 12:00:00Z";

                clientChannel.Encrypt();
                clientChannel.WriteObject(qm);
                clientChannel.Write(dbxData);
                clientChannel.Post();

                var response = clientChannel.ReadObject<ResponseMessage>();
                Console.WriteLine(response.StatusCode + " " + response.StatusText);
                Assert.AreEqual(200, response.StatusCode);
                Console.WriteLine(response.Header["data"]);
            }
        }

        [Test]
        public void HttpService030Test()
        {
            using (var clientChannel = new HttpServiceChannel(new Uri("http://192.168.0.235:5000/api/safevault")))
            {
                clientChannel.CipherLib["rsa-public"] = RsaCipher.LoadFromPEM("data\\server\\server.pem");
                clientChannel.CipherLib["rsa-private"] = RsaCipher.LoadFromPEM("data\\client\\test-user\\cer.pem", "data\\client\\test-user\\cer.pem.key");

                var qm = new QueryMessage {Command = "dbx-GetLastModified"};
                qm.Params["username"] = "test-user";
                qm.Params["uuid"] = "safevault";
                qm.Params["password"] = "1234567890";

                clientChannel.Encrypt();
                clientChannel.WriteObject(qm);
                clientChannel.Post();

                var response = clientChannel.ReadObject<ResponseMessage>();
                Console.WriteLine(response.StatusCode + " " + response.StatusText);
                Assert.AreEqual(200, response.StatusCode);
                Console.WriteLine(response.Header["data"]);
            }
        }

        [Test]
        public void HttpService020Test()
        {
            using (var clientChannel = new HttpServiceChannel(new Uri("http://192.168.0.235:5000/api/safevault")))
            {
                clientChannel.CipherLib["rsa-public"] = RsaCipher.LoadFromPEM("data\\server\\server.pem");
                clientChannel.CipherLib["rsa-private"] = RsaCipher.LoadFromPEM("data\\client\\test-user\\cer.pem", "data\\client\\test-user\\cer.pem.key");

                var queryMessage = new QueryMessage();
                queryMessage.Command = "ping";

                clientChannel.Encrypt();
                clientChannel.WriteObject(queryMessage);
                clientChannel.Post();

                var response = clientChannel.ReadObject<ResponseMessage>();
                Console.WriteLine(response.StatusCode);
                Console.WriteLine(response.StatusText);
                Console.WriteLine(response.Header["data"]);

                Assert.AreEqual(200, response.StatusCode);
                //Console.WriteLine(response.Header["data"]);
            }
        }

        [Test]
        //[Ignore("require run external service")]
        public void HttpService010Test()
        {
            using (var clientChannel = new HttpServiceChannel(new Uri("http://192.168.0.235:5000/api/safevault")))
            {
                clientChannel.CipherLib["rsa-public"] = RsaCipher.LoadFromPEM($"data\\server\\server.pem");
                clientChannel.CipherLib["rsa-private"] = RsaCipher.LoadFromPEM($"data\\client\\test-user\\cer.pem", $"data\\client\\test-user\\cer.pem.key");

                var queryMessage = new QueryMessage();
                queryMessage.Command = "get-token";
                queryMessage.Params["username"] = "ezyuzin";

                clientChannel.Encrypt();
                clientChannel.WriteObject(queryMessage);
                clientChannel.Post();

                Logger.Info("Read Response");
                var response = clientChannel.ReadObject<ResponseMessage>();
                Console.WriteLine(response.StatusCode);
                Console.WriteLine(response.StatusText);

                Assert.AreEqual(400, response.StatusCode);
                //Console.WriteLine(response.Header["data"]);
            }
        }
    }
}