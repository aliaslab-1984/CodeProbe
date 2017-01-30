using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace CodeProbe.Sensing
{
    /// <summary>
    /// Extension class, used to decorate the ProbeManager object in order to simplify probe creation and usage and keep clean the ProbeManager implementation.
    /// </summary>
    public static class SensingExtensions
    {
        #region free

        /// <summary>
        /// Get a GaugeProbe by complete name.
        /// </summary>
        /// <param name="name">Probe name</param>
        /// <returns>Required probe. Null if not found.</returns>
        public static GaugeProbe Gauge(this ProbeManager ext, string name)
        {
            AbstractProbe result = ext.GetProbe(name);
            if (result != null && typeof(GaugeProbe).IsAssignableFrom(result.GetType()))
                return (GaugeProbe)result;
            else
                return null;
        }

        /// <summary>
        /// Create a GaugeProbe with a specific name.
        /// </summary>
        /// <param name="name">Probe name</param>
        /// <param name="producer">Producer faility of the GaugeProbe.</param>
        /// <returns>The new GaugeProbe or exception if the probe already exists.</returns>
        public static GaugeProbe Gauge(this ProbeManager ext, string name, Func<object> producer)
        {
            AbstractProbe result = ext.Gauge(name);
            if (result != null)
            {
                ((GaugeProbe)result).ChangeProducer(producer);
                return (GaugeProbe)result;
            }

            result = new GaugeProbe(name, producer);
            ext.Register(result);

            return (GaugeProbe)result;
        }

        /// <summary>
        /// Create or get a CounterProbe with a specific name,
        /// </summary>
        /// <param name="name">Probe name</param>
        /// <returns>Required probe.</returns>
        public static CounterProbe Counter(this ProbeManager ext, string name)
        {
            AbstractProbe result = ext.GetProbe(name);
            if (result != null && typeof(CounterProbe).IsAssignableFrom(result.GetType()))
                return (CounterProbe)result;

            result = new CounterProbe(name);
            ext.Register(result);

            return (CounterProbe)result;
        }

        /// <summary>
        /// Create or get a MeterProbe with a specific name,
        /// </summary>
        /// <param name="name">Probe name</param>
        /// <returns>Required probe.</returns>
        public static MeterProbe Meter(this ProbeManager ext, string name)
        {
            AbstractProbe result = ext.GetProbe(name);
            if (result != null && typeof(MeterProbe).IsAssignableFrom(result.GetType()))
                return (MeterProbe)result;

            result = new MeterProbe(name);
            ext.Register(result);

            return (MeterProbe)result;
        }

        /// <summary>
        /// Get an HistogramProbe with the specified name.
        /// </summary>
        /// <param name="name">Probe name</param>
        /// <returns>Required probe or null if not found</returns>
        public static HistogramProbe Histogram(this ProbeManager ext, string name)
        {
            AbstractProbe result = ext.GetProbe(name);
            if (result != null && typeof(HistogramProbe).IsAssignableFrom(result.GetType()))
                return (HistogramProbe)result;

            result = new HistogramProbe(name, ProbeManager.Ask().GetDefaultReservoir<IComparable>());
            ext.Register(result);

            return (HistogramProbe)result;
        }

        /// <summary>
        /// Create or get an HistogramProbe with the specified name and reservoir.
        /// </summary>
        /// <param name="name">Probe name</param>
        /// <param name="reservoir">Reservoir</param>
        /// <returns>Required HistogramProbe or exception if the probe already exists.</returns>
        public static HistogramProbe Histogram(this ProbeManager ext, string name, AbstractReservoir<IComparable> reservoir)
        {
            AbstractProbe result = ext.GetProbe(name);
            if (result != null && typeof(HistogramProbe).IsAssignableFrom(result.GetType()))
            {
                ((HistogramProbe)result).ChangeReservoir(reservoir);
                return (HistogramProbe)result;
            }

            result = new HistogramProbe(name, reservoir);
            ext.Register(result);

            return (HistogramProbe)result;
        }

        /// <summary>
        /// Get a TimerProbe with the specified name.
        /// </summary>
        /// <param name="name">Probe name</param>
        /// <returns>Required probe or null if not found.</returns>
        public static TimerProbe Timer(this ProbeManager ext, string name)
        {
            AbstractProbe result = ext.GetProbe(name);
            if (result != null && typeof(TimerProbe).IsAssignableFrom(result.GetType()))
                return (TimerProbe)result;

            result = new TimerProbe(name, ProbeManager.Ask().GetDefaultReservoir<long>());
            ext.Register(result);

            return (TimerProbe)result;
        }

        /// <summary>
        /// Create or get a TimerProbe with the specified name and reservoir.
        /// </summary>
        /// <param name="name">Probe name</param>
        /// <param name="reservoir">Reservoir</param>
        /// <returns>Required probe or exception if the probe already exists.</returns>
        public static TimerProbe Timer(this ProbeManager ext, string name, AbstractReservoir<long> reservoir)
        {
            AbstractProbe result = ext.GetProbe(name);
            if (result != null && typeof(TimerProbe).IsAssignableFrom(result.GetType()))
            {
                ((TimerProbe)result).ChangeReservoir(reservoir);
                return (TimerProbe)result;
            }

            result = new TimerProbe(name, reservoir);
            ext.Register(result);

            return (TimerProbe)result;
        }

        #endregion

        #region system

        public static GaugeProbe SystemGauge(this ProbeManager ext, string name)
        {
            return ext.Gauge("system." + name);
        }

        public static GaugeProbe SystemGauge(this ProbeManager ext, string name, Func<object> producer)
        {
            return ext.Gauge("system."+name,producer);
        }

        public static CounterProbe SystemCounter(this ProbeManager ext, string name)
        {
            return ext.Counter("system."+name);
        }

        public static MeterProbe SystemMeter(this ProbeManager ext, string name)
        {
            return ext.Meter("system." + name);
        }

        public static HistogramProbe SystemHistogram(this ProbeManager ext, string name)
        {
            return ext.Histogram("system."+name);
        }

        public static HistogramProbe SystemHistogram(this ProbeManager ext, string name, AbstractReservoir<IComparable> reservoir)
        {
            return ext.Histogram("system."+name,reservoir);
        }

        public static TimerProbe SystemTimer(this ProbeManager ext, string name)
        {
            return ext.Timer("system."+name);
        }

        public static TimerProbe SystemTimer(this ProbeManager ext, string name, AbstractReservoir<long> reservoir)
        {
            return ext.Timer("system."+name, reservoir);
        }

        #endregion

        #region machine

        public static GaugeProbe MachineGauge(this ProbeManager ext, string name)
        {
            return ext.Gauge("machine." + name);
        }

        public static GaugeProbe MachineGauge(this ProbeManager ext, string name, Func<object> producer)
        {
            return ext.Gauge("machine." + name, producer);
        }

        public static CounterProbe MachineCounter(this ProbeManager ext, string name)
        {
            return ext.Counter("machine." + name);
        }

        public static MeterProbe MachineMeter(this ProbeManager ext, string name)
        {
            return ext.Meter("machine." + name);
        }

        public static HistogramProbe MachineHistogram(this ProbeManager ext, string name)
        {
            return ext.Histogram("machine." + name);
        }

        public static HistogramProbe MachineHistogram(this ProbeManager ext, string name, AbstractReservoir<IComparable> reservoir)
        {
            return ext.Histogram("machine." + name, reservoir);
        }

        public static TimerProbe MachineTimer(this ProbeManager ext, string name)
        {
            return ext.Timer("machine." + name);
        }

        public static TimerProbe MachineTimer(this ProbeManager ext, string name, AbstractReservoir<long> reservoir)
        {
            return ext.Timer("machine." + name, reservoir);
        }

        #endregion

        #region application

        public static GaugeProbe AppGauge(this ProbeManager ext, string name)
        {
            return ext.Gauge("application." + name);
        }

        public static GaugeProbe AppGauge(this ProbeManager ext, string name, Func<object> producer)
        {
            return ext.Gauge("application." + name, producer);
        }

        public static CounterProbe AppCounter(this ProbeManager ext, string name)
        {
            return ext.Counter("application." + name);
        }

        public static MeterProbe AppMeter(this ProbeManager ext, string name)
        {
            return ext.Meter("application." + name);
        }

        public static HistogramProbe AppHistogram(this ProbeManager ext, string name)
        {
            return ext.Histogram("application." + name);
        }

        public static HistogramProbe AppHistogram(this ProbeManager ext, string name, AbstractReservoir<IComparable> reservoir)
        {
            return ext.Histogram("application." + name, reservoir);
        }

        public static TimerProbe AppTimer(this ProbeManager ext, string name)
        {
            return ext.Timer("application." + name);
        }

        public static TimerProbe AppTimer(this ProbeManager ext, string name, AbstractReservoir<long> reservoir)
        {
            return ext.Timer("application." + name, reservoir);
        }

        #endregion
    }
}
