using MiloLib.Assets.Rnd;
using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets.Band
{
    [Name("BandPlacer"), Description("Stand-in for dir in the UI, polls and draws them, can center about a mesh inside it.")]
    public class BandPlacer : Object
    {
        private Dictionary<Game.MiloGame, uint> gameRevisions = new Dictionary<Game.MiloGame, uint>
        {
            // no dirs before this
            { Game.MiloGame.GuitarHero2_PS2, 2 },
            { Game.MiloGame.GuitarHero2_360, 2 },
        };
        private ushort altRevision;
        private ushort revision;

        public RndDrawable draw = new();
        public RndTrans trans = new();

        public Symbol center = new(0, "");

        public BandPlacer Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            base.objFields = base.objFields.Read(reader, parent, entry);

            draw = draw.Read(reader, false, parent, entry);
            trans = trans.Read(reader, false, parent, entry);

            center = Symbol.Read(reader);

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw MiloLib.Exceptions.MiloAssetReadException.EndBytesNotFound(parent, entry, reader.BaseStream.Position);

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            objFields.Write(writer, parent);

            draw.Write(writer, false, parent, true);
            trans.Write(writer, false, parent, true);

            Symbol.Write(writer, center);

            if (standalone)
            {
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
            }
        }
    }
}