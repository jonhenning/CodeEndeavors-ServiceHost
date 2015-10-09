using System.Linq;
using System.IO;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace CodeEndeavors.ServiceHost.Areas.HelpPage
{
    public class HelpPageAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "HelpPage";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "HelpPage_Default",
                "Help/{action}/{apiId}",
                new { controller = "Help", action = "Index", apiId = UrlParameter.Optional });

            var files = new DirectoryInfo(HttpContext.Current.Server.MapPath("~/bin")).GetFiles("*.api.xml").Select(f => f.FullName).ToList();

            GlobalConfiguration.Configuration.SetDocumentationProvider(new MultiXmlDocumentationProvider(files));

            HelpPageConfig.Register(GlobalConfiguration.Configuration);
        }
    }
}