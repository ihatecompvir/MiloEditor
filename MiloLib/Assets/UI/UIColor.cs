using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets.UI
{
    [Name("UIColor"), Description("Just a color, used by UI components")]
    public class UIColor : Object
    {
        public ushort altRevision;
        public ushort revision;

        public float r;
        public float g;
        public float b;
        public float a;

        public UIColor Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            if (revision != 0)
            {
                throw new UnsupportedAssetRevisionException("UIColor", revision);
            }

            base.objFields.Read(reader, parent, entry);

            r = reader.ReadFloat();
            g = reader.ReadFloat();
            b = reader.ReadFloat();
            a = reader.ReadFloat();

            if (standalone)
            {
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw new Exception("Got to end of standalone asset but didn't find the expected end bytes, read likely did not succeed");
            }

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            base.objFields.Write(writer);

            writer.WriteFloat(r);
            writer.WriteFloat(g);
            writer.WriteFloat(b);
            writer.WriteFloat(a);

            if (standalone)
            {
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
            }
        }
    }
}
