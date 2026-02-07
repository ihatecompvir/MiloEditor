using MiloLib.Assets.Band;
using MiloLib.Assets.Band.UI;
using MiloLib.Assets.Char;
using MiloLib.Assets.Ham;
using MiloLib.Assets.P9;
using MiloLib.Assets.Rnd;
using MiloLib.Assets.Synth;
using MiloLib.Assets.UI;
using MiloLib.Assets.World;
using MiloLib.Classes;
using MiloLib.Utils;
using MiloLib.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using static MiloLib.Assets.DirectoryMeta;
using static MiloLib.Assets.Ham.Range;

namespace MiloLib.Assets
{
    public class DirectoryMeta
    {
        public enum Platform
        {
            GameCube,
            PS2,
            PS3,
            Wii,
            Xbox,
            [Name("PC or iPod")]
            PC_or_iPod
        }

        // Factory delegates for creating and reading/writing directory objects
        private delegate Object DirectoryFactory(ushort revision);
        private delegate void DirectoryReader(EndianReader reader, Object dir, DirectoryMeta meta, Entry entry);
        private delegate void DirectoryWriter(EndianWriter writer, Object dir, DirectoryMeta meta, Entry entry);
        private delegate Object EntryReader(EndianReader reader, DirectoryMeta meta, Entry entry);
        private delegate void EntryWriter(EndianWriter writer, Object obj, DirectoryMeta meta, Entry entry);

        // optimization using dictionaries for that sweet O(1) lookup instead of a giant switch statement or if-else chain
        private static readonly Dictionary<string, DirectoryFactory> DirectoryFactories = new Dictionary<string, DirectoryFactory>
        {
            { "ObjectDir", (rev) => new ObjectDir(rev) },
            { "EndingBonusDir", (rev) => new RndDir(rev) },
            { "RndDir", (rev) => new RndDir(rev) },
            { "PanelDir", (rev) => new PanelDir(rev) },
            { "CharClipSet", (rev) => new CharClipSet(rev) },
            { "WorldDir", (rev) => new WorldDir(rev) },
            { "Character", (rev) => new Character(rev) },
            { "CompositeCharacter", (rev) => new CompositeCharacter(rev) },
            { "UILabelDir", (rev) => new UILabelDir(rev) },
            { "UIListDir", (rev) => new UIListDir(rev) },
            { "BandCrowdMeterDir", (rev) => new BandCrowdMeterDir(rev) },
            { "CrowdMeterIcon", (rev) => new BandCrowdMeterIcon(rev) },
            { "CharBoneDir", (rev) => new CharBoneDir(rev) },
            { "BandCharacter", (rev) => new BandCharacter(rev) },
            { "WorldInstance", (rev) => new WorldInstance(rev) },
            { "GemTrackDir", (rev) => new GemTrackDir(rev) },
            { "TrackPanelDir", (rev) => new TrackPanelDir(rev) },
            { "UnisonIcon", (rev) => new UnisonIcon(rev) },
            { "BandScoreboard", (rev) => new BandScoreboard(rev) },
            { "BandStarDisplay", (rev) => new BandStarDisplay(rev) },
            { "VocalTrackDir", (rev) => new VocalTrackDir(rev) },
            { "MoveDir", (rev) => new MoveDir(rev) },
            { "SkeletonDir", (rev) => new SkeletonDir(rev) },
            { "OvershellDir", (rev) => new OvershellDir(rev) },
            { "OverdriveMeterDir", (rev) => new OverdriveMeterDir(rev) },
            { "StreakMeterDir", (rev) => new StreakMeterDir(rev) },
            { "PitchArrowDir", (rev) => new PitchArrowDir(rev) },
            { "SynthDir", (rev) => new SynthDir(rev) },
            { "P9Character", (rev) => new P9Character(rev) },
        };

        private static readonly Dictionary<string, DirectoryReader> DirectoryReaders = new Dictionary<string, DirectoryReader>
        {
            { "ObjectDir", (r, d, m, e) => ((ObjectDir)d).Read(r, true, m, e) },
            { "EndingBonusDir", (r, d, m, e) => ((RndDir)d).Read(r, true, m, e) },
            { "RndDir", (r, d, m, e) => ((RndDir)d).Read(r, true, m, e) },
            { "PanelDir", (r, d, m, e) => ((PanelDir)d).Read(r, true, m, e) },
            { "CharClipSet", (r, d, m, e) => ((CharClipSet)d).Read(r, true, m, e) },
            { "WorldDir", (r, d, m, e) => ((WorldDir)d).Read(r, true, m, e) },
            { "Character", (r, d, m, e) => ((Character)d).Read(r, true, m, e) },
            { "CompositeCharacter", (r, d, m, e) => ((CompositeCharacter)d).Read(r, true, m, e) },
            { "UILabelDir", (r, d, m, e) => ((UILabelDir)d).Read(r, true, m, e) },
            { "UIListDir", (r, d, m, e) => ((UIListDir)d).Read(r, true, m, e) },
            { "BandCrowdMeterDir", (r, d, m, e) => ((BandCrowdMeterDir)d).Read(r, true, m, e) },
            { "CrowdMeterIcon", (r, d, m, e) => ((BandCrowdMeterIcon)d).Read(r, true, m, e) },
            { "CharBoneDir", (r, d, m, e) => ((CharBoneDir)d).Read(r, true, m, e) },
            { "BandCharacter", (r, d, m, e) => ((BandCharacter)d).Read(r, true, m, e) },
            { "WorldInstance", (r, d, m, e) => ((WorldInstance)d).Read(r, true, m, e) },
            { "GemTrackDir", (r, d, m, e) => ((GemTrackDir)d).Read(r, true, m, e) },
            { "TrackPanelDir", (r, d, m, e) => ((TrackPanelDir)d).Read(r, true, m, e) },
            { "UnisonIcon", (r, d, m, e) => ((UnisonIcon)d).Read(r, true, m, e) },
            { "BandScoreboard", (r, d, m, e) => ((BandScoreboard)d).Read(r, true, m, e) },
            { "BandStarDisplay", (r, d, m, e) => ((BandStarDisplay)d).Read(r, true, m, e) },
            { "VocalTrackDir", (r, d, m, e) => ((VocalTrackDir)d).Read(r, true, m, e) },
            { "MoveDir", (r, d, m, e) => ((MoveDir)d).Read(r, true, m, e) },
            { "SkeletonDir", (r, d, m, e) => ((SkeletonDir)d).Read(r, true, m, e) },
            { "OvershellDir", (r, d, m, e) => ((OvershellDir)d).Read(r, true, m, e) },
            { "OverdriveMeterDir", (r, d, m, e) => ((OverdriveMeterDir)d).Read(r, true, m, e) },
            { "StreakMeterDir", (r, d, m, e) => ((StreakMeterDir)d).Read(r, true, m, e) },
            { "PitchArrowDir", (r, d, m, e) => ((PitchArrowDir)d).Read(r, true, m, e) },
            { "SynthDir", (r, d, m, e) => ((SynthDir)d).Read(r, true, m, e) },
            { "P9Character", (r, d, m, e) => ((P9Character)d).Read(r, true, m, e) },
        };

