using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CodeProbe.Sensing;
using CodeProbe.Reporting;
using CodeProbe.Reporting.Statistics;

namespace Test.CodeProbe.utility
{
    public class CounterSampler : AbstractSampler
    {
        public CounterSampler()
            : base("counter", new HashSet<AbstractProbeStatistic>())
        {
        }

        public int Count = 0;

        public void Begin()
        {
            
        }

        public void Complete()
        {
            
        }

        public void Abort()
        {
            
        }

        public override void Sample(AbstractProbe probe)
        {
            Count++;
        }

        public event Action<AbstractSampler, Exception> SamplingError;

        public event Action<AbstractSampler, Dictionary<string, object>> SampleTaken;

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
