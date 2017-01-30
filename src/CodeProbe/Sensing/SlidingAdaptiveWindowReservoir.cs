using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeProbe.Sensing
{
    /// <summary>
    /// Base class of a variable dimension pool.
    /// </summary>
    /// <typeparam name="T">Type of sample stored.</typeparam>
    public abstract class SlidingAdaptiveWindowReservoir<T> : SlidingWindowReservoir<T> where T : IComparable
    {
        public SlidingAdaptiveWindowReservoir()
            :base(100)
        {
        }

        /// <summary>
        /// Allows to specify custom policies to resize the pool.
        /// </summary>
        protected abstract void ResizeWindow();

        public override void Update(T value)
        {
            ResizeWindow();

            Tuple<long, T> tmp;
            while (_reservoir.Count() >= _maxSize)
                _reservoir.TryDequeue(out tmp);

            base.Update(value);
        }
    }
}
