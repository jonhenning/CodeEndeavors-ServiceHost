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


namespace CodeEndeavors.ServiceHost.Extensions
{
    public static class HttpExtensions
    {
        public static T DeserializeJSON<T>(HttpRequest request)
        {

            return HttpExtensions.GetText(request).ToObject<T>();
        }
        public static T DeserializeCompressedJSON<T>(HttpRequest request, ref ZipPayload zip)
        {
            zip = ConversionExtensions.ToDecompress(request.InputStream);
            return ConversionExtensions.ToString(zip.Bytes).ToObject<T>();
        }
        public static string GetText(WebResponse response)
        {
            string str = "";
            bool flag = response == null;
            string GetText;
            if (flag)
            {
                GetText = str;
            }
            else
            {
                StreamReader reader = new StreamReader(response.GetResponseStream());
                try
                {
                    GetText = reader.ReadToEnd();
                }
                finally
                {
                    flag = (reader != null);
                    if (flag)
                    {
                        ((IDisposable)reader).Dispose();
                    }
                }
            }
            return GetText;
        }
        public static string GetTextDecompressed(WebResponse response, ref ZipPayload zip)
        {
            zip = ConversionExtensions.ToDecompress(response.GetResponseStream());
            return ConversionExtensions.ToString(zip.Bytes);
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
            response.Write(RuntimeHelpers.GetObjectValue(data).ToJson());
        }
        public static void WriteCompressedJSON(HttpResponse response, object data, ref ZipPayload zip)
        {
            zip = ConversionExtensions.ToCompress(RuntimeHelpers.GetObjectValue(data).ToJson());
            response.BinaryWrite(zip.Bytes);
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
    }
}
