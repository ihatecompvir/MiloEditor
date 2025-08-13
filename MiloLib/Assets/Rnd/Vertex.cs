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

        // Weights
        public float weight0 { get; set; } = 0.0f;
        public float weight1 { get; set; } = 0.0f;
        public float weight2 { get; set; } = 0.0f;
        public float weight3 { get; set; } = 0.0f;

        // Bone indices
        public ushort bone0 { get; set; } = 0;
        public ushort bone1 { get; set; } = 0;
        public ushort bone2 { get; set; } = 0;
        public ushort bone3 { get; set; } = 0;

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

        public override string ToString()
        {
            return $"<{x}, {y}, {z}>";
        }

        public class SignedCompressedVec4
        {
            public uint origValue { get; private set; }
            public float x { get; set; } = 0.0f;
            public float y { get; set; } = 0.0f;
            public float z { get; set; } = 0.0f;
            public float w { get; set; } = 0.0f;

            private static int ToSNormBits(float f, int n)
            {
                f = Math.Clamp(f, -1f, 1f);
                int max = (1 << (n - 1)) - 1;
                int s = (int)MathF.Truncate(f * max);
                if (s < 0) s += (1 << n);
                return s & ((1 << n) - 1);
            }

            private static float FromSNormBits(int bits, int n)
            {
                int max = (1 << (n - 1)) - 1;
                int s = bits;
                if (s > max) s -= (1 << n);
                return Math.Max(s / (float)max, -1f);
            }

            public SignedCompressedVec4 Read(EndianReader reader)
            {
                uint value = reader.ReadUInt32();
                origValue = value;

                int xb = (int)(value & 0x3FF);
                int yb = (int)((value >> 10) & 0x3FF);
                int zb = (int)((value >> 20) & 0x3FF);
                int wb = (int)((value >> 30) & 0x003);

                x = FromSNormBits(xb, 10);
                y = FromSNormBits(yb, 10);
                z = FromSNormBits(zb, 10);
                w = FromSNormBits(wb, 2);

                return this;
            }

            public void Write(EndianWriter writer)
            {
                int xBits = ToSNormBits(x, 10);
                int yBits = ToSNormBits(y, 10);
                int zBits = ToSNormBits(z, 10);
                int wBits = ToSNormBits(w, 2);

                uint value = (uint)xBits
                           | (uint)(yBits << 10)
                           | (uint)(zBits << 20)
                           | (uint)(wBits << 30);

                writer.WriteUInt32(value);
            }
        }

        public class UnsignedCompressedVec4
        {
            public float x { get; set; } = 0.0f;
            public float y { get; set; } = 0.0f;
            public float z { get; set; } = 0.0f;
            public float w { get; set; } = 0.0f;

            public UnsignedCompressedVec4 Read(EndianReader reader)
            {
                uint value = reader.ReadUInt32();

                uint xb = (value & 0x3FF);
                uint yb = ((value >> 10) & 0x3FF);
                uint zb = ((value >> 20) & 0x3FF);
                uint wb = ((value >> 30) & 0x003);

                x = xb / 1023f;
                y = yb / 1023f;
                z = zb / 1023f;
                w = wb / 3f;

                return this;
            }

            public void Write(EndianWriter writer)
            {
                int xBits = (int)MathF.Truncate(Math.Clamp(x, 0f, 1f) * 1023f) & 0x3FF;
                int yBits = (int)MathF.Truncate(Math.Clamp(y, 0f, 1f) * 1023f) & 0x3FF;
                int zBits = (int)MathF.Truncate(Math.Clamp(z, 0f, 1f) * 1023f) & 0x3FF;
                int wBits = (int)MathF.Truncate(Math.Clamp(w, 0f, 1f) * 3f) & 0x003;

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

                public SNorm() { }
                public SNorm(short value)
                {
                    this.value = value;
                }
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
