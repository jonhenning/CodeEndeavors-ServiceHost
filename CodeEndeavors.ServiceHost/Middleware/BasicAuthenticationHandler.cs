using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Infrastructure;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CodeEndeavors.ServiceHost.Middleware
{
    public class BasicAuthenticationHandler : AuthenticationHandler<BasicAuthenticationOptions>
    {
        private readonly string _challenge;

        public BasicAuthenticationHandler(BasicAuthenticationOptions options)
        {
            _challenge = "Basic realm=" + options.Realm;
        }

        protected override async Task<AuthenticationTicket> AuthenticateCoreAsync()
        {
            var authValue = Request.Headers.Get("Authorization");
            if (string.IsNullOrEmpty(authValue) || !authValue.StartsWith("Basic "))
                return null;

            var token = authValue.Substring("Basic ".Length).Trim();
            var claims = await TryGetPrincipalFromBasicCredentials(token, Options.CredentialValidationFunction);
            if (claims == null)
                return null;
            else
            {
                var id = new ClaimsIdentity(claims, Options.AuthenticationType);
                return new AuthenticationTicket(id, new AuthenticationProperties());
            }

        }

        protected override Task ApplyResponseChallengeAsync()
        {
            if (Response.StatusCode == 401)
            {
                var challenge = Helper.LookupChallenge(Options.AuthenticationType, Options.AuthenticationMode);
                if (challenge != null)
                    Response.Headers.AppendValues("WWW-Authenticate", _challenge);
            }
            return Task.FromResult<object>(null);
        }

        async Task<IEnumerable<Claim>> TryGetPrincipalFromBasicCredentials(string credentials, BasicAuthenticationMiddleware.CredentialValidationFunction validate)
        {
            string pair;
            try
            {
                pair = Encoding.UTF8.GetString(Convert.FromBase64String(credentials));
            }
            catch (FormatException)
            {
                return null;
            }
            catch (ArgumentException)
            {
                return null;
            }

            var idx = pair.IndexOf(':');
            if (idx == -1)
                return null;
            var username = pair.Substring(0, idx);
            var pw = pair.Substring(idx + 1);
            return await validate(username, pw);
        }


    }
}