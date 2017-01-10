using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JonasPluginBase;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;

namespace SamplePlugin
{
    public class AwesomePlugin : JonasPluginBase.JonasPluginBase
    {
        public override void Execute(JonasPluginBag bag)
        {
            if (bag.Context.PrimaryEntityName != "contact" || !bag.Context.InputParameters.Contains("Target"))
            {
                bag.Trace("Context not satisfying");
                return;
            }
            var accountid = GetAccountIdFromContact(bag.TargetEntity);
            bag.Trace("Accountid from target: {0}", accountid);
            if (accountid.Equals(Guid.Empty))
            {
                accountid = GetAccountIdFromContact(bag.PostImage);
                bag.Trace("Accountid from postimage: {0}", accountid);
            }
            if (!accountid.Equals(Guid.Empty))
            {
                bag.AccountUpdateStats(accountid);
            }
            var preaccountid = GetAccountIdFromContact(bag.PreImage);
            bag.Trace("Accountid from preimage: {0}", preaccountid);
            if (!preaccountid.Equals(Guid.Empty) && !preaccountid.Equals(accountid))
            {
                bag.AccountUpdateStats(preaccountid);
            }
        }

        private static Guid GetAccountIdFromContact(Entity contact)
        {
            if (contact != null && contact.Contains("parentcustomerid"))
            {
                var parentref = (EntityReference)contact["parentcustomerid"];
                if (parentref != null && parentref.LogicalName == "account")
                {
                    return parentref.Id;
                }
            }
            return Guid.Empty;
        }
    }
}
