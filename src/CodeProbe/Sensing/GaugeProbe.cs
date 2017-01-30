using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeProbe.Sensing
{
    /// <summary>
    /// Probes a single value.
    /// </summary>
    public class GaugeProbe : AbstractProbe
    {
        Func<object> _producer;

        /// <summary>
        /// Constructs a gauge.
        /// </summary>
        /// <param name="name">Probe name</param>
        /// <param name="producer">Gauge value producer.</param>
        public GaugeProbe(string name, Func<object> producer)
            : base(name)
        {
            _producer = producer;
        }

        public void ChangeProducer(Func<object> producer)
        {
            _producer = producer;
        }

        /// <summary>
        /// Get the gauge value.
        /// </summary>
        /// <returns>Value probed.</returns>
        public override object Get()
        {
            return _producer();
        }
    }
}
