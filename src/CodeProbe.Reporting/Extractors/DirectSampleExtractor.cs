using CodeProbe.Sensing;
using log4net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeProbe.Reporting.Extractors
{
    /// <summary>
    /// Extractor that manages a single sampler and expose directly the samples sampled at each extract operation.
    /// </summary>
    public class DirectSampleExtractor : BasicSampleExtractor
    {
        public DirectSampleExtractor(string name)
            : base(name)
        {
        }

        protected Dictionary<string,object> _sample = new Dictionary<string,object>();

        /// <summary>
        /// Current sample extracted.
        /// </summary>
        public Dictionary<string, object> Current { get { lock (_sample) { return _sample.ToDictionary(p => p.Key, p => p.Value); } } }

        protected void SampleCallabck(AbstractSampler s, Dictionary<string, object> v)
        {
            lock (_sample)
            {
                _sample.Clear();
                _sample = v;
            }
        }

        public void Extract(string filter)
        {
            KeyValuePair<AbstractSampler, string> kv = _samplers.First();
            AbstractSampler sampler = kv.Key;
            bool began = false;
            sampler.SampleTaken += SampleCallabck;
            try
            {
                if (began = sampler.Begin())
                {
                    foreach (AbstractProbe item in ProbeManager.Ask().MatchFilter(filter))
                    {
                        sampler.Sample(item);
                    }
                    sampler.Complete();
                }
            }
            catch (Exception e)
            {
                if (began)
                    sampler.Abort();
                _logger.Debug(string.Format("Error sampling for sampler: {0}", sampler.Name), e);
            }
            finally
            {
                sampler.SampleTaken -= SampleCallabck;
            }
        }

        public override void Extract()
        {
            KeyValuePair<AbstractSampler, string> kv = _samplers.First();
            Extract(kv.Value);
        }

        public override void AddSampler(AbstractSampler sampler, string filter)
        {
            if (_samplers.Count == 0)
                base.AddSampler(sampler, filter);
            else
                throw new InvalidOperationException("This extractor can have only one sampler assigned.");
        }
    }
}
