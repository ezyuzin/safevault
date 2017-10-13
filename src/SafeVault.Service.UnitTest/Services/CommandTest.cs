using System;
using System.IO;
using System.Net;
using System.Reflection;
using NUnit.Framework;
using SafeVault.Configuration;
using SafeVault.Contracts;
using SafeVault.Logger;
using SafeVault.Misc;
using SafeVault.Net.SendMail;
using SafeVault.Security;
using SafeVault.Security.Ciphers;
using SafeVault.Service.Authentificate;
using SafeVault.Service.Command;
using SafeVault.Service.Command.Models;
using SafeVault.Transport;
using SafeVault.Transport.Exceptions;
using SafeVault.Transport.Models;
using SafeVault.Unity;
using ArgumentException = SafeVault.Exceptions.ArgumentException;
using Random = SafeVault.Security.Random;

namespace SafeVault.Service.UnitTest.Services
{
    [TestFixture]
    public class CommandTest
    {
        private string _location;

        [Dependency]
        private IUnity Unity { get; set; }

        [Dependency]
        public ICommandProcessor Command { get; set; }

        public IConfig Config { get; set; }

        [SetUp]
        public void SetUp()
        {
            _location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Environment.CurrentDirectory = _location;

            var config = new AppConfig($"data/app.config");

            Unity = new Unity.Unity();
            Unity.RegisterInstance<IConfig, AppConfig>(config);
            Unity.Register<ILogManager, LogManager>();

            Unity.Register<ICommandProcessor, CommandProcessor>();
            Unity.Register<IAuthenticate, Authentication>();

            Unity.Register<BanList.BanList, BanList.BanList>();
            Unity.Register<TokenList, TokenList>();
            Unity.Register<X509Store, X509Store>();
            Unity.Register<Notification.EmailNotification, Notification.EmailNotification>();
            Unity.Register<ISendMail, SendMail>();

            Log.Instance = Unity.Resolve<ILogManager>();
            Unity.BuildUp(typeof(CommandTest), this);
        }

        [TearDown]
        public void TearDown()
        {
            Unity?.Dispose();
            Unity = null;
        }

        [Test]
        public void DbxGetLastModifiedTest010()
        {
            if (Directory.Exists($"{_location}/data/client/test-user/dbx"))
                Directory.Delete($"{_location}/data/client/test-user/dbx", true);

            Directory.CreateDirectory($"{_location}/data/client/test-user/dbx");

            byte[] dbxData = Random.Get(256);
            File.WriteAllBytes($"{_location}/data/client/test-user/dbx/safevault.dbx", dbxData);
            var fileInfo = new FileInfo($"{_location}/data/client/test-user/dbx/safevault.dbx");
            fileInfo.LastWriteTimeUtc = DateTime.Parse("2017-01-01 12:00:00Z");


            using (var stream1 = new MemoryStream())
            using (var channel1 = new ServiceChannel())
            using (var channel2 = new ServiceChannel())
            {
                channel2.SetReadStream(stream1, canDispose:false);
                channel1.SetWriteStream(stream1, canDispose:false);

                Context ctx = new Context();
                ctx.ClientIP = IPAddress.None;
                ctx.Channel = channel1;

                ctx.Query = new QueryMessage {Command = "dbx-GetLastModified"};
                ctx.Query.Params["username"] = "test-user";
                ctx.Query.Params["uuid"] = "safevault";
                ctx.Query.Params["password"] = "1234567890";

                Command.Process(ctx);

                stream1.Position = 0;
                channel2.CipherLib["rsa-private"] = RsaCipher
                    .LoadFromPEM($"{_location}/data/client/test-user/cer.pem", $"{_location}/data/client/test-user/cer.pem.key");

                var response = channel2.ReadObject<ResponseMessage>();
                Assert.AreEqual(200, response.StatusCode);
                Assert.AreEqual("2017-01-01 12:00:00Z", response.Header["data"]);
            }
        }

