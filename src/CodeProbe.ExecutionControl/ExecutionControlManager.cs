using CodeProbe.ExecutionControl.Impl;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Text;

namespace CodeProbe.ExecutionControl
{
    public static class ExecutionControlManager
    {
        #region nested types

        private class ScopeCountContextDecorator : IExecutionContext, IDisposable
        {
            private IExecutionContext _ctx;
            private int _scopeCount = 0;
            private object _lk = new object();

            public ScopeCountContextDecorator(IExecutionContext ctx)
            {
                _ctx = ctx;
            }

            public IExecutionContext Current
            {
                get { return _ctx; }
            }

            public IDisposable AddScope()
            {
                lock (_lk)
                {
                    _scopeCount++;
                    return this;
                }
            }

            public void Dispose()
            {
                lock (_lk)
                {
                    if (_scopeCount-- == 0)
                        _ctx.Dispose();
                }
            }

            public List<string> GetCtxKeys()
            {
                return _ctx.GetCtxKeys();
            }

            public object GetCtxValue(string name)
            {
                return _ctx.GetCtxValue(name);
            }

            public T GetCtxValue<T>(string name)
            {
                return _ctx.GetCtxValue<T>(name);
            }

            public Dictionary<string, object> GetDump()
            {
                return _ctx.GetDump();
            }

            public IExecutionContext Merge(Dictionary<string, object> context)
            {
                return _ctx.Merge(context);
            }

            public IExecutionContext SetCtxValue(string name, object value)
            {
                return _ctx.SetCtxValue(name, value);
            }
        }

        #endregion

        #region extension methods

        public static IExecutionContext SpanRemote<T>(this IExecutionContext ext, ChannelFactory<T> channelFactory)
        {
            if (!channelFactory.Endpoint.Behaviors.Contains(typeof(ExecutionContextSpanBehavior)))
                channelFactory.Endpoint.Behaviors.Add(new ExecutionContextSpanBehavior());

            return ext;
        }

        public static IExecutionContext SpanRemote(this IExecutionContext ext, ChannelFactory channelFactory)
        {
            if (!channelFactory.Endpoint.Behaviors.Contains(typeof(ExecutionContextSpanBehavior)))
                channelFactory.Endpoint.Behaviors.Add(new ExecutionContextSpanBehavior());

            return ext;
        }

        #endregion

        private static ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
                
        private static object _lk = new object();
        
        public static IExecutionContext Current 
        {
            get
            {
                lock (_lk)
                {
                    ScopeCountContextDecorator tmp = (ScopeCountContextDecorator)System.Runtime.Remoting.Messaging.CallContext.LogicalGetData("__CURRENT_CODEPROBE_CONTEXT");
                    if (tmp == null && OperationContextHolder.Current != null)
                    {
                        OperationContextHolder.Current?.SetExecutionContext(Current = tmp = new ScopeCountContextDecorator(new RemoteExecutionContext()));
                    }
                    else if (tmp != null && OperationContextHolder.Current == null && tmp.Current is RemoteExecutionContext)
                    {
                        tmp.Dispose();
                        Current = tmp = null;
                    }
                    return tmp;
                }
            }
            internal set
            {
                lock (_lk)
                {
                    if(value!=null)
                        System.Runtime.Remoting.Messaging.CallContext.LogicalSetData("__CURRENT_CODEPROBE_CONTEXT", value);
                    else
                        System.Runtime.Remoting.Messaging.CallContext.FreeNamedDataSlot("__CURRENT_CODEPROBE_CONTEXT");
                }
            }
        }

        public static IDisposable GetScope()
        {
            lock (_lk)
            {
                if (Current == null)
                {
                    Current = new ScopeCountContextDecorator(new LocalExecutionContext());

                    return (IDisposable)Current;
                }
                else
                {
                    return ((ScopeCountContextDecorator)Current).AddScope();
                }
            }
        }

        public static IDisposable CreateScope()
        {
            lock(_lk)
            {
                if (Current != null)
                    throw new InvalidOperationException("Cannot create multiple nested scopes. Dispose the Current before.");

                Current = new ScopeCountContextDecorator(new LocalExecutionContext());

                return (IDisposable)Current;
            }
        }

        public static IDisposable EnterScope()
        {
            lock (_lk)
            {
                if (Current == null)
                    throw new InvalidOperationException("Cannot enter a NULL scope. Create a new one.");

                return ((ScopeCountContextDecorator)Current).AddScope();
            }
        }
    }
}
