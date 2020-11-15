using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Linq;
using System.Activities;
using Microsoft.Xrm.Sdk.Workflow;
using System.Diagnostics;
using Rappen.Canary365.Plugin;

namespace Jonas
{
    /// <summary>
    /// A bag of useful stuff when developing plugins of different types for Microsoft Dynamics 365
    /// </summary>
    public partial class JonasPluginBag : IDisposable
    {
        #region Public properties

        public JonasServiceProxy Service { get; }

        public JonasTracingService TracingService { get; }

        public IPluginExecutionContext PluginContext { get { return context as IPluginExecutionContext; } }

        public IWorkflowContext WorkflowContext { get { return context as IWorkflowContext; } }

        public Entity TargetEntity
        {
            get
            {
                if (context != null &&
                    context.InputParameters.Contains("Target") &&
                    context.InputParameters["Target"] is Entity)
                {
                    return (Entity)context.InputParameters["Target"];
                }
                return null;
            }
            set
            {
                context.InputParameters["Target"] = value;
            }
        }

        public Entity PreImage
        {
            get
            {
                if (context != null &&
                    context.PreEntityImages != null &&
                    context.PreEntityImages.Count > 0)
                {
                    return context.PreEntityImages.Values.FirstOrDefault();
                }
                return null;
            }
        }

        public Entity PostImage
        {
            get
            {
                if (context != null &&
                    context.PostEntityImages != null &&
                    context.PostEntityImages.Count > 0)
                {
                    return context.PostEntityImages.Values.FirstOrDefault();
                }
                return null;
            }
        }

        public Entity CompleteEntity
        {
            get
            {
                if (completeEntity == null)
                {
                    completeEntity = TargetEntity.Clone().Merge(PostImage).Merge(PreImage);
                }
                return completeEntity;
            }
        }

        #endregion Public properties

        #region Public methods

        /// <summary>
        /// Constructor to be used from a Microsoft Dynamics CRM (365) plugin
        /// </summary>
        /// <param name="serviceProvider">IServiceProvider passed to the IPlugin.Execute method</param>
        public JonasPluginBag(IServiceProvider serviceProvider)
        {
            TracingService = new JonasTracingService((ITracingService)serviceProvider.GetService(typeof(ITracingService)));
            context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = serviceFactory.CreateOrganizationService(context.InitiatingUserId);
            Service = new JonasServiceProxy(service, this);
        }

        /// <summary>
        /// Constructor to be used from a Microsoft Dynamics CRM (365) custom workflow activity
        /// </summary>
        /// <param name="executionContext">CodeActivityContext passed to the CodeActivity.Execute method</param>
        public JonasPluginBag(CodeActivityContext executionContext)
        {
            TracingService = new JonasTracingService(executionContext.GetExtension<ITracingService>());
            context = executionContext.GetExtension<IWorkflowContext>();
            var serviceFactory = executionContext.GetExtension<IOrganizationServiceFactory>();
            var service = serviceFactory.CreateOrganizationService(context.InitiatingUserId);
            Service = new JonasServiceProxy(service, this);
            codeActivityContext = executionContext;
            Init();
        }

        /// <summary>
        /// Constructor to use when JonasPluginBag is used in custom applications
        /// </summary>
        /// <param name="service">IOrganizationService connected to Microsoft Dynamics CRM (365)</param>
        /// <param name="context"></param>
        /// <param name="trace"></param>
        public JonasPluginBag(IOrganizationService service, IPluginExecutionContext context, ITracingService trace)
        {
            TracingService = new JonasTracingService(trace);
            Service = new JonasServiceProxy(service, this);
            this.context = context;
            Init();
        }

        /// <summary>
        /// Trace method automatically adding timestamp to each traced item
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public void Trace(string format, params object[] args)
        {
            TracingService.Trace(format, args);
        }

        /// <summary>
        /// Traces the text straight to the tracing service, without timestamps, indentation etc.
        /// </summary>
        /// <param name="text"></param>
        public void TraceRaw(string text)
        {
            TracingService.TraceRaw(text);
        }

