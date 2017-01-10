using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using System.Linq;

namespace JonasPluginBase
{
    public class JonasServiceProxy : IOrganizationService
    {
        private IOrganizationService service;
        private JonasPluginBag bag;

        public JonasServiceProxy(IOrganizationService Service, JonasPluginBag bag)
        {
            service = Service;
            this.bag = bag;
        }

        public void Associate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities)
        {
            bag.Trace($"Associate({entityName}, {entityId}, {relationship.SchemaName}, {relatedEntities.Count})");
            var start = DateTime.Now;
            service.Associate(entityName, entityId, relationship, relatedEntities);
            bag.Trace($"Associated in: {DateTime.Now - start}");
        }

        public Guid Create(Entity entity)
        {
            bag.Trace($"Create({entity.LogicalName}) {entity.Id} ({entity.Attributes.Count} attributes)");
            var start = DateTime.Now;
            var result = service.Create(entity);
            bag.Trace($"Created in: {DateTime.Now - start}");
            return result;
        }

        public void Delete(string entityName, Guid id)
        {
            bag.Trace($"Delete({entityName}, {id})");
            var start = DateTime.Now;
            service.Delete(entityName, id);
            bag.Trace($"Deleted in: {DateTime.Now - start}");
        }

        public void Disassociate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities)
        {
            bag.Trace($"Disassociate({entityName}, {entityId}, {relationship.SchemaName}, {relatedEntities.Count})");
            var start = DateTime.Now;
            service.Disassociate(entityName, entityId, relationship, relatedEntities);
            bag.Trace($"Disassociated in: {DateTime.Now - start}");
        }

        public OrganizationResponse Execute(OrganizationRequest request)
        {
            bag.Trace($"Execute({request.RequestName})");
            var start = DateTime.Now;
            var result = service.Execute(request);
            bag.Trace($"Executed in: {DateTime.Now - start}");
            return result;
        }

        public Entity Retrieve(string entityName, Guid id, ColumnSet columnSet)
        {
            bag.Trace($"Retrieve({entityName}, {id}, {columnSet.Columns.Count})");
            var start = DateTime.Now;
            var result = service.Retrieve(entityName, id, columnSet);
            bag.Trace($"Retrieved in: {DateTime.Now - start}");
            return result;
        }

        public EntityCollection RetrieveMultiple(QueryBase query)
        {
            bag.Trace("RetrieveMultiple({0})", query is QueryExpression ? ((QueryExpression)query).EntityName : query is QueryByAttribute ? ((QueryByAttribute)query).EntityName : query is FetchExpression ? "fetchxml" : "unkstartn");
            var start = DateTime.Now;
            var result = service.RetrieveMultiple(query);
            bag.Trace($"Retrieved {result.Entities.Count} records in: {DateTime.Now - start}");
            return result;
        }

        public void Update(Entity entity)
        {
            bag.Trace($"Update({entity.LogicalName}) {entity.Id} ({entity.Attributes.Count} attributes)");
            var start = DateTime.Now;
            service.Update(entity);
            bag.Trace($"Updated in: {DateTime.Now - start}");
        }

        public string GetOptionsetLabel(string entity, string attribute, int value)
        {
            bag.Trace($"Getting metadata for {entity}.{attribute}");
            var req = new RetrieveAttributeRequest
            {
                EntityLogicalName = entity,
                LogicalName = attribute,
                RetrieveAsIfPublished = true
            };
            var resp = (RetrieveAttributeResponse)service.Execute(req);
            var plmeta = (PicklistAttributeMetadata)resp.AttributeMetadata;
            if (plmeta == null)
            {
                throw new InvalidPluginExecutionException($"{entity}.{attribute} does not appear to be an optionset");
            }
            var result = plmeta.OptionSet.Options.FirstOrDefault(o => o.Value == value)?.Label?.UserLocalizedLabel?.Label;
            bag.Trace($"Returning label for value {value}: {result}");
            return result;
        }
    }
}