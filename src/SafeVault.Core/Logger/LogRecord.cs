using System;

namespace SafeVault.Logger
{
    public class LogRecord
    {
        public string Type { get; set; }

        public DateTime Timestamp { get; set; }
        public LogLevel Level { get; set; }
        public string Message { get; set; }

        public LogRecord()
        {
            Timestamp = DateTime.Now;
        }
    }
}