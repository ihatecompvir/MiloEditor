using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets.Rnd
{
    [Name("Set"), Description("A group of objects to propagate animation and messages")]
    public class RndSet : Object
    {
        private ushort altRevision;
        private ushort revision;

        private uint setObjectsCount;
        public List<Symbol> setObjects = new();

        public RndSet Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)(combinedRevision >> 16 & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)(combinedRevision >> 16 & 0xFFFF));

            objFields.Read(reader, parent, entry);

            setObjectsCount = reader.ReadUInt32();
            for (int i = 0; i < setObjectsCount; i++)
            {
                setObjects.Add(Symbol.Read(reader));
            }

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw MiloLib.Exceptions.MiloAssetReadException.EndBytesNotFound(parent, entry, reader.BaseStream.Position);

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)(altRevision << 16 | revision) : (uint)(revision << 16 | altRevision));

            base.Write(writer, false, parent, entry);

            writer.WriteUInt32((uint)setObjects.Count);
            foreach (Symbol setObj in setObjects)
            {
                Symbol.Write(writer, setObj);
            }

            if (standalone)
                writer.WriteEndBytes();
        }

    }
}
