using Microsoft.Xrm.Sdk;
using System;

namespace Jonas
{
    public static class JonasContextExtensions
    {
        public static T GetInputParameterForgiving<T>(this IPluginExecutionContext context, string name)
        {
            try
            {
                if (!context.InputParameters.TryGetValue(name, out object obj))
                {
                    throw new ArgumentNullException(name);
                }
                if (!(obj is T result))
                {
                    throw new ArgumentException($"Expected {typeof(T)}, got {obj.GetType()}", name);
                }
                return result;
            }
            catch (ArgumentNullException ex)
            {
                context.OutputParameters.AddOrUpdateIfNotNull("ErrorCode", -1);
                context.OutputParameters.AddOrUpdateIfNotNull("ErrorMessage", ex.Message);
            }
            catch (ArgumentException ex)
            {
                context.OutputParameters.AddOrUpdateIfNotNull("ErrorCode", -2);
                context.OutputParameters.AddOrUpdateIfNotNull("ErrorMessage", ex.Message);
            }
            return default(T);
        }

        public static T GetInputParameter<T>(this IPluginExecutionContext context, string name)
        {
            if (!context.InputParameters.TryGetValue(name, out var parameter))
            {
                throw new InvalidPluginExecutionException(OperationStatus.Failed, $"Missing input parameter {name}");
            }

            if (!(parameter is T result))
            {
                throw new InvalidPluginExecutionException(OperationStatus.Failed, $"Inpur parameter {name} is {parameter.GetType()} expected {typeof(T)}");
            }
            return result;
        }
    }
}
