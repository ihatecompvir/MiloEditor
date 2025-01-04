using MiloLib.Classes;
using MiloLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiloLib.Assets.Band
{
    [Name("BandSongPref"), Description("Band Song Preferences, per song file settable properties")]
    public class BandSongPref : Object
    {
        private Dictionary<Game.MiloGame, uint> gameRevisions = new Dictionary<Game.MiloGame, uint>
        {
            // this is only for RB3
            { Game.MiloGame.RockBand3, 3 },
        };

        public ushort altRevision;
        public ushort revision;

        [Name("Part 2 Instrument"), Description("Who should sing the vocal part2?")]
        public Symbol part2Instrument = new(0, "");

        [Name("Part 2 Instrument"), Description("Who should sing the vocal part3?")]
        public Symbol part3Instrument = new(0, "");

        [Name("Part 2 Instrument"), Description("Who should sing the vocal part4?")]
        public Symbol part4Instrument = new(0, "");

        [Name("Animation Genre"), Description("Animation genre for the song")]
        public Symbol animationGenre = new(0, "");

        public new BandSongPref Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            if (revision != 3)
            {
                throw new UnsupportedAssetRevisionException("BandSongPref", revision);
            }

            base.Read(reader, false, parent, entry);

            part2Instrument = Symbol.Read(reader);
            part3Instrument = Symbol.Read(reader);
            part4Instrument = Symbol.Read(reader);
            animationGenre = Symbol.Read(reader);

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw new Exception("Got to end of standalone asset but didn't find the expected end bytes, read likely did not succeed");


            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));
            objFields.Write(writer);

            Symbol.Write(writer, part2Instrument);
            Symbol.Write(writer, part3Instrument);
            Symbol.Write(writer, part4Instrument);
            Symbol.Write(writer, animationGenre);

            if (standalone)
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
        }

    }
}
