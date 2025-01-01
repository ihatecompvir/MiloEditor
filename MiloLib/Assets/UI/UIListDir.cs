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

        public ushort altRevision;
        public ushort revision;

        public UIListOrientation orientation;

        public int fadeOffset;
        public float elementSpacing;
        public float scrollHighlightChange;
        public bool testMode;
        public int testNumData;
        public float testGapSize;
        public bool testDisableElements;
        public int mDirection;
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

            // this is not completely correct... TODO: fix this

            orientation = (UIListOrientation)reader.ReadUInt32();

            fadeOffset = reader.ReadInt32();

            testMode = reader.ReadBoolean();
            elementSpacing = reader.ReadFloat();
            testNumData = reader.ReadInt32();
            speed = reader.ReadFloat();
            compState = reader.ReadUInt32();
            testGapSize = reader.ReadFloat();
            testDisableElements = reader.ReadBoolean();

            if (revision != 0)
            {
                scrollHighlightChange = reader.ReadFloat();
            }

            reader.ReadUInt32();

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw new Exception("Got to end of standalone asset but didn't find the expected end bytes, read likely did not succeed");

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            base.Write(writer, false);

            writer.WriteUInt32((uint)orientation);

            writer.WriteInt32(fadeOffset);

            writer.WriteBoolean(testMode);

            writer.WriteFloat(elementSpacing);

            writer.WriteInt32(testNumData);

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