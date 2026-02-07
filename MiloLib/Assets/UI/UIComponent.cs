using MiloLib.Assets.Rnd;
using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets.UI
{
    [Name("UIComponent"), Description("Base class of all UI components, defines navigation and component state")]
    public class UIComponent : Object
    {
        private ushort altRevision;
        private ushort revision;

        public RndTrans trans = new();
        public RndDrawable draw = new();

        [Name("Nav Right"), Description("Object to navigate to when the right button is pressed"), MinVersion(1)]
        public Symbol navRight = new(0, "");
        [Name("Nav Down"), Description("Object to navigate to when the down button is pressed"), MinVersion(1)]
        public Symbol navDown = new(0, "");
        [Name("Resource Name"), Description("path to resource file for this component"), MinVersion(2)]
        public Symbol resourceName = new(0, "");


        public UIComponent Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            base.Read(reader, false, parent, entry);

            trans = trans.Read(reader, false, parent, entry);
            draw = draw.Read(reader, false, parent, entry);

            if (revision > 0)
            {
                navRight = Symbol.Read(reader);
                navDown = Symbol.Read(reader);
            }

            if (revision > 1)
                resourceName = Symbol.Read(reader);


            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw MiloLib.Exceptions.MiloAssetReadException.EndBytesNotFound(parent, entry, reader.BaseStream.Position);

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            base.Write(writer, false, parent, entry);

            trans.Write(writer, false, parent, true);
            draw.Write(writer, false, parent, true);

            if (revision > 0)
            {
                Symbol.Write(writer, navRight);
                Symbol.Write(writer, navDown);
            }

            if (revision > 1)
                Symbol.Write(writer, resourceName);

            if (standalone)
                writer.WriteEndBytes();
        }

    }
}