        [Test]
        public void DbxDownloadCommandTest010()
        {
            if (Directory.Exists($"{_location}/data/client/test-user/dbx"))
                Directory.Delete($"{_location}/data/client/test-user/dbx", true);

            Directory.CreateDirectory($"{_location}/data/client/test-user/dbx");

            byte[] dbxData = Random.Get(256);
            File.WriteAllBytes($"{_location}/data/client/test-user/dbx/safevault.dbx", dbxData);

            using (var stream1 = new MemoryStream())
            using (var stream2 = new MemoryStream())
            using (var channel1 = new ServiceChannel())
            using (var channel2 = new ServiceChannel())
            {
                channel2.SetReadStream(stream1, canDispose:false);
                channel2.SetWriteStream(stream2, canDispose:false);

                stream2.Position = 0;
                channel1.SetReadStream(stream2, canDispose:false);
                channel1.SetWriteStream(stream1, canDispose:false);

                Context ctx = new Context();
                ctx.ClientIP = IPAddress.None;
                ctx.Channel = channel1;

                ctx.Query = new QueryMessage {Command = "dbx-Download"};
                ctx.Query.Params["username"] = "test-user";
                ctx.Query.Params["uuid"] = "safevault";
                ctx.Query.Params["password"] = "1234567890";

                Command.Process(ctx);

                stream1.Position = 0;
                channel2.CipherLib["rsa-private"] = RsaCipher
                    .LoadFromPEM($"{_location}/data/client/test-user/cer.pem", $"{_location}/data/client/test-user/cer.pem.key");

                var response = channel2.ReadObject<ResponseMessage>();
                Assert.AreEqual(200, response.StatusCode);
                Assert.AreEqual(Hash.MD5(dbxData), response.Header["md5"]);

                var data = channel2.Read();
                Assert.AreEqual(Hash.MD5(dbxData), Hash.MD5(data));
            }
        }

        [Test]
        public void DbxUploadCommandTest010()
        {
            if (Directory.Exists($"{_location}/data/client/test-user/dbx"))
                Directory.Delete($"{_location}/data/client/test-user/dbx", true);

            using (var stream1 = new MemoryStream())
            using (var stream2 = new MemoryStream())
            using (var channel1 = new ServiceChannel())
            using (var channel2 = new ServiceChannel())
            {
                byte[] dbxData = Random.Get(256);
                channel2.SetReadStream(stream1, canDispose:false);
                channel2.SetWriteStream(stream2, canDispose:false);
                channel2.Write(dbxData);

                stream2.Position = 0;
                channel1.SetReadStream(stream2, canDispose:false);
                channel1.SetWriteStream(stream1, canDispose:false);

                Context ctx = new Context();
                ctx.ClientIP = IPAddress.None;
                ctx.Channel = channel1;

                ctx.Query = new QueryMessage {Command = "dbx-Upload"};
                ctx.Query.Params["username"] = "test-user";
                ctx.Query.Params["uuid"] = "safevault";
                ctx.Query.Params["password"] = "1234567890";
                ctx.Query.Params["md5"] = Hash.MD5(dbxData);
                ctx.Query.Params["last-modified"] = "2017-01-01 12:00:00Z";

                Command.Process(ctx);

                stream1.Position = 0;
                channel2.CipherLib["rsa-private"] = RsaCipher
                    .LoadFromPEM($"{_location}/data/client/test-user/cer.pem", $"{_location}/data/client/test-user/cer.pem.key");

                var response = channel2.ReadObject<ResponseMessage>();
                Assert.AreEqual(200, response.StatusCode);
                Assert.AreEqual("OK", response.Header["data"]);

                var data = File.ReadAllBytes($"{_location}/data/client/test-user/dbx/safevault.dbx");
                Assert.AreEqual(dbxData, data);
 
                var fileInfo = new FileInfo($"{_location}/data/client/test-user/dbx/safevault.dbx");
                Assert.AreEqual(fileInfo.CreationTime, DateTime.Parse(ctx.Query.Params["last-modified"]));

            }
        }

