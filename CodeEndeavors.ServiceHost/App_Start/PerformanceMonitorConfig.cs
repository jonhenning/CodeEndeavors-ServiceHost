using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using CodeEndeavors.ServicePerformanceMonitor.Extensions;
using System.Configuration;
using CodeEndeavors.Extensions;

namespace CodeEndeavors.ServiceHost.App_Start
{
    public class PerformanceMonitorConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.EnableCodeEndeavorsServicePerformanceMonitor(c =>
                {
                    c.SlowestRequestCount(10);
                    c.Enable(ConfigurationManager.AppSettings.GetSetting("PerformanceMonitor.Enabled", false));
                }).EnableUI();

        }


    }
}