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
        protected T DeserializeCompressedRequest<T>()
        {
            if (System.Web.HttpContext.Current != null)
            {
                ZipPayload zip = null;
                T oResult = CodeEndeavors.ServiceHost.Extensions.HttpExtensions.DeserializeCompressedJSON<T>(System.Web.HttpContext.Current.Request, ref zip);
                if (HttpLogger.Logger.IsDebugEnabled)
                {
                    HttpLogger.Logger.Debug(zip.GetStatistics());
                }
                return oResult;
            }
            return default(T);
        }

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

        protected void WriteJSON(object Data)
        {
            this.WriteJSON(RuntimeHelpers.GetObjectValue(Data), false);
        }

        protected void WriteJSON(object data, bool compressed)
        {
            if (System.Web.HttpContext.Current != null)
            {
                System.Web.HttpContext.Current.Response.StatusCode = (int)System.Net.HttpStatusCode.OK;
                if (compressed)
                {
                    ZipPayload zip = null;

                    CodeEndeavors.ServiceHost.Extensions.HttpExtensions.WriteCompressedJSON(System.Web.HttpContext.Current.Response, RuntimeHelpers.GetObjectValue(data), ref zip);
                    if (HttpLogger.Logger.IsDebugEnabled)
                        HttpLogger.Logger.Debug(zip.GetStatistics());
                }
                else
                    CodeEndeavors.ServiceHost.Extensions.HttpExtensions.WriteJSON(System.Web.HttpContext.Current.Response, RuntimeHelpers.GetObjectValue(data));
            }
        }

        protected byte[] GetCompressed(object data)
        {
            var json = RuntimeHelpers.GetObjectValue(data).ToJson(false, null, true);

            var zip = ConversionExtensions.ToCompress(json);
            if (HttpLogger.Logger.IsDebugEnabled)
                HttpLogger.Logger.Debug(zip.GetStatistics());
            return zip.Bytes;
        }

    }

}