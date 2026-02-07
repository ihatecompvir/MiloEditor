using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets.Char
{
    [Name("CharMeshHide"), Description("")]
    public class CharMeshHide : Object
    {
        public class Hide
        {
            public Symbol draw = new(0, "");
            public int flags;
            public bool show;

            public Hide Read(EndianReader reader, uint revision)
            {
                draw = Symbol.Read(reader);
                flags = reader.ReadInt32();
                if (revision > 1)
                    show = reader.ReadBoolean();
                return this;
            }

            public void Write(EndianWriter writer, uint revision)
            {
                Symbol.Write(writer, draw);
                writer.WriteInt32(flags);
                if (revision > 1)
                    writer.WriteBoolean(show);
            }

            public override string ToString()
            {
                return $"{draw} flags: {flags} showing: {show}";
            }
        }

        [Flags]
        public enum HideOptions
        {
            None = 0,
            HideLongCoat = 1,
            HideLongDress = 2,
            HideLongSleeve = 4,
            HideMidSleeve = 8,
            HideFullSleeve = 16,
            HideHead = 32,
            HideLongGlove = 64,
            HideMask = 128,
            HideLongBoot = 256,
            HideShortSleeve = 512,
            HideShortBoot = 1024,
            HideLongPants = 2048,
            HideGlasses = 4096,
            HideVignette = 8192,
            HideSocks = 16384
        }

        private void SetFromFlags(int flags)
        {
            var options = (HideOptions)flags;

            hideLongCoat = options.HasFlag(HideOptions.HideLongCoat);
            hideLongDress = options.HasFlag(HideOptions.HideLongDress);
            hideLongSleeve = options.HasFlag(HideOptions.HideLongSleeve);
            hideMidSleeve = options.HasFlag(HideOptions.HideMidSleeve);
            hideFullSleeve = options.HasFlag(HideOptions.HideFullSleeve);
            hideHead = options.HasFlag(HideOptions.HideHead);
            hideLongGlove = options.HasFlag(HideOptions.HideLongGlove);
            hideMask = options.HasFlag(HideOptions.HideMask);
            hideLongBoot = options.HasFlag(HideOptions.HideLongBoot);
            hideShortSleeve = options.HasFlag(HideOptions.HideShortSleeve);
            hideShortBoot = options.HasFlag(HideOptions.HideShortBoot);
            hideLongPants = options.HasFlag(HideOptions.HideLongPants);
            hideGlasses = options.HasFlag(HideOptions.HideGlasses);
            hideVignette = options.HasFlag(HideOptions.HideVignette);
            hideSocks = options.HasFlag(HideOptions.HideSocks);
        }

        private int GetFlags()
        {
            var options = HideOptions.None;

            if (hideLongCoat) options |= HideOptions.HideLongCoat;
            if (hideLongDress) options |= HideOptions.HideLongDress;
            if (hideLongSleeve) options |= HideOptions.HideLongSleeve;
            if (hideMidSleeve) options |= HideOptions.HideMidSleeve;
            if (hideFullSleeve) options |= HideOptions.HideFullSleeve;
            if (hideHead) options |= HideOptions.HideHead;
            if (hideLongGlove) options |= HideOptions.HideLongGlove;
            if (hideMask) options |= HideOptions.HideMask;
            if (hideLongBoot) options |= HideOptions.HideLongBoot;
            if (hideShortSleeve) options |= HideOptions.HideShortSleeve;
            if (hideShortBoot) options |= HideOptions.HideShortBoot;
            if (hideLongPants) options |= HideOptions.HideLongPants;
            if (hideGlasses) options |= HideOptions.HideGlasses;
            if (hideVignette) options |= HideOptions.HideVignette;
            if (hideSocks) options |= HideOptions.HideSocks;

            return (int)options;
        }


        private ushort altRevision;
        private ushort revision;

        private uint hidesCount;
        public List<Hide> hides = new();

        public int flags;


        [Name("Hide Long Coat")]
        public bool hideLongCoat;
        [Name("Hide Long Dress")]
        public bool hideLongDress;
        [Name("Hide Long Sleeve")]
        public bool hideLongSleeve;
        [Name("Hide Mid Sleeve")]
        public bool hideMidSleeve;
        [Name("Hide Full Sleeve")]
        public bool hideFullSleeve;
        [Name("Hide Head"), Description("Means the whole head should be hidden, like for a horse head mask or something crazy")]
        public bool hideHead;
        [Name("Hide Long Glove")]
        public bool hideLongGlove;
        [Name("Hide Mask")]
        public bool hideMask;
        [Name("Hide Long Boot")]
        public bool hideLongBoot;
        [Name("Hide Short Sleeve")]
        public bool hideShortSleeve;
        [Name("Hide Short Boot")]
        public bool hideShortBoot;
        [Name("Hide Long Pants")]
        public bool hideLongPants;
        [Name("Hide Glasses")]
        public bool hideGlasses;
        [Name("Hide in Vignette"), Description("Means geometry is too big for vignettes, but closet and game should be fine")]
        public bool hideVignette;
        [Name("Hide Socks"), Description("Means legs include socks, so show socked foot geometry rather than barefoot geometry")]
        public bool hideSocks;

        public CharMeshHide Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)(combinedRevision >> 16 & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)(combinedRevision >> 16 & 0xFFFF));

            base.Read(reader, false, parent, entry);

            flags = reader.ReadInt32();
            SetFromFlags(flags);

            hidesCount = reader.ReadUInt32();
            for (int i = 0; i < hidesCount; i++)
                hides.Add(new Hide().Read(reader, revision));

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw MiloLib.Exceptions.MiloAssetReadException.EndBytesNotFound(parent, entry, reader.BaseStream.Position);

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)(altRevision << 16 | revision) : (uint)(revision << 16 | altRevision));

            base.Write(writer, false, parent, entry);

            int flags = GetFlags();
            writer.WriteInt32(flags);
            
            hidesCount = (uint)hides.Count;
            writer.WriteUInt32(hidesCount);
            foreach (var hide in hides)
                hide.Write(writer, revision);

            if (standalone)
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
        }

    }
}
