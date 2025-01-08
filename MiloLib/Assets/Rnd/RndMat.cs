using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets.Rnd
{
    public enum Blend : byte
    {
        kBlendDest = 0,
        kBlendSrc = 1,
        kBlendAdd = 2,
        kBlendSrcAlpha = 3,
        kBlendSrcAlphaAdd = 4,
        kBlendSubtract = 5,
        kBlendMultiply = 6,
        kPreMultAlpha = 7,
    }

    public enum ZMode : byte
    {
        kZModeDisable = 0,
        kZModeNormal = 1,
        kZModeTransparent = 2,
        kZModeForce = 3,
        kZModeDecal = 4,
    }

    public enum StencilMode : byte
    {
        kStencilIgnore = 0,
        kStencilWrite = 1,
        kStencilTest = 2,
    }

    public enum TexGen : byte
    {
        kTexGenNone = 0,
        kTexGenXfm = 1,
        kTexGenSphere = 2,
        kTexGenProjected = 3,
        kTexGenXfmOrigin = 4,
        kTexGenEnviron = 5,
    }

    public enum TexWrap : byte
    {
        kTexWrapClamp = 0,
        kTexWrapRepeat = 1,
        kTexBorderBlack = 2,
        kTexBorderWhite = 3,
        kTexWrapMirror = 4
    }
    public enum ShaderVariation : byte
    {
        kShaderVariationNone = 0,
        kShaderVariationSkin = 1,
        kShaderVariationHair = 2
    }
    public struct MatPerfSettings
    {
        [Name("Receive Projected Lights"), Description("Check this option to allow the material to receive projected lighting")]
        public bool mRecvProjLights;

        [Name("Receive Point Cube Textures"), Description("Check this option to allow the material to receive projected cube maps from a point light")]
        public bool mRecvPointCubeTex;

        [Name("Force Trilinear Filtering (PS3)"), Description("Force trilinear filtering of diffuse map (PS3 only)")]
        public bool mPS3ForceTrilinear;

        public void Read(EndianReader reader, uint revision)
        {
            byte flags = reader.ReadByte();
            mRecvProjLights = (flags & 0x01) != 0;
            mPS3ForceTrilinear = (flags & 0x02) != 0;
            mRecvPointCubeTex = (flags & 0x04) != 0;
        }
        public void Write(EndianWriter writer, uint revision)
        {
            byte flags = 0;
            if (mRecvProjLights) flags |= 0x01;
            if (mPS3ForceTrilinear) flags |= 0x02;
            if (revision > 65)
                if (mRecvPointCubeTex) flags |= 0x04;
            writer.WriteByte(flags);
        }
    }

    [Name("Mat"), Description("Material perObjs determine texturing, blending, and the effect of lighting on drawn polys.")]
    public class RndMat : Object
    {
        private ushort altRevision;
        private ushort revision; // constant for revision 68

        [Name("Base Color"), Description("Base material color")]
        public HmxColor4 color = new HmxColor4(1f, 1f, 1f, 1f);

        [Name("Texture Transform"), Description("Transform for coordinate generation")]
        public Matrix texXfm = new Matrix();

        [Name("Diffuse Texture"), Description("Base texture map, modulated with color and alpha")]
        public Symbol diffuseTex;

        [Name("Alpha Threshold"), Description("Alpha level below which gets cut. Ranges from 0 to 255.")]
        public int alphaThreshold;

        [Name("Next Pass"), Description("Next material for object")]
        public Symbol nextPass;

        [Name("Emissive Multiplier"), Description("Multiplier to apply to emission")]
        public float emissiveMultiplier;

        [Name("Emissive Map"), Description("Map for self illumination")]
        public Symbol emissiveMap;

        public Symbol normalMap = new Symbol(0, "");

        public Symbol specularMap = new Symbol(0, "");

        public Symbol environMap = new Symbol(0, "");

        [Name("Refract Strength"), Description("The scale of the refraction of the screen under the material. Ranges from 0 to 100.")]
        public float refractStrength;

        [Name("Refract Normal Map"), Description("This is a normal map used to distort the screen under the material. If none is specified, the regular normal map will be used.")]
        public Symbol refractNormalMap;

        [Name("Color Modifiers"), Description("Color modifiers for the material")]
        public List<HmxColor4> colorModifiers = new List<HmxColor4>();

        [Name("Intensify"), Description("Double the intensity of base map")]
        public bool intensify;

        [Name("Use Environment"), Description("Modulate with environment ambient and lightsReal")]
        public bool useEnviron;

        [Name("Pre-Lit"), Description("Use vertex color and alpha for base or ambient")]
        public bool preLit;

        [Name("Alpha Cut"), Description("Cut zero alpha pixels from z-buffer")]
        public bool alphaCut;

        [Name("Alpha Write"), Description("Write pixel alpha to screen")]
        public bool alphaWrite;

        [Name("Cull"), Description("Cull backface polygons")]
        public bool cull;

        [Name("Per Pixel Lit"), Description("Use per-pixel lighting")]
        public bool perPixelLit;

        [Name("Screen Aligned"), Description("Projected material from camera's POV")]
        public bool screenAligned;

        [Name("Refract Enabled"), Description("When enabled, this material will refract the screen under the material")]
        public bool refractEnabled;

        [Name("Point Lights Enabled"), Description("Is the Mat lit with point lightsReal?")]
        public bool pointLights;

        [Name("Fog Enabled"), Description("Is the Mat affected by fog?")]
        public bool fog;

        [Name("Fadeout Enabled"), Description("Is the Mat affected its Environment's fade_out?")]
        public bool fadeout;

        [Name("Color Adjust Enabled"), Description("Is the Mat affected its Environment's color adjust?")]
        public bool colorAdjust;

        public byte unkbool;

        [Name("Blend Mode"), Description("How to blend poly into screen")]
        public Blend blend;

        [Name("Texture Coordinate Generation"), Description("How to generate texture coordinates")]
        public TexGen texGen;

        [Name("Texture Mapping Mode"), Description("Texture mapping mode")]
        public TexWrap texWrap;

        [Name("Z-Buffer Mode"), Description("How to read and write z-buffer")]
        public ZMode ZMode;

        [Name("Stencil Mode"), Description("How to read and write the stencil buffer")]
        public StencilMode stencilMode;

        [Name("Shader Variation"), Description("Select a variation on the shader to enable a new range of rendering features.")]
        public ShaderVariation shaderVariation;

        [Name("Dirty Flags"), Description("Dirty flags that denote changes to the material")]
        public byte dirty;

        public HmxColor3 specularRGB = new HmxColor3();

        public float specularPower;

        public HmxColor4 unkColor = new HmxColor4(1f, 1f, 1f, 1f);

        public bool unkBool1;

        public Symbol unkSym = new Symbol(0, "");

        public Symbol fur = new Symbol(0, "");

        public float deNormal;
        public float anisotropy;
        public float normalDetailTiling;

        public float normalDetailStrength;
        public Symbol normalDetailMap;

        public HmxColor3 rimRGB = new HmxColor3();
        public float rimPower;
        public Symbol rimMap = new Symbol(0, "");
        public bool rimAlwaysShow;

        public HmxColor3 specular2RGB = new HmxColor3();

        public float specular2Power;

        public float unkFloat;

        public float unkFloat2;
        public Symbol alphaMask = new Symbol(0, "");

        public ushort unkShort;




        public RndMat Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            if (revision != 68)
            {
                throw new UnsupportedAssetRevisionException("RndMat", revision);
            }

            base.Read(reader, false, parent, entry);

            blend = (Blend)reader.ReadInt32();
            color = new HmxColor4().Read(reader);
            preLit = reader.ReadBoolean();
            useEnviron = reader.ReadBoolean();
            ZMode = (ZMode)reader.ReadInt32();
            alphaCut = reader.ReadBoolean();
            if (revision > 0x25)
                alphaThreshold = reader.ReadInt32();
            alphaWrite = reader.ReadBoolean();
            texGen = (TexGen)reader.ReadInt32();
            texWrap = (TexWrap)reader.ReadInt32();
            texXfm = new Matrix().Read(reader);
            diffuseTex = Symbol.Read(reader);
            nextPass = Symbol.Read(reader);
            intensify = reader.ReadBoolean();

            cull = reader.ReadBoolean();
            emissiveMultiplier = reader.ReadFloat();
            specularRGB = new HmxColor3().Read(reader);
            specularPower = reader.ReadFloat();
            normalMap = Symbol.Read(reader);
            emissiveMap = Symbol.Read(reader);
            specularMap = Symbol.Read(reader);
            environMap = Symbol.Read(reader);

            unkShort = reader.ReadUInt16();

            perPixelLit = reader.ReadBoolean();
            stencilMode = (StencilMode)reader.ReadInt32();
            fur = Symbol.Read(reader);

            deNormal = reader.ReadFloat();
            anisotropy = reader.ReadFloat();
            normalDetailTiling = reader.ReadFloat();
            normalDetailStrength = reader.ReadFloat();
            normalDetailMap = Symbol.Read(reader);

            pointLights = reader.ReadBoolean();
            fog = reader.ReadBoolean();
            fadeout = reader.ReadBoolean();
            colorAdjust = reader.ReadBoolean();

            rimRGB = new HmxColor3().Read(reader);
            rimPower = reader.ReadFloat();

            rimMap = Symbol.Read(reader);
            rimAlwaysShow = reader.ReadBoolean();

            screenAligned = reader.ReadBoolean();

            shaderVariation = (ShaderVariation)reader.ReadInt32();

            specular2RGB = new HmxColor3().Read(reader);
            specular2Power = reader.ReadFloat();

            unkFloat = reader.ReadFloat();
            unkFloat2 = reader.ReadFloat();

            alphaMask = Symbol.Read(reader);

            if (standalone)
            {
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw new Exception("Got to end of standalone asset but didn't find the expected end bytes, read likely did not succeed");
            }
            return this;

        }
        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            base.Write(writer, false, parent, entry);

            writer.WriteInt32((int)blend);
            color.Write(writer);
            writer.WriteBoolean(preLit);
            writer.WriteBoolean(useEnviron);
            writer.WriteInt32((int)ZMode);
            writer.WriteBoolean(alphaCut);
            if (revision > 0x25)
                writer.WriteInt32(alphaThreshold);
            writer.WriteBoolean(alphaWrite);
            writer.WriteInt32((int)texGen);
            writer.WriteInt32((int)texWrap);
            texXfm.Write(writer);
            Symbol.Write(writer, diffuseTex);
            Symbol.Write(writer, nextPass);
            writer.WriteBoolean(intensify);

            writer.WriteBoolean(cull);
            writer.WriteFloat(emissiveMultiplier);
            specularRGB.Write(writer);
            writer.WriteFloat(specularPower);
            Symbol.Write(writer, normalMap);
            Symbol.Write(writer, emissiveMap);
            Symbol.Write(writer, specularMap);
            Symbol.Write(writer, environMap);
            writer.WriteUInt16(unkShort);

            writer.WriteBoolean(perPixelLit);
            writer.WriteInt32((int)stencilMode);
            Symbol.Write(writer, fur);

            writer.WriteFloat(deNormal);
            writer.WriteFloat(anisotropy);
            writer.WriteFloat(normalDetailTiling);
            writer.WriteFloat(normalDetailStrength);
            Symbol.Write(writer, normalDetailMap);

            writer.WriteBoolean(pointLights);
            writer.WriteBoolean(fog);
            writer.WriteBoolean(fadeout);
            writer.WriteBoolean(colorAdjust);

            rimRGB.Write(writer);
            writer.WriteFloat(rimPower);

            Symbol.Write(writer, rimMap);
            writer.WriteBoolean(rimAlwaysShow);

            writer.WriteBoolean(screenAligned);

            writer.WriteInt32((int)shaderVariation);

            specular2RGB.Write(writer);
            writer.WriteFloat(specular2Power);

            writer.WriteFloat(unkFloat);
            writer.WriteFloat(unkFloat2);

            Symbol.Write(writer, alphaMask);


            if (standalone)
            {
                writer.WriteUInt32(writer.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD);
            }
        }

    }
}