using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using SafeVault.Exceptions;
using FileNotFoundException = SafeVault.Exceptions.FileNotFoundException;

namespace SafeVault.Configuration
{
    public class ConfigFile
    {
        private readonly Dictionary<string, string> _values;

        public ConfigFile(string conf)
        {
            if (!File.Exists(conf))
                throw new FileNotFoundException($"{conf}");

            _values = LoadFromFile(conf);
        }

        public static Dictionary<string, string> LoadFromFile(string conf)
        {
            var values = new Dictionary<string, string>();
            #if NETSTANDARD2_0
            var lines = File.ReadLines(conf);
            #endif

            #if NETFX
            var lines = File.ReadAllLines(conf);
            #endif
            foreach (var line in lines)
            {
                var match = Regex.Match(line, "^(.+?)=(.+?)$");
                if (match.Success)
                    values.Add(match.Groups[1].Value.Trim(), match.Groups[2].Value.Trim());
            }
            return values;
        }

        public void Export(string conf)
        {
            using (var stream = new StreamWriter($"{conf}.~tmp"))
            {
                foreach (var key in _values.Keys)
                {
                    stream.WriteLine($"{key}={_values[key]}");
                }
                stream.Flush();
            }
            File.Copy($"{conf}.~tmp", conf, true);
            File.Delete($"{conf}.~tmp");
        }

        public void SetValue(string name, string value)
        {
            if (_values.ContainsKey(name))
            {
                int ix = 0;
                while (_values.ContainsKey($"{name}.{ix}"))
                    ix++;

                _values[$"{name}.{ix}"] = _values[name];
            }

            _values[name] = value;
        }

        public string GetValue(string name, bool optional = false)
        {
            return GetValue(name, () =>
            {
                if (!optional)
                    throw new InternalErrorException($"Key not Found: {name}");

                return null;
            });
        }

        public string GetValue(string name, Func<string> keyNotFound)
        {
            if (!_values.ContainsKey(name))
            {
                return keyNotFound.Invoke();
            }
            return _values[name];
        }
    }
}