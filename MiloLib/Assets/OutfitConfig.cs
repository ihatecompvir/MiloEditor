using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets
{
    [Name("OutfitConfig"), Description("Configurable options for outfits")]
    public class OutfitConfig : Object
    {
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
            public Symbol unkSym = new(0, "");
            private uint coefficientCount;
            public List<int> coefficients;
            private uint seamCount;
            public List<Seam> seams;

            public MeshAO Read(EndianReader reader)
            {
                meshName = Symbol.Read(reader);
                unkSym = Symbol.Read(reader);
                coefficientCount = reader.ReadUInt32();
                coefficients = new List<int>();
                for (int i = 0; i < coefficientCount; i++)
                    coefficients.Add(reader.ReadInt32());
                seamCount = reader.ReadUInt32();
                seams = new List<Seam>();
                for (int i = 0; i < seamCount; i++)
                    seams.Add(new Seam().Read(reader));
                return this;
            }

            public void Write(EndianWriter writer)
            {
                Symbol.Write(writer, meshName);
                Symbol.Write(writer, unkSym);
                writer.WriteUInt32((uint)coefficients.Count);
                foreach (int coefficient in coefficients)
                    writer.WriteInt32(coefficient);
                writer.WriteUInt32((uint)seams.Count);
                foreach (Seam seam in seams)
                    seam.Write(writer);
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
            bool twoColor;

            public MatSwap Read(EndianReader reader)
            {
                mat = Symbol.Read(reader);
                resourceMat = Symbol.Read(reader);
                twoColorDiffuse = Symbol.Read(reader);
                twoColorInterp = Symbol.Read(reader);
                twoColorMask = Symbol.Read(reader);
                color1Palette = Symbol.Read(reader);
                color1Option = reader.ReadInt32();
                color2Palette = Symbol.Read(reader);
                color2Option = reader.ReadInt32();
                texturesCount = reader.ReadUInt32();
                textures = new List<Symbol>();
                for (int i = 0; i < texturesCount; i++)
                    textures.Add(Symbol.Read(reader));
                twoColor = reader.ReadBoolean();
                return this;
            }

            public void Write(EndianWriter writer)
            {
                Symbol.Write(writer, mat);
                Symbol.Write(writer, resourceMat);
                Symbol.Write(writer, twoColorDiffuse);
                Symbol.Write(writer, twoColorInterp);
                Symbol.Write(writer, twoColorMask);
                Symbol.Write(writer, color1Palette);
                writer.WriteInt32(color1Option);
                Symbol.Write(writer, color2Palette);
                writer.WriteInt32(color2Option);
                writer.WriteUInt32((uint)textures.Count);
                foreach (Symbol texture in textures)
                    Symbol.Write(writer, texture);
                writer.WriteBoolean(twoColor);
            }
        }
        private ushort altRevision;
        private ushort revision;

        private int[] colors = new int[3];

        public bool computeAO;




        public OutfitConfig Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

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
                MatSwap matSwap = new MatSwap().Read(reader);
            }



            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw new Exception("Got to end of standalone asset but didn't find the expected end bytes, read likely did not succeed");

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            base.Write(writer, false, parent, entry);

            if (standalone)
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
        }

    }
}
