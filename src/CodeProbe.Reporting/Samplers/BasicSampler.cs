using CodeProbe.Reporting.Statistics;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeProbe.Reporting.Samplers
{
    /// <summary>
    /// Basic sampler implementation. Uses base class defaults.
    /// </summary>
    public class BasicSampler : AbstractSampler
    {
        public BasicSampler(string name, HashSet<AbstractProbeStatistic> stats)
            : base(name, stats)
        {
        }
    }
}
