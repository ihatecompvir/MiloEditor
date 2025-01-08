using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets.Band
{
    [Name("BandConfiguration"), Description("")]
    public class BandConfiguration : Object
    {
        private Dictionary<Game.MiloGame, uint> gameRevisions = new Dictionary<Game.MiloGame, uint>
        {
            { Game.MiloGame.RockBand3, 3 },
        };

        private ushort altRevision;
        private ushort revision;

        private uint targTransformCount;
        public List<TargTransform> transforms = new();

        public class TargTransform
        {
            public Symbol target = new(0, "");
            public Matrix xfm = new();

            public TargTransform Read(EndianReader reader)
            {
                target = Symbol.Read(reader);
                xfm = new Matrix().Read(reader);
                return this;
            }

            public void Write(EndianWriter writer)
            {
                Symbol.Write(writer, target);
                xfm.Write(writer);
            }

            public override string ToString()
            {
                return $"{target} {xfm}";
            }
        }

        public BandConfiguration Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)(combinedRevision >> 16 & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)(combinedRevision >> 16 & 0xFFFF));

            base.Read(reader, false, parent, entry);

            targTransformCount = reader.ReadUInt32();
            // read 4 targtransforms per count
            for (int i = 0; i < targTransformCount * 4; i++)
            {
                TargTransform transform = new();
                transform.Read(reader);
                transforms.Add(transform);
            }


            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw new Exception("Got to end of standalone asset but didn't find the expected end bytes, read likely did not succeed");

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)(altRevision << 16 | revision) : (uint)(revision << 16 | altRevision));

            base.Write(writer, false, parent, entry);

            writer.WriteUInt32(targTransformCount);
            foreach (TargTransform transform in transforms)
                transform.Write(writer);

            if (standalone)
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
        }

    }
}
