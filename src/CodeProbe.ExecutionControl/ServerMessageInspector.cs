using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Dispatcher;
using System.Text;

namespace CodeProbe.ExecutionControl
{
    public class ServerMessageInspector : IDispatchMessageInspector
    {
        public object AfterReceiveRequest(ref System.ServiceModel.Channels.Message request, IClientChannel channel, InstanceContext instanceContext)
        {
#if DEBUG
            log4net.LogManager.GetLogger("TEST-S").Debug($"PASSING");
#endif
            OperationContextHolder holder = new OperationContextHolder() { Context = new Dictionary<string, object>() };
            OperationContext.Current.Extensions.Add(holder);

            if (request.Headers.FindHeader(AbstractExecutionContext.EXECUTION_CONTEXT_VALUE, AbstractExecutionContext.EXECUTION_CONTEXT_NS) >= 0)
            {
                holder.Context = request.Headers.GetHeader<Dictionary<string, object>>(AbstractExecutionContext.EXECUTION_CONTEXT_VALUE, AbstractExecutionContext.EXECUTION_CONTEXT_NS);
            }
#if DEBUG
            log4net.LogManager.GetLogger("TEST-S").Debug($"ExecutionControlManager.Current_isNull={ExecutionControlManager.Current == null}");
            log4net.LogManager.GetLogger("TEST-S").Debug($"OperationContext.Current_isNull={OperationContext.Current == null}");
            log4net.LogManager.GetLogger("TEST-S").Debug($"OperationContextHolder.Current_isNull={OperationContextHolder.Current == null}");
#endif

            return request.Headers.MessageId;
        }

        public void BeforeSendReply(ref System.ServiceModel.Channels.Message reply, object correlationState)
        {
            if (OperationContextHolder.Current != null)
            {
                int idx = reply.Headers.FindHeader(AbstractExecutionContext.EXECUTION_CONTEXT_VALUE, AbstractExecutionContext.EXECUTION_CONTEXT_NS);
                if (idx >= 0)
                    reply.Headers.RemoveAt(idx);

                reply.Headers.Add(new MessageHeader<Dictionary<string, object>>(OperationContextHolder.Current?.Context??new Dictionary<string, object>()).GetUntypedHeader(AbstractExecutionContext.EXECUTION_CONTEXT_VALUE, AbstractExecutionContext.EXECUTION_CONTEXT_NS));

                OperationContextHolder.Current?.Dispose();
            }
        }
    }
}
