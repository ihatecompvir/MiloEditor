using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets.Rnd
{
    [Name("Group"), Description("Represents a group of perObjs to which to propogate animation and messages.")]
    public class RndGroup : Object
    {
        private ushort altRevision;
        private ushort revision;

        public RndAnimatable anim = new();

        public RndTrans trans = new();

        public RndDrawable draw = new();

        private uint objectsCount;

        [Name("Objects"), Description("List of perObjs in the group"), MinVersion(11)]
        public List<Symbol> objects = new List<Symbol>();

        [Name("Environ"), MinVersion(16)]
        public Symbol environ = new(0, "");

        [Name("Draw Only"), Description("If set, only draws this member of the group"), MinVersion(13)]
        public Symbol drawOnly = new(0, "");

        [Name("LOD"), Description("Object to draw instead below lod_screen_size"), MinVersion(12), MaxVersion(15)]
        public Symbol lod = new(0, "");

        [Name("LOD Screen Size"), Description("Ratio of screen height for LOD"), MinVersion(12), MaxVersion(15)]
        public float lodScreenSize;

        [Name("Sort In World"), Description("Sort by distance to current camera per frame. This has a CPU cost if there are many perObjs."), MinVersion(14)]
        public bool sortInWorld;

        [MinVersion(7), MaxVersion(7)]
        public Symbol unknownSymbol = new(0, "");
        [MinVersion(7), MaxVersion(7)]
        public float lodWidth;
        [MinVersion(7), MaxVersion(7)]
        public float lodHeight;

        public RndGroup Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            if (revision > 7)
                base.Read(reader, false, parent, entry);

            anim = new RndAnimatable().Read(reader, parent, entry);
            trans = new RndTrans().Read(reader, false, parent, entry);
            draw = new RndDrawable().Read(reader, false, parent, entry);

            if (revision > 10)
            {
                objectsCount = reader.ReadUInt32();
                for (int i = 0; i < objectsCount; i++)
                {
                    objects.Add(Symbol.Read(reader));
                }

                if (revision < 16)
                    environ = Symbol.Read(reader);

                if (revision > 12)
                    drawOnly = Symbol.Read(reader);
            }

            if (revision > 11 && revision < 16)
            {
                lod = Symbol.Read(reader);
                lodScreenSize = reader.ReadFloat();
            }
            else if (revision == 4)
            {
                reader.ReadUInt32();

                objectsCount = reader.ReadUInt32();
                for (int i = 0; i < objectsCount; i++)
                {
                    objects.Add(Symbol.Read(reader));
                }

                unknownSymbol = Symbol.Read(reader);
                reader.ReadUInt32();
                reader.ReadUInt32();
            }

            if (revision == 7)
            {
                unknownSymbol = Symbol.Read(reader);
                lodWidth = reader.ReadFloat();
                lodHeight = reader.ReadFloat();
            }

            if (revision > 13)
                sortInWorld = reader.ReadBoolean();

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw new Exception("Got to end of standalone asset but didn't find the expected end bytes, read likely did not succeed");


            return this;

        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            if (revision > 7)
                base.Write(writer, false, parent, entry);


            anim.Write(writer);
            trans.Write(writer, false, true);
            draw.Write(writer, false, true);

            if (revision > 10)
            {
                writer.WriteUInt32((uint)objects.Count);

                foreach (var obj in objects)
                {
                    Symbol.Write(writer, obj);
                }

                if (revision < 16)
                    Symbol.Write(writer, environ);
                if (revision > 12)
                    Symbol.Write(writer, drawOnly);
            }

            if (revision > 11 && revision < 16)
            {
                Symbol.Write(writer, lod);
                writer.WriteFloat(lodScreenSize);
            }
            else if (revision == 4)
            {
                writer.WriteUInt32(0);

                writer.WriteUInt32(objectsCount);
                foreach (var obj in objects)
                {
                    Symbol.Write(writer, obj);
                }

                Symbol.Write(writer, unknownSymbol);
                writer.WriteUInt32(0);
                writer.WriteUInt32(0);
            }

            if (revision == 7)
            {
                Symbol.Write(writer, unknownSymbol);
                writer.WriteFloat(lodWidth);
                writer.WriteFloat(lodHeight);
            }

            if (revision > 13)
                writer.WriteBoolean(sortInWorld);

            if (standalone)
            {
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
            }
        }

        public static RndGroup New(ushort revision, ushort altRevision)
        {
            RndGroup rndGroup = new RndGroup();
            rndGroup.revision = revision;
            rndGroup.altRevision = altRevision;
            return rndGroup;
        }
    }
}
