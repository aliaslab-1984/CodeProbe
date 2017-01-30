using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using aoProbe;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Test.aoProbe
{
    [TestClass]
    public class Performance
    {
        public static int iterations = 100;
        public static int maxVal = 1000;

        public static int testDelay = 10;


        [ClassInitialize]
        public static void Initializer(TestContext ctx)
        {
            ExecManager.Register(new Aspect(args =>
            {
                Thread.Sleep(testDelay);
                return args.Values.Sum(p => Convert.ToInt32(p));
            }) { Name = "sum" });
            ExecManager.Register(new Aspect(args =>
            {
                Thread.Sleep(testDelay);
                return Convert.ToInt32(args.Values.First()) * 2;
            }) { Name = "x2" });
            ExecManager.Register(new Aspect(args =>
            {
                Thread.Sleep(testDelay);
                return Convert.ToInt32(args.Values.First()) * 3;
            }) { Name = "x3" });
        }

        [TestMethod]
        public void Normal()
        {
            Random rnd = new Random();

            for (int i = 0; i < iterations; i++)
            {
                int val = rnd.Next(maxVal);

                val = (val * 3) + (val * 2);
            }
        }

        [TestMethod]
        public void Aop()
        {
            Random rnd = new Random();

            for (int i = 0; i < iterations; i++)
            {
                int val = rnd.Next(maxVal);

                val = (int)ExecManager.GetInstance("sum", new { q = (int)ExecManager.GetInstance("x3", new { x = val })(), k= (int)ExecManager.GetInstance("x2", new { x = val })() })();
            }
        }
    }
}
