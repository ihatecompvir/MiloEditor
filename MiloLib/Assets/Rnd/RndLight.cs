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

        public uint revision;
        [Name("Color"), Description("Color of light")]
        public HmxColor color = new();
        [Name("Color Owner"), Description("Master for light color and intensity")]
        public Symbol colorOwner = new(0, "");
        [Name("Range"), Description("Falloff distance for point lights")]
        public float range;
        [Name("Falloff Start"), Description("Distance at which falloff starts for point lights")]
        public float falloffStart;
        [Name("Light Type"), Description("Type of dynamic lighting")]
        public Type type;
        [Name("Animate Color From Preset"), Description("Animation authority for LightPreset")]
        public bool animateColorFromPreset;
        [Name("Animate Position From Preset"), Description("Animation authority for LightPreset")]
        public bool animatePositionFromPreset;
        [Name("Animate Range From Preset"), Description("Animate light's range from a LightPreset")]
        public bool animateRangeFromPreset;
        public bool showing;
        [Name("Texture"), Description("Projected texture")]
        public Symbol texture = new(0, "");
        [Name("Cube Texture"), Description("Projected cube map texture")]
        public Symbol cubeTex = new(0, "");

        private uint shadowObjectsCount;

        [Name("Shadow Objects"), Description("These objects will cast shadows for the projected light")]
        public List<Symbol> shadowObjects = new();

        public Matrix textureTransform = new();
        [Name("Top Radius"), Description("Fake cone small radius at the source")]
        public float topRadius;
        [Name("Bottom Radius"), Description("Fake cone big radius at the far end")]
        public float bottomRadius;
        [Name("Projected Blend"), Description("Specifies blending for the projected light")]
        public int projectedBlend;
        [Name("Only Projection"), Description("Only render the projected light")]
        public bool onlyProjection;

        public RndLight Read(EndianReader reader, bool standalone)
        {
            revision = reader.ReadUInt32();

            base.objFields.Read(reader);

            base.Read(reader, false);

            color = color.Read(reader);
            range = reader.ReadFloat();
            type = (Type)reader.ReadInt32();
            falloffStart = reader.ReadFloat();
            animateColorFromPreset = reader.ReadBoolean();
            animatePositionFromPreset = reader.ReadBoolean();
            topRadius = reader.ReadFloat();
            bottomRadius = reader.ReadFloat();
            texture = Symbol.Read(reader);
            colorOwner = Symbol.Read(reader);
            textureTransform = textureTransform.Read(reader);
            cubeTex = Symbol.Read(reader);
            shadowObjectsCount = reader.ReadUInt32();
            for (int i = 0; i < shadowObjectsCount; i++)
            {
                shadowObjects.Add(Symbol.Read(reader));
            }
            projectedBlend = reader.ReadInt32();
            animateRangeFromPreset = reader.ReadBoolean();

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

            color.Write(writer);
            writer.WriteFloat(range);
            writer.WriteInt32((int)type);
            writer.WriteFloat(falloffStart);
            writer.WriteByte((byte)(animateColorFromPreset ? 1 : 0));
            writer.WriteByte((byte)(animatePositionFromPreset ? 1 : 0));
            writer.WriteFloat(topRadius);
            writer.WriteFloat(bottomRadius);
            Symbol.Write(writer, texture);
            Symbol.Write(writer, colorOwner);
            textureTransform.Write(writer);
            writer.WriteUInt32(shadowObjectsCount);
            foreach (Symbol shadowObject in shadowObjects)
            {
                Symbol.Write(writer, shadowObject);
            }
            writer.WriteInt32(projectedBlend);
            writer.WriteByte((byte)(animateRangeFromPreset ? 1 : 0));

            if (standalone)
            {
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
            }
        }
    }
}