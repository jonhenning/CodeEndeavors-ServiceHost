using Owin;
using System.Web.Http;

namespace CodeEndeavors.ServiceHost.Plugins
{
    public class RoutesExtension : IServiceHostPlugin
    {
        public int? Priority
        {
            get { return 30; }
        }

        public void Configure(IAppBuilder app, HttpConfiguration config)
        {
            //routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            //routes.MapRoute(
            //    name: "Default",
            //    url: "{controller}/{action}/{id}",
            //    defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            //);

            config.Routes.MapHttpRoute(
                name: "Version",
                routeTemplate: "version",
                defaults: new { controller = "Version", action = "Get" });

            //routes.MapRoute(
            //    name: "Version",
            //    url: "version.mvc",
            //    defaults: new { controller = "Version", action = "Get" }
            //);

        }
    }
}