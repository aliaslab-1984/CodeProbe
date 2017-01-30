using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Linq;

using CodeProbe.Sensing;
using System.Collections.Generic;
using System.Diagnostics;

namespace Test.CodeProbe
{
    [TestClass]
    public class ProbeTest
    {
        /// <summary>
        /// Test a gauge
        /// </summary>
        [TestCategory("Sensing-Probe")]
        [TestMethod]
        public void Gauge()
        {
            string name = "prova";
            int value = 10;

            GaugeProbe probe = new GaugeProbe(name, () => value);

            Assert.AreEqual(name, probe.Name);
            Assert.AreEqual(value, probe.Get());
        }

        /// <summary>
        /// Test a counter
        /// </summary>
        [TestCategory("Sensing-Probe")]
        [TestMethod]
        public void Counter()
        {
            string name = "prova";

            CounterProbe probe = new CounterProbe(name);

            Assert.AreEqual(name, probe.Name);
            Assert.AreEqual(0L, probe.Get());
            probe.Increment();
            probe.Increment();
            Assert.AreEqual(2L, probe.Get());
            probe.Decrement();
            Assert.AreEqual(1L, probe.Get());
        }

        /// <summary>
        /// Test a meter.
        /// Verify confidence on a 3 sample of nearly 1/10ms frequency, with 1/1ms tolerance
        /// </summary>
        [TestCategory("Sensing-Probe")]
        [TestMethod]
        public void Meter()
        {
            string name = "prova";
            int interval_ms = 10;

            MeterProbe probe = new MeterProbe(name);

            Assert.AreEqual(name, probe.Name);

            Assert.AreEqual(0, (double)probe.Get(), 0);

            probe.Mark();
            Thread.Sleep(interval_ms);
            Thread.Sleep(interval_ms);
            probe.Mark();
            probe.Mark();
            Thread.Sleep(interval_ms);

            double delta = 1d / (Stopwatch.Frequency / 1000);
            double actual = (double)probe.Get();
            double expected = 1d / (interval_ms * (Stopwatch.Frequency/1000));

            Assert.AreEqual(expected, actual, delta);
        }

        /// <summary>
        /// Test an histogram.
        /// Verify histogram on a 10 value sample, using a SlidingWindowReservoir[3]
        /// </summary>
        [TestCategory("Sensing-Probe")]
        [TestMethod]
        public void Histogram()
        {
            string name = "prova";
            double delta = 1;
            int[] values = new int[]{1,1,1,1,2,3,3,3,4,4};

            HistogramProbe probe = new HistogramProbe(name,new SlidingWindowReservoir<IComparable>(3));

            Assert.AreEqual(name, probe.Name);

            foreach (int item in values)
            {
                probe.Update(item);
            }

            List<Tuple<long, IComparable>> result = (List<Tuple<long, IComparable>>)probe.Get();

            Assert.AreEqual(3,result[0].Item2);
            Assert.AreEqual(2, result.Count(p => (int)p.Item2 == 4));
            Assert.AreEqual(4, result[2].Item2);
            Assert.AreEqual(1, result.Count(p => (int)p.Item2 == 3));
        }

        /// <summary>
        /// Test a timer.
        /// Verify confidence on a 4 sample wait times, with 1/1ms tolerance
        /// </summary>
        [TestCategory("Sensing-Probe")]
        [TestMethod]
        public void Timer()
        {
            string name = "prova";
            double delta = 1d / (Stopwatch.Frequency / 1000);
            int[] values = new int[] { 20,20,10,10 };

            TimerProbe probe = new TimerProbe(name, new SlidingWindowReservoir<long>(3));

            Assert.AreEqual(name, probe.Name);
            
            probe.Get();
            int sum = 0;
            foreach (int item in values)
            {
                probe.Time(()=>Thread.Sleep(item));
                sum += item;
            }

            Tuple<double, List<Tuple<long, long>>> result = (Tuple<double, List<Tuple<long, long>>>)probe.Get();

            Assert.AreEqual(4d / (values.Sum() * (Stopwatch.Frequency / 1000)), result.Item1, delta);

            Assert.AreEqual(10 * (Stopwatch.Frequency / 1000), result.Item2[2].Item2, 2 * (Stopwatch.Frequency / 1000));
        }


        /// <summary>
        /// Test a timer.
        /// Verify confidence on a 4 sample wait times, with 1/1ms tolerance and als sensing events.
        /// </summary>
        [TestCategory("Sensing-Probe")]
        [TestMethod]
        public void TimerEvent()
        {
            string name = "prova";
            double delta = 1d / (Stopwatch.Frequency / 1000);
            int[] values = new int[] { 20, 20, 10, 10 };

            TimerProbe probe = new TimerProbe(name, new SlidingWindowReservoir<long>(3));

            Assert.AreEqual(name, probe.Name);

            Action<AbstractProbe,object> hdl=(o, p) =>
            {
                double t = (double)(long)p/System.Diagnostics.Stopwatch.Frequency;
                Assert.AreEqual(1, t, 0.1);
            };

            probe.Sense += hdl;

            var x=probe.Mark();

            Thread.Sleep(1000);

            x(false);
            probe.Sense -= hdl;

            int sum = 0;
            probe.Get();
            foreach (int item in values)
            {
                probe.Time(() => Thread.Sleep(item));
                sum += item;
            }

            Tuple<double, List<Tuple<long, long>>> result = (Tuple<double, List<Tuple<long, long>>>)probe.Get();

            Assert.AreEqual(4d / (values.Sum() * (Stopwatch.Frequency / 1000)), result.Item1, delta);

            Assert.AreEqual(10 * (Stopwatch.Frequency / 1000), result.Item2[2].Item2, 2 * (Stopwatch.Frequency / 1000));
        }
    }
}
