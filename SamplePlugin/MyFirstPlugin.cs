using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JonasPluginBase;
using Microsoft.Crm.Sdk.Messages;

namespace SamplePlugin
{
    public class MyFirstPlugin : JonasPluginBase.JonasPluginBase
    {
        public override void Execute(JonasPluginBag bag)
        {
            var jonas = (WhoAmIResponse)bag.Service.Execute(new WhoAmIRequest());
            bag.Trace("I am: {0}", jonas.UserId);

            var entity = bag.TargetEntity;
            string name = entity.Name(bag, true);
            bag.Trace("Triggered by record: {0}", name);

            bag.Trace("I'm done now: {0}", DateTime.Now);
        }
    }
}
