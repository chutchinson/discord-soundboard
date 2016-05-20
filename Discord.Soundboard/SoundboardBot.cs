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

using Discord.Soundboard.Data;
using Discord.Soundboard.Text;

namespace Discord.Soundboard
{
    public class SoundboardBot
    {
        private IAudioClient audio;
        private ManualResetEvent sending;
        private Task save;

        public DateTime LastInteractionTime { get; set; }

        public DiscordClient Client { get; protected set; }

        public Server Server { get; protected set; }

        public SoundboardDatabase Database { get; protected set; }

        public SoundboardStatistics Statistics { get; protected set; }

        public SoundboardBot(SoundboardBotConfiguration cfg)
        {
            Configuration = cfg ?? new SoundboardBotConfiguration();
            SoundEffectRepository = new SoundboardEffectRepository();
            SoundEffectRepository.LoadFromDirectory(Configuration.EffectsPath);
            LastInteractionTime = DateTime.Now;

            Database = new SoundboardDatabase(new SoundboardDatabaseConfiguration()
            {
                Path = Configuration.DatabasePath
            });

            Statistics = new SoundboardStatistics(Database);

            sending = new ManualResetEvent(true);
            save = CreateSaveTask();

            Client = new DiscordClient(x =>
            {
                x.AppName = Configuration.Name;
                x.MessageCacheSize = 0;
                x.UsePermissionsCache = false;
                x.EnablePreUpdateEvents = true;
                x.LogLevel = LogSeverity.Info;
            });

            Client.MessageReceived += OnMessageReceived;
            Client.UsingModules();
            Client.UsingAudio(x =>
            {
                x.Mode = AudioMode.Outgoing;
                x.EnableEncryption = false;
                x.Bitrate = 128;
                x.BufferLength = 10000;
            });
        }

        public SoundboardEffectRepository SoundEffectRepository
        { get; protected set; }

        public SoundboardBotConfiguration Configuration
        { get; protected set; }

        public Task CreateSaveTask()
        {
            return new Task(async () =>
            {
                while (Client.CancelToken != null && !Client.CancelToken.IsCancellationRequested)
                {
                    try
                    {
                        SoundboardLoggingService.Instance.Info(Properties.Resources.MessageDatabaseSaving);
                        Database.Save();
                    }
                    catch (Exception ex)
                    {
                        SoundboardLoggingService.Instance.Error(
                            Properties.Resources.MessageDatabaseLoadFailed, ex);
                    }

                    await Task.Delay(
                        TimeSpan.FromMilliseconds(Configuration.DatabaseSaveInterval));
                }
            });
        }

        public void LoadDatabase()
        {
            try
            {
                SoundboardLoggingService.Instance.Info("database loading...");

                Database.CreateIfNotExists();
                Database.Load();

                SoundboardLoggingService.Instance.Info("database loaded");
            }
            catch (Exception ex)
            {
                SoundboardLoggingService.Instance.Error(
                    Properties.Resources.MessageDatabaseLoadFailed, ex);
            }
        }

