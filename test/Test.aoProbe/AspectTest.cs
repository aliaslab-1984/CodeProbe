using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using aoProbe;

namespace Test.aoProbe
{
    [TestClass]
    public class AspectTest
    {
        [TestMethod]
        public void AspectTest1()
        {
            Aspect test = new Aspect(args => {
                return (int)args["x"] + Convert.ToInt32((string)args["val"]);
            });

            int result=(int)test.GetInstance(new { x=3,val="5" })();

            Assert.AreEqual(8, result);
        }
    }
}
