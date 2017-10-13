using System;
using System.IO;
using System.Net.Mail;
using System.Reflection;
using System.Threading;
using NUnit.Framework;
using SafeVault.Configuration;
using SafeVault.Net.SendMail;
using SafeVault.Threading;

namespace SafeVault.Core.Threading
{
    [TestFixture]
    public class QueueProcessorTest
    {
        public IConfig Config { get; set; }

        [SetUp]
        public void SetUp()
        {
            var location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Environment.CurrentDirectory = location;
        }

        [Test]
        public void QueueProcessorTest010()
        {
            QueueProcessor<string> queue = new QueueProcessor<string>("string", str => 
            { 
                Console.WriteLine($"{DateTime.Now} Renderer: {str}");
                Thread.Sleep(200);
            });

            for(int i=0; i < 70; i++)
                queue.Append($"Item {i}");

            Thread.Sleep(2000);
            Console.WriteLine($"{DateTime.Now} dispose thread");
            queue.Dispose();
            Console.WriteLine($"{DateTime.Now} continue thread");
        }
    }
}