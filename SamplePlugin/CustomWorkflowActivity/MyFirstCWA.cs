using System;
using JonasPluginBase;
using System.Activities;
using Microsoft.Xrm.Sdk.Workflow;

namespace SamplePlugin.CustomWorkflowActivity
{
    public class MyFirstCWA : JonasPluginBase.JonasCodeActivityBase
    {
        public override void Execute(JonasPluginBag bag)
        {
            var ss = SomeString;
            var s = bag.GetParameter(ref ss);


            bag.Trace("SomeString: {0}", s);
            var sn = SomeNumber;
            var n = bag.GetParameter(ref sn);
            bag.Trace("SomeNumber: {0}", n);
        }

        [Input("Some string")]
        [Default("JonasPluginBase helps me a lot.")]
        public InArgument<string> SomeString { get; set; }

        [Input("Some number")]
        [Default("5")]
        public InArgument<int> SomeNumber { get; set; }
    }
}
