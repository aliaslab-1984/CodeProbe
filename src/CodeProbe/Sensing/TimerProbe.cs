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
    /// Probes the duration and the rate of happening of an event within a pool of values.
    /// </summary>
    public class TimerProbe : AbstractProbe
    {
        #region nested types

        /// <summary>
        /// Utility class, that allows Dispose pattern to delimitate the probed event scope.
        /// </summary>
        protected class TimeMarker : IDisposable
        {
            MarkTime _mk;
            public TimeMarker(TimerProbe probe)
            {
                _mk = probe.Mark();
            }

            public void Dispose()
            {
                _mk(false);
            }
        }

        #endregion

        /// <summary>
        /// Delegate that mark the end of an event.
        /// </summary>
        /// <param name="abort">true to abort probing for this instance of the event.</param>
        public delegate void MarkTime(bool abort);

        protected AbstractReservoir<long> _reservoir;

        private long _value;
        private Stopwatch _watch;

        /// <summary>
        /// Constructs a TimerProbe
        /// </summary>
        /// <param name="name">Probe name</param>
        /// <param name="reservoir">Reservoir that stores and manages samples.</param>
        public TimerProbe(string name, AbstractReservoir<long> reservoir)
            : base(name)
        {
            _reservoir = reservoir;

            _value = 0;
            _watch = Stopwatch.StartNew();
        }

        /// <summary>
        /// Changes reservoir keeping the current values
        /// </summary>
        /// <param name="reservoir">new reservoir</param>
        public void ChangeReservoir(AbstractReservoir<long> reservoir)
        {
            AbstractReservoir<long>.Exchange(_reservoir, reservoir);
            _reservoir = reservoir;
        }

        /// <summary>
        /// Allows Dispose pattern to mark event scope
        /// </summary>
        /// <returns>Returns a TimeMarker instance for the current TimerProbe.</returns>
        public IDisposable Time()
        {
            return new TimeMarker(this);
        }

        /// <summary>
        /// Allows colsure pattern to mark event scope.
        /// </summary>
        /// <param name="procedure">Function that enclose the event scope.</param>
        public void Time(Action procedure)
        {
            MarkTime tmp = Mark();
            try
            {
                procedure();
            }
            finally
            {
                tmp(false);
            }
        }

        /// <summary>
        /// Allows colsure pattern to mark event scope, with a result return.
        /// </summary>
        /// <typeparam name="T">Type of the returned result.</typeparam>
        /// <param name="procedure">Function that enclose the event scope.</param>
        /// <returns>Result produced by the event scope.</returns>
        public T Time<T>(Func<T> procedure)
        {
            MarkTime tmp = Mark();
            try
            {
                return procedure();
            }
            finally
            {
                tmp(false);
            }
        }

        /// <summary>
        /// Produces a delegate that start the event scope and is used to close the event scope on call.
        /// </summary>
        /// <returns>MarkTime delegate relative to the currently started event scope.</returns>
        public MarkTime Mark()
        {
            Interlocked.Increment(ref _value);

            Stopwatch tmp = Stopwatch.StartNew();
            
            return (bool abort) => {
                tmp.Stop();
                if(!abort)
                    Update(tmp.ElapsedTicks);
            };
        }

        /// <summary>
        /// On event completion registers the sampled data.
        /// </summary>
        /// <param name="elapsedTicks"></param>
        public void Update(long elapsedTicks)
        {
            _reservoir.Update(elapsedTicks);
            OnSense(elapsedTicks);
        }
        
        /// <summary>
        /// Tuple: events per machine tick, event duration (machine ticks) histogram
        /// </summary>
        /// <returns>Tuple: events per machine tick, event duration (machine ticks) histogram</returns>
        public override object Get()
        {
            try
            {
                decimal x = (decimal)Interlocked.Exchange(ref _value, 0);
                long y = _watch.ElapsedTicks;
                double i1 = (double)(x / (y == 0 ? (decimal)Double.Epsilon : y));
                return new Tuple<double, List<Tuple<long,long>>>(i1, _reservoir.Get());
            }
            finally
            {
                _watch.Restart();
            }
        }
    }
}
