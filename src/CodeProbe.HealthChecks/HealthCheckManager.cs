using CodeProbe.Configuration;
using log4net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using CodeProbe.HealthChecks.Policy;
using CodeProbe.HealthChecks.Configuration;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CodeProbe.HealthChecks
{
    /// <summary>
    /// Singleton manager class for healthchecks.
    /// </summary>
    public class HealthCheckManager : ConfigurationResolverBase
    {
        private static ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected static HealthCheckManager _instance = null;

        /// <summary>
        /// Instance getter method.
        /// </summary>
        /// <returns>HealthCheckManager singleton instance.</returns>
        public static HealthCheckManager Ask()
        {
            if (_instance == null)
                throw new InvalidOperationException("Component not initialized.");
            return _instance;
        }

        protected HealthCheckManager(HealthChecksConfigurationSection conf)
        {
            if (conf != null)
            {
                foreach (HealthChecksConfigurationSection.HealthcheckElementCollection.HealthcheckElement hcConf in conf.Healthchecks)
                {
                    if (_checks.ContainsKey(hcConf.Name))
                        throw new InvalidOperationException(string.Format("Duplicate healthceck name found: {0}", hcConf.Name));

                    object[] tmp = BuildArgsList(Type.GetType(hcConf.Type), hcConf.Arguments);
                    tmp[0] = hcConf.Name;
                    AbstractHealthCheck c = (AbstractHealthCheck)Activator.CreateInstance(Type.GetType(hcConf.Type), tmp);

                    AddHealthCheck(c);
                }

                if (conf.CheckAllPolicy != null && !string.IsNullOrEmpty(conf.CheckAllPolicy.Type))
                {
                    Type type = Type.GetType(conf.CheckAllPolicy.Type);
                    CheckAllPolicy = new Tuple<Type, object[]>(type, BuildArgsList(type, conf.CheckAllPolicy.Arguments));
                }
            }
            else
            {
                _logger.Info("Initialized, with defaults unchanged.");
            }

            _logger.InfoFormat("CheckAllPolicy: type = {0}, args = {1}", CheckAllPolicy.Item1.FullName, string.Join(",", CheckAllPolicy.Item2.Select(p => p.ToString()).ToArray()));
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
                HealthChecksConfigurationSection hc = new HealthChecksConfigurationSection();
                conf.ParseSection("healthchecks", hc);

                _instance = new HealthCheckManager(hc);

                _logger.Info("Initialized.");
            }
            catch (Exception e)
            {
                _instance = null;
                _logger.Error("Error during intialization.", e);
                throw e;
            }
        }

        protected ConcurrentDictionary<string, AbstractHealthCheck> _checks = new ConcurrentDictionary<string, AbstractHealthCheck>();

        protected Tuple<Type, object[]> CheckAllPolicy = new Tuple<Type, object[]>(typeof(ZeroNullCheckAllPolicy), new object[0]);

        /// <summary>
        /// Get the deafult checkAllPolicy configured.
        /// </summary>
        /// <returns>Policy object.</returns>
        public ICheckAllPolicy GetDefaultCheckAllPolicy()
        {
            return (ICheckAllPolicy)Activator.CreateInstance(CheckAllPolicy.Item1, CheckAllPolicy.Item2);
        }

        /// <summary>
        /// Adds an healthcheck.
        /// </summary>
        /// <param name="check">HealthCheck.</param>
        public void AddHealthCheck(AbstractHealthCheck check)
        {
            _checks.AddOrUpdate(check.Name, check, (n, m) => check);
        }

        /// <summary>
        /// Removes an healthcheck, by name returning it.
        /// </summary>
        /// <param name="check">HealthCheck.</param>
        public void RemoveHealthCheck(string name, out AbstractHealthCheck check)
        {
            if (_checks.TryRemove(name, out check) == false)
                check = null;
        }

        /// <summary>
        /// Gets all the healtchecks
        /// </summary>
        /// <returns>Healtchecks list.</returns>
        public List<AbstractHealthCheck> GetAll()
        {
            return GetAll(".*", Int32.MaxValue);
        }

        /// <summary>
        /// Gets all the healtchecks with a name that mathces against a regex filter.
        /// </summary>
        /// <param name="filter">Regex filter.</param>
        /// <returns>Healtchecks list.</returns>
        public List<AbstractHealthCheck> GetAll(string filter)
        {
            return GetAll(filter, Int32.MaxValue);
        }

        /// <summary>
        /// Gets all the healtchecks with a severity less or equal to the one specificed.
        /// </summary>
        /// <param name="severity">Severity.</param>
        /// <returns>Healtchecks list.</returns>
        public List<AbstractHealthCheck> GetAll(int severity)
        {
            return GetAll(".*", severity);
        }

        /// <summary>
        /// Gets all the healtchecks with a name that mathces against a regex filter and with a severity less or equal to the one specificed.
        /// </summary>
        /// <param name="filter">Regex filter.</param>
        /// <param name="severity">Severity.</param>
        /// <returns>Healtchecks list.</returns>
        public List<AbstractHealthCheck> GetAll(string filter, int severity)
        {
            return _checks.Where(p => p.Value.Severity <= severity && Regex.IsMatch(p.Value.Name,filter)).Select(kv => kv.Value).ToList();
        }

        /// <summary>
        /// Checks the outcome of all the HealthChecks, using the specified policy.
        /// </summary>
        /// <param name="policy">CheckAll policy</param>
        /// <returns>True if the checkall policy deem the collection seuccessfull.</returns>
        public bool CheckAll(ICheckAllPolicy policy)
        {
            return CheckAll(policy, ".*", int.MaxValue);
        }

        /// <summary>
        /// Checks the outcome of all the HealthChecks, using the specified policy.
        /// </summary>
        /// <param name="policy">CheckAll policy</param>
        /// <param name="filter">Regex filter.</param>
        /// <returns>True if the checkall policy deem the collection seuccessfull.</returns>
        public bool CheckAll(ICheckAllPolicy policy, string filter)
        {
            return CheckAll(policy, filter, int.MaxValue);
        }

        /// <summary>
        /// Checks the outcome of all the HealthChecks, using the specified policy.
        /// </summary>
        /// <param name="policy">CheckAll policy</param>
        /// <param name="severity">Severity.</param>
        /// <returns>True if the checkall policy deem the collection seuccessfull.</returns>
        public bool CheckAll(ICheckAllPolicy policy, int severity)
        {
            return CheckAll(policy, ".*", severity);
        }

        /// <summary>
        /// Checks the outcome of all the HealthChecks, using the specified policy.
        /// </summary>
        /// <param name="policy">CheckAll policy</param>
        /// <param name="filter">Regex filter.</param>
        /// <param name="severity">Severity.</param>
        /// <returns>True if the checkall policy deem the collection seuccessfull.</returns>
        public bool CheckAll(ICheckAllPolicy policy, string filter, int severity)
        {
            ConcurrentBag<Tuple<int, bool?>> results = new ConcurrentBag<Tuple<int, bool?>>();
            Parallel.ForEach(GetAll(filter,severity), new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount }, p =>
            {
                try
                {
                    results.Add(new Tuple<int, bool?>(p.Severity, p.Check()));
                }
                catch (Exception e)
                {
                    _logger.Warn(string.Format("Error checking healthcheck: {0}",p.Name),e);
                    results.Add(new Tuple<int, bool?>(p.Severity,null));
                }
            });

            return policy.CheckAll(results.ToList());
        }
    }
}