        [Test]
        public void DbxUploadCommandTest020()
        {
            if (Directory.Exists($"{_location}/data/client/test-user/dbx"))
                Directory.Delete($"{_location}/data/client/test-user/dbx", true);

            Directory.CreateDirectory($"{_location}/data/client/test-user/dbx");

            byte[] dbxData1 = Random.Get(256);
            File.WriteAllBytes($"{_location}/data/client/test-user/dbx/safevault.dbx", dbxData1);

            using (var stream1 = new MemoryStream())
            using (var stream2 = new MemoryStream())
            using (var channel1 = new ServiceChannel())
            using (var channel2 = new ServiceChannel())
            {
                byte[] dbxData = Random.Get(256);
                channel2.SetReadStream(stream1, canDispose:false);
                channel2.SetWriteStream(stream2, canDispose:false);
                channel2.Write(dbxData);

                stream2.Position = 0;
                channel1.SetReadStream(stream2, canDispose:false);
                channel1.SetWriteStream(stream1, canDispose:false);

                Context ctx = new Context();
                ctx.ClientIP = IPAddress.None;
                ctx.Channel = channel1;

                ctx.Query = new QueryMessage {Command = "dbx-Upload"};
                ctx.Query.Params["username"] = "test-user";
                ctx.Query.Params["uuid"] = "safevault";
                ctx.Query.Params["password"] = "1234567890";
                ctx.Query.Params["md5"] = Hash.MD5(dbxData);
                ctx.Query.Params["last-modified"] = "2017-01-01 12:00:00Z";

                Command.Process(ctx);

                stream1.Position = 0;
                channel2.CipherLib["rsa-private"] = RsaCipher
                    .LoadFromPEM($"{_location}/data/client/test-user/cer.pem", $"{_location}/data/client/test-user/cer.pem.key");

                var response = channel2.ReadObject<ResponseMessage>();
                Assert.AreEqual(200, response.StatusCode);
                Assert.AreEqual("OK", response.Header["data"]);

                var data = File.ReadAllBytes($"{_location}/data/client/test-user/dbx/safevault.dbx");
                Assert.AreEqual(dbxData, data);
                Assert.AreNotEqual(dbxData1, data);

                var fileInfo = new FileInfo($"{_location}/data/client/test-user/dbx/safevault.dbx");
                Assert.AreEqual(fileInfo.CreationTime, DateTime.Parse(ctx.Query.Params["last-modified"]));

                var data1 = File.ReadAllBytes($"{_location}/data/client/test-user/dbx/bak/safevault.0.dbx");
                Assert.AreEqual(dbxData1, data1);
                Assert.AreNotEqual(dbxData, data1);
            }
        }


        [Test]
        public void PingCommandTest()
        {
            var qm = new QueryMessage {Command = "ping"};

            using (var stream1 = new MemoryStream())
            using (var channel1 = new ServiceChannel())
            using (var channel2 = new ServiceChannel())
            {
                channel1.SetWriteStream(stream1, canDispose:false);
                var ctx = new Context();
                ctx.ClientIP = IPAddress.None;
                ctx.Channel = channel1;
                ctx.Query = qm;

                Command.Process(ctx);

                stream1.Position = 0;
                channel2.SetReadStream(stream1, canDispose:false);
                var response = channel2.ReadObject<ResponseMessage>();
                Assert.AreEqual(200, response.StatusCode);
                var data = DateTime.Parse(response.Header["data"]);

                Assert.AreEqual(0, (int)(DateTime.Now - data).TotalMinutes);
            }

            using (var stream1 = new MemoryStream())
            using (var channel = new ServiceChannel())
            {
                channel.SetWriteStream(stream1, canDispose:false);
                var ctx = new Context();
                ctx.ClientIP = IPAddress.None;
                ctx.Channel = channel;
                ctx.Query = qm;

                Assert.Catch<ArgumentException>(() => Command.Process(ctx));
            }
        }

