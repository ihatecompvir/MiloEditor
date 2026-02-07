using MiloLib.Assets.Rnd;
using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets.World
{
    // i fucking hate this
    [Name("WorldInstance"), Description("Shared instance of a RndDir")]
    public class WorldInstance : RndDir
    {
        [Name("Persistent Objects")]
        public class PersistentObjects
        {
            public Symbol unknownString = new(0, "");
            public Symbol unknownCamReference = new(0, "");
            public ObjectFields objFields = new ObjectFields();

            public RndAnimatable anim = new RndAnimatable();
            public RndDrawable draw = new RndDrawable();
            public RndTrans trans = new RndTrans();

            public Symbol environ = new(0, "");
            public Symbol testEvent = new(0, "");

            private uint stringTableCount;
            private uint stringTableSize;

            public List<DirectoryMeta.Entry> perObjs = new();

            public bool hasPostLoadFields = false;

            private static bool IsEndBytes(EndianReader reader)
            {
                uint expected = reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD;
                long pos = reader.BaseStream.Position;
                uint val = reader.ReadUInt32();
                reader.BaseStream.Position = pos;
                return val == expected;
            }

            public PersistentObjects Read(EndianReader reader, DirectoryMeta parent, DirectoryMeta.Entry entry, uint revision)
            {
                if (entry.isProxy)
                {
                    if (!IsEndBytes(reader))
                    {
                        hasPostLoadFields = true;
 
                        unknownString = Symbol.Read(reader);
                        unknownCamReference = Symbol.Read(reader);
                        objFields.root.Read(reader);
                        objFields.note = Symbol.Read(reader);

                        anim = anim.Read(reader, parent, entry);
                        draw = draw.Read(reader, false, parent, entry);
                        trans = trans.Read(reader, false, parent, entry);

                        environ = Symbol.Read(reader);
                        testEvent = Symbol.Read(reader);

                        if (revision > 1)
                        {
                            if (revision > 2)
                            {
                                stringTableCount = reader.ReadUInt32();
                                stringTableSize = reader.ReadUInt32();
                            }

                            uint objectCount = reader.ReadUInt32();
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
                                        throw new Exception("Unknown persistent object type " + perObjs[i].type.value + " in WorldInstance");
                                }
                            }
                        }
                    }
                }
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw new Exception("Got to end of persistent objects but didn't find the expected end bytes, read likely did not succeed");

                return this;
            }

            public void Write(EndianWriter writer, DirectoryMeta parent, DirectoryMeta.Entry? entry, uint revision)
            {
                if (hasPostLoadFields)
                {
                    Symbol.Write(writer, unknownString);
                    Symbol.Write(writer, unknownCamReference);
                    objFields.root.Write(writer);
                    Symbol.Write(writer, objFields.note);

                    anim.Write(writer);
                    draw.Write(writer, false, parent, true);
                    trans.Write(writer, false, parent, true);

                    Symbol.Write(writer, environ);
                    Symbol.Write(writer, testEvent);

                    if (revision > 1)
                    {
                        if (revision > 2)
                        {
                            writer.WriteUInt32(stringTableCount);
                            writer.WriteUInt32(stringTableSize);
                        }

                        writer.WriteUInt32((uint)perObjs.Count);

                        for (int i = 0; i < perObjs.Count; i++)
                        {
                            Symbol.Write(writer, perObjs[i].type);
                            Symbol.Write(writer, perObjs[i].name);
                        }

                        for (int i = 0; i < perObjs.Count; i++)
                        {
                            switch (perObjs[i].type.value)
                            {
                                case "Mesh":
                                    ((RndMesh)perObjs[i].obj).Write(writer, false, parent, perObjs[i]);
                                    break;
                                default:
                                    throw new Exception("Unknown persistent object type " + perObjs[i].type.value + " in WorldInstance");
                            }
                        }
                    }
                }

                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
            }
        }

        public ushort altRevision;
        public ushort revision;

        [Name("Instance File"), Description("Which file we instance, only set in instances")]
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
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)(combinedRevision >> 16 & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)(combinedRevision >> 16 & 0xFFFF));

            if (revision != 0)
            {
                filePath = Symbol.Read(reader);
            }
            else
            {
                dir = Symbol.Read(reader);
            }

            base.Read(reader, false, parent, entry);

            if (standalone && !entry.isProxy)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw MiloLib.Exceptions.MiloAssetReadException.EndBytesNotFound(parent, entry, reader.BaseStream.Position);

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)(altRevision << 16 | revision) : (uint)(revision << 16 | altRevision));

            if (revision != 0)
            {
                Symbol.Write(writer, filePath);
            }
            else
            {
                Symbol.Write(writer, dir);
            }

            base.Write(writer, false, parent, entry);

            if (standalone && !entry.isProxy)
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
        }

        public override bool IsDirectory()
        {
            return true;
        }

    }
}
