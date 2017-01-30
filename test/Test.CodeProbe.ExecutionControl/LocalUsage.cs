using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CodeProbe.ExecutionControl;
using System.Threading.Tasks;
using System.Threading;

namespace Test.CodeProbe.ExecutionControl
{
    /// <summary>
    /// Summary description for LocalUsage
    /// </summary>
    [TestClass]
    public class LocalUsage
    {
        public LocalUsage()
        {
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
        }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
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

        [TestCategory("ExecutionControl-Local")]
        [TestMethod]
        public void SingleScope()
        {
            int expected = 1;

            using (ExecutionControlManager.GetScope())
            {
                IExecutionContext ctx=ExecutionControlManager.Current;
                ctx.SetCtxValue("name", expected);

                Assert.AreEqual("name", ctx.GetCtxKeys()[0]);
                Assert.AreEqual(expected,ctx.GetCtxValue<int>("name"));
            }
        }

        [TestCategory("ExecutionControl-Local")]
        [TestMethod]
        public void NestedScope()
        {
            int expected = 1;

            Action action = () =>
            {
                using (ExecutionControlManager.GetScope())
                {
                    IExecutionContext ctx = ExecutionControlManager.Current;
                    Assert.AreEqual(expected, ctx.GetCtxValue<int>("name"));

                    ctx.SetCtxValue("name_2", expected);
                }
            };

            using (ExecutionControlManager.GetScope())
            {

                IExecutionContext ctx = ExecutionControlManager.Current;
                ctx.SetCtxValue("name", expected);

                Assert.AreEqual("name", ctx.GetCtxKeys()[0]);
                Assert.AreEqual(expected, ctx.GetCtxValue<int>("name"));

                action();

                List<string> tmp = ctx.GetCtxKeys();
                foreach (string key in tmp)
                {
                    Assert.AreEqual(expected, ctx.GetCtxValue<int>(key));
                }
            }
        }

        [TestCategory("ExecutionControl-Local")]
        [TestMethod]
        public void NestedScopeMultiThread()
        {
            int expected = 1;

            Action action = () =>
            {
                using (ExecutionControlManager.GetScope())
                {
                    IExecutionContext ctx = ExecutionControlManager.Current;
                    Assert.AreEqual(expected, ctx.GetCtxValue<int>("name"));

                    ctx.SetCtxValue("name_2", expected);
                }
            };

            using (ExecutionControlManager.GetScope())
            {
                IExecutionContext ctx = ExecutionControlManager.Current;
                ctx.SetCtxValue("name", expected);

                Assert.AreEqual(expected, ctx.GetCtxValue<int>("name"));

                Task tk=Task.Factory.StartNew(action);

                tk.Wait();

                List<string> tmp = ctx.GetCtxKeys();
                foreach (string key in tmp)
                {
                    Assert.AreEqual(expected, ctx.GetCtxValue<int>(key));
                }
            }
        }

        [TestCategory("ExecutionControl-Local")]
        [TestMethod]
        public void NestedScopeMultiThreadConcurrency()
        {
            int expected = 1;
            int expectedConcurrent = 1;
            int concurrentCount = 10;
            object _lk = new object();

            Func<int,Action> newAction = idx => {
                return () =>
                {
                    using (ExecutionControlManager.GetScope())
                    {
                        IExecutionContext ctx = ExecutionControlManager.Current;
                        ctx.SetCtxValue("name_" + idx, expected);
                     
                        Thread.Sleep(500);
                        lock (_lk)
                        {
                            ctx.SetCtxValue("name", ++expectedConcurrent);
                            Assert.AreEqual(expectedConcurrent, ctx.GetCtxValue<int>("name"));
                        }
                        Thread.Sleep(500);
                    }
                };
            };

            using (ExecutionControlManager.GetScope())
            {
                IExecutionContext ctx = ExecutionControlManager.Current;
                ctx.SetCtxValue("name", expected);

                Assert.AreEqual("name", ctx.GetCtxKeys()[0]);
                Assert.AreEqual(expected, ctx.GetCtxValue<int>("name"));

                Task[] tks = new Task[concurrentCount];
                for (int i = 0; i < concurrentCount; i++)
                {
                    tks[i] = Task.Factory.StartNew(newAction(i));
                }

                Task.WaitAll(tks);

                List<string> tmp = ctx.GetCtxKeys();
                foreach (string key in tmp.Where(p=>p!="name"))
                {
                    Assert.AreEqual(expected, ctx.GetCtxValue<int>(key));
                }
                Assert.AreEqual(expectedConcurrent, ctx.GetCtxValue<int>("name"));
            }
        }
    }
}
