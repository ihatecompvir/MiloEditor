using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets.UI
{
    [Name("UILabel"), Description("Simple label, provides localization of tokens")]
    public class UILabel : UIComponent
    {
        public enum TextAlignments
        {
            kTopLeft,
            kTopCenter,
            kTopRight,
            kMiddleLeft,
            kMiddleCenter,
            kMiddleRight,
            kBottomLeft,
            kBottomCenter,
            kBottomRight,
        }

        public enum CapsModes
        {
            kCapsModeNone,
            kCapsModeLower,
            kCapsModeUpper,
        }

        public enum LabelFitTypes
        {
            kFitWrap,
            kFitJust,
            kFitEllipsis
        }
        private ushort altRevision;
        private ushort revision;

        [Name("Text Token"), Description("Localization token if localize is true")]
        public Symbol textToken = new(0, "");
        [Name("Icon"), Description("Single-character icon"), MinVersion(0xF)]
        public Symbol icon = new(0, "");


        [Name("Text Size"), Description("Text size in percentage of screen height (i.e. 50% is half the screen height for the largest glyph)"), MinVersion(2)]
        public float textSize;
        [Name("Alignment"), Description("Text alignment"), MinVersion(2)]
        public TextAlignments alignment;
        [Name("Caps Mode"), Description("Text case setting"), MinVersion(2)]
        public CapsModes capsMode;

        [Name("Supports Markup"), Description("Support markup?"), MinVersion(8)]
        public bool supportsMarkup;
        [Name("Leading"), Description("Space between lines"), MinVersion(2)]
        public float leading;
        [Name("Kerning"), Description("Additional kerning applied to text object"), MinVersion(2)]
        public float kerning;
        [Name("Italics"), Description("Italics for text object"), MinVersion(5)]
        public float italics;

        [Name("Fit Type"), Description("How to fit text in the width/height specified"), MinVersion(3)]
        public LabelFitTypes fitType;
        [Name("Width"), Description("Width of label"), MinVersion(3)]
        public float width;
        [Name("Height"), Description("Height of label"), MinVersion(3)]
        public float height;

        [Name("Fixed Length"), Description("Preallocated size of internal text object"), MinVersion(6)]
        public short fixedLength;
        [Name("Reserved Lines"), Description("Preallocated number of lines in internal text object"), MinVersion(7)]
        public short reservedLines;

        [Name("Preserve Truncated Text"), Description("Optional text to append after truncation with kFitEllipsis"), MinVersion(10)]
        public Symbol preserveTruncText = new(0, "");


        [Name("Alpha"), Description("Controls transparency of label"), MinVersion(11)]
        public float alpha;
        [Name("Color Override"), Description("Color override for this instance"), MinVersion(0xD)]
        public Symbol colorOverride = new(0, "");
        [Name("Font Material Variation"), Description("Material variation for font"), MinVersion(0x15)]
        public Symbol fontMatVariation = new(0, "");
        [Name("Alternate Material Variation"), Description("Material variation for alt font"), MinVersion(0x17)]
        public Symbol altMatVariation = new(0, "");
        [Name("Alternate Text Size"), Description("Text size of alternate style in percentage of screen height (i.e. 50% is half the screen height for the largest glyph)"), MinVersion(0x12)]
        public float altTextSize;
        [Name("Alternate Kerning"), Description("Additional kerning applied to alt text object"), MinVersion(0x13)]
        public float altKerning;
        [Name("Alternate Text Color"), Description("Color to use when in alt style"), MinVersion(0x12)]
        public Symbol altTextColor = new(0, "");
        [Name("Alternate Z Offset"), Description("Z-offset for alt text (to manually match up baselines)"), MinVersion(0x14)]
        public float altZOffset;
        [Name("Alternate Italics"), Description("Italics for text object's alt font"), MinVersion(0x18)]
        public float altItalics;
        [Name("Alternate Alpha"), Description("Controls transparency of label's alt font"), MinVersion(0x18)]
        public float altAlpha;
        [Name("Use Highlight Mesh"), Description("whether or not to use highlight mesh (if available)"), MinVersion(0x11)]
        public bool useHighlightMesh;
        [Name("Alternate Style Enabled"), Description("Whether to parse <alt> tags for alt style"), MinVersion(0x12)]
        public bool altStyleEnabled;
        [Name("Alternate Font Resource Name"), Description("path to alt font resource file for this component"), MinVersion(0x16)]
        public Symbol altFontResourceName = new(0, "");

        public Symbol unkSymbol = new(0, "");
        public bool unkBool, unkBool2;
        public int unkIntA, unkIntC, unkIntD;



        public UILabel Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            base.Read(reader, false, parent, entry);
            if (revision != 0 && revision < 0xE)
            {
                unkBool = reader.ReadBoolean();
            }
            textToken = Symbol.Read(reader);

            if (revision > 0xD)
            {
                unkSymbol = Symbol.Read(reader);
            }

            if (revision > 0xE)
            {
                icon = Symbol.Read(reader);
            }

            if (revision > 1)
            {
                textSize = reader.ReadFloat();
                alignment = (TextAlignments)reader.ReadInt32();
                capsMode = (CapsModes)reader.ReadInt32();

                if (revision > 7)
                {
                    supportsMarkup = reader.ReadBoolean();
                }
                leading = reader.ReadFloat();
                kerning = reader.ReadFloat();
            }
            if (revision > 4)
            {
                italics = reader.ReadFloat();
            }
            if (revision > 2)
            {
                fitType = (LabelFitTypes)reader.ReadInt32();
                width = reader.ReadFloat();
                height = reader.ReadFloat();
            }
            if (revision > 5)
            {
                fixedLength = (short)reader.ReadInt32();
            }
            if (revision > 6)
            {
                reservedLines = (short)reader.ReadInt32();
            }
            if (revision >= 9 && revision <= 15)
            {
                unkBool2 = reader.ReadBoolean();
                unkIntA = reader.ReadInt32();
                unkIntC = reader.ReadInt32();
                unkIntD = reader.ReadInt32();
            }

            if (revision > 9)
            {
                preserveTruncText = Symbol.Read(reader);
            }
            if (revision > 10)
            {
                alpha = reader.ReadFloat();
            }
            if (revision > 0xC)
            {
                colorOverride = Symbol.Read(reader);
            }
            if (revision > 0x10)
            {
                useHighlightMesh = reader.ReadBoolean();
            }
            if (revision > 0x11)
            {
                altTextSize = reader.ReadFloat();
                altTextColor = Symbol.Read(reader);
                altStyleEnabled = reader.ReadBoolean();
            }
            if (revision > 0x12)
            {
                altKerning = reader.ReadFloat();
            }
            else
            {
                altKerning = kerning;
            }
            if (revision > 0x13)
            {
                altZOffset = reader.ReadFloat();
            }
            if (revision > 0x14)
            {
                fontMatVariation = Symbol.Read(reader);
            }
            if (revision > 0x15)
            {
                altFontResourceName = Symbol.Read(reader);
            }
            if (revision > 0x16)
            {
                altMatVariation = Symbol.Read(reader);
            }
            if (revision > 0x17)
            {
                altItalics = reader.ReadFloat();
                altAlpha = reader.ReadFloat();
            }

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw new Exception("Got to end of standalone asset but didn't find the expected end bytes, read likely did not succeed");

            return this;
        }


        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            base.Write(writer, false, parent, entry);

            if (revision != 0 && revision < 0xE)
            {
                writer.WriteBoolean(unkBool);
            }
            Symbol.Write(writer, textToken);

            if (revision > 0xD)
            {
                Symbol.Write(writer, unkSymbol);
            }

            if (revision > 0xE)
            {
                Symbol.Write(writer, icon);
            }

            if (revision > 1)
            {
                writer.WriteFloat(textSize);
                writer.WriteInt32((int)alignment);
                writer.WriteInt32((int)capsMode);

                if (revision > 7)
                {
                    writer.WriteBoolean(supportsMarkup);
                }
                writer.WriteFloat(leading);
                writer.WriteFloat(kerning);
            }
            if (revision > 4)
            {
                writer.WriteFloat(italics);
            }
            if (revision > 2)
            {
                writer.WriteInt32((int)fitType);
                writer.WriteFloat(width);
                writer.WriteFloat(height);
            }
            if (revision > 5)
            {
                writer.WriteInt32(fixedLength);
            }
            if (revision > 6)
            {
                writer.WriteInt32(reservedLines);
            }
            if (revision >= 9 && revision <= 15)
            {
                writer.WriteBoolean(unkBool2);
                writer.WriteInt32(unkIntA);
                writer.WriteInt32(unkIntC);
                writer.WriteInt32(unkIntD);
            }
            if (revision > 9)
            {
                Symbol.Write(writer, preserveTruncText);
            }
            if (revision > 10)
            {
                writer.WriteFloat(alpha);
            }
            if (revision > 0xC)
            {
                Symbol.Write(writer, colorOverride);
            }
            if (revision > 0x10)
            {
                writer.WriteBoolean(useHighlightMesh);
            }
            if (revision > 0x11)
            {
                writer.WriteFloat(altTextSize);
                Symbol.Write(writer, altTextColor);
                writer.WriteBoolean(altStyleEnabled);
            }
            if (revision > 0x12)
            {
                writer.WriteFloat(altKerning);
            }
            if (revision > 0x13)
            {
                writer.WriteFloat(altZOffset);
            }
            if (revision > 0x14)
            {
                Symbol.Write(writer, fontMatVariation);
            }
            if (revision > 0x15)
            {
                Symbol.Write(writer, altFontResourceName);
            }
            if (revision > 0x16)
            {
                Symbol.Write(writer, altMatVariation);
            }
            if (revision > 0x17)
            {
                writer.WriteFloat(altItalics);
                writer.WriteFloat(altAlpha);
            }
            if (standalone)
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
        }
    }
}