        [Test]
        public void DbxSetKeyCommandTest010()
        {
            string newValue = Guid.NewGuid().ToString();

            if (File.Exists($"{_location}/data/client/test-user/vault.conf.bak"))
                File.Delete($"{_location}/data/client/test-user/vault.conf.bak");

            File.Copy($"{_location}/data/client/test-user/vault.conf", $"{_location}/data/client/test-user/vault.conf.bak");
            try
            {
                using (var stream1 = new MemoryStream())
                using (var channel1 = new ServiceChannel())
                using (var channel2 = new ServiceChannel())
                {
                    channel1.SetWriteStream(stream1, canDispose: false);

                    Context ctx = new Context();
                    ctx.ClientIP = IPAddress.None;
                    ctx.Channel = channel1;

                    ctx.Query = new QueryMessage {Command = "dbx-SetKey"};
                    ctx.Query.Params["username"] = "test-user";
                    ctx.Query.Params["uuid"] = "safevault";
                    ctx.Query.Params["password"] = OneTimePassword.Get(Base32.Decode("12345678"), 0);
                    ctx.Query.Params["value"] = newValue;

                    Command.Process(ctx);

                    stream1.Position = 0;
                    channel2.SetReadStream(stream1, canDispose: false);
                    channel2.CipherLib["rsa-private"] = RsaCipher
                        .LoadFromPEM($"{_location}/data/client/test-user/cer.pem",
                            $"{_location}/data/client/test-user/cer.pem.key");

                    var response = channel2.ReadObject<ResponseMessage>();
                    Assert.AreEqual(200, response.StatusCode);
                    var data = response.Header["data"];

                    Assert.AreEqual("OK", data);
                }

                Unity.Resolve<TokenList>().Reset();

                using (var stream1 = new MemoryStream())
                using (var channel1 = new ServiceChannel())
                using (var channel2 = new ServiceChannel())
                {
                    channel1.SetWriteStream(stream1, canDispose:false);

                    Context ctx = new Context();
                    ctx.ClientIP = IPAddress.None;
                    ctx.Channel = channel1;

                    ctx.Query = new QueryMessage {Command = "dbx-GetKey"};
                    ctx.Query.Params["username"] = "test-user";
                    ctx.Query.Params["uuid"] = "safevault";
                    ctx.Query.Params["password"] = OneTimePassword.Get(Base32.Decode("12345678"), 0);

                    Command.Process(ctx);

                    stream1.Position = 0;
                    channel2.SetReadStream(stream1, canDispose:false);
                    channel2.CipherLib["rsa-private"] = RsaCipher
                        .LoadFromPEM($"{_location}/data/client/test-user/cer.pem", $"{_location}/data/client/test-user/cer.pem.key");

                    var response = channel2.ReadObject<ResponseMessage>();
                    Assert.AreEqual(200, response.StatusCode);
                    var data = response.Header["data"];

                    Assert.AreEqual(newValue, data);
                }

            }
            finally
            {
                File.Copy($"{_location}/data/client/test-user/vault.conf.bak", $"{_location}/data/client/test-user/vault.conf", true);
                File.Delete($"{_location}/data/client/test-user/vault.conf.bak");
            }
        }

