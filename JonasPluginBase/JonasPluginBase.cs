using Microsoft.Xrm.Sdk;
using System;
using System.Diagnostics;

namespace JonasPluginBase
{
    public abstract class JonasPluginBase : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            using (var bag = new JonasPluginBag(serviceProvider))
            {
                var watch = Stopwatch.StartNew();
                Execute(bag);
                watch.Stop();
                bag.Trace("Internal execution time: {0} ms", watch.ElapsedMilliseconds);
            }
        }

        public abstract void Execute(JonasPluginBag bag);
    }
}
