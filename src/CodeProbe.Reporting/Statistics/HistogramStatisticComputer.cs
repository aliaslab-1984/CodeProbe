using CodeProbe.Sensing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeProbe.Reporting.Statistics
{
    public abstract class HistogramStatisticComputer : AbstractProbeStatistic
    {
        public override Type ProbeType { get { return typeof(HistogramProbe); } }

        public override string StatisticName { get { return "histogram"; } }
    }
}
