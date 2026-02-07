using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets.UI
{
    [Name("UIListArrow"), Description("Arrow widget for use with UIList")]
    public class UIListArrow : UIListWidget
    {
        public enum UIListArrowPosition
        {
            kUIListArrowBack,
            kUIListArrowNext
        };

        private ushort altRevision;
        private ushort revision;

        [Name("Mesh"), Description("arrow mesh to draw/transform")]
        public Symbol mMesh = new(0, "");
        [Name("ScrollAnim"), Description("animation to play on scroll"), MinVersion(1)]
        public Symbol mScrollAnim = new(0, "");

        [Name("Position"), Description("whether to position relative to first or last element")]
        public UIListArrowPosition mPosition;

        [Name("Show Only When Scrollable"), Description("show only when list is scrollable")]
        public bool mShowOnlyScroll;
        [Name("Relative to Highlight"), Description("position arrow relative to higlight")]
        public bool mOnHighlight;



        public UIListArrow Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            base.Read(reader, false, parent, entry);

            mMesh = Symbol.Read(reader);
            mPosition = (UIListArrowPosition)reader.ReadInt32();
            mShowOnlyScroll = reader.ReadBoolean();
            mOnHighlight = reader.ReadBoolean();

            if (revision != 0)
                mScrollAnim = Symbol.Read(reader);

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw MiloLib.Exceptions.MiloAssetReadException.EndBytesNotFound(parent, entry, reader.BaseStream.Position);

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            base.Write(writer, false, parent, entry);

            Symbol.Write(writer, mMesh);
            writer.WriteInt32((int)mPosition);
            writer.WriteBoolean(mShowOnlyScroll);
            writer.WriteBoolean(mOnHighlight);

            if (revision != 0)
                Symbol.Write(writer, mScrollAnim);

            if (standalone)
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
        }

    }
}
