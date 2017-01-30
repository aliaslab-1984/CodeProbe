using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeProbe.Reporting
{
    /// <summary>
    /// Extractor base class.
    /// </summary>
    public abstract class AbstractReporter : IDisposable
    {
        public string Name { get; protected set; }

        protected bool _started;

        protected AbstractSampler _sampler;

        /// <summary>
        /// Default constructor. The argument order must be mainteined in derived class.
        /// </summary>
        /// <param name="name">Reproter name</param>
        /// <param name="sampler">Sampler used</param>
        public AbstractReporter(string name, AbstractSampler sampler)
        {
            Name = name;
            _sampler = sampler;

            _logger = LogManager.GetLogger(AuditName);
        }

        /// <summary>
        /// Starts reporting opeartions.
        /// </summary>
        public virtual void Start()
        {
            _started = true;
        }

        /// <summary>
        /// Stops reporting opeartions.
        /// </summary>
        public virtual void Stop()
        {
            _started = false;
        }

        public virtual void Dispose()
        {
            _sampler.Dispose();
            _sampler = null;
        }

        protected ILog _logger;

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
    }
}
