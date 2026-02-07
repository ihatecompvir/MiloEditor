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

            [Name("Selected"), Description("The file you want to merge")]
            public Symbol selected = new(0, "");

            [Name("Loaded"), Description("currently loaded file")]
            public Symbol loaded = new(0, "");

            [Name("Dir"), Description("Dir to merge into, proxy, for instance")]
            public Symbol dir = new(0, "");

            [Name("Proxy"), Description("If true, merges the Dir in as a proxy, rather than the individual objects")]
            public bool proxy;

            [Name("Subdirs"), Description("How to treat subdirs in the source")]
            public uint subdirs;

            [Name("Pre Clear"), Description("Delete the old objects right at StartLoad time")]
            public bool preClear;

            public Merger Read(EndianReader reader, ushort fmRevision)
            {
                name = Symbol.Read(reader);
                selected = Symbol.Read(reader);
                loaded = Symbol.Read(reader);
                dir = Symbol.Read(reader);

                if (fmRevision != 0)
                {
                    if (fmRevision != 4)
                        proxy = reader.ReadBoolean();
                    subdirs = reader.ReadUInt32();
                    if (fmRevision > 2)
                        preClear = reader.ReadBoolean();
                }
                return this;
            }

            public void Write(EndianWriter writer, ushort fmRevision)
            {
                Symbol.Write(writer, name);
                Symbol.Write(writer, selected);
                Symbol.Write(writer, loaded);
                Symbol.Write(writer, dir);

                if (fmRevision != 0)
                {
                    if (fmRevision != 4)
                        writer.WriteBoolean(proxy);
                    writer.WriteUInt32(subdirs);
                    if (fmRevision > 2)
                        writer.WriteBoolean(preClear);
                }
            }

            public override string ToString()
            {
                return $"{name} {selected} {loaded} {dir} {proxy} {subdirs} {preClear}";
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
                files.Add(new Merger().Read(reader, revision));
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
                file.Write(writer, revision);

            if (standalone)
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
        }

    }
}
