﻿using Microsoft.ApplicationInsights.Extensibility;
using Owin;
using System.Web.Http;
using System.Web.Mvc;
using Microsoft.ApplicationInsights.Extensibility.Implementation.Tracing;

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
                config.EnableSystemDiagnosticsTracing();
                //GlobalFilters.Filters.Clear();
                GlobalFilters.Filters.Add(new AiHandleErrorAttribute());
            }
        }
    }
}