using System;
using System.Diagnostics;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Http;
//using System.Web.Mvc;
namespace CodeEndeavors.ServiceHost
{
    public class VersionController : ApiController
    {
        public VersionController()
        {
        }
        public HttpResponseMessage Get()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Welcome to the Code Endeavors Service ServiceHost!<br/>");
            sb.AppendLine(getServerName());
            sb.AppendLine("<table border='1'>");
            var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
                sb.AppendLine(string.Format("<tr><td>{0}</td><td>{1}</td></tr>", assembly.GetName().Name, getVersion(assembly)));
            sb.AppendLine("</table>");

            return new HttpResponseMessage
            {
                Content = new StringContent(sb.ToString(), Encoding.UTF8, "text/html")
            };
        }

        private static string getServerName()
        {
            var machineName = "UNKNOWN";
            if (System.Web.HttpContext.Current != null)
                machineName = System.Web.HttpContext.Current.Server.MachineName;
            return machineName;
        }

        private static string getVersion(System.Reflection.Assembly assembly)
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
