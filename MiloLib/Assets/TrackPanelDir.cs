using MiloLib.Assets.UI;
using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets
{
    [Name("TrackPanelDirBase")]
    public class TrackPanelDirBase : PanelDir
    {
        private ushort altRevision;
        private ushort revision;

        public float viewTimeEasy;
        public float viewTimeExpert;
        public float netTrackAlpha;

        private uint configurableObjectsCount;
        public List<Symbol> configurableObjects = new();

        public TrackPanelDirBase(ushort revision, ushort altRevision = 0) : base(revision, altRevision)
        {
            revision = revision;
            altRevision = altRevision;
            return;
        }

        public TrackPanelDirBase Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            base.Read(reader, false, parent, entry);

            if (!entry.isProxy)
            {
                viewTimeEasy = reader.ReadFloat();
                viewTimeExpert = reader.ReadFloat();
                netTrackAlpha = reader.ReadFloat();

                configurableObjectsCount = reader.ReadUInt32();
                for (int i = 0; i < configurableObjectsCount; i++)
                {
                    configurableObjects.Add(Symbol.Read(reader));
                }
            }

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw MiloLib.Exceptions.MiloAssetReadException.EndBytesNotFound(parent, entry, reader.BaseStream.Position);

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            base.Write(writer, false, parent, entry);

            if (!entry.isProxy)
            {
                writer.WriteFloat(viewTimeEasy);
                writer.WriteFloat(viewTimeExpert);
                writer.WriteFloat(netTrackAlpha);

                writer.WriteUInt32((uint)configurableObjects.Count);
                foreach (var configurableObject in configurableObjects)
                {
                    Symbol.Write(writer, configurableObject);
                }
            }

            if (standalone)
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
        }

        public override bool IsDirectory()
        {
            return true;
        }

    }
}
