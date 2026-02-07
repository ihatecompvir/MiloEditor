using MiloLib.Utils;
using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;

namespace MiloLib.Classes
{
    /// <summary>
    /// Represents a 2D vector.
    /// </summary>
    public struct Vector2
    {
        public float x;
        public float y;

        public Vector2()
        {
            this.x = 0.0f;
            this.y = 0.0f;
        }

        public Vector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        /// <summary>
        /// Reads a Vector2 from a stream.
        /// </summary>
        /// <param name="reader">The reader to use.</param>
        /// <returns>The 2D vector read from the stream.</returns>
        public Vector2 Read(EndianReader reader)
        {
            Span<byte> buffer = stackalloc byte[8];

            // read entire vector as a single block for performance
            reader.ReadBlock(buffer);

            Span<float> floats = stackalloc float[2];

            if (reader.Endianness == Endian.LittleEndian && BitConverter.IsLittleEndian)
            {
                MemoryMarshal.Cast<byte, float>(buffer).CopyTo(floats);
            }
            else
            {
                for (int i = 0; i < 2; i++)
                {
                    floats[i] = reader.Endianness == Endian.BigEndian
                        ? BinaryPrimitives.ReadSingleBigEndian(buffer.Slice(i * 4, 4))
                        : BinaryPrimitives.ReadSingleLittleEndian(buffer.Slice(i * 4, 4));
                }
            }

            x = floats[0];
            y = floats[1];

            return this;
        }

        /// <summary>
        /// Writes the Vector2 to a stream.
        /// </summary>
        /// <param name="writer">The writer to use.</param>
        public void Write(EndianWriter writer)
        {
            Span<float> floats =
            [
                x,
                y,
            ];
            Span<byte> buffer = stackalloc byte[8];

            if (writer.Endianness == Endian.LittleEndian && BitConverter.IsLittleEndian)
            {
                MemoryMarshal.Cast<float, byte>(floats).CopyTo(buffer);
            }
            else
            {
                for (int i = 0; i < 2; i++)
                {
                    if (writer.Endianness == Endian.BigEndian)
                        BinaryPrimitives.WriteSingleBigEndian(buffer.Slice(i * 4, 4), floats[i]);
                    else
                        BinaryPrimitives.WriteSingleLittleEndian(buffer.Slice(i * 4, 4), floats[i]);
                }
            }

            // write entire vector as a single block for performance
            writer.WriteBlock(buffer);
        }

        public override string ToString()
        {
            return $"({x}, {y})";
        }
    }

    /// <summary>
    /// Represents a 3D vector.
    /// </summary>
    public struct Vector3
    {
        public float x;
        public float y;
        public float z;

        public Vector3()
        {
            this.x = 0.0f;
            this.y = 0.0f;
            this.z = 0.0f;
        }

        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        /// <summary>
        /// Reads a Vector3 from a stream.
        /// </summary>
        /// <param name="reader">The reader to use.</param>
        /// <returns>The 3D vector read from the stream.</returns>
        public Vector3 Read(EndianReader reader)
        {
            Span<byte> buffer = stackalloc byte[12];

            // read entire vector as a single block for performance
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

            x = floats[0];
            y = floats[1];
            z = floats[2];

            return this;
        }

        /// <summary>
        /// Writes the Vector3 to a stream.
        /// </summary>
        /// <param name="writer">The writer to use.</param>
        public void Write(EndianWriter writer)
        {
            Span<float> floats =
            [
                x,
                y,
                z,
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

            // write entire vector as a single block for performance
            writer.WriteBlock(buffer);
        }

        public bool IsZero()
        {
            return x == 0.0f && y == 0.0f && z == 0.0f;
        }

        public override string ToString()
        {
            return $"({x}, {y}, {z})";
        }
    }

    /// <summary>
    /// Represents a 4D vector.
    /// </summary>
    public struct Vector4
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public Vector4()
        {
            this.x = 0.0f;
            this.y = 0.0f;
            this.z = 0.0f;
            this.w = 0.0f;
        }

        public Vector4(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = 0.0f;
        }

        /// <summary>
        /// Reads a Vector4 from a stream.
        /// </summary>
        /// <param name="reader">The reader to use.</param>
        /// <returns>The 4D vector read from the stream.</returns>
        public Vector4 Read(EndianReader reader)
        {
            Span<byte> buffer = stackalloc byte[16];

            // read entire vector as a single block for performance
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
            w = floats[3];

            return this;
        }

        /// <summary>
        /// Writes the Vector4 to a stream.
        /// </summary>
        /// <param name="writer">The writer to use.</param>
        public void Write(EndianWriter writer)
        {
            Span<float> floats =
            [
                x,
                y,
                z,
                w,
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

            // write entire vector as a single block for performance
            writer.WriteBlock(buffer);
        }

        public override string ToString()
        {
            return $"({x}, {y}, {z}, {w})";
        }
    }
}