using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JonasPluginBase;
using Microsoft.Xrm.Sdk;

namespace SamplePlugin
{
    public class AccountVerification : JonasPluginBase.JonasPluginBase
    {
        public override void Execute(JonasPluginBag bag)
        {
            VerifyAccountInfo(bag);
        }

        private static void VerifyAccountInfo(JonasPluginBag bag)
        {
            if (bag.TargetEntity.Contains("fax"))
            {
                throw new InvalidPluginExecutionException("We don't allow use of Fax attribute");
            }
            else
            {
                bag.Trace("All is well.");
            }
        }
    }
}
