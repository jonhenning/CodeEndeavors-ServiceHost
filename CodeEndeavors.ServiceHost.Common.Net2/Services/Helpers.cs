using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public static void HandleAssemblyResolve()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        public static System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, System.ResolveEventArgs args)
        {
            var name = new System.Reflection.AssemblyName(args.Name);

            if (name.Name != args.Name)
                return System.Reflection.Assembly.LoadWithPartialName(name.Name);

            return null;
        }
    }
}
