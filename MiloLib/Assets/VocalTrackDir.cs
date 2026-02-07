using MiloLib.Assets.Band;
using MiloLib.Assets.Rnd;
using MiloLib.Classes;
using MiloLib.Utils;
using System.Drawing;

namespace MiloLib.Assets
{
    [Name("VocalTrackDir"), Description("band 2 dir for vocal track")]
    public class VocalTrackDir : RndDir
    {
        public class LyricColor
        {
            public uint lyric;
            public HmxColor3 color = new HmxColor3();
            public uint unk;

            public LyricColor Read(EndianReader reader)
            {
                lyric = reader.ReadUInt32();
                color = new HmxColor3().Read(reader);
                unk = reader.ReadUInt32();
                return this;
            }

            public void Write(EndianWriter writer)
            {
                writer.WriteUInt32(lyric);
                color.Write(writer);
                writer.WriteUInt32(unk);
            }
        }

        public class LyricAlphaMap
        {
            public uint lyric;
            public float alpha;

            public LyricAlphaMap Read(EndianReader reader)
            {
                lyric = reader.ReadUInt32();
                alpha = reader.ReadFloat();
                return this;
            }

            public void Write(EndianWriter writer)
            {
                writer.WriteUInt32(lyric);
                writer.WriteFloat(alpha);
            }
        }

        private ushort altRevision;
        private ushort revision;

        private ushort altTrackRevision;
        private ushort trackRevision;

