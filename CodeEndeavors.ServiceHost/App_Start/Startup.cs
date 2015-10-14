using CodeEndeavors.Extensions;
using CodeEndeavors.ServiceHost.Middleware;
using Microsoft.Owin;
using Owin;
using System.Configuration;
using System.Web.Http;
using System.Web.Mvc;
using Swashbuckle.Application;

[assembly: OwinStartup(typeof(CodeEndeavors.ServiceHost.App_Start.Startup))]

namespace CodeEndeavors.ServiceHost.App_Start
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            //application.Error += new EventHandler(GlobalError);


            //GlobalConfig = new HttpConfiguration();
            var config = new HttpConfiguration();

            RouteConfig.RegisterRoutes(config);

            SwaggerConfig.Register(config);
            WebApiConfig.Register(config);
            LoggingConfig.Register(config);

            if (ConfigurationManager.AppSettings.GetSetting("BasicAuth.Enabled", false))
                app.UseBasicAuthentication("ServiceHost");

            app.UseWebApi(config);  //must be last

        }

    }
}
