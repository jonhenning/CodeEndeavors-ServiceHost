using CodeEndeavors.Extensions;
using CodeEndeavors.ServiceHost.Common.Services;
using CodeEndeavors.ServiceHost.Common.Services.Profiler;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;
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

        //allows for simple way to determine if a logging implementation has already been registered
        private static ConcurrentDictionary<string, string> _loggingImplementationKeys = new ConcurrentDictionary<string, string>();
        public void ConfigureLogging(string implementationKey,string logLevel, Action<string, string> onLoggingMessage)
        {
            Logger.LogLevel = logLevel.ToType<Logger.LoggingLevel>();
            //if (!_loggingImplementationKeys.ContainsKey(implementationKey))
            if (_loggingImplementationKeys.TryAdd(implementationKey, logLevel))
            {
                Logger.OnLoggingMessage += (Logger.LoggingLevel level, string message) =>
                {
                    if (onLoggingMessage != null)
                        onLoggingMessage(level.ToString(), message);
                };
                //_loggingImplementationKeys[implementationKey] = logLevel;
            }
        }

        public static T ExecuteClient<T>(Func<ClientCommandResult<T>> codeFunc) //where T : new()
        {
            using (var capture = Timeline.Capture("BaseClient.ExecuteClient"))
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

        public async static Task<T> ExecuteClientAsync<T>(Func<Task<ClientCommandResult<T>>> codeFunc) //where T : new()
        {
            ClientCommandResult<T> clientCommandResult = await codeFunc();
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
