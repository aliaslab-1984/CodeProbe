using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CodeProbe.ExecutionControl;
using System.ServiceModel;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Description;
using System.ServiceModel.Configuration;
using System.Threading;
using System.Threading.Tasks;

namespace Test.CodeProbe.ExecutionControl
{
    /// <summary>
    /// Summary description for RemoteUsage
    /// </summary>
    [TestClass]
    public class RemoteUsage
    {
        #region nested types
        
        private static ServiceHost _host;

        [ServiceContract]
        public interface ITestService
        {
            [OperationContract]
            void Op1(int exp);

            [OperationContract]
            void Op2(int exp);

            [OperationContract]
            void Op3(int expected);

            [OperationContract]
            void Op4(int expected);
        }

        public class TestService : ITestService 
        {
            public void Op1(int expected)
            {
                using (ExecutionControlManager.GetScope())
                {
                    IExecutionContext ctx = ExecutionControlManager.Current;
                    Assert.AreEqual("name", ctx.GetCtxKeys()[0]);
                    Assert.AreEqual(expected, ctx.GetCtxValue<int>("name"));
                }
            }

            public void Op2(int value)
            {
                using (ExecutionControlManager.GetScope())
                {
                    IExecutionContext ctx = ExecutionControlManager.Current;
                    ctx.SetCtxValue("name", value);

                    Assert.AreEqual("name", ctx.GetCtxKeys()[0]);
                    Assert.AreEqual(value, ctx.GetCtxValue<int>("name"));
                }
            }

            public void Op3(int expected)
            {
                using (ExecutionControlManager.GetScope())
                using (ChannelFactory<ITestService> fc = new ChannelFactory<ITestService>("test"))
                {
                    try
                    {
                        ITestService svc = fc.CreateChannel();

                        ExecutionControlManager.Current.SetCtxValue("name", expected);

                        svc.Op1(expected);

                        svc.Op2(expected+1);
                        Assert.AreEqual(expected+1, ExecutionControlManager.Current.GetCtxValue<int>("name"));
                    }
                    catch (Exception e)
                    {
                        Assert.Fail();
                    }
                }
            }

            public void Op4(int expected)
            {
                using (ExecutionControlManager.GetScope())
                {
                    Task.Factory.StartNew(() =>
                    {
                        Thread.Sleep(2000);
                        using (ChannelFactory<ITestService> fc = new ChannelFactory<ITestService>("test"))
                        {
                            using (ExecutionControlManager.GetScope())
                            {
                                try
                                {
                                    ITestService svc = fc.CreateChannel();

                                    ExecutionControlManager.Current.SetCtxValue("name", expected);

                                    svc.Op1(expected);
                                }
                                catch (Exception e)
                                {
                                    Assert.Fail();
                                }
                            }
                        }
                    });
                }
            }
        }

        #endregion

        public RemoteUsage()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            _host = new ServiceHost(typeof(TestService));
            ServiceEndpoint ep = _host.AddServiceEndpoint(typeof(ITestService), new BasicHttpBinding(), "http://localhost:12345/testService");
            ep.Behaviors.Add(new ExecutionContextSpanBehavior());

            _host.Open();
        }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        [ClassCleanup()]
        public static void MyClassCleanup() 
        {
            _host.Close();
        }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestCategory("ExecutionControl-Remote")]
        [TestMethod]
        public void SingleScope()
        {
            int expected = 1;
            using (ChannelFactory<ITestService> fc = new ChannelFactory<ITestService>(new BasicHttpBinding()))
            {
                using (ExecutionControlManager.GetScope())
                {
                    IExecutionContext ctx = ExecutionControlManager.Current.SpanRemote<ITestService>(fc);

                    ITestService svc = fc.CreateChannel(new EndpointAddress("http://localhost:12345/testService"));

                    ctx.SetCtxValue("name", expected);

                    Assert.AreEqual("name", ctx.GetCtxKeys()[0]);
                    Assert.AreEqual(expected, ctx.GetCtxValue<int>("name"));
                                        
                    svc.Op1(expected);

                    Assert.AreEqual("name", ctx.GetCtxKeys()[0]);
                    Assert.AreEqual(expected, ctx.GetCtxValue<int>("name"));

                    svc.Op2(expected + 1);

                    Assert.AreEqual("name", ctx.GetCtxKeys()[0]);
                    Assert.AreEqual(expected + 1, ctx.GetCtxValue<int>("name"));

                    svc.Op1(expected + 1);

                    Assert.AreEqual("name", ctx.GetCtxKeys()[0]);
                    Assert.AreEqual(expected + 1, ctx.GetCtxValue<int>("name"));
                }
            }
        }

        [TestCategory("ExecutionControl-Remote")]
        [TestMethod]
        public void SingleScopeConfig()
        {
            int expected = 1;
            using (ChannelFactory<ITestService> fc = new ChannelFactory<ITestService>("test"))
            {
                using (ExecutionControlManager.GetScope())
                {
                    IExecutionContext ctx = ExecutionControlManager.Current;

                    ITestService svc = fc.CreateChannel();

                    ctx.SetCtxValue("name", expected);

                    Assert.AreEqual("name", ctx.GetCtxKeys()[0]);
                    Assert.AreEqual(expected, ctx.GetCtxValue<int>("name"));

                    svc.Op1(expected);

                    Assert.AreEqual("name", ctx.GetCtxKeys()[0]);
                    Assert.AreEqual(expected, ctx.GetCtxValue<int>("name"));

                    svc.Op2(expected + 1);

                    Assert.AreEqual("name", ctx.GetCtxKeys()[0]);
                    Assert.AreEqual(expected + 1, ctx.GetCtxValue<int>("name"));

                    svc.Op1(expected + 1);

                    Assert.AreEqual("name", ctx.GetCtxKeys()[0]);
                    Assert.AreEqual(expected + 1, ctx.GetCtxValue<int>("name"));
                }
            }
        }

        [TestCategory("ExecutionControl-Remote")]
        [TestMethod]
        public void SingleScopeConfigNested()
        {
            int expected = 1;
            using (ChannelFactory<ITestService> fc = new ChannelFactory<ITestService>("test"))
            {                    
                ITestService svc = fc.CreateChannel();
                
                svc.Op3(expected);
            }
        }

        [TestCategory("ExecutionControl-Remote-DEBUG")]
        [TestMethod]
        public void SingleScopeConfigAsync()
        {
            int expected = 1;
            using (ChannelFactory<ITestService> fc = new ChannelFactory<ITestService>("test"))
            {
                ITestService svc = fc.CreateChannel();

                svc.Op4(expected);

                Thread.Sleep(10000);
            }
        }
    }
}
