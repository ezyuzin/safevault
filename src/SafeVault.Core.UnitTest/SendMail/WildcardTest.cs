using System;
using System.IO;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using SafeVault.Configuration;
using SafeVault.Misc;
using SafeVault.Security;

namespace SafeVault.Core.UnitTest
{
    [TestFixture]
    public class WildcardTest
    {
        [SetUp]
        public void SetUp()
        {
            var location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Environment.CurrentDirectory = location;
        }

        [Test]
        public void Wildcard010Test()
        {
            Assert.AreEqual(true, Wildcard.Match("Hello*", "Hello"));
            Assert.AreEqual(true, Wildcard.Match("Hello.*", "Hello"));
            Assert.AreEqual(true, Wildcard.Match("Hello.*", "Hello."));
            Assert.AreEqual(false, Wildcard.Match("Hello.Abc.*", "Hello."));

            Assert.AreEqual(true, Wildcard.Match("Hello.*", "Hello.Abc"));
            Assert.AreEqual(false, Wildcard.Match("Hello.*.Abc", "Hello.Abc"));
            Assert.AreEqual(true, Wildcard.Match("Hello.*.Abc", "Hello.Def.Abc"));
            Assert.AreEqual(true, Wildcard.Match("Hello.*.Abc", "Hello..Abc"));
            Assert.AreEqual(false, Wildcard.Match("Hello.*.Abc", "Hello..Abcdefh"));
            Assert.AreEqual(true, Wildcard.Match("Hello.*.Abc", "Hello.12345.Abc"));



        }
    }
}