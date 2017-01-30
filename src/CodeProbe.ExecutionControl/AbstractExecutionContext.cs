using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;

namespace CodeProbe.ExecutionControl
{
    internal abstract class AbstractExecutionContext : IExecutionContext
    {
        public const string EXECUTION_CONTEXT_NS = "__ExecCtx_NS";
        public const string EXECUTION_CONTEXT_VALUE = "__ExecCtx_VALUE";

        protected bool _disposed = false;
        protected object _lk = new object();

        public virtual IExecutionContext SetCtxValue(string name, object value)
        {
            lock (_lk)
            {
                if (_disposed)
                    throw new ObjectDisposedException(GetType().Name);
                Dictionary<string, object> tmp = (Dictionary<string, object>)CallContext.LogicalGetData(EXECUTION_CONTEXT_VALUE);
                if (tmp == null)
                    tmp = new Dictionary<string, object>();
                if (!tmp.ContainsKey(name))
                    tmp.Add(name, value);
                else
                    tmp[name] = value;

                CallContext.LogicalSetData(EXECUTION_CONTEXT_VALUE, tmp);
                
                return this;
            }
        }

        public virtual object GetCtxValue(string name)
        {
            lock (_lk)
            {
                if (_disposed)
                    throw new ObjectDisposedException(GetType().Name);
                Dictionary<string, object> tmp = (Dictionary<string, object>)CallContext.LogicalGetData(EXECUTION_CONTEXT_VALUE);
                if (tmp == null || !tmp.ContainsKey(name))
                    return null;
                else
                    return tmp[name];
            }
        }

        public virtual T GetCtxValue<T>(string name)
        {
            lock (_lk)
            {
                if (_disposed)
                    throw new ObjectDisposedException(GetType().Name);
                Dictionary<string, object> tmp = (Dictionary<string, object>)CallContext.LogicalGetData(EXECUTION_CONTEXT_VALUE);
                if (tmp == null || !tmp.ContainsKey(name))
                    return default(T);
                else
                    return (T)tmp[name];
            }
        }

        public virtual List<string> GetCtxKeys()
        {
            lock (_lk)
            {
                if (_disposed)
                    throw new ObjectDisposedException(GetType().Name);
                Dictionary<string, object> tmp = (Dictionary<string, object>)CallContext.LogicalGetData(EXECUTION_CONTEXT_VALUE);
                if (tmp == null)
                    return new List<string>();
                else
                    return tmp.Keys.ToList();
            }
        }

        public virtual Dictionary<string, object> GetDump()
        {
            lock (_lk)
            {
                if (_disposed)
                    throw new ObjectDisposedException(GetType().Name);
                return (Dictionary<string, object>)CallContext.LogicalGetData(EXECUTION_CONTEXT_VALUE);
            }
        }

        public virtual IExecutionContext Merge(Dictionary<string, object> context)
        {
            lock (_lk)
            {
                if (_disposed)
                    throw new ObjectDisposedException(GetType().Name);
                Dictionary<string, object> tmp = (Dictionary<string, object>)CallContext.LogicalGetData(EXECUTION_CONTEXT_VALUE);
                if (tmp == null)
                    tmp = new Dictionary<string, object>();

                if (context != null)
                {
                    foreach (KeyValuePair<string, object> kv in context)
                    {
                        if (!tmp.ContainsKey(kv.Key))
                            tmp.Add(kv.Key, kv.Value);
                        else
                            tmp[kv.Key] = kv.Value;
                    }
                }

                CallContext.LogicalSetData(EXECUTION_CONTEXT_VALUE, tmp);

                return this;
            }
        }

        public virtual void Dispose()
        {
            if (!_disposed)
            {
                CallContext.FreeNamedDataSlot(EXECUTION_CONTEXT_VALUE);
                _disposed = true;
            }
        }
    }
}
