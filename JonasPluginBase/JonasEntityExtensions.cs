using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jonas
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
            return AttributeToString(entity.LogicalName, entity[attribute]);
        }

        public static Entity Clone(this Entity entity)
        {
            var result = new Entity(entity.LogicalName, entity.Id)
            {
                EntityState = entity.EntityState,
                RowVersion = entity.RowVersion
            };
            result.KeyAttributes.AddRange(entity.KeyAttributes.Select(k => new KeyValuePair<string, object>(k.Key, k.Value)));
            result.Attributes.AddRange(entity.Attributes.Select(a => new KeyValuePair<string, object>(a.Key, a.Value)));
            return result;
        }

        public static Entity Merge(this Entity baseEntity, Entity entity)
        {
            if (baseEntity == null)
            {
                return entity;
            }
            if (entity != null)
            {
                baseEntity.Attributes.AddRange(entity.Attributes.Where(a => !baseEntity.Contains(a.Key)).Select(b => new KeyValuePair<string, object>(b.Key, b.Value)));
            }
            return baseEntity;
        }

        public static string ExtractAttributes(this Entity entity, Entity preimage)
        {
            var attrs = new StringBuilder();
            var keys = entity.Attributes.Keys
                .Where(k => k != "createdon" &&
                            k != "createdby" &&
                            k != "createdonbehalfby" &&
                            k != "modifiedon" &&
                            k != "modifiedby" &&
                            k != "modifiedonbehalfby" &&
                            k != entity.LogicalName + "id" &&
                            k != "activityid").ToList();
            keys.Sort();
            if (entity.Contains(entity.LogicalName + "id"))
            {
                keys.Insert(0, entity.LogicalName + "id");
            }
            if (entity.Contains("activityid"))
            {
                keys.Insert(0, "activityid");
            }
            var attlen = GetMaxAttributeNameLength(keys);
            foreach (string attr in keys)
            {
                object origValue, preValue, baseValue = null;
                string origType = string.Empty, resultValue = string.Empty;

                origValue = entity.Attributes[attr];

                if (origValue != null)
                {
                    origType = origValue.GetType().ToString();
                    if (origType.Contains("."))
                    {
                        origType = origType.Split('.')[origType.Split('.').Length - 1];
                    }
                    baseValue = AttributeToBaseType(origValue);
                }

                if (baseValue == null)
                {
                    resultValue = "<null>";
                }
                else
                {
                    resultValue = ValueToString(baseValue, attlen);
                    if (origValue is EntityReference)
                    {
                        var er = (EntityReference)origValue;
                        var erName = "No LogicalName available";
                        if (!string.IsNullOrEmpty(er.LogicalName))
                        {
                            erName = er.LogicalName;
                        }
                        if (!string.IsNullOrEmpty(er.Name))
                        {
                            erName += " " + er.Name;
                        }
                        resultValue += $" ({origType} {erName.Trim()})";
                    }
                    else
                    {
                        resultValue += " (" + origType + ")";
                    }
                }
                var newline = $"\n  {attr.PadRight(attlen)} = {resultValue}";
                attrs.Append(newline);
                if (preimage != null && !attr.Equals(entity.LogicalName + "id") && !attr.Equals("activityid") && preimage.Contains(attr))
                {
                    preValue = AttributeToBaseType(preimage[attr]);
                    if (preValue.Equals(baseValue))
                    {
                        preValue = "<not changed>";
                    }
                    attrs.Append($"\n   {("PRE").PadLeft(attlen)}: {ValueToString(preValue, attlen)}");
                }
            }
            return "  " + attrs.ToString().Trim();
        }

        private static int GetMaxAttributeNameLength(List<string> keys)
        {
            var attlen = 0;
            foreach (string attr in keys)
            {
                attlen = Math.Max(attlen, attr.Length);
            }
            return attlen;
        }

        private static string ValueToString(object value, int baseIndent)
        {
            string resultValue = value.ToString();
            if (resultValue.Contains("\n"))
            {
                var newLinePad = new string(' ', baseIndent + 5);
                resultValue = resultValue.Replace("\n", "\n" + newLinePad);
            }

            return resultValue;
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

        private static string AttributeToString(string entity, object value)
        {
            if (value is AliasedValue)
                return AttributeToString(entity, ((AliasedValue)value).Value);
            else if (value is EntityReference)
                return ((EntityReference)value).LogicalName + " " + ((EntityReference)value).Id.ToString();
            else if (value is OptionSetValue)
                return ((OptionSetValue)value).Value.ToString();
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
