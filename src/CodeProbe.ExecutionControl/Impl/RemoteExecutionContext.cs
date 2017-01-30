using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace CodeProbe.ExecutionControl.Impl
{
    internal class RemoteExecutionContext : IExecutionContext
    {
        protected bool _disposed = false;
        protected object _lk = new object();
        
        public virtual void Dispose()
        {
            if (!_disposed)
            {
                ExecutionControlManager.Current = null;

                _disposed = true;
            }
        }

        public List<string> GetCtxKeys()
        {
            lock (_lk)
            {
                return OperationContextHolder.Current?.Context?.Keys.ToList();
            }
        }

        public object GetCtxValue(string name)
        {
            lock (_lk)
            {
                if (OperationContextHolder.Current?.Context != null && OperationContextHolder.Current.Context.ContainsKey(name))
                    return OperationContextHolder.Current?.Context?[name];
                else
                    return null;
            }
        }

        public T GetCtxValue<T>(string name)
        {
            lock (_lk)
            {
                if (OperationContextHolder.Current?.Context != null && OperationContextHolder.Current.Context.ContainsKey(name))
                    return (T)OperationContextHolder.Current?.Context?[name];
                else
                    return default(T);
            }
        }

        public Dictionary<string, object> GetDump()
        {
            lock (_lk)
            {
                return OperationContextHolder.Current?.Context?.ToDictionary(kv => kv.Key, kv => kv.Value);
            }
        }

        public IExecutionContext Merge(Dictionary<string, object> context)
        {
            lock (_lk)
            {
                if (OperationContextHolder.Current != null)
                {
                    foreach (KeyValuePair<string, object> kv in context)
                    {
                        if (!OperationContextHolder.Current.Context.ContainsKey(kv.Key))
                            OperationContextHolder.Current.Context.Add(kv.Key, kv.Value);
                        else
                            OperationContextHolder.Current.Context[kv.Key] = kv.Value;
                    }
                }

                return this;
            }
        }

        public IExecutionContext SetCtxValue(string name, object value)
        {
            lock (_lk)
            {
                if (OperationContextHolder.Current != null)
                {
                    if (!OperationContextHolder.Current.Context.ContainsKey(name))
                        OperationContextHolder.Current.Context.Add(name, value);
                    else
                        OperationContextHolder.Current.Context[name] = value;
                }

                return this;
            }
        }
    }
}
