using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using CodeProbe.Normalization;
using log4net;
using CodeProbe.Sensing;
using CodeProbe.Reporting.Statistics;

namespace CodeProbe.Reporting
{
    /// <summary>
    /// Sampler base class. Impements every method needed by a sampler.
    /// </summary>
    public abstract class AbstractSampler
    {
        protected ILog _logger;

        protected SortedList<string, object> _samples = new SortedList<string, object>();

        protected int _lock = 0;
        protected object _syncObj = new object();

        public object Sync { get { return _syncObj; } }

        /// <summary>
        /// Event fired on sampling error. Arguments are sampler and exception.
        /// </summary>
        public event Action<AbstractSampler, Exception> SamplingError;

        /// <summary>
        /// Fires SamplingError event in a controlled way.
        /// </summary>
        /// <param name="arg">Exception thrown.</param>
        protected void OnSamplingError(Exception arg)
        {
            if (SamplingError != null)
                SamplingError(this,arg);
        }

        /// <summary>
        /// Event fired on sampling success. Argument is  key-value pair of samples-values.
        /// </summary>
        public event Action<AbstractSampler, Dictionary<string, object>> SampleTaken;

        /// <summary>
        /// Fires SampleTaken event in a controlled way.
        /// </summary>
        /// <param name="arg">Sample taken.</param>
        protected void OnSampleTaken(Dictionary<string, object> arg)
        {
            if (SampleTaken != null)
                SampleTaken(this,arg);
        }

        protected TimerProbe.MarkTime _currentSamplerTimer = null;
        protected bool _disposed = false;

        HashSet<AbstractProbeStatistic> _stats;

        /// <summary>
        /// Sampler name.
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// Sampler name concatenated to the class full name. Used for logs and unique identification of the sampler.
        /// </summary>
        protected virtual string AuditName { get { return GetType().FullName + "." + Name; } }

