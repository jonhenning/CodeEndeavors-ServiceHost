using CodeEndeavors.ServiceHost.Common;
using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace CodeEndeavors.ServiceHost.Middleware
{
    public class LoggingMiddleware
    {
        private Func<IDictionary<string, object>, Task> _next;
        public LoggingMiddleware(Func<IDictionary<string, object>, Task> next)
        {
            _next = next;
        }

        public async Task Invoke(IDictionary<string, object> env)
        {
            var context = new OwinContext(env);

            if (HttpLogger.Logger.IsDebugEnabled)
                HttpLogger.Logger.Debug(context.Request.Uri.AbsoluteUri);

            await _next(env);
        }

    }
}