using MiloLib.Classes;
using MiloLib.Utils;
using System;
using System.Collections.Generic;

namespace MiloLib.Assets.Char
{
    [Name("CharClip"), Description("This is the simple form that stores samples and linearly interpolates between them. Data is grouped by keyframe, for better RAM coherency better storage, interpolation, etc.")]
    public class CharClip : Object
    {
        public class CharBonesSamples
        {
            public class Bone
            {
                [Name("Name"), Description("Bone to blend into")]
                public Symbol name = new(0, "");
                [Name("Weight"), Description("Weight to blend with")]
                public float weight = 1.0f;

                public Bone Read(EndianReader reader, bool includeWeight)
                {
                    name = Symbol.Read(reader);
                    if (includeWeight)
                        weight = reader.ReadFloat();
                    return this;
                }

                public void Write(EndianWriter writer, bool includeWeight)
                {
                    Symbol.Write(writer, name);
                    if (includeWeight)
                        writer.WriteFloat(weight);
                }
            }

            public int version;
            public List<Bone> bones = new();
            public int[] counts = new int[7];
            public int compression;
            public int numSamples;
            public List<float> frames = new();
            public bool ver14Bool;
            public byte[] rawData = Array.Empty<byte>();

            // For old format (ver 6-9): raw ints read before compression
            public int[] oldHeaderInts = Array.Empty<int>();
            public int numCountsInStream;
            public int bytesPerSample;

            private static int TypeSize(int boneType, int comp)
            {
                switch (boneType)
                {
                    case 0: // POS
                    case 1: // SCALE
                        return comp >= 2 ? 6 : 12;
                    case 2: // QUAT
                        if (comp >= 3) return 4;
                        if (comp >= 1) return 8;
                        return 16;
                    case 3: // ROTX
                    case 4: // ROTY
                    case 5: // ROTZ
                        return comp != 0 ? 2 : 4;
                    default:
                        return 0;
                }
            }

            public void ComputeBytesPerSample()
            {
                int offset = 0;
                for (int i = 0; i < 6; i++)
                {
                    int numBones = counts[i + 1] - counts[i];
                    offset += numBones * TypeSize(i, compression);
                }
                // Stream stores data padded to 16-byte alignment per sample
                // (matches C++ mTotalSize = (mEndOffset + 0xF) & 0xFFFFFFF0)
                bytesPerSample = (offset + 0xF) & ~0xF;
            }

            public void Load(EndianReader reader)
            {
                version = reader.ReadInt32();
                LoadHeader(reader);
                LoadData(reader);
            }

            public void Save(EndianWriter writer)
            {
                writer.WriteInt32(version);
                WriteHeader(writer);
                WriteData(writer);
            }

            public void LoadHeader(EndianReader reader)
            {
                int numBones = reader.ReadInt32();
                bones = new List<Bone>();
                bool includeWeight = version > 10;
                for (int i = 0; i < numBones; i++)
                    bones.Add(new Bone().Read(reader, includeWeight));

                if (version > 9)
                {
                    numCountsInStream = version > 15 ? 7 : 10;
                    int numToKeep = Math.Min(7, numCountsInStream);
                    for (int i = 0; i < numToKeep; i++)
                        counts[i] = reader.ReadInt32();
                    for (int i = numToKeep; i < numCountsInStream; i++)
                        reader.ReadInt32();

                    compression = reader.ReadInt32();
                    numSamples = reader.ReadInt32();
                }
                else if (version > 5)
                {
                    int intCount;
                    if (version > 7) intCount = 9;
                    else if (version > 6) intCount = 6;
                    else intCount = 10;

                    oldHeaderInts = new int[intCount];
                    for (int i = 0; i < intCount; i++)
                        oldHeaderInts[i] = reader.ReadInt32();

                    compression = reader.ReadInt32();
                    numSamples = reader.ReadInt32();
                    ComputeCountsFromBones();
                }
                else if (version > 3)
                {
                    numSamples = reader.ReadInt32();
                    compression = reader.ReadInt32();
                    ComputeCountsFromBones();
                }
                else
                {
                    numSamples = reader.ReadInt32();
                    ComputeCountsFromBones();
                }

                if (version > 11)
                {
                    int frameCount = reader.ReadInt32();
                    frames = new List<float>(frameCount);
                    for (int i = 0; i < frameCount; i++)
                        frames.Add(reader.ReadFloat());
                }
                else
                    frames = new List<float>();

                ComputeBytesPerSample();
            }

