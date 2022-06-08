using Owin;
using System;
using System.Collections.Generic;
using System.Configuration;
using CodeEndeavors.Extensions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using StackExchange.Profiling.Storage;

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
            if (this.GetConfigSetting("Profiler.Enabled", false))
            {
                app.Use(typeof(StackExchangeProfilerMiddleware));
            }
            if (this.GetConfigSetting("Profiler.EF6", false))
            {
                StackExchange.Profiling.EntityFramework6.MiniProfilerEF6.Initialize();
            }
            MultiStorageProvider multiStorage = new MultiStorageProvider(new HttpRuntimeCacheStorage(TimeSpan.FromMinutes(2))); //we are sending results down to the client, so we don't need to store anything here in cache...  
            StackExchange.Profiling.MiniProfiler.Settings.Storage = multiStorage;
        }
    }
}
