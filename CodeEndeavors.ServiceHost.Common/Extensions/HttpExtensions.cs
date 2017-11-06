using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeEndeavors.Extensions;
using System.Web;
using System.IO;
using System.Net;
using CodeEndeavors.ServiceHost.Common;
using CodeEndeavors.ServiceHost.Extensions;
using System.Runtime.CompilerServices;
using System.Net.Http;


namespace CodeEndeavors.ServiceHost.Extensions
{
    public static class HttpExtensions
    {
        public static T DeserializeJSON<T>(HttpRequest request)
        {
            return HttpExtensions.GetText(request).ToObject<T>();
        }
        //public static T DeserializeCompressedJSON<T>(HttpRequest request, ref ZipPayload zip)
        //{
        //    zip = ConversionExtensions.ToDecompress(request.InputStream);
        //    return ConversionExtensions.ToString(zip.Bytes).ToObject<T>();
        //}

        public static string GetText(HttpResponseMessage response)
        {
            return response.Content.ReadAsStringAsync().Result;
        }

        //public static string GetText(WebResponse response)
        //{
        //    string str = "";
        //    bool flag = response == null;
        //    string GetText;
        //    if (flag)
        //    {
        //        GetText = str;
        //    }
        //    else
        //    {
        //        StreamReader reader = new StreamReader(response.GetResponseStream());
        //        try
        //        {
        //            GetText = reader.ReadToEnd();
        //        }
        //        finally
        //        {
        //            flag = (reader != null);
        //            if (flag)
        //            {
        //                ((IDisposable)reader).Dispose();
        //            }
        //        }
        //    }
        //    return GetText;
        //}
        //public static string GetTextDecompressed(WebResponse response, ref ZipPayload zip)
        //{
        //    zip = ConversionExtensions.ToDecompress(response.GetResponseStream());
        //    return ConversionExtensions.ToString(zip.Bytes);
        //}

        //public static string GetTextDecompressedBase64(WebResponse response, ref ZipPayload zip)
        //{
        //    var text = GetText(response);

        //    //text is prefixed and suffixed with a quote... remove
        //    text = text.TrimStart('"').TrimEnd('"');

        //    var bytes = Convert.FromBase64String(text);
        //    zip = ConversionExtensions.ToDecompress(bytes);
        //    return ConversionExtensions.ToString(zip.Bytes);
        //}

        //public static string GetTextDecompressedBase64(HttpResponseMessage response, ref ZipPayload zip)
        //{
        //    var text = GetText(response);

        //    //text is prefixed and suffixed with a quote... remove
        //    text = text.TrimStart('"').TrimEnd('"');

        //    var bytes = Convert.FromBase64String(text);
        //    zip = ConversionExtensions.ToDecompress(bytes);
        //    return ConversionExtensions.ToString(zip.Bytes);
        //}

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
        //public static void WriteCompressedJSON(HttpResponse response, object data, ref ZipPayload zip)
        //{
        //    zip = ConversionExtensions.ToCompress(RuntimeHelpers.GetObjectValue(data).ToJson(false, null, true));
        //    response.BinaryWrite(zip.Bytes);
        //}
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
        public static string GetLogRequest(this HttpClient request, string body)
        {
            var sb = new StringBuilder();
            sb.AppendLine(string.Format("REQUEST: {0} [{1}]", request.BaseAddress, string.IsNullOrEmpty(body) ? "GET" : "POST"));
            sb.AppendLine(string.Format("ContentType: {0}, Timeout: {1}", request.DefaultRequestHeaders.Accept, request.Timeout));
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

        public static string GetLogRequest(this byte[] body)
        {
            var s = System.Text.Encoding.UTF8.GetString(body);
            if (s.Length > 1000)
                s = s.Substring(0, 1000);
            if (s.IndexOf("password", StringComparison.InvariantCultureIgnoreCase) > -1)
                s = "[NOT LOGGING PASSWORDS]";
            return s;
        }

        public static string GetLogResponse(this string url, Exception ex)
        {
            var sb = new StringBuilder();
            sb.AppendLine(string.Format("RESPONSE: {0}", url));
            sb.AppendLine(string.Format("RESPONSE EXCEPTION: {0}", ex.Message));
            return sb.ToString();
        }

        public static string GetLogResponse(this HttpResponseMessage response, string body, int numChars)
        {
            var sb = new StringBuilder();
            if (response != null)
            {
                sb.AppendLine(string.Format("RESPONSE: {0} [{1}]", response.RequestMessage, response.StatusCode));
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
