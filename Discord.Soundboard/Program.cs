using System;

namespace Discord.Soundboard
{
    class Program
    {
        public static readonly string Title = "Soundbot";
        public static readonly string ConfigurationFilename = "soundbot.cfg";

        static void Main(string[] args)
        {
            Console.WriteLine(Title);
            Console.Title = Title;

            var cfg = SoundboardBotConfiguration.FromFile(ConfigurationFilename);
            var bot = new SoundboardBot(cfg);

            bot.Connect();
        }
    }
}
