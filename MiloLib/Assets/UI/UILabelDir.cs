using MiloLib.Assets.Rnd;
using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets.UI
{

    [Name("UILabelDir"), Description("Top-level resource object for UILabels")]
    public class UILabelDir : RndDir
    {
        private ushort altRevision;
        private ushort revision;

        [Name("Text Object")]
        public Symbol textObject = new(0, "");

        [Name("Font Reference"), MinVersion(3), MaxVersion(8)]
        public Symbol fontReference = new(0, "");

        [Name("Focus Anim"), MinVersion(1)]
        public Symbol focusAnim = new(0, "");
        [Name("Pulse Anim"), MinVersion(2)]
        public Symbol pulseAnim = new(0, "");

        [Name("Highlight Mesh Group"), MinVersion(4)]
        public Symbol highlightMeshGroup = new(0, "");
        [Name("Top Left Highlight Bone"), MinVersion(4)]
        public Symbol topLeftHighlightBone = new(0, "");
        [Name("Top Right Highlight Bone"), MinVersion(4)]
        public Symbol topRightHighlightBone = new(0, "");
        [Name("Bottom Left Highlight Bone"), MinVersion(5)]
        public Symbol bottomLeftHighlightBone = new(0, "");
        [Name("Bottom Right Highlight Bone"), MinVersion(5)]
        public Symbol bottomRightHighlightBone = new(0, "");

        [Name("Focused Background Group"), MinVersion(6)]
        public Symbol focusedBackgroundGroup = new(0, "");
        [Name("Unfocused Background Group"), MinVersion(6)]
        public Symbol unfocusedBackgroundGroup = new(0, "");

        [Name("Allow Edit Text"), Description("allow non-localized text with this resource?"), MinVersion(7)]
        public bool allowEditText;

        [Name("Default Color"), Description("color to use when no other color is defined for a state")]
        public Symbol defaultColor = new(0, "");
        [Name("Normal Color"), Description("color when label is normal")]
        public Symbol normalColor = new(0, "");
        [Name("Focused Color"), Description("color when label is focused")]
        public Symbol focusedColor = new(0, "");
        [Name("Disabled Color"), Description("color when label is disabled")]
        public Symbol disabledColor = new(0, "");
        [Name("Selecting Color"), Description("color when label is selecting")]
        public Symbol selectingColor = new(0, "");
        [Name("Selected Color"), Description("color when label is selected")]
        public Symbol selectedColor = new(0, "");

        [Name("Font Importer"), MinVersion(8)]
        public UIFontImporter fontImporter = new UIFontImporter();

        public UILabelDir(ushort revision, ushort altRevision = 0) : base(revision, altRevision)
        {
            revision = revision;
            altRevision = altRevision;
            return;
        }

        public UILabelDir Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            base.Read(reader, false, parent, entry);

            textObject = Symbol.Read(reader);

            if (revision >= 3 && revision <= 8)
                fontReference = Symbol.Read(reader);

            if (revision >= 1)
                focusAnim = Symbol.Read(reader);
            if (revision >= 2)
                pulseAnim = Symbol.Read(reader);


            if (revision >= 4)
            {
                highlightMeshGroup = Symbol.Read(reader);
                topLeftHighlightBone = Symbol.Read(reader);
                topRightHighlightBone = Symbol.Read(reader);
            }
            if (revision >= 5)
            {
                bottomLeftHighlightBone = Symbol.Read(reader);
                bottomRightHighlightBone = Symbol.Read(reader);
            }

            if (revision >= 6)
            {
                focusedBackgroundGroup = Symbol.Read(reader);
                unfocusedBackgroundGroup = Symbol.Read(reader);
            }

            if (revision >= 7)
                allowEditText = reader.ReadBoolean();

            defaultColor = Symbol.Read(reader);
            normalColor = Symbol.Read(reader);
            focusedColor = Symbol.Read(reader);
            disabledColor = Symbol.Read(reader);
            selectingColor = Symbol.Read(reader);
            selectedColor = Symbol.Read(reader);

            if (revision >= 8)
                fontImporter = new UIFontImporter().Read(reader, false, parent, entry);

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw MiloLib.Exceptions.MiloAssetReadException.EndBytesNotFound(parent, entry, reader.BaseStream.Position);

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            base.Write(writer, false, parent, entry);

            Symbol.Write(writer, textObject);

            if (revision >= 3 && revision <= 8)
                Symbol.Write(writer, fontReference);

            if (revision >= 1)
                Symbol.Write(writer, focusAnim);
            if (revision >= 2)
                Symbol.Write(writer, pulseAnim);

            if (revision >= 4)
            {
                Symbol.Write(writer, highlightMeshGroup);
                Symbol.Write(writer, topLeftHighlightBone);
                Symbol.Write(writer, topRightHighlightBone);
            }

            if (revision >= 5)
            {
                Symbol.Write(writer, bottomLeftHighlightBone);
                Symbol.Write(writer, bottomRightHighlightBone);
            }


            if (revision >= 6)
            {
                Symbol.Write(writer, focusedBackgroundGroup);
                Symbol.Write(writer, unfocusedBackgroundGroup);
            }


            if (revision >= 7)
                writer.WriteBoolean(allowEditText);

            Symbol.Write(writer, defaultColor);
            Symbol.Write(writer, normalColor);
            Symbol.Write(writer, focusedColor);
            Symbol.Write(writer, disabledColor);
            Symbol.Write(writer, selectingColor);
            Symbol.Write(writer, selectedColor);

            if (revision >= 8)
                fontImporter.Write(writer, false, parent, entry);

            if (standalone)
            {
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
            }
        }

        public override bool IsDirectory()
        {
            return true;
        }


    }
}
