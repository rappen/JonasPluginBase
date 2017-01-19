using JonasPluginBase;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Text;

namespace SamplePlugin.Extensions
{
    internal static class BagExtensions
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

        internal static void AccountUpdateStats(this JonasPluginBag bag, Guid accountid)
        {
            var fetchexpr = new FetchExpression(string.Format(fetch, accountid));
            var results = bag.Service.RetrieveMultiple(fetchexpr);

            var descr = new StringBuilder();
            foreach (var familystatus in results.Entities)
            {
                var status = bag.GetOptionsetLabel("contact", "familystatuscode", (int)familystatus.AttributeToBaseType("Status"));
                var count = familystatus.AttributeToBaseType("Count");
                descr.AppendLine($"{count} {status}");
            }
            bag.Trace("Description:\n{0}", descr.ToString().Trim());
            var account = new Entity("account", accountid);
            account["description"] = descr.ToString();
            bag.Service.Update(account);
        }
    }
}
