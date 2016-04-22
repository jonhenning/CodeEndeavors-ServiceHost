using CodeEndeavors.ServiceHost.Common.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CodeEndeavors.ServiceHost.Middleware
{
    public class ExceptionMiddleware
    {
        private Func<IDictionary<string, object>, Task> _next;
        public ExceptionMiddleware(Func<IDictionary<string, object>, Task> next)
        {
            _next = next;
        }

        public async Task Invoke(IDictionary<string, object> env)
        {
            try
            {
                await _next(env);
            }
            catch (Exception ex)
            {
                Logging.Error(ex, "ExceptionMiddleware");
                throw ex;
            }
        }
    }
}