using CodeEndeavors.Extensions;
using CodeEndeavors.ServiceHost.Common.Services;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Http;
//using CodeEndeavors.ServiceHost.Common.Services.LoggingServices;

namespace CodeEndeavors.ServiceHost.App_Start
{
    public class LoggingConfig
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public static void Register(HttpConfiguration config)
        {
            //Log.Configure("ServiceHostServicesLogger");  //configure generic logger for all ServiceHost services

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