using CodeEndeavors.ServiceHost.Common.Services.LoggingServices;
using System;
using System.Runtime.CompilerServices;
namespace CodeEndeavors
{
    public class ServiceLogger : ILog
    {
        private ILog _logger;
        public bool IsDebugEnabled
        {
            get
            {
                return this._logger.IsDebugEnabled;
            }
        }
        public bool IsErrorEnabled
        {
            get
            {
                return this._logger.IsErrorEnabled;
            }
        }
        public bool IsInfoEnabled
        {
            get
            {
                return this._logger.IsInfoEnabled;
            }
        }
        public bool IsWarnEnabled
        {
            get
            {
                return this._logger.IsWarnEnabled;
            }
        }
        public ServiceLogger(string logConfigFileName, string key)
        {
            bool flag = !string.IsNullOrEmpty(logConfigFileName);
            if (flag)
            {
                Log.Configure(logConfigFileName, key);
            }
            this._logger = Log.GetLogger(key);
        }
        public void Debug(object message)
        {
            this._logger.Debug(System.Runtime.CompilerServices.RuntimeHelpers.GetObjectValue(message));
        }
        public void Debug(object message, System.Exception exception)
        {
            this._logger.Debug(System.Runtime.CompilerServices.RuntimeHelpers.GetObjectValue(message), exception);
        }
        public void DebugFormat(string format, params object[] args)
        {
            this._logger.DebugFormat(format, args);
        }
        public void Error(object message)
        {
            this._logger.Error(System.Runtime.CompilerServices.RuntimeHelpers.GetObjectValue(message));
        }
        public void Error(object message, System.Exception exception)
        {
            this._logger.Error(System.Runtime.CompilerServices.RuntimeHelpers.GetObjectValue(message), exception);
        }
        public void ErrorFormat(string format, params object[] args)
        {
            this._logger.ErrorFormat(format, args);
        }
        public void Info(object message)
        {
            this._logger.Info(System.Runtime.CompilerServices.RuntimeHelpers.GetObjectValue(message));
        }
        public void Info(object message, System.Exception exception)
        {
            this._logger.Info(System.Runtime.CompilerServices.RuntimeHelpers.GetObjectValue(message), exception);
        }
        public void InfoFormat(string format, params object[] args)
        {
            this._logger.InfoFormat(format, args);
        }
        public void Warn(object message)
        {
            this._logger.Warn(System.Runtime.CompilerServices.RuntimeHelpers.GetObjectValue(message));
        }
        public void Warn(object message, System.Exception exception)
        {
            this._logger.Warn(System.Runtime.CompilerServices.RuntimeHelpers.GetObjectValue(message), exception);
        }
        public void WarnFormat(string format, params object[] args)
        {
            this._logger.WarnFormat(format, args);
        }
    }
}
