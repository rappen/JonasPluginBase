using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JonasPluginBase
{
    public static class JonasEntityExtensions
    {
        public static string Name(this Entity entity, JonasPluginBag bag, bool includeEntityType)
        {
            var result = string.Empty;
            var priatt = bag.PrimaryAttribute(entity.LogicalName);
            if (!string.IsNullOrEmpty(priatt) && entity.Contains(priatt))
            {
                result = entity[priatt].ToString();
            }
            if (string.IsNullOrEmpty(result))
            {
                result = entity.Id.ToString();
            }
            if (includeEntityType)
            {
                result = $"{entity.LogicalName} {result}";
            }
            return result;
        }

        public static object AttributeToBaseType(this Entity entity, string attribute)
        {
            if (!entity.Contains(attribute))
            {
                return null;
            }
            return AttributeToBaseType(entity[attribute]);
        }

        public static string AttributeToString(this Entity entity, string attribute)
        {
            if (!entity.Contains(attribute))
            {
                return string.Empty;
            }
            return AttributeToString(entity.LogicalName, attribute, entity[attribute]);
        }

        private static object AttributeToBaseType(object attribute)
        {
            if (attribute is AliasedValue)
                return AttributeToBaseType(((AliasedValue)attribute).Value);
            else if (attribute is EntityReference)
                return ((EntityReference)attribute).Id;
            else if (attribute is OptionSetValue)
                return ((OptionSetValue)attribute).Value;
            else if (attribute is Money)
                return ((Money)attribute).Value;
            else
                return attribute;
        }

        private static string AttributeToString(string entity, string attribute, object value)
        {
            if (value is AliasedValue)
                return AttributeToString(entity, ((AliasedValue)value).AttributeLogicalName, ((AliasedValue)value).Value);
            //else if (value is EntityReference)
            //    return ((EntityReference)value).Id;
            //else if (value is OptionSetValue)
            //    return ((OptionSetValue)value).Value;
            else if (value is DateTime)
                return ((DateTime)value).ToString("G");
            else if (value is Money)
                return ((Money)value).Value.ToString("C");
            if (value != null)
                return value.ToString();
            else
                return null;
        }
    }
}
