using System.Collections.Generic;
using System.Linq;
using System.Net;
using SafeVault.Configuration;
using SafeVault.Contracts;

namespace SafeVault.Service.Configuration 
{
    public class ServiceConf : IConfigData
    {
        public string[] Bindings { get; set; }
        public string Certificate { get; set; }
        public IPAddress[] ReverseProxyIP { get; set; }

        public virtual void Import(IConfig config)
        {
            Certificate = config.Get("safevault/certificate");
            Bindings = config.Get("safevault/bindings/address")
                .Split(";".ToCharArray())
                .Select(m => m.Trim())
                .Where(m => m != "")
                .ToArray();

            ReverseProxyIP = config.GetValues($"safevault/reverseproxy/address")
                .Select(address => IPAddress.Parse(address))
                .ToArray();
        }
    }
}