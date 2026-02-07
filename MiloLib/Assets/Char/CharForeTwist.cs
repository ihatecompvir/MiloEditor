using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets.Char
{
    [Name("CharForeTwist"), Description("Does all interpolation for the forearm. Assumes: foretwist1 and forearm are under upperarm. foretwist2 is under foretwist1 and that hand is under forearm. on the left hand offset rotation is usually 90 on the left, and -90 on the right. Feeds the bones when executed.")]
    public class CharForeTwist : Object
    {
        private ushort altRevision;
        private ushort revision;

        public Symbol hand = new(0, "");
        public Symbol twist = new(0, "");

        public float offset;
        public float bias;
        public int unk;

        public CharForeTwist Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            base.Read(reader, false, parent, entry);
            offset = reader.ReadFloat();
            hand = Symbol.Read(reader);
            twist = Symbol.Read(reader);
            if (revision == 2)
            {
                unk = reader.ReadInt32();
            }
            if (revision > 3)
                bias = reader.ReadFloat();

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw MiloLib.Exceptions.MiloAssetReadException.EndBytesNotFound(parent, entry, reader.BaseStream.Position);

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            base.Write(writer, false, parent, entry);
            writer.WriteFloat(offset);
            Symbol.Write(writer, hand);
            Symbol.Write(writer, twist);
            if (revision == 2)
            {
                writer.WriteInt32(unk);
            }
            if (revision > 3)
                writer.WriteFloat(bias);

            if (standalone)
                writer.WriteEndBytes();
        }

    }
}
