﻿using MiloLib.Classes;
using MiloLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiloLib.Assets
{
    [Name("ObjectDir"), Description("An ObjectDir keeps track of a set of Objects. It can subdir or proxy in other ObjectDirs. To rename subdir or proxy files search for remap_objectdirs in system/run/config/perObjs.dta")]
    public class ObjectDir : Object
    {
        private Dictionary<Game.MiloGame, uint> gameRevisions = new Dictionary<Game.MiloGame, uint>
        {
            // no dirs before this
            { Game.MiloGame.GuitarHero2_PS2, 16 },
            { Game.MiloGame.GuitarHero2_360, 17 },
            { Game.MiloGame.Phase, 17 },
            { Game.MiloGame.RockBand, 19 },
            { Game.MiloGame.RockBand2, 20 },
            { Game.MiloGame.LegoRockBand, 20 },
            { Game.MiloGame.TheBeatlesRockBand, 22 },
            { Game.MiloGame.GreenDayRockBand, 22 },
            { Game.MiloGame.RockBand3, 27 },
            { Game.MiloGame.DanceCentral, 27 },
            { Game.MiloGame.DanceCentral2, 28 },
            { Game.MiloGame.RockBandBlitz, 28 },
            { Game.MiloGame.DanceCentral3, 28 }
        };

        public enum ReferenceType : byte
        {
            kInlineNever = 0,
            kInlineCached = 1,
            kInlineAlways = 2,
            kInlineCachedShared = 3
        }

        private ushort altRevision;
        private ushort revision;

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

        [Name("Sub Directories"), Description("Subdirectories of perObjs"), MinVersion(3)]
        public List<Symbol> subDirs = new List<Symbol>();

        [Name("Inline Sub Directory"), Description("How is this inlined as a subdir? Note that when you change this, you must resave everything subdiring this file for it to take effect"), MinVersion(21)]
        public ReferenceType inlineSubDir;

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

        [Name("Unknown Camera Reference"), MinVersion(3), MaxVersion(10)]
        public Symbol unknownCamReference = new(0, "");

        [Name("Unknown Object Reference 1"), MinVersion(2), MaxVersion(10)]
        public Symbol unknownObjRef1 = new(0, "");

        [Name("Current Camera"), MinVersion(4), MaxVersion(10)]
        public Symbol currentCamera = new(0, "");

        [Name("Unknown String 3"), MinVersion(5), MaxVersion(5)]
        public Symbol unknownString3 = new(0, "");

        [Name("Unknown String 4"), MinVersion(15), MaxVersion(15)]
        public Symbol unknownString4 = new(0, "");

        [Name("Unknown String 5"), MinVersion(16), MaxVersion(18)]
        public Symbol unknownString5 = new(0, "");

        public List<uint> unkInts = new();

        public uint unk1;
        public uint unk2;

        // this shouldn't be under ObjectDir
        // TODO: Put this under WorldInstance where it belongs
        public bool hasPersistentObjects = false;

        public ObjectDir(ushort revision, ushort altRevision = 0)
        {
            this.revision = (ushort)revision;
            this.altRevision = (ushort)altRevision;
            return;
        }

        public ObjectDir Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            if (revision < 22)
            {
                if (revision >= 2 && revision < 17)
                {
                    objFields.Read(reader, parent, entry);
                }
            }
            else
            {
                if (revision != 26)
                {
                    objFields.altRevision = reader.ReadUInt16();
                    objFields.revision = reader.ReadUInt16();
                    objFields.type = Symbol.Read(reader);
                }
                else
                {
                    objFields.Read(reader, parent, entry);
                }
            }

            if (revision > 1)
            {

                if (revision >= 27)
                {
                    unk1 = reader.ReadUInt32();
                    unk2 = reader.ReadUInt32();
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
                        unkInts.Add(reader.ReadUInt32());

                }

                currentViewportIdx = reader.ReadUInt32();

                // sanity check, the current viewport index should be less than the viewport count but it can also be 0
                if (currentViewportIdx >= viewportCount + 1 && currentViewportIdx != 0)
                {
                    throw new InvalidDataException("Current viewport index is invalid at " + reader.BaseStream.Position + ", ObjectDir is invalid");
                }
            }

            if (revision > 12)
            {
                if (revision > 19)
                {
                    if(revision < 28) {
                        inlineProxy = reader.ReadBoolean();
                    }
                    else {
                        inlineProxy = Convert.ToBoolean(reader.ReadByte());
                    }
                }
                proxyPath = Symbol.Read(reader);
            }

            if (revision >= 2 && revision < 11)
            {
                unknownObjRef1 = Symbol.Read(reader);
            }

            if (revision >= 4 && revision < 11)
            {
                currentCamera = Symbol.Read(reader);
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
                    inlineSubDir = (ReferenceType)reader.ReadByte();
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

                        if (revision >= 26)
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
                            DirectoryMeta inlinedSubDir = new DirectoryMeta();
                            inlinedSubDir.platform = parent.platform;
                            // check if the reference types are actually there to avoid an out of bounds exception
                            if (referenceTypes.Count > 0 && referenceTypesAlt.Count > 0)
                            {
                                if (referenceTypes[0] == ReferenceType.kInlineCached && referenceTypesAlt[0] == ReferenceType.kInlineCached)
                                {
                                    reader.ReadBoolean();
                                }
                            }
                            if (referenceTypes[0] == ReferenceType.kInlineCachedShared && referenceTypesAlt[0] == ReferenceType.kInlineCached)
                            {
                            }
                            else
                            {
                                inlinedSubDir.Read(reader);
                                inlineSubDirs.Add(inlinedSubDir);
                            }
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


            if (entry.type.value == "WorldInstance")
            {
                hasPersistentObjects = reader.ReadBoolean();
                if (entry.isProxy)
                    return this;
            }

            unknownString = Symbol.Read(reader);
            unknownCamReference = Symbol.Read(reader);

            if (revision < 22)
            {
                if (revision > 16)
                {
                    objFields.Read(reader, parent, entry);
                }
            }
            else
            {
                objFields.root.Read(reader);

                objFields.note = Symbol.Read(reader);
            }

            if (standalone)
            {
                // read past padding
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw new Exception("Got to end of standalone asset but didn't find the expected end bytes, read likely did not succeed");
            }

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            if (revision < 22)
            {
                if (revision >= 2 && revision < 17)
                {
                    objFields.Write(writer);
                }
            }
            else
            {
                writer.WriteUInt16(objFields.altRevision);
                writer.WriteUInt16(objFields.revision);
                Symbol.Write(writer, objFields.type);
            }

            if (revision > 1)
            {
                if (revision >= 27)
                {
                    writer.WriteUInt32(unk1);
                    writer.WriteUInt32(unk2);
                }

                writer.WriteUInt32((uint)viewports.Count);
                foreach (var viewport in viewports)
                {
                    viewport.Write(writer);
                    if (revision <= 17)
                        writer.WriteUInt32(unkInts[viewports.IndexOf(viewport)]);
                }

                writer.WriteUInt32(currentViewportIdx);
            }

            if (revision > 12)
            {
                if (revision > 19)
                    writer.WriteBoolean(inlineProxy);
                Symbol.Write(writer, proxyPath);
            }

            if (revision >= 2 && revision < 11)
            {
                Symbol.Write(writer, unknownObjRef1);
            }

            if (revision >= 4 && revision < 11)
            {
                Symbol.Write(writer, currentCamera);
            }

            if (revision == 5)
            {
                Symbol.Write(writer, unknownString3);
            }

            if (revision > 2)
            {
                writer.WriteUInt32((uint)subDirs.Count);
                foreach (var subDir in subDirs)
                {
                    Symbol.Write(writer, subDir);
                }

                if (revision >= 21)
                {
                    writer.WriteByte((byte)inlineSubDir);
                    writer.WriteUInt32((uint)inlineSubDirs.Count);
                    foreach (var inlineSubDirName in inlineSubDirNames)
                    {
                        Symbol.Write(writer, inlineSubDirName);
                    }

                    if (revision >= 27)
                    {
                        foreach (var referenceType in referenceTypes)
                        {
                            writer.WriteByte((byte)referenceType);
                        }

                        foreach (var referenceTypeAlt in referenceTypesAlt)
                        {
                            writer.WriteByte((byte)referenceTypeAlt);
                        }
                    }


                    foreach (var inlineSubDir in inlineSubDirs)
                    {
                        // bounds check
                        if (referenceTypes.Count > 0 && referenceTypesAlt.Count > 0)
                        {
                            if (referenceTypes[0] == ReferenceType.kInlineCached && referenceTypesAlt[0] == ReferenceType.kInlineCached)
                            {
                                writer.WriteBoolean(false);
                            }
                        }
                        inlineSubDir.Write(writer);
                    }
                }
            }

            if (revision < 19)
            {
                if (revision < 16)
                {
                    if (revision > 14)
                    {
                        Symbol.Write(writer, unknownString4);
                    }
                }
                else
                {
                    Symbol.Write(writer, unknownString5);
                }
            }

            if (entry != null && entry.type.value == "WorldInstance")
            {
                writer.WriteBoolean(hasPersistentObjects);
                if (entry.isProxy)
                    return;
            }

            Symbol.Write(writer, unknownString);
            Symbol.Write(writer, unknownCamReference);

            if (revision < 22)
            {
                if (revision > 16)
                {
                    objFields.Write(writer);
                }
            }
            else
            {
                objFields.root.Write(writer);

                Symbol.Write(writer, objFields.note);
            }

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
