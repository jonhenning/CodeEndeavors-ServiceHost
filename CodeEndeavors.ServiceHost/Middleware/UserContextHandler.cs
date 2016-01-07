using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace CodeEndeavors.ServiceHost.Middleware
{
    public class UserContextHandler : DelegatingHandler
    {
        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request != null)
            {
                var config = request.GetConfiguration();
                if (config != null && config.Routes != null)
                {
                    var routeData = config.Routes.GetRouteData(request);
                    if (routeData != null)
                    {
                        if (routeData.Values.ContainsKey("userId")) //if route contains userId info, then set it on HttpContext Item
                            HttpContext.Current.Items["ServiceHost.UserId"] = routeData.Values["userId"];
                    }
                }
            }
            return await base.SendAsync(request, cancellationToken);
        }
    }
}