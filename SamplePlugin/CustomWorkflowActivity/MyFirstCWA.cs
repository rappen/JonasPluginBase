using JonasPluginBase;
using System.Activities;
using Microsoft.Xrm.Sdk.Workflow;
using Microsoft.Xrm.Sdk;
using System.Linq;

namespace SamplePlugin.CustomWorkflowActivity
{
    public class MyFirstCWA : JonasPluginBase.JonasCodeActivityBase
    {
        public override void Execute(JonasPluginBag bag)
        {
            var s = GetCodeActivityParameter(SomeString);
            bag.Trace("SomeString: {0}", s);
            var n = GetCodeActivityParameter(SomeNumber);
            bag.Trace("SomeNumber: {0}", n);

            bag.Trace("CompleteEntity:\n{0}", bag.CompleteEntity.ExtractAttributes(null));

            SetCodeActivityParameter(ResultingUser, new EntityReference("systemuser", bag.WorkflowContext.UserId));
        }

        [Input("Some string")]
        [Default("JonasPluginBase helps me a lot.")]
        public InArgument<string> SomeString { get; set; }

        [Input("Some number")]
        [Default("5")]
        public InArgument<int> SomeNumber { get; set; }

        [Output("Resulting user")]
        [ReferenceTarget("systemuser")]
        public OutArgument<EntityReference> ResultingUser { get; set; }
    }
}
