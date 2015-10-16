using CodeEndeavors.Extensions;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

namespace CodeEndeavors.ServiceHost.Provider
{
    public class SimpleAuthorizationServerProvider : OAuthAuthorizationServerProvider
    {
        public override async Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            //return base.ValidateClientAuthentication(context);
            //can get BasicAuth credentials or Form Post credentials
            //string id;
            //string secret;
            //if (context.TryGetBasicCredentials(out id, out secret))
            //{
            //    if (secret == "secret")
            //    {
            //        context.Validated();
            //    }
            //}
            context.Validated();
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            //validate
            if (context.UserName == ConfigurationManager.AppSettings.GetSetting("OAuth.User", "test") && context.Password == ConfigurationManager.AppSettings.GetSetting("OAuth.Password", "password"))
            {
                //create identity
                var id = new ClaimsIdentity(context.Options.AuthenticationType);
                id.AddClaim(new Claim("sub", context.UserName));
                //id.AddClaim(new Claim("role", "user"));   //todo: enable plugins for this
                var props = new AuthenticationProperties(new Dictionary<string, string>()
                {
                    {"as:client_id", context.ClientId }
                });
                var ticket = new AuthenticationTicket(id, props);
                context.Validated(ticket);
            }
            else 
                context.Rejected();

        }

        public override async Task GrantRefreshToken(OAuthGrantRefreshTokenContext context)
        {
            var originalClient = context.Ticket.Properties.Dictionary["as:client_id"];
            var currentClient = context.ClientId;

            // enforce client binding of refresh token
            if (originalClient != currentClient)
            {
                context.Rejected();
                return;
            }

            // chance to change authentication ticket for refresh token requests
            var newId = new ClaimsIdentity(context.Ticket.Identity);
            //newId.AddClaim(new Claim("newClaim", "refreshToken"));  //todo: enable plugins for this

            var newTicket = new AuthenticationTicket(newId, context.Ticket.Properties);
            context.Validated(newTicket);
        }
    }
}