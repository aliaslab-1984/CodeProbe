using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace CodeProbe.Configuration
{
    /// <summary>
    /// Sensing specific codeProbe configuration subsection.
    /// </summary>
    public class SensingConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("reservoir", IsRequired = false)]
        public CodeProbeConfigurationSection.ObjectElementCollection.ObjectElement Reservoir { get { return (CodeProbeConfigurationSection.ObjectElementCollection.ObjectElement)this["reservoir"]; } }
    }
}
