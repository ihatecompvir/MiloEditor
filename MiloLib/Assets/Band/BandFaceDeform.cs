using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets.Band
{
    [Name("BandFaceDeform"), Description("Band Face Deformation object for face creator, basically a compact MeshAnim with position deltas")]
    public class BandFaceDeform : Object
    {
        public class DeltaArray
        {
            [Name("Size"), Description("Size in bytes this takes up")]
            private uint size;
            public List<byte> data = new();

            public DeltaArray Read(EndianReader reader)
            {
                size = reader.ReadUInt32();
                for (int i = 0; i < size; i++)
                {
                    data.Add(reader.ReadByte());
                }

                return this;
            }

            public void Write(EndianWriter writer)
            {
                writer.WriteUInt32((uint)data.Count);
                foreach (var b in data)
                {
                    writer.WriteByte(b);
                }
            }
        }
        private ushort altRevision;
        private ushort revision;

        private uint frameCount;
        [Name("Frames"), Description("number of vertices with non-zero deltas in this keyframe")]
        public List<DeltaArray> frames = new();

        public BandFaceDeform Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            base.Read(reader, false, parent, entry);

            frameCount = reader.ReadUInt32();
            for (int i = 0; i < frameCount; i++)
            {
                frames.Add(new DeltaArray().Read(reader));
            }

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw MiloLib.Exceptions.MiloAssetReadException.EndBytesNotFound(parent, entry, reader.BaseStream.Position);

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            base.Write(writer, false, parent, entry);

            writer.WriteUInt32((uint)frames.Count);
            foreach (var frame in frames)
            {
                frame.Write(writer);
            }

            if (standalone)
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
        }

    }
}
