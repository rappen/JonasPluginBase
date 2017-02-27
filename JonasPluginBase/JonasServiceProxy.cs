using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System.Diagnostics;

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
            var watch = Stopwatch.StartNew();
            service.Associate(entityName, entityId, relationship, relatedEntities);
            watch.Stop();
            bag.Trace($"Associated in: {watch.ElapsedMilliseconds} ms");
        }

        public Guid Create(Entity entity)
        {
            bag.Trace($"Create({entity.LogicalName}) {entity.Id} ({entity.Attributes.Count} attributes)");
            var watch = Stopwatch.StartNew();
            var result = service.Create(entity);
            watch.Stop();
            bag.Trace($"Created in: {watch.ElapsedMilliseconds} ms");
            return result;
        }

        public void Delete(string entityName, Guid id)
        {
            bag.Trace($"Delete({entityName}, {id})");
            var watch = Stopwatch.StartNew();
            service.Delete(entityName, id);
            watch.Stop();
            bag.Trace($"Deleted in: {watch.ElapsedMilliseconds} ms");
        }

        public void Disassociate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities)
        {
            bag.Trace($"Disassociate({entityName}, {entityId}, {relationship.SchemaName}, {relatedEntities.Count})");
            var watch = Stopwatch.StartNew();
            service.Disassociate(entityName, entityId, relationship, relatedEntities);
            watch.Stop();
            bag.Trace($"Disassociated in: {watch.ElapsedMilliseconds} ms");
        }

        public OrganizationResponse Execute(OrganizationRequest request)
        {
            bag.Trace($"Execute({request.RequestName})");
            var watch = Stopwatch.StartNew();
            var result = service.Execute(request);
            watch.Stop();
            bag.Trace($"Executed in: {watch.ElapsedMilliseconds} ms");
            return result;
        }

        public Entity Retrieve(string entityName, Guid id, ColumnSet columnSet)
        {
            bag.Trace($"Retrieve({entityName}, {id}, {columnSet.Columns.Count})");
            var watch = Stopwatch.StartNew();
            var result = service.Retrieve(entityName, id, columnSet);
            watch.Stop();
            bag.Trace($"Retrieved in: {watch.ElapsedMilliseconds} ms");
            return result;
        }

        public EntityCollection RetrieveMultiple(QueryBase query)
        {
            bag.Trace("RetrieveMultiple({0})", query is QueryExpression ? ((QueryExpression)query).EntityName : query is QueryByAttribute ? ((QueryByAttribute)query).EntityName : query is FetchExpression ? "fetchxml" : "unkstartn");
            var watch = Stopwatch.StartNew();
            var result = service.RetrieveMultiple(query);
            watch.Stop();
            bag.Trace($"Retrieved {result.Entities.Count} records in: {watch.ElapsedMilliseconds} ms");
            return result;
        }

        public void Update(Entity entity)
        {
            bag.Trace($"Update({entity.LogicalName}) {entity.Id} ({entity.Attributes.Count} attributes)");
            var watch = Stopwatch.StartNew();
            service.Update(entity);
            watch.Stop();
            bag.Trace($"Updated in: {watch.ElapsedMilliseconds} ms");
        }
    }
}