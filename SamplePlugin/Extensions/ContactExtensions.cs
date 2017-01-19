using Microsoft.Xrm.Sdk;
using System;

namespace SamplePlugin.Extensions
{
    internal static class ContactExtensions
    {
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
