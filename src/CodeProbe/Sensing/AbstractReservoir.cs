using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeProbe.Sensing
{
    public abstract class AbstractReservoir<T> where T : IComparable
    {
        protected ConcurrentQueue<Tuple<long,T>> _reservoir;

        public AbstractReservoir()
        {
            _reservoir = new ConcurrentQueue<Tuple<long, T>>();
        }

        /// <summary>
        /// Moves the values from one reservoir to another
        /// </summary>
        /// <param name="from">source reservoir</param>
        /// <param name="to">destination reservoir</param>
        public static void Exchange(AbstractReservoir<T> from, AbstractReservoir<T> to)
        {
            to._reservoir = new ConcurrentQueue<Tuple<long, T>>();
            foreach (Tuple<long, T> item in from.Get())
            {
                to._reservoir.Enqueue(item);
            }
        }

        public virtual List<Tuple<long, T>> Get()
        {
            return _reservoir.ToList();
        }

        public virtual void Update(T value)
        {
            _reservoir.Enqueue(new Tuple<long, T>(DateTime.Now.ToFileTimeUtc(),value));
        }
    }
}
