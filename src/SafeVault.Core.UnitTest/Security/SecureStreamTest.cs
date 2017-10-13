using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using SafeVault.Security;
using SafeVault.Transport;

namespace SafeVault.Core.UnitTest
{
    [TestFixture]
    public class SecureStreamTest
    {
        public void TransferData(EncryptedStream source, EncryptedStream target)
        {
            var pos = target.Position;
            source.Position = 0;

            Stream a = source.GetRawStream();
            Stream b = target.GetRawStream();

            byte[] buf = new byte[1024];
            while (true)
            {
                int nbytes = a.Read(buf, 0, buf.Length);
                if (nbytes == 0)
                    break;

                b.Write(buf, 0, nbytes);
            }
            source.SetLength(0);

            target.Position = pos;
        }

        [Test]
        public void SecureReadWriteTest()
        {
            var location = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            using(var stream1 = new MemoryStream())
            using(var stream2 = new MemoryStream())
            using (var requestSend = new EncryptedStream(stream1, canDispose:false))
            using (var requestReceive = new EncryptedStream(stream2, canDispose:false))
            {
                byte[] data = Encoding.UTF8.GetBytes("changeit");
                requestSend.Write(data, 0, data.Length);
                requestSend.Flush();

                var streamData = stream1.ToArray();
                Console.WriteLine(string.Join(" ", data.Select(m => $"{m:X2}").ToArray()));

                Console.WriteLine(streamData.Length);
                Console.WriteLine(string.Join(" ", streamData.Select(m => $"{m:X2}").ToArray()));

                TransferData(requestSend, requestReceive);
                byte[] data1 = new byte[data.Length];
                int count = requestReceive.Read(data1, 0, data1.Length);
                Assert.AreEqual(data.Length, count);

                var str = Encoding.UTF8.GetString(data1);
                Assert.AreEqual("changeit", str);
            }
        }
    }
}