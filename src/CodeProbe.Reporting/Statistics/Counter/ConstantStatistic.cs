using CodeProbe.Sensing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeProbe.Reporting.Statistics.Counter
{
    public class ConstantStatistic : CounterStatisticComputer
    {
        public override string StatisticName
        {
            get { return base.StatisticName+".const.value"; }
        }

        public override Tuple<string, object> Compute(AbstractProbe probe)
        {
            try
            {
                return new Tuple<string, object>(probe.Name + "." + StatisticName, Convert.ToInt64(probe.Get()));
            }
            catch (Exception e)
            {
                return new Tuple<string, object>(probe.Name + "." + StatisticName, -1);
            }
        }
    }
}
