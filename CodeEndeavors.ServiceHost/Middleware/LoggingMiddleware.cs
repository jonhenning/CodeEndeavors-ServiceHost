using CodeEndeavors.ServiceHost.Common;
using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace CodeEndeavors.ServiceHost.Middleware
{
    public class LoggingMiddleware
    {
        private Func<IDictionary<string, object>, Task> _next;
        public LoggingMiddleware(Func<IDictionary<string, object>, Task> next)
        {
            _next = next;
        }

        public async Task Invoke(IDictionary<string, object> env)
        {
            var context = new OwinContext(env);

            if (HttpLogger.Logger.IsDebugEnabled)
            {
                HttpLogger.Logger.Debug(context.Request.Uri.AbsoluteUri);

                //http://stackoverflow.com/questions/26214113/how-can-i-safely-intercept-the-response-stream-in-a-custom-owin-middleware
                // Buffer the response
                var stream = context.Response.Body;
                var buffer = new MemoryStream();
                context.Response.Body = buffer;
    
                await _next(env);

                buffer.Seek(0, SeekOrigin.Begin);
                var reader = new StreamReader(buffer);
                string responseBody = await reader.ReadToEndAsync();

                // Now, you can access response body.
                HttpLogger.Logger.Debug(getLogResponse(context.Response, responseBody));

                // You need to do this so that the response we buffered
                // is flushed out to the client application.
                buffer.Seek(0, SeekOrigin.Begin);
                await buffer.CopyToAsync(stream);
            }
            else
                await _next(env);

        }

        private string getLogResponse(IOwinResponse response, string body)
        {
            if (!string.IsNullOrEmpty(body) && body.Length > 255)
                body = body.Substring(0, 255);
            return string.Format("Url: {0}  Status: {1}  Length: {2}\r\n{3}   ", response.Environment["owin.RequestPath"], response.StatusCode, response.ContentLength, body);
        }


    }
}