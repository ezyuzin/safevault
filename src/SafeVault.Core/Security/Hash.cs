using System.Linq;
using System.Text;

namespace SafeVault.Security
{
    public class Hash
    {
        public static string MD5(string content)
        {
            return Hash.MD5(Encoding.UTF8.GetBytes(content));
        }

        public static string MD5(byte[] data)
        {
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                var hash1 = md5.ComputeHash(data);
                return string.Join("", hash1.Select(m => $"{m:X2}").ToArray());
            }
        }
    }
}