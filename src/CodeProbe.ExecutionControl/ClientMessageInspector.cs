using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.ServiceModel;
using System.ServiceModel.Dispatcher;
using System.Text;

namespace CodeProbe.ExecutionControl
{
    public class ClientMessageInspector : IClientMessageInspector
    {
        public void AfterReceiveReply(ref System.ServiceModel.Channels.Message reply, object correlationState)
        {
            if (ExecutionControlManager.Current != null)
            {
                if (reply.Headers.FindHeader(AbstractExecutionContext.EXECUTION_CONTEXT_VALUE, AbstractExecutionContext.EXECUTION_CONTEXT_NS)>=0)
                {
                    Dictionary<string, object> tmp = reply.Headers.GetHeader<Dictionary<string, object>>(AbstractExecutionContext.EXECUTION_CONTEXT_VALUE, AbstractExecutionContext.EXECUTION_CONTEXT_NS);

                    ExecutionControlManager.Current.Merge(tmp);
                }
            }
        }

        public object BeforeSendRequest(ref System.ServiceModel.Channels.Message request, IClientChannel channel)
        {
#if DEBUG
            log4net.LogManager.GetLogger("TEST").Debug($"request={channel.RemoteAddress.Uri.ToString()}");

            log4net.LogManager.GetLogger("TEST").Debug($"ExecutionControlManager.Current_isNull={ExecutionControlManager.Current==null}");
            log4net.LogManager.GetLogger("TEST").Debug($"OperationContext.Current_isNull={OperationContext.Current == null}");
            log4net.LogManager.GetLogger("TEST").Debug($"OperationContextHolder.Current_isNull={OperationContextHolder.Current == null}");
#endif
            if (ExecutionControlManager.Current != null)
            {
                int idx = request.Headers.FindHeader(AbstractExecutionContext.EXECUTION_CONTEXT_VALUE, AbstractExecutionContext.EXECUTION_CONTEXT_NS);
                if (idx >= 0)
                    request.Headers.RemoveAt(idx);

                request.Headers.Add(new MessageHeader<Dictionary<string, object>>(ExecutionControlManager.Current.GetDump()??new Dictionary<string, object>()).GetUntypedHeader(AbstractExecutionContext.EXECUTION_CONTEXT_VALUE, AbstractExecutionContext.EXECUTION_CONTEXT_NS));
            }

            return request.Headers.MessageId;
        }
    }
}
