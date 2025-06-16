using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets.Synth
{
    [Name("RandomGroupSeq"), Description("Plays one or more of its child sequences, selected at random.")]
    public class RandomGroupSeq : GroupSeq
    {
        private ushort altRevision;
        private ushort revision;

        [Name("Number of Simultaneous Sequences"), Description("Number of children to play simultaneously")]
        public uint numSimultaneous;
        [Name("Allow Repeats"), Description("If false, you will never hear the same sequence again until all have played (only if num_simul is 1)"), MinVersion(2)]
        public bool allowRepeats;

        public RandomGroupSeq Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            base.Read(reader, false, parent, entry);

            numSimultaneous = reader.ReadUInt32();
            if (revision >= 2)
                allowRepeats = reader.ReadBoolean();

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw new Exception("Got to end of standalone asset but didn't find the expected end bytes, read likely did not succeed");

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            base.Write(writer, false, parent, entry);

            writer.WriteUInt32(numSimultaneous);
            if (revision >= 2)
                writer.WriteBoolean(allowRepeats);

            if (standalone)
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
        }

    }
}
