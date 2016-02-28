using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord.Soundboard.Data
{
    public class SoundboardDatabase
    {

        private static readonly int DatabaseFormatHeader = 0x53424442;
        private static readonly short DatabaseVersion = 0x0001;

        public SoundboardDatabaseConfiguration Configuration { get; protected set; }
        public RecordCollection<User> Users { get; protected set; }
        public RecordCollection<Sound> Sounds { get; protected set; }
        
        public SoundboardDatabase(SoundboardDatabaseConfiguration config)
        {
            Configuration = config ?? new SoundboardDatabaseConfiguration();
            Users = new RecordCollection<User>();
            Sounds = new RecordCollection<Sound>();
        }

        public void CreateIfNotExists()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Configuration.Path));

            if (!File.Exists(Configuration.Path))
                Save();
        }

        public void Load()
        {
            if (!File.Exists(Configuration.Path))
                throw new FileNotFoundException("failed to locate database file", Configuration.Path);

            Users.Clear();
            Sounds.Clear();

            using (var stream = File.Open(Configuration.Path, FileMode.Open))
            using (var reader = new BinaryReader(stream))
            {
                // Read database header

                if (reader.ReadInt32() != DatabaseFormatHeader)
                    throw new IOException("Invalid database format");

                if (reader.ReadInt16() < DatabaseVersion)
                    throw new IOException("Invalid or unsupported database version");

                // Read database records

                Users.Deserialize(reader);
                Sounds.Deserialize(reader);
            }
        }

        public void Save()
        {
            using (var stream = File.Open(Configuration.Path, FileMode.Create))
            using (var writer = new BinaryWriter(stream))
            {
                // Write database header

                writer.Write(DatabaseFormatHeader);
                writer.Write(DatabaseVersion);

                // Write records

                Users.Serialize(writer);
                Sounds.Serialize(writer);
            }
        }
    }
}
