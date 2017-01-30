using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using System.Diagnostics;
using CodeProbe.Normalization;

namespace CodeProbe.Sensing
{
    /// <summary>
    /// Probes the rattio of happening of an event.
    /// </summary>
    public class MeterProbe : AbstractProbe
    {
        private long _value;
        private Stopwatch _watch;

        public MeterProbe(string name)
            : base(name)
        {
            _value=0;
            _watch = Stopwatch.StartNew();
        }

        /// <summary>
        /// Marks the happening of the probed event.
        /// </summary>
        public void Mark()
        {
            Interlocked.Increment(ref _value);
            OnSense();
        }

        /// <summary>
        /// Events per machine tick
        /// </summary>
        /// <returns>Events per machine tick</returns>
        public override object Get()
        {
            try
            {
                decimal x = (decimal)Interlocked.Exchange(ref _value, 0);
                long y = _watch.ElapsedTicks;
                return (double)(x / ( y == 0 ? (decimal)Double.Epsilon : y ));
            }
            finally
            {
                _watch.Restart();
            }
        }
    }
}
