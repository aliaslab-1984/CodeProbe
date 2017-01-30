using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeProbe.Reporting.Reporters
{
    /// <summary>
    /// Reporters that sets a dictionary of key-value pairs containg the name-value tuples of a sample.
    /// </summary>
    public class DictionaryReporter : AbstractReporter
    {
        public DictionaryReporter(string name, AbstractSampler sampler) :
            base(name,sampler)
        {
            _sampler.SampleTaken += OnSampleTaken;
            _sampler.SamplingError += OnSamplingError;
        }
        
        protected void OnSampleTaken(AbstractSampler sampler, Dictionary<string, object> sample)
        {
            if (_started)
            {
                CurrentSample = sample.ToDictionary(p=>p.Key,p=>p.Value);
            }
            else
            {
                _logger.Debug("The reporter is not started, skipping sample.");
            }
        }

        protected void OnSamplingError(AbstractSampler sampler, Exception e)
        {
            _logger.Debug(string.Format("Error sampling data. started={0}",_started),e);
        }

        /// <summary>
        /// Return the last sampled sample.
        /// </summary>
        public Dictionary<string, object> CurrentSample { get; protected set; }

        public override void Dispose()
        {
            _sampler.SampleTaken -= OnSampleTaken;
            _sampler.SamplingError -= OnSamplingError;

            base.Dispose();
        }
    }
}
