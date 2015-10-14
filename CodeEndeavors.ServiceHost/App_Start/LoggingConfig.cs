using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using CodeEndeavors.ServiceHost.Common.Services.LoggingServices;

namespace CodeEndeavors.ServiceHost.App_Start
{
    public class LoggingConfig
    {
        public static void Register(HttpConfiguration config)
        {
            Log.Configure("ServiceHostServicesLogger");  //configure generic logger for all ServiceHost services
        }
    }
}