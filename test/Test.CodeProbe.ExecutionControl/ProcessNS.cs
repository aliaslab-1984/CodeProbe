using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CodeProbe.ExecutionControl.Process;
using System.Collections.Generic;
using System.Linq;

namespace Test.CodeProbe.ExecutionControl
{
    [TestClass]
    public class ProcessNS
    {
        #region nested types

        public class PrimitiveActivity : AbstractBusinessActivity
        {
            public List<Tuple<string, string>> Convert(object value)
            {
                return ConvertToObjectDictionary(value);
            }

            public bool Contained(object a, object b)
            {
                return CheckObjectDictionaryInclusion(ConvertToObjectDictionary(a), ConvertToObjectDictionary(b));
            }

            public override void Enter()
            {
                throw new NotImplementedException();
            }

            public override void Exit()
            {
                throw new NotImplementedException();
            }

            public override IDisposable Wrap()
            {
                throw new NotImplementedException();
            }

            public override void Start()
            {
                throw new NotImplementedException();
            }

            public override void End()
            {
                throw new NotImplementedException();
            }
        }
        #endregion

        [TestCategory("ExecutionControl-Process")]
        [TestMethod]
        public void AbstractProcessPrimitives()
        {
            PrimitiveActivity activity = new PrimitiveActivity();
            bool bres = false;
            List <Tuple<string, string>> tmp, res;
            object tmpObj, tmpObj2;

            tmp = new List<Tuple<string, string>>() {
                new Tuple<string, string>("/nome", "io"),
                new Tuple<string, string>("/indirizzo/via","garibaldi"),
                new Tuple<string, string>("/indirizzo/numero","12"),
                new Tuple<string, string>("/numeri/[0]","123"),
                new Tuple<string, string>("/numeri/[1]/key","a"),
                new Tuple<string, string>("/numeri/[1]/value","456")
            };
            tmpObj = new { nome="io", indirizzo=new { via="garibaldi",numero=12 }, numeri=new object[] { "123",new { key="a",value = 456 } } };

            res = activity.Convert(tmpObj);
            
            CollectionAssert.AreEquivalent(tmp.Select(p => p.Item1).OrderBy(p=>p).ToArray(), res.Select(p => p.Item1).OrderBy(p => p).ToArray());
            CollectionAssert.AreEquivalent(tmp.Select(p=>p.Item2).OrderBy(p => p).ToArray(), res.Select(p=>p.Item2).OrderBy(p => p).ToArray());
            
            tmp = new List<Tuple<string, string>>() {
                new Tuple<string, string>("/nome", "io"),
                new Tuple<string, string>("/point/x", "0"),
                new Tuple<string, string>("/point/y", "0"),
                //new Tuple<string, string>("/lst/[0]", "0"),
                //new Tuple<string, string>("/lst/[1]", "ref>/lst"),
                new Tuple<string, string>("/indirizzo/via","garibaldi"),
                new Tuple<string, string>("/indirizzo/numero","12"),
                new Tuple<string, string>("/numeri/[0]","123"),
                new Tuple<string, string>("/numeri/[1]/key","a"),
                new Tuple<string, string>("/numeri/[1]/value","456"),
                new Tuple<string, string>("/numeri/[2]","ref>/point")
            };
            object x = new { x = 0, y = 0 };
            //object[] y = new object[] { 0, 0 };
            //y[1]=y;
            tmpObj = new { nome = "io", point=x, indirizzo = new { via = "garibaldi", numero = 12 }, numeri = new object[] { "123", new { key = "a", value = 456 }, x } };

            res = activity.Convert(tmpObj);

            CollectionAssert.AreEquivalent(tmp.Select(p => p.Item1).OrderBy(p => p).ToArray(), res.Select(p => p.Item1).OrderBy(p => p).ToArray());
            CollectionAssert.AreEquivalent(tmp.Select(p => p.Item2).OrderBy(p => p).ToArray(), res.Select(p => p.Item2).OrderBy(p => p).ToArray());

            tmpObj2 = new { nome = "io", indirizzo = new { via = "garibaldi", numero = 12 }, numeri = new string[] { "123", "456" } };
            bres = activity.Contained(tmpObj, tmpObj2);
            Assert.IsFalse(bres);
            tmpObj2 = new { nome = "io", indirizzo = new { via = "garibaldi", numero = 12 }, numeri = new string[] { "123" } };
            bres = activity.Contained(tmpObj, tmpObj2);
            Assert.IsTrue(bres);
        }
    }
}
