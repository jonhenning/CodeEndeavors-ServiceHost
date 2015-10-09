using System;
using System.Diagnostics;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
namespace CodeEndeavors.ServiceHost
{
    public class VersionController : Controller
    {
        [System.Diagnostics.DebuggerNonUserCode]
        public VersionController()
        {
        }
        public ActionResult Get()
        {
            System.Web.HttpContext.Current.Response.Write("Welcome to the Code Endeavors Service ServiceHost!<br/>");
            System.Web.HttpContext.Current.Response.Write(VersionController.GetServerName());
            System.Web.HttpContext.Current.Response.Write("<table border='1'>");
            System.Reflection.Assembly[] assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
            checked
            {
                for (int i = 0; i < assemblies.Length; i++)
                {
                    System.Reflection.Assembly assembly = assemblies[i];
                    System.Web.HttpContext.Current.Response.Write(string.Format("<tr><td>{0}</td><td>{1}</td></tr>", assembly.GetName().Name, VersionController.GetVersion(assembly)));
                }
                System.Web.HttpContext.Current.Response.Write("</table>");
                return null;
            }
        }
        public static string GetServerName()
        {
            string machineName = "UNKNOWN";
            if (System.Web.HttpContext.Current != null)
                machineName = System.Web.HttpContext.Current.Server.MachineName;
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
