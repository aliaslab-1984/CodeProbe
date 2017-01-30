using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ServiceModel;
using aoProbe.Proxies;
using System.Runtime.Remoting.Messaging;

namespace Test.aoProbe
{
    [TestClass]
    public class WcfDynamicProxy
    {
        [ServiceContract]
        public interface ISvcTest
        {
            [OperationContract]
            string Greet(string name);
        }

        public class SvcTest : ISvcTest
        {
            public string Greet(string name)
            {
                return "Ciao " + name + "!";
            }
        }

        public class NullProxy<T> : WcfDynamicProxy<T> where T : class
        {
            public override IMessage Invoke(IMessage msg)
            {
                var methodCall = msg as IMethodCallMessage;
                var methodBase = methodCall.MethodBase;

                // We can't call CreateChannel() because that creates an instance of this class,
                // and we'd end up with a stack overflow. So, call CreateBaseChannel() to get the
                // actual service.
                T wcfService = ChannelFactory.CreateBaseChannel();

                var result = methodBase.Invoke(wcfService, methodCall.Args);

                return new ReturnMessage(
                    result, // Operation result
                    null, // Out arguments
                    0, // Out arguments count
                    methodCall.LogicalCallContext, // Call context
                    methodCall); // Original message
            }
        }

        protected static ServiceHost _host;

        [ClassInitialize]
        public static void InitializeTest(TestContext ctx)
        {
            Uri baseAddress = new Uri("http://localhost:8080/hello");

            _host = new ServiceHost(typeof(SvcTest), baseAddress);

            _host.Open();
        }

        [ClassCleanup]
        public static void CleanupTest()
        {
            _host.Close();
        }

        [TestMethod]
        public void WcfDynamicProxy1()
        {
            using (ChannelFactory<ISvcTest> chf = new WcfChannelFactory<ISvcTest>(new NullProxy<ISvcTest>()))
            {
                ISvcTest tmp = chf.CreateChannel(new EndpointAddress("http://localhost:8080/hello"));

                string result = tmp.Greet("Piero");

                Assert.AreEqual("Ciao Piero!", result);
            }
        }
    }
}
