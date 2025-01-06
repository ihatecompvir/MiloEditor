using MiloLib.Assets.Rnd;
using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets
{
    [Name("SpotlightDrawer"), Description("")]
    public class SpotlightDrawer : Object
    {
        private ushort altRevision;
        private ushort revision;

        public RndDrawable draw = new();

        [Name("Intensity"), Description("global intensity scale")]
        public float intensity;
        [Name("Color"), Description("color of ambient (unlit) fog")]
        public HmxColor4 color = new();
        [Name("Base Intensity"), Description("intensity of smokeless beam")]
        public float baseIntensity;
        [Name("Smoke Intensity"), Description("intensity from smoke")]
        public float smokeIntensity;
        public float halfDistance;
        [Name("Lighting Influence"), Description("The amount the spotlights will influence the real lighting of the world")]
        public float lightingInfluence;
        public Symbol texture = new(0, "");
        [Name("Proxy Fog Object"), Description("proxy fog object")]
        public Symbol proxy = new(0, "");

        [MaxVersion(3)]
        public float unkFloat1;
        [MaxVersion(3)]
        public float unkFloat2;
        [MaxVersion(3)]
        public float unkFloat3;
        [MaxVersion(3)]
        public float unkFloat4;

        [MaxVersion(3)]
        public uint unkInt1;
        [MaxVersion(3)]
        public uint unkInt2;
        [MaxVersion(3)]
        public uint unkInt3;
        [MaxVersion(3)]
        public uint unkInt4;
        [MaxVersion(3)]
        public uint unkInt5;

        [MaxVersion(2)]
        public bool unkBool;

        public SpotlightDrawer Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            if (revision > 0)
            {
                draw.Read(reader, false, parent, entry);
            }
            else
            {
                base.objFields.Read(reader, parent, entry);
            }

            intensity = reader.ReadFloat();

            if (revision > 3)
            {
                smokeIntensity = reader.ReadFloat();
                halfDistance = reader.ReadFloat();
                lightingInfluence = reader.ReadFloat();
            }
            else
            {
                unkFloat1 = reader.ReadFloat();
                unkFloat2 = reader.ReadFloat();
                unkFloat3 = reader.ReadFloat();
                unkFloat4 = reader.ReadFloat();
            }

            color.Read(reader);

            if (revision < 4)
            {
                unkInt1 = reader.ReadUInt32();
                unkInt2 = reader.ReadUInt32();
                unkInt3 = reader.ReadUInt32();
                unkInt4 = reader.ReadUInt32();
                unkInt5 = reader.ReadUInt32();
            }

            texture = Symbol.Read(reader);
            proxy = Symbol.Read(reader);

            if (revision < 3)
            {
                unkBool = reader.ReadBoolean();
            }

            if (revision > 4)
            {
                lightingInfluence = reader.ReadFloat();
            }

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw new Exception("Got to end of standalone asset but didn't find the expected end bytes, read likely did not succeed");

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            if (revision > 0)
            {
                draw.Write(writer, false, true);
            }
            else
            {
                base.objFields.Write(writer);
            }

            writer.WriteFloat(intensity);

            if (revision > 3)
            {
                writer.WriteFloat(smokeIntensity);
                writer.WriteFloat(halfDistance);
                writer.WriteFloat(lightingInfluence);
            }
            else
            {
                writer.WriteFloat(unkFloat1);
                writer.WriteFloat(unkFloat2);
                writer.WriteFloat(unkFloat3);
                writer.WriteFloat(unkFloat4);
            }

            color.Write(writer);

            if (revision < 4)
            {
                writer.WriteUInt32(unkInt1);
                writer.WriteUInt32(unkInt2);
                writer.WriteUInt32(unkInt3);
                writer.WriteUInt32(unkInt4);
                writer.WriteUInt32(unkInt5);
            }

            Symbol.Write(writer, texture);
            Symbol.Write(writer, proxy);

            if (revision < 3)
            {
                writer.WriteBoolean(unkBool);
            }

            if (revision > 4)
            {
                writer.WriteFloat(lightingInfluence);
            }

            if (standalone)
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
        }

    }
}
