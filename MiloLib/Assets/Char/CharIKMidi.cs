using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets.Char
{
    [Name("CharIKMidi"), Description("Moves an RndTransformable (bone) to another RndTransformable (spot) over time, blending from where it was relative to the parent of the spot.")]
    public class CharIKMidi : Object
    {
        private ushort altRevision;
        private ushort revision;

        [Name("Bone"), Description("The bone to move")]
        public Symbol bone = new(0, "");

        private uint unkSymbolCount;
        public List<Symbol> unkSymbols = new();

        [Name("Current Spot"), Description("Spot to go to, zero indexed")]
        public Symbol currentSpot = new(0, "");

        [Name("Anim Blend Weightable"), Description("Weightable to change animation between frets")]
        public Symbol animBlender = new(0, "");
        [Name("Max Anim Blend"), Description("Max weight for animation change")]
        public float maxAnimBlend;

        public CharIKMidi Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            base.Read(reader, false, parent, entry);

            bone = Symbol.Read(reader);

            if (revision < 3)
            {
                unkSymbolCount = reader.ReadUInt32();
                for (int i = 0; i < unkSymbolCount; i++)
                    unkSymbols.Add(Symbol.Read(reader));
            }
            if (revision == 2 || revision == 3)
            {
                currentSpot = Symbol.Read(reader);
            }
            if (revision > 4)
            {
                animBlender = Symbol.Read(reader);
                maxAnimBlend = reader.ReadFloat();
            }

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw new Exception("Got to end of standalone asset but didn't find the expected end bytes, read likely did not succeed");

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            base.Write(writer, false, parent, entry);

            Symbol.Write(writer, bone);

            if (revision < 3)
            {
                writer.WriteUInt32((uint)unkSymbols.Count);
                foreach (Symbol symbol in unkSymbols)
                    Symbol.Write(writer, symbol);
            }

            if (revision == 2 || revision == 3)
                Symbol.Write(writer, currentSpot);

            if (revision > 4)
            {
                Symbol.Write(writer, animBlender);
                writer.WriteFloat(maxAnimBlend);
            }

            if (standalone)
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
        }

    }
}
