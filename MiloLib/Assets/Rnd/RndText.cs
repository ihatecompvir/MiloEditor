using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets.Rnd
{
    [Name("RndText"), Description("A Text object draws a 3D character string.")]
    public class RndText : Object
    {

        public enum Alignment
        {
            kTopLeft = 0x11,
            kTopCenter = 0x12,
            kTopRight = 0x14,
            kMiddleLeft = 0x21,
            kMiddleCenter = 0x22,
            kMiddleRight = 0x24,
            kBottomLeft = 0x41,
            kBottomCenter = 0x42,
            kBottomRight = 0x44
        };

        public enum CapsMode
        {
            kCapsModeNone = 0,
            kForceLower = 1,
            kForceUpper = 2
        };

        private ushort altRevision;
        private ushort revision;

        public RndDrawable draw = new();
        public RndTrans trans = new();

        [Name("Font"), Description("Font to use for this Text")]
        public Symbol font = new(0, "");

        [Name("Alignment"), Description("Alignment option for the text")]
        public int align = 0x11;

        [Name("Text"), Description("Text value")]
        public Symbol text = new(0, "");

        [Name("Color"), Description("Color of the text object"), MinVersion(1)]
        public HmxColor4 color = new();

        [Name("Wrap Width"), Description("Width of text until it wraps"), MinVersion(4)]
        public float wrapWidth;

        [Name("Leading"), Description("Vertical distance between lines"), MinVersion(8)]
        public float leading = 1.0f;

        [Name("Fixed Length"), Description("Number of character maximum for the text"), MinVersion(12)]
        public int fixedLength;

        [Name("Italics"), Description("Defines the slant of the text"), MinVersion(10)]
        public float italics;

        [Name("Size"), Description("Size of the text"), MinVersion(13)]
        public float size = 1.0f;

        [Name("Markup"), Description("This text uses markup"), MinVersion(14)]
        public bool textMarkup;

        [Name("Caps Mode"), Description("Defines the CAPS mode for the text"), MinVersion(15)]
        public int capsMode;

        [MaxVersion(6)]
        private int oldObjPtrListDump;
        [MaxVersion(6)]
        private List<Symbol> oldObjPtrList = new();
        [MaxVersion(1)]
        private Vector2 oldPos = new();
        [MinVersion(4), MaxVersion(12)]
        private bool oldWrapEnabled;
        [MinVersion(5), MaxVersion(5)]
        private Symbol oldString = new(0, "");
        [MinVersion(5), MaxVersion(10)]
        private bool oldZModeBool;
        [MinVersion(9), MaxVersion(11)]
        private bool oldFixedLengthBool;
        [MinVersion(18), MaxVersion(20)]
        private bool oldUnknownBool;
        [MinVersion(19), MaxVersion(20)]
        private int oldUnknownInt1;
        [MinVersion(19), MaxVersion(20)]
        private int oldUnknownInt2;
        [MinVersion(19), MaxVersion(20)]
        private int oldUnknownInt3;

        public RndText Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            if (revision > 15)
                base.Read(reader, false, parent, entry);

            draw = new RndDrawable().Read(reader, false, parent, entry);

            if (revision < 7)
            {
                oldObjPtrListDump = reader.ReadInt32();
                uint count = reader.ReadUInt32();
                oldObjPtrList = new List<Symbol>();
                for (int i = 0; i < count; i++)
                {
                    oldObjPtrList.Add(Symbol.Read(reader));
                }
            }

            if (revision > 1)
                trans = new RndTrans().Read(reader, false, parent, entry);

            font = Symbol.Read(reader);
            align = reader.ReadInt32();

            if (revision < 2)
                oldPos = new Vector2().Read(reader);

            text = Symbol.Read(reader);

            if (revision != 0)
                color = new HmxColor4().Read(reader);

            if (revision > 0xC)
            {
                wrapWidth = reader.ReadFloat();
            }
            else if (revision > 3)
            {
                oldWrapEnabled = reader.ReadBoolean();
                wrapWidth = reader.ReadFloat();
            }

            if (revision == 5)
                oldString = Symbol.Read(reader);

            if (revision >= 5 && revision <= 10)
                oldZModeBool = reader.ReadBoolean();

            if (revision > 7)
                leading = reader.ReadFloat();

            if (revision > 0xB)
            {
                fixedLength = reader.ReadInt32();
            }
            else if (revision > 8)
            {
                oldFixedLengthBool = reader.ReadBoolean();
            }

            if (revision > 9)
                italics = reader.ReadFloat();

            if (revision > 0xC)
                size = reader.ReadFloat();

            if (revision > 0xD)
                textMarkup = reader.ReadBoolean();

            if (revision > 0xE)
                capsMode = reader.ReadInt32();

            if (revision >= 0x12 && revision <= 0x14)
                oldUnknownBool = reader.ReadBoolean();

            if (revision == 0x13 || revision == 0x14)
            {
                oldUnknownInt1 = reader.ReadInt32();
                oldUnknownInt2 = reader.ReadInt32();
                oldUnknownInt3 = reader.ReadInt32();
            }

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw MiloLib.Exceptions.MiloAssetReadException.EndBytesNotFound(parent, entry, reader.BaseStream.Position);

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            if (revision > 15)
                base.Write(writer, false, parent, entry);

            draw.Write(writer, false, parent, true);

            if (revision < 7)
            {
                writer.WriteInt32(oldObjPtrListDump);
                writer.WriteUInt32((uint)oldObjPtrList.Count);
                foreach (var sym in oldObjPtrList)
                {
                    Symbol.Write(writer, sym);
                }
            }

            if (revision > 1)
                trans.Write(writer, false, parent, true);

            Symbol.Write(writer, font);
            writer.WriteInt32(align);

            if (revision < 2)
                oldPos.Write(writer);

            Symbol.Write(writer, text);

            if (revision != 0)
                color.Write(writer);

            if (revision > 0xC)
            {
                writer.WriteFloat(wrapWidth);
            }
            else if (revision > 3)
            {
                writer.WriteBoolean(oldWrapEnabled);
                writer.WriteFloat(wrapWidth);
            }

            if (revision == 5)
                Symbol.Write(writer, oldString);

            if (revision >= 5 && revision <= 10)
                writer.WriteBoolean(oldZModeBool);

            if (revision > 7)
                writer.WriteFloat(leading);

            if (revision > 0xB)
            {
                writer.WriteInt32(fixedLength);
            }
            else if (revision > 8)
            {
                writer.WriteBoolean(oldFixedLengthBool);
            }

            if (revision > 9)
                writer.WriteFloat(italics);

            if (revision > 0xC)
                writer.WriteFloat(size);

            if (revision > 0xD)
                writer.WriteBoolean(textMarkup);

            if (revision > 0xE)
                writer.WriteInt32(capsMode);

            if (revision >= 0x12 && revision <= 0x14)
                writer.WriteBoolean(oldUnknownBool);

            if (revision == 0x13 || revision == 0x14)
            {
                writer.WriteInt32(oldUnknownInt1);
                writer.WriteInt32(oldUnknownInt2);
                writer.WriteInt32(oldUnknownInt3);
            }

            if (standalone)
                writer.WriteEndBytes();
        }

    }
}
