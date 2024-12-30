using MiloLib.Classes;
using MiloLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiloLib.Assets.Rnd
{
    public class ColorXfm
    {
        public ushort altRevision;
        public ushort revision;

        public HmxColor3 color1 = new HmxColor3();
        public HmxColor3 color2 = new HmxColor3();
        public HmxColor3 color3 = new HmxColor3();
        public HmxColor3 color4 = new HmxColor3();

        public float lightness;
        public float saturation;
        public float hue;
        public float brightness;
        public float contrast;

        public HmxColor4 levelInLo = new HmxColor4();
        public HmxColor4 levelInHi = new HmxColor4();
        public HmxColor4 levelOutLo = new HmxColor4();
        public HmxColor4 levelOutHi = new HmxColor4();

        public ColorXfm Read(EndianReader reader)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            color1 = color1.Read(reader);
            color2 = color2.Read(reader);
            color3 = color3.Read(reader);
            color4 = color4.Read(reader);

            lightness = reader.ReadFloat();
            saturation = reader.ReadFloat();
            hue = reader.ReadFloat();
            brightness = reader.ReadFloat();
            contrast = reader.ReadFloat();

            levelInLo = levelInLo.Read(reader);
            levelInHi = levelInHi.Read(reader);
            levelOutLo = levelOutLo.Read(reader);
            levelOutHi = levelOutHi.Read(reader);
            return this;
        }

        public void Write(EndianWriter writer)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            color1.Write(writer);
            color2.Write(writer);
            color3.Write(writer);
            color4.Write(writer);

            writer.WriteFloat(lightness);
            writer.WriteFloat(saturation);
            writer.WriteFloat(hue);
            writer.WriteFloat(brightness);
            writer.WriteFloat(contrast);

            levelInLo.Write(writer);
            levelInHi.Write(writer);
            levelOutLo.Write(writer);
            levelOutHi.Write(writer);
        }
    }
}
