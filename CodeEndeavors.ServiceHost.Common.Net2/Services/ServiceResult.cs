using System.Linq;
//using CodeEndeavors.ServiceHost.Common.Services.LoggingServices;
using CodeEndeavors.ServiceHost.Extensions;
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
        //public string LoggerKey;
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
        public ServiceResult(bool startTimer)
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
        public void AddException(Exception ex)
        {
            Logging.Error(ex.ToString());
            if (Helpers.IsDebug)
                this.Errors.Add(ex.ToString());
            else
                this.Errors.Add(ex.Message);
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
            var sb = new StringBuilder();

            var methodName = "Unknown";
            var frames = new System.Diagnostics.StackTrace().GetFrames();
            var executeResultFrame = frames.Where(f => f.GetMethod().Name == "ExecuteServiceResult").LastOrDefault();
            if (executeResultFrame != null)
            {
                var nextIndex = Array.IndexOf(frames, executeResultFrame) + 1;
                if (frames.Length > nextIndex)
                    methodName = frames[Array.IndexOf(frames, executeResultFrame) + 1].GetMethod().Name;
            }

            sb.AppendLine(string.Format("{0}   [Success: {1} Time: {2}]", methodName, this.Success, this.ExecutionTime / 1000.0));

            if (this.Errors.Count > 0)
            {
                List<string>.Enumerator enumerator = this.Errors.GetEnumerator();
                foreach (var error in this.Errors)
                    sb.AppendLine(string.Format("ERROR: {0}", error));
            }
            else
            {
                var json = this.Data.ToJson(false, null, true);
                if (json.Length > 255)
                    json = json.Substring(0, 255);
                if (json.ToLower().IndexOf("password") == -1)
                    sb.AppendLine("Data: " + json);
                else
                    sb.AppendLine(string.Format("Data: {0} ", "[NOT LOGGING PASSWORDS]"));
            }
            return sb.ToString();
        }
    }
}
