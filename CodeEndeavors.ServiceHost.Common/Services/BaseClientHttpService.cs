using CodeEndeavors.Extensions;
using CodeEndeavors.ServiceHost.Extensions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CodeEndeavors.ServiceHost.Common.Services
{
    public enum AuthenticationType
    {
        None,
        BasicAuthentication,
        Custom
    }

	public class BaseClientHttpService
	{
		public delegate string AquireUserId();
        public delegate void ProcessAuthenticationHandler(HttpClient request, string user, string password);
		public int HttpRequestTimeout;
		public string HttpServiceUrl;
		public string RestfulServerExtension;
        public AuthenticationType AuthenticationType;
        public string HttpUser;
		public string HttpPassword;
        public HttpClientHandler HttpClientHandler;
		
        private string _controllerName;
		private CookieContainer _cookieJar;

        private BaseClientHttpService.ProcessAuthenticationHandler _processAuthenticationHandler;
        public BaseClientHttpService.ProcessAuthenticationHandler ProcessAuthentication
        {
            get  { return _processAuthenticationHandler; }
            set  { _processAuthenticationHandler = value; }
        }
		private BaseClientHttpService.AquireUserId _aquireUserIdDelegate;
		public BaseClientHttpService.AquireUserId AquireUserIdDelegate
		{
			get
			{
				return this._aquireUserIdDelegate;
			}
			set
			{
				this._aquireUserIdDelegate = value;
			}
		}
		public BaseClientHttpService(string controllerName, string httpServiceUrl, int requestTimeout, string restfulServerExtension) : this(controllerName, httpServiceUrl, requestTimeout, restfulServerExtension, null)
		{
		}
		public BaseClientHttpService(string controllerName, string httpServiceUrl, int requestTimeout, string restfulServerExtension, string logConfigFileName) : this(controllerName, httpServiceUrl, requestTimeout, restfulServerExtension, logConfigFileName, "", "")
		{
		}
		public BaseClientHttpService(string controllerName, string httpServiceUrl, int requestTimeout, string restfulServerExtension, string logConfigFileName, string httpUser, string httpPassword)
		{
			this.HttpUser = "";
			this.HttpPassword = "";
			this._controllerName = controllerName;
			this.HttpServiceUrl = httpServiceUrl;
			this.RestfulServerExtension = restfulServerExtension;
			this.HttpRequestTimeout = requestTimeout;
			this.HttpUser = httpUser;
			this.HttpPassword = httpPassword;
			if (!string.IsNullOrEmpty(logConfigFileName))
			{
				HttpLogger.HttpLogConfigFileName = logConfigFileName;
			}
			this.AquireUserIdDelegate = new BaseClientHttpService.AquireUserId(Handlers.AquireUserId);
		}

		public T GetHttpRequestObject<T>(string Url, bool compressedRequest, bool compressedResponse)
		{
			return this.GetHttpRequestObject<T>(Url, null, compressedRequest, compressedResponse);
		}

		public T GetHttpRequestObject<T>(string url, object body, bool compressedRequest, bool compressedResponse)
		{
            var jsonRequest = body != null ? body.ToJson(false, null, true) : null;
			string jsonResponse;
			if (compressedRequest && !string.IsNullOrEmpty(jsonRequest))
			{
				ZipPayload oZip = ConversionExtensions.ToCompress(jsonRequest);
				HttpLogger.Logger.Info(oZip.GetStatistics());
				jsonResponse = this.GetResponse(url, oZip.Bytes, this.HttpRequestTimeout, string.Empty, compressedRequest, compressedResponse, false);
			}
			else
			{
				jsonResponse = this.GetResponse(url, jsonRequest, this.HttpRequestTimeout, Encoding.UTF8, "application/json", compressedRequest, compressedResponse);
			}

            if (jsonResponse.StartsWith("{\"Message\":\""))
            {
                var errorDict = jsonResponse.ToObject<Dictionary<string, object>>();
                throw new Exception(errorDict.ToJson());
            }


			return jsonResponse.ToObject<T>();
		}
		public string RequestUrl(string method)
		{
			return this.RequestUrl(method, "");
		}
		public string RequestUrl(string method, string[] suffix)
		{
			return this.RequestUrl(method, string.Join("/", suffix));
		}
		public string RequestUrl(string method, string suffix)
		{
			var userId = this.AquireUserIdDelegate();

            return this.HttpServiceUrl.PathCombine(this._controllerName + this.RestfulServerExtension, "/")
                .PathCombine(method, "/")
                .PathCombine(userId, "/")
                .PathCombine(suffix, "/");
		}
		
        public byte[] GetData(Dictionary<string, string> formFields, Encoding encoding)
		{
			var sb = new StringBuilder();
            foreach (var field in formFields.Keys)
			{
				sb.AppendFormat(string.Format("{0}={1}&", field, formFields[field]), new object[0]);
			}
			return encoding.GetBytes(sb.ToString());
		}
		private string ResolveController(string controllerName)
		{
			return controllerName + this.RestfulServerExtension;
		}
		private string GetResponse(string url, int timeOut, bool compressedRequest, bool compressedResponse)
		{
			return this.GetResponse(url, "", timeOut, Encoding.UTF8, "", compressedRequest, compressedResponse);
		}
		private string GetResponse(string url, Dictionary<string, string> formFields, int timeOut, Encoding encoding, string contentType, bool compressedRequest, bool compressedResponse)
		{
			return this.GetResponse(url, encoding.GetString(this.GetData(formFields, encoding)), timeOut, encoding, contentType, compressedRequest, compressedResponse);
		}
		private string GetResponse(string url, string body, int timeOut, Encoding encoding, string contentType, bool compressedRequest, bool compressedResponse)
		{
			return this.GetResponse(url, string.IsNullOrEmpty(body) ? null : encoding.GetBytes(body), timeOut, contentType, compressedRequest, compressedResponse, false);
		}
		private string GetResponse(string url, byte[] body, int timeOut, string contentType, bool compressedRequest, bool compressedResponse, bool triedAuthAlready)
		{
			string responseText = "";
			string GetResponse;
			try
			{
                HttpClient request = null;
                if (HttpClientHandler == null)
                    request = new HttpClient() { BaseAddress = new Uri(url) };
                else
                    request = new HttpClient(HttpClientHandler) { BaseAddress = new Uri(url) };

                if (this.AuthenticationType == AuthenticationType.BasicAuthentication && this.ProcessAuthentication == null)
                    this.ProcessAuthentication = Handlers.ProcessBasicAuthentication;

                if (this.AuthenticationType != AuthenticationType.None && this.ProcessAuthentication != null)
                    this.ProcessAuthentication(request, HttpUser, HttpPassword);

                if (timeOut > 0)
                    request.Timeout = new TimeSpan(0, 0, 0, 0, timeOut);


                Task<HttpResponseMessage> responseTask = null;
                if (body == null)
                    responseTask = request.GetAsync("");
                else
                {
                    var byteContent = new ByteArrayContent(body);
                    if (!string.IsNullOrEmpty(contentType))
                        byteContent.Headers.Add("Content-Type", contentType);
                    responseTask = request.PostAsync("", byteContent);
                }
				
                HttpLogger.Logger.InfoFormat("GetHttp Request: {0}", url);

                if (compressedRequest == false && HttpLogger.Logger.IsDebugEnabled)
					HttpLogger.Logger.Debug(HttpLogger.GetLogRequest(request, (body == null) ? "" : ConversionExtensions.ToString(body)));

                var response = responseTask.Result;
                
				if (compressedResponse)
				{
					ZipPayload zip = null;
                    responseText = CodeEndeavors.ServiceHost.Extensions.HttpExtensions.GetTextDecompressedBase64(response, ref zip);
					HttpLogger.Logger.Info(zip.GetStatistics());
				}
				else
					responseText = CodeEndeavors.ServiceHost.Extensions.HttpExtensions.GetText(response);

                HttpLogger.Logger.InfoFormat("GetHttp Response: {0}", response.StatusCode);
                if (HttpLogger.Logger.IsDebugEnabled)
					HttpLogger.Logger.Debug(HttpLogger.GetLogResponse(response, responseText, 255));

			}
            //catch (WebException ex)
            //{
            //    HttpLogger.Logger.Error("FAIL: " + ex.Message);
            //    bool flag = ex.Response != null;
            //    if (ex.Response != null)
            //    {
            //        throw new Exception(HttpLogger.GetLogResponse(url, ex));
            //        if (((HttpWebResponse)ex.Response).StatusCode == HttpStatusCode.Forbidden && !triedAuthAlready)
            //        {
            //            this._cookieJar = null;
            //            return this.GetResponse(url, body, timeOut, contentType, compressedRequest, compressedResponse, true);
            //        }
            //    }
            //    throw new Exception(HttpLogger.GetLogResponse(ex.Response, CodeEndeavors.ServiceHost.Extensions.HttpExtensions.GetText(ex.Response), 1000));
            //}
			catch (Exception ex)
			{
				HttpLogger.Logger.Error("FAIL: " + ex.Message);
				throw new Exception(HttpLogger.GetLogResponse(url, ex));
			}
			return responseText;
		}
        //private CookieContainer HandleAuth()
        //{
        //    try
        //    {
        //        if (!string.IsNullOrEmpty(this.HttpUser))
        //        {
        //            var url = this.HttpServiceUrl.PathCombine("ServiceHostAuth" + this.RestfulServerExtension, "/").PathCombine("Authenticate", "/");

        //            if (this._cookieJar == null)
        //            {
        //                var formFields = new Dictionary<string, string>();
        //                formFields["user"] = this.HttpUser;
        //                formFields["password"] = this.HttpPassword;
        //                var req = (HttpWebRequest)WebRequest.Create(url);
        //                req.Method = "POST";
        //                this._cookieJar = new CookieContainer();
        //                req.CookieContainer = this._cookieJar;
        //                CodeEndeavors.ServiceHost.Extensions.HttpExtensions.WriteText(req, ConversionExtensions.ToCompress(formFields.ToJson(false, null, true)).Bytes);
        //                var res = (HttpWebResponse)req.GetResponse();
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        HttpLogger.Logger.Error("FAIL: " + ex.Message);
        //    }
        //    return this._cookieJar;
        //}
	}
}

