using MiloLib.Assets.Band;
using MiloLib.Assets.Char;
using MiloLib.Assets.P9;
using MiloLib.Assets.Rnd;
using MiloLib.Assets.UI;
using MiloLib.Classes;
using MiloLib.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace MiloLib.Assets
{
    public class DirectoryMeta
    {
        public class Entry
        {
            public Symbol type = new(0, "");
            public Symbol name = new(0, "");
            public Object obj;
            public DirectoryMeta? dir;

            /// <summary>
            /// Set when the object has been added or otherwise created through a non-serialized fashion (i.e. as raw bytes)
            /// </summary>
            public bool dirty = false;

            // the raw bytes of the object, straight from the file
            public List<byte> objBytes = new List<byte>();

            public Entry(Symbol type, Symbol name, Object dir)
            {
                this.type = type;
                this.name = name;
                this.obj = dir;
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

        public uint revision;

        public Symbol type = new(0, "");
        public Symbol name = new(0, "");

        private uint stringTableCount;
        private uint stringTableSize;


        private uint externalResourceCount;
        public List<Symbol> externalResources = new List<Symbol>();

        private uint entryCount;
        public List<Entry> entries = new List<Entry>();

        public Object directory;

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
                    objectDir.Read(reader, true, this);
                    directory = objectDir;
                    break;
                case "RndDir":
                    Debug.WriteLine("Reading RndDir " + name.value);
                    RndDir rndDir = new RndDir(0);
                    rndDir.Read(reader, true, this);
                    directory = rndDir;
                    break;
                case "PanelDir":
                    Debug.WriteLine("Reading PanelDir " + name.value);
                    PanelDir panelDir = new PanelDir(0);
                    panelDir.Read(reader, true, this);
                    directory = panelDir;
                    break;
                case "CharClipSet":
                    Debug.WriteLine("Reading CharClipSet " + name.value);
                    CharClipSet charClipSet = new CharClipSet(0);
                    charClipSet.Read(reader, true, this);
                    directory = charClipSet;
                    break;
                case "WorldDir":
                    Debug.WriteLine("Reading WorldDir " + name.value);
                    WorldDir worldDir = new WorldDir(0);
                    worldDir.Read(reader, true, this);
                    directory = worldDir;
                    break;
                case "Character":
                    Debug.WriteLine("Reading Character " + name.value);
                    Character character = new Character(0);
                    character.Read(reader, true, this);
                    directory = character;
                    break;
                case "UILabelDir":
                    Debug.WriteLine("Reading UILabelDir " + name.value);
                    UILabelDir uiLabelDir = new UILabelDir(0);
                    uiLabelDir.Read(reader, true, this);
                    directory = uiLabelDir;
                    break;
                case "UIListDir":
                    Debug.WriteLine("Reading UIListDir " + name.value);
                    UIListDir uiListDir = new UIListDir(0);
                    uiListDir.Read(reader, true, this);
                    directory = uiListDir;
                    break;
                case "BandCrowdMeterDir":
                    Debug.WriteLine("Reading BandCrowdMeterDir " + name.value);
                    BandCrowdMeterDir bandCrowdMeterDir = new BandCrowdMeterDir(0);
                    bandCrowdMeterDir.Read(reader, true, this);
                    directory = bandCrowdMeterDir;
                    break;
                case "CrowdMeterIcon":
                    Debug.WriteLine("Reading CrowdMeterIcon " + name.value);
                    CrowdMeterIcon crowdMeterIcon = new CrowdMeterIcon(0);
                    crowdMeterIcon.Read(reader, true, this);
                    directory = crowdMeterIcon;
                    break;
                case "CharBoneDir":
                    Debug.WriteLine("Reading CharBoneDir " + name.value);
                    CharBoneDir charBoneDir = new CharBoneDir(0);
                    charBoneDir.Read(reader, true, this);
                    directory = charBoneDir;
                    break;
                case "BandCharacter":
                    Debug.WriteLine("Reading BandCharacter " + name.value);
                    BandCharacter bandCharacter = new BandCharacter(0);
                    bandCharacter.Read(reader, true, this);
                    directory = bandCharacter;
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
                            break; // Pattern found, exit loop
                        }

                        // Pattern not matched, reset to next byte after start
                        reader.BaseStream.Position = currentPos;
                    }

                    entry.objBytes.Add(b);
                }

                // reset the position
                reader.BaseStream.Position = startPos;

                switch (entry.type.value)
                {
                    // DIRS
                    // These need special handling, they are different than inlined directories

                    case "ObjectDir":
                        Debug.WriteLine("Reading entry ObjectDir " + entry.name.value);
                        entry.obj = new ObjectDir(0).Read(reader, true, this);

                        entry.dir = new DirectoryMeta().Read(reader);
                        break;
                    case "RndDir":
                        Debug.WriteLine("Reading entry RndDir " + entry.name.value);
                        entry.obj = new RndDir(0).Read(reader, true, this);

                        entry.dir = new DirectoryMeta().Read(reader);
                        break;

                    case "UIPanel":
                    case "PanelDir":
                        Debug.WriteLine("Reading entry PanelDir " + entry.name.value);
                        entry.obj = new PanelDir(0).Read(reader, true, this);

                        entry.dir = new DirectoryMeta().Read(reader);
                        break;

                    case "WorldDir":
                        Debug.WriteLine("Reading entry WorldDir " + entry.name.value);
                        entry.obj = new WorldDir(0).Read(reader, true, this);

                        entry.dir = new DirectoryMeta().Read(reader);
                        break;

                    case "Character":
                        Debug.WriteLine("Reading entry Character " + entry.name.value);
                        entry.obj = new Character(0).Read(reader, true, this);

                        entry.dir = new DirectoryMeta().Read(reader);
                        break;

                    case "P9Character":
                        Debug.WriteLine("Reading entry P9Character " + entry.name.value);
                        entry.obj = new P9Character(0).Read(reader, true, this);

                        entry.dir = new DirectoryMeta().Read(reader);
                        break;

                    case "CharClipSet":
                        Debug.WriteLine("Reading entry CharClipSet " + entry.name.value);

                        // this is unhinged, why'd they do it like this?
                        reader.ReadUInt32();
                        entry.obj = new ObjectDir(0).Read(reader, true, this);

                        entry.dir = new DirectoryMeta().Read(reader);
                        break;

                    case "CharBoneDir":
                        Debug.WriteLine("Reading entry CharBoneDir " + entry.name.value);
                        entry.obj = new CharBoneDir(0).Read(reader, true, this);

                        entry.dir = new DirectoryMeta().Read(reader);
                        break;

                    case "UIListDir":
                        Debug.WriteLine("Reading entry UIListDir " + entry.name.value);
                        entry.obj = new UIListDir(0).Read(reader, true, this);

                        entry.dir = new DirectoryMeta().Read(reader);
                        break;

                    case "UILabelDir":
                        Debug.WriteLine("Reading entry UILabelDir " + entry.name.value);
                        entry.obj = new UILabelDir(0).Read(reader, true, this);

                        entry.dir = new DirectoryMeta().Read(reader);
                        break;

                    case "BandCrowdMeterDir":
                        Debug.WriteLine("Reading entry BandCrowdMeterDir " + entry.name.value);
                        entry.obj = new BandCrowdMeterDir(0).Read(reader, true, this);

                        entry.dir = new DirectoryMeta().Read(reader);
                        break;

                    case "CrowdMeterIcon":
                        Debug.WriteLine("Reading entry CrowdMeterIcon " + entry.name.value);
                        entry.obj = new CrowdMeterIcon(0).Read(reader, true, this);

                        entry.dir = new DirectoryMeta().Read(reader);
                        break;

                    case "BandCharacter":
                        Debug.WriteLine("Reading entry BandCharacter " + entry.name.value);
                        entry.obj = new BandCharacter(0).Read(reader, true, this);

                        entry.dir = new DirectoryMeta().Read(reader);
                        break;

                    // OBJECTS

                    case "Object":
                        Debug.WriteLine("Reading entry Object " + entry.name.value);
                        entry.obj = new Object().Read(reader, true, this);
                        break;
                    case "BandSongPref":
                        Debug.WriteLine("Reading entry BandSongPref " + entry.name.value);
                        entry.obj = new BandSongPref().Read(reader, true, this);
                        break;
                    case "Sfx":
                        Debug.WriteLine("Reading entry Sfx " + entry.name.value);
                        entry.obj = new Sfx().Read(reader, true, this);
                        break;
                    case "Trans":
                        Debug.WriteLine("Reading entry Trans " + entry.name.value);
                        entry.obj = new RndTrans().Read(reader, true, this);
                        break;
                    case "View":
                    case "Group":
                        Debug.WriteLine("Reading entry Group " + entry.name.value);
                        entry.obj = new RndGroup().Read(reader, true, this);
                        break;
                    case "P9Director":
                        Debug.WriteLine("Reading entry P9Director " + entry.name.value);
                        entry.obj = new P9Director().Read(reader, true, this);
                        break;
                    // TODO: figure out how to read textures properly
                    case "Tex":
                        Debug.WriteLine("Reading entry Tex " + entry.name.value);
                        entry.obj = new RndTex().Read(reader, true, this);
                        break;
                    case "ColorPalette":
                        Debug.WriteLine("Reading entry ColorPalette " + entry.name.value);
                        entry.obj = new ColorPalette().Read(reader, true, this);
                        break;
                    //case "Mat":
                    //    Debug.WriteLine("Reading entry Mat " + entry.name.value);
                    //    entry.obj = new RndMat().Read(reader, true, this);
                    //    break;
                    case "BandCharDesc":
                        Debug.WriteLine("Reading entry BandCharDesc " + entry.name.value);
                        entry.obj = new BandCharDesc().Read(reader, true, this);
                        break;
                    case "Light":
                        Debug.WriteLine("Reading entry Light " + entry.name.value);
                        entry.obj = new RndLight().Read(reader, true, this);
                        break;
                    case "UIColor":
                        Debug.WriteLine("Reading entry UIColor" + entry.name.value);
                        entry.obj = new UIColor().Read(reader, true, this);
                        break;
                    case "ParticleSys":
                        Debug.WriteLine("Reading entry ParticleSys " + entry.name.value);
                        entry.obj = new RndParticleSys().Read(reader, true, this);
                        break;
                    case "AnimFilter":
                        Debug.WriteLine("Reading entry AnimFilter " + entry.name.value);
                        entry.obj = new RndAnimFilter().Read(reader, true, this);
                        break;
                    case "BandPlacer":
                        Debug.WriteLine("Reading entry BandPlacer " + entry.name.value);
                        entry.obj = new BandPlacer().Read(reader, true, this);
                        break;
                    case "ScreenMask":
                        Debug.WriteLine("Reading entry ScreenMask " + entry.name.value);
                        entry.obj = new RndScreenMask().Read(reader, true, this);
                        break;
                    case "TexMovie":
                        Debug.WriteLine("Reading entry TexMovie " + entry.name.value);
                        entry.obj = new TexMovie().Read(reader, true, this);
                        break;
                    case "Environ":
                        Debug.WriteLine("Reading entry Environ " + entry.name.value);
                        entry.obj = new RndEnviron().Read(reader, true, this);
                        break;
                    case "SynthSample":
                        Debug.WriteLine("Reading entry SynthSample " + entry.name.value);
                        entry.obj = new SynthSample().Read(reader, true, this);
                        break;

                    default:
                        Debug.WriteLine("Unknown entry type " + entry.type.value + " of name " + entry.name.value + ", read an Object and then read until we see 0xADDEADDE to skip over it, curpos" + reader.BaseStream.Position);

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

            return this;
        }

        public void Write(EndianWriter writer)
        {
            writer.WriteUInt32(revision);

            Symbol.Write(writer, type);
            Symbol.Write(writer, name);

            writer.WriteInt32((entries.Count * 2) + 2);
            writer.WriteUInt32(stringTableSize);

            writer.WriteInt32(entries.Count);

            foreach (Entry entry in entries)
            {
                Symbol.Write(writer, entry.type);
                Symbol.Write(writer, entry.name);
            }

            switch (type.value)
            {
                case "ObjectDir":
                    ((ObjectDir)directory).Write(writer, true);
                    break;
                case "RndDir":
                    ((RndDir)directory).Write(writer, true);
                    break;
                case "PanelDir":
                    ((PanelDir)directory).Write(writer, true);
                    break;
                case "WorldDir":
                    ((WorldDir)directory).Write(writer, true);
                    break;
                case "Character":
                    ((Character)directory).Write(writer, true);
                    break;
                case "P9Character":
                    ((P9Character)directory).Write(writer, true);
                    break;
                case "CharClipSet":
                    ((CharClipSet)directory).Write(writer, true);
                    break;
                case "UILabelDir":
                    ((UILabelDir)directory).Write(writer, true);
                    break;
                case "UIListDir":
                    ((UIListDir)directory).Write(writer, true);
                    break;
                case "BandCrowdMeterDir":
                    ((BandCrowdMeterDir)directory).Write(writer, true);
                    break;
                case "CrowdMeterIcon":
                    ((CrowdMeterIcon)directory).Write(writer, true);
                    break;
                case "BandCharacter":
                    ((BandCharacter)directory).Write(writer, true);
                    break;
                default:
                    throw new Exception("Unknown directory type: " + type.value + ", cannot continue writing Milo scene");
            }

            // write the children entries
            foreach (Entry entry in entries)
            {
                // handle dirty assets
                if (entry.dirty)
                {
                    writer.WriteBlock(entry.objBytes.ToArray());
                    writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
                    continue;
                }

                switch (entry.type.value)
                {
                    case "ObjectDir":
                        ((ObjectDir)entry.obj).Write(writer, false);
                        break;
                    case "RndDir":
                        ((RndDir)entry.obj).Write(writer, false);
                        break;
                    case "PanelDir":
                        ((PanelDir)entry.obj).Write(writer, false);
                        break;
                    case "WorldDir":
                        ((WorldDir)entry.obj).Write(writer, false);
                        break;
                    case "Character":
                        ((Character)entry.obj).Write(writer, false);
                        break;
                    case "P9Character":
                        ((P9Character)entry.obj).Write(writer, false);
                        break;
                    case "CharClipSet":
                        ((CharClipSet)entry.obj).Write(writer, false);
                        break;
                    case "UILabelDir":
                        ((UILabelDir)entry.obj).Write(writer, false);
                        break;
                    case "UIListDir":
                        ((UIListDir)entry.obj).Write(writer, false);
                        break;
                    case "BandCrowdMeterDir":
                        ((BandCrowdMeterDir)entry.obj).Write(writer, false);
                        break;
                    case "BandSongPref":
                        ((BandSongPref)entry.obj).Write(writer, true);
                        break;
                    case "Sfx":
                        ((Sfx)entry.obj).Write(writer, true);
                        break;
                    case "BandCharDesc":
                        ((BandCharDesc)entry.obj).Write(writer, true);
                        break;
                    case "Group":
                        ((RndGroup)entry.obj).Write(writer, true);
                        break;
                    case "ColorPalette":
                        ((ColorPalette)entry.obj).Write(writer, true);
                        break;
                    case "Tex":
                        ((RndTex)entry.obj).Write(writer, true);
                        break;
                    case "Trans":
                        ((RndTrans)entry.obj).Write(writer, true);
                        break;
                    case "Light":
                        ((RndLight)entry.obj).Write(writer, true);
                        break;
                    case "UIColor":
                        ((UIColor)entry.obj).Write(writer, true);
                        break;
                    case "ParticleSys":
                        ((RndParticleSys)entry.obj).Write(writer, true);
                        break;
                    case "AnimFilter":
                        ((RndAnimFilter)entry.obj).Write(writer, true);
                        break;
                    case "BandPlacer":
                        ((BandPlacer)entry.obj).Write(writer, true);
                        break;
                    case "ScreenMask":
                        ((RndScreenMask)entry.obj).Write(writer, true);
                        break;
                    case "TexMovie":
                        ((TexMovie)entry.obj).Write(writer, true);
                        break;
                    case "Environ":
                        ((RndEnviron)entry.obj).Write(writer, true);
                        break;
                    case "SynthSample":
                        ((SynthSample)entry.obj).Write(writer, true);
                        break;
                    //case "Mat":
                    //    ((RndMat)entry.obj).Write(writer, false);
                    //    break;
                    default:
                        Debug.WriteLine("Unknown entry type, dumping raw bytes for " + entry.type.value + " of name " + entry.name.value);

                        // this should allow saving Milos with types that have yet to be implemented
                        writer.WriteBlock(entry.objBytes.ToArray());

                        // write the ending bytes
                        writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
                        break;
                }
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
                    dir.directory = new CrowdMeterIcon(rootDirRevision);
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
