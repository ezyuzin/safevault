using System;

namespace SafeVault.Service.Authentificate
{
    [Flags]
    public enum AuthenticateType
    {
        Undefined,
        OneTimeToken,
        Password
    }
}