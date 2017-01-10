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
        const string fetch = @"<fetch aggregate='true' >
  <entity name='contact' >
    <attribute name='familystatuscode' alias='Status' groupby='true' />
    <attribute name='contactid' alias='Count' aggregate='count' />
    <filter>
      <condition attribute='parentcustomerid' operator='eq' value='{0}' />
      <condition attribute='familystatuscode' operator='not-null' />
    </filter>
  </entity>
</fetch>";

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
                UpdateAccountStats(bag, accountid);
            }
            var preaccountid = GetAccountIdFromContact(bag.PreImage);
            bag.Trace("Accountid from preimage: {0}", preaccountid);
            if (!preaccountid.Equals(Guid.Empty) && !preaccountid.Equals(accountid))
            {
                UpdateAccountStats(bag, preaccountid);
            }
        }

        private static void UpdateAccountStats(JonasPluginBag bag, Guid accountid)
        {
            var fetchexpr = new FetchExpression(string.Format(fetch, accountid));
            var results = bag.Service.RetrieveMultiple(fetchexpr);

            var descr = new StringBuilder();
            foreach (var familystatus in results.Entities)
            {
                var status = bag.Service.GetOptionsetLabel("contact", "familystatuscode", (int)familystatus.AttributeToBaseType("Status"));
                var count = familystatus.AttributeToBaseType("Count");
                descr.AppendLine($"{count} {status}");
            }
            bag.Trace("Description:\n{0}", descr);
            var account = new Entity("account", accountid);
            account["description"] = descr.ToString();
            bag.Service.Update(account);
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
