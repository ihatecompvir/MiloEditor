using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets.Char
{
    [Name("CharUpperTwist"), Description("Does all interpolation for the upperarm, assuming upperArm, upperTwist1 and 2 are under clavicle. Rotation about x is evenly distributed from clavicle->twist1->twist2->upperarm. Feeds the bones when executed.")]
    public class CharUpperTwist : Object
    {
        private ushort altRevision;
        private ushort revision;

        public Symbol upperArm = new(0, "");
        public Symbol twist = new(0, "");
        public Symbol twist2 = new(0, "");

        public CharUpperTwist Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            base.Read(reader, false, parent, entry);
            twist2 = Symbol.Read(reader);
            upperArm = Symbol.Read(reader);
            twist = Symbol.Read(reader);

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw MiloLib.Exceptions.MiloAssetReadException.EndBytesNotFound(parent, entry, reader.BaseStream.Position);

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            base.Write(writer, false, parent, entry);
            Symbol.Write(writer, twist2);
            Symbol.Write(writer, upperArm);
            Symbol.Write(writer, twist);

            if (standalone)
                writer.WriteEndBytes();
        }

    }
}