        private static readonly Dictionary<string, DirectoryWriter> DirectoryWriters = new Dictionary<string, DirectoryWriter>
        {
            { "ObjectDir", (w, d, m, e) => ((ObjectDir)d).Write(w, true, m, e) },
            { "RndDir", (w, d, m, e) => ((RndDir)d).Write(w, true, m, e) },
            { "PanelDir", (w, d, m, e) => ((PanelDir)d).Write(w, true, m, e) },
            { "WorldDir", (w, d, m, e) => ((WorldDir)d).Write(w, true, m, e) },
            { "Character", (w, d, m, e) => ((Character)d).Write(w, true, m, e) },
            { "P9Character", (w, d, m, e) => ((P9Character)d).Write(w, true, m, e) },
            { "CharBoneDir", (w, d, m, e) => ((CharBoneDir)d).Write(w, true, m, e) },
            { "CharClipSet", (w, d, m, e) => ((CharClipSet)d).Write(w, true, m, e) },
            { "CompositeCharacter", (w, d, m, e) => ((CompositeCharacter)d).Write(w, true, m, e) },
            { "UILabelDir", (w, d, m, e) => ((UILabelDir)d).Write(w, true, m, e) },
            { "UIListDir", (w, d, m, e) => ((UIListDir)d).Write(w, true, m, e) },
            { "BandCrowdMeterDir", (w, d, m, e) => ((BandCrowdMeterDir)d).Write(w, true, m, e) },
            { "CrowdMeterIcon", (w, d, m, e) => ((BandCrowdMeterIcon)d).Write(w, true, m, e) },
            { "BandCharacter", (w, d, m, e) => ((BandCharacter)d).Write(w, true, m, e) },
            { "WorldInstance", (w, d, m, e) => ((WorldInstance)d).Write(w, true, m, e) },
            { "TrackDir", (w, d, m, e) => ((TrackDir)d).Write(w, true, m, e) },
            { "GemTrackDir", (w, d, m, e) => ((GemTrackDir)d).Write(w, true, m, e) },
            { "TrackPanelDir", (w, d, m, e) => ((TrackPanelDir)d).Write(w, true, m, e) },
            { "UnisonIcon", (w, d, m, e) => ((UnisonIcon)d).Write(w, true, m, e) },
            { "EndingBonusDir", (w, d, m, e) => ((RndDir)d).Write(w, true, m, e) },
            { "BandStarDisplay", (w, d, m, e) => ((BandStarDisplay)d).Write(w, true, m, e) },
            { "BandScoreboard", (w, d, m, e) => ((BandScoreboard)d).Write(w, true, m, e) },
            { "VocalTrackDir", (w, d, m, e) => ((VocalTrackDir)d).Write(w, true, m, e) },
            { "MoveDir", (w, d, m, e) => ((MoveDir)d).Write(w, true, m, e) },
            { "SkeletonDir", (w, d, m, e) => ((SkeletonDir)d).Write(w, true, m, e) },
            { "PitchArrowDir", (w, d, m, e) => ((PitchArrowDir)d).Write(w, true, m, e) },
            { "OvershellDir", (w, d, m, e) => ((OvershellDir)d).Write(w, true, m, e) },
            { "OverdriveMeterDir", (w, d, m, e) => ((OverdriveMeterDir)d).Write(w, true, m, e) },
            { "StreakMeterDir", (w, d, m, e) => ((StreakMeterDir)d).Write(w, true, m, e) },
            { "SynthDir", (w, d, m, e) => ((SynthDir)d).Write(w, true, m, e) },
        };

        // Entry read/write action delegates - these handle the custom logic for each entry type
        private delegate void EntryReadAction(EndianReader reader, DirectoryMeta meta, Entry entry);
        private delegate void EntryWriteAction(EndianWriter writer, DirectoryMeta meta, Entry entry);

        // Dictionary for entry read actions (O(1) lookup)
        private static readonly Dictionary<string, EntryReadAction> EntryReadActions = new Dictionary<string, EntryReadAction>();

        // Dictionary for entry write actions (O(1) lookup)
        private static readonly Dictionary<string, EntryWriteAction> EntryWriteActions = new Dictionary<string, EntryWriteAction>();

        // Initialize entry dictionaries in static constructor
        static DirectoryMeta()
        {
            InitializeEntryReadActions();
            InitializeEntryWriteActions();
        }

