using CodeEndeavors.Extensions;
using CodeEndeavors.ServiceHost.Extensions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace CodeEndeavors.ServiceHost.Common.Services
{
	public class BaseClientHttpService
	{
		public delegate string AquireUserId();
		public int HttprequestTimeout;
		public string httpServiceUrl;
		public string restfulServerExtension;
		public string httpUser;
		public string httpPassword;
		private string _controllerName;
		private CookieContainer _cookieJar;
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
			this.httpUser = "";
			this.httpPassword = "";
			this._controllerName = controllerName;
			this.httpServiceUrl = httpServiceUrl;
			this.restfulServerExtension = restfulServerExtension;
			this.HttprequestTimeout = requestTimeout;
			this.httpUser = httpUser;
			this.httpPassword = httpPassword;
			bool flag = !string.IsNullOrEmpty(logConfigFileName);
			if (flag)
			{
				HttpLogger.HttpLogConfigFileName = logConfigFileName;
			}
			this.AquireUserIdDelegate = new BaseClientHttpService.AquireUserId(this._AquireUserId);
		}
		private string _AquireUserId()
		{
			var userId = "";
            if (Thread.CurrentPrincipal != null && !string.IsNullOrEmpty(Thread.CurrentPrincipal.Identity.Name))
                userId = Thread.CurrentPrincipal.Identity.Name;
			return userId;
		}
		public T GetHttpRequestObject<T>(string Url, bool compressedRequest, bool compressedResponse)
		{
			return this.GetHttpRequestObject<T>(Url, null, compressedRequest, compressedResponse);
		}
		public T GetHttpRequestObject<T>(string Url, object[] Body, bool compressedRequest, bool compressedResponse)
		{
			List<string> oBodyList = new List<string>();
			bool flag = Body != null;
			checked
			{
				if (flag)
				{
					for (int i = 0; i < Body.Length; i++)
					{
						object oItem = RuntimeHelpers.GetObjectValue(Body[i]);
                        oBodyList.Add(RuntimeHelpers.GetObjectValue(oItem).ToJson(false, null, true));
					}
				}
				return this.GetHttpRequestObject<T>(Url, oBodyList, compressedRequest, compressedResponse);
			}
		}
		public T GetHttpRequestObject<T>(string url, object body, bool compressedRequest, bool compressedResponse)
		{
            string sJsonRequest = RuntimeHelpers.GetObjectValue(body).ToJson(false, null, true);
			bool flag = compressedRequest && !string.IsNullOrEmpty(sJsonRequest);
			string jsonResponse;
			if (flag)
			{
				ZipPayload oZip = ConversionExtensions.ToCompress(sJsonRequest);
				HttpLogger.Logger.Info(oZip.GetStatistics());
				jsonResponse = this.GetResponse(url, oZip.Bytes, this.HttprequestTimeout, string.Empty, compressedRequest, compressedResponse, false);
			}
			else
			{
				jsonResponse = this.GetResponse(url, sJsonRequest, this.HttprequestTimeout, Encoding.UTF8, string.Empty, compressedRequest, compressedResponse);
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

            return this.httpServiceUrl.PathCombine(this._controllerName + this.restfulServerExtension, "/")
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
			return controllerName + this.restfulServerExtension;
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
				CookieContainer jar = this.HandleAuth();
				WebRequest request = WebRequest.Create(url);
				((HttpWebRequest)request).CookieContainer = jar;
                if (timeOut > 0)
                    request.Timeout = timeOut;

                if (!string.IsNullOrEmpty(contentType))
					request.ContentType = contentType;

				if (body == null)
					request.Method = "GET";
				else
				{
					request.Method = "POST";
					CodeEndeavors.ServiceHost.Extensions.HttpExtensions.WriteText(request, body);
				}
				
                HttpLogger.Logger.InfoFormat("GetHttp Request: {0}", url);

                if (compressedRequest == false && HttpLogger.Logger.IsDebugEnabled)
					HttpLogger.Logger.Debug(HttpLogger.GetLogRequest(request, (body == null) ? "" : ConversionExtensions.ToString(body)));

				var response = (HttpWebResponse)request.GetResponse();
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
			catch (WebException ex)
			{
				HttpLogger.Logger.Error("FAIL: " + ex.Message);
				bool flag = ex.Response != null;
                if (ex.Response != null)
                {
                    throw new Exception(HttpLogger.GetLogResponse(url, ex));
                    if (((HttpWebResponse)ex.Response).StatusCode == HttpStatusCode.Forbidden && !triedAuthAlready)
                    {
                        this._cookieJar = null;
                        return this.GetResponse(url, body, timeOut, contentType, compressedRequest, compressedResponse, true);
                    }
                }
				throw new Exception(HttpLogger.GetLogResponse(ex.Response, CodeEndeavors.ServiceHost.Extensions.HttpExtensions.GetText(ex.Response), 1000));
			}
			catch (Exception ex)
			{
				HttpLogger.Logger.Error("FAIL: " + ex.Message);
				throw new Exception(HttpLogger.GetLogResponse(url, ex));
			}
			GetResponse = responseText;
			return GetResponse;
		}
		private CookieContainer HandleAuth()
		{
			try
			{
                if (!string.IsNullOrEmpty(this.httpUser))
				{
                    var url = this.httpServiceUrl.PathCombine("ServiceHostAuth" + this.restfulServerExtension, "/").PathCombine("Authenticate", "/");

                    if (this._cookieJar == null)
					{
						var formFields = new Dictionary<string, string>();
						formFields["user"] = this.httpUser;
						formFields["password"] = this.httpPassword;
						var req = (HttpWebRequest)WebRequest.Create(url);
						req.Method = "POST";
						this._cookieJar = new CookieContainer();
						req.CookieContainer = this._cookieJar;
                        CodeEndeavors.ServiceHost.Extensions.HttpExtensions.WriteText(req, ConversionExtensions.ToCompress(formFields.ToJson(false, null, true)).Bytes);
						var res = (HttpWebResponse)req.GetResponse();
					}
				}
			}
			catch (Exception ex)
			{
				HttpLogger.Logger.Error("FAIL: " + ex.Message);
			}
			return this._cookieJar;
		}
	}
}

