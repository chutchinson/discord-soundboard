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

        public DateTime DateLastModified { get; set; }

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
                    this.DateLastModified = File.GetLastWriteTimeUtc(filename);

                    using (var reader = new WaveFileReader(filename))
                        this.Duration = reader.TotalTime;
                }
            }
            catch (Exception)
            {
                this.DateLastModified = DateTime.MinValue;
                this.Duration = TimeSpan.Zero;
            }
        }
    }
}
