using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord.Soundboard.Data
{
    public abstract class Record
    {
        public int ID { get; set; }

        public abstract void Serialize(BinaryWriter writer);
        public abstract void Deserialize(BinaryReader reader);

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }
    }
}
