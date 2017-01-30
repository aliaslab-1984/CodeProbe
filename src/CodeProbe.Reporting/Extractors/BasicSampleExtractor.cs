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
    /// Extractor that cause a sampling action on each sampler.
    /// </summary>
    public class BasicSampleExtractor : AbstractSampleExtractor
    {
        protected ConcurrentDictionary<AbstractSampler, string> _samplers = new ConcurrentDictionary<AbstractSampler, string>();

        protected long _period;

        public BasicSampleExtractor(string name)
            : base(name)
        {
        }

        public override void Extract()
        {
            Parallel.ForEach(_samplers, new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount }, kv =>
            {
                AbstractSampler sampler = kv.Key;
                bool began = false;
                try
                {
                    if (began = sampler.Begin())
                    {
                        ConcurrentBag<AbstractProbe> probes = new ConcurrentBag<AbstractProbe>(ProbeManager.Ask().MatchFilter(kv.Value));
                        ParallelLoopResult result = Parallel.ForEach(probes,
                            new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount },
                            (item, loopState) =>
                            {
                                try
                                {
                                    sampler.Sample(item);
                                }
                                catch (Exception e)
                                {
                                    loopState.Break();
                                    _logger.Debug(string.Format("Error sampling."), e);
                                    throw e;
                                }
                            }
                        );

                        if (!result.IsCompleted)
                            throw new Exception(string.Format("Loop stopped at: {0}", result.LowestBreakIteration));

                        sampler.Complete();
                    }
                }
                catch (Exception e)
                {
                    if (began)
                        sampler.Abort();
                    _logger.Debug(string.Format("Error sampling for sampler: {0}", sampler.Name), e);
                }
            });
        }

        public override void AddSampler(AbstractSampler sampler)
        {
            if (_disposed)
                throw new InvalidOperationException("Object disposed.");
            AddSampler(sampler, ".*");
        }

        public override void AddSampler(AbstractSampler sampler, string filter)
        {
            if (_disposed)
                throw new InvalidOperationException("Object disposed.");
            if (string.IsNullOrEmpty(filter))
                throw new ArgumentNullException("filter must be not null and not empty.");

            if (!_samplers.ContainsKey(sampler))
                _samplers.AddOrUpdate(sampler, filter, (k, v) => v);
        }

        public override void RemoveSampler(AbstractSampler sampler)
        {
            if (_disposed)
                throw new InvalidOperationException("Object disposed.");
            string tmp;
            _samplers.TryRemove(sampler, out tmp);
            tmp = null;
        }

        public override void Clear()
        {
            if (_disposed)
                throw new InvalidOperationException("Object disposed.");
            _samplers.Clear();
        }

        protected bool _disposed = false;
        public override void Dispose()
        {
            if (!_disposed)
            {
                Clear();
                _disposed = true;
            }
        }
    }
}
