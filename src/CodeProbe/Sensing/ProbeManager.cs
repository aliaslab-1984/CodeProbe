using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using CodeProbe.Configuration;
using System.Reflection;
using log4net;
using System.Diagnostics;

namespace CodeProbe.Sensing
{
    /// <summary>
    /// Singleton manager class for sensing.
    /// </summary>
    public class ProbeManager : ConfigurationResolverBase
    {
        private static ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected static ProbeManager _instance = null;

        /// <summary>
        /// Instance getter method.
        /// </summary>
        /// <returns>ProbeManager singleton instance.</returns>
        public static ProbeManager Ask()
        {
            if (_instance == null)
                throw new InvalidOperationException("Component not initialized.");
            return _instance;
        }

        private ConcurrentDictionary<string, AbstractProbe> _metrics = new ConcurrentDictionary<string, AbstractProbe>();
        private ConcurrentDictionary<string, Action<AbstractProbe, object>> _globalSense = new ConcurrentDictionary<string, Action<AbstractProbe, object>>();

        protected ProbeManager(SensingConfigurationSection conf)
        {
            if (conf != null)
            {
                if (conf.Reservoir != null && !string.IsNullOrEmpty(conf.Reservoir.Type))
                {
                    Type type = Type.GetType(conf.Reservoir.Type);
                    Reservoir = new Tuple<Type, object[]>(type, BuildArgsList(type, conf.Reservoir.Arguments));
                }
            }
            else
            {
                _logger.Info("Initialized, with defaults unchanged.");
            }

            _logger.InfoFormat("Reservoir: type = {0}, args = {1}", Reservoir.Item1.FullName, string.Join(",", Reservoir.Item2.Select(p => p.ToString()).ToArray()));


            this.SystemGauge("dateTime", () => DateTime.Now.ToFileTimeUtc());

            this.MachineGauge("process.ram", () =>
            {
                using (PerformanceCounter ramCounter = new PerformanceCounter("Process", "Working Set", Process.GetCurrentProcess().ProcessName))
                {
                    return ramCounter.NextValue();
                }
            });
            this.MachineGauge("process.cpu", () =>
            {
                using (PerformanceCounter cpuCounter = new PerformanceCounter("Process", "% Processor Time", Process.GetCurrentProcess().ProcessName))
                {
                    return cpuCounter.NextValue();
                }
            });
        }

        /// <summary>
        /// Inits the manager from configuration using the default configuration section tag delimiter (codeProbe).
        /// </summary>
        public static void Init()
        {
            Init("codeProbe");
        }
        
        /// <summary>
        /// Inits the manager from configuration using a custom configuration section tag delimiter.
        /// </summary>
        /// <param name="section">Configuration section tag delimiter</param>
        public static void Init(string section)
        {
            _logger.Info("Initializing.");

            try
            {
                if (_instance != null)
                    throw new InvalidOperationException("Already initialized.");

                CodeProbeConfigurationSection conf = (CodeProbeConfigurationSection)System.Configuration.ConfigurationManager.GetSection(section);
                SensingConfigurationSection hc = new SensingConfigurationSection();
                conf.ParseSection("sensing", hc);

                _instance = new ProbeManager(hc);

                _logger.Info("Initialized.");
            }
            catch (Exception e)
            {
                _instance = null;
                _logger.Error("Error during intialization.", e);
                throw e;
            }
        }

        #region default globals

        protected Tuple<Type, object[]> Reservoir = new Tuple<Type, object[]>(typeof(SlidingWindowReservoir<>), new object[] { 100 });

        #endregion

        /// <summary>
        /// Retreive the configured default reservoir type.
        /// </summary>
        /// <typeparam name="T">Type of data in the reservoir.</typeparam>
        /// <returns>Reservoir instance</returns>
        public AbstractReservoir<T> GetDefaultReservoir<T>() where T : IComparable
        {
            return (AbstractReservoir<T>)Activator.CreateInstance(Reservoir.Item1.MakeGenericType(new Type[] { typeof(T) }), Reservoir.Item2);
        }

