using CodeEndeavors.Extensions;
using Microsoft.AspNet.WebApi.Extensions.Compression.Server;
using Owin;
using System.Configuration;
using System.Net.Http.Extensions.Compression.Core.Compressors;
using System.Web.Http;

namespace CodeEndeavors.ServiceHost.Plugins
{
    public class CompressionExtension : IServiceHostPlugin
    {
        public int? Priority
        {
            get { return 60; }
        }

        public void Configure(IAppBuilder app, HttpConfiguration config)
        {
            if (ConfigurationManager.AppSettings.GetSetting("Compression.Enabled", true))
            {
                config.MessageHandlers.Add(new ServerCompressionHandler(new GZipCompressor(), new DeflateCompressor()));
                //config.MessageHandlers.Add(new OwinServerCompressionHandler());
            }
        }
    }
}