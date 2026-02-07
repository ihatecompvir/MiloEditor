using MiloLib.Assets.Rnd;
using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets.World
{
    [Name("WorldReflection"), Description("Reflects all drawables in draws.")]
    public class WorldReflection : Object
    {
        private ushort altRevision;
        private ushort revision;

        public RndTrans trans = new();
        public RndDrawable draw = new();

        public float verticalStretch;
        private uint drawsCount;
        public List<Symbol> draws = new();

        private uint hideListCount;
        public List<Symbol> hideList = new();
        private uint showListCount;
        public List<Symbol> showList = new();

        private uint lodCharsCount;
        public List<Symbol> lodChars = new();

        public WorldReflection Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            base.Read(reader, false, parent, entry);
            trans = trans.Read(reader, false, parent, entry);
            draw = draw.Read(reader, false, parent, entry);

            verticalStretch = reader.ReadFloat();

            drawsCount = reader.ReadUInt32();
            for (int i = 0; i < drawsCount; i++)
            {
                draws.Add(Symbol.Read(reader));
            }

            if (revision > 1)
            {
                hideListCount = reader.ReadUInt32();
                for (int i = 0; i < hideListCount; i++)
                {
                    hideList.Add(Symbol.Read(reader));
                }

                showListCount = reader.ReadUInt32();
                for (int i = 0; i < showListCount; i++)
                {
                    showList.Add(Symbol.Read(reader));
                }
            }

            if (revision > 2)
            {
                lodCharsCount = reader.ReadUInt32();
                for (int i = 0; i < lodCharsCount; i++)
                {
                    lodChars.Add(Symbol.Read(reader));
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
            trans.Write(writer, false, parent, true);
            draw.Write(writer, false, parent, true);

            writer.WriteFloat(verticalStretch);

            writer.WriteUInt32((uint)draws.Count);
            foreach (var draw in draws)
            {
                Symbol.Write(writer, draw);
            }

            if (revision > 1)
            {
                writer.WriteUInt32((uint)hideList.Count);
                foreach (var hide in hideList)
                {
                    Symbol.Write(writer, hide);
                }

                writer.WriteUInt32((uint)showList.Count);
                foreach (var show in showList)
                {
                    Symbol.Write(writer, show);
                }
            }

            if (revision > 2)
            {
                writer.WriteUInt32((uint)lodChars.Count);
                foreach (var lod in lodChars)
                {
                    Symbol.Write(writer, lod);
                }
            }

            if (standalone)
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
        }

    }
}
