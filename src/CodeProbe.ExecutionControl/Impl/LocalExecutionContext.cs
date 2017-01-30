using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeProbe.ExecutionControl.Impl
{
    internal class LocalExecutionContext : AbstractExecutionContext
    {
        public override void Dispose()
        {
            base.Dispose();
            ExecutionControlManager.Current = null;
        }
    }
}
