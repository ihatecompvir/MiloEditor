using MiloLib.Assets;
using MiloLib.Utils;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using ICSharpCode.SharpZipLib.GZip;
using System.IO;
using System.Reflection.PortableExecutable;
using MiloLib.Classes;

namespace MiloLib
{
    /// <summary>
    /// Represents a Milo scene (with the extension .milo_{platform}), which is a container for assets and directories.
    /// </summary>
    public class MiloFile
    {
        /// <summary>
        /// The maximum size a block can be.
        /// TODO: check if there is some "best" value for this, right now we just use the maximum possible block size
        /// </summary>
        private const int MAX_BLOCK_SIZE = 0xFFFFFF;

        /// <summary>
        /// The type of the Milo file. Determines if it's compressed or not and how it's compressed.
        /// </summary>
        public enum Type : uint
        {
            /// <summary>
            /// no compression, root dir starts at startOffset
            /// </summary>
            [Name("Uncompressed")]
            Uncompressed = 0xCABEDEAF,

            /// <summary>
            /// zlib compressed, without uncompressed size before the start of blocks
            /// </summary>
            [Name("ZLib (AntiGrav to GDRB)")]
            CompressedZlib = 0xCBBEDEAF,

            /// <summary>
            /// normal gzip
            /// </summary>
            [Name("GZip (Amplitude and earlier)")]
            CompressedGzip = 0xCCBEDEAF,

            /// <summary>
            /// zlib compressed, with uncompressed size before the start of blocks
            /// </summary>
            [Name("ZLib Alternate (RB3 and later)")]
            CompressedZlibAlt = 0xCDBEDEAF,
        }

        /// <summary>
        /// The path to the Milo file.
        /// </summary>
        public string? filePath
        {
            get;
        }

        /// <summary>
        /// The Milo's compression type.
        /// </summary>
        public Type compressionType { get; }

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

        /// <summary>
        /// The root directory and it's metadata such as the entries and string table data.
        /// </summary>
        public DirectoryMeta dirMeta;

        /// <summary>
        /// The endianness of the body. Header is always little endian.
        /// </summary>
        public Endian endian = Endian.BigEndian;

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

                        DirectoryMeta meta = new DirectoryMeta();
                        meta.platform = DetectPlatform();
                        dirMeta = meta.Read(decompressedReader);
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

                        meta = new DirectoryMeta();
                        meta.platform = DetectPlatform();
                        dirMeta = meta.Read(decompressedReader);
                        break;
                    case Type.CompressedGzip:
                        reader.SeekTo(startOffset);
                        reader.Endianness = Endian.BigEndian;

                        compressedStream = new MemoryStream();

                        for (int i = 0; i < numBlocks; i++)
                        {
                            MemoryStream blockStream = new MemoryStream(reader.ReadBlock((int)blockSizes[i]));

                            GZipInputStream inflater = new GZipInputStream(blockStream);
                            inflater.CopyTo(compressedStream);
                        }

                        decompressedReader = new EndianReader(compressedStream, Endian.BigEndian);

                        compressedStream.Seek(0, SeekOrigin.Begin);

                        meta = new DirectoryMeta();
                        meta.platform = DetectPlatform();
                        dirMeta = meta.Read(decompressedReader);
                        break;
                    case Type.Uncompressed:
                        reader.SeekTo(startOffset);

                        reader.Endianness = Endian.BigEndian;

                        meta = new DirectoryMeta();
                        meta.platform = DetectPlatform();
                        dirMeta = meta.Read(reader);
                        break;
                    default:
                        break;
                }

