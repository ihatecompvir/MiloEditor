using MiloLib.Assets.Rnd;
using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets
{
    [Name("WorldInstance"), Description("")]
    public class WorldInstance : RndDir
    {

        // WHY
        [Name("Persistent Objects")]
        public class PersistentObjects
        {
            public byte[] empty = new byte[9];

            public RndAnimatable anim = new RndAnimatable();
            public RndDrawable draw = new RndDrawable();
            public RndTrans trans = new RndTrans();

            public Symbol environ = new(0, "");
            public Symbol unkSym = new(0, "");
            public uint unkInt3;

            public uint stringTableCount; // maybe? seems right
            public uint stringTableSize; // maybe? seems right

            private uint objectCount;
            private List<DirectoryMeta.Entry> perObjs = new(); // the list of perObjs


            public PersistentObjects Read(EndianReader reader, DirectoryMeta parent, DirectoryMeta.Entry entry)
            {
                empty = reader.ReadBlock(13);

                anim = anim.Read(reader, parent, entry);
                draw = draw.Read(reader, false, parent, entry);
                trans = trans.Read(reader, false, parent, entry);

                environ = Symbol.Read(reader);
                unkSym = Symbol.Read(reader);

                stringTableCount = reader.ReadUInt32();
                stringTableSize = reader.ReadUInt32();

                objectCount = reader.ReadUInt32();
                for (int i = 0; i < objectCount; i++)
                {
                    perObjs.Add(new DirectoryMeta.Entry(Symbol.Read(reader).value, Symbol.Read(reader).value, null));
                }

                for (int i = 0; i < objectCount; i++)
                {
                    switch (perObjs[i].type.value)
                    {
                        case "Mesh":
                            perObjs[i].obj = new RndMesh().Read(reader, false, parent, perObjs[i]);
                            break;
                        default:
                            throw new Exception("Unknown object type " + perObjs[i].type.value + " in WorldInstance PersistentObjects");
                    }
                }
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw new Exception("Got to end of persistent perObjs but didn't find the expected end bytes, read likely did not succeed");

                return this;
            }

            public void Write(EndianWriter writer, DirectoryMeta parent, DirectoryMeta.Entry? entry)
            {
                writer.WriteBlock(new byte[13]);

                anim.Write(writer);
                draw.Write(writer, false, true);
                trans.Write(writer, false, true);

                Symbol.Write(writer, environ);
                Symbol.Write(writer, unkSym);

                writer.WriteUInt32(stringTableCount);
                writer.WriteUInt32(stringTableSize);

                writer.WriteUInt32((uint)perObjs.Count);

                for (int i = 0; i < objectCount; i++)
                {
                    Symbol.Write(writer, perObjs[i].type);
                    Symbol.Write(writer, perObjs[i].name);
                }

                for (int i = 0; i < objectCount; i++)
                {
                    switch (perObjs[i].type.value)
                    {
                        case "Mesh":
                            ((RndMesh)perObjs[i].obj).Write(writer, false, parent, perObjs[i]);
                            break;
                    }
                }

                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
            }
        }
        public ushort altRevision;
        public ushort revision;

        public Symbol filePath = new(0, "");

        public Symbol dir = new(0, "");

        public PersistentObjects persistentObjects = new();


        public WorldInstance(ushort revision, ushort altRevision = 0) : base(revision, altRevision)
        {
            revision = revision;
            altRevision = altRevision;
            return;
        }

        public WorldInstance Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {

            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            if (revision != 0)
            {
                filePath = Symbol.Read(reader);
            }
            else
            {
                dir = Symbol.Read(reader);
            }

            base.Read(reader, false, parent, entry);

            if (standalone && !entry.isEntryInRootDir)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw new Exception("Got to end of standalone asset but didn't find the expected end bytes, read likely did not succeed");

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            if (revision != 0)
            {
                Symbol.Write(writer, filePath);
            }
            else
            {
                Symbol.Write(writer, dir);
            }

            base.Write(writer, false, parent, entry);

            if (standalone && !entry.isEntryInRootDir)
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
        }

        public override bool IsDirectory()
        {
            return true;
        }

    }
}
