using Owin;
using System.Web.Http;

namespace CodeEndeavors.ServiceHost.Plugins
{
    public class UserContextExtension : IServiceHostPlugin
    {
        public int? Priority
        {
            get { return 50; }
        }

        public void Configure(IAppBuilder app, HttpConfiguration config)
        {
            config.MessageHandlers.Add(new Middleware.UserContextHandler());
        }
    }
}