            public void LoadData(EndianReader reader)
            {
                if (version == 14)
                    ver14Bool = reader.ReadBoolean();

                int totalBytes = bytesPerSample * numSamples;
                rawData = totalBytes > 0 ? reader.ReadBlock(totalBytes) : Array.Empty<byte>();
            }

            public void WriteHeader(EndianWriter writer)
            {
                writer.WriteInt32(bones.Count);
                bool includeWeight = version > 10;
                foreach (var bone in bones)
                    bone.Write(writer, includeWeight);

                if (version > 9)
                {
                    int numToWrite = version > 15 ? 7 : 10;
                    int numToKeep = Math.Min(7, numToWrite);
                    for (int i = 0; i < numToKeep; i++)
                        writer.WriteInt32(counts[i]);
                    for (int i = numToKeep; i < numToWrite; i++)
                        writer.WriteInt32(counts[6]);

                    writer.WriteInt32(compression);
                    writer.WriteInt32(numSamples);
                }
                else if (version > 5)
                {
                    foreach (int val in oldHeaderInts)
                        writer.WriteInt32(val);
                    writer.WriteInt32(compression);
                    writer.WriteInt32(numSamples);
                }
                else if (version > 3)
                {
                    writer.WriteInt32(numSamples);
                    writer.WriteInt32(compression);
                }
                else
                {
                    writer.WriteInt32(numSamples);
                }

                if (version > 11)
                {
                    writer.WriteInt32(frames.Count);
                    foreach (float f in frames)
                        writer.WriteFloat(f);
                }
            }

            public void WriteData(EndianWriter writer)
            {
                if (version == 14)
                    writer.WriteBoolean(ver14Bool);
                if (rawData.Length > 0)
                    writer.WriteBlock(rawData);
            }

            private void ComputeCountsFromBones()
            {
                for (int i = 0; i < 7; i++) counts[i] = 0;
                foreach (var bone in bones)
                {
                    int type = GetBoneType(bone.name.value);
                    if (type >= 0 && type < 6)
                        counts[type + 1]++;
                }
                for (int i = 1; i < 7; i++)
                    counts[i] += counts[i - 1];
            }

            private static int GetBoneType(string name)
            {
                int dotIdx = name.LastIndexOf('.');
                if (dotIdx < 0 || dotIdx >= name.Length - 1) return -1;
                switch (name[dotIdx + 1])
                {
                    case 'p': return 0;
                    case 's': return 1;
                    case 'q': return 2;
                    case 'r':
                        if (dotIdx + 3 < name.Length)
                        {
                            switch (name[dotIdx + 3])
                            {
                                case 'x': return 3;
                                case 'y': return 4;
                                case 'z': return 5;
                            }
                        }
                        return -1;
                    default: return -1;
                }
            }
        }

        public class GraphNode
        {
            [Name("Current Beat"), Description("where to blend from in my clip")]
            public float curBeat;
            [Name("Next Beat"), Description("where to blend to in clip")]
            public float nextBeat;
        }

        public class NodeVector
        {
            [Name("Clip"), Description("clip it's transitioning to")]
            public Symbol clip = new(0, "");
            public List<GraphNode> nodes = new();
        }

        public class BeatEvent
        {
            [Name("Event"), Description("The handler to call on the CharClip")]
            public Symbol event_ = new(0, "");
            [Name("Beat"), Description("Beat the event should trigger")]
            public float beat;
        }

        public class FrameEvent
        {
            public float frame;
            public Symbol script = new(0, "");
        }

        public class BeatTrackKey
        {
            public float frame;
            public float value;
        }

