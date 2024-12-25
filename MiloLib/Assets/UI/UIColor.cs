using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets.UI
{
    [Name("UIColor"), Description("Just a color, used by UI components")]
    public class UIColor : Object
    {
        public uint revision;

        public float r;
        public float g;
        public float b;
        public float a;

        public UIColor Read(EndianReader reader, bool standalone)
        {
            revision = reader.ReadUInt32();

            if (revision != 0)
            {
                throw new UnsupportedAssetRevisionException("UIColor", revision);
            }

            base.objFields.Read(reader);

            r = reader.ReadFloat();
            g = reader.ReadFloat();
            b = reader.ReadFloat();
            a = reader.ReadFloat();

            if (standalone)
            {
                reader.BaseStream.Position += 4;
            }

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone)
        {
            writer.WriteUInt32(revision);

            base.objFields.Write(writer);

            writer.WriteFloat(r);
            writer.WriteFloat(g);
            writer.WriteFloat(b);
            writer.WriteFloat(a);

            if (standalone)
            {
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
            }
        }
    }
}
