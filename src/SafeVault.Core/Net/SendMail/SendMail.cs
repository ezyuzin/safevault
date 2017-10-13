using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using SafeVault.Configuration;
using SafeVault.Logger;
using SafeVault.Cache;

namespace SafeVault.Net.SendMail
{
    public class SendMail : ISendMail
    {
        private static ILog Logger = Log.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private Cache<SendMailHostData, string> _blackHosts = new Cache<SendMailHostData, string>();

        public SendMailHostData[] SendMailHost { get; set; }

        public SendMail()
        {
        }

        public SendMail(IConfig config)
        {
            var conf = config.Get<SendMailConf>();
            SendMailHost = conf.SmtpHost;
        }

        public bool Post(MailMessage message)
        {
            var enabledSmtp = SendMailHost.Where(m => _blackHosts.Get(m) != "1").ToArray();
            enabledSmtp = (enabledSmtp.Length > 0) ? enabledSmtp : SendMailHost;

            foreach (var smtpHost in enabledSmtp)
            {
                var success = (Post(smtpHost, message));
                _blackHosts.Add(smtpHost, success ? "1" : "0", TimeSpan.FromMinutes(10));
                if (success)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Send Email 
        /// </summary>
        /// <param name="sendMailHost"></param>
        /// <param name="message">Message to send</param>
        public virtual bool Post(SendMailHostData sendMailHost, MailMessage message)
        {
            try
            {
                if (!string.IsNullOrEmpty(sendMailHost.From))
                    message.From = new MailAddress(sendMailHost.From);

                var client = new SmtpClient(sendMailHost.Address, sendMailHost.Port);
                try
                {
                    client.Timeout = 10;
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(sendMailHost.Username, sendMailHost.Password);
                    client.EnableSsl = sendMailHost.UseSSL;

                    #if NETSTANDARD2_0
                    if (client.SendMailAsync(message).Wait(TimeSpan.FromSeconds(client.Timeout)))
                        return true;

                    throw new TimeoutException();
                    #endif

                    #if NETFX
                    using (var send = new SendMailWorker())
                    {
                        send.Run(() => client.Send(message));
                        if (send.WaitOne(TimeSpan.FromSeconds(client.Timeout)) == false)
                            throw new TimeoutException();
                    }
                    return true;
                    #endif
                }
                finally
                {
                    #if NETSTANDARD2_0
                    client.Dispose();
                    #endif
                }
            }
            catch (SmtpException ex)
            {
                Logger.Error(ex.Message + ex.InnerException + ex.StatusCode);
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error($"Unable send email through smtp {sendMailHost.Address}:{sendMailHost.Port}");
                if (Logger.DebugEnabled)
                    Logger.Debug(ex.ToString());

                return false;
            }
        }
    }
}