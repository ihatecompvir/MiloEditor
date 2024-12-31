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

        public enum TextureEncoding
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

        public TextureEncoding encoding;

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

            encoding = (TextureEncoding)reader.ReadUInt32();

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
                int dataSize = mippedWidth * mippedHeight * bpp / 8;


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

        /// <summary>
        /// Converts the bitmap to an image.
        /// </summary>
        /// <returns>The image, as a list of bytes. The actual format will depend on the encoding.</returns>
        public List<byte> ConvertToImage()
        {
            // check if the encoding is some form of DDS
            if (encoding == TextureEncoding.DXT1_BC1 || encoding == TextureEncoding.DXT5_BC3 || encoding == TextureEncoding.ATI2_BC5)
            {
                // build a DDS header based on the information in the bitmap, then append all the texture bytes
                List<byte> ddsData = new List<byte>();
                byte[] header = CreateDDSHeader();
                ddsData.AddRange(header);

                List<List<byte>> swappedBytes = new List<List<byte>>();

                // byte swap for 360
                foreach (List<byte> texture in textures)
                {
                    List<byte> swapped = new List<byte>();
                    for (int i = 0; i < texture.Count; i += 4)
                    {
                        swapped.Add(texture[i + 1]);
                        swapped.Add(texture[i]);
                        swapped.Add(texture[i + 3]);
                        swapped.Add(texture[i + 2]);
                    }
                    swappedBytes.Add(swapped);
                }

                // Append all miplevels
                foreach (List<byte> texture in swappedBytes)
                {
                    ddsData.AddRange(texture);
                }

                return ddsData;

            }
            return new();
        }

        private byte[] CreateDDSHeader()
        {
            using (MemoryStream stream = new MemoryStream())
            using (EndianWriter writer = new EndianWriter(stream, Endian.LittleEndian))
            {
                writer.WriteBlock(Encoding.ASCII.GetBytes("DDS "));
                writer.WriteUInt32(124);
                uint flags = 0x1 | 0x2 | 0x4 | 0x1000 | 0x80000;
                if (mipMaps > 0)
                {
                    flags |= 0x20000;
                }
                writer.WriteUInt32(flags);
                writer.WriteUInt32(height);
                writer.WriteUInt32(width);

                uint pitchOrLinearSize = 0;
                if (encoding == TextureEncoding.DXT1_BC1)
                {
                    pitchOrLinearSize = (uint)(Math.Max(1, ((width + 3) / 4)) * Math.Max(1, ((height + 3) / 4)) * 8);
                }
                else if (encoding == TextureEncoding.DXT5_BC3 || encoding == TextureEncoding.ATI2_BC5)
                {
                    pitchOrLinearSize = (uint)(Math.Max(1, ((width + 3) / 4)) * Math.Max(1, ((height + 3) / 4)) * 16);
                }
                else if (encoding == TextureEncoding.ARGB || encoding == TextureEncoding.RGBA)
                {
                    pitchOrLinearSize = (uint)(width * height * bpp / 8);
                }

                writer.WriteUInt32(pitchOrLinearSize);
                writer.WriteUInt32(0);
                writer.WriteUInt32((uint)(mipMaps > 0 ? mipMaps + 1 : 0));
                writer.WriteBlock(new byte[44]);

                writer.WriteUInt32(32);

                if (encoding == TextureEncoding.DXT1_BC1 || encoding == TextureEncoding.DXT5_BC3 || encoding == TextureEncoding.ATI2_BC5)
                {
                    writer.WriteUInt32(0x00000004);
                    if (encoding == TextureEncoding.DXT1_BC1)
                    {
                        writer.WriteBlock(Encoding.ASCII.GetBytes("DXT1"));
                    }
                    else if (encoding == TextureEncoding.DXT5_BC3)
                    {
                        writer.WriteBlock(Encoding.ASCII.GetBytes("DXT5"));
                    }
                    else if (encoding == TextureEncoding.ATI2_BC5)
                    {
                        writer.WriteBlock(Encoding.ASCII.GetBytes("ATI2"));
                    }
                    writer.WriteUInt32(0);
                    writer.WriteUInt32(0);
                    writer.WriteUInt32(0);
                    writer.WriteUInt32(0);
                    writer.WriteUInt32(0);
                }
                else if (encoding == TextureEncoding.ARGB || encoding == TextureEncoding.RGBA)
                {
                    writer.WriteUInt32(0x00000040);
                    writer.WriteUInt32(0);
                    writer.WriteUInt32((uint)bpp);
                    if (encoding == TextureEncoding.ARGB)
                    {
                        writer.WriteUInt32(0x00ff0000);
                        writer.WriteUInt32(0x0000ff00);
                        writer.WriteUInt32(0x000000ff);
                        writer.WriteUInt32(0xff000000);
                    }
                    else if (encoding == TextureEncoding.RGBA)
                    {
                        writer.WriteUInt32(0x00ff0000);
                        writer.WriteUInt32(0x0000ff00);
                        writer.WriteUInt32(0x000000ff);
                        writer.WriteUInt32(0xff000000);
                    }
                }

                writer.WriteUInt32(0x00000008);
                writer.WriteUInt32(0);
                writer.WriteUInt32(0);
                writer.WriteUInt32(0);
                writer.WriteUInt32(0);

                return stream.ToArray();
            }
        }


    }
}
