using System;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SafeVault.Logger;
using SafeVault.Net.SendMail;
using SafeVault.Service.Command.Models;
using SafeVault.Unity;

namespace SafeVault.Service.Notification
{
    public class EmailNotification
    {
        private static ILog Logger = Log.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [Dependency]
        public ISendMail SendMail { get; set; }

        public async Task SendAsync(Context ctx, NotificationType notifyType)
        {
            if (string.IsNullOrEmpty(ctx.Userprofile.Email))
                return;

            switch (notifyType)
            {
                case NotificationType.GrantAccess:
                {
                    var subject = "[SafeVault] Access Granted";
                    var body = $@"
Hello,
Somebody, may be you
Requested database Key '{ctx.QueryUUID}' for Account '{ctx.Username}'

---------------------------
Timestamp: {DateTime.UtcNow.ToString("u")}
IP: {ctx.ClientIP}
";
                    await SendAsync(ctx.Userprofile, subject, body);
                    break;
                }

                case NotificationType.GrantAccessConfirmation:
                {
                    var subject = "[SafeVault] Grant Access Confirmation";
                    var body = $@"
Hello,
Somebody, may be you
Requested database Key '{ctx.QueryUUID}' for Account '{ctx.Username}'

Please Grant Access by clicking the follow link: https://safevault.puppa-pro.com/api/confirmation?uuid=
---------------------------
Timestamp: {DateTime.UtcNow.ToString("u")}
IP: {ctx.ClientIP}
";
                    await SendAsync(ctx.Userprofile, subject, body);
                    break;
                }

                case NotificationType.BadPassword:
                    break;
                
            }
        }

        public async Task SendAsync(Userprofile profile, string title, string body)
        {
            try
            {
                using (var message = new MailMessage())
                {
                    message.To.Add(new MailAddress(profile.Email));
                    message.SubjectEncoding = Encoding.UTF8;
                    message.Subject = title;

                    message.BodyEncoding = Encoding.UTF8;
                    message.Body = body;

                    SendMail.Post(message);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                if (Logger.DebugEnabled)
                    Logger.Debug(e.StackTrace);
            }
        }
    }
}