using MiloLib.Assets.Rnd;
using MiloLib.Classes;
using MiloLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiloLib.Assets.Char
{
    [Name("Character"), Description("Base class for Character objects. Contains Geometry, Outfit Loaders, and LOD + Sphere concepts.")]
    public class Character : RndDir
    {

        public class LOD
        {
            public float screenSize;
            public Symbol group = new(0, "");
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
                }
            }
        }

        public class CharacterTesting
        {
            public ushort altRevision;
            public ushort revision;
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
            public uint transition;
            public bool cycleTransition;
            public bool unkBool2;
            public bool unkBool3;
            public bool unkBool4;
            public bool unkBool5;
            public bool unkBool6;
            public uint unk3;
            public bool unk4;
            public uint unk5;
            public uint unk6;
            public uint bpm;
            public float unkFloat;
            public Symbol unkSymbol = new(0, "");
            public Symbol unkSymbol2 = new(0, "");

            public CharacterTesting Read(EndianReader reader, DirectoryMeta parent, DirectoryMeta.Entry entry)
            {
                altRevision = reader.ReadUInt16();
                revision = reader.ReadUInt16();
                driver = Symbol.Read(reader);
                clip1 = Symbol.Read(reader);
                clip2 = Symbol.Read(reader);
                teleportTo = Symbol.Read(reader);
                teleportFrom = Symbol.Read(reader);
                distMap = Symbol.Read(reader);

                if (revision <= 6)
                {
                    return this;
                }

                transition = reader.ReadUInt32();
                cycleTransition = reader.ReadBoolean();
                unk3 = reader.ReadUInt32();

                if (revision == 15)
                {
                    unk5 = reader.ReadUInt32();
                }

                if (revision == 8)
                {

                    unk6 = reader.ReadUInt32();

                    unkBool2 = reader.ReadBoolean();
                    unkBool3 = reader.ReadBoolean();
                    unkBool4 = reader.ReadBoolean();

                    unkSymbol = Symbol.Read(reader);

                    unkBool5 = reader.ReadBoolean();
                    unkBool6 = reader.ReadBoolean();

                    bpm = reader.ReadUInt32();

                    unkSymbol2 = Symbol.Read(reader);

                    unkFloat = reader.ReadFloat();
                }
                return this;
            }

            public void Write(EndianWriter writer)
            {
                writer.WriteUInt16(altRevision);
                writer.WriteUInt16(revision);

                Symbol.Write(writer, driver);
                Symbol.Write(writer, clip1);
                Symbol.Write(writer, clip2);
                Symbol.Write(writer, teleportTo);
                Symbol.Write(writer, teleportFrom);
                Symbol.Write(writer, distMap);

                if (revision <= 6)
                {
                    return;
                }

                writer.WriteUInt32(transition);
                writer.WriteBoolean(cycleTransition);
                writer.WriteUInt32(unk3);

                if (revision == 15)
                {
                    writer.WriteBoolean(unk4);
                    writer.WriteUInt32(unk5);
                }

                if (revision == 8)
                {
                    writer.WriteUInt32(unk6);

                    writer.WriteBoolean(unkBool2);
                    writer.WriteBoolean(unkBool3);
                    writer.WriteBoolean(unkBool4);

                    Symbol.Write(writer, unkSymbol);

                    writer.WriteBoolean(unkBool5);
                    writer.WriteBoolean(unkBool6);

                    writer.WriteUInt32(bpm);

                    Symbol.Write(writer, unkSymbol2);

                    writer.WriteFloat(unkFloat);
                }
            }
        }

        public ushort altRevision;
        public ushort revision;

        private uint lodCount;
        public List<LOD> lods = new();

        private uint shadowCount;
        public List<Symbol> shadows = new();

        public bool selfShadow;

        public Symbol sphereBase = new(0, "");

        public Sphere bounding = new();

        public bool frozen;

        [Name("Minimum LOD"), Description("Forces LOD, kLODPerFrame is normal behavior of picking per frame, the others force the lod (0 is highest res lod, 2 is lowest res lod)")]
        public int minLod;

        public Symbol translucentGroup = new(0, "");

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
            if (revision < 4 || !entry.isDir)
            {
                lodCount = reader.ReadUInt32();
                for (int i = 0; i < lodCount; i++)
                {
                    LOD lod = new();
                    lod.Read(reader, revision);
                    lods.Add(lod);
                }

                if (revision < 18)
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
            }

            charTest = new CharacterTesting().Read(reader, parent, entry);

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw new Exception("Got to end of standalone asset but didn't find the expected end bytes, read likely did not succeed");

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            base.Write(writer, false);

            writer.WriteUInt32((uint)lods.Count);
            foreach (var lod in lods)
            {
                lod.Write(writer, revision);
            }

            if (revision < 18)
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

            if (standalone)
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
        }




        public override bool IsDirectory()
        {
            return true;
        }
    }
}