        public enum DefaultBlend
        {
            kPlayNoDefault = 0,
            kPlayNow = 1,
            kPlayNoBlend = 2,
            kPlayFirst = 3,
            kPlayLast = 4,
            kPlayDirty = 8
        }

        public enum DefaultLoop
        {
            kPlayNoLoop = 0x10,
            kPlayLoop = 0x20,
            kPlayGraphLoop = 0x30,
            kPlayNodeLoop = 0x40
        }

        public enum BeatAlignMode
        {
            kPlayBeatTime = 0,
            kPlayRealTime = 0x200,
            kPlayUserTime = 0x400,
            kPlayBeatAlign1 = 0x1000,
            kPlayBeatAlign2 = 0x2000,
            kPlayBeatAlign4 = 0x4000,
            kPlayBeatAlign8 = 0x8000
        }

        private ushort altRevision;
        private ushort revision;
        private int oldRev;

        [Name("Frames Per Second"), Description("Frames per second")]
        public float framesPerSec = 30.0f;

        [Name("Flags"), Description("Search flags, app specific")]
        public int flags;

        [Name("Play Flags")]
        public int playFlags;

        [Name("Range"), Description("Range in frames to randomly offset by when playing")]
        public float range;

        [Name("Relative"), Description("Make the clip all relative to this other clip's first frame")]
        public Symbol relative = new(0, "");

        [Name("Do Not Compress")]
        public bool doNotCompress;

        [Name("Transition Version")]
        public int transitionVersion;

        public int transitionsByteHint;
        [Name("Transitions"), Description("Indicates transition graph needs updating")]
        public List<NodeVector> transitions = new();

        [Name("Events"), Description("Events that get triggered during play")]
        public List<BeatEvent> beatEvents = new();

        [Name("Sync Anim"), Description("An animatable, like a PropAnim, you'd like play in sync with this clip"), MinVersion(19)]
        public Symbol syncAnim = new(0, "");

        [Name("Full Samples")]
        public CharBonesSamples full = new();

        [Name("One Samples")]
        public CharBonesSamples one = new();

        [Name("Zeros"), MinVersion(15)]
        public List<CharBonesSamples.Bone> zeros = new();

        [Name("Beat Track"), MinVersion(18)]
        public List<BeatTrackKey> beatTrack = new();

        // Old format fields preserved for round-tripping
        private int oldStartBeat;
        private int oldEndBeat;
        private int blendWidth;
        private bool relativeBool;
        private bool unkBool;
        private List<Symbol> oldStrings = new();
        private Symbol enterEvent = new(0, "");
        private Symbol exitEvent = new(0, "");
        private List<FrameEvent> oldFrameEvents = new();

        // Old bone format (gRev <= 12) state
        private byte[] oneInitialRawData = Array.Empty<byte>();
        private bool oneInitialVer14Bool;
        private CharBonesSamples extraBonesSamples;
        private int fullDataVersion;

        public CharClip Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            if (revision < 0x10)
                oldRev = reader.ReadInt32();
            else
                oldRev = 0xD;

            base.Read(reader, false, parent, entry);

            if (revision < 0x12)
            {
                oldStartBeat = reader.ReadInt32();
                oldEndBeat = reader.ReadInt32();
            }

            framesPerSec = reader.ReadFloat();
            flags = reader.ReadInt32();
            playFlags = reader.ReadInt32();

            if (oldRev < 0xD)
                blendWidth = reader.ReadInt32();

            if (oldRev > 3)
                range = reader.ReadFloat();

            if (oldRev > 5)
                relative = Symbol.Read(reader);
            else if (oldRev > 4)
                relativeBool = reader.ReadBoolean();

            if (oldRev == 9 || oldRev == 10)
                unkBool = reader.ReadBoolean();

            if (oldRev > 9)
                transitionVersion = reader.ReadInt32();

            if (oldRev > 0xB)
                doNotCompress = reader.ReadBoolean();

            ReadTransitions(reader);

