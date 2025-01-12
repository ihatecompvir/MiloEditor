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
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using static MiloLib.Assets.DirectoryMeta;

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
            PC_or_iPod
        }
        public class Entry
        {
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

            // figure out how to read this directory depending on the type
            switch (type.value)
            {
                case "ObjectDir":
                    Debug.WriteLine("Reading ObjectDir " + name.value);
                    ObjectDir objectDir = new ObjectDir(0);
                    objectDir.Read(reader, true, this, new Entry(type, name, objectDir));
                    directory = objectDir;
                    break;
                case "EndingBonusDir":
                case "RndDir":
                    Debug.WriteLine("Reading RndDir " + name.value);
                    RndDir rndDir = new RndDir(0);
                    rndDir.Read(reader, true, this, new Entry(type, name, rndDir));
                    directory = rndDir;
                    break;
                case "PanelDir":
                    Debug.WriteLine("Reading PanelDir " + name.value);
                    PanelDir panelDir = new PanelDir(0);
                    panelDir.Read(reader, true, this, new Entry(type, name, panelDir));
                    directory = panelDir;
                    break;
                case "CharClipSet":
                    Debug.WriteLine("Reading CharClipSet " + name.value);
                    CharClipSet charClipSet = new CharClipSet(0);
                    charClipSet.Read(reader, true, this, new Entry(type, name, charClipSet));
                    directory = charClipSet;
                    break;
                case "WorldDir":
                    Debug.WriteLine("Reading WorldDir " + name.value);
                    WorldDir worldDir = new WorldDir(0);
                    worldDir.Read(reader, true, this, new Entry(type, name, worldDir));
                    directory = worldDir;
                    break;
                case "Character":
                    Debug.WriteLine("Reading Character " + name.value);
                    Character character = new Character(0);
                    character.Read(reader, true, this, new Entry(type, name, character));
                    directory = character;
                    break;
                case "UILabelDir":
                    Debug.WriteLine("Reading UILabelDir " + name.value);
                    UILabelDir uiLabelDir = new UILabelDir(0);
                    uiLabelDir.Read(reader, true, this, new Entry(type, name, uiLabelDir));
                    directory = uiLabelDir;
                    break;
                case "UIListDir":
                    Debug.WriteLine("Reading UIListDir " + name.value);
                    UIListDir uiListDir = new UIListDir(0);
                    uiListDir.Read(reader, true, this, new Entry(type, name, uiListDir));
                    directory = uiListDir;
                    break;
                case "BandCrowdMeterDir":
                    Debug.WriteLine("Reading BandCrowdMeterDir " + name.value);
                    BandCrowdMeterDir bandCrowdMeterDir = new BandCrowdMeterDir(0);
                    bandCrowdMeterDir.Read(reader, true, this, new Entry(type, name, bandCrowdMeterDir));
                    directory = bandCrowdMeterDir;
                    break;
                case "CrowdMeterIcon":
                    Debug.WriteLine("Reading CrowdMeterIcon " + name.value);
                    BandCrowdMeterIcon crowdMeterIcon = new BandCrowdMeterIcon(0);
                    crowdMeterIcon.Read(reader, true, this, new Entry(type, name, crowdMeterIcon));
                    directory = crowdMeterIcon;
                    break;
                case "CharBoneDir":
                    Debug.WriteLine("Reading CharBoneDir " + name.value);
                    CharBoneDir charBoneDir = new CharBoneDir(0);
                    charBoneDir.Read(reader, true, this, new Entry(type, name, charBoneDir));
                    directory = charBoneDir;
                    break;
                case "BandCharacter":
                    Debug.WriteLine("Reading BandCharacter " + name.value);
                    BandCharacter bandCharacter = new BandCharacter(0);
                    bandCharacter.Read(reader, true, this, new Entry(type, name, bandCharacter));
                    directory = bandCharacter;
                    break;
                case "WorldInstance":
                    Debug.WriteLine("Reading WorldInstance " + name.value);
                    WorldInstance worldInstance = new WorldInstance(0);
                    worldInstance.Read(reader, true, this, new Entry(type, name, worldInstance));
                    directory = worldInstance;
                    break;
                case "GemTrackDir":
                    Debug.WriteLine("Reading GemTrackDir " + name.value);
                    GemTrackDir gemTrackDir = new GemTrackDir(0);
                    gemTrackDir.Read(reader, true, this, new Entry(type, name, gemTrackDir));
                    directory = gemTrackDir;
                    break;
                case "TrackPanelDir":
                    Debug.WriteLine("Reading TrackPanelDir " + name.value);
                    TrackPanelDir trackPanelDir = new TrackPanelDir(0);
                    trackPanelDir.Read(reader, true, this, new Entry(type, name, trackPanelDir));
                    directory = trackPanelDir;
                    break;
                case "UnisonIcon":
                    Debug.WriteLine("Reading UnisonIcon " + name.value);
                    UnisonIcon unisonIcon = new UnisonIcon(0);
                    unisonIcon.Read(reader, true, this, new Entry(type, name, unisonIcon));
                    directory = unisonIcon;
                    break;
                case "BandScoreboard":
                    Debug.WriteLine("Reading BandScoreboard " + name.value);
                    BandScoreboard bandScoreboard = new BandScoreboard(0);
                    bandScoreboard.Read(reader, true, this, new Entry(type, name, bandScoreboard));
                    directory = bandScoreboard;
                    break;
                case "BandStarDisplay":
                    Debug.WriteLine("Reading BandStarDisplay " + name.value);
                    BandStarDisplay bandStarDisplay = new BandStarDisplay(0);
                    bandStarDisplay.Read(reader, true, this, new Entry(type, name, bandStarDisplay));
                    directory = bandStarDisplay;
                    break;
                case "VocalTrackDir":
                    Debug.WriteLine("Reading VocalTrackDir " + name.value);
                    VocalTrackDir vocalTrackDir = new VocalTrackDir(0);
                    vocalTrackDir.Read(reader, true, this, new Entry(type, name, vocalTrackDir));
                    directory = vocalTrackDir;
                    break;
                case "MoveDir":
                    Debug.WriteLine("Reading MoveDir " + name.value);
                    MoveDir moveDir = new MoveDir(0);
                    moveDir.Read(reader, true, this, new Entry(type, name, moveDir));
                    directory = moveDir;
                    break;
                case "SkeletonDir":
                    Debug.WriteLine("Reading SkeletonDir " + name.value);
                    SkeletonDir skeletonDir = new SkeletonDir(0);
                    skeletonDir.Read(reader, true, this, new Entry(type, name, skeletonDir));
                    directory = skeletonDir;
                    break;
                case "OvershellDir":
                    Debug.WriteLine("Reading OvershellDir " + name.value);
                    OvershellDir overshellDir = new OvershellDir(0);
                    overshellDir.Read(reader, true, this, new Entry(type, name, overshellDir));
                    directory = overshellDir;
                    break;
                case "OverdriveMeterDir":
                    Debug.WriteLine("Reading OverdriveMeterDir " + name.value);
                    OverdriveMeterDir overdriveMeterDir = new OverdriveMeterDir(0);
                    overdriveMeterDir.Read(reader, true, this, new Entry(type, name, overdriveMeterDir));
                    directory = overdriveMeterDir;
                    break;
                case "StreakMeterDir":
                    Debug.WriteLine("Reading StreakMeterDir " + name.value);
                    StreakMeterDir streakMeterDir = new StreakMeterDir(0);
                    streakMeterDir.Read(reader, true, this, new Entry(type, name, streakMeterDir));
                    directory = streakMeterDir;
                    break;
                case "PitchArrowDir":
                    Debug.WriteLine("Reading PitchArrowDir " + name.value);
                    PitchArrowDir pitchArrowDir = new PitchArrowDir(0);
                    pitchArrowDir.Read(reader, true, this, new Entry(type, name, pitchArrowDir));
                    directory = pitchArrowDir;
                    break;
                case "SynthDir":
                    Debug.WriteLine("Reading SynthDir " + name.value);
                    SynthDir synthDir = new SynthDir(0);
                    synthDir.Read(reader, true, this, new Entry(type, name, synthDir));
                    directory = synthDir;
                    break;
                case "":
                    Debug.WriteLine("GH1-style empty directory detected, just reading children");
                    break;
                default:
                    throw new Exception("Unknown directory type: " + type.value + ", cannot continue reading Milo scene");
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

            Symbol.Write(writer, type);
            Symbol.Write(writer, name);

            writer.WriteInt32((entries.Count * 2) + 4);
            writer.WriteUInt32(stringTableSize);

            writer.WriteInt32((int)entries.Count);

            foreach (Entry entry in entries)
            {
                Symbol.Write(writer, entry.type);
                Symbol.Write(writer, entry.name);
            }

            switch (type.value)
            {
                case "ObjectDir":
                    ((ObjectDir)directory).Write(writer, true, this, new Entry(type, name, directory));
                    break;
                case "RndDir":
                    ((RndDir)directory).Write(writer, true, this, new Entry(type, name, directory));
                    break;
                case "PanelDir":
                    ((PanelDir)directory).Write(writer, true, this, new Entry(type, name, directory));
                    break;
                case "WorldDir":
                    ((WorldDir)directory).Write(writer, true, this, new Entry(type, name, directory));
                    break;
                case "Character":
                    ((Character)directory).Write(writer, true, this, new Entry(type, name, directory));
                    break;
                case "P9Character":
                    ((P9Character)directory).Write(writer, true, this, new Entry(type, name, directory));
                    break;
                case "CharClipSet":
                    ((CharClipSet)directory).Write(writer, true, this, new Entry(type, name, directory));
                    break;
                case "UILabelDir":
                    ((UILabelDir)directory).Write(writer, true, this, new Entry(type, name, directory));
                    break;
                case "UIListDir":
                    ((UIListDir)directory).Write(writer, true, this, new Entry(type, name, directory));
                    break;
                //case "GemTrackDir":
                //    ((GemTrackDir)directory).Write(writer, true, this, new Entry(type, name, directory));
                //    break;
                case "BandCrowdMeterDir":
                    ((BandCrowdMeterDir)directory).Write(writer, true, this, new Entry(type, name, directory));
                    break;
                case "CrowdMeterIcon":
                    ((BandCrowdMeterIcon)directory).Write(writer, true, this, new Entry(type, name, directory));
                    break;
                case "BandCharacter":
                    ((BandCharacter)directory).Write(writer, true, this, new Entry(type, name, directory));
                    break;
                case "WorldInstance":
                    ((WorldInstance)directory).Write(writer, true, this, new Entry(type, name, directory));
                    break;
                case "TrackDir":
                    ((TrackDir)directory).Write(writer, true, this, new Entry(type, name, directory));
                    break;
                case "GemTrackDir":
                    ((GemTrackDir)directory).Write(writer, true, this, new Entry(type, name, directory));
                    break;
                case "TrackPanelDir":
                    ((TrackPanelDir)directory).Write(writer, true, this, new Entry(type, name, directory));
                    break;
                case "UnisonIcon":
                    ((UnisonIcon)directory).Write(writer, true, this, new Entry(type, name, directory));
                    break;
                case "EndingBonusDir":
                    ((RndDir)directory).Write(writer, true, this, new Entry(type, name, directory));
                    break;
                case "BandStarDisplay":
                    ((BandStarDisplay)directory).Write(writer, true, this, new Entry(type, name, directory));
                    break;
                case "BandScoreboard":
                    ((BandScoreboard)directory).Write(writer, true, this, new Entry(type, name, directory));
                    break;
                case "VocalTrackDir":
                    ((VocalTrackDir)directory).Write(writer, true, this, new Entry(type, name, directory));
                    break;
                case "MoveDir":
                    ((MoveDir)directory).Write(writer, true, this, new Entry(type, name, directory));
                    break;
                case "SkeletonDir":
                    ((SkeletonDir)directory).Write(writer, true, this, new Entry(type, name, directory));
                    break;
                case "PitchArrowDir":
                    ((PitchArrowDir)directory).Write(writer, true, this, new Entry(type, name, directory));
                    break;
                case "OvershellDir":
                    ((OvershellDir)directory).Write(writer, true, this, new Entry(type, name, directory));
                    break;
                case "OverdriveMeterDir":
                    ((OverdriveMeterDir)directory).Write(writer, true, this, new Entry(type, name, directory));
                    break;
                case "StreakMeterDir":
                    ((StreakMeterDir)directory).Write(writer, true, this, new Entry(type, name, directory));
                    break;
                case "SynthDir":
                    ((SynthDir)directory).Write(writer, true, this, new Entry(type, name, directory));
                    break;
                //case "VocalTrackDir":
                //    ((VocalTrackDir)directory).Write(writer, true, this, new Entry(type, name, directory));
                //    break;
                default:
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
            switch (entry.type.value)
            {
                //////////
                // DIRS //
                //////////
                case "BandCharacter":
                    Debug.WriteLine("Reading entry BandCharacter " + entry.name.value);
                    entry.isProxy = true;
                    entry.obj = new BandCharacter(0).Read(reader, true, this, entry);

                    DirectoryMeta dir = new DirectoryMeta();
                    dir.platform = platform;
                    dir.Read(reader);
                    entry.dir = dir;
                    break;
                case "BandCrowdMeterDir":
                    Debug.WriteLine("Reading entry BandCrowdMeterDir " + entry.name.value);
                    entry.isProxy = true;
                    entry.obj = new BandCrowdMeterDir(0).Read(reader, true, this, entry);

                    dir = new DirectoryMeta();
                    dir.platform = platform;
                    dir.Read(reader);
                    entry.dir = dir;
                    break;
                case "CrowdMeterIcon":
                    Debug.WriteLine("Reading entry CrowdMeterIcon " + entry.name.value);
                    entry.isProxy = true;
                    entry.obj = new BandCrowdMeterIcon(0).Read(reader, true, this, entry);

                    dir = new DirectoryMeta();
                    dir.platform = platform;
                    dir.Read(reader);
                    entry.dir = dir;
                    break;
                case "BandScoreboard":
                    Debug.WriteLine("Reading entry BandScoreboard " + entry.name.value);
                    entry.isProxy = true;
                    entry.obj = new BandScoreboard(0).Read(reader, true, this, entry);

                    dir = new DirectoryMeta();
                    dir.platform = platform;
                    dir.Read(reader);
                    entry.dir = dir;
                    break;
                case "BandStarDisplay":
                    Debug.WriteLine("Reading entry BandStarDisplay " + entry.name.value);
                    entry.isProxy = true;
                    entry.obj = new BandStarDisplay(0).Read(reader, true, this, entry);

                    dir = new DirectoryMeta();
                    dir.platform = platform;
                    dir.Read(reader);
                    entry.dir = dir;
                    break;
                case "Character":
                    Debug.WriteLine("Reading entry Character " + entry.name.value);
                    entry.isProxy = true;
                    entry.obj = new Character(0).Read(reader, true, this, entry);

                    if (((Character)entry.obj).proxyPath != String.Empty)
                    {
                        dir = new DirectoryMeta();
                        dir.platform = platform;
                        dir.Read(reader);
                        entry.dir = dir;
                    }

                    break;
                case "CharBoneDir":
                    Debug.WriteLine("Reading entry CharBoneDir " + entry.name.value);
                    entry.isProxy = true;
                    entry.obj = new CharBoneDir(0).Read(reader, true, this, entry);

                    dir = new DirectoryMeta();
                    dir.platform = platform;
                    dir.Read(reader);
                    entry.dir = dir;
                    break;
                case "CharClipSet":
                    Debug.WriteLine("Reading entry CharClipSet " + entry.name.value);
                    entry.isProxy = true;

                    // this is unhinged, why'd they do it like this?
                    reader.ReadUInt32();
                    entry.obj = new ObjectDir(0).Read(reader, true, this, entry);

                    dir = new DirectoryMeta();
                    dir.platform = platform;
                    dir.Read(reader);
                    entry.dir = dir;
                    break;
                case "EndingBonusDir":
                case "RndDir":
                    Debug.WriteLine("Reading entry RndDir " + entry.name.value);
                    entry.isProxy = true;
                    entry.obj = new RndDir(0).Read(reader, true, this, entry);

                    if (((ObjectDir)entry.obj).inlineProxy && ((ObjectDir)entry.obj).proxyPath.value != "")
                    {
                        dir = new DirectoryMeta();
                        dir.platform = platform;
                        dir.Read(reader);
                        entry.dir = dir;
                    }
                    break;
                case "GemTrackDir":
                    Debug.WriteLine("Reading entry GemTrackDir " + entry.name.value);
                    entry.isProxy = true;
                    entry.obj = new GemTrackDir(0).Read(reader, true, this, entry);

                    dir = new DirectoryMeta();
                    dir.platform = platform;
                    dir.Read(reader);
                    entry.dir = dir;
                    break;
                case "MoveDir":
                    Debug.WriteLine("Reading entry MoveDir " + entry.name.value);
                    entry.isProxy = true;
                    entry.obj = new MoveDir(0).Read(reader, true, this, entry);

                    dir = new DirectoryMeta();
                    dir.platform = platform;
                    dir.Read(reader);
                    entry.dir = dir;
                    break;
                case "ObjectDir":
                    Debug.WriteLine("Reading entry ObjectDir " + entry.name.value);
                    entry.isProxy = true;
                    entry.obj = new ObjectDir(0).Read(reader, true, this, entry);

                    dir = new DirectoryMeta();
                    dir.platform = platform;
                    dir.Read(reader);
                    entry.dir = dir;
                    break;
                case "OverdriveMeterDir":
                    Debug.WriteLine("Reading entry OverdriveMeterDir " + entry.name.value);
                    entry.isProxy = true;
                    entry.obj = new OverdriveMeterDir(0).Read(reader, true, this, entry);

                    dir = new DirectoryMeta();
                    dir.platform = platform;
                    dir.Read(reader);
                    entry.dir = dir;
                    break;
                case "OvershellDir":
                    Debug.WriteLine("Reading entry OvershellDir " + entry.name.value);
                    entry.isProxy = true;
                    entry.obj = new OvershellDir(0).Read(reader, true, this, entry);

                    dir = new DirectoryMeta();
                    dir.platform = platform;
                    dir.Read(reader);
                    entry.dir = dir;
                    break;
                case "P9Character":
                    Debug.WriteLine("Reading entry P9Character " + entry.name.value);
                    entry.isProxy = true;
                    entry.obj = new P9Character(0).Read(reader, true, this, entry);

                    dir = new DirectoryMeta();
                    dir.platform = platform;
                    dir.Read(reader);
                    entry.dir = dir;
                    break;
                case "PitchArrowDir":
                    Debug.WriteLine("Reading entry PitchArrowDir " + entry.name.value);
                    entry.isProxy = true;
                    entry.obj = new PitchArrowDir(0).Read(reader, true, this, entry);

                    dir = new DirectoryMeta();
                    dir.platform = platform;
                    dir.Read(reader);
                    entry.dir = dir;
                    break;
                case "PanelDir":
                case "UIPanel":
                    Debug.WriteLine("Reading entry PanelDir " + entry.name.value);
                    entry.isProxy = true;
                    entry.obj = new PanelDir(0).Read(reader, true, this, entry);

                    dir = new DirectoryMeta();
                    dir.platform = platform;
                    dir.Read(reader);
                    entry.dir = dir;
                    break;
                case "SkeletonDir":
                    Debug.WriteLine("Reading entry SkeletonDir " + entry.name.value);
                    entry.isProxy = true;
                    entry.obj = new SkeletonDir(0).Read(reader, true, this, entry);

                    dir = new DirectoryMeta();
                    dir.platform = platform;
                    dir.Read(reader);
                    entry.dir = dir;
                    break;
                case "StreakMeterDir":
                    Debug.WriteLine("Reading entry StreakMeterDir " + entry.name.value);
                    entry.isProxy = true;
                    entry.obj = new StreakMeterDir(0).Read(reader, true, this, entry);

                    dir = new DirectoryMeta();
                    dir.platform = platform;
                    dir.Read(reader);
                    entry.dir = dir;
                    break;
                case "SynthDir":
                    Debug.WriteLine("Reading entry SynthDir " + entry.name.value);
                    entry.isProxy = true;
                    entry.obj = new SynthDir(0).Read(reader, true, this, entry);

                    dir = new DirectoryMeta();
                    dir.platform = platform;
                    dir.Read(reader);
                    entry.dir = dir;
                    break;
                case "TrackDir":
                    Debug.WriteLine("Reading entry TrackDir " + entry.name.value);
                    entry.isProxy = true;
                    entry.obj = new TrackDir(0).Read(reader, true, this, entry);

                    dir = new DirectoryMeta();
                    dir.platform = platform;
                    dir.Read(reader);
                    entry.dir = dir;
                    break;
                case "TrackPanelDir":
                    Debug.WriteLine("Reading entry TrackPanelDir " + entry.name.value);
                    entry.isProxy = true;
                    entry.obj = new TrackPanelDir(0).Read(reader, true, this, entry);

                    dir = new DirectoryMeta();
                    dir.platform = platform;
                    dir.Read(reader);
                    entry.dir = dir;
                    break;
                case "UILabelDir":
                    Debug.WriteLine("Reading entry UILabelDir " + entry.name.value);
                    entry.isProxy = true;
                    entry.obj = new UILabelDir(0).Read(reader, true, this, entry);

                    dir = new DirectoryMeta();
                    dir.platform = platform;
                    dir.Read(reader);
                    entry.dir = dir;
                    break;
                case "UIListDir":
                    Debug.WriteLine("Reading entry UIListDir " + entry.name.value);
                    entry.isProxy = true;
                    entry.obj = new UIListDir(0).Read(reader, true, this, entry);

                    dir = new DirectoryMeta();
                    dir.platform = platform;
                    dir.Read(reader);
                    entry.dir = dir;
                    break;
                case "UnisonIcon":
                    Debug.WriteLine("Reading entry UnisonIcon " + entry.name.value);
                    entry.isProxy = true;
                    entry.obj = new UnisonIcon(0).Read(reader, true, this, entry);

                    dir = new DirectoryMeta();
                    dir.platform = platform;
                    dir.Read(reader);
                    entry.dir = dir;
                    break;
                case "VocalTrackDir":
                    Debug.WriteLine("Reading entry VocalTrackDir " + entry.name.value);
                    entry.isProxy = true;
                    entry.obj = new VocalTrackDir(0).Read(reader, true, this, entry);

                    dir = new DirectoryMeta();
                    dir.platform = platform;
                    dir.Read(reader);
                    entry.dir = dir;
                    break;
                case "WorldDir":
                    Debug.WriteLine("Reading entry WorldDir " + entry.name.value);
                    entry.isProxy = true;
                    entry.obj = new WorldDir(0).Read(reader, true, this, entry);

                    dir = new DirectoryMeta();
                    dir.platform = platform;
                    dir.Read(reader);
                    entry.dir = dir;
                    break;
                case "WorldInstance":
                    Debug.WriteLine("Reading entry WorldInstance " + entry.name.value);
                    entry.isProxy = true;

                    entry.obj = new WorldInstance(0).Read(reader, false, this, entry);

                    // if the world instance has no persistent perObjs, it will have a dir as expected, otherwise it won't
                    if (!((WorldInstance)entry.obj).hasPersistentObjects)
                    {
                        dir = new DirectoryMeta();
                        dir.platform = platform;
                        dir.Read(reader);
                        entry.dir = dir;

                        // these can be followed by a Character or other dirs...wtf
                        // if it is another dir it seems to always be followed by persistentobjects
                        if (entry.dir != null && entry.dir.type.value == "WorldInstance")
                        {
                            if (((WorldInstance)dir.directory).hasPersistentObjects)
                            {
                                ((WorldInstance)dir.directory).persistentObjects = new WorldInstance.PersistentObjects().Read(reader, this, entry, ((WorldInstance)entry.obj).revision);
                            }
                        }
                        else
                        {
                            // hack
                            ((WorldInstance)entry.obj).persistentObjects = new WorldInstance.PersistentObjects().Read(reader, this, entry, ((WorldInstance)entry.obj).revision);
                        }
                    }
                    else
                    {
                        ((WorldInstance)entry.obj).persistentObjects = new WorldInstance.PersistentObjects().Read(reader, this, entry, ((WorldInstance)entry.obj).revision);
                    }
                    break;

                /////////////
                // OBJECTS //
                /////////////
                case "AnimFilter":
                    Debug.WriteLine("Reading entry AnimFilter " + entry.name.value);
                    entry.obj = new RndAnimFilter().Read(reader, true, this, entry);
                    break;
                case "BandButton":
                    Debug.WriteLine("Reading entry BandButton " + entry.name.value);
                    entry.obj = new BandButton().Read(reader, true, this, entry);
                    break;
                case "BandCharDesc":
                    Debug.WriteLine("Reading entry BandCharDesc " + entry.name.value);
                    entry.obj = new BandCharDesc().Read(reader, true, this, entry);
                    break;
                case "BandConfiguration":
                    Debug.WriteLine("Reading entry BandConfiguration " + entry.name.value);
                    entry.obj = new BandConfiguration().Read(reader, true, this, entry);
                    break;
                case "BandDirector":
                    Debug.WriteLine("Reading entry BandDirector " + entry.name.value);
                    entry.obj = new BandDirector().Read(reader, true, this, entry);
                    break;
                case "BandFaceDeform":
                    Debug.WriteLine("Reading entry BandFaceDeform " + entry.name.value);
                    entry.obj = new BandFaceDeform().Read(reader, true, this, entry);
                    break;
                case "BandLabel":
                    Debug.WriteLine("Reading entry BandLabel " + entry.name.value);
                    entry.obj = new BandLabel().Read(reader, true, this, entry);
                    break;
                case "BandPlacer":
                    Debug.WriteLine("Reading entry BandPlacer " + entry.name.value);
                    entry.obj = new BandPlacer().Read(reader, true, this, entry);
                    break;
                case "BandSongPref":
                    Debug.WriteLine("Reading entry BandSongPref " + entry.name.value);
                    entry.obj = new BandSongPref().Read(reader, true, this, entry);
                    break;
                case "Cam":
                    Debug.WriteLine("Reading entry Cam " + entry.name.value);
                    entry.obj = new RndCam().Read(reader, true, this, entry);
                    break;
                case "CharClipGroup":
                    Debug.WriteLine("Reading entry CharClipGroup " + entry.name.value);
                    entry.obj = new CharClipGroup().Read(reader, true, this, entry);
                    break;
                case "CharForeTwist":
                    Debug.WriteLine("Reading entry CharForeTwist " + entry.name.value);
                    entry.obj = new CharForeTwist().Read(reader, true, this, entry);
                    break;
                case "CharGuitarString":
                    Debug.WriteLine("Reading entry CharGuitarString " + entry.name.value);
                    entry.obj = new CharGuitarString().Read(reader, true, this, entry);
                    break;
                case "CharHair":
                    Debug.WriteLine("Reading entry CharHair " + entry.name.value);
                    entry.obj = new CharHair().Read(reader, true, this, entry);
                    break;
                case "CharIKMidi":
                    Debug.WriteLine("Reading entry CharIKMidi " + entry.name.value);
                    entry.obj = new CharIKMidi().Read(reader, true, this, entry);
                    break;
                case "CharIKRod":
                    Debug.WriteLine("Reading entry CharIKRod " + entry.name.value);
                    entry.obj = new CharIKRod().Read(reader, true, this, entry);
                    break;
                case "CharInterest":
                    Debug.WriteLine("Reading entry CharInterest " + entry.name.value);
                    entry.obj = new CharInterest().Read(reader, true, this, entry);
                    break;
                case "CharMeshHide":
                    Debug.WriteLine("Reading entry CharMeshHide " + entry.name.value);
                    entry.obj = new CharMeshHide().Read(reader, true, this, entry);
                    break;
                case "CharPosConstraint":
                    Debug.WriteLine("Reading entry CharPosConstraint " + entry.name.value);
                    entry.obj = new CharPosConstraint().Read(reader, true, this, entry);
                    break;
                case "CharServoBone":
                    Debug.WriteLine("Reading entry CharServoBone " + entry.name.value);
                    entry.obj = new CharServoBone().Read(reader, true, this, entry);
                    break;
                case "CharUpperTwist":
                    Debug.WriteLine("Reading entry CharUpperTwist " + entry.name.value);
                    entry.obj = new CharUpperTwist().Read(reader, true, this, entry);
                    break;
                case "CharWalk":
                    Debug.WriteLine("Reading entry CharWalk " + entry.name.value);
                    entry.obj = new CharWalk().Read(reader, true, this, entry);
                    break;
                case "CharWeightSetter":
                    Debug.WriteLine("Reading entry CharWeightSetter " + entry.name.value);
                    entry.obj = new CharWeightSetter().Read(reader, true, this, entry);
                    break;
                case "CheckboxDisplay":
                    Debug.WriteLine("Reading entry CheckboxDisplay " + entry.name.value);
                    entry.obj = new CheckboxDisplay().Read(reader, true, this, entry);
                    break;
                case "ColorPalette":
                    Debug.WriteLine("Reading entry ColorPalette " + entry.name.value);
                    entry.obj = new ColorPalette().Read(reader, true, this, entry);
                    break;
                case "Environ":
                    Debug.WriteLine("Reading entry Environ " + entry.name.value);
                    entry.obj = new RndEnviron().Read(reader, true, this, entry);
                    break;
                //case "EventTrigger":
                //    Debug.WriteLine("Reading entry EventTrigger " + entry.name.value);
                //    entry.obj = new EventTrigger().Read(reader, true, this, entry);
                //    break;
                case "FileMerger":
                    Debug.WriteLine("Reading entry FileMerger " + entry.name.value);
                    entry.obj = new FileMerger().Read(reader, true, this, entry);
                    break;
                case "Group":
                case "View":
                    Debug.WriteLine("Reading entry Group " + entry.name.value);
                    entry.obj = new RndGroup().Read(reader, true, this, entry);
                    break;
                case "InlineHelp":
                    Debug.WriteLine("Reading entry InlineHelp " + entry.name.value);
                    entry.obj = new InlineHelp().Read(reader, true, this, entry);
                    break;
                case "Light":
                    Debug.WriteLine("Reading entry Light " + entry.name.value);
                    entry.obj = new RndLight().Read(reader, true, this, entry);
                    break;
                case "Mat":
                    Debug.WriteLine("Reading entry Mat " + entry.name.value);
                    entry.obj = new RndMat().Read(reader, true, this, entry);
                    break;
                case "MatAnim":
                    Debug.WriteLine("Reading entry MatAnim " + entry.name.value);
                    entry.obj = new RndMatAnim().Read(reader, true, this, entry);
                    break;
                case "Mesh":
                    Debug.WriteLine("Reading entry Mesh " + entry.name.value);
                    entry.obj = new RndMesh().Read(reader, true, this, entry);
                    break;
                case "MotionBlur":
                    Debug.WriteLine("Reading entry MotionBlur " + entry.name.value);
                    entry.obj = new RndMotionBlur().Read(reader, true, this, entry);
                    break;
                case "Object":
                    Debug.WriteLine("Reading entry Object " + entry.name.value);
                    entry.obj = new Object().Read(reader, true, this, entry);
                    break;
                case "OutfitConfig":
                    if (revision != 28)
                        goto default;
                    Debug.WriteLine("Reading entry OutfitConfig " + entry.name.value);
                    entry.obj = new OutfitConfig().Read(reader, true, this, entry);
                    break;
                case "P9Director":
                    Debug.WriteLine("Reading entry P9Director " + entry.name.value);
                    entry.obj = new P9Director().Read(reader, true, this, entry);
                    break;
                case "ParticleSys":
                    Debug.WriteLine("Reading entry ParticleSys " + entry.name.value);
                    entry.obj = new RndParticleSys().Read(reader, true, this, entry);
                    break;
                case "PollAnim":
                    Debug.WriteLine("Reading entry PollAnim " + entry.name.value);
                    entry.obj = new RndPollAnim().Read(reader, true, this, entry);
                    break;
                case "RandomGroupSeq":
                    Debug.WriteLine("Reading entry RandomGroupSeq " + entry.name.value);
                    entry.obj = new RandomGroupSeq().Read(reader, true, this, entry);
                    break;
                case "ScreenMask":
                    Debug.WriteLine("Reading entry ScreenMask " + entry.name.value);
                    entry.obj = new RndScreenMask().Read(reader, true, this, entry);
                    break;
                case "Set":
                    Debug.WriteLine("Reading entry RndSet " + entry.name.value);
                    entry.obj = new RndSet().Read(reader, true, this, entry);
                    break;
                case "Sfx":
                    Debug.WriteLine("Reading entry Sfx " + entry.name.value);
                    entry.obj = new Sfx().Read(reader, true, this, entry);
                    break;
                case "SpotlightDrawer":
                    Debug.WriteLine("Reading entry SpotlightDrawer " + entry.name.value);
                    entry.obj = new SpotlightDrawer().Read(reader, true, this, entry);
                    break;
                case "SynthSample":
                    Debug.WriteLine("Reading entry SynthSample " + entry.name.value);
                    entry.obj = new SynthSample().Read(reader, true, this, entry);
                    break;
                case "Tex":
                    Debug.WriteLine("Reading entry Tex " + entry.name.value);
                    entry.obj = new RndTex().Read(reader, true, this, entry);
                    break;
                case "TexBlendController":
                    Debug.WriteLine("Reading entry TexBlendController " + entry.name.value);
                    entry.obj = new RndTexBlendController().Read(reader, true, this, entry);
                    break;
                case "TexBlender":
                    Debug.WriteLine("Reading entry TexBlender " + entry.name.value);
                    entry.obj = new RndTexBlender().Read(reader, true, this, entry);
                    break;
                case "TexMovie":
                    Debug.WriteLine("Reading entry TexMovie " + entry.name.value);
                    entry.obj = new RndTexMovie().Read(reader, true, this, entry);
                    break;
                case "Trans":
                    Debug.WriteLine("Reading entry Trans " + entry.name.value);
                    entry.obj = new RndTrans().Read(reader, true, this, entry);
                    break;
                case "TransProxy":
                    Debug.WriteLine("Reading entry TransProxy " + entry.name.value);
                    entry.obj = new RndTransProxy().Read(reader, true, this, entry);
                    break;
                case "UIColor":
                    Debug.WriteLine("Reading entry UIColor" + entry.name.value);
                    entry.obj = new UIColor().Read(reader, true, this, entry);
                    break;
                case "UIGuide":
                    Debug.WriteLine("Reading entry UIGuide " + entry.name.value);
                    entry.obj = new UIGuide().Read(reader, true, this, entry);
                    break;
                case "Wind":
                    Debug.WriteLine("Reading entry Wind " + entry.name.value);
                    entry.obj = new RndWind().Read(reader, true, this, entry);
                    break;
                case "WorldCrowd":
                    Debug.WriteLine("Reading entry WorldCrowd " + entry.name.value);
                    entry.obj = new WorldCrowd().Read(reader, true, this, entry);
                    break;
                case "WorldReflection":
                    Debug.WriteLine("Reading entry WorldReflection " + entry.name.value);
                    entry.obj = new WorldReflection().Read(reader, true, this, entry);
                    break;
                // re-enable when the class is 100%
                //case "CharClip":
                //    Debug.WriteLine("Reading entry CharClip " + entry.name.value);
                //    entry.obj = new CharClip().Read(reader, true, this, entry);
                //    break;

                default:
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
                    break;
            }
        }

        public void WriteEntry(EndianWriter writer, DirectoryMeta.Entry entry)
        {
            // handle dirty assets
            if (entry.dirty)
            {
                writer.WriteBlock(entry.objBytes.ToArray());
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
                return;
            }

            Debug.WriteLine("Writing " + entry.type.value + " " + entry.name.value);
            switch (entry.type.value)
            {
                //////////
                // DIRS //
                //////////

                case "BandScoreboard":
                    ((BandScoreboard)entry.obj).Write(writer, true, this, entry);
                    entry.isProxy = false;
                    entry.dir.Write(writer);
                    break;
                case "BandCrowdMeterDir":
                    ((BandCrowdMeterDir)entry.obj).Write(writer, true, this, entry);
                    entry.isProxy = false;
                    entry.dir.Write(writer);
                    break;
                case "BandStarDisplay":
                    ((BandStarDisplay)entry.obj).Write(writer, true, this, entry);
                    entry.isProxy = false;
                    entry.dir.Write(writer);
                    break;
                case "Character":
                    ((Character)entry.obj).Write(writer, true, this, entry);
                    if (((Character)entry.obj).proxyPath != String.Empty)
                    {
                        entry.isProxy = false;
                        entry.dir.Write(writer);
                    }
                    break;
                case "CharClipSet":
                    writer.WriteUInt32(0x18);
                    ((ObjectDir)entry.obj).Write(writer, true, this, entry);
                    entry.isProxy = false;
                    entry.dir.Write(writer);
                    break;
                case "CrowdMeterIcon":
                    ((BandCrowdMeterIcon)entry.obj).Write(writer, true, this, entry);
                    entry.isProxy = false;
                    entry.dir.Write(writer);
                    break;
                case "EndingBonusDir":
                    ((RndDir)entry.obj).Write(writer, true, this, entry);
                    entry.isProxy = false;
                    entry.dir.Write(writer);
                    break;
                case "GemTrackDir":
                    ((GemTrackDir)entry.obj).Write(writer, true, this, entry);
                    entry.isProxy = false;
                    entry.dir.Write(writer);
                    break;
                case "MoveDir":
                    ((MoveDir)entry.obj).Write(writer, true, this, entry);
                    entry.isProxy = false;
                    entry.dir.Write(writer);
                    break;
                case "ObjectDir":
                    ((ObjectDir)entry.obj).Write(writer, true, this, entry);
                    entry.isProxy = false;
                    entry.dir.Write(writer);
                    break;
                case "OverdriveMeterDir":
                    ((OverdriveMeterDir)entry.obj).Write(writer, true, this, entry);
                    entry.isProxy = false;
                    entry.dir.Write(writer);
                    break;
                case "OvershellDir":
                    ((OvershellDir)entry.obj).Write(writer, true, this, entry);
                    entry.isProxy = false;
                    entry.dir.Write(writer);
                    break;
                case "P9Character":
                    ((P9Character)entry.obj).Write(writer, true, this, entry);
                    entry.isProxy = false;
                    entry.dir.Write(writer);
                    break;
                case "PanelDir":
                    ((PanelDir)entry.obj).Write(writer, true, this, entry);
                    entry.isProxy = false;
                    entry.dir.Write(writer);
                    break;
                case "PitchArrowDir":
                    ((PitchArrowDir)entry.obj).Write(writer, true, this, entry);
                    entry.isProxy = false;
                    entry.dir.Write(writer);
                    break;
                case "RndDir":
                    ((RndDir)entry.obj).Write(writer, true, this, entry);
                    if (entry.dir != null)
                    {
                        entry.isProxy = false;
                        entry.dir.Write(writer);
                    }
                    break;
                case "SkeletonDir":
                    ((SkeletonDir)entry.obj).Write(writer, true, this, entry);
                    entry.isProxy = false;
                    entry.dir.Write(writer);
                    break;
                case "StreakMeterDir":
                    ((StreakMeterDir)entry.obj).Write(writer, true, this, entry);
                    entry.isProxy = false;
                    entry.dir.Write(writer);
                    break;
                case "SynthDir":
                    ((SynthDir)entry.obj).Write(writer, true, this, entry);
                    entry.isProxy = false;
                    entry.dir.Write(writer);
                    break;
                case "TrackDir":
                    ((TrackDir)entry.obj).Write(writer, true, this, entry);
                    entry.isProxy = false;
                    entry.dir.Write(writer);
                    break;
                case "TrackPanelDir":
                    ((TrackPanelDir)entry.obj).Write(writer, true, this, entry);
                    entry.isProxy = false;
                    entry.dir.Write(writer);
                    break;
                case "UILabelDir":
                    ((UILabelDir)entry.obj).Write(writer, true, this, entry);
                    entry.isProxy = false;
                    entry.dir.Write(writer);
                    break;
                case "UIListDir":
                    ((UIListDir)entry.obj).Write(writer, true, this, entry);
                    entry.isProxy = false;
                    entry.dir.Write(writer);
                    break;
                case "UnisonIcon":
                    ((UnisonIcon)entry.obj).Write(writer, true, this, entry);
                    entry.isProxy = false;
                    entry.dir.Write(writer);
                    break;
                case "VocalTrackDir":
                    ((VocalTrackDir)entry.obj).Write(writer, true, this, entry);
                    entry.isProxy = false;
                    entry.dir.Write(writer);
                    break;
                case "WorldDir":
                    ((WorldDir)entry.obj).Write(writer, true, this, entry);
                    entry.isProxy = false;
                    entry.dir.Write(writer);
                    break;
                case "WorldInstance":
                    // Write the main object
                    ((WorldInstance)entry.obj).Write(writer, false, this, entry);
                    entry.isProxy = false;

                    if (!((WorldInstance)entry.obj).hasPersistentObjects)
                    {
                        // Write the directory
                        entry.dir.Write(writer);

                        if (entry.dir.type.value == "WorldInstance")
                        {
                            if (((WorldInstance)entry.dir.directory).hasPersistentObjects)
                            {
                                // Write the persistent perObjs
                                ((WorldInstance)entry.dir.directory).persistentObjects.Write(writer, this, entry, ((WorldInstance)entry.obj).revision);
                            }
                        }
                        else
                        {
                            // write the persistent objects we stored in the obj
                            // this is a dumb hack but prevents need to adapt the API to allow mixes like that
                            ((WorldInstance)entry.obj).persistentObjects.Write(writer, this, entry, ((WorldInstance)entry.obj).revision);
                        }
                    }
                    else
                    {
                        // Write the persistent perObjs
                        ((WorldInstance)entry.obj).persistentObjects.Write(writer, this, entry, ((WorldInstance)entry.obj).revision);
                    }


                    break;

                /////////////
                // OBJECTS //
                /////////////
                case "AnimFilter":
                    ((RndAnimFilter)entry.obj).Write(writer, true, this, entry);
                    break;
                case "BandButton":
                    ((BandButton)entry.obj).Write(writer, true, this, entry);
                    break;
                case "BandCharDesc":
                    ((BandCharDesc)entry.obj).Write(writer, true, this, entry);
                    break;
                case "BandConfiguration":
                    ((BandConfiguration)entry.obj).Write(writer, true, this, entry);
                    break;
                case "BandDirector":
                    ((BandDirector)entry.obj).Write(writer, true, this, entry);
                    break;
                case "BandFaceDeform":
                    ((BandFaceDeform)entry.obj).Write(writer, true, this, entry);
                    break;
                case "BandLabel":
                    ((BandLabel)entry.obj).Write(writer, true, this, entry);
                    break;
                case "BandPlacer":
                    ((BandPlacer)entry.obj).Write(writer, true, this, entry);
                    break;
                case "BandSongPref":
                    ((BandSongPref)entry.obj).Write(writer, true, this, entry);
                    break;
                case "Cam":
                    ((RndCam)entry.obj).Write(writer, true, this, entry);
                    break;
                case "CharClipGroup":
                    ((CharClipGroup)entry.obj).Write(writer, true, this, entry);
                    break;
                case "CharForeTwist":
                    ((CharForeTwist)entry.obj).Write(writer, true, this, entry);
                    break;
                case "CharGuitarString":
                    ((CharGuitarString)entry.obj).Write(writer, true, this, entry);
                    break;
                case "CharHair":
                    ((CharHair)entry.obj).Write(writer, true, this, entry);
                    break;
                case "CharIKMidi":
                    ((CharIKMidi)entry.obj).Write(writer, true, this, entry);
                    break;
                case "CharIKRod":
                    ((CharIKRod)entry.obj).Write(writer, true, this, entry);
                    break;
                case "CharInterest":
                    ((CharInterest)entry.obj).Write(writer, true, this, entry);
                    break;
                case "CharMeshHide":
                    ((CharMeshHide)entry.obj).Write(writer, true, this, entry);
                    break;
                case "CharPosConstraint":
                    ((CharPosConstraint)entry.obj).Write(writer, true, this, entry);
                    break;
                case "CharServoBone":
                    ((CharServoBone)entry.obj).Write(writer, true, this, entry);
                    break;
                case "CharUpperTwist":
                    ((CharUpperTwist)entry.obj).Write(writer, true, this, entry);
                    break;
                case "CharWalk":
                    ((CharWalk)entry.obj).Write(writer, true, this, entry);
                    break;
                case "CharWeightSetter":
                    ((CharWeightSetter)entry.obj).Write(writer, true, this, entry);
                    break;
                case "CheckboxDisplay":
                    ((CheckboxDisplay)entry.obj).Write(writer, true, this, entry);
                    break;
                case "ColorPalette":
                    ((ColorPalette)entry.obj).Write(writer, true, this, entry);
                    break;
                case "Environ":
                    ((RndEnviron)entry.obj).Write(writer, true, this, entry);
                    break;
                case "FileMerger":
                    ((FileMerger)entry.obj).Write(writer, true, this, entry);
                    break;
                case "Group":
                    ((RndGroup)entry.obj).Write(writer, true, this, entry);
                    break;
                case "InlineHelp":
                    ((InlineHelp)entry.obj).Write(writer, true, this, entry);
                    break;
                case "Light":
                    ((RndLight)entry.obj).Write(writer, true, this, entry);
                    break;
                case "Mat":
                    ((RndMat)entry.obj).Write(writer, true, this, entry);
                    break;
                case "MatAnim":
                    ((RndMatAnim)entry.obj).Write(writer, true, this, entry);
                    break;
                case "Mesh":
                    ((RndMesh)entry.obj).Write(writer, true, this, entry);
                    break;
                case "MotionBlur":
                    ((RndMotionBlur)entry.obj).Write(writer, true, this, entry);
                    break;
                case "ParticleSys":
                    ((RndParticleSys)entry.obj).Write(writer, true, this, entry);
                    break;
                case "PollAnim":
                    ((RndPollAnim)entry.obj).Write(writer, true, this, entry);
                    break;
                case "RandomGroupSeq":
                    ((RandomGroupSeq)entry.obj).Write(writer, true, this, entry);
                    break;
                case "ScreenMask":
                    ((RndScreenMask)entry.obj).Write(writer, true, this, entry);
                    break;
                case "Set":
                    ((RndSet)entry.obj).Write(writer, true, this, entry);
                    break;
                case "Sfx":
                    ((Sfx)entry.obj).Write(writer, true, this, entry);
                    break;
                case "SpotlightDrawer":
                    ((SpotlightDrawer)entry.obj).Write(writer, true, this, entry);
                    break;
                case "SynthSample":
                    ((SynthSample)entry.obj).Write(writer, true, this, entry);
                    break;
                case "Tex":
                    ((RndTex)entry.obj).Write(writer, true, this, entry);
                    break;
                case "TexBlendController":
                    ((RndTexBlendController)entry.obj).Write(writer, true, this, entry);
                    break;
                case "TexBlender":
                    ((RndTexBlender)entry.obj).Write(writer, true, this, entry);
                    break;
                case "TexMovie":
                    ((RndTexMovie)entry.obj).Write(writer, true, this, entry);
                    break;
                case "Trans":
                    ((RndTrans)entry.obj).Write(writer, true, false);
                    break;
                case "TransProxy":
                    ((RndTransProxy)entry.obj).Write(writer, true, this, entry);
                    break;
                case "UIColor":
                    ((UIColor)entry.obj).Write(writer, true, this, entry);
                    break;
                case "UIComponent":
                    ((UIComponent)entry.obj).Write(writer, true, this, entry);
                    break;
                case "UIGuide":
                    ((UIGuide)entry.obj).Write(writer, true, this, entry);
                    break;
                case "Wind":
                    ((RndWind)entry.obj).Write(writer, true, this, entry);
                    break;
                case "WorldReflection":
                    ((WorldReflection)entry.obj).Write(writer, true, this, entry);
                    break;
                // re-enable when the class is 100%
                //case "CharClip":
                //    Debug.WriteLine("Reading entry CharClip " + entry.name.value);
                //    entry.obj = new CharClip().Read(reader, true, this, entry);
                //    break;

                default:
                    // see if the type contains "Dir" and if so, throw an exception because the Milo that will get produced will never work
                    if (entry.type.value.Contains("Dir"))
                        throw new Exception("Trying to write an unsupported dir entry of type: " + entry.type.value + ", this Milo cannot be saved");


                    Debug.WriteLine("Unknown entry type, dumping raw bytes for " + entry.type.value + " of name " + entry.name.value);

                    // this should allow saving Milos with types that have yet to be implemented
                    writer.WriteBlock(entry.objBytes.ToArray());

                    // write the ending bytes
                    writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
                    break;
            }
        }

        public static DirectoryMeta New(string type, string name, uint sceneRevision, ushort rootDirRevision)
        {
            DirectoryMeta dir = new DirectoryMeta();
            dir.type = type;
            dir.name = name;
            // switch on type name
            switch (dir.type)
            {
                case "ObjectDir":
                    dir.directory = new ObjectDir(rootDirRevision);
                    break;
                case "RndDir":
                    dir.directory = new RndDir(rootDirRevision);
                    break;
                case "PanelDir":
                    dir.directory = new PanelDir(rootDirRevision);
                    break;
                case "WorldDir":
                    dir.directory = new WorldDir(rootDirRevision);
                    break;
                case "Character":
                    dir.directory = new Character(rootDirRevision);
                    break;
                case "P9Character":
                    dir.directory = new P9Character(rootDirRevision);
                    break;
                case "CharClipSet":
                    dir.directory = new CharClipSet(rootDirRevision);
                    break;
                case "UILabelDir":
                    dir.directory = new UILabelDir(rootDirRevision);
                    break;
                case "UIListDir":
                    dir.directory = new UIListDir(rootDirRevision);
                    break;
                case "BandCrowdMeterDir":
                    dir.directory = new BandCrowdMeterDir(rootDirRevision);
                    break;
                case "CrowdMeterIcon":
                    dir.directory = new BandCrowdMeterIcon(rootDirRevision);
                    break;
                case "BandCharacter":
                    dir.directory = new BandCharacter(rootDirRevision);
                    break;
                case "WorldInstance":
                    dir.directory = new WorldInstance(rootDirRevision);
                    break;
                case "TrackPanelDir":
                    dir.directory = new TrackPanelDir(rootDirRevision);
                    break;
                case "UnisonIcon":
                    dir.directory = new UnisonIcon(rootDirRevision);
                    break;
                case "EndingBonusDir":
                    dir.directory = new RndDir(rootDirRevision);
                    break;
                case "BandStarDisplay":
                    dir.directory = new BandStarDisplay(rootDirRevision);
                    break;
                case "BandScoreboard":
                    dir.directory = new BandScoreboard(rootDirRevision);
                    break;
                case "VocalTrackDir":
                    dir.directory = new VocalTrackDir(rootDirRevision);
                    break;
                case "GemTrackDir":
                    dir.directory = new GemTrackDir(rootDirRevision);
                    break;
                case "MoveDir":
                    dir.directory = new MoveDir(rootDirRevision);
                    break;
                case "SkeletonDir":
                    dir.directory = new SkeletonDir(rootDirRevision);
                    break;
                case "OvershellDir":
                    dir.directory = new OvershellDir(rootDirRevision);
                    break;
                case "OverdriveMeterDir":
                    dir.directory = new OverdriveMeterDir(rootDirRevision);
                    break;
                default:
                    throw new Exception("Unknown directory type: " + type.GetType().Name + ", cannot continue creating directory");
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
