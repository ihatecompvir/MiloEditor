using MiloLib.Classes;
using MiloLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiloLib.Assets.Rnd
{
    public class RndColorXfm
    {
        private ushort altRevision;
        private ushort revision;

        public Matrix colorTransform = new();

        [Name("Lightness"), Description("Lightness: -100 to 100, 0.0 is neutral")]
        public float lightness;
        [Name("Saturation"), Description("Saturation: -100 to 100, 0.0 is neutral")]
        public float saturation;
        [Name("Hue"), Description("Hue: -180 to 180, 0.0 is neutral")]
        public float hue;
        [Name("Brightness"), Description("Brightness: -100 to 100, 0.0 is neutral")]
        public float brightness;
        [Name("Contrast"), Description("Contrast: -100 to 100, 0.0 is neutral")]
        public float contrast;

        public HmxColor4 levelInLo = new HmxColor4();
        public HmxColor4 levelInHi = new HmxColor4();
        public HmxColor4 levelOutLo = new HmxColor4();
        public HmxColor4 levelOutHi = new HmxColor4();

        public RndColorXfm Read(EndianReader reader)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            if (revision > 0)
            {
                return this;
            }
            colorTransform = colorTransform.Read(reader);

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

            colorTransform.Write(writer);

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
