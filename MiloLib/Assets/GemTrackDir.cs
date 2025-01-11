using MiloLib.Assets.Band;
using MiloLib.Assets.Rnd;
using MiloLib.Classes;
using MiloLib.Utils;
using System.Diagnostics.Metrics;

namespace MiloLib.Assets
{
    [Name("GemTrackDir"), Description("band 2 TrackDir for gem tracks")]
    public class GemTrackDir : TrackDir
    {
        private ushort altRevision;
        private ushort revision;

        private ushort altTrackRevision;
        private ushort trackRevision;

        public int unkInt1;
        public Symbol effectsSelector = new(0, "");

        public Symbol unknownTex = new(0, "");
        public Symbol surfaceTexture = new(0, "");

        public Symbol surfaceMesh = new(0, "");
        public Symbol surfaceMat = new(0, "");
        public Symbol trackEnv = new(0, "");
        public Symbol gameCam = new(0, "");
        public Symbol bassSuperStreakOnTrig = new(0, "");
        public Symbol bassSuperStreakOffTrig = new(0, "");
        public Symbol kickDrummerTrig = new(0, "");
        public Symbol kickDrummerResetTrig = new(0, "");
        public Symbol spotlightPhraseSuccessTrig = new(0, "");
        public Symbol unkTrig = new(0, "");
        public Symbol unkTrig2 = new(0, "");
        public Symbol unkTrig3 = new(0, "");
        public Symbol peakStateOnTrig = new(0, "");
        public Symbol peakStateOffTrig = new(0, "");
        public Symbol drumFillResetTrig = new(0, "");
        public Symbol drumMash2ndPassActivateAnim = new(0, "");
        public Symbol drumMashHitAnimGrp = new(0, "");
        public Symbol fillColorsGrp = new(0, "");
        public Symbol lodAnim = new(0, "");
        public Symbol rotator = new(0, "");
        public Symbol unkAnim = new(0, "");
        private uint glowWidgetsCount;
        public List<Symbol> glowWidgets = new();

        private uint gemMashAnimsCount;
        public List<Symbol> gemMashAnims = new();

        private uint drumMashAnimsCount;
        public List<Symbol> drumMashAnims = new();

        private uint fillHitTrigsCount;
        public List<Symbol> fillHitTrigs = new();

        private uint realGuitarMashAnimCount;
        public List<Symbol> realGuitarMashAnims = new();

        private uint fillLaneAnimCount;
        public List<Symbol> fillLaneAnims = new();

        private uint fretPosOffsetCount;
        public List<float> fretPosOffsets = new();

        public float streakMeterOffset;
        public float streakMeterTilt;
        public float chordLabelPosOffset;

        private ushort bandTrackAltRevision;
        private ushort bandTrackRevision;

        public bool simulatedNet;
        public Symbol instrument = new(0, "");

        public Symbol starPowerMeter = new(0, "");
        public Symbol streakMeter = new(0, "");

        public Symbol playerIntro = new(0, "");
        public Symbol popupObject = new(0, "");
        public Symbol playerFeedback = new(0, "");
        public Symbol failedFeedback = new(0, "");
        public Symbol endgameFeedback = new(0, "");
        public Symbol retractTrig = new(0, "");
        public Symbol resetTrig = new(0, "");
        public Symbol deployTrig = new(0, "");
        public Symbol stopDeployTrig = new(0, "");
        public Symbol introTrig = new(0, "");


        public void LoadTrack(EndianReader reader, bool b1, bool b2, bool b3)
        {
            uint combinedTrackRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (trackRevision, altTrackRevision) = ((ushort)(combinedTrackRevision & 0xFFFF), (ushort)((combinedTrackRevision >> 16) & 0xFFFF));
            else (altTrackRevision, trackRevision) = ((ushort)(combinedTrackRevision & 0xFFFF), (ushort)((combinedTrackRevision >> 16) & 0xFFFF));

            simulatedNet = reader.ReadBoolean();
            instrument = Symbol.Read(reader);

            if (trackRevision >= 1 && !b1)
            {
                starPowerMeter = Symbol.Read(reader);
                streakMeter = Symbol.Read(reader);
            }
            bool finalbool;
            if (trackRevision < 3)
            {
                finalbool = false;
                if (!b3 || !b1)
                    finalbool = true;
            }
            else
            {
                finalbool = !b1;
            }
            if (finalbool)
            {
                playerIntro = Symbol.Read(reader);
                if (trackRevision < 1)
                {
                    starPowerMeter = Symbol.Read(reader);
                    streakMeter = Symbol.Read(reader);
                }
                popupObject = Symbol.Read(reader);
                playerFeedback = Symbol.Read(reader);
                failedFeedback = Symbol.Read(reader);
                if (trackRevision >= 2)
                    endgameFeedback = Symbol.Read(reader);
            }
            if (!b1)
            {
                retractTrig = Symbol.Read(reader);
                resetTrig = Symbol.Read(reader);
                deployTrig = Symbol.Read(reader);
                stopDeployTrig = Symbol.Read(reader);
                introTrig = Symbol.Read(reader);
            }
            return;
        }

