using System.Reflection.Metadata;
using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets.Rnd
{
    [Name("RndParticleSys"), Description("A particle system")]
    public class RndParticleSys : Object
    {
        public ushort altRevision;
        public ushort revision;

        public ObjectFields obj1 = new();
        public ObjectFields obj2 = new();

        public RndAnim anim = new();
        public RndTrans trans = new();
        public RndDraw draw = new();

        public Vector2 life = new();

        public float heightRatio;

        public Vector3 posLow = new();
        public Vector3 posHigh = new();

        public Vector2 speed = new();
        public Vector2 pitch = new();
        public Vector2 yaw = new();
        public Vector2 emitRate = new();

        public uint maxBursts;
        public Vector2 burstInterval = new();
        public Vector2 burstPeak = new();
        public Vector2 burstLength = new();

        public Vector2 startSize = new();

        public Vector2 deltaSize = new();

        public HmxColor startColorLow = new();
        public HmxColor startColorHigh = new();

        public HmxColor endColorLow = new();
        public HmxColor endColorHigh = new();

        public Symbol bounce = new(0, "");

        public Vector3 force = new();

        public Symbol material = new(0, "");

        public uint unkInt;
        public uint unkInt2;

        public float growRatio;
        public float shrinkRatio;

        public float midColorRatio;

        public HmxColor midColorLow = new();
        public HmxColor midColorHigh = new();

        public uint maxParticles;

        public uint unkInt3;
        public uint unkInt4;

        public Vector2 bubblePeriod = new();
        public Vector2 bubbleSize = new();
        public bool bubble;

        public bool rotate;

        public Vector2 rotSpeed = new();

        public float rotDrag;

        public bool rotRandomDirection;

        public uint drag;

        public Vector2 swingArmStart = new();
        public Vector2 swingArmEnd = new();

        public bool alignWithVelocity;
        public bool stretchWithVelocity;
        public bool constantArea;

        public float stretchScale;

        public bool perspectiveStretch;

        public float relativeMotion;

        public Symbol relativeParent = new(0, "");

        public Symbol mesh = new(0, "");

        public uint subSamples;

        public bool frameDrive;

        public bool pauseOffscreen;

        public bool fastForward;

        public bool preserveParticles;

        public RndParticleSys Read(EndianReader reader, bool standalone)
        {
            altRevision = reader.ReadUInt16();
            revision = reader.ReadUInt16();

            obj1.Read(reader);
            obj2.Read(reader);

            anim.Read(reader);
            trans.Read(reader, false);
            draw.Read(reader);

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

        public override void Write(EndianWriter writer, bool standalone)
        {
            writer.WriteUInt16(altRevision);
            writer.WriteUInt16(revision);

            obj1.Write(writer);
            obj2.Write(writer);

            anim.Write(writer);
            trans.Write(writer, false);
            draw.Write(writer);

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