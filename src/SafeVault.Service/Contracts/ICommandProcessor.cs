using SafeVault.Service;
using SafeVault.Service.Command.Models;
using SafeVault.Transport;
using SafeVault.Transport.Models;

namespace SafeVault.Contracts 
{
    public interface ICommandProcessor
    {
        void Process(Context ctx);
    }
}