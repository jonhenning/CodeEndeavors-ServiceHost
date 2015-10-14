using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
//using System.Web.Mvc;
using System.Web.Routing;

namespace CodeEndeavors.ServiceHost
{
    public class RouteConfig
    {
        //public static void RegisterRoutes(RouteCollection routes)
        public static void RegisterRoutes(HttpConfiguration config)
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