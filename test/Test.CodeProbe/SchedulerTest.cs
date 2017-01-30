using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using CodeProbe;
using CodeProbe.Sensing;
using CodeProbe.Reporting;

using Test.CodeProbe.utility;
using CodeProbe.Reporting.Extractors;
using System.Threading.Tasks;

namespace Test.CodeProbe
{
    [TestClass]
    public class SchedulerTest
    {
        /// <summary>
        /// Test the correctness of the test scheduling.
        /// Schedueles 2 TimedSampler based on 2 waitTime fractions
        /// 1:  waitTime/numTimes
        /// 2:  waitTime/(2*numTimes)
        /// </summary>
        [TestCategory("Reporting-Sampling")]
        [TestMethod]
        public void ScheduleTest()
        {
            #region params

            string[] names = new string[] { "prova.gauge", "prova.counter", "prova.meter", "prova.histogram", "prova.timer" };
            int waitTime = 1000;
            int numTimes = 5;

            CounterSampler sampler = new CounterSampler();
            CounterSampler samplerX2 = new CounterSampler();

            TimedSampleExtractor _extractor = new TimedSampleExtractor("test",waitTime / numTimes);
            TimedSampleExtractor _extractor2 = new TimedSampleExtractor("test",waitTime / (numTimes * 2));

            #endregion

            ProbeManager.Ask().Gauge(names[0], () => 10);
            ProbeManager.Ask().Counter(names[1]);
            ProbeManager.Ask().Meter(names[2]);
            ProbeManager.Ask().Histogram(names[3], new SlidingWindowReservoir<IComparable>(10));
            ProbeManager.Ask().Timer(names[4], new SlidingWindowReservoir<long>(10));

            _extractor.AddSampler(sampler, ".*");
            _extractor2.AddSampler(samplerX2, ".*");

            Thread.Sleep(waitTime);
            _extractor2.RemoveSampler(samplerX2);
            _extractor.RemoveSampler(sampler);

            Assert.AreEqual(numTimes, sampler.Count / names.Length, 1);
            Assert.AreEqual(numTimes * 2, samplerX2.Count / names.Length, 1*2);

            Thread.Sleep(waitTime);

            Assert.AreEqual(numTimes, sampler.Count / names.Length, 1);
        }
    }
}
