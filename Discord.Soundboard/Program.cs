using System;

namespace Discord.Soundboard
{
    class Program
    {
        public static readonly string DefaultConfigurationFilename = "soundbot.cfg";

        static void Main(string[] args)
        {
            var configurationFilename = (args.Length > 0) ? args[0] : DefaultConfigurationFilename;
            var configuration = SoundboardBotConfiguration.FromFile(configurationFilename);
            var bot = new SoundboardBot(configuration);

            Console.Title = configuration.Name;

            bot.Connect();
        }
    }
}
