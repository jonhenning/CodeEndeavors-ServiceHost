using Owin;
using System.Web.Http;

namespace CodeEndeavors.ServiceHost.Plugins
{
    public class ExceptionExtension : IServiceHostPlugin
    {
        public int? Priority
        {
            get { return 10; }
        }

        public void Configure(IAppBuilder app, HttpConfiguration config)
        {
            app.Use(typeof(Middleware.ExceptionMiddleware));
        }
    }
}