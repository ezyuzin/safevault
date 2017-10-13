using System;
using System.Net;

namespace SafeVault.Service.BanList
{
    public class IPAttempt
    {
        public IPAddress IP { get; set; }
        public int BadPasswordCount;
        public int RequestCount;
        public DateTime LastRequest = DateTime.Now;
    }
}