using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using System.Diagnostics;

using CodeProbe.Normalization;

namespace CodeProbe.Sensing
{
    /// <summary>
    /// Probes the current count of an event happening.
    /// </summary>
    public class CounterProbe : AbstractProbe
    {
        private long _value;

        public CounterProbe(string name):base(name)
        {
            _value=0;
        }
        
        /// <summary>
        /// Increment the counter by 1.
        /// </summary>
        public void Increment()
        {
            Interlocked.Increment(ref _value);
            OnSense(1);
        }

        /// <summary>
        /// Increment the counter by value.
        /// </summary>
        /// <param name="value">Increment units</param>
        public void Increment(int value)
        {
            if (value <= 0)
                throw new ArgumentException("Value must be positive.");
            Interlocked.Add(ref _value,value);
            OnSense(value);
        }

        /// <summary>
        /// Decrement the counter by 1.
        /// </summary>
        public void Decrement()
        {
            Interlocked.Decrement(ref _value);
            OnSense(-1);
        }

        /// <summary>
        /// Decrement the counter by value.
        /// </summary>
        /// <param name="value">Decrement units</param>
        public void Decrement(int value)
        {
            if (value <= 0)
                throw new ArgumentException("Value must be positive.");
            Interlocked.Add(ref _value,-value);
            OnSense(-value);
        }

        /// <summary>
        /// Get the current count value.
        /// </summary>
        /// <returns>Current count value.</returns>
        public override object Get()
        {
            return _value;
        }
    }
}
