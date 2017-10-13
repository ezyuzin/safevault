using SafeVault.Configuration;
using SafeVault.Exceptions;
using SafeVault.Security;

namespace SafeVault.Service.Command.Models
{
    public class Userprofile
    {
        public string Email { get; set; }

        public string Username { get; set; }
        public string Path { get; set; }
        private ConfigFile _values;

        public string SecretKey { get; private set; }
        public string Password { get; private set; }

        public void LoadProfile()
        {
            _values = new ConfigFile($"{Path}/profile.conf");
            var password = GetValue("password", false);
            if (!password.StartsWith("md5:"))
            {
                password = Hash.MD5(password);
                _values.SetValue("password", $"md5:{password}");
                _values.Export($"{Path}/profile.conf");
            }

            SecretKey = GetValue("secret-key", false);
            Password = GetValue("password", false);
            Email = GetValue("email", false);
        }

        public string GetValue(string name, bool optional=false)
        {
            return _values.GetValue(name, () => 
            {
                if (!optional)
                    throw new InternalErrorException($"UserProfile['{name}'] is not defined");

                return null;
            });
        }
    }
}