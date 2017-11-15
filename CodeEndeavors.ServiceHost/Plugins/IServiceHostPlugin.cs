using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace CodeEndeavors.ServiceHost.Plugins
{
    public interface IServiceHostPlugin
    {
        int? Priority { get; }
        void Configure(IAppBuilder app, HttpConfiguration config);
    }
}
