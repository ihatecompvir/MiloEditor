using MiloLib.Assets.Band;
using MiloLib.Assets.Char;
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

        private uint version;

        public Symbol type = new(0, "");
        public Symbol name = new(0, "");

        private uint stringTableCount;
        private uint stringTableSize;

        private uint externalResourceCount;
        private List<Symbol> externalResources = new List<Symbol>();

        private uint entryCount;
        public List<Entry> entries = new List<Entry>();

        public Object dirObj;

        public DirectoryMeta Read(EndianReader reader)
        {
            version = reader.ReadUInt32();

            // if the version is over 50, switch to little endian and attempt the read again to guess endianness
            // this works well since the highest known version before switch to Forge was 32
            if (version > 50)
            {
                reader.Endianness = Endian.LittleEndian;
                reader.SeekTo(0);
                version = reader.ReadUInt32();
            }

            // support freq<-->dc3 versions
            if (version != 6 && version != 10 && version != 24 && version != 25 && version != 26 && version != 28 && version != 32)
            {
                throw new UnsupportedMiloSceneRevision(version);
            }

            type = Symbol.Read(reader);
            name = Symbol.Read(reader);

            stringTableCount = reader.ReadUInt32();
            stringTableSize = reader.ReadUInt32();

            entryCount = reader.ReadUInt32();

            for (int i = 0; i < entryCount; i++)
            {
                Entry entry = new Entry(Symbol.Read(reader), Symbol.Read(reader), null);
                entries.Add(entry);
            }

            // only gh1-era stuff seems to have this
            if (version == 10)
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
                    ObjectDir objectDir = new ObjectDir();
                    objectDir.Read(reader, true);
                    dirObj = objectDir;
                    break;
                case "RndDir":
                    Debug.WriteLine("Reading RndDir " + name.value);
                    RndDir rndDir = new RndDir();
                    rndDir.Read(reader, true);
                    dirObj = rndDir;
                    break;
                case "PanelDir":
                    Debug.WriteLine("Reading PanelDir " + name.value);
                    PanelDir panelDir = new PanelDir();
                    panelDir.Read(reader, true);
                    dirObj = panelDir;
                    break;
                case "CharClipSet":
                    Debug.WriteLine("Reading CharClipSet " + name.value);
                    CharClipSet charClipSet = new CharClipSet();
                    charClipSet.Read(reader, true);
                    dirObj = charClipSet;
                    break;
                case "WorldDir":
                    Debug.WriteLine("Reading WorldDir " + name.value);
                    WorldDir worldDir = new WorldDir();
                    worldDir.Read(reader, true);
                    dirObj = worldDir;
                    break;
                case "Character":
                    Debug.WriteLine("Reading Character " + name.value);
                    Character character = new Character();
                    character.Read(reader, true);
                    dirObj = character;
                    break;
                case "UILabelDir":
                    Debug.WriteLine("Reading UILabelDir " + name.value);
                    UILabelDir uiLabelDir = new UILabelDir();
                    uiLabelDir.Read(reader, true);
                    dirObj = uiLabelDir;
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
                        entry.obj = new ObjectDir().Read(reader, true);

                        entry.dir = new DirectoryMeta().Read(reader);
                        break;
                    case "RndDir":
                        Debug.WriteLine("Reading entry RndDir " + entry.name.value);
                        entry.obj = new RndDir().Read(reader, true);

                        entry.dir = new DirectoryMeta().Read(reader);
                        break;

                    case "PanelDir":
                        Debug.WriteLine("Reading entry PanelDir " + entry.name.value);
                        entry.obj = new PanelDir().Read(reader, true);

                        entry.dir = new DirectoryMeta().Read(reader);
                        break;

                    case "WorldDir":
                        Debug.WriteLine("Reading entry WorldDir " + entry.name.value);
                        entry.obj = new WorldDir().Read(reader, true);

                        entry.dir = new DirectoryMeta().Read(reader);
                        break;

                    case "Character":
                        Debug.WriteLine("Reading entry Character " + entry.name.value);
                        entry.obj = new Character().Read(reader, true);

                        entry.dir = new DirectoryMeta().Read(reader);
                        break;

                    case "CharClipSet":
                        Debug.WriteLine("Reading entry CharClipSet " + entry.name.value);
                        entry.obj = new CharClipSet().Read(reader, true);

                        entry.dir = new DirectoryMeta().Read(reader);
                        break;

                    // OBJECTS

                    case "Object":
                        Debug.WriteLine("Reading entry Object " + entry.name.value);
                        entry.obj = new Object().Read(reader, true);
                        break;
                    case "BandSongPref":
                        Debug.WriteLine("Reading entry BandSongPref " + entry.name.value);
                        entry.obj = new BandSongPref().Read(reader, true);
                        break;
                    case "Sfx":
                        Debug.WriteLine("Reading entry Sfx " + entry.name.value);
                        entry.obj = new Sfx().Read(reader, true);
                        break;
                    case "Trans":
                        Debug.WriteLine("Reading entry Trans " + entry.name.value);
                        entry.obj = new RndTrans().Read(reader, true);
                        break;
                    case "Group":
                        Debug.WriteLine("Reading entry Group " + entry.name.value);
                        entry.obj = new RndGroup().Read(reader, true);
                        break;
                    // TODO: figure out how to read textures properly
                    //case "Tex":
                    //    Debug.WriteLine("Reading entry Tex " + entry.name.value);
                    //    entry.obj = new RndTex().Read(reader, true);
                    //    break;
                    case "ColorPalette":
                        Debug.WriteLine("Reading entry ColorPalette " + entry.name.value);
                        entry.obj = new ColorPalette().Read(reader, true);
                        break;
                    //case "Mat":
                    //    Debug.WriteLine("Reading entry Mat " + entry.name.value);
                    //    entry.obj = new RndMat().Read(reader, true);
                    //    break;
                    case "BandCharDesc":
                        Debug.WriteLine("Reading entry BandCharDesc " + entry.name.value);
                        entry.obj = new BandCharDesc().Read(reader, true);
                        break;
                    case "Light":
                        Debug.WriteLine("Reading entry Light " + entry.name.value);
                        entry.obj = new RndLight().Read(reader, true);
                        break;
                    case "UIColor":
                        Debug.WriteLine("Reading entry UIColor" + entry.name.value);
                        entry.obj = new UIColor().Read(reader, true);
                        break;
                    case "ParticleSys":
                        Debug.WriteLine("Reading entry ParticleSys " + entry.name.value);
                        entry.obj = new RndParticleSys().Read(reader, true);
                        break;
                    default:
                        Debug.WriteLine("Unknown entry type " + entry.type.value + " of name " + entry.name.value + ", read an Object and then read until we see 0xADDEADDE to skip over it, curpos" + reader.BaseStream.Position);

                        // read revision and then an empty object
                        // this allows the editor to display at least *some* fields on every object
                        reader.ReadUInt32();
                        entry.obj = new Object().Read(reader, false);

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
            writer.WriteUInt32(version);

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
                    ((ObjectDir)dirObj).Write(writer, true);
                    break;
                case "RndDir":
                    ((RndDir)dirObj).Write(writer, true);
                    break;
                case "PanelDir":
                    ((PanelDir)dirObj).Write(writer, true);
                    break;
                case "WorldDir":
                    ((WorldDir)dirObj).Write(writer, true);
                    break;
                case "Character":
                    ((Character)dirObj).Write(writer, true);
                    break;
                case "CharClipSet":
                    ((CharClipSet)dirObj).Write(writer, true);
                    break;
                case "UILabelDir":
                    ((UILabelDir)dirObj).Write(writer, true);
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
                    case "CharClipSet":
                        ((CharClipSet)entry.obj).Write(writer, false);
                        break;
                    case "UILabelDir":
                        ((UILabelDir)entry.obj).Write(writer, false);
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

        public static DirectoryMeta New(string type, string name)
        {
            DirectoryMeta dir = new DirectoryMeta();
            dir.type = type;
            dir.name = name;
            // switch on type name
            switch (dir.type)
            {
                case "ObjectDir":
                    dir.dirObj = new ObjectDir();
                    break;
                case "RndDir":
                    dir.dirObj = new RndDir();
                    break;
                case "PanelDir":
                    dir.dirObj = new PanelDir();
                    break;
                case "WorldDir":
                    dir.dirObj = new WorldDir();
                    break;
                case "Character":
                    dir.dirObj = new Character();
                    break;
                case "CharClipSet":
                    dir.dirObj = new CharClipSet();
                    break;
                default:
                    throw new Exception("Unknown directory type: " + type.GetType().Name + ", cannot continue creating directory");
            }

            dir.entries = new List<Entry>();

            dir.version = 0x1c;
            return dir;
        }

        public override string ToString()
        {
            return string.Format("{1} ({0})", type, name);
        }
    }
}
