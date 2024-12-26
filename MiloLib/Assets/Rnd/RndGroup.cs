using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets.Rnd
{
    [Name("Group"), Description("Represents a group of objects to which to propogate animation and messages.")]
    public class RndGroup : Object
    {
        public uint revision;

        public RndAnim anim;

        public RndTrans trans;

        public RndDraw draw;

        private uint objectsCount;
        public List<Symbol> objects = new List<Symbol>();

        public Symbol environ;

        [Name("Draw Only"), Description("If set, only draws this member of the group")]
        public Symbol drawOnly;

        [Name("LOD"), Description("Object to draw instead below lod_screen_size")]
        public Symbol lod;

        [Name("LOD Screen Size"), DescriptionAttribute("Ratio of screen height for LOD")]
        public float lodScreenSize;

        [Name("Sort In World"), DescriptionAttribute("Sort by distance to current camera per frame. This has a CPU cost if there are many objects.")]
        public bool sortInWorld;

        public RndGroup Read(EndianReader reader, bool standalone)
        {
            revision = reader.ReadUInt32();

            if (revision != 14)
            {
                throw new UnsupportedAssetRevisionException("Group", revision);
            }

            base.Read(reader, false);

            anim = new RndAnim().Read(reader);
            trans = new RndTrans().Read(reader, false);
            draw = new RndDraw().Read(reader);

            objectsCount = reader.ReadUInt32();
            for (int i = 0; i < objectsCount; i++)
            {
                objects.Add(Symbol.Read(reader));
            }

            environ = Symbol.Read(reader);
            drawOnly = Symbol.Read(reader);
            lod = Symbol.Read(reader);

            lodScreenSize = reader.ReadFloat();

            sortInWorld = reader.ReadBoolean();

            if (standalone)
            {
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw new Exception("Got to end of standalone asset but didn't find the expected end bytes, read likely did not succeed");
            }


            return this;

        }

        public override void Write(EndianWriter writer, bool standalone)
        {
            writer.WriteUInt32(revision);
            base.Write(writer, false);
            anim.Write(writer);
            trans.Write(writer, false);
            draw.Write(writer);

            writer.WriteUInt32(objectsCount);

            foreach (var obj in objects)
            {
                Symbol.Write(writer, obj);
            }

            Symbol.Write(writer, environ);
            Symbol.Write(writer, drawOnly);
            Symbol.Write(writer, lod);

            writer.WriteFloat(lodScreenSize);

            writer.WriteByte((byte)(sortInWorld ? 1 : 0));

            if (standalone)
            {
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
            }
        }
    }
}
