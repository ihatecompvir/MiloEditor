using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets.Char
{
    [Name("CharClipGroup"), Description("A related group of animations.  Gives you the lru one.  Usually no extension.")]
    public class CharClipGroup : Object
    {
        private ushort altRevision;
        private ushort revision;

        private uint clipCount;
        public List<Symbol> clips = new();

        public uint which;

        public uint flags;

        public CharClipGroup Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            base.Read(reader, false, parent, entry);

            clipCount = reader.ReadUInt32();
            for (int i = 0; i < clipCount; i++)
            {
                clips.Add(Symbol.Read(reader));
            }

            which = reader.ReadUInt32();

            if (revision > 1)
                flags = reader.ReadUInt32();

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw new Exception("Got to end of standalone asset but didn't find the expected end bytes, read likely did not succeed");

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            base.Write(writer, false, parent, entry);

            writer.WriteUInt32((uint)clips.Count);
            foreach (var clip in clips)
            {
                Symbol.Write(writer, clip);
            }

            writer.WriteUInt32(which);
            if (revision > 1)
                writer.WriteUInt32(flags);

            if (standalone)
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
        }

    }
}
