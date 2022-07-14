using CodeEndeavors.Extensions;
using CodeEndeavors.ServiceHost.Common;
using CodeEndeavors.ServiceHost.Common.Services;
//using CodeEndeavors.ServiceHost.Common.Services.LoggingServices;
using System;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;

namespace CodeEndeavors.ServiceHost
{
    public abstract class BaseService
    {
        private System.Reflection.Assembly _callingAssembly;
        private Configuration _config;
        //private ServiceLogger _logger;
        private string _satilliteName;
        //protected string _serviceLogKey;
        protected Configuration Config
        {
            get
            {
                if (_config == null)
                {
                    try
                    {
                        Uri uri = new Uri(_callingAssembly.CodeBase);
                        _config = ConfigurationManager.OpenExeConfiguration(uri.LocalPath);
                    }
                    catch (Exception ex)
                    {
                        Logging.Error(ex, "OpenExeConfiguration");
                    }
                }
                return _config;
            }
        }

        protected T GetConfigSetting<T>(string key, T defaultValue)
        {
            if (Config.AppSettings.Settings[key] != null)
                return Config.AppSettings.Settings[key].Value.ToType<T>();
            return defaultValue;
        }

        //protected ServiceLogger Logger
        //{
        //    get
        //    {
        //        return this._logger;
        //    }
        //}
        protected BaseService()
        {
            this._callingAssembly = null;
            this._config = null;
            //this._logger = null;
            this._satilliteName = "";
        }
        public void Application_Error(object sender, System.EventArgs e)
        {
            System.Exception lastError = System.Web.HttpContext.Current.Server.GetLastError();
            if (lastError is System.Reflection.ReflectionTypeLoadException)
            {
                System.Reflection.ReflectionTypeLoadException ex = lastError as System.Reflection.ReflectionTypeLoadException;
                var sb = new System.Text.StringBuilder();
                foreach (var exception in ex.LoaderExceptions)
                    sb.AppendLine(exception.Message);
                sb.AppendLine(ex.Message);
                Logging.Error(sb.ToString());
            }

            Logging.Error(lastError.Message);
        }
        
        protected void Configure()
        {
            this._callingAssembly = System.Reflection.Assembly.GetCallingAssembly();
            //_serviceLogKey = serviceLogKey;
            //this._logger = new ServiceLogger(logConfigFileName, serviceLogKey);
            //this.Logger.Info(string.Format("{0} Service configured", base.GetType().ToString()));
            //bool flag = !string.IsNullOrEmpty(logConfigFileName);
            //if (flag)
            //{
            //    HttpLogger.HttpLogConfigFileName = logConfigFileName;
            //}
            //Startup.Application_Error += new Startup.ApplicationErrorHandler(this.Application_Error);
        }

        protected ServiceResult<T> ExecuteServiceResult<T>(Action<ServiceResult<T>> codeFunc) //where T : new()
        {
            return Helpers.ExecuteServiceResult<T>(codeFunc);
        }
        protected Task<ServiceResult<T>> ExecuteServiceResultAsync<T>(Func<ServiceResult<T>, Task> codeFunc) //where T : new()
        {
            return Helpers.ExecuteServiceResultAsync<T>(codeFunc);
        }

        protected string GetConnectionString(string key, string defaultValue)
        {
            if (string.IsNullOrEmpty(_satilliteName))
            {
                _satilliteName = _callingAssembly.ManifestModule.Name.Substring(0, _callingAssembly.ManifestModule.Name.LastIndexOf("."));
            }
            var resolvedKey = string.Format("{0}.Properties.Settings.{1}", _satilliteName, key);
            try
            {
                var val = Config.ConnectionStrings.ConnectionStrings[resolvedKey].ToString();
                if (_getConnectionOverride != null)
                {
                    val = _getConnectionOverride(key, val);
                }
                return val;
            }
            catch (Exception ex)
            {
                Logging.Error(ex, "GetConnectionString");
            }
            
            return defaultValue;
        }

        public static string AquireUserId()     //not sure this is best place to put static method
        {
            var userId = "-1";
            if (HttpContext.Current != null && HttpContext.Current.Items.Contains("ServiceHost.UserId"))
                userId = HttpContext.Current.Items["ServiceHost.UserId"].ToString();
            return userId;
        }

        //support for encrypted connection strings
        private static Func<string, string, string> _getConnectionOverride;
        public static Func<string, string, string> GetConnectionOverride
        {
            get
            {
                return _getConnectionOverride;
            }
            set
            {
                _getConnectionOverride = value;
            }
        }

    }
}
