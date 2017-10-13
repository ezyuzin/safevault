using System;
using System.IO;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Threading;
using NUnit.Framework;
using SafeVault.Configuration;
using SafeVault.Logger;
using SafeVault.Net.SendMail;
using SafeVault.Security;

namespace SafeVault.Core.UnitTest
{
    [TestFixture]
    public class SmtpTest
    {
        public IConfig Config { get; set; }

        [SetUp]
        public void SetUp()
        {
            var location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Environment.CurrentDirectory = location;
            Config = new AppConfig("data/smtp.config");
        }

        [Test]
        public void SendMailTest020()
        {
            var conf = Config.Get<SendMailConf>();
            Console.WriteLine(File.ReadAllText("data/smtp.config"));
        }

        [Test]
//        [Ignore("")]
        public void SendMailTest010()
        {
            var sendMail = new SendMail(Config);
            sendMail.Post(new MailMessage(
                "ezyuzin@yandex.ru",
                "evgeny.zyuzin@gmail.com",
                "TestMail Subject",
                "TestMail Body"
            ));
        }
    }
}