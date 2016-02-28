using System.IO;

namespace Discord.Soundboard.Data
{
    public class Sound : Record
    {
        public string Name { get; set; }

        public int Duration { get; set; }

        public uint PlayCount { get; set; }

        public override void Deserialize(BinaryReader reader)
        {
            Name = reader.ReadString();
            Duration = reader.ReadInt32();
            PlayCount = reader.ReadUInt32();
        }

        public override void Serialize(BinaryWriter writer)
        {
            writer.Write(Name);
            writer.Write(Duration);
            writer.Write(PlayCount);
        }
    }
}
