using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets.Rnd
{
    [Name("MotionBlur"), Description("Contains a list of objects to apply object based motion blur")]
    public class RndMotionBlur : Object
    {
        private ushort altRevision;
        private ushort revision;

        public RndDrawable draw = new();

        private uint drawListCount;
        public List<Symbol> draws = new();

        public RndMotionBlur Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            base.Read(reader, false, parent, entry);
            draw.Read(reader, false, parent, entry);

            drawListCount = reader.ReadUInt32();
            for (int i = 0; i < drawListCount; i++)
            {
                draws.Add(Symbol.Read(reader));
            }

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw MiloLib.Exceptions.MiloAssetReadException.EndBytesNotFound(parent, entry, reader.BaseStream.Position);

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            base.Write(writer, false, parent, entry);
            draw.Write(writer, false, parent, null);

            writer.WriteUInt32((uint)draws.Count);
            foreach (Symbol draw in draws)
            {
                Symbol.Write(writer, draw);
            }

            if (standalone)
                writer.WriteEndBytes();
        }

    }
}
