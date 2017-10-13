using NUnit.Framework;
using SafeVault.Exceptions;

namespace SafeVault.Service.UnitTest.Services
{
    [TestFixture]
    public class TokenListTest
    {
        [Test]
        public void TokenList010Test()
        {
            using (var tokenList = new TokenList())
            {
                tokenList.UseToken("12345");
                tokenList.UseToken("123456");
                tokenList.UseToken("1234567");
                Assert.Catch<ArgumentException>(() => tokenList.UseToken("12345"));
                Assert.Catch<ArgumentException>(() => tokenList.UseToken("123456"));
                Assert.Catch<ArgumentException>(() => tokenList.UseToken("1234567"));
                tokenList.UseToken("12345678");
                tokenList.UseToken("123456789");
                tokenList.UseToken("1234567890");
            }
        }
    }
}