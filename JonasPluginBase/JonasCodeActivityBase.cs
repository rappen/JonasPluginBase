using System;
using System.Activities;
using System.Diagnostics;

namespace Jonas
{
    public abstract class JonasCodeActivityBase : CodeActivity
    {
        protected override void Execute(CodeActivityContext context)
        {
            using (var bag = new JonasPluginBag(context))
            {
                var watch = Stopwatch.StartNew();
                try
                {
                    Execute(bag);
                }
                catch (Exception e)
                {
                    bag.trace("*** Exception ***\n{0}", e);
                    throw;
                }
                finally
                {
                    watch.Stop();
                    bag.trace("Internal execution time: {0} ms", watch.ElapsedMilliseconds);
                }
            }
        }

        public abstract void Execute(JonasPluginBag bag);
    }
    public partial class JonasPluginBag
    {
        CodeActivityContext codeActivityContext { get; set; }
        public T GetCodeActivityParameter<T>(InArgument<T> parameter)
        {
            T result = parameter.Get(codeActivityContext);
            return result;
        }

        public void SetCodeActivityParameter<T>(OutArgument<T> parameter, T value)
        {
            parameter.Set(codeActivityContext, value);
        }
    }
}
