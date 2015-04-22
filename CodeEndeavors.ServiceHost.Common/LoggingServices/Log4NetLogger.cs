using System;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeEndeavors.Extensions;
using CodeEndeavors.ServiceHost.Extensions;

namespace CodeEndeavors.ServiceHost.Common.Services.LoggingServices
{

    //If using internal logger, don't use reflection (very minor perf gain).  See LoggingServices.Test.LogTests for proof.
    //yes, code is a little messy!
    public class Log4NetLogger : ILog
    {
        private object _logger;
        private log4net.ILog _internalLogger = null;

        private delegate bool IsEnabledDelegate();
        private delegate void MessageDelegate(object message);
        private delegate void MessageExceptionDelegate(object message, Exception exception);
        private delegate void FormatArgsDelegate(string format, params object[] args);

        private Dictionary<string, IsEnabledDelegate> _enabledDelegates = new Dictionary<string, IsEnabledDelegate>();
        private Dictionary<string, MessageDelegate> _messageDelegates = new Dictionary<string, MessageDelegate>();
        private Dictionary<string, MessageExceptionDelegate> _messageExDelegates = new Dictionary<string, MessageExceptionDelegate>();
        private Dictionary<string, FormatArgsDelegate> _formatDelegates = new Dictionary<string, FormatArgsDelegate>();
        public Log4NetLogger(string key)
        {
            if (Log4NetConfig._logType == Log4NetConfig.LoggingType.Internal)
                _internalLogger = log4net.LogManager.GetLogger(key);
            else
            {
                _logger = Log4NetConfig._logManagerType.InvokeStaticMethod("GetLogger", key);

                var methods = new List<string>() { "Info", "Warn", "Error", "Debug" };
                foreach (var method in methods)
                {
                    AddMessageDelegate(method);
                    AddMessageExDelegate(method);
                    AddFormatDelegate(method);
                    AddIsEnabledDelegate(method);
                }
            }

            if (_internalLogger != null || _logger != null)
                InfoFormat("Configured using {0} log4net logger", Log4NetConfig._logType);
        }

        private void AddMessageDelegate(string method)
        {
            _messageDelegates[method] = (MessageDelegate)Delegate.CreateDelegate(typeof(MessageDelegate), _logger, _logger.GetType().GetMethod(method, new Type[] { typeof(object) }));
        }

        private void AddMessageExDelegate(string method)
        {
            _messageExDelegates[method] = (MessageExceptionDelegate)Delegate.CreateDelegate(typeof(MessageExceptionDelegate), _logger, _logger.GetType().GetMethod(method, new Type[] { typeof(object), typeof(Exception) }));
        }

        private void AddFormatDelegate(string method)
        {
            _formatDelegates[method] = (FormatArgsDelegate)Delegate.CreateDelegate(typeof(FormatArgsDelegate), _logger, _logger.GetType().GetMethod(method + "Format", new Type[] { typeof(string), typeof(object[]) }));
        }

        private void AddIsEnabledDelegate(string method)
        {
            _enabledDelegates[method] = (IsEnabledDelegate)Delegate.CreateDelegate(typeof(IsEnabledDelegate), _logger, _logger.GetType().GetProperty("Is" + method + "Enabled").GetGetMethod());
        }

        #region ILog Members

        public bool IsDebugEnabled
        {
            get { return _internalLogger != null ? _internalLogger.IsDebugEnabled : _enabledDelegates["Debug"](); }
        }

        public bool IsErrorEnabled
        {
            get { return _internalLogger != null ? _internalLogger.IsErrorEnabled : _enabledDelegates["Error"](); }
        }

        public bool IsInfoEnabled
        {
            get { return _internalLogger != null ? _internalLogger.IsInfoEnabled : _enabledDelegates["Info"](); }
        }

        public bool IsWarnEnabled
        {
            get { return _internalLogger != null ? _internalLogger.IsWarnEnabled : _enabledDelegates["Warn"](); }
        }

        public void Info(object message)
        {
            if (_internalLogger != null)
                _internalLogger.Info(message);
            else
                _messageDelegates["Info"](message);
        }

        public void Info(object message, Exception exception)
        {
            if (_internalLogger != null)
                _internalLogger.Info(message, exception);
            else
                _messageExDelegates["Info"](message, exception);
        }

        public void InfoFormat(string format, params object[] args)
        {
            if (_internalLogger != null)
                _internalLogger.InfoFormat(format, args);
            else
                _formatDelegates["Info"](format, args);
        }

        public void Warn(object message)
        {
            if (_internalLogger != null)
                _internalLogger.Warn(message);
            else
                _messageDelegates["Warn"](message);
        }

        public void Warn(object message, Exception exception)
        {
            if (_internalLogger != null)
                _internalLogger.Warn(message, exception);
            else
                _messageExDelegates["Warn"](message, exception);
        }

        public void WarnFormat(string format, params object[] args)
        {
            if (_internalLogger != null)
                _internalLogger.WarnFormat(format, args);
            else
                _formatDelegates["Warn"](format, args);
        }

        public void Error(object message)
        {
            if (_internalLogger != null)
                _internalLogger.Error(message);
            else
                _messageDelegates["Error"](message);
        }

        public void Error(object message, Exception exception)
        {
            if (_internalLogger != null)
                _internalLogger.Error(message, exception);
            else
                _messageExDelegates["Error"](message, exception);
        }

        public void ErrorFormat(string format, params object[] args)
        {
            if (_internalLogger != null)
                _internalLogger.ErrorFormat(format, args);
            else
                _formatDelegates["Error"](format, args);
        }

        public void Debug(object message)
        {
            if (_internalLogger != null)
                _internalLogger.Debug(message);
            else
                _messageDelegates["Debug"](message);
        }

        public void Debug(object message, Exception exception)
        {
            if (_internalLogger != null)
                _internalLogger.Debug(message, exception);
            else
                _messageExDelegates["Debug"](message, exception);
        }

        public void DebugFormat(string format, params object[] args)
        {
            if (_internalLogger != null)
                _internalLogger.DebugFormat(format, args);
            else
                _formatDelegates["Debug"](format, args);
        }

        #endregion
    }
}
