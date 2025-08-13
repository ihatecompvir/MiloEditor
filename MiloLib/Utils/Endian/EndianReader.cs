// originally taken from https://github.com/XboxChaos/Assembly, thanks for originally writing this code
// license in Utils/Endian/LICENSE.md

using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace MiloLib.Utils
{
    /// <summary>
    ///     A stream which can be read from and whose endianness can be changed.
    /// </summary>
    public class EndianReader : IDisposable, IReader
    {
        private readonly byte[] _buffer = new byte[8];
        private readonly StringBuilder _currentString = new StringBuilder();
        private readonly Stream _stream;
        private bool _bigEndian;

        /// <summary>
        ///     Initializes a new instance of the <see cref="EndianReader" /> class.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="endianness">The initial endianness to use when reading from the stream.</param>
        public EndianReader(Stream stream, Endian endianness)
        {
            _stream = stream;
            _bigEndian = (endianness == Endian.BigEndian);
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _stream.Dispose();
        }

        /// <summary>
        ///     Gets or sets the endianness used when reading/writing to/from the stream.
        /// </summary>
        public Endian Endianness
        {
            get { return _bigEndian ? Endian.BigEndian : Endian.LittleEndian; }
            set { _bigEndian = (value == Endian.BigEndian); }
        }

        /// <summary>
        ///     Reads a byte from the stream.
        /// </summary>
        /// <returns>
        ///     The byte that was read.
        /// </returns>
        /// <exception cref="EndOfStreamException">Thrown if the end of the stream is reached.</exception>
        public byte ReadByte()
        {
            int bytesRead = _stream.Read(_buffer, 0, 1);
            if (bytesRead < 1)
            {
                throw new EndOfStreamException("Attempted to read past the end of the stream.");
            }
            return _buffer[0];
        }

        /// <summary>
        ///     Reads a signed byte from the stream.
        /// </summary>
        /// <returns>
        ///     The signed byte that was read.
        /// </returns>
        /// <exception cref="EndOfStreamException">Thrown if the end of the stream is reached.</exception>
        public sbyte ReadSByte()
        {
            return (sbyte)ReadByte();
        }

        /// <summary>
        ///     Reads a 16-bit unsigned integer from the stream.
        /// </summary>
        /// <returns>
        ///     The 16-bit unsigned integer that was read.
        /// </returns>
        /// <exception cref="EndOfStreamException">Thrown if the end of the stream is reached.</exception>
        public ushort ReadUInt16()
        {
            int bytesRead = _stream.Read(_buffer, 0, 2);
            if (bytesRead < 2)
            {
                throw new EndOfStreamException("Attempted to read past the end of the stream.");
            }
            if (_bigEndian)
                return (ushort)((_buffer[0] << 8) | _buffer[1]);
            return (ushort)((_buffer[1] << 8) | _buffer[0]);
        }

        /// <summary>
        ///     Reads a 16-bit signed integer from the stream.
        /// </summary>
        /// <returns>
        ///     The 16-bit signed integer that was read.
        /// </returns>
        /// <exception cref="EndOfStreamException">Thrown if the end of the stream is reached.</exception>
        public short ReadInt16()
        {
            return (short)ReadUInt16();
        }

        /// <summary>
        ///     Reads a 32-bit unsigned integer from the stream.
        /// </summary>
        /// <returns>
        ///     The 32-bit unsigned integer that was read.
        /// </returns>
        /// <exception cref="EndOfStreamException">Thrown if the end of the stream is reached.</exception>
        public uint ReadUInt32()
        {
            int bytesRead = _stream.Read(_buffer, 0, 4);
            if (bytesRead < 4)
            {
                throw new EndOfStreamException("Attempted to read past the end of the stream.");
            }
            if (_bigEndian)
                return (uint)((_buffer[0] << 24) | (_buffer[1] << 16) | (_buffer[2] << 8) | _buffer[3]);
            return (uint)((_buffer[3] << 24) | (_buffer[2] << 16) | (_buffer[1] << 8) | _buffer[0]);
        }

        /// <summary>
        ///     Reads a 32-bit signed integer from the stream.
        /// </summary>
        /// <returns>
        ///     The 32-bit signed integer that was read.
        /// </returns>
        ///  <exception cref="EndOfStreamException">Thrown if the end of the stream is reached.</exception>
        public int ReadInt32()
        {
            return (int)ReadUInt32();
        }

        /// <summary>
        ///     Reads a 64-bit unsigned integer from the stream.
        /// </summary>
        /// <returns>
        ///     The 64-bit unsigned integer that was read.
        /// </returns>
        /// <exception cref="EndOfStreamException">Thrown if the end of the stream is reached.</exception>
        public ulong ReadUInt64()
        {
            ulong one = ReadUInt32();
            ulong two = ReadUInt32();
            if (_bigEndian)
                return (one << 32) | two;
            return (two << 32) | one;
        }

        /// <summary>
        ///     Reads a 64-bit signed integer from the stream.
        /// </summary>
        /// <returns>
        ///     The 64-bit signed integer that was read.
        /// </returns>
        /// <exception cref="EndOfStreamException">Thrown if the end of the stream is reached.</exception>
        public long ReadInt64()
        {
            return (long)ReadUInt64();
        }

        public float ReadHalfFloat()
        {
            ushort bits = ReadUInt16();
            return (float)BitConverter.UInt16BitsToHalf(bits);
        }

        /// <summary>
        ///     Reads a 32-bit floating-point value from the stream.
        /// </summary>
        /// <returns>
        ///     The 32-bit floating-point value that was read.
        /// </returns>
        /// <exception cref="EndOfStreamException">Thrown if the end of the stream is reached.</exception>
        public float ReadFloat()
        {
            int bytesRead = _stream.Read(_buffer, 0, 4);
            if (bytesRead < 4)
            {
                throw new EndOfStreamException("Attempted to read past the end of the stream.");
            }
            if (BitConverter.IsLittleEndian == _bigEndian)
            {
                // Flip the bytes
                // Is there a faster way to do this?
                byte temp = _buffer[0];
                _buffer[0] = _buffer[3];
                _buffer[3] = temp;
                temp = _buffer[1];
                _buffer[1] = _buffer[2];
                _buffer[2] = temp;
            }
            return BitConverter.ToSingle(_buffer, 0);
        }

        /// <summary>
        ///     Seeks to an offset in the stream.
        /// </summary>
        /// <param name="offset">The offset to move the stream pointer to.</param>
        /// <returns>
        ///     true if the seek was successful.
        /// </returns>
        public bool SeekTo(long offset)
        {
            if (offset < 0)
                return false;
            _stream.Seek(offset, SeekOrigin.Begin);
            return true;
        }

        /// <summary>
        ///     Skips over a number of bytes in the stream.
        /// </summary>
        /// <param name="count">The number of bytes to skip.</param>
        public void Skip(long count)
        {
            _stream.Seek(count, SeekOrigin.Current);
        }

        /// <summary>
        ///     Reads a null-terminated ASCII string from the stream.
        /// </summary>
        /// <returns>
        ///     The ASCII string that was read.
        /// </returns>
        /// <exception cref="EndOfStreamException">Thrown if the end of the stream is reached.</exception>
        public string ReadAscii()
        {
            _currentString.Clear();
            int ch;
            while (true)
            {
                ch = _stream.ReadByte();
                if (ch == -1)
                {
                    throw new EndOfStreamException("Attempted to read past the end of the stream.");
                }
                if (ch == 0)
                    break;
                _currentString.Append((char)ch);
            }
            return _currentString.ToString();
        }


        /// <summary>
        ///     Reads a fixed-size null-terminated ASCII string from the stream.
        /// </summary>
        /// <param name="size">The size of the string to read, including the null terminator.</param>
        /// <returns>
        ///     The ASCII string that was read, with any 0 padding bytes stripped.
        /// </returns>
        /// <exception cref="EndOfStreamException">Thrown if the end of the stream is reached.</exception>
        public string ReadAscii(int size)
        {
            _currentString.Clear();
            int ch;
            for (var i = 0; i < size; i++)
            {
                ch = _stream.ReadByte();
                if (ch == -1)
                {
                    throw new EndOfStreamException("Attempted to read past the end of the stream.");
                }
                if (ch == 0)
                    break;
                _currentString.Append((char)ch);
            }
            return _currentString.ToString();
        }

        /// <summary>
        /// Reads a length-prefixed ASCII string from the stream.
        /// </summary>
        /// <returns>The string.</returns>
        /// <exception cref="EndOfStreamException">Thrown if the end of the stream is reached.</exception>
        public string ReadLengthString()
        {
            int length = ReadInt32();
            return Encoding.ASCII.GetString(ReadBlock(length));
        }


        /// <summary>
        ///     Reads a null-terminated Windows-1252 string from the stream.
        /// </summary>
        /// <returns>
        ///     The ASCII string that was read.
        /// </returns>
        public string ReadWin1252()
        {
            return "Unsupported";
        }

        /// <summary>
        ///     Reads a null-terminated UTF-8 string from the stream.
        /// </summary>
        /// <returns>
        ///     The null-terminated UTF-8 string that was read.
        /// </returns>
        /// <exception cref="EndOfStreamException">Thrown if the end of the stream is reached.</exception>
        public string ReadUTF8()
        {
            var chars = new List<byte>();
            byte ch;
            while (true)
            {
                try
                {
                    ch = ReadByte();
                }
                catch (EndOfStreamException)
                {
                    throw new EndOfStreamException("Attempted to read past the end of the stream.");
                }

                if (ch == 0) break;
                chars.Add(ch);
            }
            return Encoding.UTF8.GetString(chars.ToArray());
        }


        /// <summary>
        ///     Reads a fixed-size null-terminated UTF-8 string from the stream.
        /// </summary>
        /// <param name="size">The size in bytes of the string to read, including the null terminator.</param>
        /// <returns>
        ///     The UTF-8 string that was read, with any padding bytes stripped.
        /// </returns>
        ///  <exception cref="EndOfStreamException">Thrown if the end of the stream is reached.</exception>
        public string ReadUTF8(int size)
        {
            var chars = new List<byte>();
            byte ch;
            for (int i = 0; i < size; i++)
            {
                try
                {
                    ch = ReadByte();
                }
                catch (EndOfStreamException)
                {
                    throw new EndOfStreamException("Attempted to read past the end of the stream.");
                }

                if (ch == 0) break;
                chars.Add(ch);
            }
            return Encoding.UTF8.GetString(chars.ToArray());
        }


        /// <summary>
        ///     Reads a null-terminated UTF-16 string from the stream.
        /// </summary>
        /// <returns>
        ///     The UTF-16 string that was read.
        /// </returns>
        /// <exception cref="EndOfStreamException">Thrown if the end of the stream is reached.</exception>
        public string ReadUTF16()
        {
            _currentString.Clear();
            int ch;
            while (true)
            {
                try
                {
                    ch = ReadInt16();
                }
                catch (EndOfStreamException)
                {
                    throw new EndOfStreamException("Attempted to read past the end of the stream.");
                }
                if (ch == 0)
                    break;
                _currentString.Append((char)ch);
            }
            return _currentString.ToString();
        }

        /// <summary>
        ///     Reads a fixed-size null-terminated UTF-16 string from the stream.
        /// </summary>
        /// <param name="size">The size in bytes of the string to read, including the null terminator.</param>
        /// <returns>
        ///     The UTF-16 string that was read, with any padding bytes stripped.
        /// </returns>
        /// <exception cref="EndOfStreamException">Thrown if the end of the stream is reached.</exception>
        public string ReadUTF16(int size)
        {
            _currentString.Clear();
            int ch;
            while (_currentString.Length * 2 < size)
            {
                try
                {
                    ch = ReadInt16();
                }
                catch (EndOfStreamException)
                {
                    throw new EndOfStreamException("Attempted to read past the end of the stream.");
                }
                if (ch == 0)
                    break;
                _currentString.Append((char)ch);
            }
            Skip(size - _currentString.Length * 2);
            return _currentString.ToString();
        }

        public string ReadBytesWithEncoding(int size, Encoding encoding)
        {
            byte[] bytes = new byte[size];
            for (int i = 0; i < size; i++)
            {
                bytes[i] = ReadByte();
            }

            int nullIndex = Array.IndexOf<byte>(bytes, 0);
            if (nullIndex >= 0)
            {
                size = nullIndex;
            }

            return encoding.GetString(bytes, 0, size);
        }


        /// <summary>
        ///     Reads an array of bytes from the stream.
        /// </summary>
        /// <param name="size">The number of bytes to read.</param>
        /// <returns>
        ///     The bytes that were read.
        /// </returns>
        /// <exception cref="EndOfStreamException">Thrown if the end of the stream is reached.</exception>
        public byte[] ReadBlock(int size)
        {
            var result = new byte[size];
            int bytesRead = _stream.Read(result, 0, size);
            if (bytesRead < size)
            {
                throw new EndOfStreamException($"Attempted to read {size} bytes but only read {bytesRead}.");
            }

            return result;
        }

        /// <summary>
        ///     Reads an array of bytes from the stream.
        /// </summary>
        /// <param name="output">The array to store the read bytes to.</param>
        /// <param name="offset">The starting index in the array to read to.</param>
        /// <param name="size">The number of bytes to read.</param>
        /// <returns>
        ///     The number of bytes that were actually read.
        /// </returns>
        /// <exception cref="IOException">Thrown if there is an issue reading the stream.</exception>
        public int ReadBlock(byte[] output, int offset, int size)
        {
            int bytesRead = _stream.Read(output, offset, size);
            if (bytesRead < size)
            {
                throw new IOException($"Attempted to read {size} bytes but only read {bytesRead}.");
            }
            return bytesRead;
        }

        /// <summary>
        ///     Reads a boolean value from the stream.
        /// </summary>
        /// <returns>
        ///     The boolean value that was read.
        /// </returns>
        /// <exception cref="EndOfStreamException">Thrown if the end of the stream is reached.</exception>
        public bool ReadBoolean()
        {
            byte value = ReadByte();
            return value != 0;
        }

        private static float HalfToFloat(ushort half)
        {
            int sign = (half >> 15) & 0x0001;
            int exp = (half >> 10) & 0x001F;
            int mant = half & 0x03FF;

            if (exp == 0)
            {
                // Zero or subnormal
                if (mant == 0)
                    return BitConverter.ToSingle(BitConverter.GetBytes(sign << 31), 0);

                // Normalize the subnormal value
                while ((mant & 0x0400) == 0)
                {
                    mant <<= 1;
                    exp--;
                }
                exp++;
                mant &= ~0x0400;
            }
            else if (exp == 31)
            {
                // Infinite or NaN
                if (mant == 0)
                    return BitConverter.ToSingle(BitConverter.GetBytes((sign << 31) | 0x7F800000), 0);
                else
                    return BitConverter.ToSingle(BitConverter.GetBytes((sign << 31) | 0x7F800000 | (mant << 13)), 0);
            }

            exp = exp + (127 - 15);
            mant <<= 13;

            int bits = (sign << 31) | (exp << 23) | mant;
            return BitConverter.ToSingle(BitConverter.GetBytes(bits), 0);
        }

        /// <summary>
        ///     Gets whether or not the stream pointer is at the end of the stream.
        /// </summary>
        public bool EOF
        {
            get { return (Position >= Length); }
        }

        /// <summary>
        ///     Gets the current position of the stream pointer.
        /// </summary>
        public long Position
        {
            get { return _stream.Position; }
        }

        /// <summary>
        ///     Gets the length of the stream in bytes.
        /// </summary>
        public long Length
        {
            get { return _stream.Length; }
        }

        /// <summary>
        ///     Gets the base Stream object the stream was constructed from.
        /// </summary>
        public Stream BaseStream
        {
            get { return _stream; }
        }
    }
}