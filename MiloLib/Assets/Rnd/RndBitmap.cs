using MiloLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiloLib.Classes;

namespace MiloLib.Assets.Rnd
{
    [Name("Bitmap"), Description("Represents a bitmap.")]
    public class RndBitmap
    {

        public enum Encoding
        {
            ARGB = 1,
            RGBA = 3,
            DXT1_BC1 = 8,
            DXT5_BC3 = 24,
            ATI2_BC5 = 32,
            TPL_CMP = 72,
            TPL_CMP_ALPHA = 328,
            TPL_CMP_2 = 583
        }
        public byte revision;

        public byte bpp;

        public Encoding encoding;

        public byte mipMaps;

        public ushort width;
        public ushort height;

        public ushort bpl;

        public ushort wiiAlphaNum;

        public List<List<byte>> textures = new List<List<byte>>();


        public RndBitmap Read(EndianReader reader, bool standalone)
        {
            revision = reader.ReadByte();

            if (revision != 1)
            {
                throw new UnsupportedAssetRevisionException("RndBitmap", revision);
            }

            bpp = reader.ReadByte();

            encoding = (Encoding)reader.ReadUInt32();

            mipMaps = reader.ReadByte();

            width = reader.ReadUInt16();
            height = reader.ReadUInt16();

            bpl = reader.ReadUInt16();

            wiiAlphaNum = reader.ReadUInt16();

            // skip empty bytes
            reader.BaseStream.Position += 17;

            for (int i = 0; i < mipMaps + 1; i++)
            {
                // this algo needs to scale down each mip map since only the first one will be the original size
                int mippedHeight = height >> i;
                int mippedWidth = width >> i;
                int dataSize = (mippedWidth * mippedHeight * bpp) / 8;


                List<byte> texture = new List<byte>();
                for (int j = 0; j < dataSize; j++)
                {
                    texture.Add(reader.ReadByte());
                }

                textures.Add(texture);
            }

            return this;
        }

        public void Write(EndianWriter writer, bool standalone)
        {
            writer.WriteByte(revision);

            writer.WriteByte(bpp);

            writer.WriteUInt32((uint)encoding);

            writer.WriteByte(mipMaps);

            writer.WriteUInt16(width);
            writer.WriteUInt16(height);

            writer.WriteUInt16(bpl);

            writer.WriteUInt16(wiiAlphaNum);

            // write 17 empty bytes
            writer.WriteBlock(new byte[17]);

            foreach (List<byte> texture in textures)
            {
                foreach (byte pixel in texture)
                {
                    writer.WriteByte(pixel);
                }
            }
        }

        public override string ToString()
        {
            return $"Width: {width}, Height: {height}";
        }
    }
}
