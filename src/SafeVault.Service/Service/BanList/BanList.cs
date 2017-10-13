using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using SafeVault.Cache;
using SafeVault.Configuration;
using SafeVault.Exceptions;
using SafeVault.Logger;
using SafeVault.Service.Configuration;

namespace SafeVault.Service.BanList
{
    public class BanList
    {
        private static ILog Logger = Log.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        
        private List<IPAddress> _banned = new List<IPAddress>();
        private Cache<IPAddress, IPAttempt> _cache;

        public string BanListPath { get; set; }
        
        public BanList()
        {
            _cache = new Cache<IPAddress, IPAttempt>();
            _cache.TTL = TimeSpan.FromMinutes(30);
        }

        public BanList(IConfig config) : this()
        {
            var conf = config.Get<StorageConf>();
            BanListPath = $"{conf.DataPath}/banlist.dat";

            if (File.Exists(BanListPath))
            {
                _banned = File.ReadAllLines(BanListPath)
                    .Select(m => IPAddress.TryParse(m, out var address) ? address : null)
                    .Where(m => m != null)
                    .ToList();
            }
        }

        public void BlockIP(IPAddress clientIP, string reason)
        {
            _banned.Add(clientIP);
            Logger.Warn($"IP: {clientIP}| Banned with reason: {reason}");

            File.AppendAllText(BanListPath, $"{clientIP}\r\n");
        }

        public void RequestCount(IPAddress clientIP)
        {
            var info = GetIPInfo(clientIP);
            if ((DateTime.Now - info.LastRequest).TotalSeconds > 10)
                info.RequestCount = 0;

            int count = Interlocked.Increment(ref info.RequestCount);
            if (count >= 500)
            {
                BlockIP(clientIP, "Too much requests");
                throw new BadRequestException("Too much requests");
            }
        }

        public void BadPassword(IPAddress clientIP)
        {
            var info = GetIPInfo(clientIP);
            int count = Interlocked.Increment(ref info.BadPasswordCount);
            if (count >= 5)
                BlockIP(clientIP, "Password attempts was exceeded");
        }

        private IPAttempt GetIPInfo(IPAddress clientIP)
        {
            return _cache.GetOrAdd(clientIP, TimeSpan.FromMinutes(15), s =>
            {
                return new IPAttempt {IP = clientIP};
            });
        }

        public bool IsBanned(IPAddress clientIP)
        {
            return (_banned.Any(ip => ip.Equals(clientIP)));
        }
    }
}