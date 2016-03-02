using CodeEndeavors.ServiceHost.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
//using System.Web.Mvc;
//using CodeEndeavors.Extensions;
using CodeEndeavors.ServiceHost.Extensions;
using System.Runtime.CompilerServices;
using CodeEndeavors.ServiceHost.Common.Services;
using System.Web.Http;
using CodeEndeavors.Extensions;

namespace CodeEndeavors.ServiceHost
{
    public class BaseController : ApiController
    {
        // Methods

        protected T DeserializeRequest<T>()
        {
            if (System.Web.HttpContext.Current != null)
            {
                return CodeEndeavors.ServiceHost.Extensions.HttpExtensions.DeserializeJSON<T>(System.Web.HttpContext.Current.Request);
            }
            return default(T);
        }

        protected T ResolveService<T>()
        {
            return ServiceLocator.Resolve<T>();
        }

        protected void WriteJSON(object data)
        {
            if (System.Web.HttpContext.Current != null)
            {
                System.Web.HttpContext.Current.Response.StatusCode = (int)System.Net.HttpStatusCode.OK;
                CodeEndeavors.ServiceHost.Extensions.HttpExtensions.WriteJSON(System.Web.HttpContext.Current.Response, RuntimeHelpers.GetObjectValue(data));
            }
        }

    }

}