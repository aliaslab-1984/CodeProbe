using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Configuration;
using System.Xml;
using System.Reflection;
using System.IO;

namespace CodeProbe.Configuration
{
    /// <summary>
    /// Global configurtion section. Defines common ConfigurationElements and collections for the library.
    /// </summary>
    public class CodeProbeConfigurationSection : ConfigurationSection
    {
        #region nested types

        public class StringConfigurationElement : ConfigurationElement
        {
            public string Value { get; private set; }

            protected override void DeserializeElement(XmlReader reader, bool s)
            {
                Value = reader.ReadInnerXml();
            }
        }

        public class ArgumentElementCollection : ConfigurationElementCollection
        {
            #region nested types

            public class ArgumentElement : ConfigurationElement
            {
                [ConfigurationProperty("name", IsRequired = true)]
                public string Name { get { return this["name"].ToString(); } }

                [ConfigurationProperty("value", IsRequired = true)]
                public string Value { get { return this["value"].ToString(); } }
            }

            #endregion

            protected override ConfigurationElement CreateNewElement()
            {
                return new ArgumentElement();
            }

            protected override object GetElementKey(ConfigurationElement element)
            {
                return ((ArgumentElement)element).Name;
            }
        }
        
        public class ObjectElementCollection : ConfigurationElementCollection
        {
            #region nested types

            public class ObjectElement : ConfigurationElement
            {
                [ConfigurationProperty("type", IsRequired = true)]
                public string Type { get { return this["type"].ToString(); } set { this["type"] = value; } }

                [ConfigurationProperty("args", IsRequired = false)]
                public ArgumentElementCollection Arguments { get { return (ArgumentElementCollection)this["args"]; } }
            }

            #endregion

            protected override ConfigurationElement CreateNewElement()
            {
                return new ObjectElement();
            }

            protected override object GetElementKey(ConfigurationElement element)
            {
                return ((ObjectElement)element).Type;
            }
        }
                        
        #endregion

        public Dictionary<string, XmlNode> _elements = new Dictionary<string, XmlNode>();

        /// <summary>
        /// Parse unhandled section, in order to pass them to specific library sub configuration manager, if used and configured.
        /// </summary>
        /// <param name="section">sub section name</param>
        /// <param name="parser">Configuration section parser for the sub section.</param>
        public void ParseSection(string section, ConfigurationSection parser)
        {
            if (!_elements.ContainsKey(section))
                return;

            using (StringReader stringReader = new StringReader(_elements[section].OuterXml))
            using (XmlReader reader = XmlReader.Create(stringReader, new XmlReaderSettings() { CloseInput = true }))
            {
                parser.GetType().GetMethod("DeserializeSection", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(parser, new object[] { reader });
            }
        }
        
        protected override void DeserializeSection(XmlReader reader)
        {
            XmlDocument doc = new XmlDocument();
            reader.Read();
            doc.LoadXml("<b>"+reader.ReadInnerXml()+"</b>");
            foreach (XmlNode node in doc.SelectNodes("/*/*"))
            {
                _elements.Add(node.Name, node);
            }
        }
    }
}