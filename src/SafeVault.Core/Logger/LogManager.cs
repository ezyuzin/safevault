using System;
using System.Collections.Generic;
using System.Linq;
using SafeVault.Configuration;
using SafeVault.Logger.Appender;
using SafeVault.Logger.Exceptions;
using SafeVault.Misc;

namespace SafeVault.Logger
{
    public class LogManager : ILogManager, IDisposable
    {
        public ILogAppender[] Appenders { get; private set; }
        private IConfigSection _config;
        private Logger _root;

        public LogLevel LogLevel { get; set; }

        public LogManager()
        {
            LogLevel = LogLevel.Debug;
            Appenders = new[] {new ConsoleAppender { Immediately = true }};
            _root = new Logger("Root", this, Appenders);

        }

        public LogManager(IConfig config) : this()
        {
            Configure(config);
        }

        public bool IsEnabled(LogLevel level)
        {
            return (LogLevel <= level);
        }

        public void Configure(IConfig config)
        {
            var section = config.GetSection("log4net", throwIfNotFound:false);
            _config = section;
            if (_config == null)
                return;

            var appenders = ConfigureAppenders(section);

            lock (_createLoggerLock)
            {
                var bakAppenders = Appenders;
                Appenders = appenders;
                foreach (var logger in _instances.Values)
                    Configure(logger);

                foreach (var appender in bakAppenders)
                    (appender as IDisposable)?.Dispose();

                var rootConf = _config.GetSection("root");
                var rootLogger = new Logger("Root", this);
                rootLogger.Configure(rootConf, appenders);
                _root = rootLogger;

                LogLevel = rootLogger.LogLevel;
            }
        }

        private object _createLoggerLock = new object();
        private readonly Dictionary<string, Logger> _instances = new Dictionary<string, Logger>();
        public ILog GetLogger(string name)
        {
            if (_root == null)
                throw new LogConfigurationException("LogManager not configured");

            lock (_instances)
            {
                Logger instance;
                if (_instances.TryGetValue(name, out instance))
                    return instance;
            }

            Logger logger;
            lock (_createLoggerLock)
            {
                if (_instances.ContainsKey(name))
                    return _instances[name];

                logger = new Logger(name, this);
                logger.Name = name;
                Configure(logger);

                _instances.Add(name, logger);
                return logger;
            }
        }

        private void Configure(Logger logger)
        {
            logger.Appenders = _root.Appenders;
            logger.LogLevel = _root.LogLevel;

            var section = GetLoggerConfig(logger);
            if (section != null)
                logger.Configure(section, Appenders);
        }

        private IConfigSection GetLoggerConfig(Logger logger)
        {
            IConfigSection loggerSection = null;
            if (_config != null)
            {
                var loggerSections = _config.GetSections("loggers/logger")
                    .Select(m => new {Section = m, Name = m.Get("@name")})
                    .ToArray();

                loggerSection = loggerSections
                    .Where(m => Wildcard.Match(m.Name, logger.Name, ignoreCase: true))
                    .Select(m => m.Section).FirstOrDefault();
            }
            return loggerSection;
        }

        public ILog GetLogger(Type type)
        {
            return GetLogger(type.FullName);
        }

        public void Dispose()
        {
            if (Appenders != null)
            {
                foreach (var appender in Appenders)
                    (appender as IDisposable)?.Dispose();
            }
            Appenders = null;
        }

        private ILogAppender[] ConfigureAppenders(IConfigSection section)
        {
            var appenderTypes = GetType().Assembly.GetTypes()
                .Where(m => m.GetInterfaces().Any(m1 => m1 == typeof(ILogAppender)))
                .ToArray();

            List<ILogAppender> appenders = new List<ILogAppender>();
            try 
            {
                foreach (var conf in section.GetSections("appenders/appender"))
                {
                    var typeName = conf.Get("@type");
                    var type = appenderTypes.FirstOrDefault(m => m.FullName.Equals(typeName, StringComparison.InvariantCultureIgnoreCase));
                    type = type ?? appenderTypes.FirstOrDefault(m => m.Name.Equals(typeName, StringComparison.InvariantCultureIgnoreCase));
                    type = type ?? appenderTypes.FirstOrDefault(m => m.Name.Equals($"{typeName}Appender", StringComparison.InvariantCultureIgnoreCase));

                    if (type == null)
                        throw new LogConfigurationException($"Unsupported appender type: {typeName}");

                    var appender = (ILogAppender) Activator.CreateInstance(type);
                    appender.Configure(conf);
                    appenders.Add(appender);
                }
            }
            catch
            {
                foreach(var appender in appenders)
                    (appender as IDisposable)?.Dispose();

                throw;
            }
            return appenders.ToArray();
        }


    }
}