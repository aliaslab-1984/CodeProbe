using CodeProbe.Configuration;
using CodeProbe.Reporting.Configuration;
using CodeProbe.Reporting.Statistics;
using CodeProbe.Sensing;
using log4net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CodeProbe.Reporting
{
    /// <summary>
    /// Singleton manager class for reporting.
    /// </summary>
    public class ReportingManager : ConfigurationResolverBase
    {
        private static ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected static ReportingManager _instance = null;

        /// <summary>
        /// Instance getter method.
        /// </summary>
        /// <returns>ReportingManager singleton instance.</returns>
        public static ReportingManager Ask()
        {
            if (_instance == null)
                throw new InvalidOperationException("Component not initialized.");

            return _instance;
        }

        protected List<AbstractSampleExtractor> _extractors = new List<AbstractSampleExtractor>();
        protected List<Tuple<AbstractReporter, AbstractSampler>> _reporters = new List<Tuple<AbstractReporter, AbstractSampler>>();

        protected ReportingManager(ReportingConfigurationSection conf)
        {
            if (conf != null && conf.Reporters!=null)
            {
                foreach (ReportingConfigurationSection.ExtractorElementCollection.ExtractorElement extConf in conf.Extractors)
                {
                    if (_extractors.Any(p => p.Name == extConf.Name))
                        throw new InvalidOperationException(string.Format("Duplicate extractor ref name found: {0}",extConf.Name));

                    object[] tmp = BuildArgsList(Type.GetType(extConf.Type), extConf.Arguments);
                    tmp[0] = extConf.Name;
                    AbstractSampleExtractor extractor = (AbstractSampleExtractor)Activator.CreateInstance(Type.GetType(extConf.Type), tmp);

                    _extractors.Add(extractor);
                }

                foreach (ReportingConfigurationSection.ReporterElementCollection.ReporterElement repConf in conf.Reporters)
                {
                    #region sampler

                    HashSet<AbstractProbeStatistic> stats = new HashSet<AbstractProbeStatistic>();
                    foreach (CodeProbeConfigurationSection.ObjectElementCollection.ObjectElement item in repConf.Sampler.Stats)
                    {
                        stats.Add((AbstractProbeStatistic)Activator.CreateInstance(Type.GetType(item.Type), BuildArgsList(Type.GetType(item.Type), item.Arguments)));
                    }

                    AbstractSampler sampler = (AbstractSampler)Activator.CreateInstance(Type.GetType(repConf.Sampler.Type), new object[] { repConf.Sampler.Name, stats } );

                    #endregion

                    #region reporter

                    if (_reporters.Any(p => p.Item1.Name == repConf.Name))
                        throw new InvalidOperationException(string.Format("Duplicate reporter ref name found: {0}", repConf.Name));

                    object[] tmp = BuildArgsList(Type.GetType(repConf.Type), repConf.Arguments);
                    tmp[0] = repConf.Name;
                    tmp[1] = sampler;
                    AbstractReporter reporter = (AbstractReporter)Activator.CreateInstance(Type.GetType(repConf.Type), tmp);

                    _reporters.Add(new Tuple<AbstractReporter, AbstractSampler>(reporter,sampler));

                    #endregion

                    #region extractor

                    AbstractSampleExtractor extractor = _extractors.FirstOrDefault(p => p.Name == repConf.ExtractorRef);
                    if (extractor == null)
                        throw new InvalidOperationException(string.Format("Extractor ref not found: {0}", repConf.ExtractorRef));
                    
                    #endregion

                    #region wiring

                    extractor.AddSampler(sampler,repConf.Filter);
                    
                    #endregion
                }
            }
            else
            {
                _logger.Info("Initialized, with defaults unchanged.");
            }
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
                ReportingConfigurationSection hc = new ReportingConfigurationSection();
                conf.ParseSection("reporting", hc);

                _instance = new ReportingManager(hc);

                foreach (Tuple<AbstractReporter, AbstractSampler> rep in _instance._reporters)
                {
                    rep.Item1.Start();
                }

                _logger.Info("Initialized.");
            }
            catch (Exception e)
            {
                _instance = null;
                _logger.Error("Error during intialization.", e);
                throw e;
            }
        }

        /// <summary>
        /// Gets the configured reporters.
        /// </summary>
        public AbstractReporter[] Reporters
        {
            get { return _reporters.Select(p=>p.Item1).ToArray(); }
        }

        /// <summary>
        /// Gets the configured Extractors.
        /// </summary>
        public AbstractSampleExtractor[] Extractors
        {
            get { return _extractors.ToArray(); }
        }

        /// <summary>
        /// Removes a reporter by name, unbinding every connected resource.
        /// </summary>
        /// <param name="name">Reporter name.</param>
        public void RemoveReporter(string name)
        {
            Tuple<AbstractReporter, AbstractSampler> rep = _reporters.FirstOrDefault(p => p.Item1.Name == name);
            if (rep != null)
            {
                rep.Item1.Stop();

                foreach (AbstractSampleExtractor item in _extractors)
                {
                    item.RemoveSampler(rep.Item2);
                }

                _reporters.Remove(rep);
            }
        }

        /// <summary>
        /// Adds a reporter, attaching a sampler sampling every probe to the named extractor.
        /// </summary>
        /// <param name="extractorName">Extractor name</param>
        /// <param name="reporter">Reporter to add</param>
        /// <param name="sampler">Sampler to use with the reporter.</param>
        public void AddReporter(string extractorName, AbstractReporter reporter, AbstractSampler sampler)
        {
            AddReporter(extractorName, reporter, sampler, ".*");
        }

        /// <summary>
        /// Adds a reporter, attaching a sampler sampling every probe with a name matching against the regex filter to the named extractor.
        /// Throws an exception if the reporter already exists.
        /// </summary>
        /// <param name="extractorName">Extractor name</param>
        /// <param name="reporter">Reporter to add</param>
        /// <param name="sampler">Sampler to use with the reporter.</param>
        /// <param name="filter">Regex filter</param>
        public void AddReporter(string extractorName, AbstractReporter reporter, AbstractSampler sampler, string filter)
        {
            AbstractSampleExtractor extractor = _extractors.FirstOrDefault(p => p.Name == extractorName);
            if (extractor == null)
                throw new InvalidOperationException(string.Format("Extractor ref not found: {0}", extractorName));
            if (reporter == null)
                throw new ArgumentNullException("reporter must be set.");
            if (sampler == null)
                throw new ArgumentNullException("sampler must be set.");
            
            if (_reporters.Any(p => p.Item1.Name == reporter.Name))
                throw new InvalidOperationException(string.Format("Duplicate reporter ref name found: {0}", reporter.Name));
            
            _reporters.Add(new Tuple<AbstractReporter, AbstractSampler>(reporter, sampler));

            extractor.AddSampler(sampler,filter);

            reporter.Start();
        }

        /// <summary>
        /// Adds an extractor. Throws an exception if the extractor is already present.
        /// </summary>
        /// <param name="extractor">Extractor to add.</param>
        public void AddExtractor(AbstractSampleExtractor extractor)
        {
            if (_extractors.Any(p => p.Name == extractor.Name))
                throw new InvalidOperationException(string.Format("Duplicate extractor ref name found: {0}", extractor.Name));

            _extractors.Add(extractor);
        }

        /// <summary>
        /// Removes the named extractor, unbinding every connected resource.
        /// </summary>
        /// <param name="extractor"></param>
        public void RemoveExtractor(AbstractSampleExtractor extractor)
        {
            if (_extractors.Any(p => p.Name == extractor.Name))
            {
                _extractors.Remove(extractor);
                extractor.Clear();
            }
        }
    }
}
