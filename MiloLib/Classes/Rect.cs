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
    /// Represents a rectangle.
    /// </summary>
    public struct Rect
    {
        public float x;
        public float y;
        public float width;
        public float height;

        /// <summary>
        /// Reads a Rect from the given reader.
        /// </summary>
        public Rect Read(EndianReader reader)
        {
            Span<byte> buffer = stackalloc byte[16];

            // read entire rect as a single block for performance
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
            width = floats[2];
            height = floats[3];

            return this;
        }

        /// <summary>
        /// Writes a Rect via the given reader.
        /// </summary>
        /// <param name="writer">The writer to write the Rect.</param>
        public void Write(EndianWriter writer)
        {
            Span<float> floats =
            [
                x,
                y,
                width,
                height,
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

            // write entire rect as a single block for performance
            writer.WriteBlock(buffer);
        }

        override public string ToString()
        {
            return "(x=" + x + ", y=" + y + ", width=" + width + ", height=" + height + ")";
        }
    }
}