        [Test]
        public void DbxGetKeyCommandTest010()
        {
            using (var stream1 = new MemoryStream())
            using (var channel1 = new ServiceChannel())
            using (var channel2 = new ServiceChannel())
            {
                channel1.SetWriteStream(stream1, canDispose:false);

                Context ctx = new Context();
                ctx.ClientIP = IPAddress.None;
                ctx.Channel = channel1;

                ctx.Query = new QueryMessage {Command = "dbx-GetKey"};
                ctx.Query.Params["username"] = "test-user";
                ctx.Query.Params["uuid"] = "safevault";
                ctx.Query.Params["password"] = OneTimePassword.Get(Base32.Decode("12345678"), 0);

                Command.Process(ctx);

                stream1.Position = 0;
                channel2.SetReadStream(stream1, canDispose:false);
                channel2.CipherLib["rsa-private"] = RsaCipher
                    .LoadFromPEM($"{_location}/data/client/test-user/cer.pem", $"{_location}/data/client/test-user/cer.pem.key");

                var response = channel2.ReadObject<ResponseMessage>();
                Assert.AreEqual(200, response.StatusCode);
                var data = response.Header["data"];

                Assert.AreEqual("1234567801234567890abcdefghiklmnopqvwxyz12345678012345678901234567890=", data);
            }
        }

        [Test]
        public void DbxGetKeyCommandTest020()
        {
            using (var stream1 = new MemoryStream())
            using (var channel1 = new ServiceChannel())
            using (var channel2 = new ServiceChannel())
            {
                channel1.SetWriteStream(stream1, canDispose:false);

                Context ctx = new Context();
                ctx.ClientIP = IPAddress.None;
                ctx.Channel = channel1;

                ctx.Query = new QueryMessage {Command = "dbx-GetKey"};
                ctx.Query.Params["username"] = "test-user";
                ctx.Query.Params["uuid"] = "safevault";
                ctx.Query.Params["password"] = OneTimePassword.Get(Base32.Decode("12345678"), 0);

                Command.Process(ctx);

                stream1.Position = 0;
                channel2.SetReadStream(stream1, canDispose:false);

                Assert.Catch<SecureChannelException>(() => channel2.ReadObject<ResponseMessage>());
            }
        }

        [Test]
        public void DbxGetKeyCommandTest030()
        {
            using (var channel = new ServiceChannel())
            {
                channel.SetWriteStream(new MemoryStream());

                // check invalid password length
                Context ctx = new Context();
                ctx.ClientIP = IPAddress.None;
                ctx.Channel = channel;
                ctx.Query = new QueryMessage {Command = "dbx-GetKey"};
                ctx.Query.Params["username"] = "test-user";
                ctx.Query.Params["uuid"] = "safevault";
                ctx.Query.Params["password"] = "1111111111111";
                
                Assert.Catch<ArgumentException>(() => Command.Process(ctx));

                // check invalid password 
                ctx = new Context();
                ctx.ClientIP = IPAddress.None;
                ctx.Channel = channel;
                ctx.Query = new QueryMessage {Command = "dbx-GetKey"};
                ctx.Query.Params["username"] = "test-user";
                ctx.Query.Params["uuid"] = "safevault";
                ctx.Query.Params["password"] = "abcdefh";

                Assert.Catch<ArgumentException>(() => Command.Process(ctx));

                // success request vault key
                ctx = new Context();
                ctx.ClientIP = IPAddress.None;
                ctx.Channel = channel;
                ctx.Query = new QueryMessage {Command = "dbx-GetKey"};
                ctx.Query.Params["username"] = "test-user";
                ctx.Query.Params["uuid"] = "safevault";
                ctx.Query.Params["password"] = OneTimePassword.Get(Base32.Decode("12345678"), 0);
                Command.Process(ctx);

                //same request with same xsfttoken should be failed
                var mQuery = ctx.Query;

                ctx = new Context();
                ctx.ClientIP = IPAddress.None;
                ctx.Channel = channel;
                ctx.Query = mQuery;
                Assert.Catch<ArgumentException>(() => Command.Process(ctx));

                //same request with same password should be failed
                ctx = new Context();
                ctx.ClientIP = IPAddress.None;
                ctx.Channel = channel;
                ctx.Query = mQuery;
                ctx.Query.XsfrToken = Guid.NewGuid().ToString();
                Assert.Catch<ArgumentException>(() => Command.Process(ctx));
            }
        }
    }
}