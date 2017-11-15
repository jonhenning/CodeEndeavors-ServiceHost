using CodeEndeavors.Extensions;
using CodeEndeavors.ServiceHost.Plugins;
using Microsoft.Owin;
using Owin;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

[assembly: OwinStartup(typeof(CodeEndeavors.ServiceHost.App_Start.Startup))]

namespace CodeEndeavors.ServiceHost.App_Start
{
    public class Startup
    {
        private static List<IServiceHostPlugin> _applicationPlugins = null;
        private static List<IServiceHostPlugin> applicationPlugins
        {
            get
            {
                if (_applicationPlugins == null)
                    _applicationPlugins = ReflectionExtensions.GetAllInstances<IServiceHostPlugin>();
                return _applicationPlugins;
            }
        }

        public void Configuration(IAppBuilder app)
        {
            var config = new HttpConfiguration();
            applicationPlugins.OrderBy(a => a.Priority ?? 0).ToList().ForEach(a => a.Configure(app, config));
            app.UseWebApi(config);  //must be last
        }

    }
}
