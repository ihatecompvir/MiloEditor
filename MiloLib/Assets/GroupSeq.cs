using System.Reflection.Metadata;
using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets
{
    [Name("GroupSeq"), Description("A sequence which plays other sequences.  Abstract base class.")]
    public class GroupSeq : Object
    {
        public uint revision;

        public Sequence seq = new();

        private uint childrenCount;
        public List<Symbol> children = new();

        public GroupSeq Read(EndianReader reader, bool standalone)
        {
            revision = reader.ReadUInt32();

            if (1 < revision)
            {
                seq.Read(reader);

                childrenCount = reader.ReadUInt32();
                for (int i = 0; i < childrenCount; i++)
                {
                    children.Add(Symbol.Read(reader));
                }
            }

            if (standalone)
            {
                reader.BaseStream.Position += 4;
            }

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone)
        {
            writer.WriteUInt32(revision);

            if (1 < revision)
            {
                seq.Write(writer, false);

                writer.WriteUInt32((uint)children.Count);
                foreach (var child in children)
                {
                    Symbol.Write(writer, child);
                }
            }

            if (standalone)
            {
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
            }
        }
    }
}