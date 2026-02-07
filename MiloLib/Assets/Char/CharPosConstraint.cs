using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets.Char
{
    [Name("CharForeTwist"), Description("Forces the targets to be within a world space bounding box relative to source.")]
    public class CharPosConstraint : Object
    {
        private ushort altRevision;
        private ushort revision;

        public Symbol src = new(0, "");
        private uint targetCount;
        public List<Symbol> targets = new List<Symbol>();

        public float[] box = new float[6];

        public CharPosConstraint Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            base.Read(reader, false, parent, entry);
            targetCount = reader.ReadUInt32();
            for (int i = 0; i < targetCount; i++)
                targets.Add(Symbol.Read(reader));
            src = Symbol.Read(reader);
            if (revision > 1)
            {
                box[0] = reader.ReadFloat();
                box[1] = reader.ReadFloat();
                box[2] = reader.ReadFloat();
                box[3] = reader.ReadFloat();
                box[4] = reader.ReadFloat();
                box[5] = reader.ReadFloat();
            }

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw MiloLib.Exceptions.MiloAssetReadException.EndBytesNotFound(parent, entry, reader.BaseStream.Position);

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            base.Write(writer, false, parent, entry);
            writer.WriteUInt32((uint)targets.Count);
            foreach (Symbol target in targets)
                Symbol.Write(writer, target);
            Symbol.Write(writer, src);
            if (revision > 1)
            {
                writer.WriteFloat(box[0]);
                writer.WriteFloat(box[1]);
                writer.WriteFloat(box[2]);
                writer.WriteFloat(box[3]);
                writer.WriteFloat(box[4]);
                writer.WriteFloat(box[5]);
            }

            if (standalone)
                writer.WriteEndBytes();
        }

    }
}
