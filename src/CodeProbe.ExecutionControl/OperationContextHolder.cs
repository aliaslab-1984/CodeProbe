using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace CodeProbe.ExecutionControl
{
    public class OperationContextHolder : IExtension<OperationContext>, IDisposable
    {
        protected IExecutionContext _ctx;
        internal void SetExecutionContext(IExecutionContext ctx)
        {
            _ctx = ctx;
        }

        public static OperationContextHolder Current
        {
            get { return OperationContext.Current?.Extensions?.Find<OperationContextHolder>(); }
        }

        public void Attach(OperationContext owner)
        {}

        public void Detach(OperationContext owner)
        {}

        public void Dispose()
        {
            _ctx?.Dispose();
            OperationContext.Current.Extensions.Remove(OperationContextHolder.Current);
        }

        public Dictionary<string, object> Context { get; set; }
    }
}
