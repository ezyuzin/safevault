using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using SafeVault.Configuration;
using SafeVault.Logger;
using SafeVault.Service;
using SafeVault.Service.Configuration;
using SafeVault.Unity;

namespace SafeVault.Web
{
    public class Service
    {
        private static SafevaultService _service;
        public static IUnity Unity { get { return _service.Unity; } }

        public static void Main(string[] args)
        {
            _service = new SafevaultService();
            try
            {
                _service.Startup();
                BuildWebHost(args).Run();
            }
            finally
            {
                _service.Dispose();
            }
        }

        public static IWebHost BuildWebHost(string[] args)
        {
            var serviceConf = Unity.Resolve<IConfig>()
                    .Get<ServiceConf>();

            return WebHost.CreateDefaultBuilder(args)
                    .UseStartup<Startup>()
                    .UseUrls(serviceConf.Bindings)
                    .Build();
        }
    }
}
