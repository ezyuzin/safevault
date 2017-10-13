using System;
using NUnit.Framework;
using SafeVault.Exceptions;
using SafeVault.Misc;

namespace SafeVault.Service.UnitTest.Services
{
    [TestFixture]
    public class OneTimePasswordTest
    {
        [Test]
        public void OneTimePassword010Test()
        {
            Assert.AreEqual("570603", OneTimePassword.Calculate("AA", 702100, 6));
            Assert.AreEqual("501433", OneTimePassword.Calculate("AA", 702200, 6));
            Assert.AreEqual("122310", OneTimePassword.Calculate("AA", 702203, 6));

            //for (int i = -1; i <= 1; i++)
            //{
            //    var passw = OneTimePassword.Get("AA", i);
            //    Console.WriteLine(passw);
            //}
        }

        [Test]
        public void OneTimePassword020Test()
        {
            var value = DateTime.UtcNow.ToString("u");

            DateTime lastModified;
            if (DateTime.TryParse(value, out lastModified) == false)
                throw new BadRequestException($"Bad {value}");

            Console.WriteLine(lastModified.ToString());
        }

        
    }
}