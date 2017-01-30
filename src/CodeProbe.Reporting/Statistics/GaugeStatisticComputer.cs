using CodeProbe.Sensing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeProbe.Reporting.Statistics
{
    public abstract class GaugeStatisticComputer : AbstractProbeStatistic
    {
        public override Type ProbeType { get { return typeof(GaugeProbe); } }

        public override string StatisticName { get { return "gauge"; } }
    }
}
