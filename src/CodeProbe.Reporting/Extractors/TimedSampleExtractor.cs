using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using CodeProbe;
using CodeProbe.Sensing;
using log4net;

namespace CodeProbe.Reporting.Extractors
{
    /// <summary>
    /// Extractor that cause a sampling action at a fixed period of time.
    /// </summary>
    public class TimedSampleExtractor : BasicSampleExtractor
    {
        protected Timer _timer;

        public TimedSampleExtractor(string name, long period)
            :base(name)
        {
            _timer = new Timer((state) => {
                Extract();
            }, null, period, period);
        }

        public override void Dispose()
        {
            if (!_disposed)
            {
                _timer.Dispose();
                base.Dispose();
            }
        }
    }
}
