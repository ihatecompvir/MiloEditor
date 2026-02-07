using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets.UI
{
    [Name("UIPicture"), Description("")]
    public class UIPicture : UIComponent
    {
        private ushort altRevision;
        private ushort revision;

        [Name("Texture File"), Description("Path of texture to load"), MinVersion(1)]
        public Symbol mTexFile = new(0, "");
        [Name("Mesh"), Description("Mesh to show loaded tex on (should have Mat)"), MinVersion(1)]
        public Symbol mMesh = new(0, "");


        [Name("In Animation"), Description("animation kicked off before texture change"), MinVersion(2)]
        public Symbol inAnim = new(0, "");
        [Name("Out Animation"), Description("animation kicked off after texture change"), MinVersion(2)]
        public Symbol outAnim = new(0, "");

        public UIPicture Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            if (revision != 0)
            {
                mTexFile = Symbol.Read(reader);
                mMesh = Symbol.Read(reader);
            }

            if (revision >= 2)
            {
                inAnim = Symbol.Read(reader);
                outAnim = Symbol.Read(reader);
            }

            base.Read(reader, false, parent, entry);

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw MiloLib.Exceptions.MiloAssetReadException.EndBytesNotFound(parent, entry, reader.BaseStream.Position);

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            if (revision != 0)
            {
                Symbol.Write(writer, mTexFile);
                Symbol.Write(writer, mMesh);
            }

            if (revision >= 2)
            {
                Symbol.Write(writer, inAnim);
                Symbol.Write(writer, outAnim);
            }

            base.Write(writer, false, parent, entry);

            if (standalone)
                writer.WriteEndBytes();
        }

    }
}
