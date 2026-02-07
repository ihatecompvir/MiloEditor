using MiloLib.Assets.Rnd;
using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets.Band
{
    [Name("BandDirector"), Description("Band Director, sits in each song file and manages camera + scene changes")]
    public class BandDirector : Object
    {
        private ushort altRevision;
        private ushort revision;

        public Object poll = new();
        public RndDrawable draw = new();
        public Object obj3 = new();
        public Symbol unkSym = new(0, "");
        public Symbol unkSym2 = new(0, "");

        public BandDirector Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            base.Read(reader, false, parent, entry);
            poll = poll.Read(reader, false, parent, entry);
            draw = draw.Read(reader, false, parent, entry);
            if (revision < 5)
                obj3 = obj3.Read(reader, false, parent, entry);
            if (revision < 6)
                unkSym = Symbol.Read(reader);
            if (revision < 4)
                unkSym2 = Symbol.Read(reader);


            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw MiloLib.Exceptions.MiloAssetReadException.EndBytesNotFound(parent, entry, reader.BaseStream.Position);

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            base.Write(writer, false, parent, entry);

            poll.Write(writer, false, parent, entry);
            draw.Write(writer, false, parent, true);
            if (revision < 5)
                obj3.Write(writer, false, parent, entry);
            if (revision < 6)
                Symbol.Write(writer, unkSym);
            if (revision < 4)
                Symbol.Write(writer, unkSym2);

            if (standalone)
                writer.WriteEndBytes();
        }

    }
}
