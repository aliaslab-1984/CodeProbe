using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeProbe.Sensing
{
    /// <summary>
    /// Fixed dimension pool.
    /// </summary>
    /// <typeparam name="T">Type of sample stored.</typeparam>
    public class SlidingWindowReservoir<T> : AbstractReservoir<T> where T : IComparable
    {
        protected int _maxSize;

        /// <summary>
        /// Construct a SlidingWindowReservoir
        /// </summary>
        /// <param name="size">Fixed size of the pool in sample count.</param>
        public SlidingWindowReservoir(int size):base()
        {
            if (size < 1)
                throw new ArgumentException("Size must be at least 1.");
            _maxSize = size;
        }

        public override void Update(T value)
        {
            if (_reservoir.Count() >= _maxSize)
            {
                Tuple<long,T> tmp;
                _reservoir.TryDequeue(out tmp);
            }
            base.Update(value);
        }
    }
}
