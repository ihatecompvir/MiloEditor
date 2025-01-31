using MiloLib.Assets.Char;
using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets.Band
{
    [Name("BandCharacter"), Description("Band Character")]
    public class BandCharacter : Character
    {
        private ushort altRevision;
        private ushort revision;

        public int playFlags;

        [Name("Tempo"), Description("song tempo")]
        public Symbol tempo = new(0, "");

        public uint unkInt1;

        public Symbol unkSymbol = new(0, "");
        public Symbol unkSymbol2 = new(0, "");

        [Name("Drum Venue"), Description("venue type for drums"), MinVersion(7)]
        public Symbol drumVenue = new(0, "");

        public bool unknownBool;

        [Name("Instrument Type"), Description("character's current instrument"), MinVersion(8)]
        public Symbol instrumentType = new(0, "");

        [Name("Test Prefab"), Description("prefab to copy from or to"), MinVersion(2)]
        public BandCharDesc testPrefab = new();

        public BandCharacter(ushort revision, ushort altRevision = 0) : base(revision, altRevision)
        {
            revision = revision;
            altRevision = altRevision;
            return;
        }

        public BandCharacter Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            base.Read(reader, false, parent, entry);

            if (revision == 1)
            {
                if (standalone)
                    if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw new Exception("Got to end of standalone asset but didn't find the expected end bytes, read likely did not succeed");

                return this;
            }

            playFlags = reader.ReadInt32();
            tempo = Symbol.Read(reader);

            if (revision < 6)
            {
                if (revision < 4)
                {
                    unkInt1 = reader.ReadUInt32();
                    if (revision < 3)
                    {
                        unkSymbol = Symbol.Read(reader);
                    }
                }
                unkSymbol2 = Symbol.Read(reader);
            }

            if (revision > 6)
            {
                drumVenue = Symbol.Read(reader);
            }

            if (revision != 0)
            {
                testPrefab = testPrefab.Read(reader, false, parent, entry);
            }

            if (revision == 2 || revision == 3 || revision == 4)
            {
                unknownBool = reader.ReadBoolean();
            }

            if (revision > 7)
            {
                instrumentType = Symbol.Read(reader);
            }

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw new Exception("Got to end of standalone asset but didn't find the expected end bytes, read likely did not succeed");

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            base.Write(writer, false, parent, entry);

            if (revision == 1)
            {
                if (standalone)
                    writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });

                return;
            }

            writer.WriteInt32(playFlags);
            Symbol.Write(writer, tempo);

            if (revision < 6)
            {
                if (revision < 4)
                {
                    writer.WriteUInt32(unkInt1);
                    if (revision < 3)
                    {
                        Symbol.Write(writer, unkSymbol);
                    }
                }
                Symbol.Write(writer, unkSymbol2);
            }

            if (revision > 6)
            {
                Symbol.Write(writer, drumVenue);
            }

            if (revision != 0)
            {
                testPrefab.Write(writer, false, parent, entry);
            }

            if (revision == 2 || revision == 3 || revision == 4)
            {
                writer.WriteBoolean(unknownBool);
            }

            if (revision > 7)
            {
                Symbol.Write(writer, instrumentType);
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
