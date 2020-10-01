using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeEndeavors.ServiceHost.Common.Services.Profiler
{
    public interface IServiceHostProfilerCapture : IDisposable
    {
        string Results { get; }
        void AppendResults(string results);
        void Step(string name);
    };
}
