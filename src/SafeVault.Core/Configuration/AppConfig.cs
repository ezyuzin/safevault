using System.IO;
using System.Xml;
using SafeVault.Configuration.Exceptions;

namespace SafeVault.Configuration
{
    public class AppConfig : BaseConfig
    {
        private AppConfigSection _section;
        private XmlDocument _doc;
        private string _filename;

        public AppConfig(string configName)
        {
            Load(configName);
        }

        private void Load(string configName)
        {
            if (!File.Exists(configName))
                throw new ConfigurationException($"Configuration file not found: {configName}");

            _filename = configName;
            _doc = new XmlDocument();
            _doc.Load(configName);
            _section = new AppConfigSection(this, _doc.DocumentElement);
        }

        public AppConfig()
        {
            if (File.Exists("app.config"))
                Load("app.config");
            else
            {
                _filename = "app.config";
                _doc = new XmlDocument();
                var root = _doc.CreateElement("configuration");
                _doc.AppendChild(root);
                _section = new AppConfigSection(this, _doc.DocumentElement);
            }
        }

        public override void Save(string filename)
        {
            _doc.Save(filename);
        }

        public override void Save()
        {
            Save(_filename);
        }

        public override bool IsExist(string xpath)
        {
            return _section.IsExist(xpath);
        }

        public override string GetSectionName()
        {
            return _section.GetSectionName();
        }

        public override IConfigSection GetSection(string key, bool throwIfNotFound)
        {
            return _section.GetSection(key, throwIfNotFound);
        }

        public override IConfigSection[] GetSections(string key)
        {
            return _section.GetSections(key);
        }

        public override string Get(string key, bool throwIfNotFound)
        {
            return _section.Get(key, throwIfNotFound);
        }

        public override void Set(string key, string value)
        {
            _section.Set(key, value);
        }

        public override void Set(string key, int index, string value)
        {
            _section.Set(key, index, value);
        }

        public override string[] GetValues(string key)
        {
            return _section.GetValues(key);
        }
    }
}