        /// <summary>
        /// Call this function to start a block in the log.
        /// Log lines will be indented, until next call to TraceBlockEnd.
        /// Block label with be the name of the calling method.
        /// </summary>
        public void TraceBlockStart()
        {
            var label = new StackTrace().GetFrame(1).GetMethod().Name;
            TraceBlockStart(label);
        }

        /// <summary>
        /// Call this function to start a block in the log.
        /// Log lines will be indented, until next call to TraceBlockEnd.
        /// </summary>
        /// <param name="label">Label to set for the block</param>
        public void TraceBlockStart(string label)
        {
            TracingService.BlockBegin(label);
        }

        /// <summary>
        /// Call this to en a block in the log.
        /// </summary>
        public void TraceBlockEnd()
        {
            TracingService.BlockEnd();
        }

        /// <summary>
        /// Get label for specified optionset attribute and value
        /// </summary>
        /// <param name="entity">Entity where the attribute is used</param>
        /// <param name="attribute">Attribute name</param>
        /// <param name="value">Value of the optionset for which to return label</param>
        /// <returns></returns>
        public string GetOptionsetLabel(string entity, string attribute, int value)
        {
            trace($"Getting metadata for {entity}.{attribute}");
            var req = new RetrieveAttributeRequest
            {
                EntityLogicalName = entity,
                LogicalName = attribute,
                RetrieveAsIfPublished = true
            };
            var resp = (RetrieveAttributeResponse)Service.Execute(req);
            var plmeta = (PicklistAttributeMetadata)resp.AttributeMetadata;
            if (plmeta == null)
            {
                throw new InvalidPluginExecutionException($"{entity}.{attribute} does not appear to be an optionset");
            }
            var result = plmeta.OptionSet.Options.FirstOrDefault(o => o.Value == value)?.Label?.UserLocalizedLabel?.Label;
            trace($"Returning label for value {value}: {result}");
            return result;
        }

        public void Dispose()
        {
            if (TracingService != null)
            {
                TracingService.Dispose();
            }
        }

        #endregion Public methods

        #region Private/internal stuff

        private readonly IExecutionContext context;

        private Entity completeEntity;

        private void Init()
        {
            LogTheContext(context);
            var entity = TargetEntity;
            if (entity != null)
            {
                var attrs = entity.ExtractAttributes(PreImage);
                trace("Incoming {0}\n{1}\n", entity.LogicalName, attrs);
            }
        }

        private void LogTheContext(IExecutionContext context)
        {
            if (context == null)
                return;
            var step = context.OwningExtension != null ? !string.IsNullOrEmpty(context.OwningExtension.Name) ? context.OwningExtension.Name : context.OwningExtension.Id.ToString() : "null";
            var stage = context is IPluginExecutionContext ? ((IPluginExecutionContext)context).Stage : 0;
            trace($@"Context details:
  Step:  {step}
  Msg:   {context.MessageName}
  Stage: {stage}
  Mode:  {context.Mode}
  Depth: {context.Depth}
  Corr:  {context.CorrelationId}
  Type:  {context.PrimaryEntityName}
  Id:    {context.PrimaryEntityId}
  User:  {context.UserId}
  IUser: {context.InitiatingUserId}
");
            if (TracingService.Verbose)
            {
                var parentcontext = context is IPluginExecutionContext ? ((IPluginExecutionContext)context).ParentContext : null;
                if (parentcontext != null)
                {
                    LogTheContext(parentcontext);
                }
            }
        }

        internal string PrimaryAttribute(string entityName)
        {
            var metabase = (RetrieveEntityResponse)Service.Execute(new RetrieveEntityRequest()
            {
                LogicalName = entityName,
                EntityFilters = EntityFilters.Entity,
                RetrieveAsIfPublished = true
            });
            trace("Metadata retrieved for {0}", entityName);
            if (metabase != null)
            {
                EntityMetadata meta = metabase.EntityMetadata;
                var result = meta.PrimaryNameAttribute;
                trace("Primary attribute is: {0}", result);
                return result;
            }
            else
            {
                throw new InvalidPluginExecutionException(
                    "Unable to retrieve metadata/primaryattribute for entity: " + entityName);
            }
        }

        internal void trace(string format, params object[] args)
        {
            Trace("[JPB] " + format, args);
        }

        #endregion Private/Internal stuff
    }
}
