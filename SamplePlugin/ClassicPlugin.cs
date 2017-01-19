using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Linq;
using System.Text;

namespace SamplePlugin
{
    public class ClassicPlugin : IPlugin
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

        public void Execute(IServiceProvider serviceProvider)
        {
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var accountid = GetAccountId(context);

            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = serviceFactory.CreateOrganizationService(null);

            var fetchexpr = new FetchExpression(string.Format(fetch, accountid));
            var results = service.RetrieveMultiple(fetchexpr);

            var descr = new StringBuilder();
            foreach (var familystatus in results.Entities)
            {
                var status = familystatus["Status"];
                var count = familystatus["Count"];
                descr.AppendLine($"{count} {status}");
            }
            var account = new Entity("account", accountid);
            account["description"] = descr.ToString();
            service.Update(account);
        }

        private static Guid GetAccountId(IPluginExecutionContext context)
        {
            var contact = (Entity)context.InputParameters["Target"];
            var accountid = GetAccountIdFromContact(contact);
            if (context.PostEntityImages.Count > 0)
            {
                var postcontact = context.PreEntityImages.Values.FirstOrDefault();
                accountid = GetAccountIdFromContact(postcontact);
            }

            return accountid;
        }

        private static Guid GetAccountIdFromContact(Entity contact)
        {
            if (contact.Contains("parentcustomerid"))
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
