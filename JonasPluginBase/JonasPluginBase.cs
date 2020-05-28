using Microsoft.Xrm.Sdk;
using Rappen.Canary365.Plugin;
using System;
using System.Diagnostics;

namespace Jonas
{
    /// <summary>
    /// JonasPluginBase implements IPlugin to encapsulate a JonasPluginBag containing stuff from the service and context, and to log service requests to the Tracing Service.
    /// Remember to merge this assemply with the plugin assembly using a post-build event like this example:
    /// ilmerge.exe /keyfile:..\..\JonasKey.snk /target:library /copyattrs /targetplatform:v4,"C:\Windows\Microsoft.NET\Framework\v4.0.30319" "/out:$(TargetName).Merged.dll" "$(TargetFileName)" "JonasPluginBase.dll"
    /// </summary>
    public abstract class JonasPluginBase : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            using (var bag = new JonasPluginBag(serviceProvider))
            {
                var watch = Stopwatch.StartNew();
                try
                {
                    bag.TracingService.TraceContext(bag.PluginContext, false, true, true, bag.Service);
                    Execute(bag);
                }
                catch (Exception e)
                {
                    bag.trace(e.ToString());
                    throw;
                }
                finally
                {
                    watch.Stop();
                    bag.trace("Internal execution time: {0} ms", watch.ElapsedMilliseconds);
                }
            }
        }

        public abstract void Execute(JonasPluginBag bag);
    }
}
