using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets.UI
{
    [Name("TrackPanelDir"), Description("panel dir that handles tracks & hud")]
    public class TrackPanelDir : PanelDir
    {
        private ushort altRevision;
        private ushort revision;

        private uint unkPreBase;

        public uint viewTimeEasy;
        public uint viewTimeExpert;
        public float netTrackAlpha;

        private uint configurableObjectsCount;
        public List<Symbol> configurableObjects = new();

        public TrackPanelDir(ushort revision, ushort altRevision = 0) : base(revision, altRevision)
        {
            revision = revision;
            altRevision = altRevision;
            return;
        }

        public TrackPanelDir Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)(combinedRevision >> 16 & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)(combinedRevision >> 16 & 0xFFFF));

            unkPreBase = reader.ReadUInt32();

            base.Read(reader, false, parent, entry);

            viewTimeEasy = reader.ReadUInt32();
            viewTimeExpert = reader.ReadUInt32();
            netTrackAlpha = reader.ReadFloat();

            configurableObjectsCount = reader.ReadUInt32();
            for (int i = 0; i < configurableObjectsCount; i++)
            {
                configurableObjects.Add(Symbol.Read(reader));
            }

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw MiloLib.Exceptions.MiloAssetReadException.EndBytesNotFound(parent, entry, reader.BaseStream.Position);

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)(altRevision << 16 | revision) : (uint)(revision << 16 | altRevision));

            writer.WriteUInt32(unkPreBase);

            base.Write(writer, false, parent, entry);

            writer.WriteUInt32(viewTimeEasy);
            writer.WriteUInt32(viewTimeExpert);
            writer.WriteFloat(netTrackAlpha);

            writer.WriteUInt32((uint)configurableObjects.Count);
            foreach (var obj in configurableObjects)
                Symbol.Write(writer, obj);

            if (standalone)
                writer.WriteEndBytes();
        }

        public override bool IsDirectory()
        {
            return true;
        }

    }
}
