using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeEndeavors.ServiceHost.Extensions;

namespace CodeEndeavors.ServiceHost.Common.Services
{
    public class ServiceLocator
    {
        static ServiceLocator()
        {
            RestfulServerExtension = ""; // ".mvc";
            DefaultHttpRequestTimeOut = 10000;
        }

        public static string RestfulServerExtension { get; set; }
        public static int DefaultHttpRequestTimeOut { get; set; }

        public static T CreateService<T>(string serviceUrl, int httpTimeOut)
        {
            return CreateService<T>(serviceUrl, httpTimeOut);
        }

        public static T CreateService<T>(string serviceUrl)
        {
            return CreateService<T>(serviceUrl, DefaultHttpRequestTimeOut);
        }

        public static T CreateService<T>(string serviceUrl, int httpTimeOut, string userName, string password, AuthenticationType authenticationType)
        {
            string classString = typeof(T).AssemblyQualifiedName; //typeof(T).FullName;
            T service = default(T);

            if (serviceUrl == "stub")
                service = (T)classString.ReflectToObject();
            else
            {
                if (serviceUrl.EndsWith("/") == false)
                    serviceUrl += "/";

                if (authenticationType != AuthenticationType.None)
                    service = (T)classString.ReflectToObject(serviceUrl, httpTimeOut, RestfulServerExtension, userName, password, authenticationType);
                else 
                    service = (T)classString.ReflectToObject(serviceUrl, httpTimeOut, RestfulServerExtension);
            }

            return service;
        }


        private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        public static Dictionary<Type, object> Services
        {
            get
            {
                return _services;
            }
        }

        public static T Resolve<T>()
        {
            try
            {
                return (T)_services[typeof(T)];
            }
            catch
            {
                throw new Exception(string.Format("Service not registered ({0})", typeof(T).ToString()));
            }
        }

        public static void Register<T>()
        {
            Register(CreateService<T>("stub"));
        }

        public static void Register<T>(string serviceUrl)
        {
            Register(CreateService<T>(serviceUrl, DefaultHttpRequestTimeOut));
        }

        public static void Register<T>(string serviceUrl, int httpTimeOut)
        {
            Register(CreateService<T>(serviceUrl, httpTimeOut, null, null, AuthenticationType.None));
        }

        public static void Register<T>(string serviceUrl, int httpTimeOut, string user, string password, AuthenticationType authenticationType)
        {
            Register(CreateService<T>(serviceUrl, httpTimeOut, user, password, authenticationType));
        }

        public static void Register<T>(T service)
        {
            _services[typeof(T)] = service;
        }
    }
}
