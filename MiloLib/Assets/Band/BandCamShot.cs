using MiloLib.Assets.World;
using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets.Band
{
    [Name("BandCamShot"), Description("Band specific camera shot")]
    public class BandCamShot : Object
    {
        public class OldTrigger
        {
            public float frame;
            public Symbol trigger = new(0, "");

            public OldTrigger Read(EndianReader reader)
            {
                frame = reader.ReadFloat();
                trigger = Symbol.Read(reader);
                return this;
            }

            public void Write(EndianWriter writer)
            {
                writer.WriteFloat(frame);
                Symbol.Write(writer, trigger);
            }
        }

        public class Target
        {
            [Name("Target"), Description("Symbolic name of target")]
            public Symbol target = new(0, "");

            [Name("Teleport"), Description("do we teleport this character?"), MinVersion(11)]
            public byte teleport = 1;

            [Name("To"), Description("the transform to teleport the character to")]
            public Matrix xfm = new();

            [MaxVersion(0)]
            public Matrix oldExtraTf = new();

            [Name("Anim Group"), Description("CharClipGroup to play on character")]
            public Symbol animGroup = new(0, "");

            [Name("Return"), Description("return to original position after shot?")]
            public byte returnVal = 1;

            [MinVersion(3), MaxVersion(28)]
            public bool oldBool;
            [MinVersion(3), MaxVersion(28)]
            public int oldBoolInt;

            [MinVersion(3), MaxVersion(11)]
            public int oldInt0C;

            [Name("Fast Forward"), Description("Fast forward chosen animation by this time, in camera units"), MinVersion(4)]
            public float fastForward;

            [Name("Forward Event"), Description("Event to fastforward relative to"), MinVersion(11)]
            public Symbol forwardEvent = new(0, "");

            [Name("Self Shadow"), Description("should character cast a self shadow"), MinVersion(6)]
            public byte selfShadow = 1;

            [MinVersion(7)]
            public byte unk1;
            [MinVersion(7)]
            public byte unk2 = 1;

            [Name("Hide"), Description("should the target be hidden"), MinVersion(32)]
            public byte hide;

            [MinVersion(8), MaxVersion(28)]
            public Symbol oldSym1 = new(0, "");
            [MinVersion(8), MaxVersion(28)]
            public Symbol oldSym2 = new(0, "");
            [MinVersion(9), MaxVersion(28)]
            public Symbol oldSym3 = new(0, "");
            [MinVersion(9), MaxVersion(28)]
            public Symbol oldSym4 = new(0, "");

            [Name("Env Override"), Description("environment override for this target during this shot"), MinVersion(10)]
            public Symbol envOverride = new(0, "");

            [MinVersion(17), MaxVersion(28)]
            public bool oldBool1728;

            [Name("Force LOD"), Description("Forces LOD, kLODPerFrame is normal behavior of picking per frame, the others force the lod (0 is highest res lod, 2 is lowest res lod)"), MinVersion(21)]
            public int forceLod = -1;

            [MinVersion(22), MaxVersion(28)]
            public Symbol oldStr1 = new(0, "");
            [MinVersion(22), MaxVersion(28)]
            public Symbol oldStr2 = new(0, "");
            [MinVersion(22), MaxVersion(28)]
            public int oldInt1;
            [MinVersion(22), MaxVersion(28)]
            public int oldInt2;
            [MinVersion(24), MaxVersion(28)]
            public int oldInt3;
            [MinVersion(24), MaxVersion(28)]
            public int oldInt4;
            [MinVersion(24), MaxVersion(28)]
            public int oldInt5;

            public Target Read(EndianReader reader, ushort bandRev)
            {
                target = Symbol.Read(reader);
                if (bandRev > 0xA)
                    teleport = reader.ReadByte();
                xfm = new Matrix().Read(reader);
                if (bandRev == 0)
                    oldExtraTf = new Matrix().Read(reader);
                animGroup = Symbol.Read(reader);
                returnVal = reader.ReadByte();
                if (bandRev > 2 && bandRev < 0x1D)
                {
                    oldBool = reader.ReadBoolean();
                    oldBoolInt = reader.ReadInt32();
                }
                if (bandRev > 2 && bandRev < 0xC)
                    oldInt0C = reader.ReadInt32();
                if (bandRev > 3)
                    fastForward = reader.ReadFloat();
                if (bandRev > 0xA)
                    forwardEvent = Symbol.Read(reader);
                if (bandRev > 5)
                    selfShadow = reader.ReadByte();
                if (bandRev > 6)
                {
                    unk1 = reader.ReadByte();
                    unk2 = reader.ReadByte();
                }
                if (bandRev > 6 && bandRev > 0x1F)
                    hide = reader.ReadByte();
                if (bandRev >= 8 && bandRev <= 28)
                {
                    oldSym1 = Symbol.Read(reader);
                    oldSym2 = Symbol.Read(reader);
                    if (bandRev > 8)
                    {
                        oldSym3 = Symbol.Read(reader);
                        oldSym4 = Symbol.Read(reader);
                    }
                }
                if (bandRev > 9)
                    envOverride = Symbol.Read(reader);
                if (bandRev >= 17 && bandRev <= 28)
                    oldBool1728 = reader.ReadBoolean();
                if (bandRev > 0x14)
                {
                    if (bandRev < 0x1E)
                        forceLod = reader.ReadInt32();
                    else
                        forceLod = (sbyte)reader.ReadByte();
                }
                if (bandRev >= 22 && bandRev <= 28)
                {
                    oldStr1 = Symbol.Read(reader);
                    oldStr2 = Symbol.Read(reader);
                    oldInt1 = reader.ReadInt32();
                    oldInt2 = reader.ReadInt32();
                    if (bandRev > 0x17)
                    {
                        oldInt3 = reader.ReadInt32();
                        oldInt4 = reader.ReadInt32();
                        oldInt5 = reader.ReadInt32();
                    }
                }
                return this;
            }

            public void Write(EndianWriter writer, ushort bandRev)
            {
                Symbol.Write(writer, target);
                if (bandRev > 0xA)
                    writer.WriteByte(teleport);
                xfm.Write(writer);
                if (bandRev == 0)
                    oldExtraTf.Write(writer);
                Symbol.Write(writer, animGroup);
                writer.WriteByte(returnVal);
                if (bandRev > 2 && bandRev < 0x1D)
                {
                    writer.WriteBoolean(oldBool);
                    writer.WriteInt32(oldBoolInt);
                }
                if (bandRev > 2 && bandRev < 0xC)
                    writer.WriteInt32(oldInt0C);
                if (bandRev > 3)
                    writer.WriteFloat(fastForward);
                if (bandRev > 0xA)
                    Symbol.Write(writer, forwardEvent);
                if (bandRev > 5)
                    writer.WriteByte(selfShadow);
                if (bandRev > 6)
                {
                    writer.WriteByte(unk1);
                    writer.WriteByte(unk2);
                }
                if (bandRev > 6 && bandRev > 0x1F)
                    writer.WriteByte(hide);
                if (bandRev >= 8 && bandRev <= 28)
                {
                    Symbol.Write(writer, oldSym1);
                    Symbol.Write(writer, oldSym2);
                    if (bandRev > 8)
                    {
                        Symbol.Write(writer, oldSym3);
                        Symbol.Write(writer, oldSym4);
                    }
                }
                if (bandRev > 9)
                    Symbol.Write(writer, envOverride);
                if (bandRev >= 17 && bandRev <= 28)
                    writer.WriteBoolean(oldBool1728);
                if (bandRev > 0x14)
                {
                    if (bandRev < 0x1E)
                        writer.WriteInt32(forceLod);
                    else
                        writer.WriteByte((byte)(sbyte)forceLod);
                }
                if (bandRev >= 22 && bandRev <= 28)
                {
                    Symbol.Write(writer, oldStr1);
                    Symbol.Write(writer, oldStr2);
                    writer.WriteInt32(oldInt1);
                    writer.WriteInt32(oldInt2);
                    if (bandRev > 0x17)
                    {
                        writer.WriteInt32(oldInt3);
                        writer.WriteInt32(oldInt4);
                        writer.WriteInt32(oldInt5);
                    }
                }
            }
        }

        private ushort altRevision;
        private ushort revision;

        public CamShot camShot = new();

        [Name("Targets")]
        public List<Target> targets = new();

        [MinVersion(2), MaxVersion(18)]
        public Symbol oldNextShot = new(0, "");

        [Name("Zero Time"), Description("synchronization time for this camshot"), MinVersion(4)]
        public float zeroTime;

        [MinVersion(13), MaxVersion(13)]
        public bool oldPS3PerPixel;

        [MinVersion(15), MaxVersion(28)]
        public List<OldTrigger> oldTriggers = new();

        [MinVersion(16), MaxVersion(28)]
        public Symbol oldEventTrig1 = new(0, "");
        [MinVersion(16), MaxVersion(28)]
        public Symbol oldEventTrig2 = new(0, "");

        [Name("Min Time"), Description("30fps reg: minimum time this shot can last, DCuts: time past zero time in which the shot can be interupted"), MinVersion(18)]
        public int minTime;

        [Name("Max Time"), Description("30fps maximum duration for this shot, 0 is infinite"), MinVersion(18)]
        public int maxTime;

        [Name("Next Shots"), Description("Next camshots, in order"), MinVersion(19)]
        public List<Symbol> nextShots = new();

        [MinVersion(20), MaxVersion(30)]
        public int oldInt2030;

        [MinVersion(23), MaxVersion(28)]
        public bool oldResetBool;

        [MinVersion(28), MaxVersion(28)]
        public List<Symbol> oldAnims1C = new();

        public new BandCamShot Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            if (revision > 4)
                camShot = new CamShot().Read(reader, false, parent, entry);

            uint targetCount = reader.ReadUInt32();
            for (int i = 0; i < targetCount; i++)
                targets.Add(new Target().Read(reader, revision));

            if (revision >= 2 && revision <= 18)
                oldNextShot = Symbol.Read(reader);
            if (revision > 3)
                zeroTime = reader.ReadFloat();

            if (revision <= 4)
                camShot = new CamShot().Read(reader, false, parent, entry);

            if (revision == 0xD)
                oldPS3PerPixel = reader.ReadBoolean();

            if (revision >= 15 && revision <= 28)
            {
                uint trigCount = reader.ReadUInt32();
                for (int i = 0; i < trigCount; i++)
                    oldTriggers.Add(new OldTrigger().Read(reader));
            }

            if (revision >= 16 && revision <= 28)
            {
                oldEventTrig1 = Symbol.Read(reader);
                oldEventTrig2 = Symbol.Read(reader);
            }

            if (revision > 0x11)
            {
                minTime = reader.ReadInt32();
                maxTime = reader.ReadInt32();
            }

            if (revision > 0x12)
            {
                uint count = reader.ReadUInt32();
                for (int i = 0; i < count; i++)
                    nextShots.Add(Symbol.Read(reader));
            }

            if (revision >= 20 && revision <= 30)
                oldInt2030 = reader.ReadInt32();
            if (revision >= 23 && revision <= 28)
                oldResetBool = reader.ReadBoolean();

            if (revision == 0x1C)
            {
                uint count = reader.ReadUInt32();
                for (int i = 0; i < count; i++)
                    oldAnims1C.Add(Symbol.Read(reader));
            }

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw MiloLib.Exceptions.MiloAssetReadException.EndBytesNotFound(parent, entry, reader.BaseStream.Position);

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            if (revision > 4)
                camShot.Write(writer, false, parent, entry);

            writer.WriteUInt32((uint)targets.Count);
            foreach (var t in targets)
                t.Write(writer, revision);

            if (revision >= 2 && revision <= 18)
                Symbol.Write(writer, oldNextShot);
            if (revision > 3)
                writer.WriteFloat(zeroTime);

            if (revision <= 4)
                camShot.Write(writer, false, parent, entry);

            if (revision == 0xD)
                writer.WriteBoolean(oldPS3PerPixel);

            if (revision >= 15 && revision <= 28)
            {
                writer.WriteUInt32((uint)oldTriggers.Count);
                foreach (var t in oldTriggers)
                    t.Write(writer);
            }

            if (revision >= 16 && revision <= 28)
            {
                Symbol.Write(writer, oldEventTrig1);
                Symbol.Write(writer, oldEventTrig2);
            }

            if (revision > 0x11)
            {
                writer.WriteInt32(minTime);
                writer.WriteInt32(maxTime);
            }

            if (revision > 0x12)
            {
                writer.WriteUInt32((uint)nextShots.Count);
                foreach (var s in nextShots)
                    Symbol.Write(writer, s);
            }

            if (revision >= 20 && revision <= 30)
                writer.WriteInt32(oldInt2030);
            if (revision >= 23 && revision <= 28)
                writer.WriteBoolean(oldResetBool);

            if (revision == 0x1C)
            {
                writer.WriteUInt32((uint)oldAnims1C.Count);
                foreach (var s in oldAnims1C)
                    Symbol.Write(writer, s);
            }

            if (standalone)
            {
                writer.WriteEndBytes();
            }
        }
    }
}
