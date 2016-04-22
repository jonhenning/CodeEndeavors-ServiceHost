using CodeEndeavors.ServiceHost.Common;
using CodeEndeavors.ServiceHost.Common.Services;
using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace CodeEndeavors.ServiceHost.Middleware
{
    public class InstallerMiddleware
    {
        private Func<IDictionary<string, object>, Task> _next;
        public InstallerMiddleware(Func<IDictionary<string, object>, Task> next)
        {
            _next = next;
        }

        public async Task Invoke(IDictionary<string, object> env)
        {
            if (!Update.Initialized)
                Task.Factory.StartNew(() => Update.WatchForUpdates());
            await _next(env);
        }
    }
}