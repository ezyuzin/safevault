using System;
using System.Reflection;
using SafeVault.Configuration;
using SafeVault.Contracts;
using SafeVault.Logger;
using SafeVault.Net.SendMail;
using SafeVault.Security;
using SafeVault.Service.Authentificate;
using SafeVault.Service.Command;
using SafeVault.Unity;

namespace SafeVault.Service
{
    public class SafevaultService : IDisposable
    {
        private ILog Logger;
        public Unity.Unity Unity { get; private set; }

        public SafevaultService()
        {
            AppConfig app = new AppConfig();
            Log.Instance = new LogManager(app);
            Logger = Log.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

            Unity = new Unity.Unity();
            Unity.RegisterInstance<IConfig, AppConfig>(app);
            Unity.Register<ICommandProcessor, CommandProcessor>();
            Unity.Register<BanList.BanList, BanList.BanList>();
            Unity.Register<TokenList, TokenList>();
            Unity.Register<X509Store, X509Store>();
            Unity.Register<IAuthenticate, Authentication>();
            Unity.Register<ISendMail, SendMail>();
            Unity.Register<Notification.EmailNotification, Notification.EmailNotification>();
        }

        public void Validate()
        {
            Unity.Validate();
        }

        public void Startup()
        {
            Logger.Info("");
            Logger.Info("------------------------------");
            Logger.Info("Safevault Service Starting ...");
            Validate();
            Logger.Info(" ... Safevault Service Started");
            Logger.Info("");
        }

        public void Dispose()
        {
            Unity?.Dispose();
            Unity = null;
        }
    }
}