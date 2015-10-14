using CodeEndeavors.Extensions;
using Microsoft.Owin.Security;
using System.Collections.Generic;
using System.Configuration;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CodeEndeavors.ServiceHost.Middleware
{
    public class BasicAuthenticationOptions : AuthenticationOptions
    {
        public BasicAuthenticationMiddleware.CredentialValidationFunction CredentialValidationFunction { get; private set; } 

        public string Realm {get; private set;}


        public BasicAuthenticationOptions(string realm)
            : base("Basic")
        {
            Realm = realm;
            CredentialValidationFunction = DefaultValidationFunction;
        }

        public BasicAuthenticationOptions(string realm, BasicAuthenticationMiddleware.CredentialValidationFunction validationFunction)
            : base("Basic")
        {
            Realm = realm;
            CredentialValidationFunction = validationFunction;
        }

        public async Task<IEnumerable<Claim>> DefaultValidationFunction(string id, string secret)
        {
            if (id == ConfigurationManager.AppSettings.GetSetting("BasicAuth.User", "test") && secret == ConfigurationManager.AppSettings.GetSetting("BasicAuth.Password", "password"))
            {
                var claims = new List<Claim>()
                    {
                        new Claim(ClaimTypes.NameIdentifier, id)
                    };
                return claims;
            }
            return null;
        }


    }
}