using MiloLib.Assets.World;
using MiloLib.Utils;
using MiloLib.Classes;

namespace MiloLib.Assets.Rnd
{
    [Name("RndDir"), Description("A RndDir specially tracks drawable and animatable perObjs.")]
    public class RndDir : ObjectDir
    {
        private ushort altRevision;
        private ushort revision;

        [Name("Anim")]
        public RndAnimatable anim = new();
        [Name("Draw")]
        public RndDrawable draw = new();
        [Name("Trans")]
        public RndTrans trans = new();

        [Name("Environ"), MinVersion(9)]
        public Symbol environ = new(0, "");

        [Name("Test Event"), Description("Test event"), MinVersion(10)]
        public Symbol testEvent = new(0, "");

        [Name("Unknown Floats"), Description("Unknown floats only found in the GH2 4-song demo."), MinVersion(6), MaxVersion(6)]
        public List<float> unknownFloats = new();

        [MinVersion(0), MaxVersion(8)]
        public RndPollable poll = new();
        [MinVersion(0), MaxVersion(8)]
        public Symbol unkSymbol1 = new(0, "");
        [MinVersion(0), MaxVersion(8)]
        public Symbol unkSymbol2 = new(0, "");

        public RndDir(ushort revision, ushort altRevision = 0) : base(revision, altRevision)
        {
            revision = revision;
            altRevision = altRevision;
            return;
        }

        public RndDir Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            base.Read(reader, false, parent, entry);

            // the RndDir ends immediately when it is an entry unless the entry is a RndDir or Character, probably others too, why?
            // TODO: investigate if this is just for RB3/DC1 or others too
            if (entry.isProxy && entry.type.value != "Character" && entry.type.value != "RndDir" && entry.type.value != "BandCrowdMeterDir" && entry.type.value != "CrowdMeterIcon" && entry.type.value != "EndingBonusDir" && entry.type.value != "UnisonIcon" && entry.type.value != "BandScoreboard" && entry.type.value != "BandStarDisplay" && entry.type.value != "PanelDir" && entry.type.value != "MoveDir" && entry.type.value != "SkeletonDir" && entry.type.value != "WorldDir" && entry.type.value != "VocalTrackDir" && entry.type.value != "OvershellDir" && entry.type.value != "BandCharacter" && entry.type.value != "GemTrackDir" && entry.type.value != "OverdriveMeterDir" && entry.type.value != "StreakMeterDir" && entry.type.value != "PitchArrowDir")
            {
                if (!(this is WorldInstance wiRev0Read && wiRev0Read.revision == 0))
                    return this;
            }

            anim = anim.Read(reader, parent, entry);
            draw = draw.Read(reader, false, parent, entry);
            if (revision != 0)
                trans = trans.Read(reader, false, parent, entry);

            if (revision < 9)
            {
                poll = poll.Read(reader, false, parent, entry);
                unkSymbol1 = Symbol.Read(reader);
                unkSymbol2 = Symbol.Read(reader);
            }
            else
            {
                environ = Symbol.Read(reader);
                if (revision > 2 && revision != 9)
                    testEvent = Symbol.Read(reader);
            }

            if (revision == 6)
            {
                for (int i = 0; i < 8; i++)
                {
                    unknownFloats.Add(reader.ReadFloat());
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

            if (entry.isProxy && entry.type.value != "Character" && entry.type.value != "RndDir" && entry.type.value != "BandCrowdMeterDir" && entry.type.value != "CrowdMeterIcon" && entry.type.value != "EndingBonusDir" && entry.type.value != "UnisonIcon" && entry.type.value != "BandScoreboard" && entry.type.value != "BandStarDisplay" && entry.type.value != "PanelDir" && entry.type.value != "MoveDir" && entry.type.value != "SkeletonDir" && entry.type.value != "WorldDir" && entry.type.value != "VocalTrackDir" && entry.type.value != "OvershellDir" && entry.type.value != "BandCharacter" && entry.type.value != "GemTrackDir" && entry.type.value != "OverdriveMeterDir" && entry.type.value != "StreakMeterDir" && entry.type.value != "PitchArrowDir")
            {
                // hack
                if (!(this is WorldInstance rev0WorldInstance && rev0WorldInstance.revision == 0))
                    return;
            }

            anim.Write(writer);
            draw.Write(writer, false, parent, true);
            trans.Write(writer, false, parent, true);

            if (revision < 9)
            {
                poll.Write(writer, false, parent, entry);
                Symbol.Write(writer, unkSymbol1);
                Symbol.Write(writer, unkSymbol2);
            }
            else
            {
                Symbol.Write(writer, environ);
                if (revision > 2 && revision != 9)
                    Symbol.Write(writer, testEvent);
            }

            if (revision == 6)
            {
                for (int i = 0; i < 8; i++)
                {
                    writer.WriteFloat(unknownFloats[i]);
                }
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
