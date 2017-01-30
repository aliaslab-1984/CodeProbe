using CodeProbe.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace CodeProbe.HealthChecks.Configuration
{
    /// <summary>
    /// HealthChecks specific codeProbe configuration subsection.
    /// </summary>
    public class HealthChecksConfigurationSection : ConfigurationSection
    {
        public class HealthcheckElementCollection : ConfigurationElementCollection
        {
            #region nested types

            public class HealthcheckElement : ConfigurationElement
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
                return new HealthcheckElement();
            }

            protected override object GetElementKey(ConfigurationElement element)
            {
                return ((HealthcheckElement)element).Name;
            }
        }
        
        [ConfigurationProperty("checks", IsRequired = true)]
        public HealthcheckElementCollection Healthchecks { get { return (HealthcheckElementCollection)this["checks"]; } }

        [ConfigurationProperty("checkAllPolicy", IsRequired = false)]
        public CodeProbeConfigurationSection.ObjectElementCollection.ObjectElement CheckAllPolicy { get { return (CodeProbeConfigurationSection.ObjectElementCollection.ObjectElement)this["checkAllPolicy"]; } }
    }
}
