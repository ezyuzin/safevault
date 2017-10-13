using System.Reflection;
using NUnit.Framework;
using SafeVault.Service.Command.Models;

namespace SafeVault.Service.UnitTest.Services
{
    [TestFixture]
    public class UserprofileTest
    {
        [Test]
        public void Userprofile010Test()
        {
            var location = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            Userprofile userprofile = new Userprofile();
            userprofile.Path = $"{location}/data/client/test-user";
            userprofile.LoadProfile();

            Assert.AreEqual("evgeny.zyuzin@gmail.com", userprofile.GetValue("email"));
            Assert.AreEqual("12345678", userprofile.GetValue("secret-key"));
            Assert.AreEqual("md5:E807F1FCF82D132F9BB018CA6738A19F", userprofile.GetValue("password"));
        }
    }
}