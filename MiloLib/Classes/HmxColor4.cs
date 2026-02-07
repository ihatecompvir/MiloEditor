using MiloLib.Utils;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MiloLib.Classes
{
    /// <summary>
    /// Represents a color with red, green, blue, and alpha components.
    /// </summary>
    public class HmxColor4
    {
        public float r;
        public float g;
        public float b;
        public float a;

        public HmxColor4()
        {
            this.r = 1.0f;
            this.g = 1.0f;
            this.b = 1.0f;
            this.a = 1.0f;
        }

        public HmxColor4(float r, float g, float b, float a)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }

        public HmxColor4 Read(EndianReader reader)
        {
            Span<byte> buffer = stackalloc byte[16];

            reader.ReadBlock(buffer);

            Span<float> floats = stackalloc float[4];

            if (reader.Endianness == Endian.LittleEndian && BitConverter.IsLittleEndian)
            {
                MemoryMarshal.Cast<byte, float>(buffer).CopyTo(floats);
            }
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    floats[i] = reader.Endianness == Endian.BigEndian
                        ? BinaryPrimitives.ReadSingleBigEndian(buffer.Slice(i * 4, 4))
                        : BinaryPrimitives.ReadSingleLittleEndian(buffer.Slice(i * 4, 4));
                }
            }

            r = floats[0];
            g = floats[1];
            b = floats[2];
            a = floats[3];

            return this;
        }

        public void Write(EndianWriter writer)
        {
            Span<float> floats =
            [
                r,
                g,
                b,
                a,
            ];
            Span<byte> buffer = stackalloc byte[16];

            if (writer.Endianness == Endian.LittleEndian && BitConverter.IsLittleEndian)
            {
                MemoryMarshal.Cast<float, byte>(floats).CopyTo(buffer);
            }
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    if (writer.Endianness == Endian.BigEndian)
                        BinaryPrimitives.WriteSingleBigEndian(buffer.Slice(i * 4, 4), floats[i]);
                    else
                        BinaryPrimitives.WriteSingleLittleEndian(buffer.Slice(i * 4, 4), floats[i]);
                }
            }

            writer.WriteBlock(buffer);
        }

        public override string ToString()
        {
            return $"R: {r}, G: {g}, B: {b}, A: {a}";
        }
    }

    /// <summary>
    /// Represents a color with red, green, and blue components.
    /// </summary>
    public class HmxColor3
    {
        public float r;
        public float g;
        public float b;

        public HmxColor3()
        {
            this.r = 1.0f;
            this.g = 1.0f;
            this.b = 1.0f;
        }

        public HmxColor3(float r, float g, float b)
        {
            this.r = r;
            this.g = g;
            this.b = b;
        }

        public HmxColor3 Read(EndianReader reader)
        {
            Span<byte> buffer = stackalloc byte[12];

            reader.ReadBlock(buffer);

            Span<float> floats = stackalloc float[3];

            if (reader.Endianness == Endian.LittleEndian && BitConverter.IsLittleEndian)
            {
                MemoryMarshal.Cast<byte, float>(buffer).CopyTo(floats);
            }
            else
            {
                for (int i = 0; i < 3; i++)
                {
                    floats[i] = reader.Endianness == Endian.BigEndian
                        ? BinaryPrimitives.ReadSingleBigEndian(buffer.Slice(i * 4, 4))
                        : BinaryPrimitives.ReadSingleLittleEndian(buffer.Slice(i * 4, 4));
                }
            }

            r = floats[0];
            g = floats[1];
            b = floats[2];

            return this;
        }

        public void Write(EndianWriter writer)
        {
            Span<float> floats =
            [
                r,
                g,
                b,
            ];
            Span<byte> buffer = stackalloc byte[12];

            if (writer.Endianness == Endian.LittleEndian && BitConverter.IsLittleEndian)
            {
                MemoryMarshal.Cast<float, byte>(floats).CopyTo(buffer);
            }
            else
            {
                for (int i = 0; i < 3; i++)
                {
                    if (writer.Endianness == Endian.BigEndian)
                        BinaryPrimitives.WriteSingleBigEndian(buffer.Slice(i * 4, 4), floats[i]);
                    else
                        BinaryPrimitives.WriteSingleLittleEndian(buffer.Slice(i * 4, 4), floats[i]);
                }
            }

            writer.WriteBlock(buffer);
        }

        public override string ToString()
        {
            return $"R: {r}, G: {g}, B: {b}";
        }
    }
}