        public void SaveTrack(EndianWriter writer, bool b1, bool b2, bool b3)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altTrackRevision << 16) | trackRevision) : (uint)((trackRevision << 16) | altTrackRevision));

            writer.WriteBoolean(simulatedNet);
            Symbol.Write(writer, instrument);

            if (trackRevision >= 1 && !b1)
            {
                Symbol.Write(writer, starPowerMeter);
                Symbol.Write(writer, streakMeter);
            }
            bool finalbool;
            if (trackRevision < 3)
            {
                finalbool = false;
                if (!b3 || !b1)
                    finalbool = true;
            }
            else
            {
                finalbool = !b1;
            }
            if (finalbool)
            {
                Symbol.Write(writer, playerIntro);
                if (trackRevision < 1)
                {
                    Symbol.Write(writer, starPowerMeter);
                    Symbol.Write(writer, streakMeter);
                }
                Symbol.Write(writer, popupObject);
                Symbol.Write(writer, playerFeedback);
                Symbol.Write(writer, failedFeedback);
                if (trackRevision >= 2)
                    Symbol.Write(writer, endgameFeedback);
            }
            if (!b1)
            {
                Symbol.Write(writer, retractTrig);
                Symbol.Write(writer, resetTrig);
                Symbol.Write(writer, deployTrig);
                Symbol.Write(writer, stopDeployTrig);
                Symbol.Write(writer, introTrig);
            }
        }

        public GemTrackDir(ushort revision, ushort altRevision = 0) : base(revision, altRevision)
        {
            revision = revision;
            altRevision = altRevision;
            return;
        }

        public GemTrackDir Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            if (revision < 9)
            {
                unkInt1 = reader.ReadInt32();
                effectsSelector = Symbol.Read(reader);
                if (revision < 1)
                {
                    surfaceTexture = Symbol.Read(reader);
                }
            }
            if (!entry.isProxy)
            {
                if (revision >= 9)
                    effectsSelector = Symbol.Read(reader);
                surfaceMesh = Symbol.Read(reader);
                surfaceMat = Symbol.Read(reader);
                trackEnv = Symbol.Read(reader);
                gameCam = Symbol.Read(reader);
                bassSuperStreakOnTrig = Symbol.Read(reader);
                bassSuperStreakOffTrig = Symbol.Read(reader);
                kickDrummerTrig = Symbol.Read(reader);
                spotlightPhraseSuccessTrig = Symbol.Read(reader);
                if (revision < 0xC)
                    unkTrig = Symbol.Read(reader);
                drumFillResetTrig = Symbol.Read(reader);
                drumMash2ndPassActivateAnim = Symbol.Read(reader);
                drumMashHitAnimGrp = Symbol.Read(reader);
                fillColorsGrp = Symbol.Read(reader);
                lodAnim = Symbol.Read(reader);
                rotator = Symbol.Read(reader);
                glowWidgetsCount = reader.ReadUInt32();
                for (int i = 0; i < glowWidgetsCount; i++)
                {
                    glowWidgets.Add(Symbol.Read(reader));
                }

                for (int i = 0; i < 5; i++)
                {
                    gemMashAnims.Add(Symbol.Read(reader));
                }

                if (revision >= 6 && revision <= 10)
                {
                    unkAnim = Symbol.Read(reader);
                }

                for (int i = 1; i < 5; i++)
                {
                    drumMashAnims.Add(Symbol.Read(reader));
                }

                for (int i = 0; i < 3; i++)
                {
                    fillHitTrigs.Add(Symbol.Read(reader));
                }

                if (revision >= 11)
                {
                    for (int i = 0; i < 6; i++)
                    {
                        realGuitarMashAnims.Add(Symbol.Read(reader));
                    }
                }

                if (revision >= 2)
                    streakMeterOffset = reader.ReadFloat();
                if (revision >= 3)
                    streakMeterTilt = reader.ReadFloat();

                if (revision >= 4)
                {
                    fretPosOffsetCount = reader.ReadUInt32();
                    for (int i = 0; i < fretPosOffsetCount; i++)
                    {
                        fretPosOffsets.Add(reader.ReadFloat());
                    }
                }
                if (revision >= 5)
                    kickDrummerResetTrig = Symbol.Read(reader);
                if (revision >= 7)
                    chordLabelPosOffset = reader.ReadFloat();
                if (revision >= 8)
                {
                    if (revision < 10)
                    {
                        unkTrig2 = Symbol.Read(reader);
                        unkTrig3 = Symbol.Read(reader);
                    }
                    peakStateOnTrig = Symbol.Read(reader);
                    peakStateOffTrig = Symbol.Read(reader);
                }

                if (revision >= 12)
                {
                    for (int i = 1; i < 5; i++)
                    {
                        fillLaneAnims.Add(Symbol.Read(reader));
                    }
                }


            }

            LoadTrack(reader, entry.isProxy, false, false);
            base.Read(reader, false, parent, entry);

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw new Exception("Got to end of standalone asset but didn't find the expected end bytes, read likely did not succeed");

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            if (revision < 9)
            {
                writer.WriteInt32(unkInt1);
                Symbol.Write(writer, effectsSelector);
                if (revision < 1)
                {
                    Symbol.Write(writer, surfaceTexture);
                }
            }

            if (!entry.isProxy)
            {
                if (revision >= 9)
                {
                    Symbol.Write(writer, effectsSelector);
                }
                Symbol.Write(writer, surfaceMesh);
                Symbol.Write(writer, surfaceMat);
                Symbol.Write(writer, trackEnv);
                Symbol.Write(writer, gameCam);
                Symbol.Write(writer, bassSuperStreakOnTrig);
                Symbol.Write(writer, bassSuperStreakOffTrig);
                Symbol.Write(writer, kickDrummerTrig);
                Symbol.Write(writer, spotlightPhraseSuccessTrig);
                if (revision < 0xC)
                {
                    Symbol.Write(writer, unkTrig);
                }
                Symbol.Write(writer, drumFillResetTrig);
                Symbol.Write(writer, drumMash2ndPassActivateAnim);
                Symbol.Write(writer, drumMashHitAnimGrp);
                Symbol.Write(writer, fillColorsGrp);
                Symbol.Write(writer, lodAnim);
                Symbol.Write(writer, rotator);

                writer.WriteUInt32((uint)glowWidgets.Count);
                foreach (var glowWidget in glowWidgets)
                {
                    Symbol.Write(writer, glowWidget);
                }

                foreach (var gemMashAnim in gemMashAnims)
                {
                    Symbol.Write(writer, gemMashAnim);
                }

                if (revision >= 6 && revision <= 10)
                {
                    Symbol.Write(writer, unkAnim);
                }

                foreach (var drumMashAnim in drumMashAnims)
                {
                    Symbol.Write(writer, drumMashAnim);
                }

                foreach (var fillHitTrig in fillHitTrigs)
                {
                    Symbol.Write(writer, fillHitTrig);
                }

                if (revision >= 11)
                {
                    foreach (var realGuitarMashAnim in realGuitarMashAnims)
                    {
                        Symbol.Write(writer, realGuitarMashAnim);
                    }
                }

                if (revision >= 2)
                {
                    writer.WriteFloat(streakMeterOffset);
                }
                if (revision >= 3)
                {
                    writer.WriteFloat(streakMeterTilt);
                }

                if (revision >= 4)
                {
                    writer.WriteUInt32((uint)fretPosOffsets.Count);
                    foreach (var fretPosOffset in fretPosOffsets)
                    {
                        writer.WriteFloat(fretPosOffset);
                    }
                }

                if (revision >= 5)
                {
                    Symbol.Write(writer, kickDrummerResetTrig);
                }
                if (revision >= 7)
                {
                    writer.WriteFloat(chordLabelPosOffset);
                }
                if (revision >= 8)
                {
                    if (revision < 10)
                    {
                        Symbol.Write(writer, unkTrig2);
                        Symbol.Write(writer, unkTrig3);
                    }
                    Symbol.Write(writer, peakStateOnTrig);
                    Symbol.Write(writer, peakStateOffTrig);
                }

                if (revision >= 12)
                {
                    foreach (var fillLaneAnim in fillLaneAnims)
                    {
                        Symbol.Write(writer, fillLaneAnim);
                    }
                }
            }

            SaveTrack(writer, entry.isProxy, false, false);
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
