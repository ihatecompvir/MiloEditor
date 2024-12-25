using MiloLib.Classes;
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

        public uint revision;

        [Name("Viewport Count"), Description("The number of viewports.")]
        private uint viewportCount;
        [Name("Viewports")]
        public List<Matrix> viewports = new List<Matrix>();
        [Name("Current Viewport Index"), Description("The index of the current viewport.")]
        public uint currentViewportIdx;

        [Name("Inline Proxy"), Description("Can this proxy be inlined?")]
        public bool inlineProxy;
        [Name("Proxy Path"), Description("The path to the proxy.")]
        public Symbol proxyPath = new(0, "");

        [Name("Sub Directory Count"), Description("The number of subdirectories in the directory.")]
        private uint subDirCount;

        [Name("Sub Directories"), Description("Subdirectories of objects")]
        public List<Symbol> subDirs = new List<Symbol>();

        [Name("Inline Sub Directory"), Description("How is this inlined as a subdir?  Note that when you change this, you must resave everything subdiring this file for it to take effect")]
        public bool inlineSubDir;

        [Name("Inline Sub Directory Count"), Description("The number of inlined subdirectories in the directory.")]
        private uint inlineSubDirCount;

        [Name("Inline Sub Directory Names")]
        public List<Symbol> inlineSubDirNames = new List<Symbol>();

        [Name("Reference Types")]
        public List<ReferenceType> referenceTypes = new List<ReferenceType>();
        [Name("Reference Types Alt")]
        public List<ReferenceType> referenceTypesAlt = new List<ReferenceType>();

        [Name("Inline Sub Directories")]
        public List<DirectoryMeta> inlineSubDirs = new List<DirectoryMeta>();

        public Symbol unknownString = new(0, "");
        public Symbol unknownString2 = new(0, "");

        public ObjectDir Read(EndianReader reader, bool standalone)
        {
            revision = reader.ReadUInt32();
            objFields.metadataRevision = reader.ReadUInt32();
            objFields.type = Symbol.Read(reader);

            // skip forward 8 bytes
            reader.BaseStream.Position += 8;

            viewportCount = reader.ReadUInt32();

            // sanity check, there should be no more than 7 viewports
            if (viewportCount > 7)
            {
                throw new InvalidDataException("Viewport count is too high, ObjectDir is invalid");
            }

            for (int i = 0; i < viewportCount; i++)
            {
                Matrix viewport = new Matrix();
                viewport.Read(reader);
                viewports.Add(viewport);
            }

            // sanity check, the current viewport index should be less than the viewport count but it can also be 0
            if (currentViewportIdx >= viewportCount && currentViewportIdx != 0)
            {
                throw new InvalidDataException("Current viewport index is invalid, ObjectDir is invalid");
            }

            currentViewportIdx = reader.ReadUInt32();

            inlineProxy = reader.ReadBoolean();
            proxyPath = Symbol.Read(reader);

            subDirCount = reader.ReadUInt32();

            // sanity check, there should be no more than 100 subdirs
            // TODO: double check if the game has an assert for this and use that if present
            if (subDirCount > 100)
            {
                throw new InvalidDataException("Subdir count is too high, ObjectDir is invalid");
            }

            for (int i = 0; i < subDirCount; i++)
            {
                Symbol subDir = Symbol.Read(reader);
                subDirs.Add(subDir);
            }

            inlineSubDir = reader.ReadBoolean();
            inlineSubDirCount = reader.ReadUInt32();

            // sanity check, there should be no more than 100 inlined subdirs
            if (inlineSubDirCount > 100)
            {
                throw new InvalidDataException("Inlined subdir count is too high, ObjectDir is invalid");
            }

            for (int i = 0; i < inlineSubDirCount; i++)
            {
                Symbol inlineSubDirName = Symbol.Read(reader);
                inlineSubDirNames.Add(inlineSubDirName);
            }

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

            for (int i = 0; i < inlineSubDirCount; i++)
            {
                DirectoryMeta inlineSubDir = new DirectoryMeta();
                inlineSubDir.Read(reader);
                inlineSubDirs.Add(inlineSubDir);
            }

            unknownString = Symbol.Read(reader);
            unknownString2 = Symbol.Read(reader);

            objFields.hasTree = reader.ReadBoolean();

            if (objFields.hasTree)
            {
                objFields.root.Read(reader);
            }

            objFields.note = Symbol.Read(reader);

            if (standalone)
            {
                // read past padding
                reader.BaseStream.Position += 4;
            }

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone)
        {
            writer.WriteUInt32(revision);
            writer.WriteUInt32(objFields.metadataRevision);
            Symbol.Write(writer, objFields.type);

            // write 8 empty bytes
            writer.WriteBlock(new byte[8]);

            writer.WriteUInt32(viewportCount);
            foreach (Matrix viewport in viewports)
            {
                viewport.Write(writer);
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
