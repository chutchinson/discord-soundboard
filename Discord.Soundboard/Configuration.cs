using System;
using System.IO;
using System.Collections.Generic;

namespace Discord.Soundboard
{
    public class Configuration
    {
        private IDictionary<string, string> Values { get; set; }

        public static Configuration FromFile(string filename)
        {
            if (!File.Exists(filename))
                return null;

            var cfg = new Configuration();
            cfg.Load(filename);

            return cfg;
        }

        public Configuration()
        {
            this.Values = new Dictionary<string, string>();
        }

        public bool HasKey(string key)
        {
            return this.Values.ContainsKey(key);
        }

        public T TryGetValue<T>(string key, T defaultValue = default(T))
        {
            try
            {
                return HasKey(key) ? (T)Convert.ChangeType(Values[key], typeof(T)) : defaultValue;
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        public void Load(string path)
        {
            if (!File.Exists(path))
                return;

            var lines = File.ReadAllLines(path);
            foreach (var line in lines)
                ParseLine(line);
        }

        private void ParseLine(string line)
        {
            var pos = line.IndexOf('=');

            if (pos > 0 && pos < line.Length)
            {
                string key = line.Substring(0, pos);
                string value = line.Substring(pos + 1, line.Length - pos - 1);

                if (!string.IsNullOrWhiteSpace(key) && !string.IsNullOrWhiteSpace(value))
                    Values.Add(key.Trim(), value.Trim());
            }
        }
    }
}
