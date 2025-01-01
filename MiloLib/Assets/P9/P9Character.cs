using MiloLib.Assets.Char;
using MiloLib.Assets.Rnd;
using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets.P9
{
    [Name("P9 Director"), Description("P9 Director, sits in each song file and manages camera + scene changes")]
    public class P9Character : Character
    {
        public ushort altRevision;
        public ushort revision;

        public Character character = new(0);

        public float headLookatWeight;

        public Symbol tempo = new(0, "");
        public Symbol era = new(0, "");
        public Symbol song = new(0, "");
        public Symbol venue = new(0, "");

        public int instrumentIndex;

        public Symbol waypoint = new(0, "");
        public Symbol micIk = new(0, "");

        public P9Character(ushort revision, ushort altRevision = 0) : base(revision, altRevision)
        {
            revision = revision;
            altRevision = altRevision;
            return;
        }

        public P9Character Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            if (revision != 8)
                throw new UnsupportedAssetRevisionException("P9Character", revision);

            character = new Character(0).Read(reader, false, parent, entry);

            headLookatWeight = reader.ReadFloat();

            tempo = Symbol.Read(reader);
            era = Symbol.Read(reader);
            song = Symbol.Read(reader);
            venue = Symbol.Read(reader);

            instrumentIndex = reader.ReadInt32();

            waypoint = Symbol.Read(reader);
            micIk = Symbol.Read(reader);

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw new Exception("Got to end of standalone asset but didn't find the expected end bytes, read likely did not succeed");

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            character.Write(writer, false, parent, entry);

            writer.WriteFloat(headLookatWeight);

            Symbol.Write(writer, tempo);
            Symbol.Write(writer, era);
            Symbol.Write(writer, song);
            Symbol.Write(writer, venue);

            writer.WriteInt32(instrumentIndex);

            Symbol.Write(writer, waypoint);
            Symbol.Write(writer, micIk);

            if (standalone)
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
        }

        public override bool IsDirectory()
        {
            return true;
        }
    }
}