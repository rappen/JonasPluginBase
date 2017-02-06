using System;
using JonasPluginBase;
using SamplePlugin.Extensions;

namespace SamplePlugin
{
    public class AggregateContactStatusToAccount : JonasPluginBase.JonasPluginBase
    {
        public override void Execute(JonasPluginBag bag)
        {
            if (!bag.PluginContext.ContactTriggered())
            {
                bag.Trace("Context not satisfying");
                return;
            }
            var accountid = bag.TargetEntity.GetAccountIdFromContact();
            bag.Trace("Accountid from target: {0}", accountid);
            if (accountid.Equals(Guid.Empty))
            {
                accountid = bag.PostImage.GetAccountIdFromContact();
                bag.Trace("Accountid from postimage: {0}", accountid);
            }
            if (!accountid.Equals(Guid.Empty))
            {
                bag.AccountUpdateStats(accountid);
            }
            var preaccountid = bag.PreImage.GetAccountIdFromContact();
            bag.Trace("Accountid from preimage: {0}", preaccountid);
            if (!preaccountid.Equals(Guid.Empty) && !preaccountid.Equals(accountid))
            {
                bag.AccountUpdateStats(preaccountid);
            }
        }
    }
}
