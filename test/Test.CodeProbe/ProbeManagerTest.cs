using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Collections.Generic;
using System.Linq;

using CodeProbe;
using CodeProbe.Sensing;
using CodeProbe.Reporting;

using Test.CodeProbe.utility;
using System.Threading;
using CodeProbe.Reporting.Reporters;
using CodeProbe.Reporting.Statistics;
using stat = CodeProbe.Reporting.Statistics;
using CodeProbe.Reporting.Samplers;

namespace Test.CodeProbe
{
    public static class TestExtension
    {
        public static void Sample(this ProbeManager ext, string filter, AbstractSampler sampler)
        {
            foreach (AbstractProbe item in ext.MatchFilter(filter))
            {
                sampler.Sample(item);
            }
        }
    }

    [TestClass]
    public class ProbeManagerTest
    {
        public ProbeManagerTest()
        {
            ProbeManager.Init();
        }

        /// <summary>
        /// Clears all the current probes and prepare a set of common probes, simulating a standard sensing.
        /// 1   gauge       ->  1
        /// 1   histogram   ->  1
        /// 1   counter     ->  +1
        /// 1   timer       ->  100ms
        /// 1   meter       ->  1/100ms
        /// </summary>
        [TestInitialize]
        public void SetUp()
        {
            ProbeManager.Ask().Clear(".*");

            ProbeManager.Ask().Gauge("gauge_test", () => 1);
            ProbeManager.Ask().Histogram("histogram_test").Update(1);
            ProbeManager.Ask().Counter("counter_test").Increment();
            for (int i = 0; i < 10; i++)
            {
                ProbeManager.Ask().Timer("test_timer").Time(() => Thread.Sleep(100));
                ProbeManager.Ask().Meter("meter_test").Mark();
            }
        }

        /// <summary>
        /// Perform a report iteration using a json reporter
        /// </summary>
        [TestCategory("Reporting")]
        [TestMethod]
        public void JsonReporterTest()
        {
            AbstractSampler sp = new BasicSampler("test",
                new HashSet<AbstractProbeStatistic>() { 
                    new stat.Gauge.ConstantStatistic(),
                    new stat.Counter.ConstantStatistic() ,
                    new stat.Meter.ConstantStatistic() ,
                    new stat.Histogram.LinearMinStatistic(), new stat.Histogram.LinearMaxStatistic(), new stat.Histogram.LinearAvgStatistic() ,
                    new stat.Timer.ConstantRatioStatistic(), new stat.Timer.LinearMinStatistic(), new stat.Timer.LinearMaxStatistic(), new stat.Timer.LinearAvgStatistic() 
                }
            );
            JsonReporter sm = new JsonReporter("test",sp, @"..\..\test\test.json");

            sm.Start();
            sp.Begin();
            foreach (AbstractProbe item in ProbeManager.Ask().MatchFilter(".*"))
            {
                sp.Sample(item);
            }
            sp.Complete();
            sm.Stop();
        }

        /// <summary>
        /// Verify fitering cababilities of the ProbeManager.
        /// starts with a set of probes, add and remove
        /// </summary>
        [TestCategory("Reporting")]
        [TestMethod]
        public void RegisterProbes()
        {
            DumperSampler tmp = new DumperSampler();
            ProbeManager.Ask().Clear(".*");

            string[] names = new string[] {"prova.gauge","prova.counter","prova.meter","prova.histogram" ,"prova.timer"};

            ProbeManager.Ask().Gauge(names[0], () => 10);
            ProbeManager.Ask().Counter(names[1]);
            ProbeManager.Ask().Meter(names[2]);
            ProbeManager.Ask().Histogram(names[3], new SlidingWindowReservoir<IComparable>(10));
            ProbeManager.Ask().Timer(names[4], new SlidingWindowReservoir<long>(10));

            ProbeManager.Ask().Sample("^prova",tmp);

            CollectionAssert.AreEquivalent(names, tmp.NameDump);

            AbstractProbe probe;
            ProbeManager.Ask().Unregister(names[1],out probe);

            Assert.IsInstanceOfType(probe,typeof(CounterProbe));

            tmp = new DumperSampler();
            ProbeManager.Ask().Sample("^prova", tmp);

            CollectionAssert.AreEquivalent(names.Where((val,id)=>id!=1).ToList(), tmp.NameDump);
        }

        /// <summary>
        /// Verify fitering cababilities of the ProbeManager.
        /// starts with a set of probes, add and remove. Test more regex filters
        /// </summary>
        [TestCategory("Reporting")]
        [TestMethod]
        public void FilterProbes()
        {
            ProbeManager.Ask().Clear(".*");

            string[] names = new string[] { "prova.gauge", "test.counter", "prova.meter", "test.histogram", "prova.counter" };

            ProbeManager.Ask().Gauge(names[0], () => 10);
            ProbeManager.Ask().Counter(names[1]);
            ProbeManager.Ask().Meter(names[2]);
            ProbeManager.Ask().Histogram(names[3], new SlidingWindowReservoir<IComparable>(10));
            ProbeManager.Ask().Timer(names[4], new SlidingWindowReservoir<long>(10));

            DumperSampler tmp = new DumperSampler();
            ProbeManager.Ask().Sample("prova", tmp);

            CollectionAssert.AreEquivalent(new string[] { "prova.gauge", "prova.meter", "prova.counter" }, tmp.NameDump);
            
            tmp = new DumperSampler();
            ProbeManager.Ask().Sample("counter$", tmp);

            CollectionAssert.AreEquivalent(new string[] { "test.counter", "prova.counter" }, tmp.NameDump);
        }

        /// <summary>
        /// Verify fitering cababilities of the ProbeManager.
        /// starts with a set of probes, add and remove. Get one and use it.
        /// </summary>
        [TestCategory("Reporting")]
        [TestMethod]
        public void SenseProbes()
        {
            ProbeManager.Ask().Clear(".*");

            string[] names = new string[] { "prova.gauge", "test.counter", "prova.meter", "test.histogram", "prova.counter" };

            ProbeManager.Ask().Gauge(names[0], () => 10);
            CounterProbe probe=ProbeManager.Ask().Counter(names[1]);
            MeterProbe nprobe=ProbeManager.Ask().Meter(names[2]);
            ProbeManager.Ask().Histogram(names[3], new SlidingWindowReservoir<IComparable>(10));
            TimerProbe tprobe = ProbeManager.Ask().Timer(names[4], new SlidingWindowReservoir<long>(10));
            int tmp=0;
            ProbeManager.Ask().AddSenseHandler("counter", (p, a) => tmp++);

            nprobe.Mark();
            Assert.AreEqual(0, tmp);
            probe.Increment();
            Assert.AreEqual(1, tmp);
            tprobe.Time(() => { });
            Assert.AreEqual(2, tmp);
        }
    }
}
