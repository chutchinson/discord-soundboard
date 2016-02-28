using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Discord.Soundboard.Data
{
    public class RecordCollection<T> : IEnumerable<T>
        where T : Record, new()
    {
        private ISet<T> Records { get; set; }

        public int IndexPosition { get; set; }

        public RecordCollection()
        {
            IndexPosition = 0;
            Records = new HashSet<T>();
        }

        public void Clear()
        {
            Records.Clear();
        }

        public void Deserialize(BinaryReader reader)
        {
            IndexPosition = reader.ReadInt32();

            int count = reader.ReadInt32();

            for (int i = 0; i < count; i++)
            {
                var record = new T();
                record.ID = reader.ReadInt32();
                record.Deserialize(reader);
                Add(record);
            }
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(IndexPosition);
            writer.Write(Count);

            foreach (var record in Records)
            {
                writer.Write(record.ID);
                record.Serialize(writer);
            }
        }

        public int Count
        {
            get
            {
                return Records.Count;
            }
        }

        public bool Add(T record)
        {
            IndexPosition++;
            record.ID = IndexPosition;
            return Records.Add(record);
        }

        public bool Remove(T record)
        {
            return Records.Remove(record);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Records.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Records.GetEnumerator();
        }
    }
}
