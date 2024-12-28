using MiloLib.Utils;
using MiloLib.Classes;
using MiloLib.Assets;

namespace MiloLib.Assets.Char
{
    [Name("CharClipSet"), Description("A CharClip container.")]
    public class CharClipSet : ObjectDir
    {
        [Name("Character File Path"), Description("Preview base character to use- for example, char/male/male_guitar.milo for male guitarist")]
        public Symbol charFilePath = new(0, "");

        [Name("Preview Clip"), Description("Pick a clip to play")]
        public Symbol previewClip = new(0, "");

        [Name("Filter Flags"), Description("Flags for filtering preview clip")]
        public uint filterFlags;

        [Name("BPM"), Description("bpm for clip playing")]
        public uint bpm;

        [Name("Preview Walk"), Description("Allow preview character to move around and walk?")]
        public bool previewWalk;

        [Name("Still Clip"), Description("Set this to view drummer play anims")]
        public Symbol stillClip = new(0, "");
        public Symbol unkSymbol = new(0, "");

        public CharClipSet Read(EndianReader reader, bool standalone)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            base.Read(reader, false);

            charFilePath = Symbol.Read(reader);
            previewClip = Symbol.Read(reader);
            filterFlags = reader.ReadUInt32();
            bpm = reader.ReadUInt32();
            previewWalk = reader.ReadBoolean();
            stillClip = Symbol.Read(reader);

            if (revision >= 25)
            {
                unkSymbol = Symbol.Read(reader);
            }

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw new Exception("Got to end of standalone asset but didn't find the expected end bytes, read likely did not succeed");

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            base.Write(writer, false);

            Symbol.Write(writer, charFilePath);
            Symbol.Write(writer, previewClip);
            writer.WriteUInt32(filterFlags);
            writer.WriteUInt32(bpm);
            writer.WriteBoolean(previewWalk);
            Symbol.Write(writer, stillClip);

            if (revision >= 25)
            {
                Symbol.Write(writer, unkSymbol);
            }

            if (standalone)
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
        }
    }
}
