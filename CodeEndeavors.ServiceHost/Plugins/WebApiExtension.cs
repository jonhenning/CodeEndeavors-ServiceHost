using Owin;
using System.Web.Http;

namespace CodeEndeavors.ServiceHost.Plugins
{
    public class WebApiExtension : IServiceHostPlugin
    {
        public int? Priority
        {
            get { return 90; }
        }

        public void Configure(IAppBuilder app, HttpConfiguration config)
        {
            //config.Routes.MapHttpRoute(
            //    name: "DefaultApi",
            //    routeTemplate: "api/{controller}/{id}",
            //    defaults: new { id = RouteParameter.Optional }
            //);

            var formatters = config.Formatters;
            formatters.Remove(formatters.XmlFormatter);
            formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            formatters.JsonFormatter.SerializerSettings.ContractResolver = new CodeEndeavors.Extensions.Serialization.SerializeIgnoreContractResolver(null);

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "{controller}/{action}/{userId}/{id}",
                defaults: new { controller = "Home", action = "Index", userId = RouteParameter.Optional, id = RouteParameter.Optional }
            );
        }
    }
}