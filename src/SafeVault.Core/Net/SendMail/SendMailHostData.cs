using System;

namespace SafeVault.Net.SendMail
{
    public class SendMailHostData
    {
        public string From { get; set; }

        public string Address { get; set; }
        public int Port { get; set; }
        public bool UseSSL { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}