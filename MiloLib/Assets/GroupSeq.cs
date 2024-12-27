using System.Reflection.Metadata;
using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets
{
    [Name("GroupSeq"), Description("A sequence which plays other sequences.  Abstract base class.")]
    public class GroupSeq : Object
    {
        public ushort altRevision;
        public ushort revision;

        public Sequence seq = new();

        private uint childrenCount;
        public List<Symbol> children = new();

        public GroupSeq Read(EndianReader reader, bool standalone)
        {
            altRevision = reader.ReadUInt16();
            revision = reader.ReadUInt16();

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
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw new Exception("Got to end of standalone asset but didn't find the expected end bytes, read likely did not succeed");
            }

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone)
        {
            writer.WriteUInt16(altRevision);
            writer.WriteUInt16(revision);

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