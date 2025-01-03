using MiloLib.Utils;
using MiloLib.Classes;
using MiloLib.Assets;

namespace MiloLib.Assets.Char
{
    [Name("CharClipSet"), Description("A CharClip container.")]
    public class CharClipSet : ObjectDir
    {
        public class CharClipPtr
        {
            public Symbol clip = new(0, "");
            public uint unk1;
            public uint unk2;

            public CharClipPtr Read(EndianReader reader)
            {
                clip = Symbol.Read(reader);
                unk1 = reader.ReadUInt32();
                unk2 = reader.ReadUInt32();

                return this;
            }

            public void Write(EndianWriter writer)
            {
                Symbol.Write(writer, clip);
                writer.WriteUInt32(unk1);
                writer.WriteUInt32(unk2);
            }

            public override string ToString()
            {
                return $"{clip} {unk1} {unk2}";
            }
        }

        public CharClipSet(ushort revision, ushort altRevision = 0) : base(revision, altRevision)
        {
            revision = revision;
            altRevision = altRevision;
            return;
        }

        private ushort altRevision;
        private ushort revision;

        [Name("Character File Path"), Description("Preview base character to use- for example, char/male/male_guitar.milo for male guitarist"), MinVersion(18)]
        public Symbol charFilePath = new(0, "");

        [Name("Preview Clip"), Description("Pick a clip to play"), MinVersion(18)]
        public Symbol previewClip = new(0, "");

        [Name("Filter Flags"), Description("Flags for filtering preview clip"), MinVersion(20)]
        public uint filterFlags;

        [Name("BPM"), Description("bpm for clip playing"), MinVersion(21)]
        public uint bpm;

        [Name("Preview Walk"), Description("Allow preview character to move around and walk?"), MinVersion(22)]
        public bool previewWalk;

        [Name("Still Clip"), Description("Set this to view drummer play anims"), MinVersion(23)]
        public Symbol stillClip = new(0, "");

        [MinVersion(25)]
        public Symbol unkSymbol = new(0, "");

        [Name("Graph Path"), MinVersion(0), MaxVersion(8)]
        public Symbol graphPath = new(0, "");

        [MinVersion(14), MaxVersion(23)]
        public bool unkBool1;
        [MinVersion(19), MaxVersion(23)]
        public bool unkBool2;
        [MinVersion(5), MaxVersion(23)]
        public bool unkBool3;
        [MinVersion(11), MaxVersion(11)]
        public bool unkBool4;


        [MinVersion(0), MaxVersion(16)]
        public int unkInt1;
        [MinVersion(0), MaxVersion(16)]
        public int unkInt2;
        [MinVersion(15), MaxVersion(16)]
        public int unkInt3;
        [MinVersion(0), MaxVersion(5)]
        public int unkInt4;
        [MinVersion(10), MaxVersion(23)]
        public int unkInt5;

        [MinVersion(0)]
        public Symbol unknownSymbol = new(0, "");
        [MinVersion(10), MaxVersion(23)]
        public Symbol unknownSymbol2 = new(0, "");

        [MinVersion(0), MaxVersion(5)]
        public string unkString;

        [Name("Char Clip Pointers"), Description("The list of CharClipSamples objects in the directory."), MinVersion(0), MaxVersion(23)]
        public List<CharClipPtr> charClipPtrs = new();

        private uint unkSymbolListCount;
        [MinVersion(0), MaxVersion(13)]
        public List<Symbol> unkSymbolList = new();

        private uint unkStrings1Count;
        [MinVersion(5), MaxVersion(23)]
        public List<string> unkStrings1 = new();
        private uint unkStrings2Count;
        [MinVersion(5), MaxVersion(23)]
        public List<string> unkStrings2 = new();

        public CharClipSet Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint charClipSampleCount = 0;
            // go through all entries in the parent and count how many CharClipSamples there are
            foreach (DirectoryMeta.Entry parentEntry in parent.entries)
            {
                if (parentEntry.type.value == "CharClipSample") charClipSampleCount++;
            }


            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            base.Read(reader, false, parent, entry);

            if (revision < 0x11)
            {
                unkInt1 = reader.ReadInt32();
                unkInt2 = reader.ReadInt32();
            }

            if (revision == 0xF || revision == 0x10)
            {
                unkInt3 = reader.ReadInt32();
            }

            if (revision < 9)
                graphPath = Symbol.Read(reader);

            if (revision < 6)
                unkString = reader.ReadUTF8();

