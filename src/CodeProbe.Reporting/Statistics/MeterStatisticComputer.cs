using CodeProbe.Sensing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeProbe.Reporting.Statistics
{
    public abstract class MeterStatisticComputer : AbstractProbeStatistic
    {
        public override Type ProbeType { get { return typeof(MeterProbe); } }

        public override string StatisticName { get { return "meter"; } }
    }
}
