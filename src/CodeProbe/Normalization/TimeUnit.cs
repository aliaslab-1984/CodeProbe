using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace CodeProbe.Normalization
{
    /// <summary>
    /// Helper class for time conversion, in order to get values indipendent from machine tick.
    /// </summary>
    public class TimeUnit
    {
        /// <summary>
        /// Constant dependent by machine tick.
        /// </summary>
        public static long UnitPerSecond { get { return Stopwatch.Frequency; } }

        /// <summary>
        /// Converts machine ticks to milliseconds.
        /// </summary>
        /// <param name="units">ticks</param>
        /// <returns>milliseconds</returns>
        public static double ToMilliseconds(long units)
        {
            return (double)((decimal)units * 1000 / UnitPerSecond);
        }

        /// <summary>
        /// Converts machine ticks to milliseconds.
        /// </summary>
        /// <param name="units">ticks</param>
        /// <returns>milliseconds</returns>
        public static double ToMilliseconds(decimal units)
        {
            return (double)((decimal)units * 1000 / UnitPerSecond);
        }

        /// <summary>
        /// Converts ratio from events per machine ticks to events per milliseconds.
        /// </summary>
        /// <param name="eventsPerMT">Events per machine ticks</param>
        /// <returns>Events per milliseconds</returns>
        public static double ToEventsPerMillisecond(decimal eventsPerMT)
        {
            return (double)((decimal)eventsPerMT * UnitPerSecond / 1000);
        }
    }
}
