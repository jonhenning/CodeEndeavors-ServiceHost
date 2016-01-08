using CodeEndeavors.ServiceHost.Common.Services.LoggingServices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CodeEndeavors.ServiceHost.Common
{
    public class HttpLogger
    {
        private static string _logConfigFileName;
        private static ILog _logger = null;
        private static string _loggerKey = "HttpServiceLogger";
        public static string HttpLogConfigFileName
        {
            get
            {
                return _logConfigFileName;
            }
            set
            {
                if (_logConfigFileName != value)
                {
                    _logConfigFileName = value;
                    ConfigureLogging();
                }
            }
        }
        public static string HttpLoggerKey
        {
            get
            {
                return _loggerKey;
            }
            set
            {
                _loggerKey = value;
                ConfigureLogging();
            }
        }
        public static ILog Logger
        {
            get
            {
                if (_logger == null)
                    _logger = Log.GetLogger(HttpLogger.HttpLoggerKey);
                return _logger;
            }
        }
        [DebuggerNonUserCode]
        public HttpLogger()
        {
        }
        public static string GetLogRequest(WebRequest request, string body)
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
        public static string GetLogRequest(HttpClient request, string body)
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
        public static string GetLogResponse(string Url, Exception ex)
        {
            var sb = new StringBuilder();
            sb.AppendLine(string.Format("RESPONSE: {0}", Url));
            sb.AppendLine(string.Format("RESPONSE EXCEPTION: {0}", ex.Message));
            return sb.ToString();
        }

        public static string GetLogResponse(HttpResponseMessage response, string body, int numChars)
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

        //public static string GetLogResponse(WebResponse Response, string Body, int NumChars)
        //{
        //    HttpWebResponse oHttpResponse = (HttpWebResponse)Response;
        //    StringBuilder sb = new StringBuilder();
        //    bool flag = Response != null;
        //    if (flag)
        //    {
        //        sb.AppendLine(string.Format("RESPONSE: {0} [{1} - {2}]", oHttpResponse.ResponseUri, (int)oHttpResponse.StatusCode, oHttpResponse.StatusDescription));
        //        sb.AppendLine(string.Format("ContentType: {0}", oHttpResponse.ContentType));
        //        flag = (Body.Length > NumChars);
        //        if (flag)
        //        {
        //            sb.AppendLine(string.Format("Body: {0}.......", Body.Substring(0, NumChars)));
        //        }
        //        else
        //        {
        //            sb.AppendLine(string.Format("Body: {0} ", Body));
        //        }
        //    }
        //    return sb.ToString();
        //}
        private static void ConfigureLogging()
        {
            _logger = null;
            Log.Configure(HttpLogger.HttpLogConfigFileName, HttpLogger.HttpLoggerKey);
        }
    }
}
