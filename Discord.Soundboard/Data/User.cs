using System.IO;

namespace Discord.Soundboard.Data
{
    public class User : Record
    {
        public ulong DiscordId { get; set; }

        public string Name { get; set; }

        public ulong MillisecondsPlayed { get; set; }

        public override void Serialize(BinaryWriter writer)
        {
            writer.Write(DiscordId);
            writer.Write(Name);
            writer.Write(MillisecondsPlayed);
        }

        public override void Deserialize(BinaryReader reader)
        {
            DiscordId = reader.ReadUInt64();
            Name = reader.ReadString();
            MillisecondsPlayed = reader.ReadUInt64();
        }
    }
}
