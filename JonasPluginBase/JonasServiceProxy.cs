using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System.Diagnostics;
using Microsoft.Crm.Sdk.Messages;
using System.Linq;

namespace Jonas
{
    public class JonasServiceProxy : IOrganizationService
    {
        private readonly IOrganizationService service;
        private readonly JonasPluginBag bag;

        public JonasServiceProxy(IOrganizationService Service, JonasPluginBag bag)
        {
            service = Service;
            this.bag = bag;
        }

        public void Associate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities)
        {
            bag.trace($"Associate({entityName}, {entityId}, {relationship.SchemaName}, {relatedEntities.Count})");
            if (bag.TracingService.Verbose)
            {
                bag.trace("Associated record(s):{0}", relatedEntities.Select(r => $"\n  {r.LogicalName} {r.Id} {r.Name}"));
            }
            var watch = Stopwatch.StartNew();
            service.Associate(entityName, entityId, relationship, relatedEntities);
            watch.Stop();
            bag.trace($"Associated in: {watch.ElapsedMilliseconds} ms");
        }

        public Guid Create(Entity entity)
        {
            bag.trace($"Create({entity.LogicalName}) {entity.Id} ({entity.Attributes.Count} attributes)");
            if (bag.TracingService.Verbose)
            {
                bag.trace("\n{0}", entity.ExtractAttributes(null));
            }
            var watch = Stopwatch.StartNew();
            var result = service.Create(entity);
            watch.Stop();
            bag.trace($"Created in: {watch.ElapsedMilliseconds} ms");
            return result;
        }

        public void Delete(string entityName, Guid id)
        {
            bag.trace($"Delete({entityName}, {id})");
            var watch = Stopwatch.StartNew();
            service.Delete(entityName, id);
            watch.Stop();
            bag.trace($"Deleted in: {watch.ElapsedMilliseconds} ms");
        }

        public void Disassociate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities)
        {
            bag.trace($"Disassociate({entityName}, {entityId}, {relationship.SchemaName}, {relatedEntities.Count})");
            if (bag.TracingService.Verbose)
            {
                bag.trace("Disassociated record(s):{0}", relatedEntities.Select(r => $"\n  {r.LogicalName} {r.Id} {r.Name}"));
            }
            var watch = Stopwatch.StartNew();
            service.Disassociate(entityName, entityId, relationship, relatedEntities);
            watch.Stop();
            bag.trace($"Disassociated in: {watch.ElapsedMilliseconds} ms");
        }

        public OrganizationResponse Execute(OrganizationRequest request)
        {
            bag.trace($"Execute({request.RequestName})");
            if (bag.TracingService.Verbose && request is ExecuteFetchRequest)
            {
                bag.trace("FetchXML: {0}", ((ExecuteFetchRequest)request).FetchXml);
            }
            var watch = Stopwatch.StartNew();
            var result = service.Execute(request);
            watch.Stop();
            bag.trace($"Executed in: {watch.ElapsedMilliseconds} ms");
            return result;
        }

        public Entity Retrieve(string entityName, Guid id, ColumnSet columnSet)
        {
            bag.trace($"Retrieve({entityName}, {id}, {columnSet.Columns.Count})");
            if (bag.TracingService.Verbose)
            {
                bag.trace("Columns:{0}", columnSet.Columns.Select(c => "\n  " + c));
            }
            var watch = Stopwatch.StartNew();
            var result = service.Retrieve(entityName, id, columnSet);
            watch.Stop();
            bag.trace($"Retrieved in: {watch.ElapsedMilliseconds} ms");
            if (bag.TracingService.Verbose)
            {
                bag.trace("Retrieved\n{0}", result.ExtractAttributes(null));
            }
            return result;
        }

        public EntityCollection RetrieveMultiple(QueryBase query)
        {
            bag.trace("RetrieveMultiple({0})", query is QueryExpression ? ((QueryExpression)query).EntityName : query is QueryByAttribute ? ((QueryByAttribute)query).EntityName : query is FetchExpression ? "fetchxml" : "unkstartn");
            if (bag.TracingService.Verbose)
            {
                var fetch = ((QueryExpressionToFetchXmlResponse)bag.Service.Execute(new QueryExpressionToFetchXmlRequest() { Query = query })).FetchXml;
                bag.trace("Query: {0}", fetch);
            }
            var watch = Stopwatch.StartNew();
            var result = service.RetrieveMultiple(query);
            watch.Stop();
            bag.trace($"Retrieved {result.Entities.Count} records in: {watch.ElapsedMilliseconds} ms");
            return result;
        }

        public void Update(Entity entity)
        {
            bag.trace($"Update({entity.LogicalName}) {entity.Id} ({entity.Attributes.Count} attributes)");
            if (bag.TracingService.Verbose)
            {
                bag.trace("\n{0}", entity.ExtractAttributes(null));
            }
            var watch = Stopwatch.StartNew();
            service.Update(entity);
            watch.Stop();
            bag.trace($"Updated in: {watch.ElapsedMilliseconds} ms");
        }
    }
}