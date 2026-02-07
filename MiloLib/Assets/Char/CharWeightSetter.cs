using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets.Char
{
    [Name("CharWeightSetter"), Description("Sets its own weight by pushing flags through a driver to see what fraction of them it has.")]
    public class CharWeightSetter : Object
    {
        private ushort altRevision;
        private ushort revision;

        public CharWeightable weightable = new();

        public Symbol driver = new(0, "");
        public Symbol baseObj = new(0, "");

        private uint maxWeightCount;
        public List<Symbol> maxWeights = new();
        private uint minWeightCount;
        public List<Symbol> minWeights = new();

        public int flags;
        public float offset;
        public float scale;
        public float baseWeight;
        public float beatsPerWeight;

        private uint weightablesCount;
        public List<Symbol> weightables = new();

        public bool unkBool;


        public CharWeightSetter Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            base.Read(reader, false, parent, entry);

            if (revision > 1)
                weightable = weightable.Read(reader, false, parent, entry);

            driver = Symbol.Read(reader);
            flags = reader.ReadInt32();

            if (revision < 3)
            {

            }
            else if (revision < 4)
            {
                unkBool = reader.ReadBoolean();
            }
            else
            {
                offset = reader.ReadFloat();
                scale = reader.ReadFloat();
            }

            if (revision < 2)
            {
                weightablesCount = reader.ReadUInt32();
                for (int i = 0; i < weightablesCount; i++)
                {
                    weightables.Add(Symbol.Read(reader));
                }
            }
            if (revision > 4)
            {
                baseWeight = reader.ReadFloat();
                beatsPerWeight = reader.ReadFloat();
            }
            if (revision > 5)
            {
                baseObj = Symbol.Read(reader);
            }
            if (revision > 8)
            {
                minWeightCount = reader.ReadUInt32();
                for (int i = 0; i < minWeightCount; i++)
                {
                    minWeights.Add(Symbol.Read(reader));
                }
                maxWeightCount = reader.ReadUInt32();
                for (int i = 0; i < maxWeightCount; i++)
                {
                    maxWeights.Add(Symbol.Read(reader));
                }
            }
            else
            {
                if (revision > 6)
                    minWeights.Add(Symbol.Read(reader));
                if (revision > 7)
                    maxWeights.Add(Symbol.Read(reader));
            }

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw MiloLib.Exceptions.MiloAssetReadException.EndBytesNotFound(parent, entry, reader.BaseStream.Position);

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            base.Write(writer, false, parent, entry);

            if (revision > 1)
                weightable.Write(writer, false, parent, entry);

            Symbol.Write(writer, driver);
            writer.WriteInt32(flags);


            if (revision < 3)
            {

            }
            else if (revision < 4)
            {
                writer.WriteBoolean(unkBool);
            }
            else
            {
                writer.WriteFloat(offset);
                writer.WriteFloat(scale);
            }

            if (revision < 2)
            {
                weightablesCount = (uint)weightables.Count;
                writer.WriteUInt32(weightablesCount);
                for (int i = 0; i < weightablesCount; i++)
                {
                    Symbol.Write(writer, weightables[i]);
                }
            }

            if (revision > 4)
            {
                writer.WriteFloat(baseWeight);
                writer.WriteFloat(beatsPerWeight);
            }

            if (revision > 5)
            {
                Symbol.Write(writer, baseObj);
            }

            if (revision > 8)
            {
                minWeightCount = (uint)minWeights.Count;
                maxWeightCount = (uint)maxWeights.Count;
                writer.WriteUInt32(minWeightCount);
                for (int i = 0; i < minWeightCount; i++)
                {
                    Symbol.Write(writer, minWeights[i]);
                }
                writer.WriteUInt32(maxWeightCount);
                for (int i = 0; i < maxWeightCount; i++)
                {
                    Symbol.Write(writer, maxWeights[i]);
                }
            }
            else
            {
                if (revision > 6 && minWeights.Count > 0)
                    Symbol.Write(writer, minWeights[0]);
                if (revision > 7 && maxWeights.Count > 0)
                    Symbol.Write(writer, maxWeights[0]);
            }

            if (standalone)
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
        }

    }
}