            if (oldRev < 3)
            {
                uint symCount = reader.ReadUInt32();
                for (int i = 0; i < symCount; i++)
                    oldStrings.Add(Symbol.Read(reader));
            }

            if (oldRev > 6)
            {
                uint eventCount = reader.ReadUInt32();
                for (int i = 0; i < eventCount; i++)
                {
                    beatEvents.Add(new BeatEvent
                    {
                        event_ = Symbol.Read(reader),
                        beat = reader.ReadFloat()
                    });
                }
            }
            else
            {
                enterEvent = Symbol.Read(reader);
                exitEvent = Symbol.Read(reader);
                uint frameCount = reader.ReadUInt32();
                for (int i = 0; i < frameCount; i++)
                {
                    oldFrameEvents.Add(new FrameEvent
                    {
                        frame = reader.ReadFloat(),
                        script = Symbol.Read(reader)
                    });
                }
            }

            if (revision > 0xC)
            {
                full.Load(reader);
                one.Load(reader);
            }
            else
            {
                full.version = revision;
                full.LoadHeader(reader);

                one.version = reader.ReadInt32();
                one.LoadHeader(reader);
                one.LoadData(reader);

                oneInitialRawData = (byte[])one.rawData.Clone();
                oneInitialVer14Bool = one.ver14Bool;

                if (revision > 7)
                {
                    extraBonesSamples = new CharBonesSamples { version = one.version };
                    extraBonesSamples.LoadHeader(reader);
                }

                fullDataVersion = one.version;
                int savedVer = full.version;
                full.version = one.version;
                full.LoadData(reader);
                full.version = savedVer;

                one.LoadData(reader);
            }

            if (revision > 0xE)
            {
                uint zeroCount = reader.ReadUInt32();
                for (int i = 0; i < zeroCount; i++)
                {
                    zeros.Add(new CharBonesSamples.Bone
                    {
                        name = Symbol.Read(reader),
                        weight = reader.ReadFloat()
                    });
                }
            }

            if (revision > 0x11)
            {
                uint keyCount = reader.ReadUInt32();
                for (int i = 0; i < keyCount; i++)
                {
                    beatTrack.Add(new BeatTrackKey
                    {
                        frame = reader.ReadFloat(),
                        value = reader.ReadFloat()
                    });
                }
            }

            if (revision > 0x12)
                syncAnim = Symbol.Read(reader);

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw MiloLib.Exceptions.MiloAssetReadException.EndBytesNotFound(parent, entry, reader.BaseStream.Position);

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            if (revision < 0x10)
                writer.WriteInt32(oldRev);

            base.Write(writer, false, parent, entry);

            if (revision < 0x12)
            {
                writer.WriteInt32(oldStartBeat);
                writer.WriteInt32(oldEndBeat);
            }

            writer.WriteFloat(framesPerSec);
            writer.WriteInt32(flags);
            writer.WriteInt32(playFlags);

            if (oldRev < 0xD)
                writer.WriteInt32(blendWidth);

            if (oldRev > 3)
                writer.WriteFloat(range);

            if (oldRev > 5)
                Symbol.Write(writer, relative);
            else if (oldRev > 4)
                writer.WriteBoolean(relativeBool);

            if (oldRev == 9 || oldRev == 10)
                writer.WriteBoolean(unkBool);

            if (oldRev > 9)
                writer.WriteInt32(transitionVersion);

            if (oldRev > 0xB)
                writer.WriteBoolean(doNotCompress);

            WriteTransitions(writer);

            if (oldRev < 3)
            {
                writer.WriteUInt32((uint)oldStrings.Count);
                foreach (var sym in oldStrings)
                    Symbol.Write(writer, sym);
            }

            if (oldRev > 6)
            {
                writer.WriteUInt32((uint)beatEvents.Count);
                foreach (var evt in beatEvents)
                {
                    Symbol.Write(writer, evt.event_);
                    writer.WriteFloat(evt.beat);
                }
            }
            else
            {
                Symbol.Write(writer, enterEvent);
                Symbol.Write(writer, exitEvent);
                writer.WriteUInt32((uint)oldFrameEvents.Count);
                foreach (var fe in oldFrameEvents)
                {
                    writer.WriteFloat(fe.frame);
                    Symbol.Write(writer, fe.script);
                }
            }

