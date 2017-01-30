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
            if (bag.TargetEntity == null)
            {
                bag.Trace("No target entity - nothing to do.");
                return;
            }
            VerifyFaxNotUsed(bag);
        }

        private static void VerifyFaxNotUsed(JonasPluginBag bag)
        {
            if (!bag.TargetEntity.Contains("fax"))
            {
                bag.Trace("Fax was not touched, keep moving.");
                return;
            }
            if (!string.IsNullOrEmpty(bag.TargetEntity["fax"].ToString()))
            {
                throw new InvalidPluginExecutionException("Fax?? Get outta here!");
            }
            else
            {
                bag.Trace("Clearing fax - welcome to the future!");
            }
        }
    }
}
