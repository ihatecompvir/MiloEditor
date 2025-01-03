using System.Reflection.Metadata;
using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets.Rnd
{
    [Name("RndParticleSys"), Description("A particle system")]
    public class RndParticleSys : Object
    {
        private ushort altRevision;
        private ushort revision;

        [MinVersion(23)]
        public ObjectFields obj1 = new();
        [MinVersion(23)]
        public ObjectFields obj2 = new();

        public RndAnimatable anim = new();
        public RndTrans trans = new();
        public RndDrawable draw = new();

        [Name("Life"), Description("Frame range of particle life.")]
        public Vector2 life = new();

        [Name("Height Ratio"), Description("Ratio of screen height to width"), MinVersion(36)]
        public float heightRatio;

        [Name("Position Low"), Description("Min point, in object coordinates, of box region that particles are emitted from.")]
        public Vector3 posLow = new();
        [Name("Position High"), Description("Max point, in object coordinates, of box region that particles are emitted from.")]
        public Vector3 posHigh = new();

        [Name("Speed"), Description("Speed range, in world units per frame, of particles.")]
        public Vector2 speed = new();
        [Name("Pitch"), Description("Rotation range around the X axis")]
        public Vector2 pitch = new();
        [Name("Yaw"), Description("Rotation range around the Y axis")]
        public Vector2 yaw = new();
        [Name("Emit Rate"), Description("Frame range to generate particles.")]
        public Vector2 emitRate = new();

        [Name("Max Bursts"), Description("Maximum number of bursts"), MinVersion(33)]
        public uint maxBursts;
        [Name("Burst Interval"), Description("Time between bursts"), MinVersion(33)]
        public Vector2 burstInterval = new();
        [Name("Burst Peak"), Description("Peak rate of particles during a burst"), MinVersion(33)]
        public Vector2 burstPeak = new();
        [Name("Burst Length"), Description("Duration of the burst."), MinVersion(33)]
        public Vector2 burstLength = new();

        [Name("Start Size"), Description("Size range, in world units, of particles.")]
        public Vector2 startSize = new();

        [Name("Delta Size"), Description("Change in size of particles, in world units."), MinVersion(16)]
        public Vector2 deltaSize = new();

        [Name("Start Color Low"), Description("Random color ranges for start and end color of particles.")]
        public HmxColor4 startColorLow = new();
        [Name("Start Color High"), Description("Random color ranges for start and end color of particles.")]
        public HmxColor4 startColorHigh = new();

        [Name("End Color Low"), Description("Random color ranges for start and end color of particles.")]
        public HmxColor4 endColorLow = new();
        [Name("End Color High"), Description("Random color ranges for start and end color of particles.")]
        public HmxColor4 endColorHigh = new();

        [Name("Bounce"), Description("Specify a collide plane to reflect particles. Used to bounce particles off surfaces.")]
        public Symbol bounce = new(0, "");

        [Name("Force Direction"), Description("Force direction in world coordinates, in units per frame added to each particle's velocity. Can be used for gravity.")]
        public Vector3 force = new();

        [Name("Material"), Description("Material for particle system")]
        public Symbol material = new(0, "");

        [MaxVersion(12)]
        public uint unkInt;
        [MinVersion(18)]
        public uint unkInt2;

        [Name("Grow Ratio"), Description("Controls the rate that the particle system grows."), MinVersion(18)]
        public float growRatio;
        [Name("Shrink Ratio"), Description("Controls the rate that the particle system shrinks."), MinVersion(18)]
        public float shrinkRatio;
        [Name("Mid Color Ratio"), Description("Controls the midpoint of the color transition."), MinVersion(18)]
        public float midColorRatio;

        [Name("Mid Color Low"), Description("The low color value when the particle reaches the mid point."), MinVersion(18)]
        public HmxColor4 midColorLow = new();
        [Name("Mid Color High"), Description("The high color value when the particle reaches the mid point."), MinVersion(18)]
        public HmxColor4 midColorHigh = new();

        [Name("Max Particles"), Description("Maximum number of particles")]
        public uint maxParticles;

        [MaxVersion(6)]
        public uint unkInt3;
        [MinVersion(7), MaxVersion(12)]
        public uint unkInt4;

        [Name("Bubble Period"), Description("Controls the period of bubble effect"), MinVersion(4)]
        public Vector2 bubblePeriod = new();
        [Name("Bubble Size"), Description("Controls the size of the bubble effect"), MinVersion(4)]
        public Vector2 bubbleSize = new();
        [Name("Bubble"), Description("Enable/disable bubble effect"), MinVersion(4)]
        public bool bubble;

        [Name("Rotate"), Description("Enable/disable particle rotation"), MinVersion(30)]
        public bool rotate;

        [Name("Rotation Speed"), Description("Rotation speed of the particles"), MinVersion(30)]
        public Vector2 rotSpeed = new();
        [Name("Rotation Drag"), Description("Drag of the particle rotation"), MinVersion(30)]
        public float rotDrag;
        [Name("Rotation Random Direction"), Description("Use random rotation direction"), MinVersion(37)]
        public bool rotRandomDirection;

        [Name("Drag"), Description("Drag of the particles"), MinVersion(30)]
        public uint drag;

        [Name("Swing Arm Start"), Description("Start offset of swing arm"), MinVersion(32)]
        public Vector2 swingArmStart = new();
        [Name("Swing Arm End"), Description("End offset of swing arm"), MinVersion(32)]
        public Vector2 swingArmEnd = new();

        [Name("Align with Velocity"), Description("Align particle direction with its velocity"), MinVersion(32)]
        public bool alignWithVelocity;
        [Name("Stretch with Velocity"), Description("Stretch the particle along its velocity"), MinVersion(32)]
        public bool stretchWithVelocity;
        [Name("Constant Area"), Description("Maintain constant particle area while scaling"), MinVersion(32)]
        public bool constantArea;

        [Name("Stretch Scale"), Description("Amount to stretch the particle"), MinVersion(32)]
        public float stretchScale;

        [Name("Perspective Stretch"), Description("Enable/disable perspective stretch"), MinVersion(34)]
        public bool perspectiveStretch;

        [Name("Relative Motion"), Description("Relative motion of the particle system")]
        public float relativeMotion;

        [Name("Relative Parent"), Description("The transform the particle system is relative to"), MinVersion(27)]
        public Symbol relativeParent = new(0, "");

        [Name("Mesh"), Description("The emitter mesh of the particle system"), MinVersion(19)]
        public Symbol mesh = new(0, "");

        // this one is weird and not supported by the versioning system (revision over 30 or exactly 21, which doesn't work with Min and Max version) - just show it at all times
        [Name("Sub Samples"), Description("Subsamples of the emitter")]
        public uint subSamples;

        [Name("Frame Drive"), Description("Enable/disable frame driven animation"), MinVersion(29)]
        public bool frameDrive;

        [Name("Pause Offscreen"), Description("Enable/disable pausing of the emitter when off screen"), MinVersion(36)]
        public bool pauseOffscreen;

        [Name("Fast Forward"), Description("Enable/disable fast forward"), MinVersion(30)]
        public bool fastForward;

        [Name("Preserve Particles"), Description("Enable/disable particle perservation"), MinVersion(11)]
        public bool preserveParticles;

        public RndParticleSys Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            // only read these if revision is greater than 22
            if (revision > 22)
            {
                obj1.Read(reader, parent, entry);
                obj2.Read(reader, parent, entry);
            }

            anim.Read(reader, parent, entry);
            trans.Read(reader, false, parent, entry);
            draw.Read(reader, false, parent, entry);

            life.Read(reader);

            if (35 < revision)
                heightRatio = reader.ReadFloat();

            posLow.Read(reader);
            posHigh.Read(reader);

            speed.Read(reader);
            pitch.Read(reader);
            yaw.Read(reader);
            emitRate.Read(reader);

            if (32 < revision)
            {
                maxBursts = reader.ReadUInt32();
                burstInterval.Read(reader);
                burstPeak.Read(reader);
                burstLength.Read(reader);
            }

            startSize.Read(reader);

            if (15 < revision)
                deltaSize.Read(reader);

            startColorLow.Read(reader);
            startColorHigh.Read(reader);

            endColorLow.Read(reader);
            endColorHigh.Read(reader);

            bounce = Symbol.Read(reader);

            force.Read(reader);

            material = Symbol.Read(reader);

            if (revision < 18)
            {
                if (revision < 13)
                {
                    unkInt = reader.ReadUInt32();
                }
            }
            else
            {
                unkInt2 = reader.ReadUInt32();
                growRatio = reader.ReadFloat();
                shrinkRatio = reader.ReadFloat();

                midColorRatio = reader.ReadFloat();

                midColorLow.Read(reader);
                midColorHigh.Read(reader);
            }

            maxParticles = reader.ReadUInt32();

            if (2 < revision)
            {
                if (revision < 7)
                {
                    unkInt3 = reader.ReadUInt32();
                }
                else if (revision < 13)
                {
                    unkInt4 = reader.ReadUInt32();
                }
            }

            if (3 < revision)
            {
                bubblePeriod.Read(reader);
                bubbleSize.Read(reader);
                bubble = reader.ReadBoolean();
            }

            if (29 < revision)
            {
                rotate = reader.ReadBoolean();
                rotSpeed.Read(reader);
                rotDrag = reader.ReadFloat();
                if (36 < revision)
                {
                    rotRandomDirection = reader.ReadBoolean();
                }

                drag = reader.ReadUInt32();
            }

            if (31 < revision)
            {
                swingArmStart.Read(reader);
                swingArmEnd.Read(reader);

                alignWithVelocity = reader.ReadBoolean();
                stretchWithVelocity = reader.ReadBoolean();
                constantArea = reader.ReadBoolean();

                stretchScale = reader.ReadFloat();
            }

            if (33 < revision)
            {
                perspectiveStretch = reader.ReadBoolean();
            }

            relativeMotion = reader.ReadFloat();

            if (26 < revision)
                relativeParent = Symbol.Read(reader);

            if (18 < revision)
                mesh = Symbol.Read(reader);

            if (30 < revision || revision == 21)
                subSamples = reader.ReadUInt32();

            if (!(revision < 28))
                frameDrive = reader.ReadBoolean();

            if (!(revision < 35))
                pauseOffscreen = reader.ReadBoolean();

            if (!(revision < 29))
                fastForward = reader.ReadBoolean();

            if (!(revision < 11))
                preserveParticles = reader.ReadBoolean();

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw new Exception("Got to end of standalone asset but didn't find the expected end bytes, read likely did not succeed");
            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            obj1.Write(writer);
            obj2.Write(writer);

            anim.Write(writer);
            trans.Write(writer, false, true);
            draw.Write(writer, false, true);

            life.Write(writer);

            if (35 < revision)
                writer.WriteFloat(heightRatio);

            posLow.Write(writer);
            posHigh.Write(writer);

            speed.Write(writer);
            pitch.Write(writer);
            yaw.Write(writer);
            emitRate.Write(writer);

            if (32 < revision)
            {
                writer.WriteUInt32(maxBursts);
                burstInterval.Write(writer);
                burstPeak.Write(writer);
                burstLength.Write(writer);
            }

            startSize.Write(writer);

            if (15 < revision)
                deltaSize.Write(writer);

            startColorLow.Write(writer);
            startColorHigh.Write(writer);

            endColorLow.Write(writer);
            endColorHigh.Write(writer);

            Symbol.Write(writer, bounce);

            force.Write(writer);

            Symbol.Write(writer, material);

            if (revision < 18)
            {
                if (revision < 13)
                {
                    writer.WriteUInt32(unkInt);
                }
            }
            else
            {
                writer.WriteUInt32(unkInt2);
                writer.WriteFloat(growRatio);
                writer.WriteFloat(shrinkRatio);

                writer.WriteFloat(midColorRatio);

                midColorLow.Write(writer);
                midColorHigh.Write(writer);
            }

            writer.WriteUInt32(maxParticles);

            if (2 < revision)
            {
                if (revision < 7)
                {
                    writer.WriteUInt32(unkInt3);
                }
                else if (revision < 13)
                {
                    writer.WriteUInt32(unkInt4);
                }
            }

            if (3 < revision)
            {
                bubblePeriod.Write(writer);
                bubbleSize.Write(writer);
                writer.WriteBoolean(bubble);
            }

            if (29 < revision)
            {
                writer.WriteBoolean(rotate);
                rotSpeed.Write(writer);
                writer.WriteFloat(rotDrag);
                if (36 < revision)
                {
                    writer.WriteBoolean(rotRandomDirection);
                }

                writer.WriteUInt32(drag);
            }

            if (31 < revision)
            {
                swingArmStart.Write(writer);
                swingArmEnd.Write(writer);

                writer.WriteBoolean(alignWithVelocity);
                writer.WriteBoolean(stretchWithVelocity);
                writer.WriteBoolean(constantArea);

                writer.WriteFloat(stretchScale);
            }

            if (33 < revision)
            {
                writer.WriteBoolean(perspectiveStretch);
            }

            writer.WriteFloat(relativeMotion);

            if (26 < revision)
                Symbol.Write(writer, relativeParent);

            if (18 < revision)
                Symbol.Write(writer, mesh);

            if (30 < revision || revision == 21)
                writer.WriteUInt32(subSamples);

            if (!(revision < 28))
                writer.WriteBoolean(frameDrive);

            if (!(revision < 35))
                writer.WriteBoolean(pauseOffscreen);

            if (!(revision < 29))
                writer.WriteBoolean(fastForward);

            if (!(revision < 11))
                writer.WriteBoolean(preserveParticles);

            if (standalone)
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });


        }
    }
}