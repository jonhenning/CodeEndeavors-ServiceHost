using CodeEndeavors.Extensions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using CodeEndeavors.ServiceHost.Common.Services.LoggingServices;

namespace CodeEndeavors.ServiceHost
{
	public class GlobalHttpModule : System.Web.IHttpModule
	{
        private static bool _initialized = false;

        public delegate void ApplicationErrorHandler(Object sender, EventArgs e);
        public static event ApplicationErrorHandler Application_Error;
        public delegate void ApplicationAuthenticateRequestHandler(Object sender, EventArgs e);
        public static event ApplicationAuthenticateRequestHandler Application_AuthenticateRequest;

        public void Init(HttpApplication application)
        {
            application.Error += new EventHandler(GlobalError);

            if (_initialized == false)
            {
                _initialized = true;
                //log4net.LogManager.GetLogger("");   //fairly confident we need this to ensure global logger is used.
                Log.Configure("ServiceHostServicesLogger");  //configure generic logger for all ServiceHost services

                RegisterRoutes(RouteTable.Routes);
            }
        }
        public void Dispose() { }

        protected void GlobalError(Object sender, EventArgs e)
        {
            if (Application_Error != null)
                Application_Error(sender, e);
            Exception ex = HttpContext.Current.Server.GetLastError();
            Log.Error("Application_Error", ex);
            HttpContext.Current.ClearError();

            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.StatusCode = (int)System.Net.HttpStatusCode.InternalServerError;
            HttpContext.Current.Response.Write(ex.ToString());
            HttpContext.Current.Response.Flush();
            HttpContext.Current.Response.End();
        }

        protected void AuthenticateRequest(object sender, System.EventArgs e)
		{
            if (!String.IsNullOrEmpty(ConfigurationSettings.AppSettings.GetSetting("User", "")))
            {
                if (HttpContext.Current.Request.IsAuthenticated == false && IsAnonymousAllowed(HttpContext.Current.Request.RawUrl) == false)
                {
                    HttpContext.Current.Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                    //'Throw New Exception("Access Denied")
                }
            }
            
            if (Application_AuthenticateRequest != null)
                Application_AuthenticateRequest(sender, e);
		}

		private bool IsAnonymousAllowed(string Url)
		{
			Url = Url.ToLower();
			return Url.IndexOf("/servicehostauth") > -1 || Url.IndexOf("/version") > -1;
		}

        public System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, System.ResolveEventArgs args)
		{
			var name = new System.Reflection.AssemblyName(args.Name);

            if (name.Name != args.Name)
				return System.Reflection.Assembly.LoadWithPartialName(name.Name);
			
            return null;
		}
		public static void RegisterRoutes(System.Web.Routing.RouteCollection routes)
		{
			routes.MapRoute("Default", "{controller}.mvc/{action}/{userid}", new {controller = "Version", action = "Get", userid = ""});
            routes.MapRoute("DefaultId", "{controller}.mvc/{action}/{userid}/{id}", new { controller = "Version", action = "Get", userid = "" });
		}
	}
}
