using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using aoProbe;

namespace Test.aoProbe
{
    [TestClass]
    public class AllTest
    {
        [TestInitialize()]
        public void SetUp()
        {
            ExecManager.Register(new Aspect(args =>
            {
                return args.Values.Sum(p => Convert.ToInt32(p));
            }) { Name = "sum" });
            ExecManager.Register(new Aspect(args =>
            {
                return Convert.ToInt32(args.Values.First()) * 2;
            }) { Name = "x2" });
            ExecManager.Register(new Aspect(args =>
            {
                return Convert.ToInt32(args.Values.First()) * 3;
            }) { Name = "x3" });
        }

        [TestMethod]
        public void First()
        {
            int res = (int)ExecManager.GetInstance("x3", new { x = 3 })();

            Assert.AreEqual(9, res);
        }

        [TestMethod]
        public void Second()
        {
            int res = (int)ExecManager.GetInstance("x3", new { x = 3 })();
            res = (int)ExecManager.GetInstance("x2", new { x = res })();
            res = (int)ExecManager.GetInstance("sum", new { x = 1, y=2, z=res })();

            Assert.AreEqual(21, res);
        }

        [TestMethod]
        public void Third()
        {
            ProcessType tp = new ProcessType(new string[] { "x3","x2","sum" });

            int res = (int)ExecManager.GetInstanceForProcessOfType(1, tp, "x3", new { x = 3 })();
            res = (int)ExecManager.GetInstanceForProcessOfType(1, tp, "x2", new { x = res })();
            res = (int)ExecManager.GetInstanceForProcessOfType(1, tp, "sum", new { x = 1, y = 2, z = res })();

            Assert.AreEqual(21, res);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void Forth()
        {
            ProcessType tp = new ProcessType(new string[]{"x3","x2","x3"});

            int res = (int)ExecManager.GetInstanceForProcessOfType(1,tp,"x3", new { x = 3 })();
            res = (int)ExecManager.GetInstanceForProcessOfType(1, tp, "x2", new { x = res })();
            res = (int)ExecManager.GetInstanceForProcessOfType(1, tp, "sum", new { x = 1, y = 2, z = res })();

            Assert.AreEqual(21, res);
        }
    }
}
