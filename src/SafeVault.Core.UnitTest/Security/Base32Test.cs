using System;
using System.Text;
using NUnit.Framework;
using SafeVault.Security;

namespace SafeVault.Core.UnitTest
{
    [TestFixture]
    public class Base32Test
    {
        [Test]
        public void Base32_010Test()
        {
            Assert.AreEqual("JBCUYTCP", Base32.Encode(Encoding.ASCII.GetBytes("HELLO")));
            Assert.AreEqual("JBCUYTCPFVEEKTCMJ4WUQRKMJRHQ====", Base32.Encode(Encoding.ASCII.GetBytes("HELLO-HELLO-HELLO")));
        }

        [Test]
        public void Base32_020Test()
        {
            byte[] bytes;
            bytes = Base32.Decode("JBCUYTCPFVEEKTCMJ4WUQRKMJRHQ====");
            Assert.AreEqual(Encoding.ASCII.GetBytes("HELLO-HELLO-HELLO"), bytes);
            
            bytes = Base32.Decode("JBCUYTCP");
            Assert.AreEqual(Encoding.ASCII.GetBytes("HELLO"), bytes);
        }

        [Test]
        public void Base32_030Test()
        {
            var s = Base32.Encode(new byte[] {0x00});
            Console.WriteLine(s);

            var s1 = Base32.Decode(s);
            Assert.AreEqual(1, s1.Length);
            Assert.AreEqual(0x00, s1[0]);

            s = Base32.Encode(Encoding.UTF8.GetBytes("A"));
            Console.WriteLine(s);

            //"ONQXC53FOF3WK4LXONQWIZTEONSGM6DDOZ4GG5TYONSGM43EMZ3XOZLSO5SQ===="
            
        }
    }
}