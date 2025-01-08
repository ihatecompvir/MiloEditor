using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets.Band
{
    public class BandPatchMesh
    {


        public class MeshPair
        {
            public class PatchPair
            {
                public Symbol patch = new(0, "");
                public Symbol tex = new(0, "");

                public PatchPair Read(EndianReader reader)
                {
                    patch = Symbol.Read(reader);
                    tex = Symbol.Read(reader);
                    return this;
                }

                public void Write(EndianWriter writer)
                {
                    Symbol.Write(writer, patch);
                    Symbol.Write(writer, tex);
                }
            }

            public Symbol mesh = new(0, "");
            private uint patchPairCount;
            public List<PatchPair> patchPairs = new();

            public class MeshVert
            {

            }

            public MeshPair Read(EndianReader reader)
            {
                mesh = Symbol.Read(reader);
                return this;
            }

            public void Write(EndianWriter writer)
            {
                Symbol.Write(writer, mesh);
            }


        }

        private ushort altRevision;
        private ushort revision;

        private uint meshPairCount;
        public List<MeshPair> meshPairs = new();

        public bool renderTo;

        public Symbol source = new(0, "");

        public int category;

        public Symbol unkSymbol1 = new(0, "");
        public Symbol unkSymbol2 = new(0, "");

        public Symbol unkSymbol3 = new(0, "");

        public BandPatchMesh Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            source = Symbol.Read(reader);

            if (revision > 3)
            {
                meshPairCount = reader.ReadUInt32();
                for (int i = 0; i < meshPairCount; i++)
                    meshPairs.Add(new MeshPair().Read(reader));
            }
            else
            {
                meshPairs.Add(new MeshPair().Read(reader));
            }
            if (revision < 1)
                unkSymbol1 = Symbol.Read(reader);

            if (revision < 4)
                unkSymbol2 = Symbol.Read(reader);

            if (revision > 1)
            {
                if (revision > 2)
                    renderTo = reader.ReadBoolean();
                else
                    unkSymbol3 = Symbol.Read(reader);
            }

            if (revision > 3)
                category = reader.ReadInt32();

            return this;
        }

        public void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)(altRevision << 16 | revision) : (uint)(revision << 16 | altRevision));

            Symbol.Write(writer, source);

            if (revision > 3)
            {
                writer.WriteUInt32((uint)meshPairs.Count);
                foreach (MeshPair meshPair in meshPairs)
                    meshPair.Write(writer);
            }
            else
            {
                meshPairs[0].Write(writer);
            }

            if (revision < 1)
                Symbol.Write(writer, unkSymbol1);

            if (revision < 4)
                Symbol.Write(writer, unkSymbol2);

            if (revision > 1)
            {
                if (revision > 2)
                    writer.WriteBoolean(renderTo);
                else
                    Symbol.Write(writer, unkSymbol3);
            }

            if (revision > 3)
                writer.WriteInt32(category);
        }

    }
}
