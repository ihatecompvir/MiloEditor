using MiloLib.Assets;
using MiloLib.Utils;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

namespace MiloLib
{
    /// <summary>
    /// Represents a Milo scene (with the extension .milo_{platform}), which is a container for assets and directories.
    /// </summary>
    public class MiloFile
    {
        /// <summary>
        /// The type of the Milo file. Determines if it's compressed or not and how it's compressed.
        /// </summary>
        public enum Type : uint
        {
            Uncompressed = 0xCABEDEAF,

            // zlib compressed, without uncompressed size before the start of blocks
            CompressedZlib = 0xCBBEDEAF,

            CompressedGzip = 0xCCBEDEAF,

            // zlib compressed, with uncompressed size before the start of blocks
            CompressedZlibAlt = 0xCDBEDEAF,
        }

        /// <summary>
        /// The path to the Milo file.
        /// </summary>
        private string filePath;

        /// <summary>
        /// The Milo's compression type.
        /// </summary>
        private Type compressionType;

        /// <summary>
        /// The offset to the start of the root asset.
        /// </summary>
        private uint startOffset;

        /// <summary>
        /// The uncompressed size of the largest block in the Milo file.
        /// </summary>
        private uint largestBlock;

        /// <summary>
        /// The sizes of each block in the Milo file.
        /// </summary>
        private List<uint> blockSizes = new List<uint>();

        public DirectoryMeta dirMeta;

        /// <summary>
        /// Loads a Milo file from a file path.
        /// </summary>
        public MiloFile(string path)
        {
            filePath = path;

            using (EndianReader reader = new EndianReader(File.OpenRead(path), Endian.LittleEndian))
            {
                compressionType = (Type)reader.ReadUInt32();

                // detect if the type is one of the compressed types
                if (compressionType != Type.Uncompressed && compressionType != Type.CompressedZlib && compressionType != Type.CompressedGzip && compressionType != Type.CompressedZlibAlt)
                {
                    // this might be a headerless milo (e.g. Phase .milo_pc) so treat it as such and just start reading the root directory
                    reader.SeekTo(0);
                    dirMeta = new DirectoryMeta().Read(reader);
                    return;
                }

                startOffset = reader.ReadUInt32();

                uint numBlocks = reader.ReadUInt32();

                largestBlock = reader.ReadUInt32();

                for (int i = 0; i < numBlocks; i++)
                {
                    blockSizes.Add(reader.ReadUInt32());
                }

                switch (compressionType)
                {
                    case Type.CompressedZlib:
                        reader.SeekTo(startOffset);
                        reader.Endianness = Endian.BigEndian;

                        MemoryStream compressedStream = new MemoryStream();

                        for (int i = 0; i < numBlocks; i++)
                        {
                            MemoryStream blockStream = new MemoryStream(reader.ReadBlock((int)blockSizes[i]));

                            InflaterInputStream inflater = new InflaterInputStream(blockStream, new Inflater(true));
                            inflater.CopyTo(compressedStream);
                        }

                        EndianReader decompressedReader = new EndianReader(compressedStream, Endian.BigEndian);

                        compressedStream.Seek(0, SeekOrigin.Begin);

                        dirMeta = new DirectoryMeta().Read(decompressedReader);
                        break;
                    case Type.CompressedZlibAlt:
                        reader.SeekTo(startOffset);
                        reader.Endianness = Endian.BigEndian;

                        compressedStream = new MemoryStream();

                        for (int i = 0; i < numBlocks; i++)
                        {
                            bool uncompressed = (blockSizes[i] & 0xFF000000) != 0;

                            uint uncompressedSize;

                            if (uncompressed)
                            {
                                blockSizes[i] &= 0x00FFFFFF;
                                uncompressedSize = blockSizes[i];
                            }
                            else
                            {
                                uncompressedSize = reader.ReadUInt32();
                            }

                            MemoryStream blockStream;

                            if (uncompressed)
                            {
                                blockStream = new MemoryStream(reader.ReadBlock((int)blockSizes[i]));
                            }
                            else
                            {
                                blockStream = new MemoryStream(reader.ReadBlock((int)blockSizes[i] - 4));
                            }

                            if (!uncompressed)
                            {
                                InflaterInputStream inflater = new InflaterInputStream(blockStream, new Inflater(true));
                                inflater.CopyTo(compressedStream);
                            }
                            else
                            {
                                blockStream.CopyTo(compressedStream);
                            }
                        }

                        decompressedReader = new EndianReader(compressedStream, Endian.BigEndian);

                        compressedStream.Seek(0, SeekOrigin.Begin);

                        dirMeta = new DirectoryMeta().Read(decompressedReader);
                        break;
                    case Type.CompressedGzip:
                        reader.SeekTo(startOffset);
                        reader.Endianness = Endian.BigEndian;

                        compressedStream = new MemoryStream();

                        for (int i = 0; i < numBlocks; i++)
                        {
                            bool compressed = (blockSizes[i] & 0xFF000000) != 0;

                            if (compressed)
                            {
                                blockSizes[i] &= 0x00FFFFFF;
                            }

                            MemoryStream blockStream = new MemoryStream(reader.ReadBlock((int)blockSizes[i]));

                            if (compressed)
                            {
                                InflaterInputStream inflater = new InflaterInputStream(blockStream);
                                inflater.CopyTo(compressedStream);
                            }
                            else
                            {
                                blockStream.CopyTo(compressedStream);
                            }
                        }

                        decompressedReader = new EndianReader(compressedStream, Endian.BigEndian);

                        compressedStream.Seek(0, SeekOrigin.Begin);

                        dirMeta = new DirectoryMeta().Read(decompressedReader);
                        break;
                    case Type.Uncompressed:
                        reader.SeekTo(startOffset);

                        reader.Endianness = Endian.BigEndian;

                        dirMeta = new DirectoryMeta().Read(reader);
                        break;
                    default:
                        break;
                }

                System.Diagnostics.Debug.WriteLine("Done reading Milo file " + path);
            }

        }

        public void Save(string path, Type type, uint startingOffset = 0x810, Endian headerEndian = Endian.LittleEndian, Endian bodyEndian = Endian.BigEndian)
        {
            using (EndianWriter writer = new EndianWriter(File.Create(path), headerEndian))
            {
                writer.WriteUInt32((uint)type);
                writer.WriteUInt32(startingOffset);
                writer.WriteUInt32(1);

                // block sizes, write nothing for now
                writer.WriteUInt32(0);
                writer.WriteUInt32(0);

                writer.WriteBlock(new byte[startingOffset - (int)writer.BaseStream.Position]);

                // switch to big endian, only the header is little
                writer.Endianness = bodyEndian;

                // try to serialize the root directory and pray it works
                dirMeta.Write(writer);

                writer.SeekTo(0xC);
                writer.Endianness = Endian.LittleEndian;
                writer.WriteUInt32((uint)writer.BaseStream.Length - startingOffset);
                writer.WriteUInt32((uint)writer.BaseStream.Length - startingOffset);
            }
        }

        public override string ToString()
        {
            return string.Format("{0}", filePath);
        }
    }
}
