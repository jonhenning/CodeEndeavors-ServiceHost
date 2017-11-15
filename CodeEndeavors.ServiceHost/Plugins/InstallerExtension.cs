using CodeEndeavors.Extensions;
using Owin;
using System.Configuration;
using System.Web.Http;

namespace CodeEndeavors.ServiceHost.Plugins
{
    public class InstallerExtension : IServiceHostPlugin
    {
        public int? Priority
        {
            get { return 20; }
        }

        public void Configure(IAppBuilder app, HttpConfiguration config)
        {
            if (ConfigurationManager.AppSettings.GetSetting("Installer.Enabled", true))
                app.Use(typeof(Middleware.InstallerMiddleware));
        }
    }
}