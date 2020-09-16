using CodeEndeavors.Extensions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeEndeavors.ServiceHost.Common.Services.Profiler
{
    public static class Timeline
    {
        private static string _profilerName = ConfigurationManager.AppSettings.GetSetting("Profiler.Name", "") + "Timeline";
        private static Type _captureType = null;

        public static IServiceHostProfilerCapture Capture(string eventName)
        {
            if (_captureType == null)
            {
                var types = typeof(IServiceHostProfilerCapture).GetAllTypes();
                _captureType = types.Where(t => t.Name == _profilerName).FirstOrDefault();
            }
            if (_captureType != null)
                return (IServiceHostProfilerCapture)Activator.CreateInstance(_captureType, eventName);
            return new NoOpProfileCapture(eventName);
        }
    }
}
