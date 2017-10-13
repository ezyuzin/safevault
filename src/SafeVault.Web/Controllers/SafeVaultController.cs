using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SafeVault.Configuration;
using SafeVault.Contracts;
using SafeVault.Exceptions;
using SafeVault.Logger;
using SafeVault.Security;
using SafeVault.Service.BanList;
using SafeVault.Service.Command.Models;
using SafeVault.Service.Configuration;
using SafeVault.Transport;
using SafeVault.Transport.Exceptions;
using SafeVault.Transport.Models;
using SafeVault.Unity;
using ArgumentException = System.ArgumentException;

namespace SafeVault.Web.Controllers
{
    [Route("api/safevault")]
    public class SafeVaultController : Controller
    {
        private static ILog Logger = Log.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public ServiceConf Conf { get; set; }

        [Dependency]
        public IConfig Config { get; set; }

        [Dependency]
        public ICommandProcessor Command { get; set; }

        [Dependency]
        public BanList BanList { get; set; }

        [Dependency]
        public X509Store X509Store { get; set; }

        public SafeVaultController()
        {
            Service.Unity.BuildUp(GetType(), this);
            Conf = Config.Get<ServiceConf>();
        }


        // GET api/values
        [HttpPost]
        public async Task<IActionResult> Get()
        {
            if (Request.HttpContext.Request.ContentType != "application/encrypted-data")
                return BadRequest();

            var context = new Context();

            using (var channel = new ServiceChannel())
            {
                context.ClientIP = IPAddress.None;
                context.Channel = channel;

                var responseStream = new MemoryStream();
                try
                {
                    context.ClientIP = GetClientIP();
                    channel.SetWriteStream(responseStream, false);

                    channel.SetReadStream(Request.HttpContext.Request.Body, canDispose: false);
                    channel.CipherLib["rsa-private"] = X509Store.GetCertificate(Conf.Certificate).Clone();

                    context.Query = channel.ReadObject<QueryMessage>();
                    channel.Encrypt();

                    Command.Process(context);
                }
                catch (Exception e)
                {
                    responseStream?.Dispose();
                    responseStream = ExceptionHandle(e, context);
                }

                responseStream.Position = 0;
                return File(responseStream, "application/encrypted-data");
            }
        }

        private static MemoryStream ExceptionHandle(Exception e, Context ctx)
        {
            int statusCode = 500;
            var type = e.GetType();
            var types = new[] {
                typeof(ReadSecureChannelException),
                typeof(ArgumentException), 
                typeof(BadRequestException)};

            if (types.Any(type1 => type1 == type || type1.IsSubclassOf(type)))
                statusCode = 400;

            Logger.Error($"IP:{ctx.ClientIP}| {e.GetType().Name} - {e.Message}");
            if (statusCode == 500 && Logger.DebugEnabled)
                Logger.Debug(e.StackTrace);

            
            var responseStream = new MemoryStream();
            ctx.Channel.SetWriteStream(responseStream, false);

            ctx.Channel.Encrypt(true);
            ctx.Channel.WriteObject(new ResponseMessage
            {
                StatusCode = statusCode,
                StatusText = e.Message,
                Timestamp = DateTime.UtcNow
            });
            return responseStream;
        }

        private IPAddress GetClientIP()
        {
            var remoteIP = HttpContext.Connection.RemoteIpAddress;

            if (Conf.ReverseProxyIP.Any(ip => remoteIP.Equals(ip)))
            {
                Logger.Debug("Reverse Proxy Request");
                var realIP = Request.Headers["X-Real-IP"];
                if (string.IsNullOrEmpty(realIP))
                    throw new HttpChannelException("Proxy not provide ClientIP");

                remoteIP = IPAddress.Parse(realIP);
            }

            if (BanList.IsBanned(remoteIP))
            {
                throw new BadRequestException("Access Denied");
            }
            return remoteIP;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}
