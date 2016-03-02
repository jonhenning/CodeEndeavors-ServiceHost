using CodeEndeavors.Extensions;
//using CodeEndeavors.ServiceHost.Common.Services.LoggingServices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace CodeEndeavors.ServiceHost.Common.Services
{
	public class ClientCommandResult<T>
	{
		private T _data;
		public TimeSpan ExecutionTime;
		public TimeSpan ServerExecutionTime;
		public string StatusMessage;
        //public string LoggerKey;
		public bool Success;
		private List<string> _errors;
		private List<string> _messages;
		private Stopwatch _watch;
		public T Data
		{
			get
			{
				return this._data;
			}
			set
			{
				this._data = value;
			}
		}
		public List<string> Messages
		{
			get
			{
				return this._messages;
			}
		}
		public List<string> Errors
		{
			get
			{
				return this._errors;
			}
		}
		public ClientCommandResult() : this(true)
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
		public void ReportResult(T data, bool success)
		{
			this.Success = success;
			this.Data = data;
			this.StopTimer();
            if (Logging.IsDebugEnabled)
				Logging.Log(Logging.LoggingLevel.Debug, this.ToString());
		}
		public void ReportResult(ServiceResult<T> result, bool success)
		{
			this.Success = result.Success;
			this.Data = result.Data;
			this.ServerExecutionTime = new TimeSpan(0, 0, 0, (int)Math.Round(result.ExecutionTime / 1000.0), (int)Math.Round(result.ExecutionTime % 1000.0));
			this.Errors.AddRange(result.Errors);
			this.StopTimer();
            if (Logging.IsDebugEnabled)
                Logging.Log(Logging.LoggingLevel.Debug, this.ToString());
		}
		public void ReportResult(ClientCommandResult<T> result, bool success)
		{
			this.Success = result.Success;
			this.Data = result.Data;
			this.ServerExecutionTime = result.ServerExecutionTime;
			this.Errors.AddRange(result.Errors);
			this.StopTimer();
            if (Logging.IsDebugEnabled)
                Logging.Log(Logging.LoggingLevel.Debug, this.ToString());
		}
		public void ReportResult<T2>(ClientCommandResult<T2> result, T data, bool success)
		{
			this.Success = result.Success;
			this.Data = data;
			this.ServerExecutionTime = result.ServerExecutionTime;
			this.Errors.AddRange(result.Errors);
			this.StopTimer();
            if (Logging.IsDebugEnabled)
                Logging.Log(Logging.LoggingLevel.Debug, this.ToString());
		}
		public void AddException(Exception ex)
		{
            Logging.Error(ex.ToString());
			this.Errors.Add(ex.ToString());
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
			sb.AppendLine(string.Format("Success: {0}   Time: {1}   Server Time: {2}", this.Success, this.ExecutionTime, this.ServerExecutionTime));
			bool flag = this.Errors.Count > 0;
			if (flag)
			{
				foreach (var error in this.Errors)
					sb.AppendLine(string.Format("ERROR: {0}", error));
			}
			return sb.ToString();
		}
	}
}

