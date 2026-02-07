using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets.UI
{
    [Name("UIListSlot"), Description("Base functionality for UIList slots")]
    public class UIListSlot : UIListWidget
    {
        public enum UIListSlotDrawType
        {
            kUIListSlotDrawAlways,
            kUIListSlotDrawHighlight,
            kUIListSlotDrawNoHighlight
        };

        private ushort altRevision;
        private ushort revision;

        public UIListSlotDrawType mSlotDrawType;

        public UIListSlot Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            base.Read(reader, false, parent, entry);

            mSlotDrawType = (UIListSlotDrawType)reader.ReadInt32();

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw MiloLib.Exceptions.MiloAssetReadException.EndBytesNotFound(parent, entry, reader.BaseStream.Position);

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            base.Write(writer, false, parent, entry);

            writer.WriteInt32((int)mSlotDrawType);

            if (standalone)
                writer.WriteEndBytes();
        }

    }
}
