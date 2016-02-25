using System;
using System.IO;
using System.Speech.AudioFormat;
using System.Speech.Recognition;

namespace Discord.Soundboard
{
    public class SoundboardSpeechRecognizer
    {
        private SpeechRecognitionEngine speech;
        private SpeechAudioFormatInfo format;

        public SoundboardBot Bot { get; protected set; }

        public Stream Stream { get; protected set; }

        public SoundboardSpeechRecognizer(SoundboardBot bot)
        {
            this.Stream = new SoundboardAudioStream(
                Convert.ToInt32((48000 * 2) * (60.0 / 1000.0)));   // 60ms per audio frame
            this.Bot = bot;
            this.format = new SpeechAudioFormatInfo(48000, AudioBitsPerSample.Sixteen, AudioChannel.Mono);
            this.speech = new SpeechRecognitionEngine();
            this.speech.SetInputToAudioStream(Stream, format);
            this.speech.SpeechRecognized += OnSpeechRecognized;
            this.BuildGrammar();
            this.Start();
        }

        public void Start()
        {
            this.speech.RecognizeAsync(RecognizeMode.Multiple);
        }

        public void Stop()
        {
            this.speech.RecognizeAsyncStop();
        }

        protected void OnSpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            if (e.Result.Confidence >= Bot.Configuration.SpeechRecognitionConfidenceThreshold)
            {
                Console.WriteLine("recognized '{0}' with confidence {1}", e.Result.Text, e.Result.Confidence);

                var tokens = e.Result.Text.Split(' ');

                //if (tokens.Length >= 2)
                //    Bot.PlaySoundEffect(tokens[1]);
            }
        }

        private void BuildGrammar()
        {
            var builder = new GrammarBuilder();
            var choices = new Choices();

            foreach (var effect in Bot.SoundEffectRepository.Effects)
                choices.Add(effect.Value.Name);

            builder.Append("soundbot");
            builder.Append(choices);

            speech.LoadGrammar(new Grammar(builder));
        }
    }
}