            if (revision < 7)
                unkInt4 = reader.ReadInt32();

            // they do not store this in the asset itself, its calculated based on the number of CharClipSamples assets in the dir? weird shit, why HMX?
            if (revision < 0x18)
            {
                for (int i = 0; i < charClipSampleCount; i++)
                {
                    CharClipPtr ptr = new();
                    ptr.Read(reader);
                    charClipPtrs.Add(ptr);
                }
            }

            if (revision > 0xD)
            {
                if (revision < 0x18)
                {
                    unkBool1 = reader.ReadBoolean();
                    if (revision > 0x12)
                        unkBool2 = reader.ReadBoolean();
                }
            }
            else
            {
                unkSymbolListCount = reader.ReadUInt32();
                for (int i = 0; i < unkSymbolListCount; i++)
                {
                    unkSymbolList.Add(Symbol.Read(reader));
                }
            }

            if (revision >= 5 && revision <= 0x17)
            {
                unkStrings1Count = reader.ReadUInt32();
                for (int i = 0; i < unkStrings1Count; i++)
                {
                    unkStrings1.Add(reader.ReadUTF8());
                }

                unkStrings2Count = reader.ReadUInt32();
                for (int i = 0; i < unkStrings2Count; i++)
                {
                    unkStrings2.Add(reader.ReadUTF8());
                }
                unkBool3 = reader.ReadBoolean();
            }

            if (revision >= 10 && revision <= 23)
            {
                unknownSymbol2 = Symbol.Read(reader);
                unkInt5 = reader.ReadInt32();
            }

            if (revision == 0xB)
            {
                unkBool4 = reader.ReadBoolean();
            }



            if (revision > 0x11)
            {
                charFilePath = Symbol.Read(reader);
                previewClip = Symbol.Read(reader);
            }
            if (revision > 0x13)
                filterFlags = reader.ReadUInt32();
            if (revision > 0x14)
                bpm = reader.ReadUInt32();
            if (revision > 0x15)
                previewWalk = reader.ReadBoolean();
            if (revision > 0x16)
                stillClip = Symbol.Read(reader);

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw new Exception("Got to end of standalone asset but didn't find the expected end bytes, read likely did not succeed");

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            base.Write(writer, false, parent, entry);

            if (revision < 0x11)
            {
                writer.WriteInt32(unkInt1);
                writer.WriteInt32(unkInt2);
            }

            if (revision == 0xF || revision == 0x10)
            {
                writer.WriteInt32(unkInt3);
            }

            if (revision < 9)
                Symbol.Write(writer, graphPath);

            if (revision < 6)
                writer.WriteUTF8(unkString);

            if (revision < 7)
                writer.WriteInt32(unkInt4);

            if (revision < 0x18)
            {
                foreach (CharClipPtr ptr in charClipPtrs)
                {
                    ptr.Write(writer);
                }
            }

            if (revision > 0xD)
            {
                if (revision < 0x18)
                {
                    writer.WriteBoolean(unkBool1);
                    if (revision > 0x12)
                        writer.WriteBoolean(unkBool2);
                }
            }
            else
            {
                writer.WriteUInt32(unkSymbolListCount);
                foreach (Symbol symbol in unkSymbolList)
                {
                    Symbol.Write(writer, symbol);
                }
            }

            if (revision >= 5 && revision <= 0x17)
            {
                writer.WriteUInt32(unkStrings1Count);
                foreach (string str in unkStrings1)
                {
                    writer.WriteUTF8(str);
                }

                writer.WriteUInt32(unkStrings2Count);
                foreach (string str in unkStrings2)
                {
                    writer.WriteUTF8(str);
                }
                writer.WriteBoolean(unkBool3);
            }

            if (revision >= 10 && revision <= 23)
            {
                Symbol.Write(writer, unknownSymbol2);
                writer.WriteInt32(unkInt5);
            }

            if (revision == 0xB)
            {
                writer.WriteBoolean(unkBool4);
            }

            if (revision > 0x11)
            {
                Symbol.Write(writer, charFilePath);
                Symbol.Write(writer, previewClip);
            }
            if (revision > 0x13)
                writer.WriteUInt32(filterFlags);
            if (revision > 0x14)
                writer.WriteUInt32(bpm);
            if (revision > 0x15)
                writer.WriteBoolean(previewWalk);
            if (revision > 0x16)
                Symbol.Write(writer, stillClip);

            if (standalone)
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
        }

        public override bool IsDirectory()
        {
            return true;
        }
    }
}
