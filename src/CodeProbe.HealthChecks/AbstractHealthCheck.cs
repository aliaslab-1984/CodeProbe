using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeProbe.HealthChecks
{
    /// <summary>
    /// Base HealthCheck class
    /// </summary>
    public abstract class AbstractHealthCheck
    {
        /// <summary>
        /// Default constructor. The argument order must be mainteined in derived class.
        /// </summary>
        /// <param name="name">HealthCheck name</param>
        public AbstractHealthCheck(string name)
        {
            Name = name;
        }

        /// <summary>
        /// HealthCheck name
        /// </summary>
        public string Name { get; protected set; }
        
        /// <summary>
        /// Severity of the check. The grater the worst.
        /// </summary>
        public abstract int Severity { get; }

        /// <summary>
        /// Check implementation.
        /// </summary>
        /// <returns>true if the check is successfull, false if it fails, null if is indeterminable.</returns>
        public abstract bool? Check();
    }
}
