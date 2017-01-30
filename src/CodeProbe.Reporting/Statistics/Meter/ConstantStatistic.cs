using CodeProbe.Normalization;
using CodeProbe.Sensing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeProbe.Reporting.Statistics.Meter
{
    public class ConstantStatistic : MeterStatisticComputer
    {
        public override string StatisticName
        {
            get { return base.StatisticName+".const_ms.value"; }
        }

        public override Tuple<string, object> Compute(AbstractProbe probe)
        {
            try
            {
                return new Tuple<string, object>(probe.Name + "." + StatisticName, TimeUnit.ToEventsPerMillisecond(Convert.ToDecimal(probe.Get())));
            }
            catch (Exception e)
            {
                return new Tuple<string, object>(probe.Name + "." + StatisticName, -1);
            }
        }
    }
}
