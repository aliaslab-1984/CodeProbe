using CodeProbe.Sensing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeProbe.Reporting.Statistics.Gauge
{
    public class ConstantStatistic : GaugeStatisticComputer
    {
        public override string StatisticName
        {
            get { return base.StatisticName+".const.value"; }
        }

        public override Tuple<string, object> Compute(AbstractProbe probe)
        {
            try
            {
                return new Tuple<string, object>(probe.Name + "." + StatisticName, probe.Get());
            }
            catch (Exception e)
            {
                return new Tuple<string, object>(probe.Name + "." + StatisticName, -1);
            }
        }
    }
}
