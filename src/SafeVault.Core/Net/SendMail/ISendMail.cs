using System.Net.Mail;

namespace SafeVault.Net.SendMail
{
    public interface ISendMail
    {
        bool Post(MailMessage message);
    }
}