using CodeEndeavors.Extensions;
using CodeEndeavors.ServiceHost.Extensions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CodeEndeavors.ServiceHost.Common.Services
{
    public enum AuthenticationType
    {
        None,
        BasicAuthentication,
        OAuth2,
        Custom
    }

    public class BaseClientHttpService
    {
        //public delegate string AquireUserId();
        public delegate void ProcessAuthenticationHandler(HttpRequestMessage request, string user, string password, ref string token);
        public int HttpRequestTimeout;
        public string HttpServiceUrl;
        public string RestfulServerExtension;
        public AuthenticationType AuthenticationType;
        public string HttpUser;
        public string HttpPassword;
        private string _token;
        [Obsolete]
        public HttpClientHandler HttpClientHandler;

        private string _controllerName;
        private CookieContainer _cookieJar;

        private BaseClientHttpService.ProcessAuthenticationHandler _processAuthenticationHandler;

        private static HttpClient _httpClient;
        private static object syncRoot = new object();

        private static HttpClient httpClient
        {
            get
            {
                if (_httpClient == null)
                {
                    lock (syncRoot)
                    {
                        if (_httpClient == null)
                        {
                            Logging.Info("Initializing HttpClient");
                            ServicePointManager.UseNagleAlgorithm = false;
                            ServicePointManager.Expect100Continue = false;
                            ServicePointManager.DefaultConnectionLimit = int.MaxValue;
                            var compressionHandler = new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate };
                            _httpClient = new HttpClient(compressionHandler);
                            _httpClient.Timeout = System.Threading.Timeout.InfiniteTimeSpan; //https://stackoverflow.com/a/46877380
                        }
                    }
                }
                return _httpClient;
            }
        }

        public BaseClientHttpService.ProcessAuthenticationHandler ProcessAuthentication
        {
            get { return _processAuthenticationHandler; }
            set { _processAuthenticationHandler = value; }
        }
        private Func<string> _aquireUserIdDelegate;
        public Func<string> AquireUserIdDelegate
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
        public BaseClientHttpService(string controllerName, string httpServiceUrl, int requestTimeout, string restfulServerExtension) : this(controllerName, httpServiceUrl, requestTimeout, restfulServerExtension, "", "", "None")
        {
        }
        public BaseClientHttpService(string controllerName, string httpServiceUrl, int requestTimeout, string restfulServerExtension, string httpUser, string httpPassword, string authenticationType)
        {
            this.HttpUser = "";
            this.HttpPassword = "";
            this._controllerName = controllerName;
            this.HttpServiceUrl = httpServiceUrl;
            this.RestfulServerExtension = restfulServerExtension;
            this.HttpRequestTimeout = requestTimeout;
            this.HttpUser = httpUser;
            this.HttpPassword = httpPassword;
            this.AuthenticationType = authenticationType.ToType<AuthenticationType>();

            this.AquireUserIdDelegate = new Func<string>(Handlers.AquireUserId);
        }

        public T GetHttpRequestObject<T>(string url)
        {
            return this.GetHttpRequestObject<T>(url, null);
        }
        public async Task<T> GetHttpRequestObjectAsync<T>(string url)
        {
            return this.GetHttpRequestObject<T>(url, null);
        }

        public T GetHttpRequestObject<T>(string url, object body)
        {
            var jsonRequest = body != null ? body.ToJson(false, null, true) : null;
            var jsonResponse = this.getResponse(url, jsonRequest, this.HttpRequestTimeout, Encoding.UTF8, "application/json");

            if (jsonResponse.StartsWith("{\"Message\":\""))
            {
                var errorDict = jsonResponse.ToObject<Dictionary<string, object>>();
                throw new Exception(errorDict.ToJson());
            }
            return jsonResponse.ToObject<T>();
        }

        public async Task<T> GetHttpRequestObjectAsync<T>(string url, object body)
        {
            var jsonRequest = body != null ? body.ToJson(false, null, true) : null;
            var jsonResponse = await this.getResponseAsync(url, jsonRequest, this.HttpRequestTimeout, Encoding.UTF8, "application/json");

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

        private string getResponse(string url, int timeOut)
        {
            return this.getResponse(url, "", timeOut, Encoding.UTF8, "");
        }
        private string getResponse(string url, Dictionary<string, string> formFields, int timeOut, Encoding encoding, string contentType)
        {
            return this.getResponse(url, encoding.GetString(this.GetData(formFields, encoding)), timeOut, encoding, contentType);
        }
        private string getResponse(string url, string body, int timeOut, Encoding encoding, string contentType)
        {
            return this.getResponse(url, string.IsNullOrEmpty(body) ? null : encoding.GetBytes(body), timeOut, contentType, false).Result;
        }

        private async Task<string> getResponseAsync(string url, string body, int timeOut, Encoding encoding, string contentType)
        {
            return await this.getResponse(url, string.IsNullOrEmpty(body) ? null : encoding.GetBytes(body), timeOut, contentType, false);
        }

        private async Task<string> getResponse(string url, byte[] body, int timeOut, string contentType, bool triedAuthAlready)
        {
            string responseText = "";
            try
            {
                using (var requestMessage = createHttpRequestMessage(url, body, contentType))
                {
                    var timer = new System.Diagnostics.Stopwatch();
                    timer.Start();

                    if (this.AuthenticationType == AuthenticationType.BasicAuthentication && this.ProcessAuthentication == null)
                        this.ProcessAuthentication = Handlers.ProcessBasicAuthentication;
                    else if (this.AuthenticationType == AuthenticationType.OAuth2 && this.ProcessAuthentication == null)
                        this.ProcessAuthentication = Handlers.ProcessOAuth;

                    if (this.AuthenticationType != AuthenticationType.None && this.ProcessAuthentication != null)
                        this.ProcessAuthentication(requestMessage, HttpUser, HttpPassword, ref _token);

                    var cancellationToken = new CancellationTokenSource(); //https://stackoverflow.com/a/46877380
                    if (timeOut > 0)
                    {
                        ServicePointManager.FindServicePoint(new Uri(url)).ConnectionLeaseTimeout = timeOut;    //todo: overhead?
                        cancellationToken.CancelAfter(TimeSpan.FromMilliseconds(timeOut));
                    }
                    else
                        cancellationToken.CancelAfter(httpClient.Timeout);  //probably unneeded.  match whatever large limit is on client

                    if (Logging.IsDebugEnabled)
                        Logging.Log(Logging.LoggingLevel.Debug, "GetHttp Request: {0} \r\n{1}", url, body != null ? body.GetLogRequest() : "");
                    using (var response = await httpClient.SendAsync(requestMessage, cancellationToken.Token).ConfigureAwait(false))
                    {

                        responseText = await response.Content.ReadAsStringAsync(); //CodeEndeavors.ServiceHost.Extensions.HttpExtensions.GetText(response);

                        if (Logging.IsDebugEnabled)
                        {
                            //Logging.Log(Logging.LoggingLevel.Debug, "GetHttp Response: {0}", response.StatusCode);
                            Logging.Log(Logging.LoggingLevel.Debug, response.GetLogResponse(responseText, 255));
                        }

                        if (timer.ElapsedMilliseconds > 700)
                            Logging.Log(Logging.LoggingLevel.Info, "LONG RUNNING REQUEST: {0}ms {1} \r\n{2}", timer.ElapsedMilliseconds, url, body != null ? body.GetLogRequest() : "");
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Error("FAIL: " + ex.Message);
                throw new Exception(url.GetLogResponse(ex));
            }
            return responseText;
        }

        private static HttpRequestMessage createHttpRequestMessage(string apiUrl, byte[] body, string contentType)
        {
            var method = body == null ? HttpMethod.Get : HttpMethod.Post;
            var request = new HttpRequestMessage()
            {
                Method = method,
                RequestUri = new Uri(apiUrl),
            };

            if (body != null)
            {
                var byteContent = new ByteArrayContent(body);
                if (!string.IsNullOrEmpty(contentType))
                    byteContent.Headers.Add("Content-Type", contentType);
                request.Content = byteContent;
            }

            return request;
        }

    }
}


