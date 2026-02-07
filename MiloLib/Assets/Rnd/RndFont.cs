using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets.Rnd
{
    [Name("Font"), Description("Font objects determine the appearance for Text objects.")]
    public class RndFont : Object
    {
        public class CharInfo
        {
            public float texU;
            public float texV;
            public float charWidth;
            public float charAdvance;

            public CharInfo Read(EndianReader reader, ushort revision)
            {
                texU = reader.ReadFloat();
                texV = reader.ReadFloat();
                charWidth = reader.ReadFloat();
                if (charWidth < 0)
                    charWidth = 0;
                if (revision > 0xE)
                    charAdvance = reader.ReadFloat();
                else
                    charAdvance = charWidth;
                if (charAdvance < 0)
                    charAdvance = 0;
                return this;
            }

            public void Write(EndianWriter writer, ushort revision)
            {
                writer.WriteFloat(texU);
                writer.WriteFloat(texV);
                writer.WriteFloat(charWidth);
                if (revision > 0xE)
                    writer.WriteFloat(charAdvance);
            }
        }

        public class KernInfo
        {
            public ushort firstChar;
            public ushort secondChar;
            public float kerning;

            public KernInfo Read(EndianReader reader, ushort fontRevision)
            {
                if (fontRevision < 17)
                {
                    firstChar = reader.ReadByte();
                    secondChar = reader.ReadByte();
                }
                else
                {
                    firstChar = reader.ReadUInt16();
                    secondChar = reader.ReadUInt16();
                }
                if (fontRevision < 6)
                {
                    reader.ReadByte();
                    reader.ReadByte();
                }
                kerning = reader.ReadFloat();
                return this;
            }

            public void Write(EndianWriter writer, ushort fontRevision)
            {
                if (fontRevision < 17)
                {
                    writer.WriteByte((byte)firstChar);
                    writer.WriteByte((byte)secondChar);
                }
                else
                {
                    writer.WriteUInt16(firstChar);
                    writer.WriteUInt16(secondChar);
                }
                if (fontRevision < 6)
                {
                    writer.WriteByte(0);
                    writer.WriteByte(0);
                }
                writer.WriteFloat(kerning);
            }
        }

        public class KerningEntry
        {
            public int key;
            public float kerning;

            public KerningEntry Read(EndianReader reader)
            {
                key = reader.ReadInt32();
                kerning = reader.ReadFloat();
                return this;
            }

            public void Write(EndianWriter writer)
            {
                writer.WriteInt32(key);
                writer.WriteFloat(kerning);
            }
        }

        private ushort altRevision;
        private ushort revision;

        [Name("Material"), Description("Material for font"), MinVersion(1)]
        public Symbol mat = new(0, "");

        [MinVersion(10), MaxVersion(11)]
        public Symbol matName = new(0, "");

        [Name("Cell Size"), MinVersion(4)]
        public Vector2 cellSize = new(1, 1);

        [MinVersion(1)]
        public float deprecatedSize;

        [Name("Base Kerning"), Description("Base kerning of the font."), MinVersion(1)]
        public float baseKerning;

        [Name("Characters In Map"), Description("List of characters this font uses"), MinVersion(17)]
        public List<ushort> chars = new();

        [Name("Characters In Map"), Description("List of characters this font uses"), MinVersion(2), MaxVersion(16)]
        public Symbol charsAsString = new(0, "");

        [MinVersion(5)]
        public bool hasKerningTable;

        [MinVersion(5), MaxVersion(6)]
        public List<KernInfo> kerningInfos = new();

        [MinVersion(7)]
        public List<KerningEntry> kerningEntries = new();

        [Name("Texture Owner"), Description("Font owner of the texture to use"), MinVersion(9)]
        public Symbol textureOwner = new(0, "");

        [Name("Monospace"), Description("Font is monospaced."), MinVersion(11)]
        public bool monospace;

        [Name("Packed"), Description("Font texture is packed"), MinVersion(15)]
        public bool packed;

        [MinVersion(13)]
        public int bitmapWidth;

        [MinVersion(13)]
        public int bitmapHeight;

        [MinVersion(14)]
        public Vector2 texCellSize = new();

        [MinVersion(14)]
        public List<KeyValuePair<ushort, CharInfo>> charInfoMap = new();

        [MinVersion(17)]
        public Symbol nextFont = new(0, "");

        // old stuff for early revisions, TODO: figure out what these actually do so we can properly name them
        [MaxVersion(2)]
        public int oldA;
        [MaxVersion(2)]
        public int oldB;
        [MaxVersion(2)]
        public int oldC;
        [MaxVersion(2)]
        public int oldD;
        [MaxVersion(2)]
        public bool oldE;
        [MaxVersion(2)]
        public Symbol oldStr = new(0, "");
        [MinVersion(1), MaxVersion(1)]
        public int oldCellWidthInt;
        [MinVersion(1), MaxVersion(1)]
        public int oldCellHeightInt;
        [MinVersion(2), MaxVersion(3)]
        public float oldCellWidthFloat;
        [MinVersion(2), MaxVersion(3)]
        public float oldCellHeightFloat;
        [MaxVersion(0)]
        public uint oldMatCharCount;
        [MaxVersion(0)]
        public List<(byte key, Symbol name, float width, float height)> oldMatChars = new();

        [MinVersion(14), MaxVersion(16)]
        public CharInfo[] charInfoArray;

        public RndFont Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            if (revision > 7)
                base.Read(reader, false, parent, entry);

            if (revision < 3)
            {
                oldA = reader.ReadInt32();
                oldB = reader.ReadInt32();
                oldC = reader.ReadInt32();
                oldE = reader.ReadBoolean();
                oldD = reader.ReadInt32();
                oldStr = Symbol.Read(reader);
            }

            if (revision < 1)
            {
                oldMatCharCount = reader.ReadUInt32();
                for (int i = 0; i < oldMatCharCount; i++)
                {
                    byte key = reader.ReadByte();
                    Symbol name = Symbol.Read(reader);
                    float width = reader.ReadFloat();
                    float height = reader.ReadFloat();
                    oldMatChars.Add((key, name, width, height));
                }
            }
            else
            {
                mat = Symbol.Read(reader);
                if (revision == 10 || revision == 11)
                {
                    matName = Symbol.Read(reader);
                }
                if (revision < 4)
                {
                    if (revision < 2)
                    {
                        oldCellWidthInt = reader.ReadInt32();
                        oldCellHeightInt = reader.ReadInt32();
                    }
                    else
                    {
                        oldCellWidthFloat = reader.ReadFloat();
                        oldCellHeightFloat = reader.ReadFloat();
                    }
                }
                else
                {
                    cellSize = cellSize.Read(reader);
                }
                deprecatedSize = reader.ReadFloat();
                baseKerning = reader.ReadFloat();
            }

            if (revision > 1)
            {
                if (revision <= 0x10)
                {
                    charsAsString = Symbol.Read(reader);
                }
                else
                {
                    uint count = reader.ReadUInt32();
                    chars = new List<ushort>((int)count);
                    for (int i = 0; i < count; i++)
                    {
                        chars.Add(reader.ReadUInt16());
                    }
                }
            }

            if (revision > 4)
            {
                hasKerningTable = reader.ReadBoolean();
                if (hasKerningTable)
                {
                    if (revision < 7)
                    {
                        uint count = reader.ReadUInt32();
                        kerningInfos = new List<KernInfo>((int)count);
                        for (int i = 0; i < count; i++)
                        {
                            kerningInfos.Add(new KernInfo().Read(reader, revision));
                        }
                    }
                    else
                    {
                        int size = reader.ReadInt32();
                        kerningEntries = new List<KerningEntry>(size);
                        for (int i = 0; i < size; i++)
                        {
                            kerningEntries.Add(new KerningEntry().Read(reader));
                        }
                    }
                }
            }

            if (revision > 8)
                textureOwner = Symbol.Read(reader);

            if (revision > 10)
                monospace = reader.ReadBoolean();

            if (revision > 0xE)
                packed = reader.ReadBoolean();

            if (revision > 0xC)
            {
                bitmapWidth = reader.ReadInt32();
                bitmapHeight = reader.ReadInt32();
            }

            if (revision > 0xD)
            {
                texCellSize = texCellSize.Read(reader);
                if (revision < 0x11)
                {
                    charInfoArray = new CharInfo[256];
                    for (int i = 0; i < 256; i++)
                    {
                        charInfoArray[i] = new CharInfo().Read(reader, revision);
                    }
                }
                else
                {
                    uint count = reader.ReadUInt32();
                    charInfoMap = new List<KeyValuePair<ushort, CharInfo>>((int)count);
                    for (int i = 0; i < count; i++)
                    {
                        ushort key = reader.ReadUInt16();
                        CharInfo info = new CharInfo();
                        info.texU = reader.ReadFloat();
                        info.texV = reader.ReadFloat();
                        info.charWidth = reader.ReadFloat();
                        info.charAdvance = reader.ReadFloat();
                        charInfoMap.Add(new KeyValuePair<ushort, CharInfo>(key, info));
                    }
                }
            }

            if (revision > 0x10)
                nextFont = Symbol.Read(reader);

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw MiloLib.Exceptions.MiloAssetReadException.EndBytesNotFound(parent, entry, reader.BaseStream.Position);

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            if (revision > 7)
                base.Write(writer, false, parent, entry);

            if (revision < 3)
            {
                writer.WriteInt32(oldA);
                writer.WriteInt32(oldB);
                writer.WriteInt32(oldC);
                writer.WriteBoolean(oldE);
                writer.WriteInt32(oldD);
                Symbol.Write(writer, oldStr);
            }

            if (revision < 1)
            {
                writer.WriteUInt32((uint)oldMatChars.Count);
                foreach (var mc in oldMatChars)
                {
                    writer.WriteByte(mc.key);
                    Symbol.Write(writer, mc.name);
                    writer.WriteFloat(mc.width);
                    writer.WriteFloat(mc.height);
                }
            }
            else
            {
                Symbol.Write(writer, mat);
                if (revision == 10 || revision == 11)
                {
                    Symbol.Write(writer, matName);
                }
                if (revision < 4)
                {
                    if (revision < 2)
                    {
                        writer.WriteInt32(oldCellWidthInt);
                        writer.WriteInt32(oldCellHeightInt);
                    }
                    else
                    {
                        writer.WriteFloat(oldCellWidthFloat);
                        writer.WriteFloat(oldCellHeightFloat);
                    }
                }
                else
                {
                    cellSize.Write(writer);
                }
                writer.WriteFloat(deprecatedSize);
                writer.WriteFloat(baseKerning);
            }

            if (revision > 1)
            {
                if (revision <= 0x10)
                {
                    Symbol.Write(writer, charsAsString);
                }
                else
                {
                    writer.WriteUInt32((uint)chars.Count);
                    foreach (ushort c in chars)
                    {
                        writer.WriteUInt16(c);
                    }
                }
            }

            if (revision > 4)
            {
                writer.WriteBoolean(hasKerningTable);
                if (hasKerningTable)
                {
                    if (revision < 7)
                    {
                        writer.WriteUInt32((uint)kerningInfos.Count);
                        foreach (var ki in kerningInfos)
                        {
                            ki.Write(writer, revision);
                        }
                    }
                    else
                    {
                        writer.WriteInt32(kerningEntries.Count);
                        foreach (var ke in kerningEntries)
                        {
                            ke.Write(writer);
                        }
                    }
                }
            }

            if (revision > 8)
                Symbol.Write(writer, textureOwner);

            if (revision > 10)
                writer.WriteBoolean(monospace);

            if (revision > 0xE)
                writer.WriteBoolean(packed);

            if (revision > 0xC)
            {
                writer.WriteInt32(bitmapWidth);
                writer.WriteInt32(bitmapHeight);
            }

            if (revision > 0xD)
            {
                texCellSize.Write(writer);
                if (revision < 0x11)
                {
                    if (charInfoArray == null)
                        charInfoArray = new CharInfo[256];
                    for (int i = 0; i < 256; i++)
                    {
                        if (charInfoArray[i] == null)
                            charInfoArray[i] = new CharInfo();
                        charInfoArray[i].Write(writer, revision);
                    }
                }
                else
                {
                    writer.WriteUInt32((uint)charInfoMap.Count);
                    foreach (var kvp in charInfoMap)
                    {
                        writer.WriteUInt16(kvp.Key);
                        writer.WriteFloat(kvp.Value.texU);
                        writer.WriteFloat(kvp.Value.texV);
                        writer.WriteFloat(kvp.Value.charWidth);
                        writer.WriteFloat(kvp.Value.charAdvance);
                    }
                }
            }

            if (revision > 0x10)
                Symbol.Write(writer, nextFont);

            if (standalone)
                writer.WriteEndBytes();
        }
    }
}
