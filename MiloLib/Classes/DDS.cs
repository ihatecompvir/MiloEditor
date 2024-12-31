using MiloLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiloLib.Classes
{
    /// <summary>
    /// Represents a DirectDraw Surface (DDS) file.
    /// </summary>
    public class DDS
    {
        // DDS header fields
        public uint dwMagic;
        public uint dwSize;
        public uint dwFlags;
        public uint dwHeight;
        public uint dwWidth;
        public uint dwPitchOrLinearSize;
        public uint dwDepth;
        public uint dwMipMapCount;
        public uint[] dwReserved1 = new uint[11];
        public uint[] dwReserved2 = new uint[4];

        // pixel format fields
        public PixelFormat pf = new PixelFormat();

        public List<byte> pixels = new List<byte>();


        public struct PixelFormat
        {
            public uint dwSize;
            public uint dwFlags;
            public uint dwFourCC;
            public uint dwRGBBitCount;
            public uint dwRBitMask;
            public uint dwGBitMask;
            public uint dwBBitMask;
            public uint dwABitMask;
        }



        public DDS Read(EndianReader reader)
        {
            DDS dds = new DDS();
            dds.dwMagic = reader.ReadUInt32();
            dds.dwSize = reader.ReadUInt32();
            dds.dwFlags = reader.ReadUInt32();
            dds.dwHeight = reader.ReadUInt32();
            dds.dwWidth = reader.ReadUInt32();
            dds.dwPitchOrLinearSize = reader.ReadUInt32();
            dds.dwDepth = reader.ReadUInt32();
            dds.dwMipMapCount = reader.ReadUInt32();
            for (int i = 0; i < dds.dwReserved1.Length; i++)
            {
                dds.dwReserved1[i] = reader.ReadUInt32();
            }

            dds.pf.dwSize = reader.ReadUInt32();
            dds.pf.dwFlags = reader.ReadUInt32();
            dds.pf.dwFourCC = reader.ReadUInt32();
            dds.pf.dwRGBBitCount = reader.ReadUInt32();
            dds.pf.dwRBitMask = reader.ReadUInt32();
            dds.pf.dwGBitMask = reader.ReadUInt32();
            dds.pf.dwBBitMask = reader.ReadUInt32();
            dds.pf.dwABitMask = reader.ReadUInt32();
            for (int i = 0; i < dds.dwReserved2.Length; i++)
            {
                dds.dwReserved2[i] = reader.ReadUInt32();
            }

            reader.BaseStream.Position += 4;

            // read the rest of the file into pixels using ReadByte one at a time
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                dds.pixels.Add(reader.ReadByte());
            }
            return dds;
        }
    }
}