using System.IO;

namespace Discord.Soundboard
{
    public class SoundboardBotConfiguration
    {
        public string Name { get; set; }

        public string Status { get; set; }

        public string User { get; set; }

        public string Password { get; set; }

        public string Token { get; set; }

        public string EffectsPath { get; set; }

        public string DatabasePath { get; set; }

        public long DatabaseSaveInterval { get; set; }

        public string Server { get; set; }

        public string VoiceChannel { get; set; }

        public int Bitrate { get; set; }

        public float SpeechRecognitionConfidenceThreshold { get; set; }

        public bool IsSpeechRecognitionEnabled { get; set; }

        /// <summary>
        /// Maximum sound effect file size (in bytes).
        /// </summary>
        public int MaximumSoundEffectSize { get; set; }

        public SoundboardBotConfiguration()
        {
            this.Name = "Discord Soundboard";
            this.Status = string.Empty;
            this.User = string.Empty;
            this.Password = string.Empty;
            this.Token = string.Empty;
            this.EffectsPath = "Effects";
            this.DatabasePath = "Data\\soundbot.db";
            this.DatabaseSaveInterval = 15000;
            this.Bitrate = 64;
            this.Server = string.Empty;
            this.VoiceChannel = string.Empty;
            this.SpeechRecognitionConfidenceThreshold = 0.85f;
            this.IsSpeechRecognitionEnabled = false;
            this.MaximumSoundEffectSize = 2 * 1024 * 1024;
        }

        public static SoundboardBotConfiguration FromFile(string filename)
        {
            var config = new SoundboardBotConfiguration();

            if (!File.Exists(filename))
                return config;

            config.Load(Configuration.FromFile(filename));

            return config;
        }

        public void Load(Configuration cfg)
        {
            Name = cfg.TryGetValue("name", "Discord Soundboard");
            Status = cfg.TryGetValue("status", "Soundboard");
            User = cfg.TryGetValue("email", string.Empty);
            Password = cfg.TryGetValue("password", string.Empty);
            Token = cfg.TryGetValue("token", string.Empty);
            EffectsPath = cfg.TryGetValue("path.effects", "Effects");
            DatabasePath = cfg.TryGetValue("path.database", "Data\\soundbot.db");
            DatabaseSaveInterval = cfg.TryGetValue("database.save.interval", 15000);
            Bitrate = cfg.TryGetValue("voice.bitrate", 64);
            Server = cfg.TryGetValue("server", string.Empty);
            VoiceChannel = cfg.TryGetValue("voice.channel", string.Empty);
            IsSpeechRecognitionEnabled = cfg.TryGetValue("voice.recognition.enabled", false);
            SpeechRecognitionConfidenceThreshold = cfg.TryGetValue("voice.recognition.threshold", 0.85f);
            MaximumSoundEffectSize = cfg.TryGetValue("repository.file.maximumSize", 2 * 1024 * 1024);
        }
    }
}
