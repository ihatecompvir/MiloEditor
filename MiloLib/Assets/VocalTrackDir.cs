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

        public ushort altRevision;
        public ushort revision;

        public float hiddenPartAlpha;
        public bool unk2a4;
        public bool unk2a5;
        public bool isTop;
        public bool unk2a7;
        public int feedbackStateLead;
        public int feedbackStateHarm1;
        public int feedbackStateHarm2;
        private uint lyricColorsCount;
        public List<LyricColor> lyricColors = new(32);
        private uint lyricAlphaMapCount;
        public List<LyricAlphaMap> lyricAlphaMaps = new(32);
        public Symbol vocalMics = new(0, "");
        public Symbol unk2f0 = new(0, "");
        public float minPitchRange;
        public float pitchDisplayMargin;
        public float arrowSmoothing;
        private uint configurableObjectsCount;
        public List<Symbol> configurableObjects = new();
        public Symbol voxCfg = new(0, "");
        public Symbol tambourineSmasher = new(0, "");
        public Symbol tambourineNowShowTrig = new(0, "");
        public Symbol tambourineNowHideTrig = new(0, "");
        public Symbol leadPhraseFeedbackBottomLbl = new(0, "");
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
        public float trackLeftX;
        public float trackRightX;
        public float trackBottomZ;
        public float trackTopZ;
        public float pitchBottomZ;
        public float pitchTopZ;
        public float nowBarX;
        public float unk42c;
        public Symbol pitchGuides = new(0, "");
        public Symbol tubeStyle = new(0, "");
        public Symbol arrowStyle = new(0, "");
        public Symbol fontStyle = new(0, "");
        public Symbol leadText = new(0, "");
        public Symbol harmText = new(0, "");
        public Symbol leadPhonemeText = new(0, "");
        public Symbol harmPhonemeText = new(0, "");
        public float lastMin;
        public float lastMax;
        public float middleCZPos;
        public int tonic;
        public Symbol rangeScaleAnim = new(0, "");
        public Symbol rangeOffsetAnim = new(0, "");
        public bool unk4b0;
        public int unk4b4;
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
        public float unk694;
        public float unk698;
        public float unk69c;
        public float unk6a0;
        public Symbol leadDeployMat = new(0, "");
        public Symbol harmDeployMat = new(0, "");
        public float glowSize;
        public float glowAlpha;
        public int unk6c4;
        public bool unk6c8;
        public Symbol arrowFxDrawGrp = new(0, "");
        public float unk6d8;
        public float unk6dc;
        public bool unk6e0;





        public VocalTrackDir(ushort revision, ushort altRevision = 0) : base(revision, altRevision)
        {
            revision = revision;
            altRevision = altRevision;
            return;
        }

        public VocalTrackDir Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            base.Read(reader, false, parent, entry);

            if (revision < 2)
            {

                // Read configurableObjects (List<Symbol>)
                configurableObjectsCount = reader.ReadUInt32();
                configurableObjects = new List<Symbol>();
                for (uint i = 0; i < configurableObjectsCount; i++)
                {
                    configurableObjects.Add(Symbol.Read(reader));
                }

                lyricColors[5] = lyricColors[5].Read(reader);
                lyricColors[6] = lyricColors[6].Read(reader);
                lyricColors[7] = lyricColors[7].Read(reader);
                lyricColors[0x15] = lyricColors[0x15].Read(reader);
                lyricColors[0x16] = lyricColors[0x16].Read(reader);
                lyricColors[0x17] = lyricColors[0x17].Read(reader);
                lyricColors[0xC] = lyricColors[0xC].Read(reader);
                lyricColors[0xD] = lyricColors[0xD].Read(reader);
                lyricColors[0xE] = lyricColors[0xE].Read(reader);
                lyricColors[0xF] = lyricColors[0xF].Read(reader);
                lyricColors[0x1C] = lyricColors[0x1C].Read(reader);
                lyricColors[0x1D] = lyricColors[0x1D].Read(reader);
                lyricColors[0x1E] = lyricColors[0x1E].Read(reader);
                lyricColors[0x1F] = lyricColors[0x1F].Read(reader);

                unk2f0 = Symbol.Read(reader);
                minPitchRange = reader.ReadFloat();
                arrowSmoothing = reader.ReadFloat();

                phraseFeedbackTrig = Symbol.Read(reader);
                spotlightSparklesOnlyTrig = Symbol.Read(reader);
                spotlightPhraseSuccessTrig = Symbol.Read(reader);
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
                unk2f0 = Symbol.Read(reader);
                minPitchRange = reader.ReadFloat();
                arrowSmoothing = reader.ReadFloat();

                if (revision >= 6)
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

                leftDecoMesh = Symbol.Read(reader);
                rightDecoMesh = Symbol.Read(reader);
                nowBarWidth = reader.ReadFloat();
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

                rangeScaleAnim = Symbol.Read(reader);
                rangeOffsetAnim = Symbol.Read(reader);

                if (revision >= 4)
                {
                    leadDeployMat = Symbol.Read(reader);
                    harmDeployMat = Symbol.Read(reader);
                }
            }


            return this;
        }


        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            base.Write(writer, false, parent, entry);

            if (standalone)
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
        }

        public override bool IsDirectory()
        {
            return true;
        }

    }
}
