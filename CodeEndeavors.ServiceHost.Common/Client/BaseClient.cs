using CodeEndeavors.Extensions;
using CodeEndeavors.ServiceHost.Common.Services;
using System;
using System.Diagnostics;
using Logger = CodeEndeavors.ServiceHost.Common.Services.Logging;

namespace CodeEndeavors.ServiceHost.Common.Client
{
    public abstract class BaseClient
    {
        public BaseClient()
        {
            Helpers.HandleAssemblyResolve();
            //_service = new Stubs.Sales();
        }

        public BaseClient(string httpServiceUrl, int requestTimeout, string restfulServerExtension)
        {
            Helpers.HandleAssemblyResolve();
        }
        public BaseClient(string httpServiceUrl, int requestTimeout, string restfulServerExtension, string httpUser, string httpPassword, string authenticationType)
        {
            Helpers.HandleAssemblyResolve();
        }

        public abstract void SetAquireUserIdDelegate(Func<string> func);

        public void ConfigureLogging(string logLevel, Action<string, string> onLoggingMessage)
        {
            Logger.LogLevel = logLevel.ToType<Logger.LoggingLevel>();
            Logger.OnLoggingMessage += (Logger.LoggingLevel level, string message) =>
            {
                if (onLoggingMessage != null)
                    onLoggingMessage(level.ToString(), message);
            };
        }

        public static T ExecuteClient<T>(Func<ClientCommandResult<T>> codeFunc) where T : new()
        {
            ClientCommandResult<T> clientCommandResult = codeFunc();
            if (clientCommandResult.Success)
            {
                return clientCommandResult.Data;
            }
            if (Helpers.IsDebug)
                throw new Exception(clientCommandResult.ToString());
            else
            {
                if (clientCommandResult.Errors.Count == 1)
                    throw new Exception(clientCommandResult.Errors[0]);
                else 
                    throw new Exception(clientCommandResult.Errors.ToJson());
            }
        }


    }
}
