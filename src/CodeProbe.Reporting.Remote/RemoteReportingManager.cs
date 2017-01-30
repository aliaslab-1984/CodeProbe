using CodeProbe.Configuration;
using CodeProbe.HealthChecks;
using CodeProbe.Reporting.Extractors;
using CodeProbe.Reporting.Remote.Configuration;
using CodeProbe.Reporting.Samplers;
using CodeProbe.Reporting.Statistics;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeProbe.Reporting.Remote
{
    /// <summary>
    /// Singleton manager class for RemoteReporting.
    /// </summary>
    public class RemoteReportingManager : ConfigurationResolverBase
    {
        private static ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected static RemoteReportingManager _instance = null;

        /// <summary>
        /// Instance getter method.
        /// </summary>
        /// <returns>RemoteReportingManager singleton instance.</returns>
        public static RemoteReportingManager Ask()
        {
            if (_instance == null)
                throw new InvalidOperationException("Component not initialized.");

            return _instance;
        }

        protected List<Tuple<DirectSampleExtractor,ICheckAllPolicy>> _probeServices=new List<Tuple<DirectSampleExtractor,ICheckAllPolicy>>();
        protected List<Tuple<string, List<RemoteReportingConfigurationSection.RemoteProbeServiceElementCollection.RemoteProbeServiceElement>, ICheckAllPolicy>> _aggregators = new List<Tuple<string, List<RemoteReportingConfigurationSection.RemoteProbeServiceElementCollection.RemoteProbeServiceElement>, ICheckAllPolicy>>();

        protected RemoteReportingManager(RemoteReportingConfigurationSection conf)
        {
            RemoteExtractorPrefix = "remote";
            ProbeServiceBindingName = "ProbeServiceWebHttpBinding";

            if (conf != null && conf != null)
            {
                if (!string.IsNullOrEmpty(conf.RemoteExtractorNamePrefix))
                    RemoteExtractorPrefix = conf.RemoteExtractorNamePrefix;
                if (!string.IsNullOrEmpty(conf.ProbeServiceBindingName))
                    ProbeServiceBindingName = conf.ProbeServiceBindingName;

                #region reporting manager
                
                foreach (RemoteReportingConfigurationSection.ProbeServiceElementCollection.ProbeServiceElement repConf in conf.ProbeServices)
                {
                    #region sampler

                    HashSet<AbstractProbeStatistic> stats = new HashSet<AbstractProbeStatistic>();
                    foreach (CodeProbeConfigurationSection.ObjectElementCollection.ObjectElement item in repConf.Sampler.Stats)
                    {
                        stats.Add((AbstractProbeStatistic)Activator.CreateInstance(Type.GetType(item.Type), BuildArgsList(Type.GetType(item.Type), item.Arguments)));
                    }

                    AbstractSampler sampler = (AbstractSampler)Activator.CreateInstance(Type.GetType(repConf.Sampler.Type), new object[] { repConf.Sampler.Name, stats });

                    #endregion

                    #region extractor

                    DirectSampleExtractor extractor = new DirectSampleExtractor(RemoteExtractorPrefix +"."+ repConf.Name);

                    extractor.AddSampler(sampler);

                    ReportingManager.Ask().AddExtractor(extractor);

                    ICheckAllPolicy checkAllPolicy = HealthCheckManager.Ask().GetDefaultCheckAllPolicy();
                    if (repConf.CheckAllPolicy != null && !string.IsNullOrEmpty(repConf.CheckAllPolicy.Type))
                    {
                        Type type = Type.GetType(repConf.CheckAllPolicy.Type);
                        checkAllPolicy = (ICheckAllPolicy)Activator.CreateInstance(type, BuildArgsList(type, repConf.CheckAllPolicy.Arguments));
                    }

                    _probeServices.Add(new Tuple<DirectSampleExtractor, ICheckAllPolicy>(extractor, checkAllPolicy));
                    
                    #endregion
                }

                #endregion

                #region aggregators

                foreach (RemoteReportingConfigurationSection.ProbeServiceAggregatorElementCollection.ProbeServiceAggregatorElement repConf in conf.Aggregators)
                {
                    ICheckAllPolicy checkAllPolicy = HealthCheckManager.Ask().GetDefaultCheckAllPolicy();
                    if (repConf.CheckAllPolicy != null && !string.IsNullOrEmpty(repConf.CheckAllPolicy.Type))
                    {
                        Type type = Type.GetType(repConf.CheckAllPolicy.Type);
                        checkAllPolicy = (ICheckAllPolicy)Activator.CreateInstance(type, BuildArgsList(type, repConf.CheckAllPolicy.Arguments));
                    }

                    List<RemoteReportingConfigurationSection.RemoteProbeServiceElementCollection.RemoteProbeServiceElement> endpoints = new List<RemoteReportingConfigurationSection.RemoteProbeServiceElementCollection.RemoteProbeServiceElement>();
                    foreach (RemoteReportingConfigurationSection.RemoteProbeServiceElementCollection.RemoteProbeServiceElement endpoint in repConf.Endpoints)
	                {
		                endpoints.Add(endpoint);
	                }

                    _aggregators.Add(new Tuple<string, List<RemoteReportingConfigurationSection.RemoteProbeServiceElementCollection.RemoteProbeServiceElement>, ICheckAllPolicy>(RemoteExtractorPrefix + "." + repConf.Name, endpoints, checkAllPolicy));
                }

                #endregion
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
                ReportingManager.Ask();
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Initialize ReportingManager first.", e);
            }

            try
            {
                if (_instance != null)
                    throw new InvalidOperationException("Already initialized.");

                CodeProbeConfigurationSection conf = (CodeProbeConfigurationSection)System.Configuration.ConfigurationManager.GetSection(section);
                RemoteReportingConfigurationSection hc = new RemoteReportingConfigurationSection();
                conf.ParseSection("remoteReporting", hc);

                _instance = new RemoteReportingManager(hc);
                
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
        /// Prefix attached to remote extracotrs.
        /// </summary>
        public string RemoteExtractorPrefix { get; protected set; }

        /// <summary>
        /// Binding configuration name of each binding used by RemoteReporting services.
        /// </summary>
        public string ProbeServiceBindingName { get; protected set; }

        /// <summary>
        /// Get a ProbeService configured extractor, linked by the ProbeService class name.
        /// </summary>
        /// <param name="service">ProbeService implementation full name.</param>
        /// <returns>Tuple with extractor and checkAllPolicy</returns>
        public Tuple<DirectSampleExtractor, ICheckAllPolicy> GetProbeServiceExtractor(string service)
        {
            Tuple<DirectSampleExtractor, ICheckAllPolicy> tmp = _probeServices.FirstOrDefault(p => p.Item1.Name == RemoteExtractorPrefix + "." + service);
            if (tmp == null)
                throw new ArgumentException(string.Format("No valid extractor found for probe service: {0}",service));

            return tmp;
        }

        public struct ProbeServiceEndpointDescriptor
        {
            public string Name { get; set; }
            public string Endpoint { get; set; }
            public int Severity { get; set; }
        }

        /// <summary>
        /// Get a ProbeServiceAggregator configuration, linked by the ProbeServiceAggregator class name.
        /// </summary>
        /// <param name="service">ProbeServiceAggregator implementation full name</param>
        /// <returns>Tuple with aggregated service endpoints and checkAllPolicy</returns>
        public Tuple<List<ProbeServiceEndpointDescriptor>, ICheckAllPolicy> GetProbeServiceAggregator(string service)
        {
            Tuple<string, List<RemoteReportingConfigurationSection.RemoteProbeServiceElementCollection.RemoteProbeServiceElement>, ICheckAllPolicy> conf = 
                _aggregators.FirstOrDefault(p => p.Item1 == RemoteExtractorPrefix + "." + service);
            if (conf == null)
                throw new ArgumentException(string.Format("No valid agrgegator configuration found for aggragator probe service: {0}", service));

            return new Tuple<List<ProbeServiceEndpointDescriptor>, ICheckAllPolicy>(conf.Item2.Select(p => new ProbeServiceEndpointDescriptor(){Name=p.Name, Endpoint=p.Endpoint, Severity= p.Severity}).ToList(), conf.Item3);
        }
    }
}
