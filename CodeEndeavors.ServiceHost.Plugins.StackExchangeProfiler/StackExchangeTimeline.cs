﻿using System;
using StackExchange.Profiling;
using CodeEndeavors.ServiceHost.Common.Services.Profiler;

namespace CodeEndeavors.ServiceHost.Plugins.StackExchangeProfiler
{
    public class StackExchangeTimeline : IServiceHostProfilerCapture
    {
        private IDisposable _step = null;
        private string _timingJson = null;
        private string _results = null;

        public StackExchangeTimeline(string eventName)
        {
            _step = MiniProfiler.Current?.Step(eventName + " " + StackExchange.Profiling.Helpers.StackTraceSnippet.Get());
        }

        public string Results
        {
            get
            {
                if (_timingJson == null)
                    _timingJson = MiniProfiler.ToJson();
                return _timingJson;
            }
        }

        public void AppendResults(string results)
        {
            _results = results;
        }
        //public void AppendResults(string results)
        //{
        //    if (!string.IsNullOrEmpty(results))
        //    {
        //        var profiler = MiniProfiler.FromJson(results);
        //        //profiler?.Root?.Children.ForEach(child => MiniProfiler.Current?.Head?.AddChild(child));
        //        profiler?.GetTimingHierarchy().ToList().ForEach(child => MiniProfiler.Current?.Head?.AddChild(child));
        //    }
        //}

        public IDisposable CustomTiming(string category, string commandString)
        {
            IDisposable ret = (IDisposable)MiniProfiler.Current?.CustomTiming(category, commandString);
            return ret != null ? ret : new NoOpDisposable();
        }

        public void Dispose()
        {
            _step?.Dispose();
            if (!string.IsNullOrEmpty(_results))
            {
                var profiler = MiniProfiler.FromJson(_results);
                profiler?.Root?.Children.ForEach(child => MiniProfiler.Current?.Head?.AddChild(child));
            }
        }
    }


}
