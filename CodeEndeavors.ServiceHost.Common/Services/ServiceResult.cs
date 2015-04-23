using CodeEndeavors.ServiceHost.Common.Services.LoggingServices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
namespace CodeEndeavors.ServiceHost.Common.Services
{
    public class ServiceResult<T>
    {
        private T _data;
        public double ExecutionTime;
        public string StatusMessage;
        public bool Success;
        public string LoggerKey;
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
            set
            {
                this._errors = value;
            }
        }
        public ServiceResult()
            : this(false)
        {
        }
        public ServiceResult(bool StartTimer)
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
            this.Success = true;
            this.Data = Data;
            this.StopTimer();
            bool isDebugEnabled = Log.IsDebugEnabled;
            if (isDebugEnabled)
            {
                Log.Debug(this.ToString(), LoggerKey);
            }
        }
        public void AddException(Exception ex)
        {
            Log.Error(ex.ToString(), LoggerKey);
            this.Errors.Add(ex.ToString());
        }
        public void StartTimer()
        {
            this._watch.Start();
        }
        public void StopTimer()
        {
            this._watch.Stop();
            this.ExecutionTime = this._watch.Elapsed.TotalMilliseconds;
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Format("Success: {0}   Time: {1}", this.Success, this.ExecutionTime / 1000.0));

            if (this.Errors.Count > 0)
            {
                List<string>.Enumerator enumerator = this.Errors.GetEnumerator();
                foreach (var error in this.Errors)
                    sb.AppendLine(string.Format("ERROR: {0}", error));
            }
            return sb.ToString();
        }
    }
}
