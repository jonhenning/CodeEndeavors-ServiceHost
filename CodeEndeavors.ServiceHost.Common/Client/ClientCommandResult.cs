using CodeEndeavors.Extensions;
using CodeEndeavors.ServiceHost.Common.Services;
using CodeEndeavors.ServiceHost.Common.Services.Profiler;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Logger = CodeEndeavors.ServiceHost.Common.Services.Logging;

namespace CodeEndeavors.ServiceHost.Common.Client
{
    public class ClientCommandResult<TData>
    {
        private TData _data;
        public TimeSpan ExecutionTime;
        public TimeSpan ServerExecutionTime;
        public string StatusMessage;
        public string ProfilerResults { get; set; }
        //public string LoggerKey;
        public bool Success;
        private List<string> _errors;
        private List<string> _messages;
        private Stopwatch _watch;
        public TData Data
        {
            get { return this._data; }
            set { this._data = value; }
        }
        public List<string> Messages
        {
            get { return this._messages; }
        }
        public List<string> Errors
        {
            get { return this._errors; }
        }
        public ClientCommandResult()
            : this(true)
        {
        }
        public ClientCommandResult(bool startTimer)
        {
            this._messages = new List<string>();
            this._errors = new List<string>();
            this._watch = new Stopwatch();
            if (startTimer)
            {
                this.StartTimer();
            }
        }
        public void ReportResult(TData data, bool success)
        {
            this.Success = success;
            this.Data = data;
            this.StopTimer();
            if (Logger.IsDebugEnabled)
                Logger.Debug(this.ToString());
        }
        public void ReportResult(ServiceResult<TData> result, bool success)
        {
            this.Success = result.Success;
            this.Data = result.Data;
            this.ServerExecutionTime = new TimeSpan(0, 0, 0, (int)Math.Round(result.ExecutionTime / 1000.0), (int)Math.Round(result.ExecutionTime % 1000.0));
            this.Errors.AddRange(result.Errors);
            this.Messages.AddRange(result.Messages);
            this.ProfilerResults = result.ProfilerResults;
            this.StopTimer();
            if (Logger.IsDebugEnabled)
                Logger.Debug(this.ToString());
        }
        public void ReportResult(ClientCommandResult<TData> result, bool success)
        {
            this.Success = result.Success;
            this.Data = result.Data;
            this.ServerExecutionTime = result.ServerExecutionTime;
            this.Errors.AddRange(result.Errors);
            this.ProfilerResults = result.ProfilerResults;
            this.StopTimer();
            if (Logger.IsDebugEnabled)
                Logger.Debug(this.ToString());
        }
        public void ReportResult<T2>(ClientCommandResult<T2> result, TData data, bool success)
        {
            this.Success = result.Success;
            this.Data = data;
            this.ServerExecutionTime = result.ServerExecutionTime;
            this.Errors.AddRange(result.Errors);
            this.ProfilerResults = result.ProfilerResults;
            this.StopTimer();
            if (Logger.IsDebugEnabled)
                Logger.Debug(this.ToString());
        }
        public void AddException(Exception ex)
        {
            Logger.Error(ex.ToString());
            if (Helpers.IsDebug)
                this.Errors.Add(ex.ToString());
            else
            {
                this.Errors.Add(ex.Message);
                if (ex.InnerException != null)
                    this.Errors.Add(ex.InnerException.Message);
            }
        }
        public void StartTimer()
        {
            this._watch.Start();
        }
        public void StopTimer()
        {
            this._watch.Stop();
            this.ExecutionTime = this._watch.Elapsed;
        }
        public override string ToString()
        {
            var sb = new StringBuilder();
            if (Helpers.IsDebug)
            {
                sb.AppendLine(string.Format("Success: {0}   Time: {1}   Server Time: {2}", this.Success, this.ExecutionTime, this.ServerExecutionTime));
                if (this.Errors.Count > 0)
                {
                    foreach (var error in this.Errors)
                        sb.AppendLine(string.Format("ERROR: {0}", error));
                }
            }
            else
                foreach (var error in this.Errors)
                    sb.AppendLine(error);
            return sb.ToString();
        }

        public static ClientCommandResult<TData> Execute(Action<ClientCommandResult<TData>> codeFunc)
        {
            using (var capture = Timeline.Capture("ClientCommandResult.Execute"))
            {
                var result = new ClientCommandResult<TData>(true);
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
                    capture?.AppendResults(result.ProfilerResults);

                    if (result.ServerExecutionTime != null && result.ServerExecutionTime.TotalMilliseconds > 0 && capture != null)
                    {
                        using (var custom = capture?.CustomTiming("SERVER EXECUTION TIME (App)", result.ServerExecutionTime.TotalMilliseconds.ToString() + "ms")) { };
                        using (var custom = capture?.CustomTiming("NETWORK LATENCY (App -> Web)", result.ExecutionTime.Subtract(result.ServerExecutionTime).TotalMilliseconds.ToString() + "ms")) { };
                    }

                }
                return result;
            }
        }

        public static ClientCommandResult<TData> Execute(Func<ServiceResult<TData>> codeFunc)
        {
            return Execute(result =>
            {
                var sr = codeFunc.Invoke();
                result.ReportResult(sr, true);
            });
        }

        public static async Task<ClientCommandResult<TData>> ExecuteAsync(Func<ClientCommandResult<TData>, Task> codeFunc)
        {
            var result = new ClientCommandResult<TData>(true);
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

        public static async Task<ClientCommandResult<TData>> ExecuteAsync(Func<Task<ServiceResult<TData>>> codeFunc)
        {
            return await ExecuteAsync(async result =>
            {
                var ret = await codeFunc.Invoke();
                result.ReportResult(ret, true);
            });
        }

    }
}
