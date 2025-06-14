using MiloLib.Assets.Band;
using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets.Char
{
    [Name("OutfitConfig"), Description("Configurable options for outfits")]
    public class OutfitConfig : Object
    {
        public class Overlay
        {
            public int category;
            public Symbol texture = new(0, "");

            public Overlay Read(EndianReader reader)
            {
                category = reader.ReadInt32();
                texture = Symbol.Read(reader);
                return this;
            }

            public void Write(EndianWriter writer)
            {
                writer.WriteInt32(category);
                Symbol.Write(writer, texture);
            }
        }
        public class Piercing
        {
            public class Piece
            {
                public Symbol attachment = new(0, "");
                public bool highlight;
                public int vert;
                private uint unkShortCount;
                public List<ushort> unkShorts = new();

                public int unkInt1;
                public int unkInt2;

                public bool unkBool1;

                public Matrix xfm = new();

                private uint unkUShortCount;
                public List<ushort> unkUShorts = new();

                public Piece Read(EndianReader reader, uint revision)
                {
                    if (revision > 0xF)
                        vert = reader.ReadInt32();
                    else
                    {
                        unkInt1 = reader.ReadInt32();
                        unkInt2 = reader.ReadInt32();
                    }

                    if (revision < 0xF)
                        unkBool1 = reader.ReadBoolean();

                    if (revision < 0xE)
                    {
                        xfm.Read(reader);
                        unkUShortCount = reader.ReadUInt32();
                        for (int i = 0; i < unkUShortCount; i++)
                            unkUShorts.Add(reader.ReadUInt16());
                    }
                    else
                    {
                        attachment = Symbol.Read(reader);
                    }

                    unkShortCount = reader.ReadUInt32();
                    for (int i = 0; i < unkShortCount; i++)
                        unkShorts.Add(reader.ReadUInt16());

                    return this;
                }

                public void Write(EndianWriter writer, uint revision)
                {
                    if (revision > 0xF)
                        writer.WriteInt32(vert);
                    else
                    {
                        writer.WriteInt32(unkInt1);
                        writer.WriteInt32(unkInt2);
                    }

                    if (revision < 0xF)
                        writer.WriteBoolean(unkBool1);

                    if (revision < 0xE)
                    {
                        xfm.Write(writer);
                        writer.WriteUInt32((uint)unkUShorts.Count);
                        foreach (ushort unkUShort in unkUShorts)
                            writer.WriteUInt16(unkUShort);
                    }
                    else
                    {
                        Symbol.Write(writer, attachment);
                    }

                    writer.WriteUInt32((uint)unkShorts.Count);
                    foreach (ushort unkShort in unkShorts)
                        writer.WriteUInt16(unkShort);
                }
            }

            public Symbol piercing = new(0, "");
            public Matrix transform = new();
            public bool reskin;
            private uint pieceCount;
            public List<Piece> pieces = new();

            public Piercing Read(EndianReader reader, uint revision)
            {
                piercing = Symbol.Read(reader);
                if (revision < 0xD)
                {
                    // TODO: Implement this
                }
                else
                {
                    transform.Read(reader);
                    pieceCount = reader.ReadUInt32();
                    pieces = new List<Piece>();
                    for (int i = 0; i < pieceCount; i++)
                        pieces.Add(new Piece().Read(reader, revision));

                    if (revision > 0x1A)
                        reskin = reader.ReadBoolean();
                }
                return this;
            }

            public void Write(EndianWriter writer, uint revision)
            {
                Symbol.Write(writer, piercing);
                if (revision < 0xD)
                {
                    // TODO: Implement this
                }
                else
                {
                    transform.Write(writer);
                }
                transform.Write(writer);
                writer.WriteUInt32((uint)pieces.Count);
                foreach (Piece piece in pieces)
                    piece.Write(writer, revision);

                if (revision > 0x1A)
                    writer.WriteBoolean(reskin);
            }
        }
        public class MeshAO
        {
            public class Seam
            {
                int idx;
                int coefficient;
                public Seam Read(EndianReader reader)
                {
                    idx = reader.ReadInt32();
                    coefficient = reader.ReadInt32();
                    return this;
                }

                public void Write(EndianWriter writer)
                {
                    writer.WriteInt32(idx);
                    writer.WriteInt32(coefficient);
                }
            }

            public Symbol meshName = new(0, "");
            private uint coefficientCount;
            public List<int> coefficients = new();
            private uint seamCount;
            public List<Seam> seams = new();

            public byte[] sha1Digest = new byte[20];

            public Symbol unkSym = new(0, "");

            public MeshAO Read(EndianReader reader, uint revision)
            {
                meshName = Symbol.Read(reader);
                if (revision == 9 || revision == 10 || revision == 11 || revision == 12 || revision == 13 || revision == 14 || revision == 15 || revision == 16 || revision == 17 || revision == 18 || revision == 19 || revision == 20 || revision == 21 || revision == 22)
                {
                    sha1Digest = reader.ReadBlock(20);
                }
                coefficientCount = reader.ReadUInt32();
                coefficients = new List<int>();
                for (int i = 0; i < coefficientCount; i++)
                    coefficients.Add(reader.ReadInt32());
                seamCount = reader.ReadUInt32();
                seams = new List<Seam>();
                for (int i = 0; i < seamCount; i++)
                    seams.Add(new Seam().Read(reader));
                if (revision > 0x18)
                    unkSym = Symbol.Read(reader);
                return this;
            }

            public void Write(EndianWriter writer, uint revision)
            {
                Symbol.Write(writer, meshName);
                if (revision == 9 || revision == 10 || revision == 11 || revision == 12 || revision == 13 || revision == 14 || revision == 15 || revision == 16 || revision == 17 || revision == 18 || revision == 19 || revision == 20 || revision == 21 || revision == 22)
                {
                    writer.WriteBlock(sha1Digest);
                }
                writer.WriteUInt32((uint)coefficients.Count);
                foreach (int coefficient in coefficients)
                    writer.WriteInt32(coefficient);
                writer.WriteUInt32((uint)seams.Count);
                foreach (Seam seam in seams)
                    seam.Write(writer);
                if (revision > 0x18)
                    Symbol.Write(writer, unkSym);
            }
        }

        public class MatSwap
        {
            public Symbol mat = new(0, "");
            public Symbol resourceMat = new(0, "");
            public Symbol twoColorDiffuse = new(0, "");
            public Symbol twoColorInterp = new(0, "");
            public Symbol twoColorMask = new(0, "");
            public Symbol color1Palette = new(0, "");
            int color1Option;
            public Symbol color2Palette = new(0, "");
            int color2Option;
            private uint texturesCount;
            public List<Symbol> textures = new();

            public MatSwap Read(EndianReader reader, uint revision)
            {
                mat = Symbol.Read(reader);
                resourceMat = Symbol.Read(reader);
                if (revision < 5)
                {
                    // TODO: Implement this
                }
                else
                {
                    twoColorDiffuse = Symbol.Read(reader);
                    twoColorInterp = Symbol.Read(reader);
                }
                twoColorMask = Symbol.Read(reader);
                if (revision > 4)
                {
                    color1Palette = Symbol.Read(reader);
                    color1Option = reader.ReadInt32();
                    color2Palette = Symbol.Read(reader);
                    color2Option = reader.ReadInt32();
                    texturesCount = reader.ReadUInt32();
                    textures = new List<Symbol>();
                    for (int i = 0; i < texturesCount; i++)
                        textures.Add(Symbol.Read(reader));
                }
                return this;
            }

            public void Write(EndianWriter writer, uint revision)
            {
                Symbol.Write(writer, mat);
                Symbol.Write(writer, resourceMat);
                if (revision < 5)
                {
                    // TODO: Implement this
                }
                else
                {
                    Symbol.Write(writer, twoColorDiffuse);
                    Symbol.Write(writer, twoColorInterp);
                }
                Symbol.Write(writer, twoColorMask);
                if (revision > 4)
                {
                    Symbol.Write(writer, color1Palette);
                    writer.WriteInt32(color1Option);
                    Symbol.Write(writer, color2Palette);
                    writer.WriteInt32(color2Option);
                    writer.WriteUInt32((uint)textures.Count);
                    foreach (Symbol texture in textures)
                        Symbol.Write(writer, texture);
                }
            }

            public override string ToString()
            {
                return $"Mat: {mat} Resource Mat: {resourceMat} {twoColorDiffuse} {twoColorInterp} {twoColorMask} {color1Palette} {color1Option} {color2Palette} {color2Option} {texturesCount}";
            }
        }
        private ushort altRevision;
        private ushort revision;

        private int[] colors = new int[3];

        public bool computeAO;

        private uint matSwapCount;
        public List<MatSwap> matSwaps = new();

        private uint meshAOCount;
        public List<MeshAO> meshAOs = new();

        private uint bandPatchMeshCount;
        public List<BandPatchMesh> bandPatchMeshes = new();

        private uint piercingCount;
        public List<Piercing> piercings = new();

        public Symbol texBlender = new(0, "");
        public Symbol wrinkleBlender = new(0, "");

        public Symbol bandLogo = new(0, "");

        private uint overlaysCount;
        public List<Overlay> overlays = new();

        public byte[] sha1Digest = new byte[20];






        public OutfitConfig Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)(combinedRevision >> 16 & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)(combinedRevision >> 16 & 0xFFFF));

            base.Read(reader, false, parent, entry);

            if (revision > 4)
            {
                colors[0] = reader.ReadInt32();
                colors[1] = reader.ReadInt32();
                if (revision > 10)
                    colors[2] = reader.ReadInt32();
            }

            if (revision > 3)
            {
                matSwapCount = reader.ReadUInt32();
                for (int i = 0; i < matSwapCount; i++)
                    matSwaps.Add(new MatSwap().Read(reader, revision));
            }

            meshAOCount = reader.ReadUInt32();
            for (int i = 0; i < meshAOCount; i++)
                meshAOs.Add(new MeshAO().Read(reader, revision));
            computeAO = reader.ReadBoolean();
            bandPatchMeshCount = reader.ReadUInt32();
            for (int i = 0; i < bandPatchMeshCount; i++)
                bandPatchMeshes.Add(new BandPatchMesh().Read(reader, false, parent, entry));

            piercingCount = reader.ReadUInt32();
            for (int i = 0; i < piercingCount; i++)
                piercings.Add(new Piercing().Read(reader, revision));

            texBlender = Symbol.Read(reader);

            overlaysCount = reader.ReadUInt32();
            for (int i = 0; i < overlaysCount; i++)
                overlays.Add(new Overlay().Read(reader));

            bandLogo = Symbol.Read(reader);

            sha1Digest = reader.ReadBlock(20);

            wrinkleBlender = Symbol.Read(reader);

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw new Exception("Got to end of standalone asset but didn't find the expected end bytes, read likely did not succeed");

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)(altRevision << 16 | revision) : (uint)(revision << 16 | altRevision));

            base.Write(writer, false, parent, entry);

            if (revision > 4)
            {
                writer.WriteInt32(colors[0]);
                writer.WriteInt32(colors[1]);
                if (revision > 10)
                    writer.WriteInt32(colors[2]);
            }

            if (revision > 3)
            {
                writer.WriteUInt32((uint)matSwaps.Count);
                foreach (MatSwap matSwap in matSwaps)
                    matSwap.Write(writer, revision);
            }

            writer.WriteUInt32((uint)meshAOs.Count);
            foreach (MeshAO meshAO in meshAOs)
                meshAO.Write(writer, revision);
            writer.WriteBoolean(computeAO);
            writer.WriteUInt32((uint)bandPatchMeshes.Count);
            foreach (BandPatchMesh bandPatchMesh in bandPatchMeshes)
                bandPatchMesh.Write(writer, false, parent, entry);

            writer.WriteUInt32((uint)piercings.Count);
            foreach (Piercing piercing in piercings)
                piercing.Write(writer, revision);

            Symbol.Write(writer, texBlender);

            writer.WriteUInt32((uint)overlays.Count);
            foreach (Overlay overlay in overlays)
                overlay.Write(writer);

            Symbol.Write(writer, bandLogo);

            writer.WriteBlock(sha1Digest);

            Symbol.Write(writer, wrinkleBlender);

            if (standalone)
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
        }

    }
}
