using MiloLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiloLib.Assets.Rnd
{
    public class Vertex
    {
        // Basic fields
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
        public float w { get; set; } = 0.0f;

        // Normals
        public float nx { get; set; } = 0.0f;
        public float ny { get; set; } = 0.0f;
        public float nz { get; set; } = 0.0f;
        public float nw { get; set; } = 0.0f;

        // UVs
        public float u { get; set; } = 0.0f;
        public float v { get; set; } = 0.0f;
        public ushort halfU { get; set; } = 0;
        public ushort halfV { get; set; } = 0;

        // Weights
        public float weight0 { get; set; } = 0.0f;
        public float weight1 { get; set; } = 0.0f;
        public float weight2 { get; set; } = 0.0f;
        public float weight3 { get; set; } = 0.0f;
        public byte weightU0 { get; set; } = 0;
        public byte weightU1 { get; set; } = 0;
        public byte weightU2 { get; set; } = 0;
        public byte weightU3 { get; set; } = 0;

        // Bone indices
        public ushort bone0 { get; set; } = 0;
        public ushort bone1 { get; set; } = 0;
        public ushort bone2 { get; set; } = 0;
        public ushort bone3 { get; set; } = 0;
        public byte boneU0 { get; set; } = 0;
        public byte boneU1 { get; set; } = 0;
        public byte boneU2 { get; set; } = 0;
        public byte boneU3 { get; set; } = 0;

        // Tangents
        public float tangent0 { get; set; } = 0.0f;
        public float tangent1 { get; set; } = 0.0f;
        public float tangent2 { get; set; } = 0.0f;
        public float tangent3 { get; set; } = 0.0f;

        // Packed data
        public uint packed1 { get; set; } = 0;
        public uint packed2 { get; set; } = 0;
        public uint packed3 { get; set; } = 0;
        public uint packed4 { get; set; } = 0;

        // Special fields
        public float unknown1 { get; set; } = 0.0f;
        public float unknown2 { get; set; } = 0.0f;
        public float neg1 { get; set; } = 0.0f;
        public float pos1 { get; set; } = 0.0f;

        public int uvCheck { get; set; }

        public QTangent qTangents { get; set; } = new();

        // Compressed data
        public SignedCompressedVec4 normals { get; set; } = new SignedCompressedVec4();
        public SignedCompressedVec4 tangents { get; set; } = new SignedCompressedVec4();
        public UnsignedCompressedVec4 weights { get; set; } = new UnsignedCompressedVec4();

        public class SignedCompressedVec4
        {
            public uint origValue { get; set; }
            public float x { get; set; } = 0.0f;
            public float y { get; set; } = 0.0f;
            public float z { get; set; } = 0.0f;
            public float w { get; set; } = 0.0f;

            public SignedCompressedVec4 Read(EndianReader reader)
            {
                uint value = reader.ReadUInt32();
                origValue = value;

                const int MAX_2_BIT_SIGNED = (1 << 1) - 1; // 1
                const int MASK_2_BIT = (1 << 2) - 1; // 3
                const int MAX_10_BIT_SIGNED = (1 << 9) - 1; // 511
                const int MASK_10_BIT = (1 << 10) - 1; // 1023

                int w_bits = (int)((value >> 30) & MASK_2_BIT);
                int z_bits = (int)((value >> 20) & MASK_10_BIT);
                int y_bits = (int)((value >> 10) & MASK_10_BIT);
                int x_bits = (int)(value & MASK_10_BIT);

                if (x_bits > MAX_10_BIT_SIGNED)
                    x_bits = -1 * (~(x_bits - 1) & (MASK_10_BIT >> 1));
                if (y_bits > MAX_10_BIT_SIGNED)
                    y_bits = -1 * (~(y_bits - 1) & (MASK_10_BIT >> 1));
                if (z_bits > MAX_10_BIT_SIGNED)
                    z_bits = -1 * (~(z_bits - 1) & (MASK_10_BIT >> 1));

                if (w_bits > MAX_2_BIT_SIGNED)
                    w_bits = -1 * (~(w_bits - 1) & (MASK_2_BIT >> 1));

                x = Math.Max((float)x_bits / (float)MAX_10_BIT_SIGNED, -1.0f);
                y = Math.Max((float)y_bits / (float)MAX_10_BIT_SIGNED, -1.0f);
                z = Math.Max((float)z_bits / (float)MAX_10_BIT_SIGNED, -1.0f);
                w = Math.Max((float)w_bits / (float)MAX_2_BIT_SIGNED, -1.0f);

                return this;
            }

            private int Compress10Bit(float f)
            {
                f = Math.Clamp(f, -1.0f, 1.0f);
                int bits = (int)Math.Round(f * 511);
                if (bits < 0) bits = 1024 + bits;
                return bits & 0x3FF;
            }

            private int Compress2Bit(float f)
            {
                f = Math.Clamp(f, -1.0f, 1.0f);
                int bits = (int)Math.Round(f);
                if (bits < 0) bits = 4 + bits;
                return bits & 0x3;
            }

            public void Write(EndianWriter writer)
            {

                int xBits = Compress10Bit(x);
                int yBits = Compress10Bit(y);
                int zBits = Compress10Bit(z);
                int wBits = Compress2Bit(w);

                uint value = (uint)xBits
                             | (uint)(yBits << 10)
                             | (uint)(zBits << 20)
                             | (uint)(wBits << 30);

                //writer.WriteUInt32(value);
                writer.WriteUInt32(origValue);
            }
        }

        public class UnsignedCompressedVec4
        {
            public float x { get; set; } = 0.0f;
            public float y { get; set; } = 0.0f;
            public float z { get; set; } = 0.0f;
            public float w { get; set; } = 0.0f; // Derived as 1.0 - (x + y + z)

            public UnsignedCompressedVec4 Read(EndianReader reader)
            {
                uint value = reader.ReadUInt32();

                const int MAX_2_BIT_UNSIGNED = (1 << 2) - 1;
                const int MASK_2_BIT = (1 << 2) - 1;
                const int MAX_10_BIT_UNSIGNED = (1 << 10) - 1;
                const int MASK_10_BIT = (1 << 10) - 1;

                uint w_bits = (value >> 30) & MASK_2_BIT;
                uint z_bits = (value >> 20) & MASK_10_BIT;
                uint y_bits = (value >> 10) & MASK_10_BIT;
                uint x_bits = value & MASK_10_BIT;

                x = (float)x_bits / MAX_10_BIT_UNSIGNED;
                y = (float)y_bits / MAX_10_BIT_UNSIGNED;
                z = (float)z_bits / MAX_10_BIT_UNSIGNED;
                w = (float)w_bits / MAX_2_BIT_UNSIGNED;

                return this;
            }

            public void Write(EndianWriter writer)
            {
                int xBits = (int)(x * 1023);
                int yBits = (int)(y * 1023);
                int zBits = (int)(z * 1023);
                int wBits = (int)(w * 3);

                uint value = (uint)xBits
                             | (uint)(yBits << 10)
                             | (uint)(zBits << 20)
                             | (uint)(wBits << 30);

                writer.WriteUInt32(value);
            }
        }

        public class QTangent
        {
            public SNorm x { get; set; } = new SNorm();
            public SNorm y { get; set; } = new SNorm();
            public SNorm z { get; set; } = new SNorm();
            public SNorm w { get; set; } = new SNorm();

            public class SNorm
            {
                public short value { get; set; }
                public float fValue => Math.Max((float)value / 32767.0f, -1.0f);
            }

            public QTangent Read(EndianReader reader)
            {
                x.value = reader.ReadInt16();
                y.value = reader.ReadInt16();
                z.value = reader.ReadInt16();
                w.value = reader.ReadInt16();

                return this;
            }

            public void Write(EndianWriter writer)
            {
                writer.WriteInt16(x.value);
                writer.WriteInt16(y.value);
                writer.WriteInt16(z.value);
                writer.WriteInt16(w.value);
            }
        }
    }
}
