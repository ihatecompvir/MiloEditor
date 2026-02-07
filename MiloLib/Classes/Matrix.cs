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
    /// Represents a 3x4 matrix.
    /// </summary>
    public class Matrix
    {
        public float m11;
        public float m12;
        public float m13;
        public float m21;
        public float m22;
        public float m23;
        public float m31;
        public float m32;
        public float m33;
        public float m41;
        public float m42;
        public float m43;

        /// <summary>
        /// Reads a matrix from the given reader.
        /// </summary>
        public Matrix Read(EndianReader reader)
        {
            Span<byte> buffer = stackalloc byte[48];

            // read entire matrix as a single block for performance
            reader.ReadBlock(buffer);

            Span<float> floats = stackalloc float[12];

            if (reader.Endianness == Endian.LittleEndian && BitConverter.IsLittleEndian)
            {
                MemoryMarshal.Cast<byte, float>(buffer).CopyTo(floats);
            }
            else
            {
                for (int i = 0; i < 12; i++)
                {
                    floats[i] = reader.Endianness == Endian.BigEndian
                        ? BinaryPrimitives.ReadSingleBigEndian(buffer.Slice(i * 4, 4))
                        : BinaryPrimitives.ReadSingleLittleEndian(buffer.Slice(i * 4, 4));
                }
            }

            m11 = floats[0]; m12 = floats[1]; m13 = floats[2];
            m21 = floats[3]; m22 = floats[4]; m23 = floats[5];
            m31 = floats[6]; m32 = floats[7]; m33 = floats[8];
            m41 = floats[9]; m42 = floats[10]; m43 = floats[11];

            return this;
        }

        /// <summary>
        /// Writes the matrix to the given writer.
        /// </summary>
        /// <param name="writer">The writer to write with.</param>
        public void Write(EndianWriter writer)
        {
            Span<float> floats =
            [
                m11,
                m12,
                m13,
                m21,
                m22,
                m23,
                m31,
                m32,
                m33,
                m41,
                m42,
                m43,
            ];
            Span<byte> buffer = stackalloc byte[48];

            if (writer.Endianness == Endian.LittleEndian && BitConverter.IsLittleEndian)
            {
                MemoryMarshal.Cast<float, byte>(floats).CopyTo(buffer);
            }
            else
            {
                for (int i = 0; i < 12; i++)
                {
                    if (writer.Endianness == Endian.BigEndian)
                        BinaryPrimitives.WriteSingleBigEndian(buffer.Slice(i * 4, 4), floats[i]);
                    else
                        BinaryPrimitives.WriteSingleLittleEndian(buffer.Slice(i * 4, 4), floats[i]);
                }
            }

            // write entire matrix as a single block for performance
            writer.WriteBlock(buffer);
        }

        override public string ToString()
        {
            return $"[{m11}, {m12}, {m13}],[{m21}, {m22}, {m23}],[{m31}, {m32}, {m33}],[{m41}, {m42}, {m43}]";
        }
    }

    /// <summary>
    /// Represents a 3x3 matrix.
    /// </summary>
    public class Matrix3
    {
        public float m11;
        public float m12;
        public float m13;
        public float m21;
        public float m22;
        public float m23;
        public float m31;
        public float m32;
        public float m33;

        /// <summary>
        /// Reads a matrix3 from the given reader.
        /// </summary>
        public Matrix3 Read(EndianReader reader)
        {
            m11 = reader.ReadFloat();
            m12 = reader.ReadFloat();
            m13 = reader.ReadFloat();
            m21 = reader.ReadFloat();
            m22 = reader.ReadFloat();
            m23 = reader.ReadFloat();
            m31 = reader.ReadFloat();
            m32 = reader.ReadFloat();
            m33 = reader.ReadFloat();
            return this;
        }

        /// <summary>
        /// Writes the matrix3 to the given writer.
        /// </summary>
        /// <param name="writer">The writer to write with.</param>
        public void Write(EndianWriter writer)
        {
            writer.WriteFloat(m11);
            writer.WriteFloat(m12);
            writer.WriteFloat(m13);
            writer.WriteFloat(m21);
            writer.WriteFloat(m22);
            writer.WriteFloat(m23);
            writer.WriteFloat(m31);
            writer.WriteFloat(m32);
            writer.WriteFloat(m33);
        }

        override public string ToString()
        {
            return $"[{m11}, {m12}, {m13}],[{m21}, {m22}, {m23}],[{m31}, {m32}, {m33}]";
        }
    }
}
