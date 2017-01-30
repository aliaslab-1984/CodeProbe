using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Configuration;
using System.Text;

namespace CodeProbe.ExecutionControl
{
    public class ExecutionContextSpanBehaviorExtensionElement : BehaviorExtensionElement
    {
        public override Type BehaviorType
        {
            get { return typeof(ExecutionContextSpanBehavior); }
        }

        protected override object CreateBehavior()
        {
            return new ExecutionContextSpanBehavior();
        }
    }
}
