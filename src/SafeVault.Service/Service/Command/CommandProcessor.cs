using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using SafeVault.Configuration;
using SafeVault.Contracts;
using SafeVault.Exceptions;
using SafeVault.Logger;
using SafeVault.Security;
using SafeVault.Service.Authentificate;
using SafeVault.Service.Command.Models;
using SafeVault.Service.Configuration;
using SafeVault.Service.Notification;
using SafeVault.Transport.Models;
using SafeVault.Unity;
using ArgumentException = SafeVault.Exceptions.ArgumentException;
using FileNotFoundException = SafeVault.Exceptions.FileNotFoundException;

namespace SafeVault.Service.Command
{
    public class CommandProcessor : ICommandProcessor
    {
        private static ILog Logger = Log.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public string UserDataPath { get; set; }

        [Dependency]
        public X509Store X509Store { get; set; }

        [Dependency]
        public BanList.BanList BanList { get; set; }

        [Dependency]
        public TokenList TokenList { get; set; }

        [Dependency]
        public IAuthenticate Authenticate { get; set; }

        [Dependency]
        public Notification.EmailNotification Notification { get; set; }

        public CommandProcessor()
        {
        }
        public CommandProcessor(IConfig config)
        {
            UserDataPath = config.Get<StorageConf>().UserData;
        }

        public void Process(Context ctx)
        {
            ctx.UserDataPath = UserDataPath;

            var query = ctx.Query;
            TokenList.UseToken($"xsfr/{query.XsfrToken}");
            BanList.RequestCount(ctx.ClientIP);

            try
            {
                ProcessCommand(ctx);
            }
            catch (AccessDeniedException)
            {
                BanList.BadPassword(ctx.ClientIP);
                throw;
            }
        }

