using MiloLib.Utils;
using MiloLib.Classes;

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

    [Name("Mat"), Description("Material objects determine texturing, blending, and the effect of lighting on drawn polys.")]
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
        public HmxColor4 mColor = new HmxColor4(1f, 1f, 1f, 1f);

        [Name("Texture Transform"), Description("Transform for coordinate generation")]
        public Matrix mTexXfm = new Matrix();

        [Name("Diffuse Texture"), Description("Base texture map, modulated with color and alpha")]
        public Symbol mDiffuseTex;

        [Name("Alpha Threshold"), Description("Alpha level below which gets cut. Ranges from 0 to 255.")]
        public int mAlphaThresh;

        [Name("Next Pass"), Description("Next material for object")]
        public Symbol mNextPass;

        [Name("Emissive Multiplier"), Description("Multiplier to apply to emission")]
        public float mEmissiveMultiplier;

        [Name("Emissive Map"), Description("Map for self illumination")]
        public Symbol mEmissiveMap;

        [Name("Refract Strength"), Description("The scale of the refraction of the screen under the material. Ranges from 0 to 100.")]
        public float mRefractStrength;

        [Name("Refract Normal Map"), Description("This is a normal map used to distort the screen under the material. If none is specified, the regular normal map will be used.")]
        public Symbol mRefractNormalMap;

        [Name("Color Modifiers"), Description("Color modifiers for the material")]
        public List<HmxColor4> mColorMod = new List<HmxColor4>();

        [Name("Performance Settings"), Description("Performance options for this material")]
        public MatPerfSettings mPerfSettings;

        [Name("Shader Options"), Description("Options pertaining to the shader capabilities")]
        public MatShaderOptions mShaderOptions;

        [Name("Intensify"), Description("Double the intensity of base map")]
        public bool mIntensify;

        [Name("Use Environment"), Description("Modulate with environment ambient and lightsReal")]
        public bool mUseEnviron;

        [Name("Pre-Lit"), Description("Use vertex color and alpha for base or ambient")]
        public bool mPreLit;

        [Name("Alpha Cut"), Description("Cut zero alpha pixels from z-buffer")]
        public bool mAlphaCut;

        [Name("Alpha Write"), Description("Write pixel alpha to screen")]
        public bool mAlphaWrite;

        [Name("Cull"), Description("Cull backface polygons")]
        public bool mCull;

        [Name("Per Pixel Lit"), Description("Use per-pixel lighting")]
        public bool mPerPixelLit;

        [Name("Screen Aligned"), Description("Projected material from camera's POV")]
        public bool mScreenAligned;

        [Name("Refract Enabled"), Description("When enabled, this material will refract the screen under the material")]
        public bool mRefractEnabled;

        [Name("Point Lights Enabled"), Description("Is the Mat lit with point lightsReal?")]
        public bool mPointLights;

        [Name("Fog Enabled"), Description("Is the Mat affected by fog?")]
        public bool mFog;

        [Name("Fadeout Enabled"), Description("Is the Mat affected its Environment's fade_out?")]
        public bool mFadeout;

        [Name("Color Adjust Enabled"), Description("Is the Mat affected its Environment's color adjust?")]
        public bool mColorAdjust;

        public byte unkbool;

        [Name("Blend Mode"), Description("How to blend poly into screen")]
        public Blend mBlend;

        [Name("Texture Coordinate Generation"), Description("How to generate texture coordinates")]
        public TexGen mTexGen;

        [Name("Texture Mapping Mode"), Description("Texture mapping mode")]
        public TexWrap mTexWrap;

        [Name("Z-Buffer Mode"), Description("How to read and write z-buffer")]
        public ZMode mZMode;

        [Name("Stencil Mode"), Description("How to read and write the stencil buffer")]
        public StencilMode mStencilMode;

        [Name("Shader Variation"), Description("Select a variation on the shader to enable a new range of rendering features.")]
        public ShaderVariation mShaderVariation;

        [Name("Color Modification Flags"), Description("Flags pertaining to color modifiers")]
        public ColorModFlags mColorModFlags;

        [Name("Dirty Flags"), Description("Dirty flags that denote changes to the material")]
        public byte mDirty;

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

            mBlend = (Blend)reader.ReadInt32();
            mColor = new HmxColor4().Read(reader);
            mUseEnviron = reader.ReadBoolean();
            mPreLit = reader.ReadBoolean();
            mZMode = (ZMode)reader.ReadInt32();
            mAlphaCut = reader.ReadBoolean();
            mAlphaThresh = reader.ReadInt32();
            mAlphaWrite = reader.ReadBoolean();
            mTexGen = (TexGen)reader.ReadInt32();
            mTexWrap = (TexWrap)reader.ReadInt32();
            mTexXfm = new Matrix().Read(reader);
            mDiffuseTex = Symbol.Read(reader);
            mNextPass = Symbol.Read(reader);

            mIntensify = reader.ReadBoolean();
            mDirty = 3;
            HmxColor4 loc_color;
            mCull = reader.ReadBoolean();
            mEmissiveMultiplier = reader.ReadFloat();
            loc_color = new HmxColor4().Read(reader);

            Symbol texPtr = Symbol.Read(reader);

            mEmissiveMap = Symbol.Read(reader);
            texPtr = Symbol.Read(reader);
            texPtr = Symbol.Read(reader);


            mStencilMode = (StencilMode)reader.ReadInt32();


            Symbol sym = Symbol.Read(reader);
            texPtr = Symbol.Read(reader);

            HmxColor4 color2 = new HmxColor4().Read(reader);
            texPtr = Symbol.Read(reader);

            int i = reader.ReadInt32();
            i = reader.ReadInt32();

            int i2 = reader.ReadInt32();
            i2 = reader.ReadInt32();
            new HmxColor4().Read(reader);
            int b2 = reader.ReadInt32();
            texPtr = Symbol.Read(reader);
            texPtr = Symbol.Read(reader);

            mPointLights = reader.ReadBoolean();
            mFog = reader.ReadBoolean();
            mFadeout = reader.ReadBoolean();
            mColorAdjust = reader.ReadBoolean();


            HmxColor4 color2f = new HmxColor4().Read(reader);
            texPtr = Symbol.Read(reader);
            byte b = reader.ReadByte();

            mScreenAligned = reader.ReadBoolean();

            mShaderVariation = (ShaderVariation)reader.ReadInt32();
            HmxColor4 col32 = new HmxColor4().Read(reader);


            mColorMod = new List<HmxColor4>();
            for (int x = 0; x < 3; x++)
            {
                mColorMod.Add(new HmxColor4(1f, 1f, 1f, 1f));
            }



            Symbol objPtr = Symbol.Read(reader);

            byte b3 = reader.ReadByte();
            mPerfSettings.mPS3ForceTrilinear = b3 != 0;

            mPerfSettings.Read(reader);
            mPerfSettings.mRecvPointCubeTex = reader.ReadBoolean();

            mRefractEnabled = reader.ReadBoolean();
            mRefractStrength = reader.ReadFloat();
            mRefractNormalMap = Symbol.Read(reader);

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
            writer.WriteInt32((int)mBlend);
            mColor.Write(writer);
            writer.WriteByte((byte)(mUseEnviron ? 1 : 0));
            writer.WriteByte((byte)(mPreLit ? 1 : 0));
            writer.WriteInt32((int)mZMode);
            writer.WriteByte((byte)(mAlphaCut ? 1 : 0));
            writer.WriteInt32(mAlphaThresh);
            writer.WriteByte((byte)(mAlphaWrite ? 1 : 0));
            writer.WriteInt32((int)mTexGen);
            writer.WriteInt32((int)mTexWrap);
            mTexXfm.Write(writer);
            Symbol.Write(writer, mDiffuseTex);
            Symbol.Write(writer, mNextPass);
            writer.WriteByte((byte)(mIntensify ? 1 : 0));
            writer.WriteByte((byte)(mCull ? 1 : 0));
            writer.WriteFloat(mEmissiveMultiplier);
            new HmxColor4().Write(writer);
            Symbol.Write(writer, null);
            Symbol.Write(writer, mEmissiveMap);
            Symbol.Write(writer, null);
            Symbol.Write(writer, null);

            writer.WriteInt32((int)mStencilMode);

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

            writer.WriteByte((byte)(mPointLights ? 1 : 0));
            writer.WriteByte((byte)(mFog ? 1 : 0));
            writer.WriteByte((byte)(mFadeout ? 1 : 0));
            writer.WriteByte((byte)(mColorAdjust ? 1 : 0));

            new HmxColor4().Write(writer);
            Symbol.Write(writer, null);
            writer.WriteByte(0);
            writer.WriteByte((byte)(mScreenAligned ? 1 : 0));

            writer.WriteInt32((int)mShaderVariation);
            new HmxColor4().Write(writer);


            if (mColorMod == null)
            {
                for (int i = 0; i < 3; i++)
                {
                    mColorMod = new List<HmxColor4>();
                    mColorMod.Add(new HmxColor4(1f, 1f, 1f, 1f));
                }
            }
            Symbol.Write(writer, null);
            writer.WriteByte((byte)(mPerfSettings.mPS3ForceTrilinear ? 1 : 0));
            mPerfSettings.Write(writer);
            writer.WriteByte((byte)(mPerfSettings.mRecvPointCubeTex ? 1 : 0));
            writer.WriteByte((byte)(mRefractEnabled ? 1 : 0));
            writer.WriteFloat(mRefractStrength);
            Symbol.Write(writer, mRefractNormalMap);

            if (standalone)
            {
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
            }
        }

    }
}