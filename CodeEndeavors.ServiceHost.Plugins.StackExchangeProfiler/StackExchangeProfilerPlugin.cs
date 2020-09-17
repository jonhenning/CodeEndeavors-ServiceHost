using Owin;
using System;
using System.Collections.Generic;
using System.Configuration;
using CodeEndeavors.Extensions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace CodeEndeavors.ServiceHost.Plugins.PerformanceMonitor
{
    public class StackExchangeProfilerPlugin : BasePlugin, IServiceHostPlugin
    {
        public int? Priority
        {
            get { return 10; }
        }

        public void Configure(IAppBuilder app, HttpConfiguration config)
        {
            if (this.GetConfigSetting("Profiler.Enabled", true))
            {
                app.Use(typeof(StackExchangeProfilerMiddleware));
            }
            if (this.GetConfigSetting("Profiler.EF6", true))
            {
                StackExchange.Profiling.EntityFramework6.MiniProfilerEF6.Initialize();
            }
        }
    }
}
