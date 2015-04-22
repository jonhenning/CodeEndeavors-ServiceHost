using System;
using System.Reflection;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeEndeavors.Extensions;
using CodeEndeavors.ServiceHost.Extensions;

namespace CodeEndeavors.ServiceHost.Common.Services.LoggingServices
{
    public class Log4NetConfig
    {
        //If using internal logger, don't use reflection (very minor perf gain).  See LoggingServices.Test.LogTests for proof.
        //yes, code is a little messy!
        public enum LoggingType
        {
            Internal,
            Global
        }

        #region Member Variables
        private static readonly Dictionary<string, ILog> _loggers = new Dictionary<string, ILog>();
        private static readonly List<string> _configuredFiles = new List<string>();
        internal static Type _xmlConfiguratorType;
        internal static Type _logManagerType;
        internal static Type _logHierarchyType;
        internal static Type _logGlobalContext;
        internal static LoggingType _logType = LoggingType.Internal;
        private static bool _initialized = false;
        #endregion

        #region Constructors
        #endregion

        #region Properties
        internal static string DefaultLoggerKey { get; set; }

        internal static Log.LogLevel Level
        {
            get
            {
                if (Log.IsDebugEnabled)
                    return Log.LogLevel.Debug;
                if (Log.IsInfoEnabled)
                    return Log.LogLevel.Info;
                if (Log.IsWarnEnabled)
                    return Log.LogLevel.Warn;
                if (Log.IsErrorEnabled)
                    return Log.LogLevel.Error;
                return Log.LogLevel.Unknown;
                //log4net.Core.Level level = ((log4net.Repository.Hierarchy.Hierarchy)log4net.LogManager.GetRepository()).Root.Level;
                //return MapFromLog4NetLevel(level);
            }

            set
            {
                if (value != Log.LogLevel.Unknown)
                {
                    if (_logType == LoggingType.Internal)
                    {
                        var hierarchy = (log4net.Repository.Hierarchy.Hierarchy)log4net.LogManager.GetRepository();
                        hierarchy.Root.Level = hierarchy.LevelMap[value.ToString()];
                    }
                    else
                    {
                        var hierarchy = _logManagerType.InvokeStaticMethod("GetRepository");
                        var levelMap = hierarchy.InvokePropertyGet("LevelMap");
                        var level = levelMap.InvokePropertyGet("Item", value.ToString());
                        var root = hierarchy.InvokePropertyGet("Root");
                        root.InvokePropertySet("Level", level);
                    }
                }
            }
        }
        #endregion

        protected static void VerifyConfigured()
        {
            if (_loggers.Count == 0)
            {
                DefaultLoggerKey = "UnitTestLogger";
                _loggers["UnitTestLogger"] = new Log4NetLogger("UnitTestLogger");    //allow unit tests to work without configuration
                //throw new Exception("Logger Not Configured.  Please call Configure before invoking.");
            }
        }

        internal static ILog DefaultLogger
        {
            get
            {
                Log4NetConfig.VerifyConfigured();
                return _loggers[DefaultLoggerKey];
            }
        }

        private static void Initialize()
        {
            if (!_initialized)
            {
                _logManagerType = "log4net.LogManager".ToType(true, false);
                _xmlConfiguratorType = "log4net.Config.XmlConfigurator".ToType(true, false);
                _logHierarchyType = "log4net.Repository.Hierarchy.Hierarchy".ToType(true, false);
                _logGlobalContext = "log4net.GlobalContext".ToType(true, false);
                if (_xmlConfiguratorType == null)
                {
                    _logManagerType = typeof(log4net.LogManager);
                    _xmlConfiguratorType = typeof(log4net.Config.XmlConfigurator);
                    _logHierarchyType = typeof(log4net.Repository.Hierarchy.Hierarchy);
                    _logGlobalContext = typeof(log4net.GlobalContext);
                }
                else
                    _logType = LoggingType.Global;

            }
        }


        public static void Configure(string file, string loggerKey)
        {
            Initialize();

            file = string.IsNullOrEmpty(file) ? "" : Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, file);

            //if (_configuredFiles.Contains(file)) // if we've already configured this file, do nothing
            //    return;

            lock (_configuredFiles)
            {
                if (!_configuredFiles.Contains(file)) // double check
                {
                    if (file == "")
                    {
                        if (_logType == LoggingType.Internal)
                            log4net.Config.XmlConfigurator.Configure();
                        else
                            _xmlConfiguratorType.InvokeStaticMethod("Configure");
                    }
                    else
                    {
                        if (_logType == LoggingType.Internal)
                            log4net.Config.XmlConfigurator.ConfigureAndWatch(new FileInfo(file));
                        else
                            _xmlConfiguratorType.InvokeStaticMethod("ConfigureAndWatch", new FileInfo(file));
                        
                    }

                    _configuredFiles.Add(file);
                }
            }

            // Make it possible to log client IP address. This is a log4net 
            // "active property" (see http://logging.apache.org/log4net/release/manual/contexts.html)
            if (_logType == LoggingType.Internal)
                log4net.GlobalContext.Properties["IP"] = new IPAddressHelper();
            else
                _logGlobalContext.InvokeStaticPropertySet("Properties", "IP", new IPAddressHelper());

            if (string.IsNullOrEmpty(DefaultLoggerKey))
                DefaultLoggerKey = loggerKey;
            if (_loggers.ContainsKey(loggerKey) == false)
            {
                _loggers[loggerKey] = new Log4NetLogger(loggerKey);
                Log.Info(string.Format("Log4net loggerKey {0} configured{1} ({2})", loggerKey, string.IsNullOrEmpty(file) ? "" : ": " + file, _logType));
            }

        }

        public static ILog GetLogger(string loggerKey)
        {
            if (!string.IsNullOrEmpty(loggerKey) && _loggers.ContainsKey(loggerKey))
                return _loggers[loggerKey];
            else
                DefaultLogger.Error("Logger not configured: " + loggerKey);
            return DefaultLogger;
        }

    }

    // For IP property
    class IPAddressHelper
    {
        public override string ToString()
        {
            if (System.Web.HttpContext.Current != null)
            {
                // Request is accessed in a try/catch, because the HttpContext
                // may not be a request associated with it (e.g. in Application_Start
                // when running in Integrated pipeline mode). The framework throws
                // an exception if you access the Request property when it's not available.
                // See http://mvolo.com/blogs/serverside/archive/2007/11/10/Integrated-mode-Request-is-not-available-in-this-context-in-Application_5F00_Start.aspx
                try
                {
                    return System.Web.HttpContext.Current.Request.UserHostAddress;
                }
                catch
                {
                    //ignore
                }
            }
            return "Unknown";
        }
    }

}
