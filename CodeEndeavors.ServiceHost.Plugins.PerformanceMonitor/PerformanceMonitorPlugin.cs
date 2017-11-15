using Owin;
using System;
using System.Collections.Generic;
using System.Configuration;
using CodeEndeavors.Extensions;
using CodeEndeavors.ServicePerformanceMonitor.Extensions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace CodeEndeavors.ServiceHost.Plugins.PerformanceMonitor
{
    public class PerformanceMonitorPlugin : BasePlugin, IServiceHostPlugin
    {
        public int? Priority
        {
            get { return 80; }
        }

        public void Configure(IAppBuilder app, HttpConfiguration config)
        {
            if (this.GetConfigSetting("PerformanceMonitor.Enabled", true))
            {
                config.EnableCodeEndeavorsServicePerformanceMonitor(c =>
                {
                    c.SlowestRequestCount(10);
                    c.Enable(this.GetConfigSetting("PerformanceMonitor.Active", false));
                }).EnableUI(this.GetConfigSetting("PerformanceMonitor.Url", "codeendeavors/perfmon"));  //note: if this changes, you must navigate to this setting + /index"
            }
        }
    }
}
