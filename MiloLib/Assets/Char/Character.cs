using MiloLib.Assets.Rnd;
using MiloLib.Classes;
using MiloLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MiloLib.Assets.DirectoryMeta;

namespace MiloLib.Assets.Char
{
    [Name("Character"), Description("Base class for Character perObjs. Contains Geometry, Outfit Loaders, and LOD + Sphere concepts.")]
    public class Character : RndDir
    {
        private Dictionary<Game.MiloGame, uint> gameRevisions = new Dictionary<Game.MiloGame, uint>
        {
            // no dirs before this
            { Game.MiloGame.GuitarHero2_PS2, 9 },
            { Game.MiloGame.GuitarHero2_360, 10 },
            { Game.MiloGame.RockBand, 12 },
            { Game.MiloGame.RockBand2, 12 },
            { Game.MiloGame.LegoRockBand, 12 },
            { Game.MiloGame.TheBeatlesRockBand, 15 },
            { Game.MiloGame.GreenDayRockBand, 15 },
            { Game.MiloGame.RockBand3, 18 },
            { Game.MiloGame.DanceCentral, 18 },
            { Game.MiloGame.DanceCentral2, 18 },
            { Game.MiloGame.RockBandBlitz, 21 },
            { Game.MiloGame.DanceCentral3, 21 }
        };

        public class LOD
        {
            public float screenSize;

            [Name("Group"), Description("group to show at this LOD.  Drawables not in any lod group will be drawn at every LOD")]
            public Symbol group = new(0, "");
            [Name("Transparency Group"), Description("translucency group to show at this LOD.  Drawables in it are guaranteed to be drawn last.")]
            public Symbol transGroup = new(0, "");

            private uint opaqueCount;
            public List<Symbol> opaque = new();

            private uint translucentCount;
            public List<Symbol> translucent = new();

            public LOD Read(EndianReader reader, uint revision)
            {
                screenSize = reader.ReadFloat();
                if (revision < 18)
                {
                    group = Symbol.Read(reader);
                    if (revision >= 15)
                        transGroup = Symbol.Read(reader);
                }
                else
                {
                    opaqueCount = reader.ReadUInt32();
                    for (int i = 0; i < opaqueCount; i++)
                    {
                        opaque.Add(Symbol.Read(reader));
                    }

                    translucentCount = reader.ReadUInt32();
                    for (int i = 0; i < translucentCount; i++)
                    {
                        translucent.Add(Symbol.Read(reader));
                    }
                }
                return this;
            }

            public void Write(EndianWriter writer, uint revision)
            {
                writer.WriteFloat(screenSize);
                if (revision < 18)
                {
                    Symbol.Write(writer, group);
                    if (revision >= 15)
                        Symbol.Write(writer, transGroup);
                }
                else
                {
                    writer.WriteUInt32((uint)opaque.Count);
                    foreach (var op in opaque)
                    {
                        Symbol.Write(writer, op);
                    }

                    writer.WriteUInt32((uint)translucent.Count);
                    foreach (var trans in translucent)
                    {
                        Symbol.Write(writer, trans);
                    }
                }
            }

            public override string ToString()
            {
                return $"{group} {transGroup} opaque: {opaqueCount} translucent: {translucentCount}";
            }
        }

        public class CharacterTesting
        {
            private ushort altRevision;
            private ushort revision;
            [Name("Driver"), Description("The driver to animate")]
            public Symbol driver = new(0, "");
            [Name("Clip 1"), Description("Clip to play")]
            public Symbol clip1 = new(0, "");
            [Name("Clip 2"), Description("Clip to transition to, if any")]
            public Symbol clip2 = new(0, "");
            [Name("Teleport To"), Description("Teleport to this Waypoint")]
            public Symbol teleportTo = new(0, "");
            public Symbol teleportFrom = new(0, "");
            [Name("Distance Map"), Description("Displays the transition distance map between clip1 and clip2, raw means the raw graph, no nodes")]
            public Symbol distMap = new(0, "");
            [Name("Transition"), Description("Which transition to use between clip1 and clip2")]
            public uint transition;
            [Name("Cycle Transition"), Description("Cycle through all the transitions")]
            public bool cycleTransition;
            [Name("Internal Transition")]
            public uint internalTransition;

