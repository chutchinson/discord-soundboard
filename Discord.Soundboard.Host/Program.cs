using System;
using System.Net;

namespace Discord.Soundboard.Host
{
    internal sealed class Program
    {
        public static readonly string DefaultConfigurationFilename = "soundbot.cfg";

        static void Main(string[] args)
        {
            var configurationFilename = (args.Length > 0) ? args[0] : DefaultConfigurationFilename;
            var configuration = SoundboardBotConfiguration.FromFile(configurationFilename);
            var bot = new SoundboardBot(configuration);

            Console.Title = configuration.Name;

            if (IsRunningOnMono())
            {
                ServicePointManager.ServerCertificateValidationCallback +=
                    (sender, cert, chain, err) => true;
            }

            try
            {
                bot.LoadDatabase();
                bot.Connect();
            }
            finally
            {
                bot.Database.Save();
            }
        }

        public static bool IsRunningOnMono()
        {
            return Type.GetType("Mono.Runtime") != null;
        }
    }
}
