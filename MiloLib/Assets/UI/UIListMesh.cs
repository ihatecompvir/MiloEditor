using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets.UI
{
    [Name("UIListMesh"), Description("Custom slot for use with UIList")]
    public class UIListMesh : UIListSlot
    {
        private ushort altRevision;
        private ushort revision;

        [Name("Mesh"), Description("mesh to draw/transform")]
        public Symbol mMesh = new(0, "");
        [Name("Default Material"), Description("default material")]
        public Symbol mDefaultMat = new(0, "");

        public UIListMesh Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            base.Read(reader, false, parent, entry);

            mMesh = Symbol.Read(reader);
            mDefaultMat = Symbol.Read(reader);

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw MiloLib.Exceptions.MiloAssetReadException.EndBytesNotFound(parent, entry, reader.BaseStream.Position);

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            base.Write(writer, false, parent, entry);

            Symbol.Write(writer, mMesh);
            Symbol.Write(writer, mDefaultMat);

            if (standalone)
                writer.WriteEndBytes();
        }

    }
}
