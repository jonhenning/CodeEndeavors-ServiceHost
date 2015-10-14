using Owin;

namespace CodeEndeavors.ServiceHost.Middleware
{
    public static class BasicAuthenticationMiddlewareExtensions
    {
        public static IAppBuilder UseBasicAuthentication(this IAppBuilder app, string realm)
        {
            return app.UseBasicAuthentication(new BasicAuthenticationOptions(realm));
        }

        public static IAppBuilder UseBasicAuthentication(this IAppBuilder app, string realm, BasicAuthenticationMiddleware.CredentialValidationFunction validationFunction)
        {
            var options = new BasicAuthenticationOptions(realm, validationFunction);
            return app.UseBasicAuthentication(options);
        }

        public static IAppBuilder UseBasicAuthentication(this IAppBuilder app, BasicAuthenticationOptions options )
        {
            return app.Use <BasicAuthenticationMiddleware>(options);
        }
    }
}