        public bool unkBool1;
        public bool unkBool2;
        public bool unkBool3;
        public bool unkBool4;
        private uint lyricColorsCount;
        public List<LyricColor> lyricColors = new(32);
        public List<HmxColor4> lyricColorsOld = new(14);
        private uint lyricAlphaMapCount;
        public List<LyricAlphaMap> lyricAlphaMaps = new(32);
        public Symbol vocalMics = new(0, "");
        public Symbol unkSymbol1 = new(0, "");
        public float minPitchRange;
        public float arrowSmoothing;
        private uint configurableObjectsCount;
        public List<Symbol> configurableObjects = new();
        public Symbol voxCfg = new(0, "");
        public Symbol tambourineSmasher = new(0, "");
        public Symbol tambourineNowShowTrig = new(0, "");
        public Symbol tambourineNowHideTrig = new(0, "");
        public Symbol phraseFeedbackTrig = new(0, "");
        public Symbol spotlightSparklesOnlyTrig = new(0, "");
        public Symbol spotlightPhraseSuccessTrig = new(0, "");
        public Symbol pitchArrow1 = new(0, "");
        public Symbol pitchArrow2 = new(0, "");
        public Symbol pitchArrow3 = new(0, "");
        public bool pitchWindow;
        public float pitchWindowHeight;
        public Symbol pitchWindowMesh = new(0, "");
        public Symbol pitchWindowOverlay = new(0, "");
        public bool leadLyrics;
        public float leadLyricHeight;
        public Symbol leadLyricMesh = new(0, "");
        public bool harmLyrics;
        public float harmLyricHeight;
        public Symbol harmLyricMesh = new(0, "");
        public Symbol leftDecoMesh = new(0, "");
        public Symbol rightDecoMesh = new(0, "");
        public float nowBarWidth;
        public Symbol nowBarMesh = new(0, "");
        public bool remoteVocals;
        public float unkFloat1;
        public float trackLeftX;
        public float trackRightX;
        public float trackBottomZ;
        public float trackTopZ;
        public float pitchBottomZ;
        public float pitchTopZ;
        public float nowBarX;
        public Symbol pitchGuides = new(0, "");
        public Symbol tubeStyle = new(0, "");
        public Symbol arrowStyle = new(0, "");
        public Symbol fontStyle = new(0, "");
        public Symbol leadText = new(0, "");
        public Symbol harmText = new(0, "");
        public Symbol leadPhonemeText = new(0, "");
        public Symbol harmPhonemeText = new(0, "");
        public Symbol rangeScaleAnim = new(0, "");
        public Symbol rangeOffsetAnim = new(0, "");
        public Symbol tubeRangeGrp = new(0, "");
        public Symbol tubeSpotlightGrp = new(0, "");
        public Symbol tubeBack0Grp = new(0, "");
        public Symbol tubeBack1Grp = new(0, "");
        public Symbol tubeBack2Grp = new(0, "");
        public Symbol tubeFront0Grp = new(0, "");
        public Symbol tubeFront1Grp = new(0, "");
        public Symbol tubeFront2Grp = new(0, "");
        public Symbol tubeGlow0Grp = new(0, "");
        public Symbol tubeGlow1Grp = new(0, "");
        public Symbol tubeGlow2Grp = new(0, "");
        public Symbol tubePhoneme0Grp = new(0, "");
        public Symbol tubePhoneme1Grp = new(0, "");
        public Symbol tubePhoneme2Grp = new(0, "");
        public Symbol spotlightMat = new(0, "");
        public Symbol leadBackMat = new(0, "");
        public Symbol harm1BackMat = new(0, "");
        public Symbol harm2BackMat = new(0, "");
        public Symbol leadFrontMat = new(0, "");
        public Symbol harm1FrontMat = new(0, "");
        public Symbol harm2FrontMat = new(0, "");
        public Symbol leadGlowMat = new(0, "");
        public Symbol harm1GlowMat = new(0, "");
        public Symbol harm2GlowMat = new(0, "");
        public Symbol leadPhonemeMat = new(0, "");
        public Symbol harm1PhonemeMat = new(0, "");
        public Symbol harm2PhonemeMat = new(0, "");
        public Symbol vocalsGrp = new(0, "");
        public Symbol breGrp = new(0, "");
        public Symbol leadBreGrp = new(0, "");
        public Symbol harmonyBreGrp = new(0, "");
        public Symbol pitchScrollGroup = new(0, "");
        public Symbol leadLyricScrollGroup = new(0, "");
        public Symbol harmonyLyricScrollGroup = new(0, "");
        public float unkFloat2;
        public float unkFloat3;
        public float unkFloat4;
        public float unkFloat5;
        public Symbol leadDeployMat = new(0, "");
        public Symbol harmDeployMat = new(0, "");
        public Symbol arrowFxDrawGrp = new(0, "");
        public float unkFloat6;
        public float unkFloat7;
        public bool unkBool6;

        public float lastMin;
        public float lastMax;
        public float middleCZPos;
        public int tonic;

        public uint unkInt1;
        public uint unkInt2;
        public float unkFloat8;
        public uint unkInt3;

        public Symbol unkSymbol2 = new(0, "");
        public Symbol unkSymbol3 = new(0, "");
        public Symbol unkSymbol4 = new(0, "");
        public Symbol unkSymbol5 = new(0, "");

        public Symbol unkSymbol6 = new(0, "");
        public Symbol unkSymbol7 = new(0, "");
        public Symbol unkSymbol8 = new(0, "");
        public Symbol unkSymbol9 = new(0, "");

        public HmxColor4 unkColor = new HmxColor4();
        public HmxColor4 unkColor2 = new HmxColor4();

        public float unkFloat9;

        public float unkFloat10;

        public float unkFloat11;

        public float unkFloat12;
        public float unkFloat13;

        public float unkFloat14;

        public float unkFloat15;
        public float unkFloat16;

        public Symbol unkSymbol10 = new(0, "");
        public Symbol unkSymbol11 = new(0, "");
        public Symbol unkSymbol12 = new(0, "");
        public Symbol unkSymbol13 = new(0, "");

        public Symbol unkSymbol14 = new(0, "");
        public Symbol unkSymbol15 = new(0, "");
        public Symbol unkSymbol16 = new(0, "");
        public Symbol unkSymbol17 = new(0, "");

