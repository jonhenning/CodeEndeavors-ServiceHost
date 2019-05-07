using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeEndeavors.ServiceHost.Common.Services
{
    public class Helpers
    {
        public static ServiceResult<T> ExecuteServiceResult<T>(Action<ServiceResult<T>> codeFunc) //where T : new()
        {
            var result = new ServiceResult<T>(true);
            try
            {
                codeFunc.Invoke(result);
            }
            catch (Exception ex)
            {
                result.AddException(ex);
            }
            finally
            {
                result.StopTimer();
            }
            return result;
        }

        public static async Task<ServiceResult<T>> ExecuteServiceResultAsync<T>(Func<ServiceResult<T>, Task> codeFunc) //where T : new()
        {
            var result = new ServiceResult<T>(true);
            try
            {
                await codeFunc.Invoke(result);
            }
            catch (Exception ex)
            {
                result.AddException(ex);
            }
            finally
            {
                result.StopTimer();
            }
            return result;
        }

        public static void HandleAssemblyResolve()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        private static ConcurrentDictionary<string, System.Reflection.Assembly> _resolvedNames = new ConcurrentDictionary<string, System.Reflection.Assembly>();
        public static System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, System.ResolveEventArgs args)
        {
            var name = new System.Reflection.AssemblyName(args.Name);

            if (name.Name != args.Name)
            {
                if (!_resolvedNames.ContainsKey(name.Name))
                {
                    Logging.Debug("CurrentDomain_AssemblyResolve: {0} != {1}", name.Name, args.Name);
                    _resolvedNames[name.Name] = System.Reflection.Assembly.LoadWithPartialName(name.Name);
                }
                else
                    Logging.Debug("CurrentDomain_AssemblyResolve (cached): {0} != {1}", name.Name, args.Name);
                return _resolvedNames[name.Name];
            }
            return null;    //should never happen
        }

        public static bool IsDebug
        {
            get { return System.Web.HttpContext.Current != null && System.Web.HttpContext.Current.IsDebuggingEnabled;  }
        }

    }
}
