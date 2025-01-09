using MiloLib.Assets;
using MiloLib.Classes;
using MiloLib.Utils;

[Name("UIFontImporter"), Description("Class supporting font importing. To be included in font resource file classes.")]
public class UIFontImporter
{
    public enum FontSuperSample
    {
        kFontSuperSample_None,
        kFontSuperSample_2x,
        kFontSuperSample_4x
    }
    private ushort altRevision;
    private ushort revision;

    [Name("Include Lower Case"), Description("include uppercase letters")]
    public bool includeLowerCase;
    [Name("Include Upper Case"), Description("include lowercase letters")]
    public bool includeUpperCase;
    [Name("Include Numbers"), Description("include the number 0-9")]
    public bool includeNumbers;
    [Name("Include Punctuation"), Description("include punctuation characters")]
    public bool includePunctuation;
    [Name("Include Upper Euro"), Description("include uppercase euro chars")]
    public bool includeUpperEuro;
    [Name("Include Lower Euro"), Description("include lowercase euro chars")]
    public bool includeLowerEuro;
    [Name("Plus Chars"), Description("type in extra characters to include here")]
    public Symbol plusChars = new(0, "");
    [Name("Minus Chars"), Description("type in characters to exclude here")]
    public Symbol minusChars = new(0, "");
    [Name("Font Name"), Description("name of the font")]
    public Symbol fontName = new(0, "");

    [Name("Font Pct Size"), Description("default font size in percent screen height")]
    public float fontPctSize;
    [Name("Font Weight"), Description("the equivalent point size")]
    public int fontWeight;

    [Name("Is Font Italic"), Description("italic variation?")]
    public bool isFontItalic;

    [Name("Pitch And Family"), Description("pitch and family of font - comes from font picker")]
    public int pitchAndFamily;

    [Name("Font Quality"), Description("quality of font")]
    public int fontQuality;

    [Name("Font Char Set"), Description("character set for this font - comes from font picker")]
    public int fontCharSet;

    [Name("Font Super Sample"), Description("our own supersampling that draws the font texture at 2 or 4x and scales down like in photoshop.  Might improve anti-aliasing at small font sizes."), MinVersion(2)]
    public FontSuperSample fontSupersample;

    public Symbol bitmapPath = new(0, "");
    public Symbol bitmapFilename = new(0, "");

    [Name("Texture Padding Left"), Description("pixels of padding on the left side of each character")]
    public int texPadLeft;
    [Name("Texture Padding Right"), Description("pixels of padding on the right side of each character")]
    public int texPadRight;
    [Name("Texture Padding Top"), Description("pixels of padding on the top side of each character")]
    public int texPadTop;
    [Name("Texture Padding Bottom"), Description("pixels of padding on the bottom side of each character")]
    public int texPadBottom;

    [Name("Fill With Safe White"), Description("fill texture with safe white color (235)")]
    public bool fillWithSafeWhite;

    private int genedFontsCount;
    [Name("Generated Fonts"), Description("the font(s) we've gen-ed for this resource file.  We will maintain a connection to these objects when you re-generate")]
    public List<Symbol> genFonts = new();

    [Name("Kerning Reference"), Description("A font which we'll transfer the kerning info from for any gen-ed fonts")]
    public Symbol kerningReference = new(0, "");

    private int matVariationsCount;
    [Name("Mat Variations"), Description("A list of materials we will expose for this font"), MinVersion(4)]
    public List<Symbol> matVariations = new();

    [Name("Default Material"), MinVersion(6)]
    public Symbol defaultMat = new(0, "");

    [Name("Handmade Font"), Description("If you want to handmake a font texture, assign it here and the importer will no longer try to generate textures"), MinVersion(7)]
    public Symbol handmadeFont = new(0, "");

    [Name("Sync Resource"), MinVersion(8)]
    public Symbol syncResource = new(0, "");

    [Name("Last Gen Was Next-Gen"), Description("was the texture for this font last genned for an NG platform?"), MinVersion(9)]
    public bool lastGenWasNg;

    [MaxVersion(4)]
    public int unkInt1;


    [Name("Font to Import From"), MaxVersion(7)]
    public Symbol fontToImportFrom = new(0, "");

    [MinVersion(3), MaxVersion(3)]
    public Symbol unkMatSymbol = new(0, "");

