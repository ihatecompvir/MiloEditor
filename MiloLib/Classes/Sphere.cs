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
    /// Represents a sphere.
    /// </summary>
    public struct Sphere
    {
        public float x;
        public float y;
        public float z;
        public float radius;

        /// <summary>
        /// Reads a sphere from the given reader.
        /// </summary>
        public Sphere Read(EndianReader reader)
        {
            Span<byte> buffer = stackalloc byte[16];

            // read entire sphere as a single block for performance
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

            x = floats[0];
            y = floats[1];
            z = floats[2];
            radius = floats[3];

            return this;
        }

        public void Write(EndianWriter writer)
        {
            Span<float> floats =
            [
                x,
                y,
                z,
                radius,
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

            // write entire sphere as a single block for performance
            writer.WriteBlock(buffer);
        }

        override public string ToString()
        {
            return "(x=" + x + ", y=" + y + ", z=" + z + "), r=" + radius;
        }
    }
}