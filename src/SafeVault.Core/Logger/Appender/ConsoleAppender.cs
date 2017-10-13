using System;
using System.Text.RegularExpressions;
using SafeVault.Threading;

namespace SafeVault.Logger.Appender
{
    public class ConsoleAppender : BaseAppender, IDisposable
    {
        private QueueProcessor<LogRecord> _queue;
        public bool Immediately { get; set; }

        public ConsoleAppender()
        {
            _queue = new QueueProcessor<LogRecord>("ConsoleAppender", RenderMessage);
        }

        public void Dispose()
        {
            _queue?.Dispose();
            _queue = null;
        }

        public override void LogRecord(LogRecord rec)
        {
            if (IsLevelEnabled(rec.Level))
            {
                if (Immediately)
                    RenderMessage(rec);
                else
                    _queue?.Append(rec);
            }
        }

        private void RenderMessage(LogRecord rec)
        {
            if (rec.Level == LogLevel.Debug)
                RenderToConsole(rec, ConsoleColor.White);

            if (rec.Level == LogLevel.Info)
                RenderToConsole(rec, ConsoleColor.Green);

            if (rec.Level == LogLevel.Warn)
                RenderToConsole(rec, ConsoleColor.Yellow);

            if (rec.Level == LogLevel.Error)
                RenderToConsole(rec, ConsoleColor.Red);

            if (rec.Level == LogLevel.Fatal)
                RenderToConsole(rec, ConsoleColor.Magenta);
        }

        private void RenderToConsole(LogRecord rec, ConsoleColor foreground)
        {
            try
            {
                RenderToConsoleEx(rec, foreground);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void RenderToConsoleEx(LogRecord rec, ConsoleColor foreground)
        {
            //var msg = FormatMessage(rec);
            var timestamp = rec.Timestamp.ToString("HH:mm:ss");
            string level = rec.Level.ToString().ToUpper();
            level = level.PadRight(5, ' ');

            var type = Regex.Match(rec.Type, @"(\.)?(\w+)$").Groups[2].Value;

            var fc = Console.ForegroundColor;
            var bc = Console.BackgroundColor;

            int pad = 0;
            Console.ForegroundColor = ConsoleColor.Gray;
            var consoleOut = $"{timestamp}";
            Console.Write(consoleOut);
            pad += consoleOut.Length;

            Console.ForegroundColor = foreground;
            Console.BackgroundColor = bc;
            Console.Write("|");
            pad += 1;

            Console.ForegroundColor = foreground;
            if (rec.Level == LogLevel.Error)
            {
                Console.BackgroundColor = ConsoleColor.Yellow;
                Console.ForegroundColor = ConsoleColor.Black;
            }
            if (rec.Level == LogLevel.Fatal)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.White;
            }
            consoleOut = $" {level} ";
            Console.Write(consoleOut);
            pad += consoleOut.Length;

            Console.ForegroundColor = foreground;
            Console.BackgroundColor = bc;
            Console.Write("|");
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

                Console.Write(line);
                if (count == 0)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.BackgroundColor = bc;
                    Console.Write($" [{type}]");
                }
                else
                {
                    Console.WriteLine();
                    Console.Write("|".PadRight(pad));
                }
            }

            Console.ForegroundColor = fc;
            Console.BackgroundColor = bc;
            Console.WriteLine();
        }
    }
}