            [MaxVersion(9)]
            public uint unk1;

            [Name("Metronome"), Description("Click on every beat transition")]
            public bool metronome;
            [Name("Zero Travel"), Description("Character does not travel, constantly zeros out position and facing")]
            public bool zeroTravel;
            [Name("Show Screen Size"), Description("graphically displays the screensize and lod next to the character")]
            public bool showScreenSize;
            public bool footExtents;

            [MaxVersion(0xD)]
            public bool clip2RealTime;

            [MaxVersion(13)]
            public uint unk2;

            [MaxVersion(13), MinVersion(10)]
            public Symbol unkSymbol2 = new(0, "");

            [MaxVersion(0xD)]
            public uint bpm;

            [MaxVersion(13)]
            public float unkFloat;

            [MaxVersion(0xB)]
            public Symbol unkSymbol = new(0, "");


            public CharacterTesting Read(EndianReader reader, DirectoryMeta parent, DirectoryMeta.Entry entry)
            {
                uint combinedRevision = reader.ReadUInt32();
                if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
                else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
                driver = Symbol.Read(reader);
                clip1 = Symbol.Read(reader);
                clip2 = Symbol.Read(reader);
                teleportTo = Symbol.Read(reader);
                teleportFrom = Symbol.Read(reader);
                distMap = Symbol.Read(reader);

                if (revision < 6)
                {
                    return this;
                }

                transition = reader.ReadUInt32();
                cycleTransition = reader.ReadBoolean();
                internalTransition = reader.ReadUInt32();

                if (revision < 10)
                {
                    unk1 = reader.ReadUInt32();
                }

                metronome = reader.ReadBoolean();
                zeroTravel = reader.ReadBoolean();
                showScreenSize = reader.ReadBoolean();

                if (revision < 0xC)
                {
                    unkSymbol = Symbol.Read(reader);
                }

                footExtents = reader.ReadBoolean();

                if (revision < 15)
                {
                    clip2RealTime = reader.ReadBoolean();
                    bpm = reader.ReadUInt32();
                }

                if (revision == 6)
                {
                    return this;
                }

                if (revision < 14)
                {
                    unk2 = reader.ReadUInt32();
                    unkFloat = reader.ReadFloat();
                }

                // not really sure if this is the exact ranges but RB2 (8) doesn't have this field while TBRB (10) does, and bank5 (14) doesn't again
                if (revision < 14 && revision > 9)
                {
                    unkSymbol2 = Symbol.Read(reader);
                }

                return this;
            }

            public void Write(EndianWriter writer)
            {
                writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

                Symbol.Write(writer, driver);
                Symbol.Write(writer, clip1);
                Symbol.Write(writer, clip2);
                Symbol.Write(writer, teleportTo);
                Symbol.Write(writer, teleportFrom);
                Symbol.Write(writer, distMap);

                if (revision < 6)
                {
                    return;
                }

                writer.WriteUInt32(transition);
                writer.WriteBoolean(cycleTransition);
                writer.WriteUInt32(internalTransition);

                if (revision < 10)
                {
                    writer.WriteUInt32(unk1);
                }

                writer.WriteBoolean(metronome);
                writer.WriteBoolean(zeroTravel);
                writer.WriteBoolean(showScreenSize);

                if (revision < 0xC)
                {
                    Symbol.Write(writer, unkSymbol);
                }

                writer.WriteBoolean(footExtents);

                if (revision < 15)
                {
                    writer.WriteBoolean(clip2RealTime);
                    writer.WriteUInt32(bpm);
                }

                if (revision == 6)
                {
                    return;
                }

                if (revision < 14)
                {
                    writer.WriteUInt32(unk2);
                    writer.WriteFloat(unkFloat);
                }

                // not really sure if this is the exact ranges but RB2 (8) doesn't have this field while TBRB (10) does, and bank5 (14) doesn't again
                if (revision < 14 && revision > 9)
                {
                    Symbol.Write(writer, unkSymbol2);
                }

            }
        }

        private ushort altRevision;
        private ushort revision;

        private uint lodCount;
        [Name("LODs"), Description("List of LODs for the character")]
        public List<LOD> lods = new();

