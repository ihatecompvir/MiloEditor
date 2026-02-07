using MiloLib.Assets.UI;
using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets.Band.UI
{
    [Name("BandList"), Description("Band specific UIList")]
    public class BandList : UIList
    {
        public class HighlightObjects
        {
            [Name("Target Object"), Description("The object to attach to the highlight")]
            public Symbol targetObject = new(0, "");
            [Name("X Offset"), Description("x offset from list position")]
            public float xOffset;
            [Name("Y Offset"), Description("y offset from list position")]
            public float yOffset;
            [Name("Z Offset"), Description("z offset from list position")]
            public float zOffset;

            public HighlightObjects Read(EndianReader reader)
            {
                targetObject = Symbol.Read(reader);
                xOffset = reader.ReadFloat();
                yOffset = reader.ReadFloat();
                zOffset = reader.ReadFloat();
                return this;
            }

            public void Write(EndianWriter writer)
            {
                Symbol.Write(writer, targetObject);
                writer.WriteFloat(xOffset);
                writer.WriteFloat(yOffset);
                writer.WriteFloat(zOffset);
            }
        }
        private ushort altRevision;
        private ushort revision;

        [Name("Focus Anim"), Description("Animation to play on a selected entry to transition into and out of focus")]
        public Symbol focusAnim = new(0, "");
        [Name("Pulse Anim"), Description("Animation to play on a selected entry after focus is played - focus anim must exist")]
        public Symbol pulseAnim = new(0, "");

        [Name("Reveal Anim"), Description("animation to play on each entry when list is revealed")]
        public Symbol revealAnim = new(0, "");
        [Name("Conceal Anim"), Description("animation to play on each entry when list is concealed")]
        public Symbol concealAnim = new(0, "");
        [Name("Reveal Sound"), Description("sound to play on each entry when list is revealed")]
        public Symbol revealSound = new(0, "");
        [Name("Conceal Sound"), Description("sound to play on each entry when list is concealed")]
        public Symbol concealSound = new(0, "");

        [Name("Reveal Sound Delay"), Description("delay for sound to play on each entry when list is revealed")]
        public float revealSoundDelay;
        [Name("Conceal Sound Delay"), Description("delay for sound to play on each entry when list is concealed")]
        public float concealSoundDelay;
        [Name("Reveal Start Delay"), Description("delay before playing reveal animation")]
        public float revealStartDelay;
        [Name("Reveal Entry Delay"), Description("delay between list entries playing reveal animation")]
        public float revealEntryDelay;
        [Name("Reveal Scale"), Description("amount to scale reveal animation")]
        public float revealScale;
        [Name("Conceal Start Delay"), Description("delay before playing conceal animation")]
        public float concealStartDelay;
        [Name("Conceal Entry Delay"), Description("delay between list entries playing conceal animation")]
        public float concealEntryDelay;
        [Name("Conceal Scale"), Description("amount to scale conceal animation")]
        public float concealScale;
        [Name("Auto Reveal"), Description("Whether or not to start revealed")]
        public bool autoReveal;

        private uint highlightObjectsCount;
        public List<HighlightObjects> highlightObjects = new();

        public BandList Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            base.Read(reader, false, parent, entry);

            if (revision >= 0x12)
            {
                focusAnim = Symbol.Read(reader);
                pulseAnim = Symbol.Read(reader);
            }

            if (revision >= 0x13)
            {
                revealAnim = Symbol.Read(reader);
                revealStartDelay = reader.ReadFloat();
                revealEntryDelay = reader.ReadFloat();
                concealAnim = Symbol.Read(reader);
                concealStartDelay = reader.ReadFloat();
                concealEntryDelay = reader.ReadFloat();
            }
            if (revision >= 0x14)
            {
                revealScale = reader.ReadFloat();
                concealScale = reader.ReadFloat();
                autoReveal = reader.ReadBoolean();
            }

            if (revision >= 0x15)
            {
                revealSound = Symbol.Read(reader);
                concealSound = Symbol.Read(reader);
                revealSoundDelay = reader.ReadFloat();
                concealSoundDelay = reader.ReadFloat();
            }
            if (revision >= 0x16)
            {
                highlightObjectsCount = reader.ReadUInt32();
                for (int i = 0; i < highlightObjectsCount; i++)
                {
                    highlightObjects.Add(new HighlightObjects().Read(reader));
                }
            }

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw MiloLib.Exceptions.MiloAssetReadException.EndBytesNotFound(parent, entry, reader.BaseStream.Position);

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            base.Write(writer, false, parent, entry);

            if (revision >= 0x12)
            {
                Symbol.Write(writer, focusAnim);
                Symbol.Write(writer, pulseAnim);
            }

            if (revision >= 0x13)
            {
                Symbol.Write(writer, revealAnim);
                writer.WriteFloat(revealStartDelay);
                writer.WriteFloat(revealEntryDelay);
                Symbol.Write(writer, concealAnim);
                writer.WriteFloat(concealStartDelay);
                writer.WriteFloat(concealEntryDelay);
            }

            if (revision >= 0x14)
            {
                writer.WriteFloat(revealScale);
                writer.WriteFloat(concealScale);
                writer.WriteBoolean(autoReveal);
            }

            if (revision >= 0x15)
            {
                Symbol.Write(writer, revealSound);
                Symbol.Write(writer, concealSound);
                writer.WriteFloat(revealSoundDelay);
                writer.WriteFloat(concealSoundDelay);
            }

            if (revision >= 0x16)
            {
                writer.WriteUInt32((uint)highlightObjects.Count);
                foreach (var highlightObject in highlightObjects)
                {
                    highlightObject.Write(writer);
                }
            }

            if (standalone)
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
        }

    }
}
