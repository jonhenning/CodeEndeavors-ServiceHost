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
using Microsoft.AspNet.WebApi.Extensions.Compression.Server;
using System.Net.Http.Extensions.Compression.Core.Compressors;

[assembly: OwinStartup(typeof(CodeEndeavors.ServiceHost.App_Start.Startup))]

namespace CodeEndeavors.ServiceHost.App_Start
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            //application.Error += new EventHandler(GlobalError);

            app.Use(typeof(Middleware.ExceptionMiddleware));
            app.Use(typeof(Middleware.InstallerMiddleware));

            //GlobalConfig = new HttpConfiguration();
            var config = new HttpConfiguration();

            RouteConfig.RegisterRoutes(config);

            app.Use(typeof(Middleware.LoggingMiddleware));
            config.MessageHandlers.Add(new Middleware.UserContextHandler());
            if (ConfigurationManager.AppSettings.GetSetting("Compression.Enabled", true))
            {
                config.MessageHandlers.Add(new ServerCompressionHandler(new GZipCompressor(), new DeflateCompressor()));
                //config.MessageHandlers.Add(new OwinServerCompressionHandler());
            }

            SwaggerConfig.Register(config);
            PerformanceMonitorConfig.Register(config);
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

            //Update.WatchForUpdates();

        }

    }
}
