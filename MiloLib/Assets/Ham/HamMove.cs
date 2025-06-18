using System.Diagnostics;
using System.Numerics;
using System.Reflection.PortableExecutable;
using MiloLib.Assets.Rnd;
using MiloLib.Classes;
using MiloLib.Utils;
using static MiloLib.Assets.Rnd.RndPropAnim.PropKey;

namespace MiloLib.Assets.Ham
{
    public class HamMove : RndPropAnim
    {
        private Dictionary<Game.MiloGame, uint> gameRevisions = new Dictionary<Game.MiloGame, uint>
        {
            { Game.MiloGame.DanceCentral, 28 },
            { Game.MiloGame.DanceCentral3, 50 }
        };
        public class LocalizedName
        {
            public Symbol locale = new(0, "");
            public Symbol name = new(0, "");

            public LocalizedName Read(EndianReader reader)
            {
                locale = Symbol.Read(reader);
                name = Symbol.Read(reader);
                return this;
            }

            public void Write(EndianWriter writer)
            {
                Symbol.Write(writer, locale);
                Symbol.Write(writer, name);
            }

            public override string ToString()
            {
                return $"{locale} - {name}";
            }
        }

        private ushort altRevision;
        private ushort revision;

        [Name("Mirror"), Description("Move to mirror"), MinVersion(8)]
        public Symbol mirror = new(0, "");

        [Name("Texture"), Description("Texture to describe the move")]
        public Symbol tex = new(0, "");

        [Name("Scored"), Description("True if this is move is scored. False if it's a rest or some kind of indicator (like freestyle)"), MinVersion(2)]
        public bool mScored;
        [Name("Final Pose"), Description("True if this move is the final pose in the song"), MinVersion(20)]
        public bool mFinalPose;

        private uint languageCount;
        [Name("Localized Names"), Description("This move's name for each language"), MinVersion(5)]
        public List<LocalizedName> languages = new();

        public enum TexState
        {
            kTexNone,
            kTexNormal,
            kTexFlip,
            kTexDblFlip
        }
        [Name("Texture State"), Description("Texture state describes how to display the tex"), MinVersion(12)]
        public TexState mTexState;

        private uint numMoveFrames;
        public List<MoveFrame> frames = new();

        [Name("Paradiddle"), Description("True if this move is a paradiddle"), MinVersion(18)]
        public bool mParadiddle;
        [Name("Supress Guide"), Description("Prevent the Guide Gesture from appearing for the duration of this move"), MinVersion(21)]
        public bool mSuppressGuide;
        [Name("Supress Practice Options"), Description("Prevent the Practice Options from appearing for the duration of this move"), MinVersion(50)]
        public bool mSuppressPracticeOptions;
        [Name("Omit from Minigame"), Description("Prevent this move from appear in the dance battle minigame"), MinVersion(34)]
        public bool mOmitMinigame;

        private uint numRatingStates;
        public List<float> mRatingStates = new();

        [Name("Shoulder Displacements"), Description("Whether to use shoulder displacements for detection - specific to Ham1!"), MinVersion(27)]
        public bool mShoulderDisplacements;

        [Name("Thresholds"), Description("Generated threshold for super perfect / perfect/flawless / awesome/nice / ok/almost"), MinVersion(36)]
        public float[] mThresholds = new float[4]; // enum MoveRating
        [Name("Thresholds"), Description("Override threshold for super perfect / perfect/flawless / awesome/nice / ok/almost (0 means no override)"), MinVersion(45)]
        public float[] mOverrides = new float[4]; // enum MoveRating

        private uint numConfusabilities;
        public Dictionary<uint, float> mConfusabilities = new();

        public int unk94;

        public Symbol mDancerSeq = new(0, "");

        [Name("Confusability ID"), Description("id used when comparing to other moves"), MinVersion(49)]
        public uint mConfusabilityID;

