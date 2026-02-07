using MiloLib.Assets.Rnd;
using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets.UI
{
    [Name("UIList"), Description("Component for displaying 1- or 2-dimensional lists of data. Can be oriented horizontally or vertically, can scroll normally or circularly, and can have any number of visible elements (even just one, a.k.a. a spin button).")]
    public class UIList : UIComponent
    {
        private ushort altRevision;
        private ushort revision;


        [MaxVersion(0xE)]
        public int i;
        [MaxVersion(0xE)]
        public int x;
        [MaxVersion(0xE)]
        public int j;
        [MinVersion(5), MaxVersion(5)]
        public int k;
        [MinVersion(9), MaxVersion(0xE)]
        public bool ba;
        [MinVersion(7), MaxVersion(0xE)]
        public bool b9;
        public bool b8;

        [Name("Number to Display"), Description("Number of rows/columns")]
        public int numDisplay;
        [Name("Grid Span"), Description("Number across for a grid."), MinVersion(0x12)]
        public int gridSpan;

        [Name("Circular"), Description("Does the list scrolling wrap?")]
        public bool circular;
        [Name("Speed"), Description("Time (seconds) to scroll one step - 0 for instant scrolling")]
        public float speed;
        [Name("Scroll Past Min"), Description("Allow selected data to move beyond min highlight?"), MinVersion(0xD)]
        public bool scrollPastMin;
        [Name("Scroll Past Max"), Description("Allow selected data to move beyond max highlight?"), MinVersion(8)]
        public bool scrollPastMax;
        [Name("Paginate"), Description("Allow scrolling by pages?"), MinVersion(3)]
        public bool paginate;

        [Name("Select to Scroll"), Description("Does list need to be selected before user can scroll?"), MaxVersion(4)]
        public bool selectToScroll;


        [Name("Min Display"), Description("How far from top of list to start scrolling"), MinVersion(10)]
        public int minDisplay;
        [Name("Max Display"), Description("How far down can the highlight travel before scoll? Use -1 for no limit"), MinVersion(6)]
        public int maxDisplay;

        public int unk1;
        public int unk2;
        [MaxVersion(0xE)]
        public int unk3;

        [Name("Number of Data"), Description("Num data to show (only for milo)"), MinVersion(12)]
        public int numData;
        [Name("Auto Scroll Pause"), Description("Time to pause when auto scroll changes directions (seconds)"), MinVersion(14)]
        public float autoScrollPause;

        [Name("Auto Scroll Send Messages"), Description("Should this list send UIComponentScroll* messages while auto-scrolling?"), MinVersion(19)]
        public bool autoScrollSendMessages;

        private uint extendedLabelEntriesCount;
        [Name("Extended Label Entries"), Description("labels to be filled in by list provider at runtime"), MinVersion(0x10)]
        public List<Symbol> extendedLabelEntries = new();
        private uint extendedMeshEntriesCount;
        [Name("Extended Mesh Entries"), Description("meshes to be filled in by list provider at runtime"), MinVersion(0x10)]
        public List<Symbol> extendedMeshEntries = new();
        private uint extendedCustomEntriesCount;
        [Name("Extended Custom Entries"), Description("custom objects to be filled in by list provider at runtime"), MinVersion(0x10)]
        public List<Symbol> extendedCustomEntries = new();

        [Name("In Anim"), Description("animation kicked off before extended entries are updated"), MinVersion(0x11)]
        public Symbol inAnim = new(0, "");
        [Name("Out Anim"), Description("animation kicked off after extended entries are updated"), MinVersion(0x11)]
        public Symbol outAnim = new(0, "");


        public UIList Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            base.Read(reader, false, parent, entry);

            if (revision < 0xF)
            {
                i = reader.ReadInt32();
                j = reader.ReadInt32();
                if (revision > 4)
                {
                    if (revision > 6)
                    {
                        k = reader.ReadInt32();
                    }
                    else
                    {
                        b8 = reader.ReadBoolean();
                    }
                }
                if (revision > 6)
                    b9 = reader.ReadBoolean();
                if (revision > 8)
                    ba = reader.ReadBoolean();
                if (revision > 10)
                    unk3 = reader.ReadInt32();
                x = reader.ReadInt32();
            }

            numDisplay = reader.ReadInt32();
            if (revision > 0x11)
                gridSpan = reader.ReadInt32();
            circular = reader.ReadBoolean();
            speed = reader.ReadFloat();
            if (revision > 0xC)
                scrollPastMin = reader.ReadBoolean();
            if (revision > 7)
                scrollPastMax = reader.ReadBoolean();
            if (revision > 2)
                paginate = reader.ReadBoolean();
            if (revision > 3)
                selectToScroll = reader.ReadBoolean();
            if (revision >= 10)
                minDisplay = reader.ReadInt32();
            if (revision >= 6)
                maxDisplay = reader.ReadInt32();
            if (revision == 1)
            {
                unk1 = reader.ReadInt32();
                unk2 = reader.ReadInt32();
            }
            if (revision >= 12)
                numData = reader.ReadInt32();
            if (revision >= 14)
                autoScrollPause = reader.ReadFloat();
            if (revision >= 19)
                autoScrollSendMessages = reader.ReadBoolean();
            if (revision >= 0x10)
            {
                extendedLabelEntriesCount = reader.ReadUInt32();
                for (int i = 0; i < extendedLabelEntriesCount; i++)
                {
                    extendedLabelEntries.Add(Symbol.Read(reader));
                }
                extendedMeshEntriesCount = reader.ReadUInt32();
                for (int i = 0; i < extendedMeshEntriesCount; i++)
                {
                    extendedMeshEntries.Add(Symbol.Read(reader));
                }
                extendedCustomEntriesCount = reader.ReadUInt32();
                for (int i = 0; i < extendedCustomEntriesCount; i++)
                {
                    extendedCustomEntries.Add(Symbol.Read(reader));
                }
            }
            if (revision >= 17)
            {
                inAnim = Symbol.Read(reader);
                outAnim = Symbol.Read(reader);
            }

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw MiloLib.Exceptions.MiloAssetReadException.EndBytesNotFound(parent, entry, reader.BaseStream.Position);

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            base.Write(writer, false, parent, entry);

            if (revision < 0xF)
            {
                writer.WriteInt32(i);
                writer.WriteInt32(j);
                if (revision > 4)
                {
                    if (revision > 6)
                    {
                        writer.WriteInt32(k);
                    }
                    else
                    {
                        writer.WriteBoolean(b8);
                    }
                }
                if (revision > 6)
                    writer.WriteBoolean(b9);
                if (revision > 8)
                    writer.WriteBoolean(ba);
                if (revision > 10)
                    writer.WriteInt32(unk3);
                writer.WriteInt32(x);
            }

            writer.WriteInt32(numDisplay);
            if (revision > 0x11)
                writer.WriteInt32(gridSpan);
            writer.WriteBoolean(circular);
            writer.WriteFloat(speed);
            if (revision > 0xC)
                writer.WriteBoolean(scrollPastMin);
            if (revision > 7)
                writer.WriteBoolean(scrollPastMax);
            if (revision > 2)
                writer.WriteBoolean(paginate);
            if (revision > 3)
                writer.WriteBoolean(selectToScroll);
            if (revision >= 10)
                writer.WriteInt32(minDisplay);
            if (revision >= 6)
                writer.WriteInt32(maxDisplay);
            if (revision == 1)
            {
                writer.WriteInt32(unk1);
                writer.WriteInt32(unk2);
            }
            if (revision >= 12)
                writer.WriteInt32(numData);
            if (revision >= 14)
                writer.WriteFloat(autoScrollPause);
            if (revision >= 19)
                writer.WriteBoolean(autoScrollSendMessages);
            if (revision >= 0x10)
            {
                writer.WriteUInt32((uint)extendedLabelEntries.Count);
                foreach (var sym in extendedLabelEntries)
                {
                    Symbol.Write(writer, sym);
                }
                writer.WriteUInt32((uint)extendedMeshEntries.Count);
                foreach (var sym in extendedMeshEntries)
                {
                    Symbol.Write(writer, sym);
                }
                writer.WriteUInt32((uint)extendedCustomEntries.Count);
                foreach (var sym in extendedCustomEntries)
                {
                    Symbol.Write(writer, sym);
                }
            }
            if (revision >= 17)
            {
                Symbol.Write(writer, inAnim);
                Symbol.Write(writer, outAnim);
            }

            if (standalone)
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
        }

    }
}
