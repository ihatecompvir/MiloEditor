using MiloLib.Classes;
using MiloLib.Utils;
using System.Numerics;

namespace MiloLib.Assets.Rnd
{
    [Name("Light"), Description("Light objects are added to environments for drawing.")]
    public class RndLight : RndTrans
    {
        public enum Type
        {
            kPoint = 0,
            kSpot = 1,
            kDirectional = 2
        }

        public ushort altRevision;
        public ushort revision;
        [Name("Color"), Description("Color of light")]
        public HmxColor4 color = new();
        [Name("Color Owner"), Description("Master for light color and intensity"), MinVersion(11)]
        public Symbol colorOwner = new(0, "");
        [Name("Range"), Description("Falloff distance for point lightsReal"), MinVersion(0)]
        public float range;
        [Name("Falloff Start"), Description("Distance at which falloff starts for point lightsReal"), MinVersion(12)]
        public float falloffStart;
        [Name("Light Type"), Description("Type of dynamic lighting"), MinVersion(1)]
        public Type type;
        [Name("Animate Color From Preset"), Description("Animation authority for LightPreset"), MinVersion(6)]
        public bool animateColorFromPreset;
        [Name("Animate Position From Preset"), Description("Animation authority for LightPreset"), MinVersion(6)]
        public bool animatePositionFromPreset;
        [Name("Animate Range From Preset"), Description("Animate light's range from a LightPreset"), MinVersion(16)]
        public bool animateRangeFromPreset;

        [Name("Texture"), Description("Projected texture"), MinVersion(8)]
        public Symbol texture = new(0, "");
        [Name("Cube Texture"), Description("Projected cube map texture"), MinVersion(14)]
        public Symbol cubeTex = new(0, "");

        private uint shadowObjectsCount;

        [Name("Shadow Objects"), Description("These objects will cast shadows for the projected light"), MinVersion(15)]
        public List<Symbol> shadowObjects = new();

        [MinVersion(13)]
        public Matrix textureTransform = new();
        [Name("Top Radius"), Description("Fake cone small radius at the source"), MinVersion(7)]
        public float topRadius;
        [Name("Bottom Radius"), Description("Fake cone big radius at the far end"), MinVersion(7)]
        public float bottomRadius;
        [Name("Projected Blend"), Description("Specifies blending for the projected light"), MinVersion(15)]
        public int projectedBlend;

        private uint drawCount;
        [MinVersion(9), MaxVersion(9)]
        public List<Symbol> drawList = new();
        [MinVersion(8), MaxVersion(8)]
        public Symbol draw = new(0, "");

        public RndLight Read(EndianReader reader, bool standalone)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));


            if (revision > 3)
                base.objFields.Read(reader);

            base.Read(reader, false, true);

            color = color.Read(reader);
            range = reader.ReadFloat();

            if (revision != 0)
                type = (Type)reader.ReadInt32();

            if (revision > 0xB)
                falloffStart = reader.ReadFloat();

            if (revision > 5)
            {
                animateColorFromPreset = reader.ReadBoolean();
                animatePositionFromPreset = reader.ReadBoolean();
            }

            if (revision > 6)
            {
                topRadius = reader.ReadFloat();
                bottomRadius = reader.ReadFloat();
            }

            if (revision > 7)
            {
                texture = Symbol.Read(reader);
                if (revision == 9)
                {
                    drawCount = reader.ReadUInt32();
                    for (int i = 0; i < drawCount; i++)
                    {
                        drawList.Add(Symbol.Read(reader));
                    }
                }
                else if (revision == 8)
                {
                    draw = Symbol.Read(reader);
                }
            }

            if (revision > 10)
            {
                colorOwner = Symbol.Read(reader);
            }

            if (revision > 0xC)
                textureTransform = textureTransform.Read(reader);

            if (revision > 0xD)
                cubeTex = Symbol.Read(reader);

            if (revision > 0xE)
            {
                shadowObjectsCount = reader.ReadUInt32();
                for (int i = 0; i < shadowObjectsCount; i++)
                {
                    shadowObjects.Add(Symbol.Read(reader));
                }
                projectedBlend = reader.ReadInt32();
            }

            if (revision > 0xF)
                animateRangeFromPreset = reader.ReadBoolean();

            if (standalone)
            {
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw new Exception("Got to end of standalone asset but didn't find the expected end bytes, read likely did not succeed");
            }

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            if (revision > 3)
                base.Write(writer, false);

            color.Write(writer);
            writer.WriteFloat(range);

            if (revision != 0)
                writer.WriteInt32((int)type);

            if (revision > 0xB)
                writer.WriteFloat(falloffStart);

            if (revision > 5)
            {
                writer.WriteBoolean(animateColorFromPreset);
                writer.WriteBoolean(animatePositionFromPreset);
            }

            if (revision > 6)
            {
                writer.WriteFloat(topRadius);
                writer.WriteFloat(bottomRadius);
            }

            if (revision > 7)
            {
                Symbol.Write(writer, texture);
                if (revision == 9)
                {
                    writer.WriteUInt32((uint)drawList.Count);
                    foreach (Symbol draw in drawList)
                    {
                        Symbol.Write(writer, draw);
                    }
                }
                else if (revision == 8)
                {
                    Symbol.Write(writer, draw);
                }
            }

            if (revision > 10)
                Symbol.Write(writer, colorOwner);

            if (revision > 0xC)
                textureTransform.Write(writer);

            if (revision > 0xD)
                Symbol.Write(writer, cubeTex);

            if (revision > 0xE)
            {
                writer.WriteUInt32(shadowObjectsCount);
                foreach (Symbol shadowObject in shadowObjects)
                {
                    Symbol.Write(writer, shadowObject);
                }
                writer.WriteInt32(projectedBlend);
            }

            if (revision > 0xF)
                writer.WriteBoolean(animateRangeFromPreset);

            if (standalone)
            {
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
            }
        }
    }
}