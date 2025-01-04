using MiloLib.Classes;
using MiloLib.Utils;
using System.Numerics;
using System.Reflection.PortableExecutable;

namespace MiloLib.Assets.Rnd
{
    [Name("RndMesh"), Description("")]
    public class RndMesh : Object
    {
        public class BoneTransform
        {
            public Symbol name = new(0, "");
            public Matrix transform = new();

            public BoneTransform Read(EndianReader reader)
            {
                name = Symbol.Read(reader);
                transform = transform.Read(reader);
                return this;
            }

            public void Write(EndianWriter writer)
            {
                Symbol.Write(writer, name);
                transform.Write(writer);
            }

            public override string ToString()
            {
                return $"{name} {transform}";
            }
        }

        public class Face
        {
            ushort idx1;
            ushort idx2;
            ushort idx3;

            public Face Read(EndianReader reader)
            {
                idx1 = reader.ReadUInt16();
                idx2 = reader.ReadUInt16();
                idx3 = reader.ReadUInt16();
                return this;
            }

            public void Write(EndianWriter writer)
            {
                writer.WriteUInt16(idx1);
                writer.WriteUInt16(idx2);
                writer.WriteUInt16(idx3);
            }

            public override string ToString()
            {
                return $"{idx1} {idx2} {idx3}";
            }
        }

        public class Vertices
        {
            private uint count;
            public bool isNextGen;
            public uint vertexSize;
            public uint unkType;
            private List<byte> vertices = new();

            public Vertices Read(EndianReader reader, uint meshVersion)
            {
                count = reader.ReadUInt32();
                if (meshVersion >= 36)
                {
                    isNextGen = reader.ReadBoolean();
                    if (isNextGen)
                    {
                        vertexSize = reader.ReadUInt32();
                        unkType = reader.ReadUInt32();

                        // read all vertices
                        for (int i = 0; i < count * vertexSize; i++)
                        {
                            vertices.Add(reader.ReadByte());
                        }
                    }
                    else
                    {
                        // last gen, so read in a different way
                        for (int i = 0; i < count * 88; i++)
                        {
                            vertices.Add(reader.ReadByte());
                        }
                    }
                }

                if (meshVersion == 33 || meshVersion == 34)
                {
                    for (int i = 0; i < count * 72; i++)
                    {
                        vertices.Add(reader.ReadByte());
                    }
                }

                return this;
            }

            public void Write(EndianWriter writer, uint meshVersion)
            {
                writer.WriteUInt32(count);
                if (meshVersion >= 36)
                {
                    writer.WriteBoolean(isNextGen);
                    if (isNextGen)
                    {
                        writer.WriteUInt32(vertexSize);
                        writer.WriteUInt32(unkType);

                        // write all vertices
                        foreach (byte vertex in vertices)
                        {
                            writer.WriteByte(vertex);
                        }
                    }
                }
            }
        }
        public enum Mutable : uint
        {
            kMutableNone = 0,
            kMutableVerts = 31,
            kMutableFaces = 32,
            kMutableAll = 63,
        }

        public enum Volume : uint
        {
            kVolumeEmpty,
            kVolumeTriangles,
            kVolumeBSP,
            kVolumeBox,
        }

        public class BSPNode
        {
            public bool hasValue;
            public MiloLib.Classes.Vector4 vec;
            public BSPNode? left;
            public BSPNode? right;

            public BSPNode Read(EndianReader reader)
            {
                hasValue = reader.ReadBoolean();
                if (hasValue)
                {
                    vec = vec.Read(reader);
                    left = new BSPNode().Read(reader);
                    right = new BSPNode().Read(reader);
                }
                return this;
            }

            public void Write(EndianWriter writer)
            {
                writer.WriteBoolean(hasValue);
                if (hasValue)
                {
                    vec.Write(writer);
                    if (left != null)
                        left.Write(writer);
                    if (right != null)
                        right.Write(writer);
                }
            }
        }

        private ushort altRevision;
        private ushort revision;

        public RndTrans trans = new();
        public RndDrawable draw = new();

        public Symbol mat = new(0, "");
        public Symbol geomOwner = new(0, "");

        public Mutable mutable;
        public Volume volume;

        public BSPNode bspNode = new();

        public Vertices vertices = new();
        public List<Face> faces = new();

        private uint groupSizesCount;
        public List<byte> groupSizes = new();

        private uint boneCount;
        public List<BoneTransform> boneTransforms = new();

        public bool keepMeshData;
        public bool hasAOCalculation;


        public RndMesh Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            base.Read(reader, false, parent, entry);

            trans = trans.Read(reader, false, parent, entry);
            draw = draw.Read(reader, false, parent, entry);
            mat = Symbol.Read(reader);
            geomOwner = Symbol.Read(reader);
            mutable = (Mutable)reader.ReadUInt32();
            volume = (Volume)reader.ReadUInt32();

            bspNode = bspNode.Read(reader);

            vertices = vertices.Read(reader, revision);

            uint faceCount = reader.ReadUInt32();
            faces = new List<Face>();
            for (int i = 0; i < faceCount; i++)
            {
                faces.Add(new Face().Read(reader));
            }

            groupSizesCount = reader.ReadUInt32();
            groupSizes = new List<byte>();
            for (int i = 0; i < groupSizesCount; i++)
            {
                groupSizes.Add(reader.ReadByte());
            }

            boneCount = reader.ReadUInt32();
            boneTransforms = new List<BoneTransform>();
            for (int i = 0; i < boneCount; i++)
            {
                boneTransforms.Add(new BoneTransform().Read(reader));
            }

            if (revision > 33)
            {
                keepMeshData = reader.ReadBoolean();
                hasAOCalculation = reader.ReadBoolean();
            }



            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw new Exception("Got to end of standalone asset but didn't find the expected end bytes, read likely did not succeed");

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            base.Write(writer, false, parent, entry);

            trans.Write(writer, false, true);
            draw.Write(writer, false, true);
            Symbol.Write(writer, mat);
            Symbol.Write(writer, geomOwner);
            writer.WriteUInt32((uint)mutable);
            writer.WriteUInt32((uint)volume);

            bspNode.Write(writer);

            vertices.Write(writer, revision);

            writer.WriteUInt32((uint)faces.Count);
            foreach (Face face in faces)
            {
                face.Write(writer);
            }

            writer.WriteUInt32(groupSizesCount);
            foreach (byte groupSize in groupSizes)
            {
                writer.WriteByte(groupSize);
            }

            writer.WriteUInt32(boneCount);
            foreach (BoneTransform boneTransform in boneTransforms)
            {
                boneTransform.Write(writer);
            }

            writer.WriteBoolean(keepMeshData);
            writer.WriteBoolean(hasAOCalculation);

            if (standalone)
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
        }

    }
}
