using MiloLib.Assets.Rnd;
using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets.P9
{
    [Name("P9 Director"), Description("P9 Director, sits in each song file and manages camera + scene changes")]
    public class P9Director : Object
    {
        private Dictionary<Game.MiloGame, uint> gameRevisions = new Dictionary<Game.MiloGame, uint>
        {
            { Game.MiloGame.TheBeatlesRockBand, 5 },
            { Game.MiloGame.GreenDayRockBand, 5 },
            { Game.MiloGame.RockBand3, 5 },
        };
        private ushort altRevision;
        private ushort revision;

        public ObjectFields objFields1 = new();
        public ObjectFields objFields2 = new();

        public RndDrawable draw = new();

        public Symbol venue = new(0, "");

        public P9Director Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            objFields1 = objFields1.Read(reader, parent, entry);
            objFields2 = objFields2.Read(reader, parent, entry);

            draw = new RndDrawable().Read(reader, false, parent, entry);
            venue = Symbol.Read(reader);

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw MiloLib.Exceptions.MiloAssetReadException.EndBytesNotFound(parent, entry, reader.BaseStream.Position);
            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            objFields1.Write(writer, parent);
            objFields2.Write(writer, parent);

            draw.Write(writer, false, parent);
            Symbol.Write(writer, venue);

            if (standalone)
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
        }
    }
}