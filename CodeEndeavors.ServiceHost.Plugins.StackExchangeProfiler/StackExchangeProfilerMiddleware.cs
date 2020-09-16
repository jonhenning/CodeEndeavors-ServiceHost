using Owin;
using System;
using System.Collections.Generic;
using System.Configuration;
using CodeEndeavors.Extensions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using StackExchange.Profiling;

namespace CodeEndeavors.ServiceHost.Plugins.PerformanceMonitor
{
    public class StackExchangeProfilerMiddleware
    {
        private Func<IDictionary<string, object>, Task> _next;
        public StackExchangeProfilerMiddleware(Func<IDictionary<string, object>, Task> next)
        {
            _next = next;
        }

        public async Task Invoke(IDictionary<string, object> env)
        {
            MiniProfiler.Start();
            await _next(env);
            MiniProfiler.Stop();
        }
    }
}
