using MiloLib.Classes;
using MiloLib.Utils;
using System;
using System.Drawing;

namespace MiloLib.Assets.Rnd
{

    [Name("Mat"), Description("Material perObjs determine texturing, blending, and the effect of lighting on drawn polys.")]
    public class RndMat : Object
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
            public bool recvProjLights;

            [Name("Receive Point Cube Textures"), Description("Check this option to allow the material to receive projected cube maps from a point light")]
            public bool recvPointCubeTex;

            [Name("Force Trilinear Filtering (PS3)"), Description("Force trilinear filtering of diffuse map (PS3 only)")]
            public bool ps3ForceTrilinear;

            public void Read(EndianReader reader, uint revision)
            {
                recvProjLights = reader.ReadBoolean();
                ps3ForceTrilinear = reader.ReadBoolean();
                if (revision > 0x41)
                    recvPointCubeTex = reader.ReadBoolean();
            }
            public void Write(EndianWriter writer, uint revision)
            {
                writer.WriteBoolean(recvProjLights);
                writer.WriteBoolean(ps3ForceTrilinear);
                if (revision > 0x41)
                    writer.WriteBoolean(recvPointCubeTex);
            }
        }


        public class TextureEntry
        {
            public int unk;
            public int unk2;
            public Matrix texXfm = new Matrix();
            public int texWrap;
            public Symbol name = new Symbol(0, "");

            public TextureEntry Read(EndianReader reader)
            {
                unk = reader.ReadInt32();
                unk2 = reader.ReadInt32();
                texXfm = new Matrix().Read(reader);
                texWrap = reader.ReadInt32();
                name = Symbol.Read(reader);
                return this;
            }

            public void Write(EndianWriter writer)
            {
                writer.WriteInt32(unk);
                writer.WriteInt32(unk2);
                texXfm.Write(writer);
                writer.WriteInt32(texWrap);
                Symbol.Write(writer, name);
            }
        }
        private ushort altRevision;
        private ushort revision;

        [Name("Blend Mode"), Description("How to blend poly into screen")]
        public Blend blend;

        [Name("Base Color"), Description("Base material color")]
        public HmxColor4 color = new HmxColor4(1f, 1f, 1f, 1f);

        [Name("Pre-Lit"), Description("Use vertex color and alpha for base or ambient")]
        public bool preLit;

        [Name("Use Environment"), Description("Modulate with environment ambient and lightsReal")]
        public bool useEnviron;

        [Name("Z-Buffer Mode"), Description("How to read and write z-buffer")]
        public ZMode zMode;

        [Name("Alpha Cut"), Description("Cut zero alpha pixels from z-buffer")]
        public bool alphaCut;

        [Name("Alpha Threshold"), Description("Alpha level below which gets cut. Ranges from 0 to 255.")]
        public int alphaThreshold;

        [Name("Alpha Write"), Description("Write pixel alpha to screen")]
        public bool alphaWrite;

        [Name("Texture Coordinate Generation"), Description("How to generate texture coordinates")]
        public TexGen texGen;

        [Name("Texture Mapping Mode"), Description("Texture mapping mode")]
        public TexWrap texWrap;

        [Name("Texture Transform"), Description("Transform for coordinate generation")]
        public Matrix texXfm = new Matrix();

        [Name("Diffuse Texture"), Description("Base texture map, modulated with color and alpha")]
        public Symbol diffuseTex = new(0, "");

        [Name("Next Pass"), Description("Next material for object")]
        public Symbol nextPass = new(0, "");

        [Name("Intensify"), Description("Double the intensity of base map")]
        public bool intensify;

        [Name("Cull"), Description("Cull backface polygons")]
        public bool cull;

        [Name("Emissive Multiplier"), Description("Multiplier to apply to emission")]
        public float emissiveMultiplier;

        [Name("Specular RGB"), Description("Color to use when not driven by texture")]
        public HmxColor3 specularRGB = new HmxColor3();

        [Name("Specular Power"), Description("Power to use when not driven by texture")]
        public float specularPower;

        [Name("Normal Map"), Description("Texture map to define lighting normals")]
        public Symbol normalMap = new Symbol(0, "");

        [Name("Emissive Map"), Description("Map for self illumination")]
        public Symbol emissiveMap = new(0, "");

        [Name("Specular Map"), Description("Texture map for specular color and power")]
        public Symbol specularMap = new Symbol(0, "");

        public Symbol unkSymbol2 = new Symbol(0, "");

        [Name("Environment Map"), Description("Cube texture for reflections. Does not apply to particles")]
        public Symbol environMap = new Symbol(0, "");

        public ushort unkShort;

        [Name("Per Pixel Lit"), Description("Use per-pixel lighting")]
        public bool perPixelLit;

        public bool unkBool1;

        [Name("Stencil Mode"), Description("How to read and write the stencil buffer")]
        public StencilMode stencilMode;

        [Name("Fur"), Description("Use fur shader")]
        public Symbol fur = new Symbol(0, "");

        [Name("De-Normal"), Description("Amount to diminish normal map bumpiness, 0 is neutral, 1 is no bumps, -1 exaggerates")]
        public float deNormal;

        [Name("Anisotropy"), Description("Specular power in downward (strand) direction, 0 to disable")]
        public float anisotropy;

        [Name("Normal Detail Tiling"), Description("Texture tiling scale for the detail map")]
        public float normalDetailTiling;

        [Name("Normal Detail Strength"), Description("Strength of the detail map bumpiness")]
        public float normalDetailStrength;

        [Name("Normal Detail Map"), Description("Detail map texture")]
        public Symbol normalDetailMap = new Symbol(0, "");

        [Name("Point Lights Enabled"), Description("Is the Mat lit with point lightsReal?")]
        public bool pointLights;

        [Name("Projected Lights"), Description("Is the Mat lit with projected lights?")]
        public bool projLights;

        [Name("Fog Enabled"), Description("Is the Mat affected by fog?")]
        public bool fog;

        [Name("Fadeout Enabled"), Description("Is the Mat affected its Environment's fade_out?")]
        public bool fadeout;

        [Name("Color Adjust Enabled"), Description("Is the Mat affected its Environment's color adjust?")]
        public bool colorAdjust;

        [Name("Rim RGB"), Description("Rim lighting color. If a rim texture is present, this color is multiplied by the rim texture RGB color.")]
        public HmxColor3 rimRGB = new HmxColor3();

        [Name("Rim Power"), Description("Rim lighting power. This is the sharpness of the wrap-around effect; higher numbers result in a sharper rim lighting effect. If a rim texture is present, this value is multiplied by the rim texture alpha channel.")]
        public float rimPower;

        [Name("Rim Map"), Description("Texture map that defines the rim lighting color (in the RGB channels) and power (in the Alpha channel).")]
        public Symbol rimMap = new Symbol(0, "");

        [Name("Rim Always Show"), Description("When enabled, this causes the rim effect to highlight the undersides of meshes")]
        public bool rimAlwaysShow;

        [Name("Screen Aligned"), Description("Projected material from camera's POV")]
        public bool screenAligned;

        [Name("Shader Variation"), Description("Select a variation on the shader to enable a new range of rendering features.")]
        public ShaderVariation shaderVariation;

        public HmxColor3 specular2RGB = new HmxColor3();

        public float specular2Power;

        public float unkFloat;

        public float unkFloat2;

        public Symbol alphaMask = new Symbol(0, "");

        public MatPerfSettings perfSettings = new MatPerfSettings();

        [Name("Refract Enabled"), Description("When enabled, this material will refract the screen under the material")]
        public bool refractEnabled;

        [Name("Refract Strength"), Description("The scale of the refraction of the screen under the material. Ranges from 0 to 100.")]
        public float refractStrength;

        [Name("Refract Normal Map"), Description("This is a normal map used to distort the screen under the material. If none is specified, the regular normal map will be used.")]
        public Symbol refractNormalMap = new Symbol(0, "");

        public byte unkbool;

        [Name("Dirty Flags"), Description("Dirty flags that denote changes to the material")]
        public byte dirty;

        public HmxColor4 unkColor = new HmxColor4(1f, 1f, 1f, 1f);

        public bool unkBool;

        public Symbol unkSym = new Symbol(0, "");

        public Symbol unkSym1 = new Symbol(0, "");

        public Symbol unkSym2 = new Symbol(0, "");

        public HmxColor4 unkColor2 = new HmxColor4();

        private uint colorsCount;
        public List<HmxColor4> colors = new();

        private uint textureCount;
        public List<TextureEntry> textures = new();

        public int unkInt1;
        public int unkInt2;
        public int unkInt3;

        public bool unkBool2;
        public HmxColor3 unkColor3 = new();
        public float unkFloat3;

        public Symbol unkSym3 = new Symbol(0, "");





        public RndMat Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            if (revision <= 9)
            {
            }
            else if (revision <= 21)
            {
                textureCount = reader.ReadUInt32();
                for (int i = 0; i < textureCount; i++)
                {
                    textures.Add(new TextureEntry().Read(reader));
                }
            }

            base.Read(reader, false, parent, entry);

            blend = (Blend)reader.ReadInt32();
            color = new HmxColor4().Read(reader);

            if (revision <= 21)
            {
                reader.ReadByte();
                reader.ReadUInt16();
                reader.ReadInt32();
                reader.ReadUInt16();
                reader.ReadUInt32();
                reader.ReadUInt16();

                if (standalone)
                {
                    if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw new Exception("Got to end of standalone asset but didn't find the expected end bytes, read likely did not succeed");
                }
                return this;
            }

            preLit = reader.ReadBoolean();
            useEnviron = reader.ReadBoolean();
            zMode = (ZMode)reader.ReadInt32();
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
            if (revision < 51)
            {
                unkSymbol2 = Symbol.Read(reader);
            }

            environMap = Symbol.Read(reader);

            if (revision > 25)
            {
                if (revision == 68)
                {
                    unkShort = reader.ReadUInt16();
                }
            }

            perPixelLit = reader.ReadBoolean();
            if (revision >= 27 && revision < 50)
            {
                unkBool1 = reader.ReadBoolean();
            }

            if (revision > 27)
                stencilMode = (StencilMode)reader.ReadInt32();

            if (revision < 33)
            {
            }
            else
            {
                fur = Symbol.Read(reader);
            }

            if (revision >= 34 && revision < 49)
            {
                unkBool2 = reader.ReadBoolean();
                unkColor3 = new HmxColor3().Read(reader);
                unkFloat3 = reader.ReadFloat();

                if (revision > 34)
                {
                    unkSym3 = Symbol.Read(reader);
                }
            }

            if (revision <= 28)
            {
                if (standalone)
                {
                    if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw new Exception("Got to end of standalone asset but didn't find the expected end bytes, read likely did not succeed");
                }
                return this;
            }




            if (revision > 35)
            {
                deNormal = reader.ReadFloat();
                anisotropy = reader.ReadFloat();
            }

            if (revision > 38)
            {
                normalDetailTiling = reader.ReadFloat();
                normalDetailStrength = reader.ReadFloat();
                normalDetailMap = Symbol.Read(reader);
            }

            pointLights = reader.ReadBoolean();
            if (revision < 0x3F)
                projLights = reader.ReadBoolean();
            fog = reader.ReadBoolean();
            fadeout = reader.ReadBoolean();
            colorAdjust = reader.ReadBoolean();

            if (revision > 47)
            {
                rimRGB = new HmxColor3().Read(reader);
                rimPower = reader.ReadFloat();

                rimMap = Symbol.Read(reader);
                rimAlwaysShow = reader.ReadBoolean();
            }

            if (revision > 48)
                screenAligned = reader.ReadBoolean();

            if (revision > 0x32)
            {
                shaderVariation = (ShaderVariation)reader.ReadInt32();
                specular2RGB = new HmxColor3().Read(reader);
                specular2Power = reader.ReadFloat();
            }

            if (revision >= 52 && revision <= 67)
            {
                if (revision < 0x35)
                    unkBool = reader.ReadBoolean();
                else
                    unkInt3 = reader.ReadInt32();

                if (revision >= 53 && revision <= 59)
                {
                    unkColor2 = new HmxColor4().Read(reader);
                }

                if (revision >= 0x3C)
                {
                    colorsCount = reader.ReadUInt32();
                    for (int i = 0; i < colorsCount; i++)
                    {
                        colors.Add(new HmxColor4().Read(reader));

                    }
                }

            }

            if (revision >= 54 && revision <= 61)
            {
                unkSym2 = Symbol.Read(reader);
            }

            if (revision >= 55 && revision <= 62)
                perfSettings.ps3ForceTrilinear = reader.ReadBoolean();

            if (revision == 0x38)
            {
                unkInt1 = reader.ReadInt32();
                unkInt2 = reader.ReadInt32();
            }

            if (revision > 0x3E)
            {
                perfSettings.Read(reader, revision);
            }

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
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            base.Write(writer, false, parent, entry);

            writer.WriteInt32((int)blend);
            color.Write(writer);
            writer.WriteBoolean(preLit);
            writer.WriteBoolean(useEnviron);
            writer.WriteInt32((int)zMode);
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
            if (revision < 51)
            {
                Symbol.Write(writer, unkSymbol2);
            }
            Symbol.Write(writer, environMap);

            if (revision > 25)
            {
                if (revision == 68)
                    writer.WriteUInt16(unkShort);
            }

            writer.WriteBoolean(perPixelLit);

            if (revision >= 27 && revision < 50)
            {
                writer.WriteBoolean(unkBool1);
            }

            writer.WriteInt32((int)stencilMode);
            if (revision < 33)
            {
            }
            else
            {
                Symbol.Write(writer, fur);
            }

            if (revision >= 34 && revision < 49)
            {
                writer.WriteBoolean(unkBool2);
                unkColor3.Write(writer);
                writer.WriteFloat(unkFloat3);

                if (revision > 34)
                {
                    Symbol.Write(writer, unkSym3);
                }
            }

            if (revision <= 28)
            {
                if (standalone)
                {
                    writer.WriteUInt32(writer.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD);
                }
                return;
            }

            writer.WriteFloat(deNormal);
            writer.WriteFloat(anisotropy);
            writer.WriteFloat(normalDetailTiling);
            writer.WriteFloat(normalDetailStrength);
            Symbol.Write(writer, normalDetailMap);

            writer.WriteBoolean(pointLights);
            if (revision < 0x3F)
                writer.WriteBoolean(projLights);
            writer.WriteBoolean(fog);
            writer.WriteBoolean(fadeout);
            writer.WriteBoolean(colorAdjust);

            if (revision > 47)
            {
                rimRGB.Write(writer);
                writer.WriteFloat(rimPower);

                Symbol.Write(writer, rimMap);
                writer.WriteBoolean(rimAlwaysShow);
            }

            if (revision > 48)
                writer.WriteBoolean(screenAligned);

            if (revision > 0x32)
            {
                writer.WriteInt32((int)shaderVariation);
                specular2RGB.Write(writer);
                writer.WriteFloat(specular2Power);
            }

            if (revision >= 52 && revision <= 67)
            {
                if (revision < 0x35)
                    writer.WriteBoolean(unkBool);
                else
                    writer.WriteInt32(unkInt3);

                if (revision >= 53 && revision <= 59)
                {
                    unkColor2.Write(writer);
                }

                if (revision >= 0x3C)
                {
                    writer.WriteUInt32((uint)colors.Count);
                    foreach (var color in colors)
                    {
                        color.Write(writer);
                    }
                }

            }

            if (revision >= 54 && revision <= 61)
            {
                Symbol.Write(writer, unkSym2);
            }

            if (revision >= 55 && revision <= 62)
                writer.WriteBoolean(perfSettings.ps3ForceTrilinear);

            if (revision == 0x38)
            {
                writer.WriteInt32(unkInt1);
                writer.WriteInt32(unkInt2);
            }

            if (revision > 0x3E)
            {
                perfSettings.Write(writer, revision);
            }

            if (revision > 0x3F)
            {
                writer.WriteBoolean(refractEnabled);
                writer.WriteFloat(refractStrength);
                Symbol.Write(writer, refractNormalMap);
            }



            if (standalone)
            {
                writer.WriteUInt32(writer.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD);
            }
        }

        public static RndMat New(ushort revision, ushort altRevision)
        {
            RndMat rndMat = new RndMat();
            rndMat.revision = revision;
            rndMat.altRevision = altRevision;
            return rndMat;
        }

    }
}