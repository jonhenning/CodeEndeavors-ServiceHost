using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.IO;
using System.Net;
using CodeEndeavors.ServiceHost.Common;
using System.Runtime.CompilerServices;


namespace CodeEndeavors.ServiceHost.Extensions
{
    public static class HttpExtensions
    {
        public static T DeserializeJSON<T>(HttpRequest request)
        {
            return HttpExtensions.GetText(request).ToObject<T>();
        }

        public static string GetText(HttpResponse response)
        {
            using (var reader = new StreamReader(response.OutputStream))
                return reader.ReadToEnd();
            //return response.Content.ReadAsStringAsync().Result;
        }

        public static string GetText(HttpRequest request)
        {
            StreamReader reader = new StreamReader(request.InputStream);
            string GetText;
            try
            {
                GetText = reader.ReadToEnd();
            }
            finally
            {
                bool flag = reader != null;
                if (flag)
                {
                    ((IDisposable)reader).Dispose();
                }
            }
            return GetText;
        }
        public static void WriteJSON(HttpResponse response, object data)
        {
            var body = RuntimeHelpers.GetObjectValue(data).ToJson(false, null, true);
            response.Write(body);
        }

        public static void WriteText(WebRequest request, byte[] body)
        {
            request.ContentLength = (long)body.Length;
            Stream stream = request.GetRequestStream();
            try
            {
                stream.Write(body, 0, body.Length);
            }
            finally
            {
                bool flag = stream != null;
                if (flag)
                {
                    ((IDisposable)stream).Dispose();
                }
            }
        }

        public static string GetLogRequest(this WebRequest request, string body)
        {
            var sb = new StringBuilder();
            sb.AppendLine(string.Format("REQUEST: {0} [{1}]", request.RequestUri, request.Method));
            sb.AppendLine(string.Format("ContentType: {0}, Timeout: {1}", request.ContentType, request.Timeout));
            if (body != null)
            {
                if (body.ToLower().IndexOf("password") == -1)
                {
                    if (body.Length > 255)
                        sb.AppendLine(string.Format("Body: {0}.......", body.Substring(0, 255)));
                    else
                        sb.AppendLine(string.Format("Body: {0} ", body));
                }
                else
                    sb.AppendLine(string.Format("Body: {0} ", "[NOT LOGGING PASSWORDS]"));
            }
            return sb.ToString();
        }

        public static string GetLogResponse(this string url, Exception ex)
        {
            var sb = new StringBuilder();
            sb.AppendLine(string.Format("RESPONSE: {0}", url));
            sb.AppendLine(string.Format("RESPONSE EXCEPTION: {0}", ex.Message));
            return sb.ToString();
        }

        public static string GetLogResponse(this HttpWebResponse response, string body, int numChars)
        {
            var sb = new StringBuilder();
            if (response != null)
            {
                var message = response.Headers.ToJson();    //FIX
                sb.AppendLine(string.Format("RESPONSE: {0} [{1}]", message, response.StatusCode));
                //sb.AppendLine(string.Format("ContentType: {0}", oHttpResponse.ContentType));
                if (body.Length > numChars)
                    sb.AppendLine(string.Format("Body: {0}.......", body.Substring(0, numChars)));
                else
                    sb.AppendLine(string.Format("Body: {0} ", body));
            }
            return sb.ToString();
        }

    }
}
