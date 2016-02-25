using System;
using System.IO;

using NAudio.Wave;

namespace Discord.Soundboard
{
    public class SoundboardEffect
    {
        public string Name { get; set; }

        public string Path { get; set; }

        public TimeSpan Duration { get; set; }

        public SoundboardEffect(string filename)
        {
            if (filename == null)
                throw new ArgumentNullException();

            this.Name = System.IO.Path.GetFileNameWithoutExtension(filename);
            this.Path = filename;

            try
            {
                if (File.Exists(filename))
                {
                    using (var reader = new WaveFileReader(filename))
                        this.Duration = reader.TotalTime;
                }
            }
            catch (Exception)
            {
                this.Duration = TimeSpan.Zero;
            }
        }
    }
}
