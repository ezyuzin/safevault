using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using KeePassLib.Security;
using NUnit.Framework;
using SafeVault.Client;
using SafeVault.Configuration;
using SafeVault.Exceptions;

namespace SafeVaultKeyProvTest
{
    [TestFixture]
    public class SafeVaultQueryTest
    {
        [Test]
        public void InternalVisibleTo()
        {
            var assembly = Assembly.GetExecutingAssembly();
            Console.WriteLine(@"[assembly: InternalsVisibleTo(""{0}, PublicKey={1}"")]",
                assembly.GetName().Name,
                string.Concat(assembly.GetName().GetPublicKey().Select(b => b.ToString("X2")).ToArray()));
        }

        [Test]
        public void GetTokenTest()
        {
            SafeVaultConf qc = new SafeVaultConf();
            qc.ServerUrl = "https://test.safevault.com/safevault/api.cgi";
            qc.ServerCertificateName = "SafeVault Server Certificate";
            qc.ClientCertificateName = "Evgeny Zyuzin SafeVault Client";
            qc.Username = "ezyuzin";

            SafeVaultWebClient wc = new SafeVaultWebClient(qc);
            wc.GetToken();
        }

        [Test]
        public void UploadTest()
        {
            SafeVaultConf qc = new SafeVaultConf();
            qc.ServerUrl = "https://test.safevault.com/safevault/api.cgi";
            qc.ServerCertificateName = "SafeVault Server Certificate";
            qc.ClientCertificateName = "Evgeny Zyuzin SafeVault Client";
            qc.Username = "ezyuzin";
            qc.SyncPassword = new ProtectedString(false, "000000");

            SafeVaultWebClient wc = new SafeVaultWebClient(qc);
            var uuid = Guid.NewGuid().ToString();
            wc.Upload(uuid, new byte[256], DateTime.UtcNow.ToString());
        }

        [Test]
        public void DownloadTest()
        {
            SafeVaultConf qc = new SafeVaultConf();
            qc.ServerUrl = "https://test.safevault.com/safevault/api.cgi";
            qc.ServerCertificateName = "SafeVault Server Certificate";
            qc.ClientCertificateName = "Evgeny Zyuzin SafeVault Client";
            qc.Username = "ezyuzin";
            qc.SyncPassword = new ProtectedString(false, "000000");

            SafeVaultWebClient wc = new SafeVaultWebClient(qc);
            var uuid = "ac030f4f-3dba-491f-9019-e31ff3d01540";
            wc.Download(uuid);
        }

        
        [Test]
        public void VaultKeynameNotSet010Test()
        {
            SafeVaultConf qc = new SafeVaultConf();
            qc.ServerUrl = "https://test.safevault.com/safevault/api.cgi";
            qc.ServerCertificateName = "SafeVault Server Certificate";
            qc.ClientCertificateName = "Evgeny Zyuzin SafeVault Client";
            qc.Username = "ezyuzin";

            SafeVaultWebClient wc = new SafeVaultWebClient(qc);
            string vaultKeyname = "";

            qc.Username = "";
            AssertException<SafeVaultHttpException>(() => { wc.GetValue(vaultKeyname, "12345"); }, (ex) => {
                Assert.AreEqual(400, (int)ex.StatusCode, "NoUsername expected BadRequest Response");
            });

            qc.Username = "ezyuzin1";
            AssertException<SafeVaultHttpException>(() => { wc.GetValue(vaultKeyname, "12345"); }, (ex) => {
                Assert.AreEqual(400, (int)ex.StatusCode, "Unknown Username Expected Unauthorized Response");
            });

            qc.Username = "ezyuzin";
            AssertException<SafeVaultHttpException>(() => { wc.GetValue(vaultKeyname, "12345"); }, (ex) => {
                Assert.AreEqual(400, (int)ex.StatusCode, "HttpCode Unauthorized Expected");
            });


        }

        [Test]
        public void UsernameTest020Test()
        {

            SafeVaultConf qc = new SafeVaultConf();
            qc.ServerUrl = "https://test.safevault.com/safevault/api.cgi";
            qc.ServerCertificateName = "SafeVault Server Certificate";
            qc.ClientCertificateName = "Evgeny Zyuzin SafeVault Client";
            qc.Username = "ezyuzin";

            SafeVaultWebClient wc = new SafeVaultWebClient(qc);

            string vaultKeyname = "0ed37546-f597-4526-b835-f5a0425ca2e0";
            qc.Username = "";
            AssertException<SafeVaultHttpException>(() => { wc.GetValue(vaultKeyname, "12345"); }, (ex) => {
                Assert.AreEqual(400, (int)ex.StatusCode, "NoUsername expected BadRequest Response");
            });

            qc.Username = "ezyuzin1";
            AssertException<SafeVaultHttpException>(() => { wc.GetValue(vaultKeyname, "12345"); }, (ex) => {
                Assert.AreEqual(401, (int)ex.StatusCode, "Unknown Username Expected Unauthorized Response");
            });

            qc.Username = "ezyuzin";
            AssertException<SafeVaultResponseException>(() => { wc.GetValue(vaultKeyname, "000001"); }, (ex) =>
            {
                Assert.AreEqual("InvalidPassword", ex.Status);
            });

            var value = wc.GetValue(vaultKeyname, "000000");
            Assert.AreNotEqual("", value);
            Console.WriteLine(value);
        }

        public void AssertException<T>(Action action, Action<T> exc) where T : Exception
        {
            try
            {
                action.Invoke();
                throw new AssertionException(string.Format("Exception<{0}> expected", typeof(T).Name));
            }
            catch (T ex)
            {
                exc.Invoke(ex);
            }
        }
    }
}
