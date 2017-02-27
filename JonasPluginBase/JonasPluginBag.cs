﻿using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using Microsoft.Xrm.Sdk.Workflow;

namespace JonasPluginBase
{
    /// <summary>
    /// A bag of useful stuff when developing plugins of different types for Microsoft Dynamics 365
    /// </summary>
    public class JonasPluginBag : IDisposable
    {
        #region Public properties

        public JonasServiceProxy Service { get; }

        public JonasTracingService TracingService { get; }

        private IExecutionContext context { get; }

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

        #endregion Public properties

        #region Public methods

        public JonasPluginBag(IServiceProvider serviceProvider)
        {
            TracingService = new JonasTracingService((ITracingService)serviceProvider.GetService(typeof(ITracingService)));
            context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = serviceFactory.CreateOrganizationService(null);
            Service = new JonasServiceProxy(service, this);
            Init();
        }

        public JonasPluginBag(CodeActivityContext executionContext)
        {
            TracingService = new JonasTracingService(executionContext.GetExtension<ITracingService>());
            context = executionContext.GetExtension<IWorkflowContext>();
            var serviceFactory = executionContext.GetExtension<IOrganizationServiceFactory>();
            var service = serviceFactory.CreateOrganizationService(null);
            Service = new JonasServiceProxy(service, this);
            Init();
        }

        public JonasPluginBag(IOrganizationService service, IPluginExecutionContext context, ITracingService trace)
        {
            TracingService = new JonasTracingService(trace);
            Service = new JonasServiceProxy(service, this);
            this.context = context;
            Init();
        }

        public void Trace(string format, params object[] args)
        {
            TracingService.Trace(format, args);
        }

        public string GetOptionsetLabel(string entity, string attribute, int value)
        {
            Trace($"Getting metadata for {entity}.{attribute}");
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
            Trace($"Returning label for value {value}: {result}");
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

        private void Init()
        {
            LogTheContext(context);
            var entity = TargetEntity;
            if (entity != null)
            {
                var attrs = ExtractAttributesFromEntity(entity, PreImage);
                Trace("Incoming {0}:{1}\n", entity.LogicalName, attrs);
            }
        }

        private void LogTheContext(IExecutionContext context)
        {
            if (context == null)
                return;
            var step = context.OwningExtension != null ? !string.IsNullOrEmpty(context.OwningExtension.Name) ? context.OwningExtension.Name : context.OwningExtension.Id.ToString() : "null";
            var stage = context is IPluginExecutionContext ? ((IPluginExecutionContext)context).Stage : 0;
            Trace($@"Context details:
  Step:  {step}
  Msg:   {context.MessageName}
  Stage: {stage}
  Mode:  {context.Mode}
  Depth: {context.Depth}
  Corr:  {context.CorrelationId}
  Type:  {context.PrimaryEntityName}
  Id:    {context.PrimaryEntityId}
  User:  {context.UserId}
");
            var parentcontext = context is IPluginExecutionContext ? ((IPluginExecutionContext)context).ParentContext : null;
            while (parentcontext != null /*&& parentcontext.Stage == 30*/)
            {   // Skip mainoperation
                parentcontext = parentcontext.ParentContext;
            }
            if (parentcontext != null)
            {
                LogTheContext(parentcontext);
            }
        }

        private static string ExtractAttributesFromEntity(Entity entity, Entity preimage)
        {
            string attrs = "";
            List<string> keys = new List<string>(entity.Attributes.Keys);
            keys.Sort();
            int attlen = 0;
            foreach (string attr in keys)
            {
                if (attr != "createdon" & attr != "createdby" && attr != "createdonbehalfby" && attr != "modifiedon" & attr != "modifiedby" && attr != "modifiedonbehalfby")
                {
                    attlen = Math.Max(attlen, attr.Length);
                }
            }
            foreach (string attr in keys)
            {
                if (attr != "createdon" & attr != "createdby" && attr != "createdonbehalfby" && attr != "modifiedon" & attr != "modifiedby" && attr != "modifiedonbehalfby")
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
                        resultValue += " (" + origType + ")";
                        if (origValue is EntityReference)
                        {
                            var er = (EntityReference)origValue;
                            if (!string.IsNullOrEmpty(er.Name))
                            {
                                resultValue += " " + er.Name;
                            }
                            if (!string.IsNullOrEmpty(er.LogicalName))
                            {
                                resultValue += " " + er.LogicalName;
                            }
                            else
                            {
                                resultValue += " No LogicalName available!";
                            }
                        }
                    }
                    var newline = $"\n  {attr.PadRight(attlen)} = {resultValue}";
                    if (attr.Equals(entity.LogicalName + "id"))
                    {
                        attrs = newline + attrs;
                    }
                    else
                    {
                        attrs += newline;
                        if (preimage != null && !attr.Equals(entity.LogicalName + "id") && preimage.Contains(attr))
                        {
                            preValue = AttributeToBaseType(preimage[attr]);
                            if (preValue.Equals(baseValue))
                            {
                                preValue = "<not changed>";
                            }
                            var a = new string(' ', attlen);
                            attrs += $"\n   {("PRE").PadLeft(attlen)}: {ValueToString(preValue, attlen)}";
                        }
                    }
                }
            }
            return attrs;
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

        internal string PrimaryAttribute(string entityName)
        {
            var metabase = (RetrieveEntityResponse)Service.Execute(new RetrieveEntityRequest()
            {
                LogicalName = entityName,
                EntityFilters = EntityFilters.Entity,
                RetrieveAsIfPublished = true
            });
            Trace("Metadata retrieved for {0}", entityName);
            if (metabase != null)
            {
                EntityMetadata meta = metabase.EntityMetadata;
                var result = meta.PrimaryNameAttribute;
                Trace("Primary attribute is: {0}", result);
                return result;
            }
            else
            {
                throw new InvalidPluginExecutionException(
                    "Unable to retrieve metadata/primaryattribute for entity: " + entityName);
            }
        }

        #endregion Private/Internal stuff
    }
}
