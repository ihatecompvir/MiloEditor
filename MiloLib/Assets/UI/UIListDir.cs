using MiloLib.Assets.Rnd;
using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets.UI
{
    [Name("UIListDir"), Description("Component for displaying 1- or 2-dimensional lists of data. Can be oriented horizontally or vertically, can scroll normally or circularly, and can have any number of visible elements(even just one, a.k.a.a spin button)")]
    public class UIListDir : RndDir
    {
        public enum UIListOrientation
        {
            kUIListVertical,
            kUIListHorizontal
        }

        private ushort altRevision;
        private ushort revision;

        [Name("Orientation"), Description("scroll direction of list")]
        public UIListOrientation orientation;

        [Name("Fade Offset"), Description("Number of elements to fade from beginning/end of list")]
        public int fadeOffset;
        [Name("Element Spacing"), Description("spacing between elements")]
        public float elementSpacing;
        [Name("Scroll Highlight Change"), Description("point during scroll when highlight changes")]
        public float scrollHighlightChange;
        [Name("Test Mode"), Description("draw widgets in preview mode?")]
        public bool testMode;
        [Name("Test Num Data"), Description("total number of data elements")]
        public int testNumData;
        [Name("Test Gap Size"), Description("test gaps between elements")]
        public float testGapSize;
        [Name("Test Disable Elements"), Description("test disable every other element")]
        public bool testDisableElements;
        [Name("Test Num Display"), Description("number of elements to draw")]
        public int numDisplay;
        public float speed;
        public uint compState;

        public UIListDir(ushort revision, ushort altRevision = 0) : base(revision, altRevision)
        {
            revision = revision;
            altRevision = altRevision;
            return;
        }

        public UIListDir Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            base.Read(reader, false, parent, entry);

            orientation = (UIListOrientation)reader.ReadUInt32();
            fadeOffset = reader.ReadInt32();
            testMode = reader.ReadBoolean();
            numDisplay = reader.ReadInt32();
            elementSpacing = reader.ReadFloat();
            speed = reader.ReadFloat();
            testNumData = reader.ReadInt32();
            compState = reader.ReadUInt32();
            testGapSize = reader.ReadFloat();
            testDisableElements = reader.ReadBoolean();

            if (revision != 0)
            {
                scrollHighlightChange = reader.ReadFloat();
            }

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw MiloLib.Exceptions.MiloAssetReadException.EndBytesNotFound(parent, entry, reader.BaseStream.Position);

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            base.Write(writer, false, parent, entry);

            writer.WriteUInt32((uint)orientation);
            writer.WriteUInt32((uint)fadeOffset);
            writer.WriteBoolean(testMode);
            writer.WriteInt32(numDisplay);
            writer.WriteFloat(elementSpacing);
            writer.WriteFloat(speed);
            writer.WriteInt32(testNumData);
            writer.WriteUInt32(compState);
            writer.WriteFloat(testGapSize);
            writer.WriteBoolean(testDisableElements);

            if (revision != 0)
            {
                writer.WriteFloat(scrollHighlightChange);
            }

            if (standalone)
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
        }

        public override bool IsDirectory()
        {
            return true;
        }
    }
}