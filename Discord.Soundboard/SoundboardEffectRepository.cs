using System.Text.RegularExpressions;
using System.Linq;
using System.IO;
using System.Collections.Generic;

namespace Discord.Soundboard
{
    public class SoundboardEffectRepository
    {
        public IDictionary<string, SoundboardEffect> Effects { get; protected set; }

        public static Regex ValidFilenameRegex = new Regex("^[a-z0-9_.]+", 
            RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);

        public static string[] SupportedExtensions = { ".wav" };

        public SoundboardEffectRepository()
        {
            this.Effects = new Dictionary<string, SoundboardEffect>();
        }

        public bool Exists(string name)
        {
            if (name == null)
                return false;

            return Effects.ContainsKey(name.ToLowerInvariant());
        }


        public bool ValidateFilename(string filename)
        {
            return ValidFilenameRegex.IsMatch(filename);
        }

        public bool ValidateFileExtension(string ext)
        {
            return SupportedExtensions.Contains(ext);
        }

        public SoundboardEffect FindByName(string name)
        {
            return Effects.ContainsKey(name) ? Effects[name] : null;
        }

        public void Add(SoundboardEffect effect)
        {
            this.Effects.Add(effect.Name, effect);
        }

        public void Remove(SoundboardEffect effect)
        {
            this.Effects.Add(effect.Name, effect);
        }

        public void LoadFromDirectory(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return;

            if (!Directory.Exists(path))
                return;

            var files = Directory.GetFiles(path, "*.wav", SearchOption.AllDirectories);
            foreach (var file in files)
                this.Add(new SoundboardEffect(file));
        }
    }
}
