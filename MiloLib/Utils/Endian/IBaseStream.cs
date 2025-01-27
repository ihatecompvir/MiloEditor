﻿// originally taken from https://github.com/XboxChaos/Assembly, thanks for originally writing this code
// license in Utils/Endian/LICENSE.md

namespace MiloLib.Utils
{
    public interface IBaseStream : IDisposable
    {
        /// <summary>
        ///     Gets whether or not the stream pointer is at the end of the stream.
        /// </summary>
        bool EOF { get; }

        /// <summary>
        ///     Gets the length of the stream in bytes.
        /// </summary>
        long Length { get; }

        /// <summary>
        ///     Gets the current position of the stream pointer.
        /// </summary>
        long Position { get; }

        /// <summary>
        ///     Gets or sets the endianness used when reading/writing to/from the stream.
        /// </summary>
        Endian Endianness { get; set; }

        /// <summary>
        ///     Gets the base Stream object the stream was constructed from.
        /// </summary>
        Stream BaseStream { get; }

        /// <summary>
        ///     Seeks to an offset in the stream.
        /// </summary>
        /// <param name="offset">The offset to move the stream pointer to.</param>
        /// <returns>true if the seek was successful.</returns>
        bool SeekTo(long offset);

        /// <summary>
        ///     Skips over a number of bytes in the stream.
        /// </summary>
        /// <param name="count">The number of bytes to skip.</param>
        void Skip(long count);
    }
}