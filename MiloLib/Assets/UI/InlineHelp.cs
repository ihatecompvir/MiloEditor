using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets.UI
{
    [Name("InlineHelp"), Description("Inline Help")]
    public class InlineHelp : UIComponent
    {
        public class ActionElement
        {
            public int joypadAction;
            public Symbol symbol = new(0, "");
            public Symbol symbol2 = new(0, "");

            public ActionElement Read(EndianReader reader, uint revision)
            {
                joypadAction = reader.ReadInt32();
                symbol = Symbol.Read(reader);
                if (revision >= 2)
                    symbol2 = Symbol.Read(reader);

                return this;
            }

            public void Write(EndianWriter writer, uint revision)
            {
                writer.WriteInt32(joypadAction);
                Symbol.Write(writer, symbol);
                if (revision >= 2)
                    Symbol.Write(writer, symbol2);
            }

            public override string ToString()
            {
                return $"{joypadAction} {symbol} {symbol2}";
            }
        }
        private ushort altRevision;
        private ushort revision;

        public bool horizontal;
        public float spacing;

        public int unkInt;

        private uint elementsCount;
        public List<ActionElement> elements = new();

        [Name("Text Color"), Description("UIColor of text")]
        public Symbol textColorObject = new Symbol(0, "");

        [Name("Use Connected Controllers"), Description("Use all connected controllers for button icon, rather than just joined (rare)")]
        public bool useConnectedControllers;

        public InlineHelp Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            horizontal = reader.ReadBoolean();
            spacing = reader.ReadFloat();

            elementsCount = reader.ReadUInt32();
            for (int i = 0; i < elementsCount; i++)
            {
                elements.Add(new ActionElement().Read(reader, revision));
            }

            if (revision >= 1)
                textColorObject = Symbol.Read(reader);

            // ?
            if ((short)(revision + 0xFFFE) <= 1)
            {
                unkInt = reader.ReadInt32();
            }

            if (revision >= 3)
                useConnectedControllers = reader.ReadBoolean();

            base.Read(reader, false, parent, entry);


            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw MiloLib.Exceptions.MiloAssetReadException.EndBytesNotFound(parent, entry, reader.BaseStream.Position);

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            writer.WriteBoolean(horizontal);
            writer.WriteFloat(spacing);

            writer.WriteUInt32(elementsCount);
            foreach (var element in elements)
            {
                element.Write(writer, revision);
            }

            if (revision >= 1)
                Symbol.Write(writer, textColorObject);

            // ?
            if ((short)(revision + 0xFFFE) <= 1)
            {
                writer.WriteInt32(unkInt);
            }

            if (revision >= 3)
                writer.WriteBoolean(useConnectedControllers);

            base.Write(writer, false, parent, entry);

            if (standalone)
                writer.WriteEndBytes();
        }

    }
}