    public UIFontImporter Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
    {
        uint combinedRevision = reader.ReadUInt32();
        if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
        else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

        includeLowerCase = reader.ReadBoolean();
        includeUpperCase = reader.ReadBoolean();
        includeNumbers = reader.ReadBoolean();
        includePunctuation = reader.ReadBoolean();
        includeUpperEuro = reader.ReadBoolean();
        includeLowerEuro = reader.ReadBoolean();

        plusChars = Symbol.Read(reader);
        minusChars = Symbol.Read(reader);
        fontName = Symbol.Read(reader);

        if (revision <= 4)
        {
            unkInt1 = reader.ReadInt32();
        }

        fontPctSize = reader.ReadFloat();
        fontWeight = reader.ReadInt32();

        isFontItalic = reader.ReadBoolean();

        pitchAndFamily = reader.ReadInt32();

        fontQuality = reader.ReadInt32();
        fontCharSet = reader.ReadInt32();

        if (revision > 1)
            fontSupersample = (FontSuperSample)reader.ReadInt32();

        bitmapPath = Symbol.Read(reader);
        bitmapFilename = Symbol.Read(reader);

        texPadLeft = reader.ReadInt32();
        texPadRight = reader.ReadInt32();
        texPadTop = reader.ReadInt32();
        texPadBottom = reader.ReadInt32();

        fillWithSafeWhite = reader.ReadBoolean();

        if (revision < 8)
            fontToImportFrom = Symbol.Read(reader);

        if (revision > 2)
        {
            genedFontsCount = reader.ReadInt32();
            for (int i = 0; i < genedFontsCount; i++)
            {
                genFonts.Add(Symbol.Read(reader));
            }

            kerningReference = Symbol.Read(reader);
        }

        if (revision == 3)
        {
            unkMatSymbol = Symbol.Read(reader);
        }

        if (revision > 3)
        {
            matVariationsCount = reader.ReadInt32();
            for (int i = 0; i < matVariationsCount; i++)
            {
                matVariations.Add(Symbol.Read(reader));
            }
        }

        if (revision > 5)
            defaultMat = Symbol.Read(reader);

        if (revision > 6)
            handmadeFont = Symbol.Read(reader);

        if (revision > 7)
            syncResource = Symbol.Read(reader);

        if (revision > 8)
            lastGenWasNg = reader.ReadBoolean();

        if (standalone)
        {
            if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw new Exception("Got to end of standalone asset but didn't find the expected end bytes, read likely did not succeed");
        }
        return this;
    }

    public void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
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

        if (revision <= 4)
        {
            writer.WriteInt32(unkInt1);
        }

        writer.WriteFloat(fontPctSize);
        writer.WriteInt32(fontWeight);

        writer.WriteBoolean(isFontItalic);

        writer.WriteInt32(pitchAndFamily);

        writer.WriteInt32(fontQuality);

        writer.WriteInt32(fontCharSet);
        writer.WriteInt32((int)fontSupersample);

        Symbol.Write(writer, bitmapPath);
        Symbol.Write(writer, bitmapFilename);

        writer.WriteInt32(texPadLeft);
        writer.WriteInt32(texPadRight);
        writer.WriteInt32(texPadTop);
        writer.WriteInt32(texPadBottom);

        writer.WriteBoolean(fillWithSafeWhite);

        if (revision < 8)
            Symbol.Write(writer, fontToImportFrom);

        if (revision > 2)
        {
            writer.WriteInt32(genedFontsCount);
            foreach (var genFont in genFonts)
            {
                Symbol.Write(writer, genFont);
            }

            Symbol.Write(writer, kerningReference);
        }

        if (revision == 3)
        {
            Symbol.Write(writer, unkMatSymbol);
        }

        if (revision > 3)
        {
            writer.WriteInt32(matVariationsCount);
            foreach (var matVariation in matVariations)
            {
                Symbol.Write(writer, matVariation);
            }
        }

        if (revision > 5)
            Symbol.Write(writer, defaultMat);

        if (revision > 6)
            Symbol.Write(writer, handmadeFont);

        if (revision > 7)
            Symbol.Write(writer, syncResource);

        if (revision > 8)
            writer.WriteBoolean(lastGenWasNg);

        if (standalone)
        {
            writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
        }
    }

}