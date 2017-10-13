using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using SafeVault.Configuration.Exceptions;

namespace SafeVault.Configuration
{
    internal class AppConfigSection : IConfigSection
    {
        private readonly AppConfig _parent;
        private readonly XmlElement _section;

        public AppConfigSection(AppConfig parent, XmlElement section)
        {
            _parent = parent;
            _section = section;
        }

        public IConfig Config
        {
            get
            {
                return _parent;
            }
        }

        public string Get(string key)
        {
            return Get(key, true);
        }

        public virtual string Get(string key, bool throwIfNotFound)
        {
            return GetSingle(GetInternal(key), key, throwIfNotFound)?.Value;
        }

        public void Set(string key, string value)
        {
            GetSingle(GetInternal(key), key, true).Value = value;
        }
        public void Set(string key, int index, string value)
        {
            foreach (var entry in GetInternal(key))
            {
                if (index-- != 0)
                    continue;

                entry.Value = value;
                return;
            }
            throw new ConfigurationEntryNotFoundException(GetSectionName() + $"/{key}[{index}]");
        }

        public IConfigSection GetSection(string key)
        {
            return GetSection(key, true);
        }

        public virtual IConfigSection GetSection(string key, bool throwIfNotFound)
        {
            return GetSingle(GetSectionInternal(key), key, throwIfNotFound);
        }

        public virtual IConfigSection[] GetSections(string key)
        {
            return GetSectionInternal(key).ToArray();
        }

        private IEnumerable<IConfigSection> GetSectionInternal(string xpath)
        {
            var nodes = _section.SelectNodes(xpath);
            if (nodes == null)
                yield break;

            foreach (var node in nodes)
            {
                if (node is XmlElement)
                    yield return new AppConfigSection(_parent, (XmlElement) node);
            }
        }

        public string GetSectionName()
        {
            string xpath = _section.Name;
            XmlNode section = _section.ParentNode as XmlElement;
            while (section != null)
            {
                xpath = section.Name + "/" + xpath;
                section = section.ParentNode as XmlElement;
            }
            return xpath;
        }

        private T GetSingle<T>(IEnumerable<T> values, string key, bool throwIfNotFound)
        {
            int count = 0;
            T value = default(T);
            foreach (T value1 in values)
            {
                value = value1;
                if (++count > 1)
                    throw new ConfigurationMultiEntryException(GetSectionName() + "/" + key);
            }
            if (count == 0 && throwIfNotFound)
                throw new ConfigurationEntryNotFoundException(GetSectionName() + "/" + key);
            return value;
        }

        public virtual string[] GetValues(string xpath)
        {
            return GetInternal(xpath)
                .Select(m => m.Value)
                .ToArray();
        }

        private IEnumerable<AppConfigEntry> GetInternal(string xpath)
        {
            if (_section == null)
                yield break;

            var nodes = _section.SelectNodes(xpath);
            if (nodes == null)
                yield break;

            foreach (XmlNode node in nodes)
            {
                if (node is XmlAttribute)
                    yield return new AppConfigEntry(node);

                var element = node as XmlElement;
                if (element != null && element.ChildNodes.Count == 1)
                {
                    if (element.ChildNodes[0] is XmlText)
                        yield return new AppConfigEntry((XmlText)element.ChildNodes[0]);
                }
            }
        }

        public virtual bool IsExist(string xpath)
        {
            if (_section == null)
                return false;

            var nodes = _section.SelectNodes(xpath);
            if (nodes == null)
                return false;

            foreach (var node in nodes)
            {
                var attribute = node as XmlAttribute;
                if (attribute != null)
                    return true;

                var element = node as XmlElement;
                if (element != null && element.ChildNodes.Count == 1)
                {
                    var text = (element.ChildNodes[0] as XmlText);
                    if (text != null)
                        return true;
                }
            }
            return false;
        }
    }
}