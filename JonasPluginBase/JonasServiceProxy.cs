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
            bag.Trace.Trace($"Associate({entityName}, {entityId}, {relationship.SchemaName}, {relatedEntities.Count})");
            var watch = Stopwatch.StartNew();
            service.Associate(entityName, entityId, relationship, relatedEntities);
            watch.Stop();
            bag.Trace.Trace($"Associated in: {watch.ElapsedMilliseconds} ms");
        }

        public Guid Create(Entity entity)
        {
            bag.Trace.Trace($"Create({entity.LogicalName}) {entity.Id} ({entity.Attributes.Count} attributes)");
            var watch = Stopwatch.StartNew();
            var result = service.Create(entity);
            watch.Stop();
            bag.Trace.Trace($"Created in: {watch.ElapsedMilliseconds} ms");
            return result;
        }

        public void Delete(string entityName, Guid id)
        {
            bag.Trace.Trace($"Delete({entityName}, {id})");
            var watch = Stopwatch.StartNew();
            service.Delete(entityName, id);
            watch.Stop();
            bag.Trace.Trace($"Deleted in: {watch.ElapsedMilliseconds} ms");
        }

        public void Disassociate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities)
        {
            bag.Trace.Trace($"Disassociate({entityName}, {entityId}, {relationship.SchemaName}, {relatedEntities.Count})");
            var watch = Stopwatch.StartNew();
            service.Disassociate(entityName, entityId, relationship, relatedEntities);
            watch.Stop();
            bag.Trace.Trace($"Disassociated in: {watch.ElapsedMilliseconds} ms");
        }

        public OrganizationResponse Execute(OrganizationRequest request)
        {
            bag.Trace.Trace($"Execute({request.RequestName})");
            var watch = Stopwatch.StartNew();
            var result = service.Execute(request);
            watch.Stop();
            bag.Trace.Trace($"Executed in: {watch.ElapsedMilliseconds} ms");
            return result;
        }

        public Entity Retrieve(string entityName, Guid id, ColumnSet columnSet)
        {
            bag.Trace.Trace($"Retrieve({entityName}, {id}, {columnSet.Columns.Count})");
            var watch = Stopwatch.StartNew();
            var result = service.Retrieve(entityName, id, columnSet);
            watch.Stop();
            bag.Trace.Trace($"Retrieved in: {watch.ElapsedMilliseconds} ms");
            return result;
        }

        public EntityCollection RetrieveMultiple(QueryBase query)
        {
            bag.Trace.Trace("RetrieveMultiple({0})", query is QueryExpression ? ((QueryExpression)query).EntityName : query is QueryByAttribute ? ((QueryByAttribute)query).EntityName : query is FetchExpression ? "fetchxml" : "unkstartn");
            var watch = Stopwatch.StartNew();
            var result = service.RetrieveMultiple(query);
            watch.Stop();
            bag.Trace.Trace($"Retrieved {result.Entities.Count} records in: {watch.ElapsedMilliseconds} ms");
            return result;
        }

        public void Update(Entity entity)
        {
            bag.Trace.Trace($"Update({entity.LogicalName}) {entity.Id} ({entity.Attributes.Count} attributes)");
            var watch = Stopwatch.StartNew();
            service.Update(entity);
            watch.Stop();
            bag.Trace.Trace($"Updated in: {watch.ElapsedMilliseconds} ms");
        }
    }
}