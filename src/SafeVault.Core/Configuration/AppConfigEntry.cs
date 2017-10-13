using System.Xml;
using SafeVault.Configuration.Exceptions;

namespace SafeVault.Configuration
{
    internal class AppConfigEntry
    {
        public XmlNode Node { get; }
        public string Value 
        {
            get
            {
                var attribute = Node as XmlAttribute;
                if (attribute != null)
                    return attribute.Value;

                var text = Node as XmlText;
                if (text != null)
                    return text.Value;
                
                throw new ConfigurationException("Bad Configuration Node Type");
            }
            set
            {
                var attribute = Node as XmlAttribute;
                if (attribute != null)
                {
                    attribute.Value = value;
                    return;
                }

                var text = Node as XmlText;
                if (text != null)
                {
                    text.Value = value;
                    return;
                }
                throw new ConfigurationException("Bad Configuration Node Type");
            }
        }

        public AppConfigEntry(XmlNode node)
        {
            Node = node;
        }
    }
}