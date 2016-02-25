using System.IO;

namespace Discord.Soundboard
{
    public class SoundboardBotConfiguration
    {
        public string Status { get; set; }

        public string User { get; set; }

        public string Password { get; set; }

        public string Token { get; set; }

        public string EffectsPath { get; set; }

        public string VoiceChannel { get; set; }

        public int Bitrate { get; set; }

        public float SpeechRecognitionConfidenceThreshold { get; set; }

        public bool IsSpeechRecognitionEnabled { get; set; }

        public SoundboardBotConfiguration()
        {
            this.Status = string.Empty;
            this.User = string.Empty;
            this.Password = string.Empty;
            this.Token = string.Empty;
            this.EffectsPath = "Effects";
            this.Bitrate = 128;
            this.VoiceChannel = string.Empty;
            this.SpeechRecognitionConfidenceThreshold = 0.85f;
            this.IsSpeechRecognitionEnabled = false;
        }

        public static SoundboardBotConfiguration FromFile(string filename)
        {
            if (!File.Exists(filename))
                return null;

            var result = new SoundboardBotConfiguration();
            result.Load(Configuration.FromFile(filename));

            return result;
        }

        public void Load(Configuration cfg)
        {
            Status = cfg.TryGetValue("status", "Soundboard");
            User = cfg.TryGetValue("email", string.Empty);
            Password = cfg.TryGetValue("password", string.Empty);
            Token = cfg.TryGetValue("token", string.Empty);
            EffectsPath = cfg.TryGetValue("path.effects", "Effects");
            Bitrate = cfg.TryGetValue("voice.bitrate", 128);
            VoiceChannel = cfg.TryGetValue("voice.channel", string.Empty);
            IsSpeechRecognitionEnabled = cfg.TryGetValue("voice.recognition.enabled", false);
            SpeechRecognitionConfidenceThreshold = cfg.TryGetValue("voice.recognition.threshold", 0.85f);
        }
    }
}
