﻿using MiloLib.Classes;
using MiloLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiloLib.Assets
{
    [Name("ObjectDir"), Description("An ObjectDir keeps track of a set of Objects. It can subdir or proxy in other ObjectDirs. To rename subdir or proxy files search for remap_objectdirs in system/run/config/objects.dta")]
    public class ObjectDir : Object
    {
        public enum ReferenceType
        {
            Import = 1,
            SubDir = 3,
        }

        public ushort altRevision;
        public ushort revision;

        [Name("Viewport Count"), Description("The number of viewports."), MinVersion(2)]
        private uint viewportCount;

        [Name("Viewports"), MinVersion(2)]
        public List<Matrix> viewports = new List<Matrix>();

        [Name("Current Viewport Index"), Description("The index of the current viewport."), MinVersion(2)]
        public uint currentViewportIdx;

        [Name("Inline Proxy"), Description("Can this proxy be inlined?"), MinVersion(20)]
        public bool inlineProxy;

        [Name("Proxy Path"), Description("The path to the proxy."), MinVersion(13)]
        public Symbol proxyPath = new(0, "");

        [Name("Sub Directory Count"), Description("The number of subdirectories in the directory."), MinVersion(3)]
        private uint subDirCount;

        [Name("Sub Directories"), Description("Subdirectories of objects"), MinVersion(3)]
        public List<Symbol> subDirs = new List<Symbol>();

        [Name("Inline Sub Directory"), Description("How is this inlined as a subdir? Note that when you change this, you must resave everything subdiring this file for it to take effect"), MinVersion(21)]
        public bool inlineSubDir;

        [Name("Inline Sub Directory Count"), Description("The number of inlined subdirectories in the directory."), MinVersion(21)]
        private uint inlineSubDirCount;

        [Name("Inline Sub Directory Names"), MinVersion(21)]
        public List<Symbol> inlineSubDirNames = new List<Symbol>();

        [Name("Reference Types"), MinVersion(27)]
        public List<ReferenceType> referenceTypes = new List<ReferenceType>();

        [Name("Reference Types Alt"), MinVersion(27)]
        public List<ReferenceType> referenceTypesAlt = new List<ReferenceType>();

        [Name("Inline Sub Directories"), MinVersion(21)]
        public List<DirectoryMeta> inlineSubDirs = new List<DirectoryMeta>();

        [Name("Unknown String 1"), MinVersion(3), MaxVersion(10)]
        public Symbol unknownString = new(0, "");

        [Name("Unknown String 2"), MinVersion(3), MaxVersion(10)]
        public Symbol unknownString2 = new(0, "");

        [Name("Unknown Object Reference 1"), MinVersion(2), MaxVersion(10)]
        public Symbol unknownObjRef1 = new(0, "");

        [Name("Unknown Object Reference 2"), MinVersion(4), MaxVersion(10)]
        public Symbol unknownObjRef2 = new(0, "");

        [Name("Unknown String 3"), MinVersion(5), MaxVersion(5)]
        public Symbol unknownString3 = new(0, "");

        [Name("Unknown String 4"), MinVersion(15), MaxVersion(15)]
        public Symbol unknownString4 = new(0, "");

        [Name("Unknown String 5"), MinVersion(16), MaxVersion(18)]
        public Symbol unknownString5 = new(0, "");

        public ObjectDir Read(EndianReader reader, bool standalone)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            if (revision < 22)
            {
                if (revision >= 2 && revision < 17)
                {
                    objFields.Read(reader);
                }
            }
            else
            {
                objFields.metadataAltRevision = reader.ReadUInt16();
                objFields.metadataRevision = reader.ReadUInt16();
                objFields.type = Symbol.Read(reader);
            }

            if (revision > 1)
            {

                if (revision >= 27)
                {
                    // skip forward 8 bytes
                    reader.BaseStream.Position += 8;
                }

                viewportCount = reader.ReadUInt32();

                // sanity check, there should be no more than 7 viewports
                if (viewportCount > 7)
                {
                    throw new InvalidDataException("Viewport count is too high at " + reader.BaseStream.Position + ", ObjectDir is invalid");
                }

                for (int i = 0; i < viewportCount; i++)
                {
                    Matrix viewport = new Matrix();
                    viewport.Read(reader);
                    viewports.Add(viewport);
                    if (revision <= 17)
                        reader.BaseStream.Position += 4;

                }
                // sanity check, the current viewport index should be less than the viewport count but it can also be 0
                if (currentViewportIdx >= viewportCount && currentViewportIdx != 0)
                {
                    throw new InvalidDataException("Current viewport index is invalid at " + reader.BaseStream.Position + ", ObjectDir is invalid");
                }

                currentViewportIdx = reader.ReadUInt32();
            }

            if (revision > 12)
            {
                if (revision > 19)
                    inlineProxy = reader.ReadBoolean();
                proxyPath = Symbol.Read(reader);
            }

            if (revision >= 2 && revision < 11)
            {
                unknownObjRef1 = Symbol.Read(reader);
            }

            if (revision >= 4 && revision < 11)
            {
                unknownObjRef2 = Symbol.Read(reader);
            }

            if (revision == 5)
            {
                unknownString3 = Symbol.Read(reader);
            }

            if (revision > 2)
            {
                subDirCount = reader.ReadUInt32();

                // sanity check, there should be no more than 100 subdirs
                // TODO: double check if the game has an assert for this and use that if present
                if (subDirCount > 100)
                {
                    throw new InvalidDataException("Subdir count is too high at " + reader.BaseStream.Position + ", ObjectDir is invalid");
                }

                for (int i = 0; i < subDirCount; i++)
                {
                    Symbol subDir = Symbol.Read(reader);
                    subDirs.Add(subDir);
                }

                if (revision >= 21)
                {
                    inlineSubDir = reader.ReadBoolean();
                    inlineSubDirCount = reader.ReadUInt32();

                    // sanity check, there should be no more than 100 inlined subdirs
                    if (inlineSubDirCount > 100)
                    {
                        throw new InvalidDataException("Inlined subdir count is too high at " + reader.BaseStream.Position + ", ObjectDir is invalid");
                    }

                    if (inlineSubDirCount > 0)
                    {
                        for (int i = 0; i < inlineSubDirCount; i++)
                        {
                            Symbol inlineSubDirName = Symbol.Read(reader);
                            inlineSubDirNames.Add(inlineSubDirName);
                        }

                        if (revision >= 27)
                        {
                            for (int i = 0; i < inlineSubDirCount; i++)
                            {
                                ReferenceType referenceType = (ReferenceType)reader.ReadByte();
                                referenceTypes.Add(referenceType);
                            }

                            for (int i = 0; i < inlineSubDirCount; i++)
                            {
                                ReferenceType referenceTypeAlt = (ReferenceType)reader.ReadByte();
                                referenceTypesAlt.Add(referenceTypeAlt);
                            }
                        }

                        for (int i = 0; i < inlineSubDirCount; i++)
                        {
                            DirectoryMeta inlineSubDir = new DirectoryMeta();
                            inlineSubDir.Read(reader);
                            inlineSubDirs.Add(inlineSubDir);
                        }

                    }
                }
            }

            if (revision < 19)
            {
                if (revision < 16)
                {
                    if (revision > 14)
                    {
                        unknownString4 = Symbol.Read(reader);
                    }
                }
                else
                {
                    unknownString5 = Symbol.Read(reader);
                }
            }



            unknownString = Symbol.Read(reader);
            unknownString2 = Symbol.Read(reader);

            if (revision < 22)
            {
                if (revision > 16)
                {
                    objFields.Read(reader);
                }
            }
            else
            {
                objFields.hasTree = reader.ReadBoolean();

                if (objFields.hasTree)
                {
                    objFields.root.Read(reader);
                }

                if (revision >= 25)
                {
                    objFields.note = Symbol.Read(reader);
                }
            }

            if (standalone)
            {
                // read past padding
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw new Exception("Got to end of standalone asset but didn't find the expected end bytes, read likely did not succeed");
            }

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));
            writer.WriteUInt32(objFields.metadataRevision);
            Symbol.Write(writer, objFields.type);

            // write 8 empty bytes
            writer.WriteBlock(new byte[8]);

            writer.WriteUInt32(viewportCount);
            foreach (Matrix viewport in viewports)
            {
                viewport.Write(writer);
                if (revision <= 17)
                    writer.WriteUInt32(0);
            }
            writer.WriteUInt32(currentViewportIdx);

            writer.WriteBoolean(inlineProxy);
            Symbol.Write(writer, proxyPath);

            writer.WriteUInt32(subDirCount);
            foreach (Symbol subDir in subDirs)
            {
                Symbol.Write(writer, subDir);
            }

            writer.WriteBoolean(inlineSubDir);
            writer.WriteUInt32(inlineSubDirCount);
            foreach (Symbol inlineSubDirName in inlineSubDirNames)
            {
                Symbol.Write(writer, inlineSubDirName);
            }

            foreach (ReferenceType referenceType in referenceTypes)
            {
                writer.WriteByte((byte)referenceType);
            }

            foreach (ReferenceType referenceTypeAlt in referenceTypesAlt)
            {
                writer.WriteByte((byte)referenceTypeAlt);
            }

            foreach (DirectoryMeta inlineSubDir in inlineSubDirs)
            {
                inlineSubDir.Write(writer);
            }

            Symbol.Write(writer, unknownString);
            Symbol.Write(writer, unknownString2);

            writer.WriteByte(objFields.hasTree ? (byte)1 : (byte)0);

            Symbol.Write(writer, objFields.note);

            if (standalone)
            {
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
            }
        }

        public override bool IsDirectory()
        {
            return true;
        }
    }
}
