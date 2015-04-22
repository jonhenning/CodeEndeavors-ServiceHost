using System;
using System.Collections.Generic;
using System.IO;

namespace CodeEndeavors.ServiceHost.Common.Services.LoggingServices
{

    public static class Log
    {
        public enum LogLevel
        {
            Unknown,
            Debug,
            Info,
            Warn,
            Error
        }

        //private static ILog _logger = null;
        public static string ConfigFileName;

        public static string DefaultLoggerKey
        {
            get
            {
                return Log4NetConfig.DefaultLoggerKey;
            }
            set
            {
                Log4NetConfig.DefaultLoggerKey = value;
            }
        }

        public static ILog DefaultLogger
        {
            get
            {
                return Log4NetConfig.DefaultLogger;
            }
        }

        public static bool IsDebugEnabled
        {
            get 
            {
                return DefaultLogger.IsDebugEnabled; 
            }
        }
        public static bool IsErrorEnabled
        {
            get 
            {
                return DefaultLogger.IsErrorEnabled; 
            }
        }
        public static bool IsWarnEnabled
        {
            get 
            {
                return DefaultLogger.IsWarnEnabled; 
            }
        }
        public static bool IsInfoEnabled
        {
            get 
            {
                return DefaultLogger.IsInfoEnabled; 
            }
        }

        public static void Configure(string loggerKey)
        {
            log4net.LogManager.GetLogger("");   //fairly confident we need this to ensure global logger is used.
            Configure(null, loggerKey);
        }

        public static void Configure(string configFile, string loggerKey)
        {
            ConfigFileName = configFile;
            Log4NetConfig.Configure(configFile, loggerKey);
        }

        public static LogLevel Level
        {
            get
            {
                return Log4NetConfig.Level;
            }

            set
            {
                Log4NetConfig.Level = value;
            }
        }

        public static ILog GetLogger(string loggerKey)
        {
            return Log4NetConfig.GetLogger(loggerKey);
        }

        public static void Error(object message)
        {
            Error(message, DefaultLoggerKey);
        }

        public static void Error(object message, string loggerKey)
        {
            GetLogger(loggerKey).Error(message);
        }

        public static void Error(object message, Exception exception)
        {
            Error(message, exception, DefaultLoggerKey);
        }
        public static void Error(object message, Exception exception, string loggerKey)
        {
            GetLogger(loggerKey).Error(message, exception);
        }

        public static void Warn(object message)
        {
            Warn(message, DefaultLoggerKey);
        }

        public static void Warn(object message, string loggerKey)
        {
            GetLogger(loggerKey).Warn(message);
        }

        public static void Warn(object message, Exception exception)
        {
            Warn(message, exception, DefaultLoggerKey);
        }

        public static void Warn(object message, Exception exception, string loggerKey)
        {
            GetLogger(loggerKey).Warn(message, exception);
        }

        public static void Info(object message)
        {
            Info(message, DefaultLoggerKey);
        }

        public static void Info(object message, string loggerKey)
        {
            GetLogger(loggerKey).Info(message);
        }

        public static void Debug(object message)
        {
            Debug(message, DefaultLoggerKey);
        }

        public static void Debug(object message, string loggerKey)
        {
            GetLogger(loggerKey).Debug(message);
        }

    }
}