                System.Diagnostics.Debug.WriteLine("Done reading Milo file " + path);
            }
        }

        private DirectoryMeta.Platform DetectPlatform()
        {
            // detect the platform from the file extension
            string extension = Path.GetExtension(filePath);
            if (extension == null)
            {
                throw new Exception("Could not detect platform from file extension");
            }

            // ps2 if extension ends in _ps2
            if (extension.EndsWith("_ps2"))
            {
                return DirectoryMeta.Platform.PS2;
            }

            // xbox if extension ends in _xbox
            if (extension.EndsWith("_xbox"))
            {
                return DirectoryMeta.Platform.Xbox;
            }

            // pc if extension ends in _pc
            if (extension.EndsWith("_pc"))
            {
                return DirectoryMeta.Platform.PC_or_iPod;
            }

            // wii if extension ends in _wii
            if (extension.EndsWith("_wii"))
            {
                return DirectoryMeta.Platform.Wii;
            }

            // ps3 if extension ends in _ps3
            if (extension.EndsWith("_ps3"))
            {
                return DirectoryMeta.Platform.PS3;
            }

            // gamecube if extension ends in gc
            if (extension.EndsWith("_gc"))
            {
                return DirectoryMeta.Platform.GameCube;
            }

            return DirectoryMeta.Platform.PS3;
        }

        /// <summary>
        /// Constructs a new MiloFile.
        /// </summary>
        /// <param name="meta">The root directory to create the MiloFile with.</param>
        public MiloFile(DirectoryMeta meta)
        {
            this.compressionType = Type.Uncompressed;
            this.dirMeta = meta;
            this.startOffset = 0x810;
            this.blockSizes = new List<uint>();
            return;
        }

        /// <summary>
        /// Saves a Milo scene to disk.
        /// </summary>
        /// <param name="path">The path at which the Milo scene will be saved.</param>
        /// <param name="type">The compression type to use.</param>
        /// <param name="startingOffset">The offset at which the root directory starts.</param>
        /// <param name="headerEndian">The endianness of the header.</param>
        /// <param name="bodyEndian">The endianness of the body. Certain games require little endian bodies, such as GH2.</param>
        public void Save(string? path, Type? type, uint startingOffset = 0x810, Endian headerEndian = Endian.LittleEndian, Endian bodyEndian = Endian.BigEndian)
        {
            endian = bodyEndian;
            if (path == null)
            {
                path = filePath;
            }

            using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                fs.SetLength(0);
            }

            using (EndianWriter writer = new EndianWriter(File.Create(path), headerEndian))
            {
                if (type == null)
                {
                    // no specified type, so just use what is already there
                    type = compressionType;
                }
                writer.WriteUInt32((uint)type);
                writer.WriteUInt32(startingOffset);
                writer.WriteUInt32(1);

                // block sizes, write nothing for now
                writer.WriteUInt32(0);
                writer.WriteUInt32(0);

                writer.WriteBlock(new byte[startingOffset - (int)writer.BaseStream.Position]);

                // switch to big endian, only the header is little
                writer.Endianness = bodyEndian;

                MemoryStream compressedStream;
                EndianWriter compressedWriter;
                List<byte[]> compressedBlocks;
                List<int> uncompressedBlockSizes = new List<int>();

                switch (type)
                {
                    case Type.CompressedZlib:
                    case Type.CompressedZlibAlt:
                    case Type.CompressedGzip:

                        compressedStream = new MemoryStream();
                        compressedWriter = new EndianWriter(compressedStream, Endian.BigEndian);

                        dirMeta.Write(compressedWriter);

                        compressedStream.Seek(0, SeekOrigin.Begin);

                        compressedBlocks = new List<byte[]>();
                        byte[] buffer = new byte[MAX_BLOCK_SIZE];
                        int bytesRead;
                        while ((bytesRead = compressedStream.Read(buffer, 0, MAX_BLOCK_SIZE)) > 0)
                        {
                            byte[] block = new byte[bytesRead];
                            Array.Copy(buffer, block, bytesRead);

                            uncompressedBlockSizes.Add(block.Length);

                            byte[] compressedBlock = null;
                            using (MemoryStream blockStream = new MemoryStream())
                            {
                                if (type == Type.CompressedZlib || type == Type.CompressedZlibAlt)
                                {
                                    DeflaterOutputStream deflater = new DeflaterOutputStream(blockStream, new Deflater(Deflater.BEST_COMPRESSION, true));
                                    deflater.Write(block, 0, block.Length);
                                    deflater.Close();
                                }
                                else if (type == Type.CompressedGzip)
                                {
                                    GZipOutputStream gzipStream = new GZipOutputStream(blockStream);
                                    gzipStream.Write(block, 0, block.Length);
                                    gzipStream.Close();
                                }

                                compressedBlock = blockStream.ToArray();

                            }
                            compressedBlocks.Add(compressedBlock);
                            if (type == Type.CompressedZlibAlt)
                            {
                                Endian origEndian = writer.Endianness;
                                writer.Endianness = Endian.LittleEndian;
                                writer.WriteUInt32(MAX_BLOCK_SIZE);
                                writer.Endianness = origEndian;
                            }
                            writer.WriteBlock(compressedBlock);
                        }


                        writer.SeekTo(0x8);
                        writer.Endianness = Endian.LittleEndian;
                        writer.WriteUInt32((uint)compressedBlocks.Count);
                        uint maxUncompressedBlockSize = (uint)uncompressedBlockSizes.Max();
                        writer.WriteUInt32(maxUncompressedBlockSize);
                        foreach (var block in compressedBlocks)
                        {
                            if (type == Type.CompressedZlibAlt)
                            {
                                writer.WriteUInt32((uint)block.Length + 4);
                            }
                            else
                            {
                                writer.WriteUInt32((uint)block.Length);
                            }
                        }

                        break;

                    case Type.Uncompressed:
                        dirMeta.Write(writer);

                        writer.SeekTo(0x8);
                        writer.Endianness = Endian.LittleEndian;

                        // Calculate the number of blocks
                        uint totalSize = (uint)writer.BaseStream.Length;
                        uint numBlocks = (totalSize + MAX_BLOCK_SIZE - 1) / MAX_BLOCK_SIZE;

                        writer.WriteUInt32(numBlocks);
                        writer.WriteUInt32(MAX_BLOCK_SIZE);


                        for (int i = 0; i < numBlocks; i++)
                        {
                            if (i == numBlocks - 1)
                            {
                                uint remainingSize = (totalSize % MAX_BLOCK_SIZE) - startingOffset;
                                writer.WriteUInt32(remainingSize == 0 ? MAX_BLOCK_SIZE : remainingSize);
                            }
                            else
                            {
                                writer.WriteUInt32(MAX_BLOCK_SIZE);
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        public override string ToString()
        {
            return string.Format("{0}", filePath);
        }
    }
}
