using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Text;
using System.Threading.Tasks;

namespace aoProbe.Proxies
{
    public abstract class WcfDynamicProxy<T> : RealProxy where T : class
    {
        public WcfChannelFactory<T> ChannelFactory {get;set;}
    }
}