        private string PrintMoveKeys()
        {
            string str = "RndPropAnim: ";
            str += $"revs ({revision}, {altRevision}) ";
            str += anim.ToString();
            for (int i = 0; i < propKeys.Count; i++)
            {
                str += propKeys[i].dtb.children[0].value.ToString() + $" keys ({propKeys[i].keys.Count}): \n";
                for (int j = 0; j < propKeys[i].keys.Count; j++)
                {
                    str += "\t" + propKeys[i].keys[j].ToString() + "\n";
                }
            }
            str += $"Loop: {mLoop}\n";
            str += $"Intensity: {mIntensity}\n";
            return str;
        }

        public override string ToString()
        {
            string str = "HamMove: ";
            str += $"revs ({revision}, {altRevision}) ";
            // condensed dump of RndPropAnim specifically for HamMove
            str += PrintMoveKeys();

            if (revision > 7) str += $"Move to mirror: {mirror}\n";
            str += $"Texture: {tex}\n";
            if (revision > 1) str += $"Scored: {mScored}\n";
            if (revision > 19) str += $"Final pose: {mFinalPose}\n";
            if (revision > 4)
            {
                str += $"Localized names ({languageCount}): \n";
                for (int i = 0; i < languageCount; i++)
                {
                    str += $"\t {languages[i]}\n";
                }
            }
            if (revision > 11) str += $"Tex state: {mTexState}\n";
            if (revision > 12)
            {
                str += $"MoveFrames ({numMoveFrames}):\n";
                for (int i = 0; i < numMoveFrames; i++)
                {
                    str += frames[i];
                }
            }

            if (revision > 17) str += $"Paradiddle: {mParadiddle}\n";
            if (revision > 20) str += $"Suppress Guide: {mSuppressGuide}\n";
            if (revision > 49) str += $"Suppress Practice Options: {mSuppressPracticeOptions}\n";
            if (revision > 33) str += $"Omit from Minigame: {mOmitMinigame}\n";

            if (revision > 21)
            {
                str += $"Rating states ({numRatingStates}): ";
                for (int i = 0; i < numRatingStates; i++)
                {
                    str += $" {mRatingStates[i]} ";
                }
                str += "\n";
            }

            if (revision > 26) str += $"Use shoulder displacements (DC1 only): {mShoulderDisplacements}\n";

            if (revision > 35)
            {
                str += $"Thresholds: \n";
                str += $"\tSuper perfect: {mThresholds[0]}\n";
                str += $"\tPerfect/flawless: {mThresholds[1]}\n";
                str += $"\tAwesome/nice: {mThresholds[2]}\n";
                str += $"\tOk/almost: {mThresholds[3]}\n";
            }

            if (revision > 44)
            {
                str += $"Overrides: \n";
                str += $"\tSuper perfect: {mOverrides[0]}\n";
                str += $"\tPerfect/flawless: {mOverrides[1]}\n";
                str += $"\tAwesome/nice: {mOverrides[2]}\n";
                str += $"\tOk/almost: {mOverrides[3]}\n";
            }

            if (revision > 42)
            {
                str += $"Confusabilities ({mConfusabilities.Count}): \n\t";
                foreach (var item in mConfusabilities)
                {
                    str += $"({item.Key}, {item.Value}) ";
                }
                str += "\n";
            }

            if (revision > 46) str += $"Unknown value at 0x94: {unk94}\n";
            if (revision > 47) str += $"Dancer sequence: {mDancerSeq}\n";
            if (revision > 48) str += $"Confusability ID: {mConfusabilityID}\n";

            return str;
        }

        public HamMove Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            // TODO: if revision < 4, should read superclass Object instead of RndPropAnim

            base.Read(reader, false, parent, entry);

            if (revision > 7) mirror = Symbol.Read(reader);

            // read empty string
            if (revision < 5) Symbol.Read(reader);

            tex = Symbol.Read(reader);

            if (revision > 1)
                mScored = reader.ReadBoolean();
            if (revision > 19)
                mFinalPose = reader.ReadBoolean();

            // if(revision > 2 && revision < 11), do a bunch of unused, deprecated stuff
            // if(revision < 16), do more unused, deprecated stuff

            if (revision > 4)
            {
                languageCount = reader.ReadUInt32();
                for (int i = 0; i < languageCount; i++)
                {
                    languages.Add(new LocalizedName().Read(reader));
                }
            }
            // if(revision < 4) MILO_WARN("Can't load version older than 4");
            // if(revision < 39) a bunch of unused stuff

