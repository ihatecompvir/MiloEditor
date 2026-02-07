// originally taken from https://github.com/XboxChaos/Assembly, thanks for originally writing this code
// license in Utils/Endian/LICENSE.md

using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

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
            int result = _stream.ReadByte();
            if (result == -1)
                throw new EndOfStreamException("Attempted to read past the end of the stream.");
            return (byte)result;
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
            Span<byte> buf = stackalloc byte[2];
            if (_stream.Read(buf) != 2)
                throw new EndOfStreamException();

            return _bigEndian
                ? BinaryPrimitives.ReadUInt16BigEndian(buf)
                : BinaryPrimitives.ReadUInt16LittleEndian(buf);
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
            Span<byte> buf = stackalloc byte[4];
            if (_stream.Read(buf) != 4)
                throw new EndOfStreamException();

            return _bigEndian
                ? BinaryPrimitives.ReadUInt32BigEndian(buf)
                : BinaryPrimitives.ReadUInt32LittleEndian(buf);
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
            Span<byte> buf = stackalloc byte[8];
            if (_stream.Read(buf) != 8)
                throw new EndOfStreamException();
            return _bigEndian
                ? BinaryPrimitives.ReadUInt64BigEndian(buf)
                : BinaryPrimitives.ReadUInt64LittleEndian(buf);
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
            Span<byte> buf = stackalloc byte[4];
            if (_stream.Read(buf) != 4)
                throw new EndOfStreamException();

            uint bits = _bigEndian
                ? BinaryPrimitives.ReadUInt32BigEndian(buf)
                : BinaryPrimitives.ReadUInt32LittleEndian(buf);

            return BitConverter.UInt32BitsToSingle(bits);
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
            Span<byte> stackBuf = stackalloc byte[256];
            byte[]? rented = null;
            int index = 0;

            try
            {
                while (true)
                {
                    int b = _stream.ReadByte();
                    if (b < 0)
                        throw new EndOfStreamException();
                    if (b == 0)
                        break;

                    if (index == stackBuf.Length)
                    {
                        var newArr = ArrayPool<byte>.Shared.Rent(stackBuf.Length * 2);
                        stackBuf.CopyTo(newArr);
                        if (rented != null)
                            ArrayPool<byte>.Shared.Return(rented);
                        rented = newArr;
                        stackBuf = rented;
                    }

                    stackBuf[index++] = (byte)b;
                }

                return Encoding.UTF8.GetString(stackBuf[..index]);
            }
            finally
            {
                if (rented != null)
                    ArrayPool<byte>.Shared.Return(rented);
            }
        }

        /// <summary>
        ///     Reads a fixed-size null-terminated UTF-8 string from the stream.
        /// </summary>
        /// <param name="size">The size in bytes of the string to read, including the null terminator.</param>
        /// <returns>
        ///     The UTF-8 string that was read, with any padding bytes stripped.
        /// </returns>
        /// <exception cref="EndOfStreamException">Thrown if the end of the stream is reached.</exception>
        public string ReadUTF8(int size)
        {
            if (size <= 0)
                return string.Empty;

            byte[]? rented = null;
            Span<byte> buffer = size <= 1024
                ? stackalloc byte[size]
                : (rented = ArrayPool<byte>.Shared.Rent(size)).AsSpan(0, size);

            try
            {
                _stream.ReadExactly(buffer);
                int len = buffer.IndexOf((byte)0);
                if (len < 0) len = size;
                return Encoding.UTF8.GetString(buffer[..len]);
            }
            finally
            {
                if (rented != null)
                    ArrayPool<byte>.Shared.Return(rented);
            }
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
            Span<byte> buf = stackalloc byte[512];
            int pos = 0;

            while (true)
            {
                if (pos + 2 > buf.Length)
                    throw new InvalidOperationException("String too long for buffer; use fixed-size variant with larger size.");

                _stream.ReadExactly(buf.Slice(pos, 2));
                short val = BinaryPrimitives.ReadInt16LittleEndian(buf.Slice(pos, 2));
                if (val == 0)
                    break;
                pos += 2;
            }

            return new string(MemoryMarshal.Cast<byte, char>(buf[..pos]));
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
            if (size <= 0)
                return string.Empty;

            byte[]? rented = null;
            Span<byte> buffer = size <= 1024
                ? stackalloc byte[size]
                : (rented = ArrayPool<byte>.Shared.Rent(size)).AsSpan(0, size);

            try
            {
                _stream.ReadExactly(buffer);
                int length = 0;
                for (; length < size - 1; length += 2)
                {
                    if (BinaryPrimitives.ReadInt16LittleEndian(buffer[length..]) == 0)
                        break;
                }

                return new string(MemoryMarshal.Cast<byte, char>(buffer[..length]));
            }
            finally
            {
                if (rented != null)
                    ArrayPool<byte>.Shared.Return(rented);
            }
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
            int bytesRead = _stream.Read(result.AsSpan());
            if (bytesRead < size)
                throw new EndOfStreamException($"Attempted to read {size} bytes but only read {bytesRead}.");
            return result;
        }

        /// <summary>
        ///     Reads bytes into a span.
        /// </summary>
        /// <param name="buffer">The span to read bytes into.</param>
        /// <exception cref="EndOfStreamException">Thrown if the end of the stream is reached.</exception>
        public void ReadBlock(Span<byte> buffer)
        {
            int bytesRead = _stream.Read(buffer);
            if (bytesRead < buffer.Length)
                throw new EndOfStreamException($"Attempted to read {buffer.Length} bytes but only read {bytesRead}.");
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