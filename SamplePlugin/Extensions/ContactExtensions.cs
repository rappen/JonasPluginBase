using JonasPluginBase;
using Microsoft.Xrm.Sdk;
using System;

namespace SamplePlugin
{
    internal static class ContactExtensions
    {
        internal static bool ContactTriggered(this IPluginExecutionContext context)
        {
            return context.PrimaryEntityName == "contact" && context.InputParameters.Contains("Target");
        }

        internal static Guid GetAccountIdFromContact(this Entity contact)
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
