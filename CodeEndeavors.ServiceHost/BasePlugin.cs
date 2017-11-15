using CodeEndeavors.Extensions;
using CodeEndeavors.ServiceHost.Common.Services;
using System;
using System.Configuration;

namespace CodeEndeavors.ServiceHost
{
    public abstract class BasePlugin 
    {
        private System.Reflection.Assembly _callingAssembly;
        private Configuration _config;
        private string _satilliteName;

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


        protected BasePlugin()
        {
            this._callingAssembly = System.Reflection.Assembly.GetCallingAssembly();
            this._config = null;
        }

    }
}