        private uint shadowCount;
        [Name("Shadows"), Description("Group containing shadow geometry")]
        public List<Symbol> shadows = new();

        [Name("Self Shadow"), Description("Whether this character should be self-shadowed."), MinVersion(3)]
        public bool selfShadow;

        [Name("Sphere Base"), Description("Base for bounding sphere, such as bone_pelvis.mesh"), MinVersion(5)]
        public Symbol sphereBase = new(0, "");

        [Name("Bounding Sphere"), Description("bounding sphere for the character, fixed"), MinVersion(11)]
        public Sphere bounding = new();

        [Name("Frozen"), Description("if true, is frozen in place, no polling happens"), MinVersion(0xD)]
        public bool frozen;

        [Name("Minimum LOD"), Description("Forces LOD, kLODPerFrame is normal behavior of picking per frame, the others force the lod (0 is highest res lod, 2 is lowest res lod)"), MinVersion(0xF)]
        public int minLod;

        [Name("Translucency Group"), Description("translucency group to show independent of lod.  Drawables in it are guaranteed to be drawn last."), MinVersion(0x11)]
        public Symbol translucentGroup = new(0, "");

        [Name("Character Test"), Description("Test Character by animating it")]
        public CharacterTesting charTest = new();

        public Character(ushort revision, ushort altRevision = 0) : base(revision, altRevision)
        {
            revision = revision;
            altRevision = altRevision;
            return;
        }

        public Character Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            base.Read(reader, false, parent, entry);

            // these fields only present if dir is not proxied
            if (revision < 4 || !entry.isProxy)
            {
                lodCount = reader.ReadUInt32();
                for (int i = 0; i < lodCount; i++)
                {
                    LOD lod = new();
                    lod.Read(reader, revision);
                    lods.Add(lod);
                }

                if (revision < 17)
                {
                    shadowCount = 1;
                    shadows.Add(Symbol.Read(reader));
                }
                else
                {
                    shadowCount = reader.ReadUInt32();
                    for (int i = 0; i < shadowCount; i++)
                    {
                        shadows.Add(Symbol.Read(reader));
                    }
                }

                if (revision > 2)
                    selfShadow = reader.ReadBoolean();

                if (revision > 4)
                    sphereBase = Symbol.Read(reader);

                if (revision <= 9)
                    return this;

                if (revision > 10)
                {
                    bounding = new();
                    bounding.Read(reader);
                }

                if (revision > 0xC)
                    frozen = reader.ReadBoolean();

                if (revision > 0xE)
                    minLod = reader.ReadInt32();

                if (revision > 0x10)
                    translucentGroup = Symbol.Read(reader);

                charTest = new CharacterTesting().Read(reader, parent, entry);
            }
            else if (revision > 0xF)
            {
                charTest = new CharacterTesting().Read(reader, parent, entry);
            }

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw new Exception("Got to end of standalone asset but didn't find the expected end bytes, read likely did not succeed");

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            base.Write(writer, false, parent, entry);


            if (revision < 4 || !entry.isProxy)
            {
                writer.WriteUInt32((uint)lods.Count);
                foreach (var lod in lods)
                {
                    lod.Write(writer, revision);
                }

                if (revision < 17)
                {
                    Symbol.Write(writer, shadows[0]);
                }
                else
                {
                    writer.WriteUInt32((uint)shadows.Count);
                    foreach (var shadow in shadows)
                    {
                        Symbol.Write(writer, shadow);
                    }
                }

                if (revision > 2)
                    writer.WriteBoolean(selfShadow);

                if (revision > 4)
                    Symbol.Write(writer, sphereBase);

                if (revision <= 9)
                    return;

                if (revision > 10)
                    bounding.Write(writer);

                if (revision > 0xC)
                    writer.WriteBoolean(frozen);

                if (revision > 0xE)
                    writer.WriteInt32(minLod);

                if (revision > 0x10)
                    Symbol.Write(writer, translucentGroup);

                charTest.Write(writer);
            }
            else if (revision > 0xF)
            {
                charTest.Write(writer);
            }


            if (standalone)
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
        }




        public override bool IsDirectory()
        {
            return true;
        }
    }
}
