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

        public void Read(EndianReader reader)
        {
            byte flags = reader.ReadByte();
            mRecvProjLights = (flags & 0x01) != 0;
            mPS3ForceTrilinear = (flags & 0x02) != 0;
            mRecvPointCubeTex = (flags & 0x04) != 0;

            // mRecvPointCubeTex is read later based on rev
        }
        public void Write(EndianWriter writer)
        {
            byte flags = 0;
            if (mRecvProjLights) flags |= 0x01;
            if (mPS3ForceTrilinear) flags |= 0x02;
            if (mRecvPointCubeTex) flags |= 0x04;
            writer.WriteByte(flags);
        }
    }
    public struct MatShaderOptions
    {
        public int itop;
        public bool mHasAOCalc;
        public bool mHasBones;
        public int i5;
        public int i4;
        public int i3;
        public int i2;
        public int i1;
        public int i0;

        [Name("Temporary Material"), Description("If set, is a temporary material")]
        public bool mTempMat;
        public uint Value
        {
            get
            {
                uint result = 0;
                result |= (uint)(itop & 0xffffff) << 8;
                result |= (mHasAOCalc ? 1u : 0) << 7;
                result |= (mHasBones ? 1u : 0) << 6;
                result |= (uint)(i5 & 0x01) << 5;
                result |= (uint)(i4 & 0x01) << 4;
                result |= (uint)(i3 & 0x01) << 3;
                result |= (uint)(i2 & 0x01) << 2;
                result |= (uint)(i1 & 0x01) << 1;
                result |= (uint)(i0 & 0x01);
                return result;
            }
            set
            {
                itop = (int)((value >> 8) & 0xffffff);
                mHasAOCalc = (value & (1 << 7)) != 0;
                mHasBones = (value & (1 << 6)) != 0;
                i5 = (int)((value >> 5) & 0x01);
                i4 = (int)((value >> 4) & 0x01);
                i3 = (int)((value >> 3) & 0x01);
                i2 = (int)((value >> 2) & 0x01);
                i1 = (int)((value >> 1) & 0x01);
                i0 = (int)(value & 0x01);
            }
        }

        public void Read(EndianReader reader)
        {
            Value = reader.ReadUInt32();
            mTempMat = reader.ReadBoolean();
        }

        public void Write(EndianWriter writer)
        {
            writer.WriteUInt32(Value);
            writer.WriteByte((byte)(mTempMat ? 1 : 0));
        }

        public void SetLast5(int mask)
        {
            uint value = Value;
            value = (value & ~0x1fu) | (uint)(mask & 0x1f);
            Value = value;
        }
    }

    [Name("Mat"), Description("Material perObjs determine texturing, blending, and the effect of lighting on drawn polys.")]
    public class RndMat : Object
    {
        public enum ColorModFlags : byte
        {
            kColorModNone = 0,
            kColorModAlphaPack = 1,
            kColorModAlphaUnpackModulate = 2,
            kColorModModulate = 3,
            kColorModNum = 3
        }

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

        [Name("Refract Strength"), Description("The scale of the refraction of the screen under the material. Ranges from 0 to 100.")]
        public float refractStrength;

        [Name("Refract Normal Map"), Description("This is a normal map used to distort the screen under the material. If none is specified, the regular normal map will be used.")]
        public Symbol refractNormalMap;

        [Name("Color Modifiers"), Description("Color modifiers for the material")]
        public List<HmxColor4> colorModifiers = new List<HmxColor4>();

        [Name("Performance Settings"), Description("Performance options for this material")]
        public MatPerfSettings performanceSettings;

        [Name("Shader Options"), Description("Options pertaining to the shader capabilities")]
        public MatShaderOptions shaderOptions;

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

        [Name("Color Modification Flags"), Description("Flags pertaining to color modifiers")]
        public ColorModFlags colorModFlags;

        [Name("Dirty Flags"), Description("Dirty flags that denote changes to the material")]
        public byte dirty;

        public HmxColor4 shaderVariationColor = new HmxColor4(1f, 1f, 1f, 1f);

        public HmxColor4 locColor = new HmxColor4(1f, 1f, 1f, 1f);

        public HmxColor4 unkColor = new HmxColor4(1f, 1f, 1f, 1f);

        public bool unkBool1;

        public Symbol unkSym = new Symbol(0, "");

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
            useEnviron = reader.ReadBoolean();
            preLit = reader.ReadBoolean();
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
            locColor = new HmxColor4().Read(reader);

            emissiveMap = Symbol.Read(reader);


            stencilMode = (StencilMode)reader.ReadInt32();


            Symbol sym = Symbol.Read(reader);

            HmxColor4 color2 = new HmxColor4().Read(reader);
            //texPtr = Symbol.Read(reader);

            int i = reader.ReadInt32();
            i = reader.ReadInt32();

            int i2 = reader.ReadInt32();
            i2 = reader.ReadInt32();
            new HmxColor4().Read(reader);
            int b2 = reader.ReadInt32();
            //texPtr = Symbol.Read(reader);
            //texPtr = Symbol.Read(reader);

            if (revision > 0x2a)
            {
                if (revision > 0x2c)
                    pointLights = reader.ReadBoolean();

                fog = reader.ReadBoolean();
                fadeout = reader.ReadBoolean();
                if (revision > 0x2E)
                    colorAdjust = reader.ReadBoolean();
            }


            if (revision > 0x2F)
            {
                unkColor = new HmxColor4().Read(reader);
                unkSym = Symbol.Read(reader);
                byte b = reader.ReadByte();
            }

            if (revision > 0x30)
                screenAligned = reader.ReadBoolean();

            if (revision == 0x32)
                unkBool1 = reader.ReadBoolean();

            if (revision > 0x32)
            {
                shaderVariation = (ShaderVariation)reader.ReadInt32();
                shaderVariationColor = new HmxColor4().Read(reader);
            }


            colorModifiers = new List<HmxColor4>();
            for (int x = 0; x < 3; x++)
            {
                colorModifiers.Add(new HmxColor4(1f, 1f, 1f, 1f));
            }



            Symbol objPtr = Symbol.Read(reader);

            if (revision > 0x3E)
                performanceSettings.Read(reader);

            if (revision > 0x3F)
            {
                refractEnabled = reader.ReadBoolean();
                refractStrength = reader.ReadFloat();
                refractNormalMap = Symbol.Read(reader);
            }

            if (standalone)
            {
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw new Exception("Got to end of standalone asset but didn't find the expected end bytes, read likely did not succeed");
            }
            return this;

        }
        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            base.Write(writer, standalone, parent, entry);
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));
            writer.WriteInt32((int)blend);
            color.Write(writer);
            writer.WriteByte((byte)(useEnviron ? 1 : 0));
            writer.WriteByte((byte)(preLit ? 1 : 0));
            writer.WriteInt32((int)ZMode);
            writer.WriteByte((byte)(alphaCut ? 1 : 0));
            writer.WriteInt32(alphaThreshold);
            writer.WriteByte((byte)(alphaWrite ? 1 : 0));
            writer.WriteInt32((int)texGen);
            writer.WriteInt32((int)texWrap);
            texXfm.Write(writer);
            Symbol.Write(writer, diffuseTex);
            Symbol.Write(writer, nextPass);
            writer.WriteByte((byte)(intensify ? 1 : 0));
            writer.WriteByte((byte)(cull ? 1 : 0));
            writer.WriteFloat(emissiveMultiplier);
            new HmxColor4().Write(writer);
            Symbol.Write(writer, null);
            Symbol.Write(writer, emissiveMap);
            Symbol.Write(writer, null);
            Symbol.Write(writer, null);

            writer.WriteInt32((int)stencilMode);

            Symbol.Write(writer, null);
            Symbol.Write(writer, null);
            new HmxColor4().Write(writer);
            Symbol.Write(writer, null);
            writer.WriteInt32(0);
            writer.WriteInt32(0);
            writer.WriteInt32(0);
            writer.WriteInt32(0);
            new HmxColor4().Write(writer);
            writer.WriteInt32(0);
            Symbol.Write(writer, null);
            Symbol.Write(writer, null);

            writer.WriteByte((byte)(pointLights ? 1 : 0));
            writer.WriteByte((byte)(fog ? 1 : 0));
            writer.WriteByte((byte)(fadeout ? 1 : 0));
            writer.WriteByte((byte)(colorAdjust ? 1 : 0));

            new HmxColor4().Write(writer);
            Symbol.Write(writer, null);
            writer.WriteByte(0);
            writer.WriteByte((byte)(screenAligned ? 1 : 0));

            writer.WriteInt32((int)shaderVariation);
            new HmxColor4().Write(writer);


            if (colorModifiers == null)
            {
                for (int i = 0; i < 3; i++)
                {
                    colorModifiers = new List<HmxColor4>();
                    colorModifiers.Add(new HmxColor4(1f, 1f, 1f, 1f));
                }
            }
            Symbol.Write(writer, null);
            writer.WriteByte((byte)(performanceSettings.mPS3ForceTrilinear ? 1 : 0));
            performanceSettings.Write(writer);
            writer.WriteByte((byte)(performanceSettings.mRecvPointCubeTex ? 1 : 0));
            writer.WriteByte((byte)(refractEnabled ? 1 : 0));
            writer.WriteFloat(refractStrength);
            Symbol.Write(writer, refractNormalMap);

            if (standalone)
            {
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
            }
        }

    }
}