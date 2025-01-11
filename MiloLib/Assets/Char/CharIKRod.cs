using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets.Char
{
    [Name("CharIKRod"), Description("Rigs a bone between two other bones and sets the orientation from that. When you set up all the bone pointers, the rig xfm will be computed, an inverse from that to the dst bone will be computed, and everything will come from that. So the dst bone will maintain the exact same position in that pose. That makes it easy to author the bones.")]
    public class CharIKRod : Object
    {
        private ushort altRevision;
        private ushort revision;

        public Symbol leftEnd = new(0, "");
        public Symbol rightEnd = new(0, "");
        public float destPos;
        public Symbol sideAxis = new(0, "");
        public bool vertical;
        public Symbol dest = new(0, "");
        public Matrix xfm = new();


        public CharIKRod Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            base.Read(reader, false, parent, entry);

            leftEnd = Symbol.Read(reader);
            rightEnd = Symbol.Read(reader);
            destPos = reader.ReadFloat();
            sideAxis = Symbol.Read(reader);
            vertical = reader.ReadBoolean();
            dest = Symbol.Read(reader);
            xfm.Read(reader);

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw new Exception("Got to end of standalone asset but didn't find the expected end bytes, read likely did not succeed");

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            base.Write(writer, false, parent, entry);

            Symbol.Write(writer, leftEnd);
            Symbol.Write(writer, rightEnd);
            writer.WriteFloat(destPos);
            Symbol.Write(writer, sideAxis);
            writer.WriteBoolean(vertical);
            Symbol.Write(writer, dest);
            xfm.Write(writer);

            if (standalone)
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
        }

    }
}
