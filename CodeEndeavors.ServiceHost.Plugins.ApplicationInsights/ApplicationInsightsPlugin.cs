using Microsoft.ApplicationInsights.Extensibility;
using Owin;
using System.Web.Http;
using System.Web.Mvc;
using Microsoft.ApplicationInsights.Extensibility.Implementation.Tracing;
using Microsoft.ApplicationInsights;

namespace CodeEndeavors.ServiceHost.Plugins.ApplicationInsights
{
    public class ApplicationInsightsPlugin : BasePlugin, IServiceHostPlugin
    {
        public int? Priority
        {
            get { return 85; }
        }

        public void Configure(IAppBuilder app, HttpConfiguration config)
        {
            if (this.GetConfigSetting("ApplicationInsights.Enabled", true) && !string.IsNullOrEmpty(this.GetConfigSetting("ApplicationInsights.InstrumentationKey", "")))
            {
                TelemetryConfiguration.Active.InstrumentationKey = this.GetConfigSetting("ApplicationInsights.InstrumentationKey", "");
                ServiceHost.Common.Services.Logging.Info("ApplicationInsights Configured with key: " + TelemetryConfiguration.Active.InstrumentationKey);
                if (this.GetConfigSetting("ApplicationInsights.DiagnosticsTracing", false))
                    config.EnableSystemDiagnosticsTracing();
                GlobalFilters.Filters.Clear();
                GlobalFilters.Filters.Add(new AiHandleErrorAttribute());

                new TelemetryClient().TrackEvent("Application Insights Started from: " + System.Environment.MachineName);
            }
            else
                ServiceHost.Common.Services.Logging.Info("ApplicationInsights NOT Configured");
        }
    }
}
