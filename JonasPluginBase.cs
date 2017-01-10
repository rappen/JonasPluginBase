using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JonasPluginBase
{
    public abstract class JonasPluginBase : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            using (var bag = new JonasPluginBag(serviceProvider))
            {
                var start = DateTime.Now;
                Execute(bag);
                bag.Trace("Internal execution time: {0}", DateTime.Now - start);
            }
        }

        public abstract void Execute(JonasPluginBag bag);
    }
}
