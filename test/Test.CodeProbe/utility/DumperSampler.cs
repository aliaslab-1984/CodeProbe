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
    public class DumperSampler : AbstractSampler
    {
        public DumperSampler()
            : base("dumpler", new HashSet<AbstractProbeStatistic>())
        {
        }

        public List<string> NameDump = new List<string>();

        protected void _Sample(AbstractProbe probe)
        {
            NameDump.Add(probe.Name);
        }

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
            _Sample(probe);
        }

        public event Action<AbstractSampler, Exception> SamplingError;

        public event Action<AbstractSampler, Dictionary<string, object>> SampleTaken;

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
