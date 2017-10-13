using System;
using SafeVault.Configuration;
using SafeVault.Contracts;

namespace SafeVault.Service.Configuration
{
    public class StorageConf : IConfigData
    {
        public string DataPath { get; set; }
        public string UserData { get; set; }

        public virtual void Import(IConfig config)
        {
            DataPath = config.Get("safevault/datapath");
            UserData = config.Get("safevault/userdata");
        }
    }
}