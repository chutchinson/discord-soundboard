using System;

using NLog;
using NLog.Common;

namespace Discord.Soundboard
{
    public sealed class SoundboardLoggingService
    {
        private static readonly SoundboardLoggingService instance = new SoundboardLoggingService();
        private Logger logger = LogManager.GetCurrentClassLogger();

        static SoundboardLoggingService() { }

        public static SoundboardLoggingService Instance
        {
            get { return instance; }
        }

        public void Info(string msg)
        {
            logger.Info(msg);
        }

        public void Warning(string msg)
        {
            logger.Warn(msg);
        }

        public void Error(string msg)
        {
            logger.Error(msg);
        }

        public void Error(string msg, Exception ex)
        {
            logger.Error(ex, msg);
        }
    }
}
