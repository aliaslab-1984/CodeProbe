﻿using CodeProbe.Sensing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeProbe.Reporting.Statistics.Histogram
{
    public class LinearMaxStatistic : HistogramStatisticComputer
    {
        public override string StatisticName
        {
            get { return base.StatisticName+".lin.max"; }
        }

        public override Tuple<string, object> Compute(AbstractProbe probe)
        {
            try
            {
                List<Tuple<long, IComparable>> probeValue = (List<Tuple<long, IComparable>>)probe.Get();
                return new Tuple<string, object>(probe.Name + "." + StatisticName, probeValue.Max(p => p.Item2));
            }
            catch (Exception e)
            {
                return new Tuple<string, object>(probe.Name + "." + StatisticName, -1);
            }
        }
    }
}
