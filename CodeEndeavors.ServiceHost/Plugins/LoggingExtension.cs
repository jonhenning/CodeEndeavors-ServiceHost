using CodeEndeavors.Extensions;
using CodeEndeavors.ServiceHost.Common.Services;
using NLog;
using Owin;
using System.Configuration;
using System.Web.Http;

namespace CodeEndeavors.ServiceHost.Plugins
{
    public class LoggingExtension : IServiceHostPlugin
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public int? Priority
        {
            get { return 100; }
        }

        public void Configure(IAppBuilder app, HttpConfiguration config)
        {
            Logging.LogLevel = ConfigurationManager.AppSettings.GetSetting("LogLevel", "Info").ToType<Logging.LoggingLevel>();

            Logging.OnLoggingMessage += (Logging.LoggingLevel level, string message) =>
            {
                _logger.Log(getLogLevel(level), message);
            };
        }


        private static LogLevel getLogLevel(Logging.LoggingLevel level)
        {
            if (level == Logging.LoggingLevel.Debug)
                return LogLevel.Debug;
            if (level == Logging.LoggingLevel.Info)
                return LogLevel.Info;
            if (level == Logging.LoggingLevel.Error)
                return LogLevel.Error;
            if (level == Logging.LoggingLevel.Trace)
                return LogLevel.Trace;

            return LogLevel.Off;
        }

    }
}