        private uint colorsCount;
        public List<HmxColor4> colors = new();

        private uint floatsCount;
        public List<float> floats = new();

        public Symbol unkSymbol18 = new(0, "");
        public Symbol unkSymbol19 = new(0, "");
        public Symbol unkSymbol20 = new(0, "");
        public Symbol unkSymbol21 = new(0, "");

        public uint unkInt4;
        public bool unkBool7;

        public Symbol unkSymbol22 = new(0, "");
        public Symbol unkSymbol23 = new(0, "");
        public Symbol unkSymbol24 = new(0, "");
        public Symbol unkSymbol25 = new(0, "");

        public Symbol unkSymbol26 = new(0, "");
        public Symbol unkSymbol27 = new(0, "");
        public Symbol unkSymbol28 = new(0, "");
        public Symbol unkSymbol29 = new(0, "");

        public Symbol unkSymbol30 = new(0, "");
        public Symbol unkSymbol31 = new(0, "");
        public Symbol unkSymbol32 = new(0, "");
        public Symbol unkSymbol33 = new(0, "");


        public Symbol streakMeter1 = new(0, "");
        public Symbol streakMeter2 = new(0, "");

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

        public VocalTrackDir(ushort revision, ushort altRevision = 0) : base(revision, altRevision)
        {
            this.revision = revision;
            this.altRevision = altRevision;
        }

