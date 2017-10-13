using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using SafeVault.Configuration;
using SafeVault.Logger;

namespace SafeVault.Core.UnitTest.Logger
{
    [TestFixture]
    public class LoggerTest
    {
        public IConfig Config { get; set; }

        [SetUp]
        public void SetUp()
        {
            var location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Environment.CurrentDirectory = location;
            Config = new AppConfig("data/logger.config");
        }

        [Test]
        public void LoggerTest010()
        {
            using (LogManager lm = new LogManager(Config))
            {
                var logger = lm.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
                logger.Info("HELLO1");
                logger.Info("HELLO2");
                logger.Info("HELLO3");
                logger.Debug("HELLO");
            }
        }
    }
}