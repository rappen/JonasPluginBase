using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JonasPluginBase
{
    public class JonasTracingService : ITracingService, IDisposable
    {
        private ITracingService trace;

        public JonasTracingService(ITracingService Trace)
        {
            trace = Trace;
            this.Trace("*** Enter");
        }

        public void Dispose()
        {
            Trace("*** Exit");
        }

        public void Trace(string format, params object[] args)
        {
            if (trace != null)
            {
                var s = string.Format(format, args);
                trace.Trace(DateTime.Now.ToString("HH:mm:ss.fff") + "\t" + s);
            }
        }
    }
}
