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

        public void Step(string name)
        {

        }

        public void Dispose()
        {

        }
    }
}
