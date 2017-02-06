using System;
using System.Activities;
using System.Diagnostics;

namespace JonasPluginBase
{
    public abstract class JonasCodeActivityBase : CodeActivity
    {
        private CodeActivityContext codeActivityContext;

        protected override void Execute(CodeActivityContext context)
        {
            codeActivityContext = context;

            using (var bag = new JonasPluginBag(context))
            {
                var watch = Stopwatch.StartNew();
                try
                {
                    Execute(bag);
                }
                catch (Exception e)
                {
                    bag.Trace("*** Exception ***\n{0}", e);
                    throw;
                }
                finally
                {
                    watch.Stop();
                    bag.Trace("Internal execution time: {0} ms", watch.ElapsedMilliseconds);
                }
            }
        }

        public abstract void Execute(JonasPluginBag bag);

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
