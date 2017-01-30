using CodeProbe.Normalization;
using CodeProbe.Sensing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeProbe.Reporting.Statistics.Timer
{
    public class LinearMaxStatistic : TimerStatisticComputer
    {
        public override string StatisticName
        {
            get { return base.StatisticName+".lin_ms.max"; }
        }

        public override Tuple<string, object> Compute(AbstractProbe probe)
        {
            try
            {
                Tuple<double, List<Tuple<long, long>>> probeValue = (Tuple<double, List<Tuple<long, long>>>)probe.Get();
                return new Tuple<string, object>(probe.Name + "." + StatisticName, TimeUnit.ToMilliseconds(probeValue.Item2.Max(p => p.Item2)));
            }
            catch (Exception e)
            {
                return new Tuple<string, object>(probe.Name + "." + StatisticName, -1);
            }
        }
    }
}
