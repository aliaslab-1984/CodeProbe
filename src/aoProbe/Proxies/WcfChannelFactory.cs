using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace aoProbe.Proxies
{
    public class WcfChannelFactory<T> : ChannelFactory<T> where T : class
    {
        protected WcfDynamicProxy<T> _proxy;

        public WcfChannelFactory(WcfDynamicProxy<T> proxy)
        {
            _proxy = proxy;
            _proxy.ChannelFactory = this;
        }

        public T CreateBaseChannel()
        {
            return base.CreateChannel(this.Endpoint.Address, null);
        }

        public override T CreateChannel(EndpointAddress address, Uri via)
        {
            this.Endpoint.Address = address;
            return _proxy.GetTransparentProxy() as T;
        }
    }
}
