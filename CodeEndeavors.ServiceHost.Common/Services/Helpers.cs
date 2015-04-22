using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeEndeavors.ServiceHost.Common.Services
{
    public class Helpers
    {
        public delegate void ClientCommandResultHandler<T>(ClientCommandResult<T> result) where T : new();
        public delegate void ServiceResultHandler<T>(ServiceResult<T> result) where T : new();
        public static ClientCommandResult<T> ExecuteClientResult<T>(ClientCommandResultHandler<T> codeFunc) where T : new()
        {
            var result = new ClientCommandResult<T>(true);
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

        public static ServiceResult<T> ExecuteServiceResult<T>(ServiceResultHandler<T> codeFunc) where T : new()
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