        private static void InitializeEntryReadActions()
        {
            // simple object entries (don't require special cases or random bullshit - just read the object)
            var simpleObjectReaders = new Dictionary<string, System.Func<EndianReader, DirectoryMeta, Entry, Object>>
            {
                { "AnimFilter", (r, m, e) => new RndAnimFilter().Read(r, true, m, e) },
                { "BandButton", (r, m, e) => new BandButton().Read(r, true, m, e) },
                { "BandCamShot", (r, m, e) => new BandCamShot().Read(r, true, m, e) },
                { "BandCharDesc", (r, m, e) => new BandCharDesc().Read(r, true, m, e) },
                { "BandConfiguration", (r, m, e) => new BandConfiguration().Read(r, true, m, e) },
                { "BandDirector", (r, m, e) => new BandDirector().Read(r, true, m, e) },
                { "BandFaceDeform", (r, m, e) => new BandFaceDeform().Read(r, true, m, e) },
                { "BandLabel", (r, m, e) => new BandLabel().Read(r, true, m, e) },
                { "BandList", (r, m, e) => new BandList().Read(r, true, m, e) },
                { "BandPlacer", (r, m, e) => new BandPlacer().Read(r, true, m, e) },
                { "BandScreen", (r, m, e) => new Object().Read(r, true, m, e) },
                { "BandSongPref", (r, m, e) => new BandSongPref().Read(r, true, m, e) },
                { "BandSwatch", (r, m, e) => new BandSwatch().Read(r, true, m, e) },
                { "BustAMoveData", (r, m, e) => new BustAMoveData().Read(r, true, m, e) },
                { "Cam", (r, m, e) => new RndCam().Read(r, true, m, e) },
                { "CamShot", (r, m, e) => new CamShot().Read(r, true, m, e) },
                { "CharClipGroup", (r, m, e) => new CharClipGroup().Read(r, true, m, e) },
                { "CharCollide", (r, m, e) => new CharCollide().Read(r, true, m, e) },
                { "CharForeTwist", (r, m, e) => new CharForeTwist().Read(r, true, m, e) },
                { "CharGuitarString", (r, m, e) => new CharGuitarString().Read(r, true, m, e) },
                { "CharHair", (r, m, e) => new CharHair().Read(r, true, m, e) },
                { "CharIKMidi", (r, m, e) => new CharIKMidi().Read(r, true, m, e) },
                { "CharIKRod", (r, m, e) => new CharIKRod().Read(r, true, m, e) },
                { "CharInterest", (r, m, e) => new CharInterest().Read(r, true, m, e) },
                { "CharMeshHide", (r, m, e) => new CharMeshHide().Read(r, true, m, e) },
                { "CharPosConstraint", (r, m, e) => new CharPosConstraint().Read(r, true, m, e) },
                { "CharServoBone", (r, m, e) => new CharServoBone().Read(r, true, m, e) },
                { "CharUpperTwist", (r, m, e) => new CharUpperTwist().Read(r, true, m, e) },
                { "CharWalk", (r, m, e) => new CharWalk().Read(r, true, m, e) },
                { "CharWeightSetter", (r, m, e) => new CharWeightSetter().Read(r, true, m, e) },
                { "CheckboxDisplay", (r, m, e) => new CheckboxDisplay().Read(r, true, m, e) },
                { "ColorPalette", (r, m, e) => new ColorPalette().Read(r, true, m, e) },
                { "DancerSequence", (r, m, e) => new DancerSequence().Read(r, true, m, e) },
                { "Environ", (r, m, e) => new RndEnviron().Read(r, true, m, e) },
                { "EventTrigger", (r, m, e) => new EventTrigger().Read(r, true, m, e) },
                { "FileMerger", (r, m, e) => new FileMerger().Read(r, true, m, e) },
                { "Font", (r, m, e) => new RndFont().Read(r, true, m, e) },
                { "Fur", (r, m, e) => new RndFur().Read(r, true, m, e) },
                { "Group", (r, m, e) => new RndGroup().Read(r, true, m, e) },
                { "View", (r, m, e) => new RndGroup().Read(r, true, m, e) },
                { "HamBattleData", (r, m, e) => new HamBattleData().Read(r, true, m, e) },
                { "HamMove", (r, m, e) => new HamMove().Read(r, true, m, e) },
                { "HamPartyJumpData", (r, m, e) => new HamPartyJumpData().Read(r, true, m, e) },
                { "HamSupereasyData", (r, m, e) => new HamSupereasyData().Read(r, true, m, e) },
                { "InlineHelp", (r, m, e) => new InlineHelp().Read(r, true, m, e) },
                { "InterstitialPanel", (r, m, e) => new Object().Read(r, true, m, e) },
                { "Light", (r, m, e) => new RndLight().Read(r, true, m, e) },
                { "Mat", (r, m, e) => new RndMat().Read(r, true, m, e) },
                { "MatAnim", (r, m, e) => new RndMatAnim().Read(r, true, m, e) },
                { "Mesh", (r, m, e) => new RndMesh().Read(r, true, m, e) },
                { "MotionBlur", (r, m, e) => new RndMotionBlur().Read(r, true, m, e) },
                { "MoveGraph", (r, m, e) => new MoveGraph().Read(r, true, m, e) },
                { "MsgSource", (r, m, e) => new Object().Read(r, true, m, e) },
                { "Object", (r, m, e) => new Object().Read(r, true, m, e) },
                { "P9Director", (r, m, e) => new P9Director().Read(r, true, m, e) },
                { "ParticleSys", (r, m, e) => new RndParticleSys().Read(r, true, m, e) },
                { "ParticleSysAnim", (r, m, e) => new RndParticleSysAnim().Read(r, true, m, e) },
                { "PollAnim", (r, m, e) => new RndPollAnim().Read(r, true, m, e) },
                { "PostProc", (r, m, e) => new RndPostProc().Read(r, true, m, e) },
                { "PracticeSection", (r, m, e) => new PracticeSection().Read(r, true, m, e) },
                { "PropAnim", (r, m, e) => new RndPropAnim().Read(r, true, m, e) },
                { "RandomGroupSeq", (r, m, e) => new RandomGroupSeq().Read(r, true, m, e) },
                { "ScreenMask", (r, m, e) => new RndScreenMask().Read(r, true, m, e) },
                { "Text", (r, m, e) => new RndText().Read(r, true, m, e) },
                { "Set", (r, m, e) => new RndSet().Read(r, true, m, e) },
                { "Sfx", (r, m, e) => new Sfx().Read(r, true, m, e) },
                { "SpotlightDrawer", (r, m, e) => new SpotlightDrawer().Read(r, true, m, e) },
                { "SynthSample", (r, m, e) => new SynthSample().Read(r, true, m, e) },
                { "Tex", (r, m, e) => new RndTex().Read(r, true, m, e) },
                { "TexBlendController", (r, m, e) => new RndTexBlendController().Read(r, true, m, e) },
                { "TexBlender", (r, m, e) => new RndTexBlender().Read(r, true, m, e) },
                { "TexMovie", (r, m, e) => new RndTexMovie().Read(r, true, m, e) },
                { "TrackWidget", (r, m, e) => new TrackWidget().Read(r, true, m, e) },
                { "TrainerChallenge", (r, m, e) => new Object().Read(r, true, m, e) },
                { "Trans", (r, m, e) => new RndTrans().Read(r, true, m, e) },
                { "TransAnim", (r, m, e) => new RndTransAnim().Read(r, true, m, e) },
                { "TransProxy", (r, m, e) => new RndTransProxy().Read(r, true, m, e) },
                { "UIButton", (r, m, e) => new UIButton().Read(r, true, m, e) },
                { "UIColor", (r, m, e) => new UIColor().Read(r, true, m, e) },
                { "UIGuide", (r, m, e) => new UIGuide().Read(r, true, m, e) },
                { "UILabel", (r, m, e) => new UILabel().Read(r, true, m, e) },
                { "UIList", (r, m, e) => new UIList().Read(r, true, m, e) },
                { "UIListArrow", (r, m, e) => new UIListArrow().Read(r, true, m, e) },
                { "UIListCustom", (r, m, e) => new UIListCustom().Read(r, true, m, e) },
                { "UIListHighlight", (r, m, e) => new UIListHighlight().Read(r, true, m, e) },
                { "UIListLabel", (r, m, e) => new UIListLabel().Read(r, true, m, e) },
                { "UIListMesh", (r, m, e) => new UIListMesh().Read(r, true, m, e) },
                { "UIListSlot", (r, m, e) => new UIListSlot().Read(r, true, m, e) },
                { "UIListWidget", (r, m, e) => new UIListWidget().Read(r, true, m, e) },
                { "UIPanel", (r, m, e) => new Object().Read(r, true, m, e) },
                { "UIPicture", (r, m, e) => new UIPicture().Read(r, true, m, e) },
                { "UISlider", (r, m, e) => new UISlider().Read(r, true, m, e) },
                { "UITrigger", (r, m, e) => new UITrigger().Read(r, true, m, e) },
                { "Wind", (r, m, e) => new RndWind().Read(r, true, m, e) },
                { "WorldCrowd", (r, m, e) => new WorldCrowd().Read(r, true, m, e) },
                { "WorldReflection", (r, m, e) => new WorldReflection().Read(r, true, m, e) },
            };

            foreach (var kvp in simpleObjectReaders)
            {
                EntryReadActions[kvp.Key] = (reader, meta, entry) =>
                {
                    Debug.WriteLine($"Reading entry {kvp.Key} {entry.name.value}");
                    entry.obj = kvp.Value(reader, meta, entry);
                };
            }

            // Complex directory entries with special handling

            // Helper to create simple directory entries (read obj, set isProxy, read dir)
            Action<string, Func<EndianReader, DirectoryMeta, Entry, Object>> AddSimpleDirEntry = (typeName, objReader) =>
            {
                EntryReadActions[typeName] = (reader, meta, entry) =>
                {
                    Debug.WriteLine($"Reading entry {typeName} {entry.name.value}");
                    entry.isProxy = true;
                    entry.obj = objReader(reader, meta, entry);

                    DirectoryMeta dir = new DirectoryMeta();
                    dir.platform = meta.platform;
                    dir.Read(reader);
                    entry.dir = dir;
                };
            };

            // Simple directory entries (all follow same pattern: read obj, set isProxy, read dir)
            AddSimpleDirEntry("BandCharacter", (r, m, e) => new BandCharacter(0).Read(r, true, m, e));
            AddSimpleDirEntry("BandCrowdMeterDir", (r, m, e) => new BandCrowdMeterDir(0).Read(r, true, m, e));
            AddSimpleDirEntry("CrowdMeterIcon", (r, m, e) => new BandCrowdMeterIcon(0).Read(r, true, m, e));
            AddSimpleDirEntry("BandScoreboard", (r, m, e) => new BandScoreboard(0).Read(r, true, m, e));
            AddSimpleDirEntry("BandStarDisplay", (r, m, e) => new BandStarDisplay(0).Read(r, true, m, e));
            AddSimpleDirEntry("CharBoneDir", (r, m, e) => new CharBoneDir(0).Read(r, true, m, e));
            AddSimpleDirEntry("CompositeCharacter", (r, m, e) => new CompositeCharacter(0).Read(r, true, m, e));
            AddSimpleDirEntry("GemTrackDir", (r, m, e) => new GemTrackDir(0).Read(r, true, m, e));
            AddSimpleDirEntry("MoveDir", (r, m, e) => new MoveDir(0).Read(r, true, m, e));
            AddSimpleDirEntry("OverdriveMeterDir", (r, m, e) => new OverdriveMeterDir(0).Read(r, true, m, e));
            AddSimpleDirEntry("OvershellDir", (r, m, e) => new OvershellDir(0).Read(r, true, m, e));
            AddSimpleDirEntry("P9Character", (r, m, e) => new P9Character(0).Read(r, true, m, e));
            AddSimpleDirEntry("PitchArrowDir", (r, m, e) => new PitchArrowDir(0).Read(r, true, m, e));
            AddSimpleDirEntry("PanelDir", (r, m, e) => new PanelDir(0).Read(r, true, m, e));
            AddSimpleDirEntry("SkeletonDir", (r, m, e) => new SkeletonDir(0).Read(r, true, m, e));
            AddSimpleDirEntry("StreakMeterDir", (r, m, e) => new StreakMeterDir(0).Read(r, true, m, e));
            AddSimpleDirEntry("SynthDir", (r, m, e) => new SynthDir(0).Read(r, true, m, e));
            AddSimpleDirEntry("TrackDir", (r, m, e) => new TrackDir(0).Read(r, true, m, e));
            AddSimpleDirEntry("TrackPanelDir", (r, m, e) => new TrackPanelDir(0).Read(r, true, m, e));
            AddSimpleDirEntry("UILabelDir", (r, m, e) => new UILabelDir(0).Read(r, true, m, e));
            AddSimpleDirEntry("UIListDir", (r, m, e) => new UIListDir(0).Read(r, true, m, e));
            AddSimpleDirEntry("UnisonIcon", (r, m, e) => new UnisonIcon(0).Read(r, true, m, e));
            AddSimpleDirEntry("VocalTrackDir", (r, m, e) => new VocalTrackDir(0).Read(r, true, m, e));
            AddSimpleDirEntry("WorldDir", (r, m, e) => new WorldDir(0).Read(r, true, m, e));

            // Character - conditional dir read based on proxyPath
            EntryReadActions["Character"] = (reader, meta, entry) =>
            {
                Debug.WriteLine($"Reading entry Character {entry.name.value}");
                entry.isProxy = true;
                entry.obj = new Character(0).Read(reader, true, meta, entry);

                if (((Character)entry.obj).proxyPath != String.Empty)
                {
                    DirectoryMeta dir = new DirectoryMeta();
                    dir.platform = meta.platform;
                    dir.Read(reader);
                    entry.dir = dir;
                }
            };

            // CharClipSet - conditional dir read based on inlineProxy
            EntryReadActions["CharClipSet"] = (reader, meta, entry) =>
            {
                Debug.WriteLine($"Reading entry CharClipSet {entry.name.value}");
                entry.isProxy = true;
                entry.obj = new CharClipSet(0).Read(reader, true, meta, entry);

                if (((ObjectDir)entry.obj).inlineProxy && ((ObjectDir)entry.obj).proxyPath.value != "")
                {
                    DirectoryMeta dir = new DirectoryMeta();
                    dir.platform = meta.platform;
                    dir.Read(reader);
                    entry.dir = dir;
                }
            };

            // RndDir and EndingBonusDir - conditional dir read based on inlineProxy
            EntryReadActions["RndDir"] = EntryReadActions["EndingBonusDir"] = (reader, meta, entry) =>
            {
                Debug.WriteLine($"Reading entry RndDir {entry.name.value}");
                entry.isProxy = true;
                entry.obj = new RndDir(0).Read(reader, true, meta, entry);

                if (((ObjectDir)entry.obj).inlineProxy && ((ObjectDir)entry.obj).proxyPath.value != "")
                {
                    DirectoryMeta dir = new DirectoryMeta();
                    dir.platform = meta.platform;
                    dir.Read(reader);
                    entry.dir = dir;
                }
            };

            // ObjectDir - conditional dir read based on inlineProxy
            EntryReadActions["ObjectDir"] = (reader, meta, entry) =>
            {
                Debug.WriteLine($"Reading entry ObjectDir {entry.name.value}");
                entry.isProxy = true;
                entry.obj = new ObjectDir(0).Read(reader, true, meta, entry);

                if (((ObjectDir)entry.obj).inlineProxy && ((ObjectDir)entry.obj).proxyPath.value != "")
                {
                    DirectoryMeta dir = new DirectoryMeta();
                    dir.platform = meta.platform;
                    dir.Read(reader);
                    entry.dir = dir;
                }
            };

            // WorldInstance - i dont even know
            EntryReadActions["WorldInstance"] = (reader, meta, entry) =>
            {
                entry.isProxy = true;

                entry.obj = new WorldInstance(0).Read(reader, false, meta, entry);

                if (((WorldInstance)entry.obj).revision == 0)
                {
                    if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32())
                        throw new Exception($"WorldInstance '{entry.name.value}' revision 0: end bytes not found at 0x{reader.BaseStream.Position:X}");
                    return;
                }

                // if the world instance has no persistent objects, it will have a dir as expected, otherwise it won't
                if (!((WorldInstance)entry.obj).hasPersistentObjects)
                {
                    DirectoryMeta dir = new DirectoryMeta();
                    dir.platform = meta.platform;
                    dir.Read(reader);
                    entry.dir = dir;

                    if (entry.dir != null && entry.dir.type.value == "WorldInstance" && ((WorldInstance)dir.directory).hasPersistentObjects)
                    {
                        ((WorldInstance)dir.directory).persistentObjects = new WorldInstance.PersistentObjects().Read(reader, meta, entry, ((WorldInstance)entry.obj).revision);
                    }
                    else
                    {
                        ((WorldInstance)entry.obj).persistentObjects = new WorldInstance.PersistentObjects().Read(reader, meta, entry, ((WorldInstance)entry.obj).revision);
                    }
                }
                else
                {
                    ((WorldInstance)entry.obj).persistentObjects = new WorldInstance.PersistentObjects().Read(reader, meta, entry, ((WorldInstance)entry.obj).revision);
                }
            };

