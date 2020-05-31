using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Extensions;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jonas
{
    public static class QueryExtensions
    {
        public static ConditionExpression GetCondition(this QueryExpression query, string attribute, ConditionOperator? oper, ITracingService trace = null)
        {
            var result = query.Criteria?.GetCondition(attribute, oper, trace);
            if (result != null)
            {
                return result;
            }
            foreach (var link in query.LinkEntities)
            {
                var linkresult = link.GetCondition(attribute, oper, trace);
                if (linkresult != null)
                {
                    return linkresult;
                }
            }
            return null;
        }

        public static ConditionExpression GetCondition(this LinkEntity linkentity, string attribute, ConditionOperator? oper, ITracingService trace = null)
        {
            var result = linkentity.LinkCriteria?.GetCondition(attribute, oper, trace);
            if (result != null)
            {
                return result;
            }
            foreach (var link in linkentity.LinkEntities)
            {
                var linkresult = link.GetCondition(attribute, oper, trace);
                if (linkresult != null)
                {
                    return linkresult;
                }
            }
            return null;
        }

        public static ConditionExpression GetCondition(this FilterExpression filter, string attribute, ConditionOperator? oper, ITracingService trace = null)
        {
            var result = filter?.Conditions?.FirstOrDefault(c => c.AttributeName.Equals(attribute) && (oper == null || oper == c.Operator));
            if (result != null)
            {
                trace?.Trace($"Found condition: {result.Stringify()}");
                return result;
            }
            foreach (var subfilter in filter?.Filters)
            {
                if (subfilter.GetCondition(attribute, oper, trace) is ConditionExpression subresult)
                {
                    return subresult;
                }
            }
            return null;
        }

        public static bool RemoveCondition(this QueryExpression query, ConditionExpression condition, ITracingService trace = null)
        {
            if (query.Criteria.RemoveCondition(condition, trace))
            {
                return true;
            }
            var result = false;
            foreach (var link in query.LinkEntities)
            {
                if (link.RemoveCondition(condition, trace))
                {
                    result = true;
                }
            }
            query.LinkEntities.RemoveEmptyLinkEntities(trace);
            return result;
        }

        public static bool RemoveCondition(this LinkEntity linkentity, ConditionExpression condition, ITracingService trace = null)
        {
            if (linkentity.LinkCriteria.RemoveCondition(condition, trace))
            {
                return true;
            }
            var result = false;
            foreach (var sublink in linkentity.LinkEntities)
            {
                if (sublink.RemoveCondition(condition, trace))
                {
                    result = true;
                }
            }
            linkentity.LinkEntities.RemoveEmptyLinkEntities(trace);
            return result;
        }

        public static bool RemoveCondition(this FilterExpression filter, ConditionExpression condition, ITracingService trace = null)
        {
            if (filter?.Conditions?.Contains(condition) == true)
            {
                trace?.Trace($"Removing condition: {condition.Stringify()}");
                filter.Conditions.Remove(condition);
                return true;
            }
            var result = false;
            foreach (var subfilter in filter.Filters)
            {
                if (subfilter.RemoveCondition(condition, trace))
                {
                    result = true;
                }
            }
            filter.Filters.RemoveEmptyFilters(trace);
            return result;
        }

        public static void RemoveEmptyLinkEntities(this DataCollection<LinkEntity> linkEntities, ITracingService trace = null)
        {
            var i = 0;
            while (i < linkEntities.Count)
            {
                var sublink = linkEntities[i];
                if (sublink.LinkEntities.Count == 0 && sublink.LinkCriteria.Conditions.Count == 0 && sublink.LinkCriteria.Filters.Count == 0)
                {
                    trace?.Trace($"Removing link-entity: {sublink.Stringify()}");
                    linkEntities.Remove(sublink);
                }
                else
                {
                    i++;
                }
            }
        }

        public static void RemoveEmptyFilters(this DataCollection<FilterExpression> filters, ITracingService trace = null)
        {
            var i = 0;
            while (i < filters.Count)
            {
                var subfilter = filters[i];
                if (subfilter.Conditions.Count == 0 && subfilter.Filters.Count == 0)
                {
                    trace?.Trace($"Removing empty filter");
                    filters.Remove(subfilter);
                }
                else
                {
                    i++;
                }
            }
        }

        public static string Stringify(this LinkEntity linkEntity)
        {
            return $"From {linkEntity.LinkFromEntityName}.{linkEntity.LinkFromAttributeName} To {linkEntity.LinkToEntityName}.{linkEntity.LinkToAttributeName} Alias {linkEntity.EntityAlias}";
        }

        public static string Stringify(this ConditionExpression condition)
        {
            return $"{condition.AttributeName} {condition.Operator} {string.Join(", ", condition.Values.Select(v => v.ToString()))}".Trim();
        }
    }
}
