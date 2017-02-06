using Microsoft.Xrm.Sdk;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JonasPluginBase
{
    public abstract class JonasCodeActivityBase : CodeActivity
    {
        protected override void Execute(CodeActivityContext context)
        {
            using (var bag = new JonasPluginBag(context))
            {
                var watch = Stopwatch.StartNew();
                try
                {
                    Execute(bag);
                }
                catch (Exception e)
                {
                    bag.Trace("*** Exception ***\n{0}", e);
                    throw;
                }
                finally
                {
                    watch.Stop();
                    bag.Trace("Internal execution time: {0} ms", watch.ElapsedMilliseconds);
                }
            }
        }

        public abstract void Execute(JonasPluginBag bag);
    }
}