        private void ProcessCommand(Context ctx)
        {
            switch (ctx.Query.Command)
            {
                case "ping":
                    PingCommand(ctx);
                    break;

                case "dbx-GetLastModified":
                    DbxGetLastModifiedCommand(ctx);
                    break;

                case "dbx-Download":
                    DbxDownloadCommand(ctx);
                    break;

                case "dbx-Upload":
                    DbxUploadCommand(ctx);
                    break;

                case "dbx-GetKey":
                    DbxGetKeyCommand(ctx);
                    break;

                case "dbx-SetKey":
                    DbxSetKeyCommand(ctx);
                    break;

                default:
                    throw new ArgumentException($"Unknown command '{ctx.Query.Command}'");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ctx"></param>
        public void DbxGetLastModifiedCommand(Context ctx)
        {
            var uuid = ctx.QueryUUID;
            Authenticate.Authenticate(ctx, AuthenticateType.Password);
            EncryptResponse(ctx);

            string dbxName = $"{ctx.Userprofile.Path}/dbx/{uuid}.dbx";
            if (!File.Exists(dbxName))
                throw new FileNotFoundException($"Database '{uuid}' not found");

            var dbxInfo = new FileInfo(dbxName);

            var response = new ResponseMessage();
            response.Header["data"] = dbxInfo.LastWriteTimeUtc.ToString("u");
            ctx.Channel.WriteObject(response);

            Logger.Info($"IP:{ctx.ClientIP} GetLastModified() for dbx '{uuid}' for user '{ctx.Username}'");
        }

        public void DbxUploadCommand(Context ctx)
        {
            var uuid = ctx.QueryUUID;
            var md5 = ctx.QueryParam("md5");
            var lastModified = ctx.QueryDateTimeParam("last-modified");

            Authenticate.Authenticate(ctx, AuthenticateType.Password);
            EncryptResponse(ctx);

            var data = ctx.Channel.Read();
            if (Hash.MD5(data) != md5)
                throw new BadRequestException("Bad MD5");

            string dbxName = $"{ctx.Userprofile.Path}/dbx/{uuid}.dbx";
            if (File.Exists(dbxName))
                FileCreateBackup(dbxName);

            if (!Directory.Exists($"{ctx.Userprofile.Path}/dbx"))
                Directory.CreateDirectory($"{ctx.Userprofile.Path}/dbx");

            //Logger.Info($"Db LastModified: {lastModified}");

            File.WriteAllBytes(dbxName, data);
            var dbxInfo1 = new FileInfo(dbxName);
            dbxInfo1.CreationTime = lastModified;
            dbxInfo1.LastWriteTime = lastModified;
            dbxInfo1.LastAccessTime = lastModified;

            var response = new ResponseMessage();
            response.Header["data"] = "OK";
            ctx.Channel.WriteObject(response);

            Logger.Info($"IP:{ctx.ClientIP} Upload dbx '{uuid}' for user '{ctx.Username}'");
        }

        public void DbxDownloadCommand(Context ctx)
        {
            var uuid = ctx.QueryUUID;
            Authenticate.Authenticate(ctx, AuthenticateType.Password);
            EncryptResponse(ctx);

            string dbxName = $"{ctx.Userprofile.Path}/dbx/{uuid}.dbx";
            if (!File.Exists(dbxName))
                throw new FileNotFoundException($"Database '{uuid}' not found");

            var data = File.ReadAllBytes(dbxName);

            var response = new ResponseMessage();
            response.Header["data"] = "OK";
            response.Header["md5"] = Hash.MD5(data);
            ctx.Channel.WriteObject(response);
            ctx.Channel.Write(data);

            Logger.Info($"IP:{ctx.ClientIP} Download dbx '{uuid}' for user '{ctx.Username}'");
        }

        public void PingCommand(Context ctx)
        {
            Logger.Info($"{ctx.ClientIP} requested 'ping'");
            var response = new ResponseMessage();
            response.Header["data"] = DateTime.UtcNow.ToString("u");
            ctx.Channel.WriteObject(response);
        }

        public void DbxSetKeyCommand(Context ctx)
        {
            var uuid = ctx.QueryUUID;
            var value = ctx.QueryParam("value");

            Authenticate.Authenticate(ctx, AuthenticateType.OneTimeToken);
            EncryptResponse(ctx);

            var vault = new ConfigFile($"{ctx.Userprofile.Path}/vault.conf");
            vault.SetValue(uuid, value);
            vault.Export($"{ctx.Userprofile.Path}/vault.conf");

            var response = new ResponseMessage();
            response.Header["data"] = "OK";
            ctx.Channel.WriteObject(response);
            Logger.Info($"IP:{ctx.ClientIP} Set new Dbx key '{uuid}' for user '{ctx.Username}'");
        }

        public void DbxGetKeyCommand(Context ctx)
        {
            var uuid = ctx.QueryUUID;
            Authenticate.Authenticate(ctx, AuthenticateType.OneTimeToken);
            EncryptResponse(ctx);

            var vaultConf = new ConfigFile($"{ctx.Userprofile.Path}/vault.conf");
            var value = vaultConf.GetValue(uuid, () => throw new BadRequestException("VaultKey not Found"));

            var response = new ResponseMessage();
            response.Header["data"] = value;
            ctx.Channel.WriteObject(response);
            Logger.Info($"IP:{ctx.ClientIP} requested key '{uuid}' for user '{ctx.Username}'");

            #pragma warning disable 4014
            if (!IsHomeNetwork(ctx.ClientIP))
                Notification.SendAsync(ctx, NotificationType.GrantAccess);
            #pragma warning restore 4014
        }

        public bool IsHomeNetwork(IPAddress address)
        {
            var clientIP = address.ToString();
            return Regex.IsMatch(clientIP, @"^(192\.168\.0\.\d{1,3}|127\.0\.0\.1)$");
        }

        private void EncryptResponse(Context ctx)
        {
            ctx.Channel.CipherLib["rsa-public"] = X509Store.GetCertificate($"{ctx.Userprofile.Path}/cer.pem").Clone();
            ctx.Channel.Encrypt();
        }

        private static void FileCreateBackup(string dbxName)
        {
            if (!File.Exists(dbxName))
                return;

            var location = Path.GetDirectoryName(dbxName);
            var filename = Path.GetFileNameWithoutExtension(dbxName);
            var ext = Path.GetExtension(dbxName);
            
            int ix = 0;
            string dbxBackup;
            while (true)
            {
                dbxBackup = $"{location}/bak/{filename}.{ix++}{ext}";
                if (!File.Exists(dbxBackup))
                    break;
            }

            if (!Directory.Exists($"{location}/bak"))
                Directory.CreateDirectory($"{location}/bak");

            File.Copy(dbxName, dbxBackup);
            var dbxInfo = new FileInfo(dbxName);
            var backupInfo = new FileInfo(dbxBackup);

            backupInfo.CreationTime = dbxInfo.CreationTime;
            backupInfo.LastWriteTime = dbxInfo.LastWriteTime;
            backupInfo.LastAccessTime = dbxInfo.LastAccessTime;
        }
    }
}