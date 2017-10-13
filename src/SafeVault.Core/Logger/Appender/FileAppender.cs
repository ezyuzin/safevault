using System;
using System.IO;
using SafeVault.Configuration;
using SafeVault.Threading;

namespace SafeVault.Logger.Appender
{
    public class FileAppender : BaseAppender, IDisposable
    {
        private QueueProcessor<LogRecord> _queue;
        private volatile StreamWriter _fs1;
        private object _streamLock = new object();
        
        public string FileName { get; set; }

        public FileAppender()
        {
            _queue = new QueueProcessor<LogRecord>("FileAppender", RenderMessage);
        }

        public void Dispose()
        {
            _queue?.Dispose();
            _queue = null;

            _fs1?.Dispose();
            _fs1 = null;
        }

        public override void Configure(IConfigSection section)
        {
            base.Configure(section);
            FileName = section.Get("filename");
            lock (_streamLock)
            {
                _fs1?.Dispose();
                _fs1 = null;
            }
        }

        private StreamWriter GetStreamWriter()
        {
            if (_fs1 != null)
                return _fs1;
            
            lock (_streamLock)
            {
                if (_fs1 != null)
                    return _fs1;
                
                StreamWriter fs = null;
                try
                {
                    var folder = Path.GetDirectoryName(FileName);
                    if (!Directory.Exists(folder))
                        Directory.CreateDirectory(folder);

                    fs = new StreamWriter(FileName, true);
                    fs.AutoFlush = true;
                }
                catch
                {
                    fs?.Dispose();
                    throw;
                }
                _fs1 = fs;
                return fs;
            }
        }

        public override void LogRecord(LogRecord rec)
        {
            if (IsLevelEnabled(rec.Level))
                _queue?.Append(rec);
        }

        protected virtual string FormatMessage(LogRecord rec)
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var level = rec.Level.ToString().ToUpper().PadRight(5);
            return $"{timestamp}|{level}|{rec.Message} [{rec.Type}]";
        }

        private void RenderMessage(LogRecord rec)
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var level = rec.Level.ToString().ToUpper().PadRight(5);
            var msg = $"{timestamp}|{level}|";

            var fs = GetStreamWriter();
            fs.Write(msg);

            var body = (rec.Message ?? "").TrimEnd().Replace("\r\n", "\n").TrimStart('\n');
            int offset = 0;
            int count = body.Length;
            while (count > 0)
            {
                var nbytes = (count < 120) ? count : 120;
                var line = body.Substring(offset, nbytes);
                line = line.Split('\n')[0];
                if (line.Length != nbytes)
                    nbytes = line.Length + 1;

                count -= nbytes;
                offset += nbytes;

                line = line.TrimEnd();

                fs.Write(line);
                if (count == 0)
                {
                    fs.Write($" [{rec.Type}]");
                }
                else
                {
                    fs.WriteLine();
                    fs.Write("|".PadLeft(msg.Length));
                }
            }
            fs.WriteLine();
        }
    }
}