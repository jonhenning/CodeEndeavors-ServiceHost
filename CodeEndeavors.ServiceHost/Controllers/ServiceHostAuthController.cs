using CodeEndeavors.Extensions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Reflection;
using System.Web;
using System.Web.Security;

namespace CodeEndeavors.ServiceHost.Controllers
{
    public class ServiceHostAuthController : BaseController
    {
        /// <summary>
        /// Allows service to be authenticated using Basic Auth 
        /// </summary>
        public void Authenticate()
        {
            System.Collections.Generic.Dictionary<string, string> reqDict = this.DeserializeCompressedRequest<System.Collections.Generic.Dictionary<string, string>>();
            string user = System.Configuration.ConfigurationSettings.AppSettings.GetSetting("User", "");
            string password = System.Configuration.ConfigurationSettings.AppSettings.GetSetting("Password", "");
            bool flag = !string.IsNullOrEmpty(user);
            if (flag)
            {
                bool flag2 = reqDict.ContainsKey("user") && reqDict["user"] == user && reqDict.ContainsKey("password") && reqDict["password"] == password;
                if (flag2)
                {
                    var ticket = new System.Web.Security.FormsAuthenticationTicket(reqDict["user"], true, int.MaxValue);
                    var cookie = new System.Web.HttpCookie(System.Web.Security.FormsAuthentication.FormsCookieName, System.Web.Security.FormsAuthentication.Encrypt(ticket));
                    cookie.Expires = ticket.Expiration;
                    System.Web.HttpContext.Current.Response.SetCookie(cookie);
                }
                else
                {
                    System.Web.HttpContext.Current.Response.StatusCode = 403;
                }
            }
        }
        public static string GetServerName()
        {
            string machineName = "UNKNOWN";
            bool flag = System.Web.HttpContext.Current != null;
            if (flag)
            {
                machineName = System.Web.HttpContext.Current.Server.MachineName;
            }
            return machineName;
        }

        public static string GetVersion(System.Reflection.Assembly assembly)
        {
            try
            {
                return assembly.GetName().Version.ToString();
            }
            catch (System.Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
