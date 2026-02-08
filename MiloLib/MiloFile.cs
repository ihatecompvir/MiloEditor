using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using MiloLib.Assets;
using MiloLib.Utils;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace MiloLib
{
    /// <summary>
    /// Represents a Milo scene (with the extension .milo_{platform}), which is a container for assets and directories.
    /// </summary>
    public class MiloFile
    {
        /// <summary>
        /// The maximum size a block can be.
        /// 0x20000 is what DirLoader::SaveObjects uses, so presumably that is a safe default.
        /// </summary>
        private const int MAX_BLOCK_SIZE = 0x20000;

        /// <summary>
        /// The type of the Milo file. Determines if it's compressed or not and how it's compressed.
        /// </summary>
        public enum Type : uint
        {
            /// <summary>
            /// no compression, root dir starts at startOffset
            /// </summary>
            Uncompressed = 0xCABEDEAF,

            /// <summary>
            /// zlib compressed, without uncompressed size before the start of blocks
            /// </summary>
            CompressedZlib = 0xCBBEDEAF,

            /// <summary>
            /// normal gzip
            /// </summary>
            CompressedGzip = 0xCCBEDEAF,

            /// <summary>
            /// zlib compressed, with uncompressed size before the start of blocks
            /// </summary>
            CompressedZlibAlt = 0xCDBEDEAF,
        }

        /// <summary>
        /// The path to the Milo file.
        /// </summary>
        public string? filePath;

        /// <summary>
        /// The Milo's compression type.
        /// </summary>
        public Type compressionType;

        /// <summary>
        /// The offset to the start of the root asset.
        /// </summary>
        private uint startOffset;

        /// <summary>
        /// Gets the starting offset that was read from the file when it was loaded.
        /// Returns 0 if the file was created new (not loaded from disk).
        /// </summary>
        public uint StartOffset => startOffset;

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
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            filePath = path;

            // buffer entire file into memory first
            byte[] fileBuffer = File.ReadAllBytes(path);

            using (EndianReader reader = new EndianReader(new MemoryStream(fileBuffer), Endian.LittleEndian))
            {
                compressionType = (Type)reader.ReadUInt32();

                // detect if the type is one of the compressed types
                if (compressionType != Type.Uncompressed && compressionType != Type.CompressedZlib && compressionType != Type.CompressedGzip && compressionType != Type.CompressedZlibAlt)
                {
                    // this might be a headerless milo (e.g. Phase .milo_pc) so treat it as such and just start reading the root directory
                    reader.SeekTo(0);
                    dirMeta = new DirectoryMeta().Read(reader);
                    endian = reader.Endianness;
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
                        endian = decompressedReader.Endianness;
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
                        endian = decompressedReader.Endianness;
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
                        endian = decompressedReader.Endianness;
                        break;
                    case Type.Uncompressed:
                        reader.SeekTo(startOffset);

                        reader.Endianness = Endian.BigEndian;

                        meta = new DirectoryMeta();
                        meta.platform = DetectPlatform();
                        dirMeta = meta.Read(reader);
                        endian = reader.Endianness;
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

        private void WriteHandler(object sender, DirectoryMeta.Entry.EntryOperationEventArgs args, uint startingOffset, List<uint> blockSizes, ref uint bytesWritten, ref uint cumulativeBlockSize, MiloFile.Type? type = MiloFile.Type.Uncompressed)
        {
            if (blockSizes.Count == 0)
            {
                if (type == Type.CompressedZlibAlt)
                {
                    bytesWritten = (uint)args.Writer.BaseStream.Position;
                    blockSizes.Add(bytesWritten);
                    cumulativeBlockSize += bytesWritten;
                    bytesWritten = 0;
                    return;
                }
                else
                {
                    bytesWritten = (uint)args.Writer.BaseStream.Position;
                }
            }
            else
            {
                bytesWritten = (uint)args.Writer.BaseStream.Position - cumulativeBlockSize;
            }

            if (bytesWritten > MAX_BLOCK_SIZE)
            {
                blockSizes.Add(bytesWritten);
                cumulativeBlockSize += bytesWritten;
                bytesWritten = 0;
            }
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
        /// <param name="startingOffset">The offset at which the root directory starts. If null, uses the original startOffset from the loaded file, or 0x810 for new files.</param>
        /// <param name="headerEndian">The endianness of the header.</param>
        /// <param name="bodyEndian">The endianness of the body. Certain games require little endian bodies, such as GH2.</param>
        public void Save(string? path, Type? type, uint? startingOffset = null, Endian headerEndian = Endian.LittleEndian, Endian bodyEndian = Endian.BigEndian)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            endian = bodyEndian;
            if (path == null)
            {
                path = filePath;
            }

            // Use the original startOffset if not specified, or 0x810 for new files
            uint actualStartingOffset = startingOffset ?? (startOffset != 0 ? startOffset : 0x810);

            using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
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
                writer.WriteUInt32(actualStartingOffset);
                writer.WriteUInt32(1);

                // block sizes, write nothing for now
                writer.WriteUInt32(0);
                writer.WriteUInt32(0);

                writer.WriteBlock(new byte[actualStartingOffset - (int)writer.BaseStream.Position]);

                // switch to big endian, only the header is little
                writer.Endianness = bodyEndian;
                List<uint> uncompressedBlockSizes = new List<uint>();

                // CREATE UNCOMPRESSED BLOCKS

                MemoryStream uncompressedStream = new MemoryStream();
                EndianWriter uncompressedWriter = new EndianWriter(uncompressedStream, bodyEndian);

                uint bytesWritten = 0;
                uint cumulativeBlockSize = 0;

                // handler fired after any asset is saved
                EventHandler<DirectoryMeta.Entry.EntryOperationEventArgs> handler = (sender, args) =>
                        WriteHandler(sender, args, actualStartingOffset, uncompressedBlockSizes, ref bytesWritten, ref cumulativeBlockSize, type);

                // recursively traverse all entries to add our handler
                foreach (var entry in dirMeta.entries)
                {
                    AddHandlerRecursively(entry, handler);
                }

                // make sure we also traverse inline subdirectories
                if (dirMeta.directory is ObjectDir objectDir)
                {
                    TraverseInlineSubDirs(objectDir, handler);
                }

                dirMeta.Write(uncompressedWriter);

                if (uncompressedBlockSizes.Count == 0)
                {
                    // if we have no uncompressed block sizes, add the size of the entire stream as a single block
                    uncompressedBlockSizes.Add((uint)uncompressedStream.Length);
                }
                else
                {
                    // get the last block's uncompressed size by taking the length of the uncompressed stream and subtracting all the blocks combined
                    uint lastBlockSize = (uint)uncompressedStream.Length - cumulativeBlockSize;
                    if (lastBlockSize > 0)
                    {
                        uncompressedBlockSizes.Add(lastBlockSize);
                    }
                }

                // now that we have the entire uncompressed stream, we can begin splitting it into blocks depending on compression type
                uint maxBlockSize = uncompressedBlockSizes.Max();

                switch (type)
                {
                    case Type.Uncompressed:
                        // if we have no block sizes, write a single block size of the total size
                        if (uncompressedBlockSizes.Count == 0)
                        {
                            uncompressedBlockSizes.Add(bytesWritten);
                        }

                        // write the uncompressed stream to the writer
                        writer.WriteBlock(uncompressedStream.GetBuffer(), 0, (int)uncompressedStream.Length);


                        writer.SeekTo(0x8);
                        writer.Endianness = Endian.LittleEndian;

                        // Calculate the total size of the data (after writing the directory)
                        uint totalSize = (uint)writer.BaseStream.Length;

                        // Calculate the number of blocks
                        writer.WriteUInt32((uint)uncompressedBlockSizes.Count);

                        // get the size of the largest block and write it
                        writer.WriteUInt32(maxBlockSize);

                        foreach (uint blockSize in uncompressedBlockSizes)
                        {
                            writer.WriteUInt32(blockSize);
                        }
                        break;
                    case Type.CompressedZlib:
                    case Type.CompressedGzip:
                        int offset = 0;
                        List<byte[]> compressedBlocks = new();
                        foreach (var block in uncompressedBlockSizes)
                        {
                            using (MemoryStream blockStream = new MemoryStream())
                            {
                                if (type == Type.CompressedGzip)
                                {
                                    // gzip
                                    GZipOutputStream gzipStream = new GZipOutputStream(blockStream);
                                    gzipStream.Write(uncompressedStream.GetBuffer(), offset, (int)block);
                                    gzipStream.Close();
                                    compressedBlocks.Add(blockStream.ToArray());
                                    offset += (int)block;
                                }
                                else
                                {
                                    // deflate (zlib)
                                    // "best compression" and also skip the header
                                    DeflaterOutputStream deflater = new DeflaterOutputStream(blockStream, new Deflater(Deflater.BEST_COMPRESSION, true));
                                    deflater.Write(uncompressedStream.GetBuffer(), offset, (int)block);
                                    deflater.Close();
                                    compressedBlocks.Add(blockStream.ToArray());
                                    offset += (int)block;
                                }
                            }
                        }

                        // write the compressed blocks to the writer
                        foreach (var block in compressedBlocks)
                        {
                            writer.WriteBlock(block);
                        }

                        writer.SeekTo(0x8);
                        writer.Endianness = Endian.LittleEndian;

                        writer.WriteUInt32((uint)compressedBlocks.Count);
                        uint maxUncompressedBlockSize = maxBlockSize;
                        writer.WriteUInt32(maxUncompressedBlockSize);
                        foreach (var block in compressedBlocks)
                        {
                            writer.WriteUInt32((uint)block.Length);
                        }
                        break;
                    case Type.CompressedZlibAlt:
                        offset = 0;
                        compressedBlocks = new();
                        foreach (var block in uncompressedBlockSizes)
                        {
                            using (MemoryStream blockStream = new MemoryStream())
                            {
                                DeflaterOutputStream deflater = new DeflaterOutputStream(blockStream, new Deflater(Deflater.BEST_COMPRESSION, true));
                                deflater.Write(uncompressedStream.GetBuffer(), offset, (int)block);
                                deflater.Close();
                                compressedBlocks.Add(blockStream.ToArray());
                                offset += (int)block;
                            }
                        }
                        // write the compressed blocks to the writer
                        for (int i = 0; i < compressedBlocks.Count; i++)
                        {
                            byte[] block = compressedBlocks[i];
                            if (i == 0)
                            {
                                // first block always uncompressed
                                writer.WriteBlock(uncompressedStream.GetBuffer(), 0, (int)uncompressedBlockSizes[0]);
                            }
                            else
                            {
                                writer.WriteUInt32((uint)block.Length + 4);
                                writer.WriteBlock(block);
                            }
                        }
                        writer.SeekTo(0x8);
                        writer.Endianness = Endian.LittleEndian;
                        writer.WriteUInt32((uint)compressedBlocks.Count);
                        maxUncompressedBlockSize = (uint)uncompressedBlockSizes.Max();
                        writer.WriteUInt32(maxUncompressedBlockSize);
                        for (int i = 0; i < compressedBlocks.Count; i++)
                        {
                            if (i == 0)
                            {
                                // apply flag to the first block to indicate it's uncompressed
                                writer.WriteUInt32(uncompressedBlockSizes[0] | 0x01000000);
                            }
                            else
                            {
                                writer.WriteUInt32((uint)compressedBlocks[i].Length + 4);
                            }
                        }
                        break;
                }
            }
        }

        void AddHandlerRecursively(DirectoryMeta.Entry entry, EventHandler<DirectoryMeta.Entry.EntryOperationEventArgs> handler)
        {
            // Attach the handler to the current entry
            entry.AfterWrite -= handler;
            entry.AfterWrite += handler;

            // If entry has a directory, iterate over its entries
            if (entry.dir != null)
            {
                foreach (var subEntry in entry.dir.entries)
                {
                    AddHandlerRecursively(subEntry, handler);
                }

                // Now check if entry.dir itself is an ObjectDir (cast DirectoryMeta to ObjectDir)
                if (entry.dir is DirectoryMeta dirMeta && dirMeta.directory is ObjectDir objDir)
                {
                    TraverseInlineSubDirs(objDir, handler);
                }
            }
        }

        void TraverseInlineSubDirs(ObjectDir objDir, EventHandler<DirectoryMeta.Entry.EntryOperationEventArgs> handler)
        {
            foreach (var subDir in objDir.inlineSubDirs)
            {
                foreach (var entry in subDir.entries)
                {
                    AddHandlerRecursively(entry, handler);
                }

                // Recursively traverse deeper inline subdirectories
                if (subDir.directory is ObjectDir subObjDir)
                {
                    TraverseInlineSubDirs(subObjDir, handler);
                }
            }
        }

        public override string ToString()
        {
            return string.Format("{0}", filePath);
        }
    }
}