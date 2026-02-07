using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets.Rnd
{
    [Name("PostProc"), Description("A PostProc drives post-processing effects.")]
    public class RndPostProc : Object
    {
        public enum HallOfTimeType
        {
            kHOTBlended,
            kHOTSolidRingsDepth,
            kHOTSolidRingsAlpha
        }
        private ushort altRevision;
        private ushort revision;

        public float priority;
        [Name("Bloom Color"), Description("Color tint for bloom effect")]
        public HmxColor4 bloomColor = new();
        [Name("Bloom Threshold"), Description("Luminance intensity at which to bloom")]
        public float bloomThreshold;
        [Name("Bloom Intensity"), Description("Bloom intensity")]
        public float bloomIntensity;
        [Name("Bloom Glare"), Description("Whether or not to use the glare effect")]
        public bool bloomGlare;
        [Name("Bloom Streak"), Description("Whether or not to use directional light streaks")]
        public bool bloomStreak;
        [Name("Bloom Streak Attenuation"), Description("Attenuation (scattering amount) of light streak.\n0.9 to 0.95 is the sweet spot.")]
        public float bloomStreakAttenuation;
        [Name("Bloom Streak Angle"), Description("Angle for light streak")]
        public float bloomStreakAngle;
        [Name("Luminance Map"), Description("Luminance map")]
        public Symbol luminanceMap = new Symbol(0, "");
        public bool forceCurrentInterp;
        [Name("Color Xfm")]
        public RndColorXfm colorXfm = new();
        [Name("Poster Levels")]
        public float posterLevels;
        [Name("Poster Min")]
        public float posterMin;
        [Name("Kaleidoscope Complexity"), Description("Number of slices in kaleidoscope, 0 turns off, 2 for vertical mirror")]
        public float kaleidoscopeComplexity;
        [Name("Kaleidoscope Size"), Description("Smaller size means more repeated areas, but each area is smaller")]
        public float kaleidoscopeSize;
        [Name("Kaleidoscope Angle"), Description("Additional clockwise degrees of rotation around center.")]
        public float kaleidoscopeAngle;
        [Name("Kaleidoscope Radius"), Description("Additional distance from center")]
        public float kaleidoscopeRadius;
        [Name("Kaleidoscope Flip UVs"), Description("Flip texture UV coords when reflect")]
        public bool kaleidoscopeFlipUVs;
        [Name("Flicker Mod Bounds")]
        public Vector2 flickerModBounds = new();
        [Name("Flicker Time Bounds")]
        public Vector2 flickerTimeBounds = new();
        [Name("Flicker Seconds"), Description("Min & max number of seconds for a light to dark cycle")]
        public Vector2 flickerSeconds = new();

        [Name("Color Modulation")]
        public float colorModulation;

        [Name("Noise Base Scale"), Description("X and Y tiling of the noise map")]
        public Vector2 noiseBaseScale = new();

        [Name("Noise Top Scale")]
        public float noiseTopScale;

        [Name("Noise Intensity"), Description("intensity of the noise [-1..1], 0.0 to disable")]
        public float noiseIntensity;
        [Name("Noise Stationary"), Description("keep the noise map static over the screen")]
        public bool noiseStationary;
        [Name("Noise Midtone"), Description("Applies the noise using at mid-tones of the scene, using an Overlay blend mode.")]
        public bool noiseMidtone;
        [Name("Noise Map"), Description("Optional noise bitmap")]
        public Symbol noiseMap = new Symbol(0, "");
        [Name("Trail Threshold"), Description("Min pixel value to leave trails [0..1]")]
        public float trailThreshold;
        [Name("Trail Duration"), Description("Seconds for the trails to last")]
        public float trailDuration;

        public Vector3 blendVec = new();
        [Name("Emulate FPS"), Description("Frame rate to emulate, e.g. 24 for film. 0 disables emulation.")]
        public float emulateFPS;

        [Name("Hall Of Time Type"), Description("Should the effect be blended, or should it produce solid colors?")]
        public HallOfTimeType hallOfTimeType;
        [Name("Hall Of Time Rate"), Description("Speed of effect.  0 is off.  1 is regular speed.")]
        public float hallOfTimeRate;
        [Name("Hall Of Time Color"), Description("Seconds for the trails to last.")]
        public HmxColor4 hallOfTimeColor = new();
        [Name("Hall Of Time Mix"), Description("Amount of color to blend. 0 is no color, 1 is solid color.\nNot applicable if solid rings checked.")]
        public float hallOfTimeMix;
        [Name("Motion Blur Weight"), Description("The weighting for individual color channels in the previous frame blend.")]
        public HmxColor4 motionBlurWeight = new();
        [Name("Motion Blur Blend"), Description("The amount of the previous frame to blend into the current frame. This can be used to efficiently simulate motion blur or other effects. Set to zero to disable.")]
        public float motionBlurBlend;
        [Name("Motion Blur Velocity"), Description("Whether or not to use the velocity motion blur effect. Should be enabled almost all the time.")]
        public bool motionBlurVelocity;
        [Name("Gradient Map"), Description("Gradient map; this texture should be layed out horizontally such that the color to use when the pixel is black is on the left and white is on the right.")]
        public Symbol gradientMap = new Symbol(0, "");
        [Name("Gradient Map Opacity"), Description("The opacity of the gradient map effect.")]
        public float gradientMapOpacity;
        [Name("Gradient Map Index"), Description("This indexes veritically into the gradient map texture. This is useful for storing multiple gradient map textures in a single texture, and to blend between them.")]
        public float gradientMapIndex;
        [Name("Gradient Map Start"), Description("The depth where the gradient map will begin to take effect.")]
        public float gradientMapStart;
        [Name("Gradient Map End"), Description("The depth where the gradient map will no longer take effect.")]
        public float gradientMapEnd;
        [Name("Refract Map"), Description("This is a normal map used to distort the screen.")]
        public Symbol refractMap = new Symbol(0, "");
        [Name("Refract Dist"), Description("The distance to refract each pixel of the screen. This can also be negative to reverse the direction. Set to zero to disable.")]
        public float refractDist;
        [Name("Refract Scale"), Description("This scales the refraction texture before distorting the screen, in the X and Y directions.")]
        public Vector2 refractScale = new();
        [Name("Refract Panning"), Description("The amount to offset the refraction texture, in the X and Y directions. This is a fixed amount to offset the refraction effect.")]
        public Vector2 refractPanning = new();
        [Name("Refract Velocity"), Description("The velocity to scroll the refraction texture, in the X and Y directions. The value is specified in units per second, and will offset the refraction effect over time.")]
        public Vector2 refractVelocity = new();
        [Name("Refract Angle"), Description("The angle to rotate the refraction texture, in degrees.")]
        public float refractAngle;
        [Name("Chromatic Aberration Offset"), Description("The size, in pixels, of the chromatic aberration or sharpen effect.")]
        public float chromaticAberrationOffset;
        [Name("Chromatic Sharpen"), Description("Whether to sharpen the chromatic image or apply the aberration effect.")]
        public bool chromaticSharpen;
        [Name("Vignette Color"), Description("Color tint for vignette effect")]
        public HmxColor4 vignetteColor = new();
        [Name("Vignette Intensity"), Description("0 for no effect, 1 for normal, less than one for smaller effect, 2 is full color")]
        public float vignetteIntensity;


        public RndPostProc Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            base.Read(reader, false, parent, entry);

            if (revision > 4)
            {
                if (revision > 10)
                {
                    bloomColor = bloomColor.Read(reader);
                    if (revision < 24)
                    {
                        //bs >> x;
                    }
                    bloomIntensity = reader.ReadFloat();
                    bloomThreshold = reader.ReadFloat();
                }
                else
                {
                    //Hmx::Color c;
                    //bs >> c;
                }
            }
            luminanceMap = Symbol.Read(reader);

            colorXfm = colorXfm.Read(reader);

            flickerModBounds = flickerModBounds.Read(reader);
            flickerTimeBounds = flickerTimeBounds.Read(reader);

            noiseBaseScale = noiseBaseScale.Read(reader);
            noiseIntensity = reader.ReadFloat();
            noiseTopScale = reader.ReadFloat();
            noiseStationary = reader.ReadBoolean();
            noiseMap = Symbol.Read(reader);
            noiseMidtone = reader.ReadBoolean();

            if (revision > 7)
            {
                trailThreshold = reader.ReadFloat();
                trailDuration = reader.ReadFloat();
                emulateFPS = reader.ReadFloat();
            }

            if (revision > 9)
            {
                if (revision > 0x12)
                {
                    // color correction stuff?
                }
                posterLevels = reader.ReadFloat();
            }

            if (revision > 13)
                posterMin = reader.ReadFloat();

            kaleidoscopeComplexity = reader.ReadFloat();
            kaleidoscopeSize = reader.ReadFloat();
            kaleidoscopeAngle = reader.ReadFloat();
            kaleidoscopeRadius = reader.ReadFloat();
            kaleidoscopeFlipUVs = reader.ReadBoolean();

            hallOfTimeRate = reader.ReadFloat();
            hallOfTimeColor = hallOfTimeColor.Read(reader);
            hallOfTimeMix = reader.ReadFloat();
            hallOfTimeType = (HallOfTimeType)reader.ReadInt32();

            motionBlurBlend = reader.ReadFloat();
            motionBlurWeight = motionBlurWeight.Read(reader);
            motionBlurVelocity = reader.ReadBoolean();


            gradientMap = Symbol.Read(reader);
            gradientMapOpacity = reader.ReadFloat();
            gradientMapIndex = reader.ReadFloat();
            gradientMapStart = reader.ReadFloat();
            gradientMapEnd = reader.ReadFloat();

            refractMap = Symbol.Read(reader);
            refractDist = reader.ReadFloat();
            refractScale = refractScale.Read(reader);
            refractPanning = refractPanning.Read(reader);
            refractAngle = reader.ReadFloat();
            refractVelocity = refractVelocity.Read(reader);

            chromaticAberrationOffset = reader.ReadFloat();
            chromaticSharpen = reader.ReadBoolean();

            vignetteColor = vignetteColor.Read(reader);
            vignetteIntensity = reader.ReadFloat();

            if (revision > 32)
                bloomGlare = reader.ReadBoolean();
            if (revision > 35)
            {
                bloomStreak = reader.ReadBoolean();
                bloomStreakAttenuation = reader.ReadFloat();
                bloomStreakAngle = reader.ReadFloat();
            }

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw MiloLib.Exceptions.MiloAssetReadException.EndBytesNotFound(parent, entry, reader.BaseStream.Position);

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)(altRevision << 16 | revision) : (uint)(revision << 16 | altRevision));
            base.Write(writer, false, parent, entry);
            if (revision > 4)
            {
                if (revision > 10)
                {
                    bloomColor.Write(writer);
                    if (revision < 24)
                    {
                        //bs << x;
                    }
                    writer.WriteFloat(bloomIntensity);
                    writer.WriteFloat(bloomThreshold);
                }
                else
                {
                    //Hmx::Color c;
                    //bs << c;
                }
            }
            Symbol.Write(writer, luminanceMap);
            colorXfm.Write(writer);
            flickerModBounds.Write(writer);
            flickerTimeBounds.Write(writer);
            noiseBaseScale.Write(writer);
            writer.WriteFloat(noiseIntensity);
            writer.WriteFloat(noiseTopScale);
            writer.WriteBoolean(noiseStationary);
            Symbol.Write(writer, noiseMap);
            writer.WriteBoolean(noiseMidtone);
            if (revision > 7)
            {
                writer.WriteFloat(trailThreshold);
                writer.WriteFloat(trailDuration);
                writer.WriteFloat(emulateFPS);
            }
            if (revision > 9)
            {
                if (revision > 0x12)
                {
                    // color correction stuff?
                }
                writer.WriteFloat(posterLevels);
            }
            if (revision > 13)
                writer.WriteFloat(posterMin);
            writer.WriteFloat(kaleidoscopeComplexity);
            writer.WriteFloat(kaleidoscopeSize);
            writer.WriteFloat(kaleidoscopeAngle);
            writer.WriteFloat(kaleidoscopeRadius);
            writer.WriteBoolean(kaleidoscopeFlipUVs);
            writer.WriteFloat(hallOfTimeRate);
            hallOfTimeColor.Write(writer);
            writer.WriteFloat(hallOfTimeMix);
            writer.WriteUInt32((uint)hallOfTimeType);
            writer.WriteFloat(motionBlurBlend);
            motionBlurWeight.Write(writer);
            writer.WriteBoolean(motionBlurVelocity);
            Symbol.Write(writer, gradientMap);
            writer.WriteFloat(gradientMapOpacity);
            writer.WriteFloat(gradientMapIndex);
            writer.WriteFloat(gradientMapStart);
            writer.WriteFloat(gradientMapEnd);
            Symbol.Write(writer, refractMap);
            writer.WriteFloat(refractDist);
            refractScale.Write(writer);
            refractPanning.Write(writer);
            writer.WriteFloat(refractAngle);
            refractVelocity.Write(writer);
            writer.WriteFloat(chromaticAberrationOffset);
            writer.WriteBoolean(chromaticSharpen);
            vignetteColor.Write(writer);
            writer.WriteFloat(vignetteIntensity);
            if (revision > 32)
                writer.WriteBoolean(bloomGlare);
            if (revision > 35)
            {
                writer.WriteBoolean(bloomStreak);
                writer.WriteFloat(bloomStreakAttenuation);
                writer.WriteFloat(bloomStreakAngle);
            }

            if (standalone)
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
        }
    }
}