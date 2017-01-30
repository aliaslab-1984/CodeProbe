using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Text;
using System.Threading.Tasks;

namespace aoProbe.Proxies
{
    public abstract class DynamicProxy<T> where T:class
    {
        protected T _decorated;
        public DynamicProxy(T decorated)
        {
            _decorated = decorated;
        }

        public T GetTransparentProxy()
        {
            return null;
        }

        public abstract IMessage Invoke(IMessage msg);
    }
}