            if (revision > 11) mTexState = (TexState)reader.ReadUInt32();

            if (revision > 12)
            {
                numMoveFrames = reader.ReadUInt32();
                for (int i = 0; i < numMoveFrames; i++)
                {
                    frames.Add(new MoveFrame().Read(reader));
                }
            }

            // if(revision > 14 && revision < 42) unused
            if (revision > 17) mParadiddle = reader.ReadBoolean();
            if (revision > 20) mSuppressGuide = reader.ReadBoolean();
            if (revision > 49) mSuppressPracticeOptions = reader.ReadBoolean();
            if (revision > 33) mOmitMinigame = reader.ReadBoolean();

            if (revision > 21)
            {
                numRatingStates = reader.ReadUInt32();
                for (int i = 0; i < numRatingStates; i++)
                {
                    mRatingStates.Add(reader.ReadFloat());
                }
            }

            // if gRev < 26, unused

            if (revision > 26) mShoulderDisplacements = reader.ReadBoolean();

            if (revision > 35)
            {
                for (int i = 0; i < 4; i++)
                {
                    mThresholds[i] = reader.ReadFloat();
                    if (revision > 44) mOverrides[i] = reader.ReadFloat();
                }
            }

            if (revision > 42)
            {
                numConfusabilities = reader.ReadUInt32();
                for (int i = 0; i < numConfusabilities; i++)
                {
                    mConfusabilities.Add(reader.ReadUInt32(), reader.ReadFloat());
                }
            }

            if (revision > 46)
            {
                unk94 = reader.ReadInt32();
            }

            if (revision > 47)
            {
                mDancerSeq = Symbol.Read(reader);
            }

            if (revision > 48)
            {
                mConfusabilityID = reader.ReadUInt32();
            }

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw new Exception("Got to end of standalone asset but didn't find the expected end bytes, read likely did not succeed");

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            base.Write(writer, false, parent, entry);

            if (revision > 7) Symbol.Write(writer, mirror);

            if (revision < 5) Symbol.Write(writer, new Symbol(0, "")); // Write empty string

            Symbol.Write(writer, tex);

            if (revision > 1)
                writer.WriteBoolean(mScored);
            if (revision > 19)
                writer.WriteBoolean(mFinalPose);

            if (revision > 4)
            {
                writer.WriteUInt32(languageCount);
                foreach (var lang in languages)
                {
                    lang.Write(writer);
                }
            }

            if (revision > 11) writer.WriteUInt32((uint)mTexState);

            if (revision > 12)
            {
                writer.WriteUInt32(numMoveFrames);
                foreach (var frame in frames)
                {
                    frame.Write(writer);
                }
            }

            if (revision > 17) writer.WriteBoolean(mParadiddle);
            if (revision > 20) writer.WriteBoolean(mSuppressGuide);
            if (revision > 49) writer.WriteBoolean(mSuppressPracticeOptions);
            if (revision > 33) writer.WriteBoolean(mOmitMinigame);

            if (revision > 21)
            {
                writer.WriteUInt32(numRatingStates);
                foreach (var state in mRatingStates)
                {
                    writer.WriteFloat(state);
                }
            }

            if (revision > 26) writer.WriteBoolean(mShoulderDisplacements);

            if (revision > 35)
            {
                for (int i = 0; i < 4; i++)
                {
                    writer.WriteFloat(mThresholds[i]);
                    if (revision > 44) writer.WriteFloat(mOverrides[i]);
                }
            }

            if (revision > 42)
            {
                writer.WriteUInt32(numConfusabilities);
                foreach (var confusability in mConfusabilities)
                {
                    writer.WriteUInt32(confusability.Key);
                    writer.WriteFloat(confusability.Value);
                }
            }

            if (revision > 46)
            {
                writer.WriteInt32(unk94);
            }

            if (revision > 47)
            {
                Symbol.Write(writer, mDancerSeq);
            }

            if (revision > 48)
            {
                writer.WriteUInt32(mConfusabilityID);
            }

            if (standalone)
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
        }
    }
}