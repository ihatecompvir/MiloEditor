using MiloLib.Utils;
using MiloLib.Classes;

namespace MiloLib.Assets.Rnd
{

    // this is a misleading description from Harmonix lol, I guess I could add this though so it is no longer false
    [Name("Tex"), Description("Tex perObjs represent bitmaps used by materials. These can be created automatically with 'import overrideMap' on the file menu.")]
    public class RndTex : Object
    {
        public enum Type
        {
            kRegular = 1,
            kRendered = 2,
            kMovie = 4,
            kBackBuffer = 8,
            kFrontBuffer = 0x18,
            kRenderedNoZ = 0x22,
            kShadowMap = 0x42,
            kDepthVolumeMap = 0xA2,
            kDensityMap = 0x122,
            kScratch = 0x200,
            kDeviceTexture = 0x1000
        };

        private ushort altRevision;
        private ushort revision;

        [Name("Width"), Description("Width of the texture in pixels.")]
        public uint width;
        [Name("Height"), Description("Height of the texture in pixels.")]
        public uint height;

        [Name("BPP"), Description("Bits per pixel.")]
        public uint bpp;

        [Name("External Path"), Description("Path to the texture to be loaded externally.")]
        public Symbol externalPath = new(0, "");

        [MinVersion(8)]
        public float mipMapK;
        public Type type;

        [MinVersion(11)]
        public bool optimizeForPS3;

        [Name("Use External Path"), Description("Whether or not to use the external path.")]
        public bool useExternalPath;

        [Name("Bitmap"), Description("The bitmap data.")]
        public RndBitmap bitmap = new();

        public ushort unkShort;

        public uint unkInt;
        public uint unkInt2;
        public ushort unkShort2;

        public bool isRegular;



        public RndTex Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            if (revision > 8)
                base.Read(reader, false, parent, entry);

            width = reader.ReadUInt32();
            height = reader.ReadUInt32();

            bpp = reader.ReadUInt32();

            // lego gotta be special
            if (parent.revision == 25 && revision == 11)
            {
                reader.ReadUInt32();
            }

            externalPath = Symbol.Read(reader);

            if (revision >= 8)
                mipMapK = reader.ReadFloat();

            if (revision > 6)
            {
                type = (Type)reader.ReadUInt32();
            }
            else if (revision > 5)
            {
                type = (Type)reader.ReadUInt32();
            }
            else if (revision > 4)
            {
                isRegular = reader.ReadBoolean();
            }

            if (revision >= 11 && parent.revision != 25)
                optimizeForPS3 = reader.ReadBoolean();

            if (revision != 7)
                useExternalPath = reader.ReadBoolean();
            else
                useExternalPath = reader.ReadUInt32() == 1;

            if (revision == 5)
            {
                if (standalone)
                {
                    if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw MiloLib.Exceptions.MiloAssetReadException.EndBytesNotFound(parent, entry, reader.BaseStream.Position);
                }
                return this;
            }


            if (parent.platform == DirectoryMeta.Platform.Wii && revision > 10)
                unkShort = reader.ReadUInt16();


            // bitmaps are stored as Little endian on Wii? wack
            Endian origEndian = reader.Endianness;

            if (parent.platform == DirectoryMeta.Platform.Wii && revision > 10)
                reader.Endianness = Endian.LittleEndian;

            bitmap = new RndBitmap().Read(reader, false, parent, entry);

            reader.Endianness = origEndian;

            if (parent.platform == DirectoryMeta.Platform.Wii && revision > 10)
            {
                unkInt = reader.ReadUInt32();
                unkInt2 = reader.ReadUInt32();
                unkShort2 = reader.ReadUInt16();
            }

            if (standalone)
            {
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw MiloLib.Exceptions.MiloAssetReadException.EndBytesNotFound(parent, entry, reader.BaseStream.Position);
            }

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            if (revision > 8)
                base.Write(writer, false, parent, entry);

            writer.WriteUInt32(width);
            writer.WriteUInt32(height);

            writer.WriteUInt32(bpp);

            Symbol.Write(writer, externalPath);

            if (revision >= 8)
                writer.WriteFloat(mipMapK);
            if (revision > 6)
            {
                writer.WriteUInt32((uint)type);
            }
            else if (revision > 5)
            {
                writer.WriteUInt32((uint)type);
            }
            else if (revision > 4)
            {
                writer.WriteBoolean(isRegular);
            }

            if (revision >= 11)
                writer.WriteBoolean(optimizeForPS3);

            if (revision != 7)
                writer.WriteBoolean(useExternalPath);
            else
                writer.WriteUInt32(useExternalPath ? 1u : 0u);

            if (parent.platform == DirectoryMeta.Platform.Wii && altRevision == 1)
                writer.WriteUInt16(unkShort);

            Endian origEndian = writer.Endianness;

            if (parent.platform == DirectoryMeta.Platform.Wii && revision > 10)
                writer.Endianness = Endian.LittleEndian;

            bitmap.Write(writer, false, parent, entry);

            writer.Endianness = origEndian;

            if (parent.platform == DirectoryMeta.Platform.Wii && revision > 10)
            {
                writer.WriteUInt32(unkInt);
                writer.WriteUInt32(unkInt2);
                writer.WriteUInt16(unkShort2);
            }

            if (standalone)
            {
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
            }

        }

        public static RndTex New(ushort revision, ushort altRevision)
        {
            RndTex newRndTex = new RndTex();
            newRndTex.revision = revision;
            newRndTex.altRevision = altRevision;
            return newRndTex;
        }

    }
}
