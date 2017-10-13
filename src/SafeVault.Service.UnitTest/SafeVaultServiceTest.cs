using NUnit.Framework;

namespace SafeVault.Service.UnitTest
{
    [TestFixture]
    public class SafeVaultServiceTest
    {
        [Test]
        public void ValidateTest010()
        {
            using (var service = new SafevaultService())
            {
                service.Validate();
            }
        }
    }
}