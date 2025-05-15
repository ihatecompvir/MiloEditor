using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets.UI
{
    [Name("UIListWidget"), Description("Base functionality for UIList widgets")]
    public class UIListWidget : Object
    {
        public enum UIListWidgetState
        {
            kUIListWidgetActive,
            kUIListWidgetHighlight,
            kUIListWidgetInactive,
            kNumUIListWidgetStates
        };

        public enum UIListWidgetDrawType
        {
            kUIListWidgetDrawAlways,
            kUIListWidgetDrawOnlyFocused,
            kUIListWidgetDrawFocusedOrManual,
            kNumUIListWidgetDrawTypes
        };
        private ushort altRevision;
        private ushort revision;

        [Name("Draw Order"), Description("order this widget will be drawn")]
        public float mDrawOrder;

        public int unkInt1;
        [MinVersion(1)]
        public int unkInt2;
        [MinVersion(1)]
        public int unkInt3;

        [Name("Disabled Alpha Scale"), Description("scale for widget alpha when list is disabled"), MinVersion(2)]
        public float mDisabledAlphaScale;

        [Name("Widget Draw Type"), Description("under what conditions to draw this widget")]
        public UIListWidgetDrawType mDrawType;

        public List<Symbol> colorPtrs = new();

        [Name("Default Color"), Description("color applied if no state specific color is set")]
        public Symbol mDefaultColor = new(0, "");

        public UIListWidget Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            base.Read(reader, false, parent, entry);

            mDrawOrder = reader.ReadFloat();

            if (revision < 1)
            {
                unkInt2 = reader.ReadInt32();
                unkInt2 = reader.ReadInt32();
            }

            mDefaultColor = Symbol.Read(reader);
            mDrawType = (UIListWidgetDrawType)reader.ReadInt32();

            if (revision >= 2)
                mDisabledAlphaScale = reader.ReadFloat();

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    colorPtrs.Add(Symbol.Read(reader));
                }
            }

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw new Exception("Got to end of standalone asset but didn't find the expected end bytes, read likely did not succeed");

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            base.Write(writer, false, parent, entry);

            writer.WriteFloat(mDrawOrder);

            if (revision < 1)
            {
                writer.WriteInt32(unkInt2);
                writer.WriteInt32(unkInt2);
            }

            Symbol.Write(writer, mDefaultColor);
            writer.WriteInt32((int)mDrawType);

            if (revision >= 2)
                writer.WriteFloat(mDisabledAlphaScale);

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    Symbol.Write(writer, colorPtrs[i * 5 + j]);
                }
            }

            if (standalone)
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
        }

    }
}
