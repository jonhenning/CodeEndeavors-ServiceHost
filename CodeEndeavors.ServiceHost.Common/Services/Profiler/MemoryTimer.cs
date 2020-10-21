using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeEndeavors.ServiceHost.Common.Services.Profiler
{
    //there are places in code that logging and timeline code are not possible (app start and background threads).
    //this class allows those timings to be captured in memory and will be exposed from some portal endpoint
    public class MemoryTimer : IDisposable
    {
        public static ConcurrentDictionary<string, List<string>> StaticTimings = new ConcurrentDictionary<string, List<string>>();

        public readonly Stopwatch StopWatch = new Stopwatch();
        private readonly string _message;
        private bool _info;
        public string Id { get; set; }


        public MemoryTimer(string message)
        {
            Id = Guid.NewGuid().ToString();
            StaticTimings[Id] = new List<string>();
            StaticTimings[Id].Add($"{message} - {DateTimeOffset.Now.ToString()} Start");
            _message = message;
            StopWatch.Start();
        }

        public MemoryTimer(string message, bool info)
        {
            _message = message;
            _info = info;
            StopWatch.Start();
        }

        public void Dispose()
        {
            StopWatch.Stop();
            StaticTimings[Id].Add($"{_message} - {StopWatch.ElapsedMilliseconds} ms Total");
            GC.SuppressFinalize(this);
        }
    }

    public class MemoryTiming : IDisposable
    {
        private MemoryTimer _timer;
        public MemoryTiming(MemoryTimer timer, string message)
        {
            _timer = timer;
            StartMilliseconds = timer.StopWatch.ElapsedMilliseconds;
            Message = message;
        }
        public string Message { get; set; }
        public long StartMilliseconds { get; set; }

        public void Dispose()
        {
            MemoryTimer.StaticTimings[_timer.Id].Add($"{Message} - {_timer.StopWatch.ElapsedMilliseconds - StartMilliseconds} ms Total");
            _timer = null;
        }

    }

}
