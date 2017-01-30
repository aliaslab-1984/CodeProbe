using CodeProbe.Sensing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeProbe.Reporting.Statistics
{
    /// <summary>
    /// Probes statistic base class.
    /// </summary>
    public abstract class AbstractProbeStatistic
    {
        public abstract Type ProbeType { get; }

        public abstract string StatisticName { get; }

        public abstract Tuple<string, object> Compute(AbstractProbe probe);

        public override string ToString()
        {
            return string.Format("{0}.{1}[{2}]", GetType().FullName, StatisticName, ProbeType.FullName);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            else
                return obj.ToString() == ToString();
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
    }
}
