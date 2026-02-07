using MiloLib.Classes;
using MiloLib.Utils;
using System.Numerics;

namespace MiloLib.Assets.Ham
{
    [Name("MoveDir"), Description("Dir for HamMoves, contains debugging functionality")]
    public class MoveDir : SkeletonDir
    {
        private ushort altRevision;
        private ushort revision;

        public uint unkInt1;
        public uint unkInt2;
        public uint unkInt3;
        public uint unkInt4;
        public bool mFiltersEnabled;
        public bool mMoveOverlayEnabled;
        public int mDebugNodeTypes;
        public Symbol mImportClipPath;
        public Symbol mFilterVersion;

        public MoveDir(ushort revision, ushort altRevision = 0) : base(revision, altRevision)
        {
            revision = revision;
            altRevision = altRevision;
            return;
        }

        public MoveDir Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            base.Read(reader, false, parent, entry);

            mMoveOverlayEnabled = reader.ReadBoolean();
            mDebugNodeTypes = reader.ReadInt32();
            mImportClipPath = Symbol.Read(reader);

            if (entry != null && entry.isProxy)
            {
                unkBool = reader.ReadBoolean();
            }

            mFilterVersion = Symbol.Read(reader);

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw MiloLib.Exceptions.MiloAssetReadException.EndBytesNotFound(parent, entry, reader.BaseStream.Position);

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            base.Write(writer, false, parent, entry);

            writer.WriteBoolean(mMoveOverlayEnabled);
            writer.WriteInt32(mDebugNodeTypes);
            Symbol.Write(writer, mImportClipPath);
            if (entry != null && entry.isProxy)
            {
                writer.WriteBoolean(unkBool);
            }
            Symbol.Write(writer, mFilterVersion);



            if (standalone)
                writer.WriteEndBytes();
        }

        public override bool IsDirectory()
        {
            return true;
        }

    }
}
