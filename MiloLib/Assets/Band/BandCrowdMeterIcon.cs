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
    [Name("BandCrowdMeterIcon"), Description("Individual player icon for crowd meter")]
    public class BandCrowdMeterIcon : RndDir
    {
        private Dictionary<Game.MiloGame, uint> gameRevisions = new Dictionary<Game.MiloGame, uint>
        {
            { Game.MiloGame.GuitarHero2_PS2, 0 },
            { Game.MiloGame.GuitarHero2_360, 0 },
            { Game.MiloGame.Phase, 0 },
            { Game.MiloGame.RockBand, 0 },
            { Game.MiloGame.RockBand2, 0 },
            { Game.MiloGame.LegoRockBand, 0 },
            { Game.MiloGame.TheBeatlesRockBand, 0 },
            { Game.MiloGame.GreenDayRockBand, 0 },
            { Game.MiloGame.RockBand3, 0 },
            { Game.MiloGame.DanceCentral, 0 },
            { Game.MiloGame.DanceCentral2, 0 },
            { Game.MiloGame.RockBandBlitz, 0 },
            { Game.MiloGame.DanceCentral3, 0 }
        };

        public ushort altRevision;
        public ushort revision;

        public BandCrowdMeterIcon(ushort revision, ushort altRevision = 0) : base(revision, altRevision)
        {
            revision = revision;
            altRevision = altRevision;
            return;
        }

        public BandCrowdMeterIcon Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)(combinedRevision >> 16 & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)(combinedRevision >> 16 & 0xFFFF));

            if (revision > 0)
            {
                throw new UnsupportedAssetRevisionException("CrowdMeterDir", revision);
            }

            base.Read(reader, false, parent, entry);

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw new Exception("Got to end of standalone asset but didn't find the expected end bytes, read likely did not succeed");

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)(altRevision << 16 | revision) : (uint)(revision << 16 | altRevision));

            base.Write(writer, false, parent, entry);

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
