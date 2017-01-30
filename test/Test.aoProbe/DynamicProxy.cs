using System;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using aoProbe.Proxies;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.aoProbe
{
    [TestClass]
    public class DynamicProxy
    {
        #region nested types

        public class TestProxy1<T> : DynamicProxy<T> where T : class
        {
            public TestProxy1(T decorated)
                : base(decorated)
            { }

            public override IMessage Invoke(IMessage msg)
            {
                IMethodCallMessage methodCall = msg as IMethodCallMessage;
                MethodInfo methodInfo = methodCall.MethodBase as MethodInfo;
                try
                {
                    var result = methodInfo.Invoke(_decorated, methodCall.InArgs);
                    return new ReturnMessage(result, null, 0, methodCall.LogicalCallContext, methodCall);
                }
                catch (Exception e)
                {
                    return new ReturnMessage(e, methodCall);
                }
            }
        }

        public class TestProxy2<T> : DynamicProxy<T> where T : class
        {
            public TestProxy2(T decorated)
                : base(decorated)
            { }

            public override IMessage Invoke(IMessage msg)
            {
                IMethodCallMessage methodCall = msg as IMethodCallMessage;
                MethodInfo methodInfo = methodCall.MethodBase as MethodInfo;
                try
                {
                    var result = methodInfo.Invoke(_decorated, methodCall.InArgs);
                    return new ReturnMessage(result, null, 0, methodCall.LogicalCallContext, methodCall);
                }
                catch (Exception e)
                {
                    return new ReturnMessage(e, methodCall);
                }
            }
        }

        #endregion
        
        #region nested types

        public interface IDoSomething
        {
            int DoOne();
            int DoTwo();
        }

        public class DoSomething : IDoSomething
        {
            public int DoOne()
            {
                return 1;
            }

            public int DoTwo()
            {
                return 2;
            }
        }

        #endregion

        [TestMethod]
        public void TestDynamicProxy1()
        {
            DynamicProxy<DoSomething> prx = new TestProxy1<DoSomething>(new DoSomething());
            DoSomething exec = prx.GetTransparentProxy() as DoSomething;
            exec.DoOne();

            prx = new TestProxy2<DoSomething>(exec);
            exec = prx.GetTransparentProxy() as DoSomething;
            exec.DoOne();
                
            Console.WriteLine();
        }
    }
}
