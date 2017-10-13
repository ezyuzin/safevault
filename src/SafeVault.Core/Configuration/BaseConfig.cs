using System;
using System.Collections.Generic;

namespace SafeVault.Configuration
{
    public abstract class BaseConfig : IConfig
    {
        private Dictionary<Type, IConfigData> _cached = new Dictionary<Type, IConfigData>();

        public string Get(string key)
        {
            return Get(key, true);
        }

        public abstract void Set(string key, string value);
        public abstract void Set(string key, int index, string value);

        public IConfigSection GetSection(string key)
        {
            return GetSection(key, true);
        }

        public abstract string Get(string key, bool throwIfNotFound);
        public abstract IConfigSection GetSection(string key, bool throwIfNotFound);
        public abstract string[] GetValues(string name);

        public IConfigData GetConfig(Type type)
        {
            IConfigData config;
            lock (_cached)
            {
                if (_cached.TryGetValue(type, out config))
                    return config;
            }

            config = (IConfigData)Activator.CreateInstance(type);
            config.Import(this);
            lock (_cached)
            {
                if (!_cached.ContainsKey(type))
                    _cached.Add(type, config);

                return _cached[type];
            }
        }

        public T Get<T>() where T : IConfigData
        {
            return (T)GetConfig(typeof(T));
        }

        public abstract bool IsExist(string xpath);
        public abstract IConfigSection[] GetSections(string key);
        public abstract string GetSectionName();
        public abstract void Save(string filename);
        public abstract void Save();
    }
}