using MiloLib.Utils;
using MiloLib.Classes;
using MiloLib.Assets.UI;

namespace MiloLib.Assets
{
    [Name("TrackDir"), Description("Base class for track system. Contains configuration for track speed, length, slot positions.  Manages TrackWidget instances.")]
    public class TrackDir : PanelDir
    {
        private ushort altRevision;
        private ushort revision;

        public Symbol drawGroup = new(0, "");
        public Symbol animGroup = new(0, "");

        public float YperSecond;
        public float topY;
        public float bottomY;

        private uint slotsCount;
        public List<Matrix> slots = new();

        public bool warnOnResort;

        public Symbol testWidget = new(0, "");
        public int testSlot;

        public int oldSlotsNum;

        public TrackDir(ushort revision, ushort altRevision = 0) : base(revision, altRevision)
        {
            revision = revision;
            altRevision = altRevision;
            return;
        }

        public TrackDir Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            base.Read(reader, false, parent, entry);

            if (!entry.isProxy)
            {
                if (revision != 0)
                {
                    drawGroup = Symbol.Read(reader);
                    if (revision > 1)
                        animGroup = Symbol.Read(reader);
                    YperSecond = reader.ReadFloat();
                    topY = reader.ReadFloat();
                    bottomY = reader.ReadFloat();
                }
                if (revision > 2)
                {
                    if (revision > 5)
                    {
                        slotsCount = reader.ReadUInt32();
                        for (int i = 0; i < slotsCount; i++)
                        {
                            slots.Add(new Matrix().Read(reader));
                        }
                    }
                    else
                    {
                        oldSlotsNum = reader.ReadInt32();
                        slots.Add(new Matrix());
                        for (int i = 0; i < oldSlotsNum; i++)
                        {
                            // i think this is right?
                            slots[i].m41 = reader.ReadFloat();
                            slots[i].m43 = reader.ReadFloat();
                        }
                    }
                }
                if (revision > 4)
                    warnOnResort = reader.ReadBoolean();

                if (revision > 3)
                {
                    testWidget = Symbol.Read(reader);
                    testSlot = reader.ReadInt32();
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

            if (!entry.isProxy)
            {
                if (revision != 0)
                {
                    Symbol.Write(writer, drawGroup);
                    if (revision > 1)
                        Symbol.Write(writer, animGroup);
                    writer.WriteFloat(YperSecond);
                    writer.WriteFloat(topY);
                    writer.WriteFloat(bottomY);
                }
                if (revision > 2)
                {
                    if (revision > 5)
                    {
                        writer.WriteUInt32((uint)slots.Count);
                        for (int i = 0; i < slots.Count; i++)
                        {
                            slots[i].Write(writer);
                        }
                    }
                    else
                    {
                        writer.WriteInt32(oldSlotsNum);
                        for (int i = 0; i < oldSlotsNum; i++)
                        {
                            writer.WriteFloat(slots[i].m41);
                            writer.WriteFloat(slots[i].m43);
                        }
                    }
                }
                if (revision > 4)
                    writer.WriteBoolean(warnOnResort);

                if (revision > 3)
                {
                    Symbol.Write(writer, testWidget);
                    writer.WriteInt32(testSlot);
                }
            }

            if (standalone)
                writer.WriteEndBytes();
        }

        public override bool IsDirectory()
        {
            return true;
        }

    }
}
