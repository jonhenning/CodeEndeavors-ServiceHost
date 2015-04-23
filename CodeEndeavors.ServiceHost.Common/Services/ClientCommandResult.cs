using CodeEndeavors.Extensions;
using CodeEndeavors.ServiceHost.Common.Services.LoggingServices;
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
        public string LoggerKey;
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
		public ClientCommandResult(bool StartTimer)
		{
			this._messages = new List<string>();
			this._errors = new List<string>();
			this._watch = new Stopwatch();
			if (StartTimer)
			{
				this.StartTimer();
			}
		}
		public void ReportResult(T Data, bool Success)
		{
			this.Success = Success;
			this.Data = Data;
			this.StopTimer();
			bool isDebugEnabled = Log.IsDebugEnabled;
			if (isDebugEnabled)
			{
				Log.Debug(this.ToString(), this.LoggerKey);
			}
		}
		public void ReportResult(ServiceResult<T> Result, bool Success)
		{
			this.Success = Result.Success;
			this.Data = Result.Data;
			this.ServerExecutionTime = checked(new TimeSpan(0, 0, 0, (int)Math.Round(Result.ExecutionTime / 1000.0), (int)Math.Round(Result.ExecutionTime % 1000.0)));
			this.Errors.AddRange(Result.Errors);
			this.StopTimer();
			bool isDebugEnabled = Log.IsDebugEnabled;
			if (isDebugEnabled)
			{
                Log.Debug(this.ToString(), this.LoggerKey);
			}
		}
		public void ReportResult(ClientCommandResult<T> Result, bool Success)
		{
			this.Success = Result.Success;
			this.Data = Result.Data;
			this.ServerExecutionTime = Result.ServerExecutionTime;
			this.Errors.AddRange(Result.Errors);
			this.StopTimer();
			bool isDebugEnabled = Log.IsDebugEnabled;
			if (isDebugEnabled)
			{
                Log.Debug(this.ToString(), this.LoggerKey);
			}
		}
		public void ReportResult<T2>(ClientCommandResult<T2> Result, T Data, bool Success)
		{
			this.Success = Result.Success;
			this.Data = Data;
			this.ServerExecutionTime = Result.ServerExecutionTime;
			this.Errors.AddRange(Result.Errors);
			this.StopTimer();
			bool isDebugEnabled = Log.IsDebugEnabled;
			if (isDebugEnabled)
			{
                Log.Debug(this.ToString(), this.LoggerKey);
			}
		}
		public void AddException(Exception ex)
		{
            Log.Error(ex.ToString(), this.LoggerKey);
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