        /// <summary>
        /// Get a list of probes, matching a specified regex against their name.
        /// </summary>
        /// <param name="filter">Regex filter.</param>
        /// <returns>List of probes.</returns>
        public IEnumerable<AbstractProbe> MatchFilter(string filter)
        {
            return _metrics.Where(kv => Regex.IsMatch(kv.Key,filter)).Select(kv => kv.Value);
        }

        private void _applyGlobalSense(AbstractProbe metric)
        {
            foreach (Action<AbstractProbe, object> item in _globalSense.Where(kv => Regex.IsMatch(metric.Name, kv.Key)).Select(kv => kv.Value))
            {
                metric.Sense += item;
            }
        }

        private void _removeGlobalSense(AbstractProbe metric)
        {
            foreach (Action<AbstractProbe, object> item in _globalSense.Where(kv => Regex.IsMatch(metric.Name, kv.Key)).Select(kv => kv.Value))
            {
                metric.Sense -= item;
            }
        }

        /// <summary>
        /// Adds an event handler to the sensing event of every probe with a name matching against the filter regex.
        /// Every handler will be automatically added to every new probe created by the ProbeManager.
        /// </summary>
        /// <param name="filter">Regex filter.</param>
        /// <param name="handler">Sensing event handler. The argument passed are the probe and the value sensed.</param>
        public void AddSenseHandler(string filter, Action<AbstractProbe, object> handler)
        {
            _globalSense.AddOrUpdate(filter,handler,(f,h)=>handler);

            foreach (AbstractProbe probe in MatchFilter(filter))
	        {
                probe.Sense += handler;
	        }
        }

        /// <summary>
        /// Removes an event handler to the sensing event of every probe with a name matching against the filter regex.
        /// </summary>
        /// <param name="filter">Regex filter.</param>
        public void RemoveSenseHandler(string filter)
        {
            Action<AbstractProbe, object> handler;
            _globalSense.TryRemove(filter, out handler);

            foreach (AbstractProbe probe in MatchFilter(filter))
            {
                probe.Sense -= handler;
            }
        }

        /// <summary>
        /// Adds a probe created out of the ProbeManager scope to the ProbeManager scope, automatically applying the sensing event registrations.
        /// </summary>
        /// <param name="metric"></param>
        public void Register(AbstractProbe metric)
        {
            _metrics.AddOrUpdate(metric.Name, metric, (n, m) => metric);

            _applyGlobalSense(metric);
        }

        /// <summary>
        /// Get a Probe by complete name
        /// </summary>
        /// <param name="name">Probe name</param>
        /// <returns>The requested probe or null if not found.</returns>
        public AbstractProbe GetProbe(string name)
        {
            if (!_metrics.ContainsKey(name))
                return null;
            else
                return _metrics[name];
        }

        /// <summary>
        /// Removes a probe from the ProbeManager scope by complete name, returning it.
        /// </summary>
        /// <param name="name">Probe name</param>
        /// <param name="metric">Removed probe. Null if not found.</param>
        public void Unregister(string name, out AbstractProbe metric)
        {
            if (_metrics.TryRemove(name, out metric) == false)
                metric = null;

            _removeGlobalSense(metric);
        }

        /// <summary>
        /// Unregisters all probes matching against a regex filter.
        /// </summary>
        /// <param name="filter">Regex filter.</param>
        public void Clear(string filter)
        {
            AbstractProbe tmp;
            foreach (AbstractProbe probe in MatchFilter(filter))
            {
                Unregister(probe.Name, out tmp);
            }
        }

        /// <summary>
        /// Removes all the sense handler form all the registerd probes.
        /// </summary>
        public void ClearSenseHandler()
        {
            RemoveSenseHandler(".*");
            _globalSense.Clear();
        }
    }
}
