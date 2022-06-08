using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeEndeavors.ServiceHost.Common.Services.Profiler
{
    public class NoOpProfileCapture : IServiceHostProfilerCapture
    {

        public NoOpProfileCapture(string eventName)
        {
            //
        }

        public string Results
        {
            get { return null; }
        }

        public void AppendResults(string results)
        {
        }

        public IDisposable CustomTiming(string category, string commandString)
        {
            return new NoOpDisposable();
        }

        public void Dispose()
        {

        }
    }
}