            // OutfitConfig - conditional on revision
            EntryReadActions["OutfitConfig"] = (reader, meta, entry) =>
            {
                if (meta.revision != 28)
                {
                    // Fall through to default behavior (read until 0xADDEADDE)
                    entry.typeRecognized = false;
                    while (true)
                    {
                        byte b = reader.ReadByte();
                        if (b == 0xAD)
                        {
                            byte b2 = reader.ReadByte();
                            if (b2 == 0xDE)
                            {
                                byte b3 = reader.ReadByte();
                                if (b3 == 0xAD)
                                {
                                    byte b4 = reader.ReadByte();
                                    if (b4 == 0xDE)
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    return;
                }

                Debug.WriteLine($"Reading entry OutfitConfig {entry.name.value}");
                entry.obj = new OutfitConfig().Read(reader, true, meta, entry);
            };
        }

        private static void InitializeEntryWriteActions()
        {
            // Simple object entries (write object only)
            var simpleObjectWriters = new Dictionary<string, System.Action<EndianWriter, Object, DirectoryMeta, Entry>>
            {
                { "AnimFilter", (w, o, m, e) => ((RndAnimFilter)o).Write(w, true, m, e) },
                { "BandButton", (w, o, m, e) => ((BandButton)o).Write(w, true, m, e) },
                { "BandCamShot", (w, o, m, e) => ((BandCamShot)o).Write(w, true, m, e) },
                { "BandCharDesc", (w, o, m, e) => ((BandCharDesc)o).Write(w, true, m, e) },
                { "BandConfiguration", (w, o, m, e) => ((BandConfiguration)o).Write(w, true, m, e) },
                { "BandDirector", (w, o, m, e) => ((BandDirector)o).Write(w, true, m, e) },
                { "BandFaceDeform", (w, o, m, e) => ((BandFaceDeform)o).Write(w, true, m, e) },
                { "BandLabel", (w, o, m, e) => ((BandLabel)o).Write(w, true, m, e) },
                { "BandList", (w, o, m, e) => ((BandList)o).Write(w, true, m, e) },
                { "BandPlacer", (w, o, m, e) => ((BandPlacer)o).Write(w, true, m, e) },
                { "BandSongPref", (w, o, m, e) => ((BandSongPref)o).Write(w, true, m, e) },
                { "BandSwatch", (w, o, m, e) => ((BandSwatch)o).Write(w, true, m, e) },
                { "BustAMoveData", (w, o, m, e) => ((BustAMoveData)o).Write(w, true, m, e) },
                { "Cam", (w, o, m, e) => ((RndCam)o).Write(w, true, m, e) },
                { "CamShot", (w, o, m, e) => ((CamShot)o).Write(w, true, m, e) },
                { "CharClipGroup", (w, o, m, e) => ((CharClipGroup)o).Write(w, true, m, e) },
                { "CharCollide", (w, o, m, e) => ((CharCollide)o).Write(w, true, m, e) },
                { "CharForeTwist", (w, o, m, e) => ((CharForeTwist)o).Write(w, true, m, e) },
                { "CharGuitarString", (w, o, m, e) => ((CharGuitarString)o).Write(w, true, m, e) },
                { "CharHair", (w, o, m, e) => ((CharHair)o).Write(w, true, m, e) },
                { "CharIKMidi", (w, o, m, e) => ((CharIKMidi)o).Write(w, true, m, e) },
                { "CharIKRod", (w, o, m, e) => ((CharIKRod)o).Write(w, true, m, e) },
                { "CharInterest", (w, o, m, e) => ((CharInterest)o).Write(w, true, m, e) },
                { "CharMeshHide", (w, o, m, e) => ((CharMeshHide)o).Write(w, true, m, e) },
                { "CharPosConstraint", (w, o, m, e) => ((CharPosConstraint)o).Write(w, true, m, e) },
                { "CharServoBone", (w, o, m, e) => ((CharServoBone)o).Write(w, true, m, e) },
                { "CharUpperTwist", (w, o, m, e) => ((CharUpperTwist)o).Write(w, true, m, e) },
                { "CharWalk", (w, o, m, e) => ((CharWalk)o).Write(w, true, m, e) },
                { "CharWeightSetter", (w, o, m, e) => ((CharWeightSetter)o).Write(w, true, m, e) },
                { "CheckboxDisplay", (w, o, m, e) => ((CheckboxDisplay)o).Write(w, true, m, e) },
                { "ColorPalette", (w, o, m, e) => ((ColorPalette)o).Write(w, true, m, e) },
                { "DancerSequence", (w, o, m, e) => ((DancerSequence)o).Write(w, true, m, e) },
                { "Environ", (w, o, m, e) => ((RndEnviron)o).Write(w, true, m, e) },
                { "EventTrigger", (w, o, m, e) => ((EventTrigger)o).Write(w, true, m, e) },
                { "FileMerger", (w, o, m, e) => ((FileMerger)o).Write(w, true, m, e) },
                { "Font", (w, o, m, e) => ((RndFont)o).Write(w, true, m, e) },
                { "Fur", (w, o, m, e) => ((RndFur)o).Write(w, true, m, e) },
                { "Group", (w, o, m, e) => ((RndGroup)o).Write(w, true, m, e) },
                { "HamBattleData", (w, o, m, e) => ((HamBattleData)o).Write(w, true, m, e) },
                { "HamMove", (w, o, m, e) => ((HamMove)o).Write(w, true, m, e) },
                { "HamPartyJumpData", (w, o, m, e) => ((HamPartyJumpData)o).Write(w, true, m, e) },
                { "HamSupereasyData", (w, o, m, e) => ((HamSupereasyData)o).Write(w, true, m, e) },
                { "InlineHelp", (w, o, m, e) => ((InlineHelp)o).Write(w, true, m, e) },
                { "Light", (w, o, m, e) => ((RndLight)o).Write(w, true, m, e) },
                { "Mat", (w, o, m, e) => ((RndMat)o).Write(w, true, m, e) },
                { "MatAnim", (w, o, m, e) => ((RndMatAnim)o).Write(w, true, m, e) },
                { "Mesh", (w, o, m, e) => ((RndMesh)o).Write(w, true, m, e) },
                { "MotionBlur", (w, o, m, e) => ((RndMotionBlur)o).Write(w, true, m, e) },
                { "MoveGraph", (w, o, m, e) => ((MoveGraph)o).Write(w, true, m, e) },
                { "OutfitConfig", (w, o, m, e) => ((OutfitConfig)o).Write(w, true, m, e) },
                { "ParticleSys", (w, o, m, e) => ((RndParticleSys)o).Write(w, true, m, e) },
                { "ParticleSysAnim", (w, o, m, e) => ((RndParticleSysAnim)o).Write(w, true, m, e) },
                { "PollAnim", (w, o, m, e) => ((RndPollAnim)o).Write(w, true, m, e) },
                { "PostProc", (w, o, m, e) => ((RndPostProc)o).Write(w, true, m, e) },
                { "PracticeSection", (w, o, m, e) => ((PracticeSection)o).Write(w, true, m, e) },
                { "PropAnim", (w, o, m, e) => ((RndPropAnim)o).Write(w, true, m, e) },
                { "RandomGroupSeq", (w, o, m, e) => ((RandomGroupSeq)o).Write(w, true, m, e) },
                { "ScreenMask", (w, o, m, e) => ((RndScreenMask)o).Write(w, true, m, e) },
                { "Text", (w, o, m, e) => ((RndText)o).Write(w, true, m, e) },
                { "Set", (w, o, m, e) => ((RndSet)o).Write(w, true, m, e) },
                { "Sfx", (w, o, m, e) => ((Sfx)o).Write(w, true, m, e) },
                { "SpotlightDrawer", (w, o, m, e) => ((SpotlightDrawer)o).Write(w, true, m, e) },
                { "SynthSample", (w, o, m, e) => ((SynthSample)o).Write(w, true, m, e) },
                { "Tex", (w, o, m, e) => ((RndTex)o).Write(w, true, m, e) },
                { "TexBlendController", (w, o, m, e) => ((RndTexBlendController)o).Write(w, true, m, e) },
                { "TexBlender", (w, o, m, e) => ((RndTexBlender)o).Write(w, true, m, e) },
                { "TexMovie", (w, o, m, e) => ((RndTexMovie)o).Write(w, true, m, e) },
                { "TrackWidget", (w, o, m, e) => ((TrackWidget)o).Write(w, true, m, e) },
                { "TransAnim", (w, o, m, e) => ((RndTransAnim)o).Write(w, true, m, e) },
                { "TransProxy", (w, o, m, e) => ((RndTransProxy)o).Write(w, true, m, e) },
                { "UIButton", (w, o, m, e) => ((UIButton)o).Write(w, true, m, e) },
                { "UIColor", (w, o, m, e) => ((UIColor)o).Write(w, true, m, e) },
                { "UIComponent", (w, o, m, e) => ((UIComponent)o).Write(w, true, m, e) },
                { "UIGuide", (w, o, m, e) => ((UIGuide)o).Write(w, true, m, e) },
                { "UILabel", (w, o, m, e) => ((UILabel)o).Write(w, true, m, e) },
                { "UIList", (w, o, m, e) => ((UIList)o).Write(w, true, m, e) },
                { "UIListArrow", (w, o, m, e) => ((UIListArrow)o).Write(w, true, m, e) },
                { "UIListCustom", (w, o, m, e) => ((UIListCustom)o).Write(w, true, m, e) },
                { "UIListHighlight", (w, o, m, e) => ((UIListHighlight)o).Write(w, true, m, e) },
                { "UIListLabel", (w, o, m, e) => ((UIListLabel)o).Write(w, true, m, e) },
                { "UIListMesh", (w, o, m, e) => ((UIListMesh)o).Write(w, true, m, e) },
                { "UIListSlot", (w, o, m, e) => ((UIListSlot)o).Write(w, true, m, e) },
                { "UIListWidget", (w, o, m, e) => ((UIListWidget)o).Write(w, true, m, e) },
                { "UIPicture", (w, o, m, e) => ((UIPicture)o).Write(w, true, m, e) },
                { "UISlider", (w, o, m, e) => ((UISlider)o).Write(w, true, m, e) },
                { "UITrigger", (w, o, m, e) => ((UITrigger)o).Write(w, true, m, e) },
                { "Wind", (w, o, m, e) => ((RndWind)o).Write(w, true, m, e) },
                { "WorldReflection", (w, o, m, e) => ((WorldReflection)o).Write(w, true, m, e) },
            };

            foreach (var kvp in simpleObjectWriters)
            {
                EntryWriteActions[kvp.Key] = (writer, meta, entry) =>
                {
                    kvp.Value(writer, entry.obj, meta, entry);
                };
            }

            // Special case for Trans which has different parameters
            EntryWriteActions["Trans"] = (writer, meta, entry) =>
            {
                ((RndTrans)entry.obj).Write(writer, true, meta, false);
            };

            // Complex directory entry write actions

            // Helper to create simple directory entry writers (write obj, set isProxy false, write dir)
            Action<string, Action<EndianWriter, Object, DirectoryMeta, Entry>> AddSimpleDirEntryWriter = (typeName, objWriter) =>
            {
                EntryWriteActions[typeName] = (writer, meta, entry) =>
                {
                    objWriter(writer, entry.obj, meta, entry);
                    entry.dir.Write(writer);
                };
            };

            // Simple directory entry writers
            AddSimpleDirEntryWriter("BandScoreboard", (w, o, m, e) => ((BandScoreboard)o).Write(w, true, m, e));
            AddSimpleDirEntryWriter("BandCrowdMeterDir", (w, o, m, e) => ((BandCrowdMeterDir)o).Write(w, true, m, e));
            AddSimpleDirEntryWriter("BandStarDisplay", (w, o, m, e) => ((BandStarDisplay)o).Write(w, true, m, e));
            AddSimpleDirEntryWriter("CharBoneDir", (w, o, m, e) => ((CharBoneDir)o).Write(w, true, m, e));
            AddSimpleDirEntryWriter("CompositeCharacter", (w, o, m, e) => ((CompositeCharacter)o).Write(w, true, m, e));
            AddSimpleDirEntryWriter("CrowdMeterIcon", (w, o, m, e) => ((BandCrowdMeterIcon)o).Write(w, true, m, e));
            AddSimpleDirEntryWriter("EndingBonusDir", (w, o, m, e) => ((RndDir)o).Write(w, true, m, e));
            AddSimpleDirEntryWriter("GemTrackDir", (w, o, m, e) => ((GemTrackDir)o).Write(w, true, m, e));
            AddSimpleDirEntryWriter("OverdriveMeterDir", (w, o, m, e) => ((OverdriveMeterDir)o).Write(w, true, m, e));
            AddSimpleDirEntryWriter("OvershellDir", (w, o, m, e) => ((OvershellDir)o).Write(w, true, m, e));
            AddSimpleDirEntryWriter("P9Character", (w, o, m, e) => ((P9Character)o).Write(w, true, m, e));
            AddSimpleDirEntryWriter("PanelDir", (w, o, m, e) => ((PanelDir)o).Write(w, true, m, e));
            AddSimpleDirEntryWriter("PitchArrowDir", (w, o, m, e) => ((PitchArrowDir)o).Write(w, true, m, e));
            AddSimpleDirEntryWriter("SkeletonDir", (w, o, m, e) => ((SkeletonDir)o).Write(w, true, m, e));
            AddSimpleDirEntryWriter("StreakMeterDir", (w, o, m, e) => ((StreakMeterDir)o).Write(w, true, m, e));
            AddSimpleDirEntryWriter("SynthDir", (w, o, m, e) => ((SynthDir)o).Write(w, true, m, e));
            AddSimpleDirEntryWriter("TrackDir", (w, o, m, e) => ((TrackDir)o).Write(w, true, m, e));
            AddSimpleDirEntryWriter("TrackPanelDir", (w, o, m, e) => ((TrackPanelDir)o).Write(w, true, m, e));
            AddSimpleDirEntryWriter("UILabelDir", (w, o, m, e) => ((UILabelDir)o).Write(w, true, m, e));
            AddSimpleDirEntryWriter("UIListDir", (w, o, m, e) => ((UIListDir)o).Write(w, true, m, e));
            AddSimpleDirEntryWriter("UnisonIcon", (w, o, m, e) => ((UnisonIcon)o).Write(w, true, m, e));
            AddSimpleDirEntryWriter("VocalTrackDir", (w, o, m, e) => ((VocalTrackDir)o).Write(w, true, m, e));
            AddSimpleDirEntryWriter("WorldDir", (w, o, m, e) => ((WorldDir)o).Write(w, true, m, e));
            AddSimpleDirEntryWriter("BandCharacter", (w, o, m, e) => ((BandCharacter)o).Write(w, true, m, e));

            // Character - conditional dir write based on proxyPath
            EntryWriteActions["Character"] = (writer, meta, entry) =>
            {
                ((Character)entry.obj).Write(writer, true, meta, entry);
                if (((Character)entry.obj).proxyPath != String.Empty)
                {
                    entry.dir.Write(writer);
                }
            };

            // CharClipSet - write as CharClipSet (includes PostLoad fields for non-proxy)
            EntryWriteActions["CharClipSet"] = (writer, meta, entry) =>
            {
                ((CharClipSet)entry.obj).Write(writer, true, meta, entry);
                if (entry.dir != null)
                {
                    entry.dir.Write(writer);
                }
            };

            // MoveDir - conditional dir write
            EntryWriteActions["MoveDir"] = (writer, meta, entry) =>
            {
                ((MoveDir)entry.obj).Write(writer, true, meta, entry);
                if (entry.dir != null)
                {
                    entry.dir.Write(writer);
                }
            };

            // ObjectDir - conditional dir write
            EntryWriteActions["ObjectDir"] = (writer, meta, entry) =>
            {
                ((ObjectDir)entry.obj).Write(writer, true, meta, entry);
                if (entry.dir != null)
                {
                    entry.dir.Write(writer);
                }
            };

            // RndDir - conditional dir write
            EntryWriteActions["RndDir"] = (writer, meta, entry) =>
            {
                ((RndDir)entry.obj).Write(writer, true, meta, entry);
                if (entry.dir != null)
                {
                    entry.dir.Write(writer);
                }
            };

            // WorldInstance - fuck
            EntryWriteActions["WorldInstance"] = (writer, meta, entry) =>
            {
                ((WorldInstance)entry.obj).Write(writer, false, meta, entry);

                if (((WorldInstance)entry.obj).revision == 0)
                {
                    writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
                    return;
                }

                if (!((WorldInstance)entry.obj).hasPersistentObjects)
                {
                    entry.dir.Write(writer);

                    if (entry.dir.type.value == "WorldInstance" && ((WorldInstance)entry.dir.directory).hasPersistentObjects)
                    {
                        ((WorldInstance)entry.dir.directory).persistentObjects.Write(writer, meta, entry, ((WorldInstance)entry.dir.directory).revision);
                    }
                    else
                    {
                        ((WorldInstance)entry.obj).persistentObjects.Write(writer, meta, entry, ((WorldInstance)entry.obj).revision);
                    }
                }
                else
                {
                    ((WorldInstance)entry.obj).persistentObjects.Write(writer, meta, entry, ((WorldInstance)entry.obj).revision);
                }
            };
        }

        public class Entry
        {
            public class EntryOperationEventArgs : EventArgs
            {
                public EndianReader? Reader { get; }
                public EndianWriter? Writer { get; }
                public Entry Entry { get; }


                public EntryOperationEventArgs(EndianReader reader, Entry entry)
                {
                    Reader = reader;
                    Entry = entry;
                }
                public EntryOperationEventArgs(EndianWriter writer, Entry entry)
                {
                    Writer = writer;
                    Entry = entry;
                }

            }

            /// <summary>
            /// The type of the object, i.e. "Object", "RndDir", etc.
            /// </summary>
            public Symbol type = new(0, "");

            /// <summary>
            /// The name of the object, i.e. "uniq0", "NewRndDir", "palette.pal", etc.
            /// </summary>
            public Symbol name = new(0, "");

            /// <summary>
            /// The object itself, can be a directory or an asset
            /// </summary>
            public Object obj;

            /// <summary>
            /// The directory of the object, if it's a directory (confusing, indeed)
            /// </summary>
            public DirectoryMeta? dir;

            /// <summary>
            /// Whether or not the entry is an entry inside the root directory (aka proxied). Used to handle writing directories that are entries inside of another directory and not inlined subdirectories.
            /// Non-directories do not have the concept of being proxied so it will always be false for objects that are not directories.
            /// </summary>
            public bool isProxy;

            /// <summary>
            /// Set when the object has been added or otherwise created through a non-serialized fashion (i.e. as raw bytes)
            /// </summary>
            public bool dirty = false;

            /// <summary>
            /// Whether or not the type is recognized by MiloLib.
            /// Used to handle duplicating and other actions on assets that aren't yet supported.
            /// </summary>
            public bool typeRecognized = true;

            /// <summary>
            /// The raw bytes of the object. If we can't deserialize/serialize a particular type yet, we just read and write this directly.
            /// Also used to enable extracting assets.
            /// </summary>
            public List<byte> objBytes = new List<byte>();

            public Entry(Symbol type, Symbol name, Object obj)
            {
                this.type = type;
                this.name = name;
                this.obj = obj;
            }

            public static Entry CreateDirtyAssetFromBytes(string type, string name, List<byte> bytes)
            {
                Entry entry = new Entry(new Symbol(0, ""), new Symbol(0, ""), null);
                entry.type = type;
                entry.name = name;
                entry.objBytes = bytes;
                entry.dirty = true;
                return entry;
            }

            public event EventHandler<EntryOperationEventArgs> BeforeRead;
            public event EventHandler<EntryOperationEventArgs> AfterRead;
            public event EventHandler<EntryOperationEventArgs> BeforeWrite;
            public event EventHandler<EntryOperationEventArgs> AfterWrite;


            internal void OnBeforeRead(EndianReader reader)
            {
                BeforeRead?.Invoke(this, new EntryOperationEventArgs(reader, this));
            }

            internal void OnAfterRead(EndianReader reader)
            {
                AfterRead?.Invoke(this, new EntryOperationEventArgs(reader, this));
            }

            internal void OnBeforeWrite(EndianWriter writer)
            {
                BeforeWrite?.Invoke(this, new EntryOperationEventArgs(writer, this));
            }

            internal void OnAfterWrite(EndianWriter writer)
            {
                AfterWrite?.Invoke(this, new EntryOperationEventArgs(writer, this));
            }
        }

        /// <summary>
        /// The revision of the Milo scene.
        /// </summary>
        public uint revision;

        /// <summary>
        /// The type of the directory, i.e. "ObjectDir", "RndDir", etc.
        /// </summary>
        public Symbol type = new(0, "");

        /// <summary>
        /// The name of the directory, i.e. "uniq0", "NewRndDir", etc.
        /// </summary>
        public Symbol name = new(0, "");

        /// <summary>
        /// The amount of strings in the string table. Usually calculated as (numEntries * 2) + 2.
        /// </summary>
        private uint stringTableCount;

        /// <summary>
        /// The size of the string table. Not sure how this is calculated, but the game will fix it itself if it's not right.
        /// </summary>
        private uint stringTableSize;

        private uint externalResourceCount;

        /// <summary>
        /// The external resources of the directory. Only used in GH1-era scenes.
        /// </summary>
        public List<Symbol> externalResources = new List<Symbol>();

        private uint entryCount;

        /// <summary>
        /// The entries of the directory.
        /// </summary>
        public List<Entry> entries = new List<Entry>();

        public Object directory;

        /// <summary>
        /// The platform the Milo scene is for. Determines certain platform specific things (texture swapping on Xbox 360, etc.)
        /// </summary>
        public Platform platform = Platform.PS3;

        public List<byte> DirObjBytes { get; set; } = new List<byte>();

        public DirectoryMeta Read(EndianReader reader)
        {
            revision = reader.ReadUInt32();

            // if the revision is over 50, switch to little endian and attempt the read again to guess endianness
            // this works well since the highest known revision before switch to Forge was 32
            if (revision > 50)
            {
                reader.Endianness = Endian.LittleEndian;
                reader.BaseStream.Position -= 4;
                revision = reader.ReadUInt32();
            }

            // support freq<-->dc3 versions
            if (revision != 6 && revision != 10 && revision != 24 && revision != 25 && revision != 26 && revision != 28 && revision != 32)
            {
                throw new UnsupportedMiloSceneRevision(revision);
            }

            if (revision > 10)
            {
                type = Symbol.Read(reader);
                name = Symbol.Read(reader);

                stringTableCount = reader.ReadUInt32();
                stringTableSize = reader.ReadUInt32();

                if (revision >= 32)
                {
                    reader.ReadBoolean();
                }
            }

            entryCount = reader.ReadUInt32();

            for (int i = 0; i < entryCount; i++)
            {
                Entry entry = new Entry(Symbol.Read(reader), Symbol.Read(reader), null);
                entries.Add(entry);
            }

            // only gh1-era stuff seems to have this
            if (revision == 10)
            {
                externalResourceCount = reader.ReadUInt32();
                for (int i = 0; i < externalResourceCount; i++)
                {
                    externalResources.Add(Symbol.Read(reader));
                }
            }

            long dirDataStart = reader.BaseStream.Position;

            if (type.value == "")
            {
                Debug.WriteLine("GH1-style empty directory detected, just reading children");
            }
            else if (DirectoryFactories.TryGetValue(type.value, out var factory) &&
                     DirectoryReaders.TryGetValue(type.value, out var readerFunc))
            {
                Debug.WriteLine($"Reading {type.value} {name.value}");
                directory = factory(0);
                readerFunc(reader, directory, this, new Entry(type, name, directory));
            }
            else
            {
                throw new Exception("Unknown directory type: " + type.value + ", cannot continue reading Milo scene");
            }

            long dirDataEnd = reader.BaseStream.Position;
            if (dirDataEnd > dirDataStart)
            {
                reader.BaseStream.Position = dirDataStart;
                this.DirObjBytes = reader.ReadBlock((int)(dirDataEnd - dirDataStart)).ToList();
            }

            foreach (Entry entry in entries)
            {
                long startPos = reader.BaseStream.Position;

                while (true)
                {
                    byte b = reader.ReadByte();
                    if (b == 0xAD)
                    {
                        long currentPos = reader.BaseStream.Position;

                        if (reader.ReadByte() == 0xDE &&
                            reader.ReadByte() == 0xAD &&
                            reader.ReadByte() == 0xDE)
                        {
                            break;
                        }

                        reader.BaseStream.Position = currentPos;
                    }

                    entry.objBytes.Add(b);
                }

                reader.BaseStream.Position = startPos;

                ReadEntry(reader, entry);

                if (entry.dir != null)
                    entry.dir.platform = platform;
            }

            return this;
        }

        public void Write(EndianWriter writer)
        {
            writer.WriteUInt32(revision);

            // Only write type, name, and string table info if revision > 10
            if (revision > 10)
            {
                Symbol.Write(writer, type);
                Symbol.Write(writer, name);

                writer.WriteInt32((entries.Count * 2) + 4);
                writer.WriteUInt32(stringTableSize);

                if (revision >= 32)
                {
                    writer.WriteBoolean(false);
                }
            }

            writer.WriteInt32((int)entries.Count);

            foreach (Entry entry in entries)
            {
                Symbol.Write(writer, entry.type);
                Symbol.Write(writer, entry.name);
            }

            // only gh1-era stuff seems to have this
            if (revision == 10)
            {
                writer.WriteUInt32((uint)externalResources.Count);
                foreach (var externalResource in externalResources)
                {
                    Symbol.Write(writer, externalResource);
                }
            }

            // Handle GH1-style empty directory types (no root directory object)
            if (type.value == "")
            {
                Debug.WriteLine("GH1-style empty directory detected, skipping directory object write");
            }
            // Use dictionary-based factory pattern for O(1) lookup
            else if (DirectoryWriters.TryGetValue(type.value, out var writerFunc))
            {
                writerFunc(writer, directory, this, new Entry(type, name, directory));
            }
            else
            {
                throw new Exception("Unknown directory type: " + type.value + ", cannot continue writing Milo scene");
            }

            // write the children entries
            foreach (Entry entry in entries)
            {
                WriteEntry(writer, entry);
            }
        }

        public void ReadEntry(EndianReader reader, DirectoryMeta.Entry entry)
        {
            entry.OnBeforeRead(reader);

            try
            {
                if (EntryReadActions.TryGetValue(entry.type.value, out var readAction))
                {
                    readAction(reader, this, entry);
                }
                else
                {
                    // Unknown entry type - read until we see 0xADDEADDE to skip over it
                    Debug.WriteLine("Unknown entry type " + entry.type.value + " of name " + entry.name.value + ", read an Object and then read until we see 0xADDEADDE to skip over it, curpos" + reader.BaseStream.Position);

                    entry.typeRecognized = false;

                    // TODO: improve this shit
                    while (true)
                    {
                        byte b = reader.ReadByte();
                        if (b == 0xAD)
                        {
                            byte b2 = reader.ReadByte();
                            if (b2 == 0xDE)
                            {
                                byte b3 = reader.ReadByte();
                                if (b3 == 0xAD)
                                {
                                    byte b4 = reader.ReadByte();
                                    if (b4 == 0xDE)
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    Debug.WriteLine("Found ending of file, new position: " + reader.BaseStream.Position);
                }
            }
            catch (Exception ex)
            {
                // Wrap any exception with context about which asset failed
                throw MiloAssetReadException.WrapException(ex, this, entry, reader.BaseStream.Position);
            }

            entry.OnAfterRead(reader);
        }

        public void WriteEntry(EndianWriter writer, DirectoryMeta.Entry entry)
        {
            // handle dirty assets

            entry.OnBeforeWrite(writer);

            if (entry.dirty)
            {
                writer.WriteBlock(entry.objBytes.ToArray());
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
                return;
            }

            Debug.WriteLine("Writing " + entry.type.value + " " + entry.name.value);

            // Use dictionary-based factory pattern for O(1) lookup
            if (EntryWriteActions.TryGetValue(entry.type.value, out var writeAction))
            {
                writeAction(writer, this, entry);
            }
            else
            {
                // Unknown entry type - check if it's a directory type (which would be a problem)
                if (entry.type.value.Contains("Dir"))
                    throw new Exception("Trying to write an unsupported dir entry of type: " + entry.type.value + ", this Milo cannot be saved");

                Debug.WriteLine("Unknown entry type, dumping raw bytes for " + entry.type.value + " of name " + entry.name.value);

                // this should allow saving Milos with types that have yet to be implemented
                writer.WriteBlock(entry.objBytes.ToArray());

                // write the ending bytes
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
            }

            entry.OnAfterWrite(writer);
        }

        public static DirectoryMeta New(string type, string name, uint sceneRevision, ushort rootDirRevision)
        {
            DirectoryMeta dir = new DirectoryMeta();
            dir.type = type;
            dir.name = name;

            // Use dictionary-based factory pattern for O(1) lookup
            if (DirectoryFactories.TryGetValue(type, out var factory))
            {
                dir.directory = factory(rootDirRevision);
            }
            else
            {
                throw new Exception("Unknown directory type: " + type + ", cannot continue creating directory");
            }

            dir.entries = new List<Entry>();
            dir.revision = sceneRevision;
            return dir;
        }

        public override string ToString()
        {
            return string.Format("{1} ({0})", type, name);
        }
    }
}
