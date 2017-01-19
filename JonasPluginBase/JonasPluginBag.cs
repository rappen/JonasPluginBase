using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JonasPluginBase
{
    public class JonasPluginBag : IDisposable
    {
        #region Private properties
        private IServiceProvider provider;

        private ITracingService trace { get; }
        #endregion Private properties

        #region Public properties

        public JonasServiceProxy Service { get; }

        public IPluginExecutionContext Context { get; }

        public Entity TargetEntity
        {
            get
            {
                if (Context != null &&
                    Context.InputParameters.Contains("Target") &&
                    Context.InputParameters["Target"] is Entity)
                {
                    return (Entity)Context.InputParameters["Target"];
                }
                return null;
            }
            set
            {
                Context.InputParameters["Target"] = value;
            }
        }

        public Entity PreImage
        {
            get
            {
                if (Context != null &&
                    Context.PreEntityImages != null &&
                    Context.PreEntityImages.Count > 0)
                {
                    return Context.PreEntityImages.Values.FirstOrDefault();
                }
                return null;
            }
        }

        public Entity PostImage
        {
            get
            {
                if (Context != null &&
                    Context.PostEntityImages != null &&
                    Context.PostEntityImages.Count > 0)
                {
                    return Context.PostEntityImages.Values.FirstOrDefault();
                }
                return null;
            }
        }

        #endregion Public properties

        #region Public methods

        public JonasPluginBag(IServiceProvider serviceProvider)
        {
            provider = serviceProvider;
            trace = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            Context = (IPluginExecutionContext)provider.GetService(typeof(IPluginExecutionContext));
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = serviceFactory.CreateOrganizationService(null);
            Service = new JonasServiceProxy(service, this);
            Init();
        }

        public JonasPluginBag(IOrganizationService service, IPluginExecutionContext context, ITracingService trace)
        {
            this.Service = new JonasServiceProxy(service, this);
            this.Context = context;
            this.trace = trace;
            Init();
        }

        public void Trace(string format, params object[] args)
        {
            var s = string.Format(format, args);
            trace.Trace(DateTime.Now.ToString("HH:mm:ss.fff") + "\t" + s);
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
            Trace("*** Exit");
        }

        #endregion Public methods

        #region Private/internal stuff

        private void Init()
        {
            Trace("*** Enter");
            var entity = TargetEntity;
            if (entity != null)
            {
                var attrs = ExtractAttributesFromEntity(entity);
                Trace("Incoming {0}:{1}", entity.LogicalName, attrs);
            }
        }

        private static string ExtractAttributesFromEntity(Entity entity)
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
            var newLinePad = "".PadLeft(attlen + 7);
            foreach (string attr in keys)
            {
                if (attr != "createdon" & attr != "createdby" && attr != "createdonbehalfby" && attr != "modifiedon" & attr != "modifiedby" && attr != "modifiedonbehalfby")
                {
                    object origValue, baseValue = null;
                    string origType = string.Empty, baseType = string.Empty;

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
                        baseType = "<null>";
                    }
                    else
                    {
                        baseType = baseValue.ToString();
                        if (baseType.Contains("\n"))
                        {
                            baseType = "\n" + baseType;
                            baseType = baseType.Replace("\n", "\n" + newLinePad);
                        }
                        baseType += " (" + origType + ")";
                        if (origValue is EntityReference)
                        {
                            var er = (EntityReference)origValue;
                            if (!string.IsNullOrEmpty(er.LogicalName))
                            {
                                baseType += " " + er.LogicalName;
                            }
                            else
                            {
                                baseType += " No LogicalName available!";
                            }
                            if (!string.IsNullOrEmpty(er.Name))
                            {
                                baseType += " " + er.Name;
                            }
                        }
                    }
                    var attpad = new StringBuilder(attr);
                    while (attpad.Length < attlen)
                    {
                        attpad.Append(" ");
                    }
                    attrs = $"{attrs}\n  {attpad} = {baseType}";
                }
            }
            return attrs;
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
