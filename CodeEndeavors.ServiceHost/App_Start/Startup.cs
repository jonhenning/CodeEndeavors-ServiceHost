using CodeEndeavors.Extensions;
using CodeEndeavors.ServiceHost.Middleware;
using Microsoft.Owin;
using Owin;
using System.Configuration;
using System.Web.Http;
//using System.Web.Mvc;
using Swashbuckle.Application;
using Microsoft.Owin.Security.OAuth;
using System;
using CodeEndeavors.ServiceHost.Provider;

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

            app.Use(typeof(Middleware.LoggingMiddleware));
            config.MessageHandlers.Add(new Middleware.UserContextHandler());

            SwaggerConfig.Register(config);
            WebApiConfig.Register(config);
            LoggingConfig.Register(config);

            if (ConfigurationManager.AppSettings.GetSetting("BasicAuth.Enabled", false))
                app.UseBasicAuthentication("ServiceHost");

            if (ConfigurationManager.AppSettings.GetSetting("OAuth.Enabled", false))
            {
                app.UseOAuthAuthorizationServer(new OAuthAuthorizationServerOptions()
                    {
                        AllowInsecureHttp = true,
                        TokenEndpointPath = new PathString("/token"),
                        AccessTokenExpireTimeSpan = TimeSpan.FromHours(8),
                        Provider = new SimpleAuthorizationServerProvider(),
                        RefreshTokenProvider = new SimpleRefreshTokenProvider()
                    });

                app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions());
            }

            app.UseWebApi(config);  //must be last

        }

    }
}