        public async void ConnectToVoice()
        {
            if (string.IsNullOrWhiteSpace(Configuration.VoiceChannel))
                return;

            if (Server == null)
                return;

            // If we are already connected or connecting then don't attempt to connect to voice again.

            if (audio != null && (audio.State == ConnectionState.Connected || audio.State == ConnectionState.Connecting))
                return;

            // Find the voice channel within the connected server.

            var channel = Server.FindChannels(Configuration.VoiceChannel, ChannelType.Voice, true).FirstOrDefault();

            if (channel == null)
            {
                SoundboardLoggingService.Instance.Error(
                    string.Format("voice channel <{0}> does not exist", Configuration.VoiceChannel));
                return;
            }

            SoundboardLoggingService.Instance.Info("connecting to voice channel...");

            try
            {
                audio = await channel.JoinAudio();

                if (audio != null)
                {
                    switch (audio.State)
                    {
                        case ConnectionState.Connected:
                            SoundboardLoggingService.Instance.Info("connected to voice");
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                SoundboardLoggingService.Instance.Error("failed to connect to voice", ex);
                return;
            }
            
        }

        public void SetStatusMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            Client.SetGame(message);
        }

        public void Connect()
        {
            Client.ExecuteAndWait(async () =>
            {
                SoundboardLoggingService.Instance.Info("authenticating...");

                try
                {
                    await Client.Connect("MTc2MDQxNjUzNTgzMzQ3NzEy.CiEvdw.wrrrFWWNh7naoXXon8KkVKoCnXw"); //  Configuration.User, Configuration.Password);
                }
                catch (Exception ex)
                {
                    SoundboardLoggingService.Instance.Error("authentication failed", ex);
                    return;
                }

                SoundboardLoggingService.Instance.Info("authenticated");

                // Cache the server information

                Server = Client.FindServers(Configuration.Server).FirstOrDefault();
                
                // Set the status (game) message

                SetStatusMessage(Configuration.Status);

                SoundboardLoggingService.Instance.Info("ready");

                // Start database save task

                save.Start();
            });
        }

        public void Disconnect()
        {
            Client.Disconnect();
        }

        public void PlaySoundEffect(User user, Channel ch, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return;

            // Ensure voice channel is connected

            ConnectToVoice();

            // Play the sound effect

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

                        SoundboardLoggingService.Instance.Info(
                            string.Format("[{0}] playing <{1}>", user.Name, name));

                        // Records play statistics

                        Statistics.Play(user, effect);

                        // Notify users soundbot will begin playing

                        SendMessage(ch, string.Format(Properties.Resources.MessagePlayingSound, name));

                        // Resample and stream sound effect over the configured voice channel

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

                        audio.Wait();
                    }

                }
                catch (Exception ex)
                {
                    SoundboardLoggingService.Instance.Error(
                        string.Format(Properties.Resources.MessagePlayingFailed, name), ex);
                }
                finally
                {
                    sending.Set();
                }
            });

        }

        public async void SendMessage(Channel channel, string text)
        {
            if (channel == null)
                return;

            await channel.SendMessage(text);
        }

        protected void OnAttachmentReceived(object sender, MessageEventArgs e)
        {
            var attachment = e.Message.Attachments.FirstOrDefault();

            if (attachment != null)
            {
                var ext = Path.GetExtension(attachment.Filename);

                if (attachment.Size > Configuration.MaximumSoundEffectSize)
                {
                    SendMessage(e.Channel, Properties.Resources.MessageInvalidFileSize);
                    return;
                }

                if (!SoundEffectRepository.ValidateFilename(attachment.Filename))
                {
                    SendMessage(e.Channel, Properties.Resources.MessageInvalidFilename);
                    return;
                }

                if (!SoundEffectRepository.ValidateFileExtension(ext))
                {
                    SendMessage(e.Channel, Properties.Resources.MessageUnsupportedFileExtension);
                    return;
                }

                var key = Path.GetFileNameWithoutExtension(attachment.Filename);
                var name = Path.GetFileName(attachment.Filename);
                var path = Path.Combine(Configuration.EffectsPath, name);

                if (SoundEffectRepository.Exists(name))
                {
                    SendMessage(e.Channel, Properties.Resources.MessageSoundExists);
                    return;
                }

                Task.Run(() =>
                {
                    try
                    {
                        using (var web = new WebClient())
                        {
                            SoundboardLoggingService.Instance.Info(
                                string.Format("downloading sound <{0}>", name));

                            web.DownloadFile(attachment.Url, path);

                            SoundboardLoggingService.Instance.Info(
                                string.Format("downloaded <{0}>", name));

                            SoundEffectRepository.Add(new SoundboardEffect(path));
                            SendMessage(e.Channel, string.Format(Properties.Resources.MessageSoundReady, name));

                            SoundboardLoggingService.Instance.Info(
                                string.Format("sound <{0}> is ready", name));
                        }
                    }
                    catch (Exception ex)
                    {
                        SoundboardLoggingService.Instance.Error("failed to download sound <{0}>", ex);
                        SendMessage(e.Channel, string.Format(Properties.Resources.MessageDownloadFailed, name));
                    }
                });

            }
        }

        protected void OnMessageReceived(object sender, MessageEventArgs e)
        {
            if (e.User.Id == Client.CurrentUser.Id)
                return;

            // Avoid rapid interactions to avoid abuse and bot loops

            var interactionTimeSpan = (DateTime.Now - LastInteractionTime).TotalMilliseconds;

            if (interactionTimeSpan <= Configuration.MinimumInteractionThreshold)
            {
                SoundboardLoggingService.Instance.Warning(
                    string.Format("interaction avoided (interaction allowed in {0} ms)", 
                    Configuration.MinimumInteractionThreshold - interactionTimeSpan));
                return;
            }

            // Unflip tables

            if (e.Message.RawText.Contains("┻━┻"))
                SendMessage(e.Channel, "┬─┬﻿ ノ( ゜-゜ノ)");

            // Process attachments

            if (e.Channel.IsPrivate && e.Message.Attachments.Length > 0)
                OnAttachmentReceived(sender, e);

            // Process commands

            if (e.Message.IsMentioningMe())
            {
                var tokens = e.Message.Text.Split(' ');
                var cmd = (tokens.Length >= 2) ? tokens[1].ToLowerInvariant() : string.Empty;

                SoundboardLoggingService.Instance.Info(
                    string.Format("[{0}] sent command <{1}>", e.User.Name, cmd));

                switch (cmd)
                {
                    case "connect":
                        Server = e.Channel.Server;
                        ConnectToVoice();
                        break;
                    case "list":
                        CommandListSounds(e.User, e.Channel, tokens);
                        break;
                    case "stats":
                        CommandStatistics(e.User, e.Channel, tokens);
                        break;
                    case "random":
                        CommandPlayRandomEffect(e.User, e.Channel);
                        break;
                    default:
                        CommandDefault(e.User, e.Channel, cmd);
                        break;
                }
            }

            // Update last interaction timestamp

            LastInteractionTime = DateTime.Now;

        }

        protected void CommandDefault(User user, Channel ch, string cmd)
        {
            if (SoundEffectRepository.Exists(cmd))
                CommandPlayEffect(user, ch, cmd);
            else
                CommandInvalid(user, ch, cmd);
        }

        protected void CommandStatistics(User user, Channel ch, string[] tokens)
        {
            var cmd = (tokens.Length > 2) ? tokens[2] : tokens[1];

            switch (cmd)
            {
                case "totals":
                    CommandStatisticsTotals(user, ch, tokens);
                    break;
                case "topusers":
                    CommandStatisticsTopUsers(user, ch, tokens);
                    break;
                case "topsounds":
                    CommandStatisticsTopSounds(user, ch, tokens);
                    break;
            }
        }

        protected void CommandStatisticsTotals(User user, Channel ch, string[] tokens)
        {
            var totalDuration = Statistics.GetTotalDuration();
            var totalPlayCount = Statistics.GetTotalPlayCounts();
            var count = Database.Sounds.Count;

            var message = string.Format("played {0} {3} {1} {4} for {2}",
                count, totalPlayCount, totalDuration, "sound".Pluralize(count), "time".Pluralize(totalPlayCount));

            SendMessage(ch, message);
        }

        protected void CommandStatisticsTopSounds(User user, Channel ch, string[] tokens)
        {
            var sounds = Statistics.GetTopSounds(5);

            if (sounds.Any())
            {
                var message = string.Join(", ", sounds.Select((x, p) =>
                    string.Format("{0}. {1} ({2})", p + 1, x.Name, x.PlayCount)));

                SendMessage(ch, string.Format("top sounds by play count: {0}", message));
            }
        }

        protected void CommandStatisticsTopUsers(User user, Channel ch, string[] tokens)
        {
            var users = Statistics.GetTopUsers(5);

            if (users.Any())
            {
                var message = string.Join(", ", users.Select((x, p) =>
                    string.Format("{0}. {1} ({2})", p + 1, x.Name, TimeSpan.FromMilliseconds(x.MillisecondsPlayed))));

                SendMessage(ch, string.Format("top users are {0}", message));
            }
        }

        protected void CommandListSounds(User user, Channel ch, string[] tokens)
        {
            IEnumerable<string> query = null;

            var cmd = tokens.Length > 2 ? tokens[2] : tokens[1];

            switch (cmd)
            {
                case "recent":
                    query = from e in SoundEffectRepository.Effects
                            orderby e.Value.DateLastModified descending
                            select e.Value.Name;
                    break;
                default:
                    query = from e in SoundEffectRepository.Effects
                            orderby e.Value.Name ascending
                            select e.Value.Name;
                    break;
            }


            var list = string.Join(", ", query);

            SendMessage(ch, list);
        }

        protected void CommandPlayRandomEffect(User user, Channel ch)
        {
            var ran = new Random((int) DateTime.Now.Ticks);
            var index = ran.Next(SoundEffectRepository.Effects.Count);
            var effect = SoundEffectRepository.Effects.Values.ElementAt(index);

            PlaySoundEffect(user, ch, effect.Name);
        }

        protected void CommandPlayEffect(User user, Channel ch, string effect)
        {
            if (effect == null)
                return;

            PlaySoundEffect(user, ch, effect);
        }

        protected void CommandInvalid(User user, Channel ch, string command)
        {
            SendMessage(ch, string.Format(Properties.Resources.MessageInvalidCommand, command));
        }
    }
}