            if (revision > 0xC)
            {
                full.Save(writer);
                one.Save(writer);
            }
            else
            {
                full.WriteHeader(writer);

                writer.WriteInt32(one.version);
                one.WriteHeader(writer);
                if (one.version == 14)
                    writer.WriteBoolean(oneInitialVer14Bool);
                if (oneInitialRawData.Length > 0)
                    writer.WriteBlock(oneInitialRawData);

                if (revision > 7)
                    extraBonesSamples.WriteHeader(writer);

                int savedVer = full.version;
                full.version = fullDataVersion;
                full.WriteData(writer);
                full.version = savedVer;

                one.WriteData(writer);
            }

            if (revision > 0xE)
            {
                writer.WriteUInt32((uint)zeros.Count);
                foreach (var bone in zeros)
                {
                    Symbol.Write(writer, bone.name);
                    writer.WriteFloat(bone.weight);
                }
            }

            if (revision > 0x11)
            {
                writer.WriteUInt32((uint)beatTrack.Count);
                foreach (var key in beatTrack)
                {
                    writer.WriteFloat(key.frame);
                    writer.WriteFloat(key.value);
                }
            }

            if (revision > 0x12)
                Symbol.Write(writer, syncAnim);

            if (standalone)
                writer.WriteEndBytes();
        }

        private void ReadTransitions(EndianReader reader)
        {
            if (oldRev < 8)
            {
                transitionsByteHint = -1;
                uint count = reader.ReadUInt32();
                for (int i = 0; i < count; i++)
                {
                    var nv = new NodeVector();
                    nv.clip = Symbol.Read(reader);
                    uint nodeCount = reader.ReadUInt32();
                    for (int j = 0; j < nodeCount; j++)
                    {
                        nv.nodes.Add(new GraphNode
                        {
                            curBeat = reader.ReadFloat(),
                            nextBeat = reader.ReadFloat()
                        });
                    }
                    transitions.Add(nv);
                }
            }
            else
            {
                transitionsByteHint = reader.ReadInt32();
                uint count = reader.ReadUInt32();
                for (int i = 0; i < count; i++)
                {
                    var nv = new NodeVector();
                    nv.clip = Symbol.Read(reader);
                    uint nodeCount = reader.ReadUInt32();
                    for (int j = 0; j < nodeCount; j++)
                    {
                        nv.nodes.Add(new GraphNode
                        {
                            curBeat = reader.ReadFloat(),
                            nextBeat = reader.ReadFloat()
                        });
                    }
                    transitions.Add(nv);
                }
            }
        }

        private void WriteTransitions(EndianWriter writer)
        {
            if (oldRev < 8)
            {
                writer.WriteUInt32((uint)transitions.Count);
                foreach (var nv in transitions)
                {
                    Symbol.Write(writer, nv.clip);
                    writer.WriteUInt32((uint)nv.nodes.Count);
                    foreach (var node in nv.nodes)
                    {
                        writer.WriteFloat(node.curBeat);
                        writer.WriteFloat(node.nextBeat);
                    }
                }
            }
            else
            {
                // i think this is right? seems so anyway
                // idk
                int byteHint = 0;
                foreach (var nv in transitions)
                    byteHint += 8 + 8 * nv.nodes.Count;

                writer.WriteInt32(transitionsByteHint >= 0 ? transitionsByteHint : byteHint);
                writer.WriteUInt32((uint)transitions.Count);
                foreach (var nv in transitions)
                {
                    Symbol.Write(writer, nv.clip);
                    writer.WriteUInt32((uint)nv.nodes.Count);
                    foreach (var node in nv.nodes)
                    {
                        writer.WriteFloat(node.curBeat);
                        writer.WriteFloat(node.nextBeat);
                    }
                }
            }
        }
    }
}
