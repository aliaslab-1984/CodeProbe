using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeProbe.Sensing
{
    /// <summary>
    /// Probes the trend of a value using a pool of sample of that value.
    /// </summary>
    public class HistogramProbe : AbstractProbe
    {
        protected AbstractReservoir<IComparable> _reservoir;

        public HistogramProbe(string name, AbstractReservoir<IComparable> reservoir)
            : base(name)
        {
            _reservoir = reservoir;
        }

        /// <summary>
        /// Changes reservoir keeping the current values
        /// </summary>
        /// <param name="reservoir">new reservoir</param>
        public void ChangeReservoir(AbstractReservoir<IComparable> reservoir)
        {
            AbstractReservoir<IComparable>.Exchange(_reservoir,reservoir);
            _reservoir = reservoir;
        }

        /// <summary>
        /// Adds a sample to the configured pool.
        /// </summary>
        /// <param name="value">Value to add.</param>
        public void Update(IComparable value)
        {
            _reservoir.Update(value);
            OnSense(value);
        }

        /// <summary>
        /// Get the values of the histogram
        /// </summary>
        /// <returns>List of tuple with time of sampling (ms UTC) and value sampled.</returns>
        public override object Get()
        {
            return _reservoir.Get();
        }
    }
}
