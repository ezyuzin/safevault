
using System;
using System.Linq;
using System.Text;
using SafeVault.Configuration;
using SafeVault.Security.Ciphers;

namespace SafeVault.Net.SendMail
{
    public class SendMailConf : IConfigData
    {
        public SendMailHostData[] SmtpHost { get; set; }

        public void Import(IConfig config)
        {
            SmtpHost = config.GetSections("sendmail/smtp").Select(host =>
            {
                SendMailHostData hostData = new SendMailHostData();
                hostData.Address = host.Get("address");
                hostData.Port = int.Parse(host.Get("port"));
                hostData.UseSSL = host.Get("ssl", false) == "true";
                hostData.Username = host.Get("username", false);
                hostData.Password = GetPassword(host);

                hostData.From = host.Get("from", false);
                return hostData;
            })
            .ToArray();
        }

        private static string GetPassword(IConfigSection host)
        {
            var password = host.Get("password", false);
            if (string.IsNullOrEmpty(password))
                return password;

            using (var cipher = new Aes256Cipher(Encoding.UTF8.GetBytes("he1sQWc8SSPpkdIA")))
            {
                if (password.StartsWith("enc:"))
                {
                    password = password.Substring(4);
                    var data = Convert.FromBase64String(password);
                    return Encoding.UTF8.GetString(cipher.Decrypt(data));
                }
                else
                {
                    var data = cipher.Encrypt(Encoding.UTF8.GetBytes(password));
                    host.Set("password", "enc:" + Convert.ToBase64String(data));
                    host.Config.Save();
                    return password;
                }
            }
        }
    }
}