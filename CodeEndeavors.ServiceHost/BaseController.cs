using CodeEndeavors.ServiceHost.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
//using CodeEndeavors.Extensions;
using CodeEndeavors.ServiceHost.Extensions;
using System.Runtime.CompilerServices;
using CodeEndeavors.ServiceHost.Common.Services;

namespace CodeEndeavors.ServiceHost
{
    public class BaseController : Controller
    {
        // Methods
        public T DeserializeCompressedRequest<T>()
        {
            if (System.Web.HttpContext.Current != null)
            {
                ZipPayload zip = null;
                T oResult = HttpExtensions.DeserializeCompressedJSON<T>(System.Web.HttpContext.Current.Request, ref zip);
                if (HttpLogger.Logger.IsDebugEnabled)
                {
                    HttpLogger.Logger.Debug(zip.GetStatistics());
                }
                return oResult;
            }
            return default(T);
        }

        public T DeserializeRequest<T>()
        {
            if (System.Web.HttpContext.Current != null)
            {
                return HttpExtensions.DeserializeJSON<T>(System.Web.HttpContext.Current.Request);
            }
            return default(T);
        }

        public T ResolveService<T>()
        {
            return ServiceLocator.Resolve<T>();
        }

        public void WriteJSON(object Data)
        {
            this.WriteJSON(RuntimeHelpers.GetObjectValue(Data), false);
        }

        public void WriteJSON(object Data, bool Compressed)
        {
            if (System.Web.HttpContext.Current != null)
            {
                if (Compressed)
                {
                    ZipPayload zip = null;
                    HttpExtensions.WriteCompressedJSON(System.Web.HttpContext.Current.Response, RuntimeHelpers.GetObjectValue(Data), ref zip);
                    if (HttpLogger.Logger.IsDebugEnabled)
                    {
                        HttpLogger.Logger.Debug(zip.GetStatistics());
                    }
                }
                else
                {
                    HttpExtensions.WriteJSON(System.Web.HttpContext.Current.Response, RuntimeHelpers.GetObjectValue(Data));
                }
            }
        }
    }

}