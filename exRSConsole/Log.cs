using Common.Logging;
using Common.Logging.Simple;
using System;
using System.Text;

namespace exRSConsole
{
    internal class Log : AbstractSimpleLogger
    {
        public Log(
            string logName,
            LogLevel logLevel,
            bool showlevel,
            bool showDateTime,
            bool showLogName,
            string dateTimeFormat)
            : base(logName,
                  logLevel,
                  showlevel,
                  showDateTime,
                  showLogName,
                  dateTimeFormat)
        {
        }

        private void Warn(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"{DateTime.Now}:{message}");
                Console.ResetColor();
            }
        }

        private void WaitTime(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                Console.WriteLine(message);
                Console.SetCursorPosition(0, Console.CursorTop > 0 ? Console.CursorTop - 1 : 0);
            }
        }

        private void Trace(string message)
        {
            if (message.Equals("EmptyLine"))
            {
                Console.WriteLine();
                return;
            }
            if (message.StartsWith("Wait "))
            {
                WaitTime(message);
                return;
            }
            if (!string.IsNullOrWhiteSpace(message))
            {
                Console.WriteLine($"{DateTime.Now}:{message}");
            }
        }

        private void Info(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"{DateTime.Now}:{message}");
                Console.ResetColor();
            }
        }

        private void Question(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"{message}");
                Console.ResetColor();
            }
        }

        private void Error(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{DateTime.Now}:{message}");
                Console.ResetColor();
            }
        }

        protected override void WriteInternal(LogLevel level, object message, Exception exception)
        {
            var sb = new StringBuilder();
            if (message != null)
            {
                sb.AppendLine(message.ToString());
            }
            if (exception != null)
            {
                sb.AppendLine(exception.Message.ToString());
            }
            var output = sb.ToString().Trim();

            switch (level)
            {
                case LogLevel.Debug:
                    Question(output);
                    break;
                case LogLevel.Error:
                    Error(output);
                    break;
                case LogLevel.Warn:
                    Warn(output);
                    break;
                case LogLevel.Info:
                    Info(output);
                    break;
                case LogLevel.Trace:
                    Trace(output);
                    break;
            }
        }
    }
}
