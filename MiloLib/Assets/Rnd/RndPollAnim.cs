using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets.Rnd
{
    [Name("RndPollAnim"), Description("Class that drives Anims with time based on their rate.")]
    public class RndPollAnim : Object
    {
        private ushort altRevision;
        private ushort revision;

        public RndAnimatable anim = new();
        public Object poll = new();

        private uint animsCount;
        [Name("Anims"), Description("List of anims that will have SetFrame called on them according to their rate and the TheTaskMgr.Seconds or Beat")]
        public List<Symbol> anims = new();

        public RndPollAnim Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            base.Read(reader, false, parent, entry);

            anim = anim.Read(reader, parent, entry);
            poll = poll.Read(reader, false, parent, entry);

            animsCount = reader.ReadUInt32();
            for (int i = 0; i < animsCount; i++)
            {
                anims.Add(Symbol.Read(reader));
            }

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw new Exception("Got to end of standalone asset but didn't find the expected end bytes, read likely did not succeed");

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            base.Write(writer, false, parent, entry);

            anim.Write(writer);
            poll.Write(writer, false, parent, entry);

            writer.WriteUInt32((uint)anims.Count);
            foreach (Symbol anim in anims)
            {
                Symbol.Write(writer, anim);
            }

            if (standalone)
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
        }

    }
}
