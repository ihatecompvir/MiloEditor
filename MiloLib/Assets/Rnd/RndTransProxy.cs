using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets.Rnd
{
    [Name("RndTransProxy"), Description("Stand-in for a RndTransformable inside of a proxy, so you can use it")]
    public class RndTransProxy : Object
    {
        private ushort altRevision;
        private ushort revision;

        public RndTrans trans = new();
        [Name("Proxy"), Description("Proxy object this will look into.")]
        public Symbol proxy = new(0, "");
        [Name("Part"), Description("The part inside it")]
        public Symbol part = new(0, "");

        public RndTransProxy Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            base.Read(reader, false, parent, entry);

            if (revision != 0)
                trans = trans.Read(reader, false, parent, entry);

            proxy = Symbol.Read(reader);
            part = Symbol.Read(reader);

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw new Exception("Got to end of standalone asset but didn't find the expected end bytes, read likely did not succeed");

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            base.Write(writer, false, parent, entry);

            if (revision != 0)
                trans.Write(writer, false, parent, entry);

            Symbol.Write(writer, proxy);
            Symbol.Write(writer, part);

            if (standalone)
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
        }

    }
}
