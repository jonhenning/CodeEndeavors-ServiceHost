using CodeEndeavors.ServiceHost.Common.Services.LoggingServices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
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
                return HttpLogger._logConfigFileName;
            }
            set
            {
                HttpLogger._logConfigFileName = value;
                HttpLogger.ConfigureLogging();
            }
        }
        public static string HttpLoggerKey
        {
            get
            {
                return HttpLogger._loggerKey;
            }
            set
            {
                HttpLogger._loggerKey = value;
                HttpLogger.ConfigureLogging();
            }
        }
        public static ILog Logger
        {
            get
            {
                bool flag = HttpLogger._logger == null;
                if (flag)
                {
                    HttpLogger._logger = Log.GetLogger(HttpLogger.HttpLoggerKey);
                }
                return HttpLogger._logger;
            }
        }
        [DebuggerNonUserCode]
        public HttpLogger()
        {
        }
        public static string GetLogRequest(WebRequest Request, string Body)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Format("REQUEST: {0} [{1}]", Request.RequestUri, Request.Method));
            sb.AppendLine(string.Format("ContentType: {0}, Timeout: {1}", Request.ContentType, Request.Timeout));
            bool flag = Body != null;
            if (flag)
            {
                bool flag2 = Body.ToLower().IndexOf("password") == -1;
                if (flag2)
                {
                    bool flag3 = Body.Length > 255;
                    if (flag3)
                    {
                        sb.AppendLine(string.Format("Body: {0}.......", Body.Substring(0, 255)));
                    }
                    else
                    {
                        sb.AppendLine(string.Format("Body: {0} ", Body));
                    }
                }
                else
                {
                    sb.AppendLine(string.Format("Body: {0} ", "[NOT LOGGING PASSWORDS]"));
                }
            }
            return sb.ToString();
        }
        public static string GetLogResponse(string Url, Exception ex)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Format("RESPONSE: {0}", Url));
            sb.AppendLine(string.Format("RESPONSE EXCEPTION: {0}", ex.Message));
            return sb.ToString();
        }
        public static string GetLogResponse(WebResponse Response, string Body, int NumChars)
        {
            HttpWebResponse oHttpResponse = (HttpWebResponse)Response;
            StringBuilder sb = new StringBuilder();
            bool flag = Response != null;
            if (flag)
            {
                sb.AppendLine(string.Format("RESPONSE: {0} [{1} - {2}]", oHttpResponse.ResponseUri, (int)oHttpResponse.StatusCode, oHttpResponse.StatusDescription));
                sb.AppendLine(string.Format("ContentType: {0}", oHttpResponse.ContentType));
                flag = (Body.Length > NumChars);
                if (flag)
                {
                    sb.AppendLine(string.Format("Body: {0}.......", Body.Substring(0, NumChars)));
                }
                else
                {
                    sb.AppendLine(string.Format("Body: {0} ", Body));
                }
            }
            return sb.ToString();
        }
        private static void ConfigureLogging()
        {
            HttpLogger._logger = null;
            Log.Configure(HttpLogger.HttpLogConfigFileName, HttpLogger.HttpLoggerKey);
        }
    }
}
