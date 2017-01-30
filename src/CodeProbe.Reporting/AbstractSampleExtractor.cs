using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeProbe.Reporting
{
    /// <summary>
    /// Base Extractor class
    /// </summary>
    public abstract class AbstractSampleExtractor : IDisposable
    {
        protected ILog _logger;

        public string Name { get; protected set; }

        /// <summary>
        /// Default constructor. The argument order must be mainteined in derived class.
        /// </summary>
        /// <param name="name">Extractor name</param>
        public AbstractSampleExtractor(string name)
        {
            Name = name;
            _logger = LogManager.GetLogger(AuditName);
        }

        /// <summary>
        /// Causes the extraction of a sample by each managed sampler.
        /// </summary>
        public abstract void Extract();

        /// <summary>
        /// Add a sampler sampling from every probe.
        /// </summary>
        /// <param name="sampler">sampler to add</param>
        public abstract void AddSampler( AbstractSampler sampler);

        /// <summary>
        /// Add a sampler sampling from the probes with a name matching the regex filter.
        /// </summary>
        /// <param name="sampler">sampler to add</param>
        /// <param name="filter">regex filter</param>
        public abstract void AddSampler(AbstractSampler sampler, string filter);

        /// <summary>
        /// Removes a sampler.
        /// </summary>
        /// <param name="sampler">sampler to remove</param>
        public abstract void RemoveSampler(AbstractSampler sampler);

        /// <summary>
        /// Removes every sampler.
        /// </summary>
        public abstract void Clear();

        /// <summary>
        /// Extractor name concatenated to the class full name. Used for logs and unique identification of the extractor.
        /// </summary>
        protected virtual string AuditName { get { return GetType().FullName + "." + Name; } }

        public override string ToString()
        {
            return AuditName;
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

        public abstract void Dispose();
    }
}
