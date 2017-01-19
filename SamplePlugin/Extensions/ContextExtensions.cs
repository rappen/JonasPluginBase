using Microsoft.Xrm.Sdk;
using System;

namespace SamplePlugin.Extensions
{
    internal static class ContextExtensions
    {
        internal static bool ContactTriggered(this IPluginExecutionContext context)
        {
            return context.PrimaryEntityName == "contact" && context.InputParameters.Contains("Target");
        }
    }
}
