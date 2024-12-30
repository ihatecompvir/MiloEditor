using MiloLib.Assets.Rnd;
using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets.UI
{

    [Name("UIFontImporter")]
    public class UIFontImporter
    {
        public ushort altRevision;
        public ushort revision;

        public bool includeLowerCase;
        public bool includeUpperCase;
        public bool includeNumbers;
        public bool includePunctuation;
        public bool includeUpperEuro;
        public bool includeLowerEuro;
        public Symbol plusChars = new(0, "");
        public Symbol minusChars = new(0, "");
        public Symbol fontName = new(0, "");

        public float fontPctSize;
        public int fontWeight;

        public bool isFontItalic;

        public int pitchAndFamily;

        public int fontQuality;

        public int fontCharSet;
        public int fontSupersample;

        public Symbol bitmapPath = new(0, "");
        public Symbol bitmapFilename = new(0, "");

        public int texPadLeft;
        public int texPadRight;
        public int texPadTop;
        public int texPadBottom;

        public bool fillWithSafeWhite;

        public int genedFontsCount;
        public List<Symbol> genFonts = new();

        public Symbol kerningReference = new(0, "");

        public int matVariationsCount;
        public List<Symbol> matVariations = new();

        public Symbol defaultMat = new(0, "");

        public Symbol handmadeFont = new(0, "");

        public Symbol syncResource = new(0, "");

        public bool lastGenWasNg;

        public UIFontImporter Read(EndianReader reader, bool standalone)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            if (revision != 9)
            {
                throw new UnsupportedAssetRevisionException("UIFontImporter", revision);
            }

            includeLowerCase = reader.ReadBoolean();
            includeUpperCase = reader.ReadBoolean();
            includeNumbers = reader.ReadBoolean();
            includePunctuation = reader.ReadBoolean();
            includeUpperEuro = reader.ReadBoolean();
            includeLowerEuro = reader.ReadBoolean();

            plusChars = Symbol.Read(reader);
            minusChars = Symbol.Read(reader);
            fontName = Symbol.Read(reader);

            fontPctSize = reader.ReadFloat();
            fontWeight = reader.ReadInt32();

            isFontItalic = reader.ReadBoolean();

            pitchAndFamily = reader.ReadInt32();

            fontQuality = reader.ReadInt32();

            fontCharSet = reader.ReadInt32();
            fontSupersample = reader.ReadInt32();

            bitmapPath = Symbol.Read(reader);
            bitmapFilename = Symbol.Read(reader);

            texPadLeft = reader.ReadInt32();
            texPadRight = reader.ReadInt32();
            texPadTop = reader.ReadInt32();
            texPadBottom = reader.ReadInt32();

            fillWithSafeWhite = reader.ReadBoolean();

            genedFontsCount = reader.ReadInt32();
            for (int i = 0; i < genedFontsCount; i++)
            {
                genFonts.Add(Symbol.Read(reader));
            }

            kerningReference = Symbol.Read(reader);

            matVariationsCount = reader.ReadInt32();
            for (int i = 0; i < matVariationsCount; i++)
            {
                matVariations.Add(Symbol.Read(reader));
            }

            defaultMat = Symbol.Read(reader);

            handmadeFont = Symbol.Read(reader);

            syncResource = Symbol.Read(reader);

            lastGenWasNg = reader.ReadBoolean();

            if (standalone)
            {
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw new Exception("Got to end of standalone asset but didn't find the expected end bytes, read likely did not succeed");
            }
            return this;
        }

        public void Write(EndianWriter writer, bool standalone)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            writer.WriteBoolean(includeLowerCase);
            writer.WriteBoolean(includeUpperCase);
            writer.WriteBoolean(includeNumbers);
            writer.WriteBoolean(includePunctuation);
            writer.WriteBoolean(includeUpperEuro);
            writer.WriteBoolean(includeLowerEuro);

            Symbol.Write(writer, plusChars);
            Symbol.Write(writer, minusChars);
            Symbol.Write(writer, fontName);

            writer.WriteFloat(fontPctSize);
            writer.WriteInt32(fontWeight);

            writer.WriteBoolean(isFontItalic);

            writer.WriteInt32(pitchAndFamily);

            writer.WriteInt32(fontQuality);

            writer.WriteInt32(fontCharSet);
            writer.WriteInt32(fontSupersample);

            Symbol.Write(writer, bitmapPath);
            Symbol.Write(writer, bitmapFilename);

            writer.WriteInt32(texPadLeft);
            writer.WriteInt32(texPadRight);
            writer.WriteInt32(texPadTop);
            writer.WriteInt32(texPadBottom);

            writer.WriteBoolean(fillWithSafeWhite);

            writer.WriteInt32(genedFontsCount);
            foreach (var genFont in genFonts)
            {
                Symbol.Write(writer, genFont);
            }

            Symbol.Write(writer, kerningReference);

            writer.WriteInt32(matVariationsCount);
            foreach (var matVariation in matVariations)
            {
                Symbol.Write(writer, matVariation);
            }

            Symbol.Write(writer, defaultMat);

            Symbol.Write(writer, handmadeFont);

            Symbol.Write(writer, syncResource);

            writer.WriteBoolean(lastGenWasNg);

            if (standalone)
            {
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
            }
        }

    }

    [Name("UILabelDir"), Description("Top-level resource object for UILabels")]
    public class UILabelDir : RndDir
    {
        public ushort altRevision;
        public ushort revision;

        public Symbol textObject = new(0, "");

        public Symbol fontReference = new(0, "");

        public Symbol focusAnim = new(0, "");
        public Symbol pulseAnim = new(0, "");

        public Symbol highlightMeshGroup = new(0, "");

        public Symbol topLeftHighlightBone = new(0, "");
        public Symbol topRightHighlightBone = new(0, "");
        public Symbol bottomLeftHighlightBone = new(0, "");
        public Symbol bottomRightHighlightBone = new(0, "");

        public Symbol focusedBackgroundGroup = new(0, "");
        public Symbol unfocusedBackgroundGroup = new(0, "");

        public bool allowEditText;

        public Symbol defaultColor = new(0, "");
        public List<Symbol> colors = new List<Symbol> { new Symbol(0, ""), new Symbol(0, ""), new Symbol(0, ""), new Symbol(0, ""), new Symbol(0, "") };

        public UIFontImporter fontImporter = new UIFontImporter();

        public UILabelDir(ushort revision, ushort altRevision = 0) : base(revision, altRevision)
        {
            revision = revision;
            altRevision = altRevision;
            return;
        }

        public UILabelDir Read(EndianReader reader, bool standalone)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            base.Read(reader, false);

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

            for (int i = 0; i < 5; i++)
                colors[i] = Symbol.Read(reader);

            if (revision >= 8)
                fontImporter = new UIFontImporter().Read(reader, false);

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw new Exception("Got to end of standalone asset but didn't find the expected end bytes, read likely did not succeed");

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            base.Write(writer, false);

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
            foreach (var color in colors)
            {
                Symbol.Write(writer, color);
            }

            if (revision >= 8)
                fontImporter.Write(writer, false);

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
