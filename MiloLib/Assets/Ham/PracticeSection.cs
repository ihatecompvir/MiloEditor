using System.Diagnostics;
using System.Numerics;
using MiloLib.Assets.Rnd;
using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets.Ham {
    public class PracticeSection : Object {

        private ushort altRevision;
        private ushort revision;

        public RndAnimatable superAnimatable = new();

        [Name("Display Name"), Description("Display name used to show in selection screens")]
        public Symbol mDisplayName = new(0, "");

        [Name("Difficulty"), Description("Difficulty this section is tied to")]
        public HamDifficulty mDifficulty;

        public class PracticeStep {
            [Name("Step Type"), Description("Type of step. Options are: learn, review, freestyle")]
            Symbol mType = new(0, "");
            [Name("Start"), Description("Start of sequence")]
            Symbol mStart = new(0, "");
            [Name("End"), Description("End of sequence")]
            Symbol mEnd = new(0, "");
            // min version 1
            [Name("Boundary"), Description("True if this step is the START of a subsection"), MinVersion(1)]
            bool mBoundary;
            // min version 3
            [Name("Name Override"), Description("Name to display on the PracticeChoosePanel, if left blank it tries to automatically pick one"), MinVersion(1)]
            Symbol mNameOverride = new(0, ""); // String in milo's code

            public PracticeStep Read(EndianReader reader, uint stepRev) {
                mType = Symbol.Read(reader);
                mStart = Symbol.Read(reader);
                mEnd = Symbol.Read(reader);
                if (stepRev > 0) mBoundary = reader.ReadBoolean();
                if (stepRev > 2) mNameOverride = Symbol.Read(reader);
                return this;
            }
        };

        private uint numSteps;
        [Name("Practice Steps"), Description("List of steps for this practice section")]
        public List<PracticeStep> mSteps = new();

        private uint numSeqs;
        public List<DancerSequence> mSeqs = new();

        [Name("Test Step/Sequence Index"), Description("Index of step/sequence to test"), MinVersion(2)]
        public int mTestStepSequence;

        public PracticeSection Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry) {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            base.Read(reader, false, parent, entry);
            superAnimatable.Read(reader, parent, entry);

            mDisplayName = Symbol.Read(reader);
            mDifficulty = (HamDifficulty)reader.ReadUInt32();

            numSteps = reader.ReadUInt32();
            for(int i = 0; i < numSteps; i++) {
                mSteps.Add(new PracticeStep().Read(reader, revision));
            }

            if(revision > 1) {
                numSeqs = reader.ReadUInt32();
                for(int i = 0; i < numSeqs; i++) {
                    mSeqs.Add(new DancerSequence().Read(reader, false, parent, entry));
                }
                mTestStepSequence = reader.ReadInt32();
            }

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw new Exception("Got to end of standalone asset but didn't find the expected end bytes, read likely did not succeed");

            return this;
        }
    }
}