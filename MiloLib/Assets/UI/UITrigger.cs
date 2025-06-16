using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets.UI
{
    [Name("UITrigger"), Description("Triggers anims based on UI events (enter, exit, etc.)")]
    public class UITrigger : EventTrigger
    {
        private ushort altRevision;
        private ushort revision;

        public UIComponent component = new();

        public Symbol unkSym = new(0, "");

        public Symbol animRef = new(0, "");

        [Name("Block Transition"), Description("Block enter/exit transition during animation?")]
        public bool blockTransition;

        public UITrigger Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            if (revision < 1)
            {
                component = component.Read(reader, false, parent, entry);

                unkSym = Symbol.Read(reader);

                animRef = Symbol.Read(reader);
            }

            base.Read(reader, false, parent, entry);
            blockTransition = reader.ReadBoolean();

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw new Exception("Got to end of standalone asset but didn't find the expected end bytes, read likely did not succeed");

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            if (revision < 1)
            {
                component.Write(writer, false, parent, entry);

                Symbol.Write(writer, unkSym);

                Symbol.Write(writer, animRef);
            }

            base.Write(writer, false, parent, entry);
            writer.WriteBoolean(blockTransition);

            if (standalone)
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
        }

    }
}
