using System;
using System.Collections.Generic;
using System.Linq;
using Discord.Soundboard.Data;

namespace Discord.Soundboard
{
    public class SoundboardStatistics
    {
        private SoundboardDatabase Database { get; set; }

        public SoundboardStatistics(SoundboardDatabase db)
        {
            Database = db;
        }

        public void Load()
        {
            Database.Load();
        }

        public void Save()
        {
            Database.Save();
        }

        /// <summary>
        /// Records statistics when a user plays a sound.
        /// </summary>
        /// <param name="user">Discord user</param>
        /// <param name="effect">Effect name</param>
        public void Play(Discord.User profile, SoundboardEffect effect)
        {
            var sound = GetSound(effect);
            var user = GetUser(profile);

            if (sound != null)
            {
                sound.PlayCount++;
            }

            if (user != null)
            {
                user.MillisecondsPlayed += Convert.ToUInt64(effect.Duration.TotalMilliseconds);
            }
        }

        public IEnumerable<Data.Sound> GetTopSounds(int limit)
        {
            var query = from s in Database.Sounds
                        orderby s.PlayCount descending
                        select s;

            return query.Take(limit);
        }

        public IEnumerable<Data.User> GetTopUsers(int limit)
        {
            var query = from u in Database.Users
                        orderby u.MillisecondsPlayed descending
                        select u;

            return query.Take(limit);
        }

        public Data.Sound GetTopPlayedSound()
        {
            var query = from s in Database.Sounds
                        orderby s.PlayCount descending
                        select s;

            return query.FirstOrDefault();
        }

        public TimeSpan GetTimePlayedBySound(SoundboardEffect effect)
        {
            var sound = GetSound(effect);

            if (sound != null)
            {
                return TimeSpan.FromMilliseconds(
                    Convert.ToDouble(sound.Duration * sound.PlayCount));
            }

            return TimeSpan.Zero;
        }

        public TimeSpan GetTimePlayedByUser(Discord.User profile)
        {
            var user = GetUser(profile);
            var time = user.MillisecondsPlayed;

            return GetTimePlayed(time);
        }

        public TimeSpan GetTotalDuration()
        {
            var time = Database.Sounds.Sum(x => x.Duration * x.PlayCount);
            return TimeSpan.FromMilliseconds(time);
        }

        public long GetTotalPlayCounts()
        {
            var counts = Database.Sounds.Sum(x => x.PlayCount);
            return counts;
        }

        private Data.User GetUser(Discord.User profile)
        {
            var query = from u in Database.Users
                        where u.DiscordId == profile.Id
                        select u;

            var user = query.FirstOrDefault();

            if (user == null)
            {
                user = new Data.User()
                {
                    DiscordId = profile.Id,
                    Name = profile.Name,
                    MillisecondsPlayed = 0
                };

                Database.Users.Add(user);
            }

            return user;
        }

        private Data.Sound GetSound(SoundboardEffect effect)
        {
            var query = from s in Database.Sounds
                        where s.Name == effect.Name
                        select s;

            var sound = query.FirstOrDefault();

            if (sound == null)
            {
                sound = new Data.Sound()
                {
                    Name = effect.Name,
                    Duration = Convert.ToInt32(effect.Duration.TotalMilliseconds),
                    PlayCount = 0
                };

                Database.Sounds.Add(sound);
            }

            return sound;
        }

        private TimeSpan GetTimePlayed(ulong time)
        {
            if (time <= 0)
                return TimeSpan.Zero;

            return TimeSpan.FromMilliseconds(Convert.ToDouble(time));
        }

    }
}
