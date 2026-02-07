using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets
{
    [Name("FileMerger"), Description("Merges files into ObjectDirs, much like a milo file merge.")]
    public class FileMerger : Object
    {
        public class Merger
        {
            [Name("Name"), Description("Name of the merger, just for identification")]
            public Symbol name = new(0, "");

            public uint unkInt1;
            public uint unkInt2;

            [Name("Name 2"), Description("The file you want to merge")]
            public Symbol name2 = new(0, "");

            public uint unkInt3;

            // one of these is this bool, need to adjust once this is decomped
            [Name("Proxy"), Description("If true, merges the Dir in as a proxy, rather than the individual objects")]
            public bool proxy;
            public bool unkBool2;

            public Merger Read(EndianReader reader)
            {
                name = Symbol.Read(reader);
                unkInt1 = reader.ReadUInt32();
                unkInt2 = reader.ReadUInt32();

                name2 = Symbol.Read(reader);

                unkInt3 = reader.ReadUInt32();
                proxy = reader.ReadBoolean();
                unkBool2 = reader.ReadBoolean();
                return this;
            }

            public void Write(EndianWriter writer)
            {
                Symbol.Write(writer, name);

                writer.WriteUInt32(unkInt1);
                writer.WriteUInt32(unkInt2);

                Symbol.Write(writer, name2);

                writer.WriteUInt32(unkInt3);
                writer.WriteBoolean(proxy);
                writer.WriteBoolean(unkBool2);
            }

            public override string ToString()
            {
                return $"{name} {unkInt1} {unkInt2} {name2} {unkInt3} {proxy} {unkBool2}";
            }
        }
        private ushort altRevision;
        private ushort revision;

        public Symbol unkSym = new(0, "");

        private uint filesCount;
        public List<Merger> files = new();

        public FileMerger Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            base.Read(reader, false, parent, entry);

            if (revision < 2)
            {
                unkSym = Symbol.Read(reader);
            }

            filesCount = reader.ReadUInt32();
            for (int i = 0; i < filesCount; i++)
            {
                files.Add(new Merger().Read(reader));
            }

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw MiloLib.Exceptions.MiloAssetReadException.EndBytesNotFound(parent, entry, reader.BaseStream.Position);

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            base.Write(writer, false, parent, entry);

            if (revision < 2)
            {
                Symbol.Write(writer, unkSym);
            }

            writer.WriteUInt32((uint)files.Count);
            foreach (var file in files)
                file.Write(writer);

            if (standalone)
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
        }

    }
}
