using Common.Logging;
using Common.Logging.Configuration;
using Common.Logging.Simple;

namespace exRSConsole
{
    internal class LogAdapter : AbstractSimpleLoggerFactoryAdapter
    {
        public LogAdapter(NameValueCollection properties) : base(properties)
        {
        }

        protected override ILog CreateLogger(string name, LogLevel level, bool showLevel, bool showDateTime,
            bool showLogName, string dateTimeFormat) =>
            new Log(name, level, showLevel, showDateTime, showLogName, dateTimeFormat);
    }
}
