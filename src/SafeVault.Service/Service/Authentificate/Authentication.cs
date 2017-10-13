using System.Reflection;
using System.Text.RegularExpressions;
using SafeVault.Contracts;
using SafeVault.Exceptions;
using SafeVault.Logger;
using SafeVault.Misc;
using SafeVault.Security;
using SafeVault.Service.Command.Models;
using SafeVault.Unity;

namespace SafeVault.Service.Authentificate {

    public class Authentication : IAuthenticate
    {
        private static ILog Logger = Log.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [Dependency]
        public TokenList TokenList { get; set; }
        [Dependency]
        public BanList.BanList BanList { get; set; }

        public void Authenticate(Context ctx, AuthenticateType authType)
        {
            var passw = ctx.QueryParam("password");

            bool authenticated = false;
            
            if ((authType & AuthenticateType.OneTimeToken) != 0)
                authenticated = AuthenticateWithOneTimePassword(ctx.Userprofile, passw);

            if (!authenticated && (authType & AuthenticateType.Password) != 0)
            {
                authenticated = (ctx.Userprofile.Password.StartsWith("md5:"))
                    ? ctx.Userprofile.Password.Substring(4) == Hash.MD5(passw)
                    : ctx.Userprofile.Password == passw;
            }

            if (!authenticated)
            {
                throw new AccessDeniedException("Bad Password");
            }

        }

        private bool AuthenticateWithOneTimePassword(Userprofile profile, string password)
        {
            if (password.Length > 8 || password != Regex.Replace(password, @"[^0-9]", ""))
                return false;

            bool authenticated = false;
            var secret = Base32.Decode(profile.SecretKey);
            for (int i = 0; !authenticated && i < 3; i++)
            {
                var passw = OneTimePassword.Get(secret, i);
                authenticated = (passw == password);

                if (!authenticated && i != -i)
                {
                    passw = OneTimePassword.Get(secret, -i);
                    authenticated = (passw == password);
                }
            }

            if (authenticated)
                TokenList.UseToken($"otp/{profile.Username}/{password}");

            return authenticated;
        }
    }
}