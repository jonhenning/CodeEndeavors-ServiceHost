using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CodeEndeavors.ServiceHost.Common.Services
{
    public class Handlers
    {
        public static string AquireUserId()
        {
            var userId = "";
            if (Thread.CurrentPrincipal != null && !string.IsNullOrEmpty(Thread.CurrentPrincipal.Identity.Name))
                userId = Thread.CurrentPrincipal.Identity.Name;
            return userId;
        }

        public static void ProcessBasicAuthentication(HttpClient request, string user, string password)
        {
            request.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.Default.GetBytes(user + ":" + password))); 
            //request.Headers["Authorization"] = "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes(user + ":" + password));
        }

    }
}