        public void LoadTrack(EndianReader reader, bool b1, bool b2, bool b3)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (trackRevision, altTrackRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altTrackRevision, trackRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

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

        public VocalTrackDir Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            base.Read(reader, false, parent, entry);

            if (!entry.isProxy)
            {
                if (revision < 2)
                {
                    unkInt1 = reader.ReadUInt32();
                    unkInt2 = reader.ReadUInt32();
                    unkFloat8 = reader.ReadFloat();
                    unkInt3 = reader.ReadUInt32();

                    configurableObjectsCount = reader.ReadUInt32();
                    configurableObjects = new List<Symbol>();
                    for (uint i = 0; i < configurableObjectsCount; i++)
                    {
                        configurableObjects.Add(Symbol.Read(reader));
                    }


                    for (int i = 0; i < 14; i++)
                    {
                        lyricColorsOld.Add(new HmxColor4().Read(reader));
                    }

                    unkSymbol2 = Symbol.Read(reader);
                    unkSymbol3 = Symbol.Read(reader);
                    unkSymbol4 = Symbol.Read(reader);
                    unkSymbol5 = Symbol.Read(reader);

                    unkSymbol6 = Symbol.Read(reader);
                    unkSymbol7 = Symbol.Read(reader);
                    unkSymbol8 = Symbol.Read(reader);
                    unkSymbol9 = Symbol.Read(reader);


                    unkColor = new HmxColor4().Read(reader);
                    unkColor2 = new HmxColor4().Read(reader);

                    unkFloat9 = reader.ReadFloat();
                    unkFloat10 = reader.ReadFloat();

                    if (revision >= 1)
                    {
                        unkFloat11 = reader.ReadFloat();
                        unkFloat12 = reader.ReadFloat();
                    }

                    unkFloat13 = reader.ReadFloat();

                    unkSymbol1 = Symbol.Read(reader);

                    unkBool1 = reader.ReadBoolean();

                    minPitchRange = reader.ReadFloat();
                    arrowSmoothing = reader.ReadFloat();

                    unkFloat14 = reader.ReadFloat();
                    unkFloat15 = reader.ReadFloat();

                    colorsCount = reader.ReadUInt32();
                    for (int i = 0; i < colorsCount; i++)
                    {
                        colors.Add(new HmxColor4().Read(reader));
                    }

                    floatsCount = reader.ReadUInt32();
                    for (int i = 0; i < floatsCount; i++)
                    {
                        floats.Add(reader.ReadFloat());
                    }

                    unkSymbol10 = Symbol.Read(reader);
                    unkSymbol11 = Symbol.Read(reader);
                    unkSymbol12 = Symbol.Read(reader);
                    unkSymbol13 = Symbol.Read(reader);

                    unkSymbol14 = Symbol.Read(reader);
                    unkSymbol15 = Symbol.Read(reader);
                    unkSymbol16 = Symbol.Read(reader);
                    unkSymbol17 = Symbol.Read(reader);


                    phraseFeedbackTrig = Symbol.Read(reader);
                    spotlightSparklesOnlyTrig = Symbol.Read(reader);
                    spotlightPhraseSuccessTrig = Symbol.Read(reader);

                    unkSymbol18 = Symbol.Read(reader);
                    unkSymbol19 = Symbol.Read(reader);
                    unkSymbol20 = Symbol.Read(reader);
                    unkSymbol21 = Symbol.Read(reader);

                    unkInt4 = reader.ReadUInt32();
                    unkBool7 = reader.ReadBoolean();

                    unkSymbol22 = Symbol.Read(reader);
                    unkSymbol23 = Symbol.Read(reader);
                    unkSymbol24 = Symbol.Read(reader);
                    unkSymbol25 = Symbol.Read(reader);

                    unkSymbol26 = Symbol.Read(reader);
                    unkSymbol27 = Symbol.Read(reader);
                    unkSymbol28 = Symbol.Read(reader);
                    unkSymbol29 = Symbol.Read(reader);


                    unkSymbol30 = Symbol.Read(reader);
                    unkSymbol31 = Symbol.Read(reader);
                    unkSymbol32 = Symbol.Read(reader);
                    unkSymbol33 = Symbol.Read(reader);
                }
                else
                {
                    configurableObjectsCount = reader.ReadUInt32();
                    configurableObjects = new List<Symbol>();
                    for (uint i = 0; i < configurableObjectsCount; i++)
                    {
                        configurableObjects.Add(Symbol.Read(reader));
                    }

                    voxCfg = Symbol.Read(reader);
                    unkSymbol1 = Symbol.Read(reader);
                    minPitchRange = reader.ReadFloat();
                    arrowSmoothing = reader.ReadFloat();

                    if (revision < 3)
                        unkFloat1 = reader.ReadFloat();
                    if (revision < 7)
                        unkFloat2 = reader.ReadFloat();

                    if (revision < 3)
                    {
                        colorsCount = reader.ReadUInt32();
                        for (int i = 0; i < colorsCount; i++)
                        {
                            colors.Add(new HmxColor4().Read(reader));
                        }

                        unkSymbol2 = Symbol.Read(reader);
                        unkSymbol3 = Symbol.Read(reader);
                        unkSymbol4 = Symbol.Read(reader);
                        unkSymbol5 = Symbol.Read(reader);
                    }
                    else if (revision >= 6)
                    {
                        tambourineSmasher = Symbol.Read(reader);
                        tambourineNowShowTrig = Symbol.Read(reader);
                        tambourineNowHideTrig = Symbol.Read(reader);
                    }

                    phraseFeedbackTrig = Symbol.Read(reader);
                    spotlightSparklesOnlyTrig = Symbol.Read(reader);
                    spotlightPhraseSuccessTrig = Symbol.Read(reader);

                    lyricColorsCount = reader.ReadUInt32();
                    for (int i = 0; i < lyricColorsCount; i++)
                    {
                        LyricColor lyricColor = new LyricColor().Read(reader);
                        lyricColors.Add(lyricColor);
                    }

                    lyricAlphaMapCount = reader.ReadUInt32();
                    for (int i = 0; i < lyricAlphaMapCount; i++)
                    {
                        LyricAlphaMap lyricAlphaMap = new LyricAlphaMap().Read(reader);
                        lyricAlphaMaps.Add(lyricAlphaMap);
                    }

                    if (revision < 5)
                    {
                        streakMeter1 = Symbol.Read(reader);
                        streakMeter2 = Symbol.Read(reader);
                    }



                    pitchWindow = reader.ReadBoolean();
                    pitchWindowHeight = reader.ReadFloat();
                    pitchWindowMesh = Symbol.Read(reader);
                    pitchWindowOverlay = Symbol.Read(reader);
                    leadLyrics = reader.ReadBoolean();
                    leadLyricHeight = reader.ReadFloat();
                    leadLyricMesh = Symbol.Read(reader);
                    harmLyrics = reader.ReadBoolean();
                    harmLyricHeight = reader.ReadFloat();
                    harmLyricMesh = Symbol.Read(reader);

                    if (revision < 3)
                    {
                        unkSymbol6 = Symbol.Read(reader);
                        unkSymbol7 = Symbol.Read(reader);
                        unkSymbol8 = Symbol.Read(reader);
                        unkSymbol9 = Symbol.Read(reader);

                        unkFloat3 = reader.ReadFloat();
                        unkBool1 = reader.ReadBoolean();
                    }

                    leftDecoMesh = Symbol.Read(reader);
                    rightDecoMesh = Symbol.Read(reader);
                    nowBarWidth = reader.ReadFloat();
                    if (revision < 3)
                        unkBool2 = reader.ReadBoolean();
                    nowBarMesh = Symbol.Read(reader);

                    remoteVocals = reader.ReadBoolean();
                    trackLeftX = reader.ReadFloat();
                    trackRightX = reader.ReadFloat();
                    trackBottomZ = reader.ReadFloat();
                    trackTopZ = reader.ReadFloat();
                    pitchBottomZ = reader.ReadFloat();
                    pitchTopZ = reader.ReadFloat();
                    nowBarX = reader.ReadFloat();
                    pitchGuides = Symbol.Read(reader);
                    tubeStyle = Symbol.Read(reader);
                    arrowStyle = Symbol.Read(reader);
                    fontStyle = Symbol.Read(reader);

                    leadText = Symbol.Read(reader);
                    harmText = Symbol.Read(reader);

                    if (revision >= 4)
                    {
                        leadPhonemeText = Symbol.Read(reader);
                        harmPhonemeText = Symbol.Read(reader);
                    }

                    lastMin = reader.ReadFloat();
                    lastMax = reader.ReadFloat();
                    middleCZPos = reader.ReadFloat();
                    tonic = reader.ReadInt32();

                    if (revision < 3)
                    {
                        unkSymbol10 = Symbol.Read(reader);
                        unkSymbol11 = Symbol.Read(reader);
                        unkBool3 = reader.ReadBoolean();
                        unkBool4 = reader.ReadBoolean();
                    }



                    rangeScaleAnim = Symbol.Read(reader);
                    rangeOffsetAnim = Symbol.Read(reader);

                    if (revision >= 4)
                    {
                        leadDeployMat = Symbol.Read(reader);
                        harmDeployMat = Symbol.Read(reader);
                    }
                }
            }

            LoadTrack(reader, entry.isProxy, false, true);

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
                if (revision < 2)
                {
                    writer.WriteUInt32(unkInt1);
                    writer.WriteUInt32(unkInt2);
                    writer.WriteFloat(unkFloat8);
                    writer.WriteUInt32(unkInt3);

                    writer.WriteUInt32((uint)configurableObjects.Count);
                    foreach (var obj in configurableObjects)
                    {
                        Symbol.Write(writer, obj);
                    }

                    foreach (var color in lyricColorsOld)
                    {
                        color.Write(writer);
                    }

                    Symbol.Write(writer, unkSymbol2);
                    Symbol.Write(writer, unkSymbol3);
                    Symbol.Write(writer, unkSymbol4);
                    Symbol.Write(writer, unkSymbol5);

                    Symbol.Write(writer, unkSymbol6);
                    Symbol.Write(writer, unkSymbol7);
                    Symbol.Write(writer, unkSymbol8);
                    Symbol.Write(writer, unkSymbol9);

                    unkColor.Write(writer);
                    unkColor2.Write(writer);

                    writer.WriteFloat(unkFloat9);
                    writer.WriteFloat(unkFloat10);

                    if (revision >= 1)
                    {
                        writer.WriteFloat(unkFloat11);
                        writer.WriteFloat(unkFloat12);
                    }

                    writer.WriteFloat(unkFloat13);
                    Symbol.Write(writer, unkSymbol1);
                    writer.WriteBoolean(unkBool1);

                    writer.WriteFloat(minPitchRange);
                    writer.WriteFloat(arrowSmoothing);
                    writer.WriteFloat(unkFloat14);
                    writer.WriteFloat(unkFloat15);

                    writer.WriteUInt32((uint)colors.Count);
                    foreach (var color in colors)
                    {
                        color.Write(writer);
                    }

                    writer.WriteUInt32((uint)floats.Count);
                    foreach (var value in floats)
                    {
                        writer.WriteFloat(value);
                    }

                    Symbol.Write(writer, unkSymbol10);
                    Symbol.Write(writer, unkSymbol11);
                    Symbol.Write(writer, unkSymbol12);
                    Symbol.Write(writer, unkSymbol13);

                    Symbol.Write(writer, unkSymbol14);
                    Symbol.Write(writer, unkSymbol15);
                    Symbol.Write(writer, unkSymbol16);
                    Symbol.Write(writer, unkSymbol17);

                    Symbol.Write(writer, phraseFeedbackTrig);
                    Symbol.Write(writer, spotlightSparklesOnlyTrig);
                    Symbol.Write(writer, spotlightPhraseSuccessTrig);

                    Symbol.Write(writer, unkSymbol18);
                    Symbol.Write(writer, unkSymbol19);
                    Symbol.Write(writer, unkSymbol20);
                    Symbol.Write(writer, unkSymbol21);

                    writer.WriteUInt32(unkInt4);
                    writer.WriteBoolean(unkBool7);

                    Symbol.Write(writer, unkSymbol22);
                    Symbol.Write(writer, unkSymbol23);
                    Symbol.Write(writer, unkSymbol24);
                    Symbol.Write(writer, unkSymbol25);

                    Symbol.Write(writer, unkSymbol26);
                    Symbol.Write(writer, unkSymbol27);
                    Symbol.Write(writer, unkSymbol28);
                    Symbol.Write(writer, unkSymbol29);

                    Symbol.Write(writer, unkSymbol30);
                    Symbol.Write(writer, unkSymbol31);
                    Symbol.Write(writer, unkSymbol32);
                    Symbol.Write(writer, unkSymbol33);
                }
                else
                {
                    writer.WriteUInt32((uint)configurableObjects.Count);
                    foreach (var obj in configurableObjects)
                    {
                        Symbol.Write(writer, obj);
                    }

                    Symbol.Write(writer, voxCfg);
                    Symbol.Write(writer, unkSymbol1);
                    writer.WriteFloat(minPitchRange);
                    writer.WriteFloat(arrowSmoothing);

                    if (revision < 3)
                        writer.WriteFloat(unkFloat1);
                    if (revision < 7)
                        writer.WriteFloat(unkFloat2);

                    if (revision < 3)
                    {
                        writer.WriteUInt32((uint)colors.Count);
                        foreach (var color in colors)
                        {
                            color.Write(writer);
                        }

                        Symbol.Write(writer, unkSymbol2);
                        Symbol.Write(writer, unkSymbol3);
                        Symbol.Write(writer, unkSymbol4);
                        Symbol.Write(writer, unkSymbol5);
                    }
                    else if (revision >= 6)
                    {
                        Symbol.Write(writer, tambourineSmasher);
                        Symbol.Write(writer, tambourineNowShowTrig);
                        Symbol.Write(writer, tambourineNowHideTrig);
                    }

                    Symbol.Write(writer, phraseFeedbackTrig);
                    Symbol.Write(writer, spotlightSparklesOnlyTrig);
                    Symbol.Write(writer, spotlightPhraseSuccessTrig);

                    writer.WriteUInt32((uint)lyricColors.Count);
                    foreach (var lyricColor in lyricColors)
                    {
                        lyricColor.Write(writer);
                    }

                    writer.WriteUInt32((uint)lyricAlphaMaps.Count);
                    foreach (var lyricAlphaMap in lyricAlphaMaps)
                    {
                        lyricAlphaMap.Write(writer);
                    }

                    if (revision < 5)
                    {
                        Symbol.Write(writer, streakMeter1);
                        Symbol.Write(writer, streakMeter2);
                    }

                    writer.WriteBoolean(pitchWindow);
                    writer.WriteFloat(pitchWindowHeight);
                    Symbol.Write(writer, pitchWindowMesh);
                    Symbol.Write(writer, pitchWindowOverlay);
                    writer.WriteBoolean(leadLyrics);
                    writer.WriteFloat(leadLyricHeight);
                    Symbol.Write(writer, leadLyricMesh);
                    writer.WriteBoolean(harmLyrics);
                    writer.WriteFloat(harmLyricHeight);
                    Symbol.Write(writer, harmLyricMesh);

                    if (revision < 3)
                    {
                        Symbol.Write(writer, unkSymbol6);
                        Symbol.Write(writer, unkSymbol7);
                        Symbol.Write(writer, unkSymbol8);
                        Symbol.Write(writer, unkSymbol9);

                        writer.WriteFloat(unkFloat3);
                        writer.WriteBoolean(unkBool1);
                    }

                    Symbol.Write(writer, leftDecoMesh);
                    Symbol.Write(writer, rightDecoMesh);
                    writer.WriteFloat(nowBarWidth);
                    if (revision < 3)
                        writer.WriteBoolean(unkBool2);
                    Symbol.Write(writer, nowBarMesh);

                    writer.WriteBoolean(remoteVocals);
                    writer.WriteFloat(trackLeftX);
                    writer.WriteFloat(trackRightX);
                    writer.WriteFloat(trackBottomZ);
                    writer.WriteFloat(trackTopZ);
                    writer.WriteFloat(pitchBottomZ);
                    writer.WriteFloat(pitchTopZ);
                    writer.WriteFloat(nowBarX);
                    Symbol.Write(writer, pitchGuides);
                    Symbol.Write(writer, tubeStyle);
                    Symbol.Write(writer, arrowStyle);
                    Symbol.Write(writer, fontStyle);

                    Symbol.Write(writer, leadText);
                    Symbol.Write(writer, harmText);

                    if (revision >= 4)
                    {
                        Symbol.Write(writer, leadPhonemeText);
                        Symbol.Write(writer, harmPhonemeText);
                    }

                    writer.WriteFloat(lastMin);
                    writer.WriteFloat(lastMax);
                    writer.WriteFloat(middleCZPos);
                    writer.WriteInt32(tonic);

                    if (revision < 3)
                    {
                        Symbol.Write(writer, unkSymbol10);
                        Symbol.Write(writer, unkSymbol11);
                        writer.WriteBoolean(unkBool3);
                        writer.WriteBoolean(unkBool4);
                    }

                    Symbol.Write(writer, rangeScaleAnim);
                    Symbol.Write(writer, rangeOffsetAnim);

                    if (revision >= 4)
                    {
                        Symbol.Write(writer, leadDeployMat);
                        Symbol.Write(writer, harmDeployMat);
                    }
                }
            }

            SaveTrack(writer, entry.isProxy, false, true);

            if (standalone)
            {
                writer.WriteEndBytes();
            }
        }


        public override bool IsDirectory()
        {
            return true;
        }

    }
}
