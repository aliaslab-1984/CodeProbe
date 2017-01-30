using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CodeProbe.ExecutionControl.Process.Models;
using System.Collections.Generic;
using CodeProbe.ExecutionControl.Process;

namespace Test.CodeProbe.ExecutionControl
{
    [TestClass]
    public class ProcessModels
    {
        #region neseted types

        public class StringActivity : IProcessActivity
        {
            public StringActivity(string value)
            {
                Id = value;
            }

            public string Id
            {
                get; protected set;
            }

            public override bool Equals(object obj)
            {
                if (obj == null)
                    return false;
                else if (GetType() != obj.GetType())
                    return false;
                else
                    return ((StringActivity)obj).Id == Id;
            }

            public override int GetHashCode()
            {
                return Id.GetHashCode();
            }
        }

        #endregion

        [TestCategory("CNets")]
        [TestMethod]
        public void Bindings()
        {
            CausalBinding<string> bnd = new CausalBinding<string>("a",new string[] { "b","c" },new string[] { "z" });
            CausalBinding<string> bnd2 = new CausalBinding<string>("a", new string[] { "c", "b" }, new string[] { "z" });

            Assert.IsTrue(bnd.Equals(bnd2));
            Assert.IsTrue(bnd.GetHashCode()==bnd2.GetHashCode());

            Assert.AreEqual("{a,{b,c},{z}}", bnd.ToString());
        }

        [TestCategory("CNets")]
        [TestMethod]
        public void NetworkSequenceValidation()
        {
            IProcessEvaluator ev = new CausalNetworkEvaluator();
            List<CausalBinding<IProcessActivity>> netCtr = new List<CausalBinding<IProcessActivity>>()
            {
                new CausalBinding<IProcessActivity>(new StringActivity("a"), null, new StringActivity[] { new StringActivity("b"), new StringActivity("c") }),
                new CausalBinding<IProcessActivity>(new StringActivity("a"), null, new StringActivity[] { new StringActivity("b") }),
                new CausalBinding<IProcessActivity>(new StringActivity("a"), null, new StringActivity[] { new StringActivity("d") }),
                new CausalBinding<IProcessActivity>(new StringActivity("b"), new StringActivity[] { new StringActivity("a")}, new StringActivity[] { new StringActivity("e") }),
                new CausalBinding<IProcessActivity>(new StringActivity("c"), new StringActivity[] { new StringActivity("a")}, new StringActivity[] { new StringActivity("e") }),
                new CausalBinding<IProcessActivity>(new StringActivity("d"), new StringActivity[] { new StringActivity("a")}, new StringActivity[] { new StringActivity("e") }),
                new CausalBinding<IProcessActivity>(new StringActivity("e"), new StringActivity[] { new StringActivity("d")}, null),
                new CausalBinding<IProcessActivity>(new StringActivity("e"), new StringActivity[] { new StringActivity("b"), new StringActivity("c")}, null),
                new CausalBinding<IProcessActivity>(new StringActivity("e"), new StringActivity[] { new StringActivity("b")}, null)
            };
            CausalNetwork<IProcessActivity> net = new CausalNetwork<IProcessActivity>(netCtr);
            List<IProcessActivity> seq = new List<IProcessActivity>() { new StringActivity("a"), new StringActivity("c"), new StringActivity("b"), new StringActivity("e") };
            
            Assert.IsTrue(ev.IsPossibleSequence(net,seq));
            Assert.IsTrue(ev.IsValidSequence(net,seq));

            seq = new List<IProcessActivity>() { new StringActivity("a"), new StringActivity("c"), new StringActivity("b") };

            Assert.IsTrue(ev.IsPossibleSequence(net, seq));
            Assert.IsFalse(ev.IsValidSequence(net, seq));
        }

        [TestCategory("CNets")]
        [TestMethod]
        public void NetworkSequenceValidationImplicit()
        {
            IProcessEvaluator ev = new CausalNetworkEvaluator();
            CausalNetwork<IProcessActivity> net = new CausalNetwork<IProcessActivity>();
            StringActivity a = new StringActivity("a");
            StringActivity b = new StringActivity("b");
            StringActivity c = new StringActivity("c");
            StringActivity d = new StringActivity("d");
            StringActivity e = new StringActivity("e");

            net.AddActivity(a);
            net.AddActivity(b);
            net.AddActivity(c);
            net.AddActivity(d);
            net.AddAndSplit(null, a, new IProcessActivity[] { b, c });
            net.AddXOrSplit(null, a, new IProcessActivity[] { b, d });
            net.AddXOrJoin(new IProcessActivity[]{ a }, b, new IProcessActivity[] { e });
            net.AddXOrJoin(new IProcessActivity[]{ a }, c, new IProcessActivity[] { e });
            net.AddXOrJoin(new IProcessActivity[]{ a }, d, new IProcessActivity[] { e });
            net.AddXOrJoin(new IProcessActivity[] { d }, e, null);
            net.AddAndJoin(new IProcessActivity[] { b,c }, e, null);
            net.AddXOrJoin(new IProcessActivity[] { b }, e, null);

            List<IProcessActivity> seq = new List<IProcessActivity>() { a, c, b, e };

            Assert.IsTrue(ev.IsPossibleSequence(net, seq));
            Assert.IsTrue(ev.IsValidSequence(net, seq));

            seq = new List<IProcessActivity>() { a, c, b };

            Assert.IsTrue(ev.IsPossibleSequence(net, seq));
            Assert.IsFalse(ev.IsValidSequence(net, seq));
        }
    }
}
