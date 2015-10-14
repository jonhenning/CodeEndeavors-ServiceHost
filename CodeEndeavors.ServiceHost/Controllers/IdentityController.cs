using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;

namespace CodeEndeavors.ServiceHost.Controllers
{
    public class IdentityController : ApiController
    {
        [Authorize]
        public IEnumerable<Models.ViewClaim> Get()
        {
            var principal = Request.GetRequestContext().Principal as ClaimsPrincipal;

            return from c in principal.Claims
                   select new Models.ViewClaim
                   {
                       Type = c.Type,
                       Value = c.Value
                   };
        }
    }
}
