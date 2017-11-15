using CodeEndeavors.Extensions;
using CodeEndeavors.ServiceHost.Middleware;
using CodeEndeavors.ServiceHost.Provider;
using Microsoft.Owin;
using Microsoft.Owin.Security.OAuth;
using Owin;
using System;
using System.Configuration;
using System.Web.Http;

namespace CodeEndeavors.ServiceHost.Plugins
{
    public class AuthenticationExtension : IServiceHostPlugin
    {
        public int? Priority
        {
            get { return 110; }
        }

        public void Configure(IAppBuilder app, HttpConfiguration config)
        {
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

        }
    }
}