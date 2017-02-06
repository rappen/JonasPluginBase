using Microsoft.Xrm.Sdk;
using System;

namespace SamplePlugin.Extensions
{
    internal static class ContextExtensions
    {
        internal static bool ContactTriggered(this IExecutionContext context)
        {
            return context.PrimaryEntityName == "contact" && context.InputParameters.Contains("Target");
        }
    }
}
