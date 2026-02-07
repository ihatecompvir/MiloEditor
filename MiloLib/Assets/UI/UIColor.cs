using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets.UI
{
    [Name("UIColor"), Description("Just a color, used by UI components")]
    public class UIColor : Object
    {

        private ushort altRevision;
        private ushort revision;

        [Name("Color")]
        public HmxColor4 color = new();

        public UIColor Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            base.objFields.Read(reader, parent, entry);

            color.Read(reader);

            if (standalone)
            {
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw MiloLib.Exceptions.MiloAssetReadException.EndBytesNotFound(parent, entry, reader.BaseStream.Position);
            }

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            base.objFields.Write(writer, parent);

            color.Write(writer);

            if (standalone)
            {
                writer.WriteEndBytes();
            }
        }
    }
}
