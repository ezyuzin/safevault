using System;
using SafeVault.Logger;

namespace SafeVault.Logger
{
    public class Log
    {
        private static ILogManager _instance;
        public static ILogManager Instance
        {
            get
            {
                return _instance;
            }
            set
            {
                (_instance as IDisposable)?.Dispose();
                _instance = value;
            }
        }

        public static ILog GetLogger(Type type)
        {
            return Instance.GetLogger(type);
        }

        static Log()
        {
            Instance = new LogManager();
        }
    }
}