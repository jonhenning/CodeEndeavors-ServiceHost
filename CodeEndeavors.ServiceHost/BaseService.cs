﻿using CodeEndeavors.ServiceHost.Common;
using CodeEndeavors.ServiceHost.Common.Services;
using CodeEndeavors.ServiceHost.Common.Services.LoggingServices;
using System;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web;

namespace CodeEndeavors.ServiceHost
{
    public abstract class BaseService
    {
        private System.Reflection.Assembly _callingAssembly;
        private Configuration _config;
        private ServiceLogger _logger;
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
                        Logger.Error("OpenExeConfiguration", ex);
                    }
                }
                return _config;
            }
        }

        protected ServiceLogger Logger
        {
            get
            {
                return this._logger;
            }
        }
        protected BaseService()
        {
            this._callingAssembly = null;
            this._config = null;
            this._logger = null;
            this._satilliteName = "";
        }
        public void Application_Error(object sender, System.EventArgs e)
        {
            System.Exception lastError = System.Web.HttpContext.Current.Server.GetLastError();
            this.Logger.Error("Application_Error", lastError);
        }
        
        protected void Configure(string logConfigFileName, string serviceLogKey)
        {
            this._callingAssembly = System.Reflection.Assembly.GetCallingAssembly();
            this._logger = new ServiceLogger(logConfigFileName, serviceLogKey);
            this.Logger.Info(string.Format("{0} Service configured", base.GetType().ToString()));
            bool flag = !string.IsNullOrEmpty(logConfigFileName);
            if (flag)
            {
                HttpLogger.HttpLogConfigFileName = logConfigFileName;
            }
            GlobalHttpModule.Application_Error += new GlobalHttpModule.ApplicationErrorHandler(this.Application_Error);
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
                return Config.ConnectionStrings.ConnectionStrings[resolvedKey].ToString();
            }
            catch (Exception ex)
            {
                Log.Error("GetConnectionString", ex);
            }

            return defaultValue;
        }
    }
}
