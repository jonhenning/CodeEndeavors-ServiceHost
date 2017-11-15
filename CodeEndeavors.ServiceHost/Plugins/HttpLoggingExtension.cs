using Owin;
using System.Web.Http;

namespace CodeEndeavors.ServiceHost.Plugins
{
    public class HttpLoggingExtension : IServiceHostPlugin
    {
        public int? Priority
        {
            get { return 40; }
        }

        public void Configure(IAppBuilder app, HttpConfiguration config)
        {
            app.Use(typeof(Middleware.LoggingMiddleware));
        }
    }
}