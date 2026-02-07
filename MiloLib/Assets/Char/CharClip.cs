using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets.Char
{
    [Name("CharClip"), Description("")]
    public class CharClip : Object
    {
        public class ClipNodeFloats
        {
            public float unk1;
            public float unk2;

            public ClipNodeFloats Read(EndianReader reader)
            {
                unk1 = reader.ReadFloat();
                unk2 = reader.ReadFloat();
                return this;
            }

            public void Write(EndianWriter writer)
            {
                writer.WriteFloat(unk1);
                writer.WriteFloat(unk2);
            }
        }

        public class ClipNode
        {
            public Symbol name = new(0, "");
            private uint floatsCount;
            public List<ClipNodeFloats> cnfNodes = new List<ClipNodeFloats>();

            public ClipNode Read(EndianReader reader)
            {
                name = Symbol.Read(reader);
                floatsCount = reader.ReadUInt32();
                for (int i = 0; i < floatsCount; i++)
                {
                    cnfNodes.Add(new ClipNodeFloats().Read(reader));
                }
                return this;
            }

            public void Write(EndianWriter writer)
            {
                Symbol.Write(writer, name);
                writer.WriteUInt32((uint)cnfNodes.Count);
                foreach (var cnfNode in cnfNodes)
                {
                    cnfNode.Write(writer);
                }
            }
        }

        public class ClipEvent
        {
            public Symbol name = new(0, "");
            private uint vectorCount;
            public List<Vector2> vector = new();

            public ClipEvent Read(EndianReader reader)
            {
                name = Symbol.Read(reader);
                vectorCount = reader.ReadUInt32();
                for (int i = 0; i < vectorCount; i++)
                {
                    vector.Add(new Vector2(reader.ReadFloat(), reader.ReadFloat()));
                }
                return this;
            }

            public void Write(EndianWriter writer)
            {
                Symbol.Write(writer, name);
                writer.WriteUInt32((uint)vector.Count);
                foreach (var vec in vector)
                {
                    vec.Write(writer);
                }
            }
        }
        public class FrameEvent
        {
            public float frame;
            public Symbol script = new(0, "");

            public FrameEvent Read(EndianReader reader)
            {
                frame = reader.ReadFloat();
                script = Symbol.Read(reader);
                return this;
            }

            public void Write(EndianWriter writer)
            {
                writer.WriteFloat(frame);
                Symbol.Write(writer, script);
            }
        }
        public enum PlayTimeFlags
        {
            kPlayBeatTime = 0,
            kPlayRealTime = 512,
            kPlayBeatAlign1 = 4096,
            kPlayBeatAlign2 = 8192,
            kPlayBeatAlign4 = 16384,
            kPlayBeatAlign8 = 32768
        }

        public enum PlayLoopFlags
        {
            // kPlayNoDefault = 0,
            kPlayNoLoop = 16,
            kPlayLoop = 32,
            kPlayGraphLoop = 48,
            kPlayNodeLoop = 64,
        }

        public enum PlayBlendFlags
        {
            kPlayNoDefault = 0,
            kPlayNow = 1,
            kPlayNoBlend = 2,
            kPlayFirst = 3,
            kPlayLast = 4,
            kPlayDirty = 8
        }
        private ushort altRevision;
        private ushort revision;

        public float startBeat;
        public float endBeat;

        public float beatsPerSecond;

        public byte[] unkBytes19 = Array.Empty<byte>();

        private uint transitionsCount;
        public List<Symbol> transitions = new();

        private uint unkSymsCount;
        public List<Symbol> unkSyms = new();

        public PlayBlendFlags flags;
        public PlayLoopFlags playFlags;

        public float blendWidth;

        public float range;

        public bool unkBool1;

        public Symbol relative = new(0, "");

        public bool unkBool2;

        public int unkInt;

        public bool doNotDecompress;

        private uint nodeCount;

        public Symbol enterEvent = new(0, "");
        public Symbol exitEvent = new(0, "");

        uint nodesSize;
        private uint clipNodeCount;
        public List<ClipNode> clipNode = new();

        private uint frameNodeCount;
        public List<FrameEvent> frameEvents = new();


        public CharClip Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            base.Read(reader, false, parent, entry);

            startBeat = reader.ReadFloat();
            endBeat = reader.ReadFloat();

            beatsPerSecond = reader.ReadFloat();

            if (revision >= 19)
            {
                unkBytes19 = reader.ReadBlock(17);

                transitionsCount = reader.ReadUInt32();
                for (int i = 0; i < transitionsCount; i++)
                {
                    transitions.Add(Symbol.Read(reader));
                }

                // charbonessamples
            }

            flags = (PlayBlendFlags)reader.ReadUInt32();
            playFlags = (PlayLoopFlags)reader.ReadUInt32();

            blendWidth = reader.ReadFloat();

            if (revision > 3)
                range = reader.ReadFloat();

            if (revision == 5)
            {
                unkBool1 = reader.ReadBoolean();
            }
            else if (revision > 5)
            {
                relative = Symbol.Read(reader);
            }

            if ((revision - 9) < 2)
            {
                unkBool2 = reader.ReadBoolean();
            }

            if (revision > 9)
            {
                unkInt = reader.ReadInt32();
            }

            if (revision > 11)
            {
                doNotDecompress = reader.ReadBoolean();
            }

            if (revision < 8)
            {
                nodeCount = reader.ReadUInt32();
                for (int i = 0; i < nodeCount; i++)
                {
                    clipNode.Add(new ClipNode().Read(reader));
                }
            }
            else
            {
                nodesSize = reader.ReadUInt32();
                nodeCount = reader.ReadUInt32();
                for (int i = 0; i < nodesSize; i++)
                {
                    clipNode.Add(new ClipNode().Read(reader));
                }
            }

            if (revision < 3)
            {
                unkSymsCount = reader.ReadUInt32();
                for (int i = 0; i < unkSymsCount; i++)
                {
                    unkSyms.Add(Symbol.Read(reader));
                }
            }

            if (revision < 7)
            {
                enterEvent = Symbol.Read(reader);
                exitEvent = Symbol.Read(reader);

                frameNodeCount = reader.ReadUInt32();
                for (int i = 0; i < frameNodeCount; i++)
                {
                    frameEvents.Add(new FrameEvent().Read(reader));
                }
            }
            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw MiloLib.Exceptions.MiloAssetReadException.EndBytesNotFound(parent, entry, reader.BaseStream.Position);

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            base.Write(writer, false, parent, entry);

            writer.WriteFloat(startBeat);
            writer.WriteFloat(endBeat);

            writer.WriteFloat(beatsPerSecond);

            if (revision >= 19)
            {
                writer.WriteBlock(unkBytes19);

                writer.WriteUInt32((uint)transitions.Count);
                foreach (var transition in transitions)
                {
                    Symbol.Write(writer, transition);
                }
            }

            writer.WriteUInt32((uint)flags);
            writer.WriteUInt32((uint)playFlags);

            writer.WriteFloat(blendWidth);

            if (revision > 3)
                writer.WriteFloat(range);

            if (revision == 5)
            {
                writer.WriteBoolean(unkBool1);
            }
            else if (revision > 5)
            {
                Symbol.Write(writer, relative);
            }

            if ((revision - 9) < 2)
            {
                writer.WriteBoolean(unkBool2);
            }

            if (revision > 9)
            {
                writer.WriteInt32(unkInt);
            }

            if (revision > 11)
            {
                writer.WriteBoolean(doNotDecompress);
            }

            if (revision < 8)
            {
                writer.WriteUInt32((uint)clipNode.Count);
                foreach (var node in clipNode)
                {
                    node.Write(writer);
                }
            }
            else
            {
                writer.WriteUInt32(nodesSize);
                writer.WriteUInt32(nodeCount);
                foreach (var node in clipNode)
                {
                    node.Write(writer);
                }
            }

            if (revision < 3)
            {
                writer.WriteUInt32((uint)unkSyms.Count);
                foreach (var sym in unkSyms)
                {
                    Symbol.Write(writer, sym);
                }
            }

            if (revision < 7)
            {
                Symbol.Write(writer, enterEvent);
                Symbol.Write(writer, exitEvent);

                writer.WriteUInt32((uint)frameEvents.Count);
                foreach (var frameEvent in frameEvents)
                {
                    frameEvent.Write(writer);
                }
            }

            if (standalone)
                writer.WriteEndBytes();
        }

    }
}
