using CodeProbe.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace CodeProbe.Reporting.Configuration
{
    /// <summary>
    /// Reporting specific codeProbe configuration subsection.
    /// </summary>
    public class ReportingConfigurationSection : ConfigurationSection
    {
        public class ExtractorElementCollection : ConfigurationElementCollection
        {
            #region nested types

            public class ExtractorElement : ConfigurationElement
            {
                [ConfigurationProperty("name", IsRequired = true)]
                public string Name { get { return this["name"].ToString(); } }

                [ConfigurationProperty("type", IsRequired = true)]
                public string Type { get { return this["type"].ToString(); } }

                [ConfigurationProperty("args", IsRequired = false)]
                public CodeProbeConfigurationSection.ArgumentElementCollection Arguments { get { return (CodeProbeConfigurationSection.ArgumentElementCollection)this["args"]; } }
            }

            #endregion

            protected override ConfigurationElement CreateNewElement()
            {
                return new ExtractorElement();
            }

            protected override object GetElementKey(ConfigurationElement element)
            {
                return ((ExtractorElement)element).Name;
            }
        }

        public class ReporterElementCollection : ConfigurationElementCollection
        {
            #region nested types

            public class ReporterElement : ConfigurationElement
            {
                [ConfigurationProperty("name", IsRequired = true)]
                public string Name { get { return this["name"].ToString(); } }

                [ConfigurationProperty("type", IsRequired = true)]
                public string Type { get { return this["type"].ToString(); } }

                [ConfigurationProperty("filter", IsRequired = false, DefaultValue = ".*")]
                public string Filter { get { return this["filter"].ToString(); } }

                [ConfigurationProperty("args", IsRequired = false)]
                public CodeProbeConfigurationSection.ArgumentElementCollection Arguments { get { return (CodeProbeConfigurationSection.ArgumentElementCollection)this["args"]; } }

                [ConfigurationProperty("sampler", IsRequired = true)]
                public SamplerElement Sampler { get { return (SamplerElement)this["sampler"]; } }

                [ConfigurationProperty("extractorRef", IsRequired = true)]
                public string ExtractorRef { get { return (string)this["extractorRef"]; } }
            }

            public class SamplerElement : ConfigurationElement
            {
                [ConfigurationProperty("name", IsRequired = true)]
                public string Name { get { return this["name"].ToString(); } }

                [ConfigurationProperty("type", IsRequired = true)]
                public string Type { get { return this["type"].ToString(); } }

                [ConfigurationProperty("stats", IsRequired = false)]
                public CodeProbeConfigurationSection.ObjectElementCollection Stats { get { return (CodeProbeConfigurationSection.ObjectElementCollection)this["stats"]; } }
            }

            #endregion

            protected override ConfigurationElement CreateNewElement()
            {
                return new ReporterElement();
            }

            protected override object GetElementKey(ConfigurationElement element)
            {
                return ((ReporterElement)element).Name;
            }
        }

        [ConfigurationProperty("extractors", IsRequired = true)]
        public ExtractorElementCollection Extractors { get { return (ExtractorElementCollection)this["extractors"]; } }

        [ConfigurationProperty("reporters", IsRequired = true)]
        public ReporterElementCollection Reporters { get { return (ReporterElementCollection)this["reporters"]; } }
    }
}
