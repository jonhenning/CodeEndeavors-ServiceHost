using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeEndeavors.ServiceHost.Common.Services
{
    public class Logging
    {
        public enum LoggingLevel
        {
            None = 0,
            Error = 1,
            Info = 2,
            Debug = 3,
            Trace = 4
        }

        public static LoggingLevel LogLevel { get; set; }

        public static bool IsDebugEnabled
        {
            get
            {
                return (int)Logging.LogLevel >= (int)Logging.LoggingLevel.Debug;
            }
        }

        public static event Action<LoggingLevel, string> OnLoggingMessage;

        public static void Info(string msg)
        {
            Info(msg, "");
        }
        public static void Info(string msg, params object[] args)
        {
            Log(LoggingLevel.Info, msg, args);
        }
        public static void Debug(string msg)
        {
            Debug(msg, "");
        }
        public static void Debug(string msg, params object[] args)
        {
            Log(LoggingLevel.Debug, msg, args);
        }

        public static void Log(LoggingLevel level, string msg)
        {
            Log(level, msg, "");
        }
        public static void Log(LoggingLevel level, string msg, params object[] args)
        {
            if ((int)level <= (int)LogLevel && OnLoggingMessage != null)
            {
                try
                {
                    OnLoggingMessage(level, (msg.IndexOf("{0}") > -1 ? string.Format(msg, args) : msg));
                }
                catch { }
            }
        }

        public static void Error(Exception ex)
        {
            Error("ERROR: {0}", ex.Message);
            if (ex.InnerException != null)
                Error(ex.InnerException);
        }
        public static void Error(Exception ex, string msg)
        {
            Error("ERROR: {0}:{1}", msg, ex.Message);
            if (ex.InnerException != null)
                Error(ex.InnerException, msg);
        }

        public static void Error(string msg)
        {
            Error("ERROR: " + msg, "");
        }
        public static void Error(string msg, params object[] args)
        {
            Log(LoggingLevel.Error, msg, args);
        }

    }
}
