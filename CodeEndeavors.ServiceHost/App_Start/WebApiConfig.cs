using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
//using System.Web.Mvc;

namespace CodeEndeavors.ServiceHost
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
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
