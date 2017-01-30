using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeProbe.Sensing
{
    /// <summary>
    /// Variable dimension pool. The pool change dimension in order to contain samples of a fixed time span.
    /// </summary>
    /// <typeparam name="T">Type of sample stored.</typeparam>
    public class TimedSlidingWindowReservoir<T> : SlidingAdaptiveWindowReservoir<T> where T : IComparable
    {
        protected int _seconds;

        /// <summary>
        /// Construct a TimedSlidingWindowReservoir
        /// </summary>
        /// <param name="spanSeconds">Time sizeof the pool in seconds.</param>
        public TimedSlidingWindowReservoir(int spanSeconds)
        {
            _seconds = spanSeconds;
        }

        protected override void ResizeWindow()
        {
            if (_reservoir.Count > 0)
            {
                long first = _reservoir.First().Item1;
                _maxSize = _reservoir.Count(p => p.Item1 - first <= 1000 * _seconds);
            }
        }
    }
}
