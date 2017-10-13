using SafeVault.Service.Authentificate;
using SafeVault.Service.Command.Models;

namespace SafeVault.Contracts
{
    public interface IAuthenticate
    {
        void Authenticate(Context ctx, AuthenticateType authType);
    }
}