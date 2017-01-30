using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeProbe.Sensing
{
    /// <summary>
    /// Base probe class
    /// </summary>
    public abstract class AbstractProbe
    {
        /// <summary>
        /// Probe name.
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// Default constructor. The argument order must be mainteined in derived class.
        /// </summary>
        /// <param name="name">Probe name</param>
        public AbstractProbe(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Event that notifies the sensing of a value by the probe. The arguments are the probe and the sensed value.
        /// </summary>
        public event Action<AbstractProbe, object> Sense;

        /// <summary>
        /// Fires a Sense event in a controlled way, with no value.
        /// </summary>
        protected void OnSense()
        {
            if (Sense != null)
                Sense(this,null);
        }

        /// <summary>
        /// Fires a Sense event in a controlled way.
        /// </summary>
        /// <param name="args">Sensed value.</param>
        protected void OnSense(object args)
        {
            if (Sense != null)
                Sense(this,args);
        }

        /// <summary>
        /// Get the current probe value.
        /// </summary>
        /// <returns>Current probe value.</returns>
        public abstract object Get();
    }
}
