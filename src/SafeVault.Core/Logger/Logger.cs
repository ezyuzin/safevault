using System;
using System.Collections.Generic;
using System.Linq;
using SafeVault.Configuration;
using SafeVault.Logger.Exceptions;

namespace SafeVault.Logger
{
    public class Logger : ILog
    {
        public ILogAppender[] Appenders { get; set; }
        protected LogManager LogManager { get; private set; }

        public string Name { get; set; }
        public LogLevel LogLevel { get; set; }

        public bool IsEnabled(LogLevel level)
        {
            return (LogLevel <= level 
                && LogManager.IsEnabled(level)
                && Appenders.Any(m => m.IsLevelEnabled(level)));
        }

        public bool DebugEnabled 
        {
            get { return IsEnabled(LogLevel.Debug);  }
        }

        public bool InfoEnabled 
        {
            get { return IsEnabled(LogLevel.Info);  }
        }

        public bool WarnEnabled 
        {
            get { return IsEnabled(LogLevel.Warn);  }
        }

        public bool ErrorEnabled 
        {
            get { return IsEnabled(LogLevel.Error);  }
        }

        public bool FatalEnabled 
        {
            get { return IsEnabled(LogLevel.Fatal);  }

        }

        public Logger(string name, LogManager logManager, ILogAppender[] appenders)
        {
            Appenders = appenders;
            LogManager = logManager;
            Name = name; 
        }

        public Logger(string name, LogManager logManager)
        {
            Appenders = new ILogAppender[0];
            LogManager = logManager;
            Name = name;
        }

        public void Configure(IConfigSection section, ILogAppender[] appenders)
        {
            var level = section.Get("level", false);
            if (level != null)
            {
                LogLevel = (LogLevel) Enum.Parse(typeof(LogLevel), level, true);
            }

            var appenderConf = section.GetSections("appender");
            if (appenderConf.Length != 0)
            {
                List<ILogAppender> used = new List<ILogAppender>();
                foreach (var appender in section.GetSections("appender"))
                {
                    string refName = appender.Get("@ref");
                    var app = appenders.FirstOrDefault(m => m.Name == refName);

                    if (app != null)
                        used.Add(app);
                }
                Appenders = used.ToArray();
            }
        }

        public virtual void Debug(string message, params object[] args)
        {
            if (DebugEnabled)
            {
                message = (args.Length != 0) ? string.Format(message, args) : message;
                LogRecordInternal(GetLogRecord(LogLevel.Debug, message));
            }
        }
        public virtual void Info(string message, params object[] args)
        {
            if (InfoEnabled)
            {
                message = (args.Length != 0) ? string.Format(message, args) : message;
                LogRecordInternal(GetLogRecord(LogLevel.Info, message));
            }
        }
        public virtual void Warn(string message, params object[] args)
        {
            if (WarnEnabled)
            {
                message = (args.Length != 0) ? string.Format(message, args) : message;
                LogRecordInternal(GetLogRecord(LogLevel.Warn, message));
            }
        }
        public virtual void Error(string message, params object[] args)
        {
            if (ErrorEnabled)
            {
                message = (args.Length != 0) ? string.Format(message, args) : message;
                LogRecordInternal(GetLogRecord(LogLevel.Error, message));
            }
        }
        public virtual void Fatal(string message, params object[] args)
        {
            if (FatalEnabled)
            {
                message = (args.Length != 0) ? string.Format(message, args) : message;
                LogRecordInternal(GetLogRecord(LogLevel.Fatal, message));
            }
        }

        public virtual void Debug(string message)
        {
            if (DebugEnabled)
            {
                LogRecordInternal(GetLogRecord(LogLevel.Debug, message));
            }
        }

        public virtual void Info(string message)
        {
            if (InfoEnabled)
            {
                LogRecordInternal(GetLogRecord(LogLevel.Info, message));
            }        
        }

        public virtual void Warn(string message)
        {
            if (WarnEnabled)
            {
                LogRecordInternal(GetLogRecord(LogLevel.Warn, message));
            }  
        }

        public virtual void Error(string message)
        {
            if (ErrorEnabled)
            {
                LogRecordInternal(GetLogRecord(LogLevel.Error, message));
            }
        }

        public virtual void Fatal(string message)
        {
            if (FatalEnabled)
            {
                LogRecordInternal(GetLogRecord(LogLevel.Fatal, message));
            }
        }

        public void LogRecord(LogRecord rec)
        {
            if (!IsEnabled(this, rec.Level))
                return;

            LogRecordInternal(rec);
        }

        protected virtual void LogRecordInternal(LogRecord rec)
        {
            foreach (var appender in Appenders.Where(a => a.IsLevelEnabled(rec.Level)))
            {
                appender.LogRecord(rec);
            }
        }

        private LogRecord GetLogRecord(LogLevel level, string message)
        {
            var rec = new LogRecord {Level = level, Message = message, Type = Name};
            return rec;
        }

        public bool IsEnabled(ILog logger, LogLevel level)
        {
            return (LogLevel <= level);
        }
    }
}