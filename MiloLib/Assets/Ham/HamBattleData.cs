using System.Diagnostics;
using System.Numerics;
using MiloLib.Assets.Rnd;
using MiloLib.Classes;
using MiloLib.Utils;
using static MiloLib.Assets.Ham.PracticeSection;

namespace MiloLib.Assets.Ham {
    struct Range {
        int start;
        int end;

        public Range Read(EndianReader reader) {
            start = reader.ReadInt32();
            end = reader.ReadInt32();
            return this;
        }
    }
    public class HamBattleData : Object {

        private ushort altRevision;
        private ushort revision;

        public class BattleStep {
            [Name("State"), Description("What's going on during this section of the battle. Options are: normal, minigame"), MinVersion(4)]
            Symbol mState = new(0, "");
            [Name("Player Flags"), Description("Which players are involved with this section")]
            PlayerFlags mPlayers;
            [Name("Music Range"), Description("Music loop start and end")]
            Range mMusicRange;
            [Name("Play Range"), Description("Playable range of the section")]
            Range mPlayRange;
            [Name("Cam"), Description("Which camera cut to use for this section. Options are: '', Area1_NEAR, Area1_FAR, Area1_MOVEMENT, Area2_NEAR, Area2_FAR, Area2_MOVEMENT, DC_PLAYER_FREESTYLE"), MinVersion(2)]
            Symbol mCam = new(0, "");
            [Name("Non-play Action"), Description("What the non-dancer is doing. Options are: idle, dance, hide"), MinVersion(3)]
            Symbol mNonplayAction = new(0, "");

            public BattleStep Read(EndianReader reader, uint stepRev) {
                mPlayers = (PlayerFlags)reader.ReadUInt32();
                mMusicRange.Read(reader);
                mPlayRange.Read(reader);
                if(stepRev > 1) mCam = Symbol.Read(reader);
                if(stepRev > 2) mNonplayAction = Symbol.Read(reader);
                if(stepRev > 3) mState = Symbol.Read(reader);
                return this;
            }
        }

        private uint numSteps;
        // "Steps for the dance battle"
        public List<BattleStep> mBattleSteps = new();

        public HamBattleData Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry) {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            base.Read(reader, false, parent, entry);

            numSteps = reader.ReadUInt32();
            for(int i = 0; i < numSteps; i++) {
                mBattleSteps.Add(new BattleStep().Read(reader, revision));
            }

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw new Exception("Got to end of standalone asset but didn't find the expected end bytes, read likely did not succeed");

            return this;
        }
    }
}