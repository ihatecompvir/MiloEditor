using MiloLib.Assets.Rnd;
using MiloLib.Classes;
using MiloLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiloLib.Assets.Band
{
    [Name("BandCrowdMeterDir"), Description("Crowd meter hud element, has a needle")]
    public class BandCrowdMeterDir : RndDir
    {
        public ushort altRevision;
        public ushort revision;

        private uint colorCount;

        [MinVersion(2)]
        public List<HmxColor> colors = new();

        [MinVersion(1)]
        public float peakValue;

        private float groupCount;

        [MaxVersion(2)]
        private List<Symbol> groups = new();

        public BandCrowdMeterDir Read(EndianReader reader, bool standalone)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            // if revision is greater than 3
            if (revision > 3)
            {
                throw new UnsupportedAssetRevisionException("BandCrowdMeterDir", revision);
            }

            if (revision < 3)
            {
                groupCount = reader.ReadUInt32();
                for (int i = 0; i < groupCount; i++)
                {
                    groups.Add(Symbol.Read(reader));
                }
            }

            if (revision >= 2)
            {
                colorCount = reader.ReadUInt32();
                for (int i = 0; i < colorCount; i++)
                {
                    colors.Add(new HmxColor().Read(reader));
                }
            }

            if (revision >= 1)
                peakValue = reader.ReadFloat();

            base.Read(reader, false);

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw new Exception("Got to end of standalone asset but didn't find the expected end bytes, read likely did not succeed");

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            if (revision < 3)
            {
                writer.WriteUInt32((uint)groups.Count);
                foreach (var group in groups)
                {
                    Symbol.Write(writer, group);
                }
            }

            if (revision >= 2)
            {
                writer.WriteUInt32((uint)colors.Count);
                foreach (var color in colors)
                {
                    color.Write(writer);
                }
            }

            if (revision >= 1)
                writer.WriteFloat(peakValue);

            base.Write(writer, false);

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
