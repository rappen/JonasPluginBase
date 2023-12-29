using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System.Diagnostics;
using Microsoft.Crm.Sdk.Messages;
using System.Linq;

namespace Jonas
{
    public class JonasEndpointNotificationService : IServiceEndpointNotificationService
    {
        private readonly IServiceEndpointNotificationService EndpointNotificationService;
        private readonly JonasPluginBag bag;

        public JonasEndpointNotificationService(IServiceEndpointNotificationService endpointNotificationService, JonasPluginBag bag)
        {
            EndpointNotificationService = endpointNotificationService;
            this.bag = bag;
        }
        public string Execute(EntityReference serviceEndpoint, IExecutionContext context)
        {
            bag.trace($"Execute({serviceEndpoint.LogicalName}, {serviceEndpoint.Id})");
            var watch = Stopwatch.StartNew();
            string response = EndpointNotificationService.Execute(serviceEndpoint, bag.PluginContext);
            if (!String.IsNullOrEmpty(response))
            {
                bag.trace($"Response : {response}");
            }
            watch.Stop();
            bag.trace($"Execute in: {watch.ElapsedMilliseconds} ms");
            return response;
        }
    }
}