        /// <summary>
        /// String representation of the sampler.
        /// </summary>
        /// <returns>AuditName</returns>
        public override string ToString()
        {
            return AuditName;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            else
                return obj.ToString()==ToString();
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        /// <summary>
        /// Default constructor. The argument order must be mainteined in derived class.
        /// Initiates and uses three default system probes:
        ///     counter->AuditName.skipped
        ///     counter->AuditName.complete
        ///     counter->AuditName.abort
        /// </summary>
        /// <param name="name">Sampler name</param>
        /// <param name="stats">applied statistics collection.</param>
        public AbstractSampler(
            string name,
            HashSet<AbstractProbeStatistic> stats
        )
        {
            Name = name;
            _stats = stats;

            _logger = LogManager.GetLogger(AuditName);
            
            ProbeManager.Ask().SystemCounter(AuditName + ".skipped").Increment();
            ProbeManager.Ask().SystemCounter(AuditName + ".skipped").Decrement();
            ProbeManager.Ask().SystemCounter(AuditName + ".completed").Increment();
            ProbeManager.Ask().SystemCounter(AuditName + ".completed").Decrement();
            ProbeManager.Ask().SystemCounter(AuditName + ".aborted").Increment();
            ProbeManager.Ask().SystemCounter(AuditName + ".aborted").Decrement();
            ProbeManager.Ask().SystemTimer(AuditName, ProbeManager.Ask().GetDefaultReservoir<long>());
        }

        /// <summary>
        /// Begins the sampling procedure.
        /// </summary>
        /// <returns>true on success</returns>
        public bool Begin()
        {
            if (_disposed)
                throw new InvalidOperationException("Object disposed.");

            if (Interlocked.Exchange(ref _lock, 1) == 0)
            {
                _currentSamplerTimer = ProbeManager.Ask().SystemTimer(AuditName).Mark();

                _samples.Clear();
                return true;
            }
            else
            {
                ProbeManager.Ask().SystemCounter(AuditName + ".skipped").Increment();
                return false;
            }
        }

        /// <summary>
        /// Complete the sampling procedure, firing SampleTaken event.
        /// </summary>
        public void Complete()
        {
            if (_disposed)
                throw new InvalidOperationException("Object disposed.");
            if (_lock == 0)
                throw new InvalidOperationException("Unable to complete an unstarted operation.");
            try
            {
                OnSampleTaken(_samples.ToDictionary(p=>p.Key,p=>p.Value));
            
                _currentSamplerTimer(false);
                _currentSamplerTimer = null;
                ProbeManager.Ask().SystemCounter(AuditName + ".completed").Increment();
            }
            catch(Exception e)
            {
                OnSamplingError(e);
                _logger.Debug("Error completing sample.",e);
            }
            finally
            {
                Interlocked.Exchange(ref _lock, 0);
            }
        }

        /// <summary>
        /// Abort sampling procedure.
        /// </summary>
        public void Abort()
        {
            if (_disposed)
                throw new InvalidOperationException("Object disposed.");
            if (_lock == 0)
                throw new InvalidOperationException("Unable to abort an unstarted operation.");
            try
            {
                if (_currentSamplerTimer != null)
                {
                    _currentSamplerTimer(true);
                    _currentSamplerTimer = null;
                }
                ProbeManager.Ask().SystemCounter(AuditName + ".aborted").Increment();
            }
            finally
            {
                Interlocked.Exchange(ref _lock, 0);
            }
        }

        /// <summary>
        /// Performs the sampling operation on a single probe.
        /// </summary>
        /// <param name="probe">Probe to sample.</param>
        public virtual void Sample(Sensing.AbstractProbe probe)
        {
            if (_disposed)
                throw new InvalidOperationException("Object disposed.");
            if (_lock == 0)
                throw new InvalidOperationException("Unable to execute the operation without locking.");

            if (probe is Sensing.CounterProbe)
            {
                Sample(probe as Sensing.CounterProbe);
            }
            else if (probe is Sensing.GaugeProbe)
            {
                Sample(probe as Sensing.GaugeProbe);
            }
            else if (probe is Sensing.HistogramProbe)
            {
                Sample(probe as Sensing.HistogramProbe);
            }
            else if (probe is Sensing.MeterProbe)
            {
                Sample(probe as Sensing.MeterProbe);
            }
            else if (probe is Sensing.TimerProbe)
            {
                Sample(probe as Sensing.TimerProbe);
            }
            else
            {
                if (!_samples.Keys.Contains(probe.Name))
                    _samples.Add(probe.Name, probe.Get());
            }
        }

        #region protected sample methods
        
        protected virtual void Sample(Sensing.GaugeProbe probe)
        {
            Tuple<string, object> sample;
            foreach (AbstractProbeStatistic item in _stats.Where(p=>p.ProbeType.IsAssignableFrom(typeof(GaugeProbe))))
            {
                sample = item.Compute(probe);
                if (!_samples.Keys.Contains(sample.Item1))
                    _samples.Add(sample.Item1, sample.Item2);   
            }
        }

        protected virtual void Sample(Sensing.CounterProbe probe)
        {
            Tuple<string, object> sample;
            foreach (AbstractProbeStatistic item in _stats.Where(p => p.ProbeType.IsAssignableFrom(typeof(CounterProbe))))
            {
                sample = item.Compute(probe);
                if (!_samples.Keys.Contains(sample.Item1))
                    _samples.Add(sample.Item1, sample.Item2);
            }
        }

        protected virtual void Sample(Sensing.MeterProbe probe)
        {
            Tuple<string, object> sample;
            foreach (AbstractProbeStatistic item in _stats.Where(p => p.ProbeType.IsAssignableFrom(typeof(MeterProbe))))
            {
                sample = item.Compute(probe);
                if (!_samples.Keys.Contains(sample.Item1))
                    _samples.Add(sample.Item1, sample.Item2);
            }
        }

        protected virtual void Sample(Sensing.HistogramProbe probe)
        {
            Tuple<string, object> sample;
            foreach (AbstractProbeStatistic item in _stats.Where(p => p.ProbeType.IsAssignableFrom(typeof(HistogramProbe))))
            {
                sample = item.Compute(probe);
                if (!_samples.Keys.Contains(sample.Item1))
                    _samples.Add(sample.Item1, sample.Item2);
            }
        }

        protected virtual void Sample(Sensing.TimerProbe probe)
        {
            Tuple<string, object> sample;
            foreach (AbstractProbeStatistic item in _stats.Where(p => p.ProbeType.IsAssignableFrom(typeof(TimerProbe))))
            {
                sample = item.Compute(probe);
                if (!_samples.Keys.Contains(sample.Item1))
                    _samples.Add(sample.Item1, sample.Item2);
            }
        }

        #endregion

        public void Dispose()
        {
            if(!_disposed)
            {
                if (_lock==1)
                {
                    Abort();
                }

                _samples.Clear();
                _samples = null;

                _disposed=true;
            }
        }
    }
}
