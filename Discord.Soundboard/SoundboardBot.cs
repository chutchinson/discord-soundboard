using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;

using Discord.Audio;
using Discord.Modules;

using NAudio.Wave;

namespace Discord.Soundboard
{
    public class SoundboardBot
    {
        private DiscordClient client;
        private IAudioClient audio;
        private IDictionary<string, SoundboardSpeechRecognizer> recognizers;
        private ManualResetEvent sending;

        public SoundboardBot(SoundboardBotConfiguration cfg)
        {
            Configuration = cfg ?? new SoundboardBotConfiguration();
            SoundEffectRepository = new SoundboardEffectRepository();
            SoundEffectRepository.LoadFromDirectory(Configuration.EffectsPath);

            sending = new ManualResetEvent(true);
            recognizers = new Dictionary<string, SoundboardSpeechRecognizer>();

            client = new DiscordClient(x =>
            {
                x.AppName = "Soundbot";
                x.MessageCacheSize = 0;
                x.UsePermissionsCache = false;
                x.EnablePreUpdateEvents = true;
                x.LogLevel = LogSeverity.Info;
            });

            client.MessageReceived += OnMessageReceived;
            client.UsingModules();
            client.UsingAudio(x =>
            {
                x.Mode = AudioMode.Outgoing;
                x.EnableEncryption = false;
                x.EnableMultiserver = false;
                x.Bitrate = 128;
                x.BufferLength = 10000;
            });
            
            
        }

        public SoundboardEffectRepository SoundEffectRepository
        { get; protected set; }

        public SoundboardBotConfiguration Configuration
        { get; protected set; }

        public void Connect()
        {
            client.ExecuteAndWait(async () =>
            {
                await client.Connect(Configuration.User, Configuration.Password);

                if (!string.IsNullOrWhiteSpace(Configuration.Status))
                    client.SetGame(Configuration.Status);

                if (!string.IsNullOrEmpty(Configuration.VoiceChannel))
                {
                    var server = client.Servers.FirstOrDefault();
                    var voiceChannel = server.FindChannels(Configuration.VoiceChannel, ChannelType.Voice, true).FirstOrDefault();
                    if (voiceChannel != null)
                        audio = await voiceChannel.JoinAudio();
                }
            });
        }

        public void PlaySoundEffect(Channel ch, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return;

            SendMessage(ch, string.Format("playing {0}", name));

            Task.Run(() =>
            {
                try
                {
                    sending.WaitOne();

                    var effect = SoundEffectRepository.FindByName(name);

                    if (audio != null && effect != null)
                    {
                        if (effect.Duration.TotalMilliseconds == 0)
                            return;

                        var format = new WaveFormat(48000, 16, 2);
                        var length = Convert.ToInt32(format.AverageBytesPerSecond / 60.0 * 1000.0);
                        var buffer = new byte[length];

                        using (var reader = new WaveFileReader(effect.Path))
                        using (var resampler = new WaveFormatConversionStream(format, reader))
                        {
                            int count = 0;
                            while ((count = resampler.Read(buffer, 0, length)) > 0)
                                audio.Send(buffer, 0, count);
                        }
                    }

                }
                finally
                {
                    sending.Set();
                }
            });

        }

        protected void OnMessageReceived(object sender, MessageEventArgs e)
        {
            if (e.User.Id == client.CurrentUser.Id)
                return;

            if (e.Message.Attachments.Length > 0)
            {
                var attachment = e.Message.Attachments.FirstOrDefault();

                if (attachment != null)
                {
                    var ext = Path.GetExtension(attachment.Filename);

                    if (attachment.Size >  2 * 1024 * 1024)
                    {
                        SendMessage(e.Channel, "sorry that file is too big :(");
                        return;
                    }

                    if (attachment.Filename.Contains(" "))
                    {
                        SendMessage(e.Channel, "sorry I can only accept effects without spaces");
                        return;
                    }

                    if (ext != ".wav")
                    {
                        SendMessage(e.Channel, "sorry I can only accept *.wav files :(");
                        return;
                    }

                    var key = Path.GetFileNameWithoutExtension(attachment.Filename);
                    var name = Path.GetFileName(attachment.Filename);
                    var path = Path.Combine(Configuration.EffectsPath, name);

                    if (File.Exists(path))
                    {
                        SendMessage(e.Channel, "sorry that sound already exists");
                        return;
                    }

                    Task.Run(() =>
                    {
                        using (var web = new WebClient())
                        {
                            web.DownloadFile(attachment.Url, path);

                            SoundEffectRepository.Add(new SoundboardEffect(path));
                            SendMessage(e.Channel, string.Format("{0} is ready", key));
                        }
                    });

                }
            }

            if (e.Message.IsMentioningMe())
            {
                var tokens = e.Message.Text.Split(' ');
                var cmd = (tokens.Length >= 2) ? tokens[1].ToLowerInvariant() : string.Empty;

                switch (cmd)
                {
                    case "list":
                        CommandListSounds(e.Channel);
                        break;
                    default:
                        CommandPlayEffect(e.Channel, cmd);
                        break;
                }
            }
        }

        protected void CommandListSounds(Channel ch)
        {
            var builder = new StringBuilder();
            var list = string.Join(", ", SoundEffectRepository.Effects.Select(x => x.Key));

            SendMessage(ch, list);
        }

        protected void CommandPlayEffect(Channel ch, string effect)
        {
            if (effect == null)
                return;

            PlaySoundEffect(ch, effect);
        }

        public void SendMessage(Channel channel, string text)
        {
            if (channel == null)
                return;

            channel.SendMessage(text);
        }
      
    }
}
