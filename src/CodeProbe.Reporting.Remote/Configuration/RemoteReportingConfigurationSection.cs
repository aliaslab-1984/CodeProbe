using CodeProbe.Configuration;
using CodeProbe.Reporting.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace CodeProbe.Reporting.Remote.Configuration
{
    /// <summary>
    /// RemoteReporting specific codeProbe configuration subsection.
    /// </summary>
    public class RemoteReportingConfigurationSection : ConfigurationSection
    {
        #region nested types

        public class ProbeServiceElementCollection : ConfigurationElementCollection
        {
            #region nested types

            public class ProbeServiceElement : ConfigurationElement
            {
                [ConfigurationProperty("name", IsRequired = true)]
                public string Name { get { return this["name"].ToString(); } }

                [ConfigurationProperty("sampler", IsRequired = true)]
                public ReportingConfigurationSection.ReporterElementCollection.SamplerElement Sampler { get { return (ReportingConfigurationSection.ReporterElementCollection.SamplerElement)this["sampler"]; } }
                
                [ConfigurationProperty("checkAllPolicy", IsRequired = false)]
                public CodeProbeConfigurationSection.ObjectElementCollection.ObjectElement CheckAllPolicy { get { return (CodeProbeConfigurationSection.ObjectElementCollection.ObjectElement)this["checkAllPolicy"]; } }
            }

            #endregion

            protected override ConfigurationElement CreateNewElement()
            {
                return new ProbeServiceElement();
            }

            protected override object GetElementKey(ConfigurationElement element)
            {
                return ((ProbeServiceElement)element).Name;
            }
        }

        public class RemoteProbeServiceElementCollection : ConfigurationElementCollection
        {
            #region nested types
            
            public class RemoteProbeServiceElement : ConfigurationElement
            {
                [ConfigurationProperty("name", IsRequired = true)]
                public string Name { get { return this["name"].ToString(); } }

                [ConfigurationProperty("endpoint", IsRequired = true)]
                public string Endpoint { get { return this["endpoint"].ToString(); } }

                [ConfigurationProperty("severity", IsRequired = true)]
                public int Severity { get { return Convert.ToInt32(this["severity"]); } }

            }

            #endregion

            protected override ConfigurationElement CreateNewElement()
            {
                return new RemoteProbeServiceElement();
            }

            protected override object GetElementKey(ConfigurationElement element)
            {
                return ((RemoteProbeServiceElement)element).Name;
            }
        }

        public class ProbeServiceAggregatorElementCollection : ConfigurationElementCollection
        {
            #region nested types

            public class ProbeServiceAggregatorElement : ConfigurationElement
            {
                [ConfigurationProperty("name", IsRequired = true)]
                public string Name { get { return this["name"].ToString(); } }
                                
                [ConfigurationProperty("endpoints", IsRequired = false)]
                public RemoteProbeServiceElementCollection Endpoints { get { return (RemoteProbeServiceElementCollection)this["endpoints"]; } }
                
                [ConfigurationProperty("checkAllPolicy", IsRequired = false)]
                public CodeProbeConfigurationSection.ObjectElementCollection.ObjectElement CheckAllPolicy { get { return (CodeProbeConfigurationSection.ObjectElementCollection.ObjectElement)this["checkAllPolicy"]; } }
            }

            #endregion

            protected override ConfigurationElement CreateNewElement()
            {
                return new ProbeServiceAggregatorElement();
            }

            protected override object GetElementKey(ConfigurationElement element)
            {
                return ((ProbeServiceAggregatorElement)element).Name;
            }
        }

        #endregion

        [ConfigurationProperty("remoteExtractorNamePrefix", IsRequired = false)]
        public string RemoteExtractorNamePrefix { get { return this["remoteExtractorNamePrefix"].ToString(); } }

        [ConfigurationProperty("probeServiceBindingName", IsRequired = false)]
        public string ProbeServiceBindingName { get { return this["probeServiceBindingName"].ToString(); } }

        [ConfigurationProperty("probeServices", IsRequired = true)]
        public ProbeServiceElementCollection ProbeServices { get { return (ProbeServiceElementCollection)this["probeServices"]; } }

        [ConfigurationProperty("probeServiceAggregators", IsRequired = true)]
        public ProbeServiceAggregatorElementCollection Aggregators { get { return (ProbeServiceAggregatorElementCollection)this["probeServiceAggregators"]; } }
    }
}
