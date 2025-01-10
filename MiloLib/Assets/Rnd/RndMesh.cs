using MiloLib.Classes;
using MiloLib.Utils;
using System;
using System.Numerics;
using System.Reflection.PortableExecutable;

namespace MiloLib.Assets.Rnd
{
    [Name("RndMesh"), Description("A Mesh object is composed of triangle faces.")]
    public class RndMesh : Object
    {
        public class BoneTransform
        {
            public Symbol name = new(0, "");
            public Matrix transform = new();

            public BoneTransform Read(EndianReader reader, uint meshRevision)
            {
                name = Symbol.Read(reader);
                transform = transform.Read(reader);
                return this;
            }

            public void Write(EndianWriter writer, uint meshRevision)
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
            private List<Vertex> vertices = new();

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
                    }
                    ReadVertices(reader, meshVersion, isNextGen);
                    return this;
                }
                else
                {
                    ReadVertices(reader, meshVersion, isNextGen);
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

                        WriteVertices(writer, meshVersion, isNextGen);
                    }
                }
            }

            public void ReadVertices(EndianReader reader, uint meshVersion, bool isNextGen)
            {
                vertices = new();
                for (int i = 0; i < count; i++)
                {
                    Vertex newVert = new();
                    newVert.x = reader.ReadFloat();
                    newVert.y = reader.ReadFloat();
                    newVert.z = reader.ReadFloat();

                    if (meshVersion == 34 || meshVersion == 33)
                        newVert.w = reader.ReadFloat();


                    // freQ
                    if (meshVersion <= 10)
                    {
                        newVert.nx = reader.ReadFloat();
                        newVert.ny = reader.ReadFloat();
                        newVert.nz = reader.ReadFloat();

                        newVert.u = reader.ReadFloat();
                        newVert.v = reader.ReadFloat();

                        newVert.weight0 = reader.ReadFloat();
                        newVert.weight1 = reader.ReadFloat();
                        newVert.weight2 = reader.ReadFloat();
                        newVert.weight3 = reader.ReadFloat();

                        newVert.bone0 = reader.ReadUInt16();
                        newVert.bone1 = reader.ReadUInt16();
                        newVert.bone2 = reader.ReadUInt16();
                        newVert.bone3 = reader.ReadUInt16();
                    }
                    else if (meshVersion <= 22)
                    {
                        newVert.bone0 = reader.ReadUInt16();
                        newVert.bone1 = reader.ReadUInt16();
                        newVert.bone2 = reader.ReadUInt16();
                        newVert.bone3 = reader.ReadUInt16();

                        newVert.nx = reader.ReadFloat();
                        newVert.ny = reader.ReadFloat();
                        newVert.nz = reader.ReadFloat();

                        newVert.weight0 = reader.ReadFloat();
                        newVert.weight1 = reader.ReadFloat();
                        newVert.weight2 = reader.ReadFloat();
                        newVert.weight3 = reader.ReadFloat();

                        newVert.u = reader.ReadFloat();
                        newVert.v = reader.ReadFloat();
                    }
                    else if (meshVersion < 35 || isNextGen == false)
                    {
                        if (meshVersion >= 38)
                        {
                            newVert.packed1 = reader.ReadUInt32();
                            newVert.unknown1 = reader.ReadFloat();

                            newVert.packed2 = reader.ReadUInt32();
                            newVert.unknown2 = reader.ReadFloat();
                        }

                        newVert.nx = reader.ReadFloat();
                        newVert.ny = reader.ReadFloat();
                        newVert.nz = reader.ReadFloat();

                        if (meshVersion == 34 || meshVersion == 33)
                            newVert.nw = reader.ReadFloat();

                        if (meshVersion >= 38)
                        {
                            newVert.u = reader.ReadFloat();
                            newVert.v = reader.ReadFloat();

                            newVert.weight0 = reader.ReadFloat();
                            newVert.weight1 = reader.ReadFloat();
                            newVert.weight2 = reader.ReadFloat();
                            newVert.weight3 = reader.ReadFloat();
                        }
                        else
                        {
                            newVert.weight0 = reader.ReadFloat();
                            newVert.weight1 = reader.ReadFloat();
                            newVert.weight2 = reader.ReadFloat();
                            newVert.weight3 = reader.ReadFloat();

                            newVert.u = reader.ReadFloat();
                            newVert.v = reader.ReadFloat();
                        }

                        if (meshVersion >= 34)
                        {
                            newVert.bone0 = reader.ReadUInt16();
                            newVert.bone1 = reader.ReadUInt16();
                            newVert.bone2 = reader.ReadUInt16();
                            newVert.bone3 = reader.ReadUInt16();

                            if (meshVersion >= 38)
                            {
                                newVert.packed3 = reader.ReadUInt32();
                                newVert.packed4 = reader.ReadUInt32();
                                newVert.neg1 = reader.ReadFloat();
                                newVert.pos1 = reader.ReadFloat();
                            }
                            else
                            {
                                newVert.tangent0 = reader.ReadFloat();
                                newVert.tangent1 = reader.ReadFloat();
                                newVert.tangent2 = reader.ReadFloat();
                                newVert.tangent3 = reader.ReadFloat();
                            }
                        }
                    }
                    else
                    {
                        newVert.uvCheck = reader.ReadInt32();
                        if (newVert.uvCheck == -1)
                        {
                            newVert.halfU = reader.ReadHalfFloat();
                            newVert.halfV = reader.ReadHalfFloat();

                            newVert.normals = newVert.normals.Read(reader);

                            newVert.tangents = newVert.tangents.Read(reader);

                            newVert.weights = newVert.weights.Read(reader);

                            newVert.bone0 = reader.ReadByte();
                            newVert.bone1 = reader.ReadByte();
                            newVert.bone2 = reader.ReadByte();
                            newVert.bone3 = reader.ReadByte();
                        }
                        else
                        {
                            newVert.halfU = reader.ReadHalfFloat();
                            newVert.halfV = reader.ReadHalfFloat();

                            newVert.ny = reader.ReadHalfFloat();
                            newVert.nz = reader.ReadHalfFloat();
                            newVert.nw = reader.ReadHalfFloat();

                            newVert.qTangents = newVert.qTangents.Read(reader);

                            newVert.weight0 = reader.ReadByte();
                            newVert.weight1 = reader.ReadByte();
                            newVert.weight2 = reader.ReadByte();
                            newVert.weight3 = reader.ReadByte();

                            newVert.bone0 = reader.ReadByte();
                            newVert.bone1 = reader.ReadByte();
                            newVert.bone2 = reader.ReadByte();
                            newVert.bone3 = reader.ReadByte();
                        }
                    }

                    vertices.Add(newVert);

                }
            }

            public void WriteVertices(EndianWriter writer, uint meshVersion, bool isNextGen)
            {
                foreach (var vertex in vertices)
                {
                    // Always write position
                    writer.WriteFloat(vertex.x);
                    writer.WriteFloat(vertex.y);
                    writer.WriteFloat(vertex.z);

                    // Possible W
                    if (meshVersion == 33 || meshVersion == 34)
                        writer.WriteFloat(vertex.w);

                    if (meshVersion <= 10)
                    {
                        writer.WriteFloat(vertex.nx);
                        writer.WriteFloat(vertex.ny);
                        writer.WriteFloat(vertex.nz);

                        writer.WriteFloat(vertex.u);
                        writer.WriteFloat(vertex.v);

                        writer.WriteFloat(vertex.weight0);
                        writer.WriteFloat(vertex.weight1);
                        writer.WriteFloat(vertex.weight2);
                        writer.WriteFloat(vertex.weight3);

                        writer.WriteUInt16(vertex.bone0);
                        writer.WriteUInt16(vertex.bone1);
                        writer.WriteUInt16(vertex.bone2);
                        writer.WriteUInt16(vertex.bone3);
                    }
                    else if (meshVersion <= 22)
                    {
                        writer.WriteUInt16(vertex.bone0);
                        writer.WriteUInt16(vertex.bone1);
                        writer.WriteUInt16(vertex.bone2);
                        writer.WriteUInt16(vertex.bone3);

                        writer.WriteFloat(vertex.nx);
                        writer.WriteFloat(vertex.ny);
                        writer.WriteFloat(vertex.nz);

                        writer.WriteFloat(vertex.weight0);
                        writer.WriteFloat(vertex.weight1);
                        writer.WriteFloat(vertex.weight2);
                        writer.WriteFloat(vertex.weight3);

                        writer.WriteFloat(vertex.u);
                        writer.WriteFloat(vertex.v);
                    }
                    else if (meshVersion < 35 || !isNextGen)
                    {
                        if (meshVersion >= 38)
                        {
                            writer.WriteUInt32(vertex.packed1);
                            writer.WriteFloat(vertex.unknown1);
                            writer.WriteUInt32(vertex.packed2);
                            writer.WriteFloat(vertex.unknown2);
                        }

                        writer.WriteFloat(vertex.nx);
                        writer.WriteFloat(vertex.ny);
                        writer.WriteFloat(vertex.nz);

                        if (meshVersion == 33 || meshVersion == 34)
                            writer.WriteFloat(vertex.nw);

                        if (meshVersion >= 38)
                        {
                            writer.WriteFloat(vertex.u);
                            writer.WriteFloat(vertex.v);

                            writer.WriteFloat(vertex.weight0);
                            writer.WriteFloat(vertex.weight1);
                            writer.WriteFloat(vertex.weight2);
                            writer.WriteFloat(vertex.weight3);
                        }
                        else
                        {
                            writer.WriteFloat(vertex.weight0);
                            writer.WriteFloat(vertex.weight1);
                            writer.WriteFloat(vertex.weight2);
                            writer.WriteFloat(vertex.weight3);

                            writer.WriteFloat(vertex.u);
                            writer.WriteFloat(vertex.v);
                        }

                        if (meshVersion >= 34)
                        {
                            writer.WriteUInt16(vertex.bone0);
                            writer.WriteUInt16(vertex.bone1);
                            writer.WriteUInt16(vertex.bone2);
                            writer.WriteUInt16(vertex.bone3);

                            if (meshVersion >= 38)
                            {
                                writer.WriteUInt32(vertex.packed3);
                                writer.WriteUInt32(vertex.packed4);
                                writer.WriteFloat(vertex.neg1);
                                writer.WriteFloat(vertex.pos1);
                            }
                            else
                            {
                                writer.WriteFloat(vertex.tangent0);
                                writer.WriteFloat(vertex.tangent1);
                                writer.WriteFloat(vertex.tangent2);
                                writer.WriteFloat(vertex.tangent3);
                            }
                        }
                    }
                    else
                    {
                        // We assume we have stored an int 'UvIndicator' in vertex indicating the read 'uv' value:
                        writer.WriteInt32(vertex.uvCheck);

                        if (vertex.uvCheck == -1)
                        {
                            writer.WriteUInt16((ushort)vertex.halfU);
                            writer.WriteUInt16((ushort)vertex.halfV);

                            vertex.normals.Write(writer);
                            vertex.tangents.Write(writer);
                            vertex.weights.Write(writer);

                            writer.WriteByte((byte)vertex.bone0);
                            writer.WriteByte((byte)vertex.bone1);
                            writer.WriteByte((byte)vertex.bone2);
                            writer.WriteByte((byte)vertex.bone3);
                        }
                        else
                        {
                            writer.WriteUInt16((ushort)vertex.halfU);
                            writer.WriteUInt16((ushort)vertex.halfV);

                            writer.WriteUInt16((ushort)vertex.ny);
                            writer.WriteUInt16((ushort)vertex.nz);
                            writer.WriteUInt16((ushort)vertex.nw);

                            vertex.qTangents.Write(writer);

                            writer.WriteByte((byte)vertex.weight0);
                            writer.WriteByte((byte)vertex.weight1);
                            writer.WriteByte((byte)vertex.weight2);
                            writer.WriteByte((byte)vertex.weight3);

                            writer.WriteByte((byte)vertex.bone0);
                            writer.WriteByte((byte)vertex.bone1);
                            writer.WriteByte((byte)vertex.bone2);
                            writer.WriteByte((byte)vertex.bone3);
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
            public MiloLib.Classes.Vector4 vec = new();
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
        public Symbol mat2 = new(0, "");
        public Symbol geomOwner = new(0, "");
        public Symbol altGeomOwner = new(0, "");
        public Symbol transParent = new(0, "");

        public Symbol unkTransReference1 = new(0, "");
        public Symbol unkTransReference2 = new(0, "");

        public Symbol unkSym1 = new(0, "");

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

        public bool excludeFromSelfShadow;

        public bool unkBool1;
        public bool unkBool2;
        public uint unkInt1;

        public Sphere sphere = new();

        public bool unkBool;
        public float unkFloat;

        public MiloLib.Classes.Vector3 unknownVector3 = new();


        public RndMesh Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            base.Read(reader, false, parent, entry);


            trans = trans.Read(reader, false, parent, entry);
            draw = draw.Read(reader, false, parent, entry);

            mat = Symbol.Read(reader);

            if (revision == 27)
                mat2 = Symbol.Read(reader);

            geomOwner = Symbol.Read(reader);

            if (revision < 13)
                altGeomOwner = Symbol.Read(reader);

            if (revision < 15)
                transParent = Symbol.Read(reader);

            if (revision < 14)
            {
                unkTransReference1 = Symbol.Read(reader);
                unkTransReference2 = Symbol.Read(reader);
            }

            if (revision < 3)
                unknownVector3 = unknownVector3.Read(reader);

            if (revision < 15)
                sphere = sphere.Read(reader);

            if (revision < 8)
                unkBool = reader.ReadBoolean();

            if (revision < 15)
            {
                unkSym1 = Symbol.Read(reader);
                unkFloat = reader.ReadFloat();
            }



            if (revision < 16)
            {
                if (revision > 11)
                {
                    unkBool1 = reader.ReadBoolean();
                }
            }
            else
                mutable = (Mutable)reader.ReadUInt32();

            if (revision > 17)
                volume = (Volume)reader.ReadUInt32();

            if (revision > 18)
                bspNode = bspNode.Read(reader);

            if (revision == 7)
                unkBool2 = reader.ReadBoolean();

            if (revision < 11)
                unkInt1 = reader.ReadUInt32();

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

            if (reader.ReadInt32() > 0)
            {
                reader.BaseStream.Position -= 4;
                if (revision >= 34)
                {
                    boneCount = reader.ReadUInt32();
                    boneTransforms = new List<BoneTransform>();
                    for (int i = 0; i < boneCount; i++)
                    {
                        boneTransforms.Add(new BoneTransform().Read(reader, revision));
                    }
                }
                else
                {
                    for (int i = 0; i < 4; i++)
                    {
                        boneTransforms.Add(new BoneTransform());
                        boneTransforms[i].name = Symbol.Read(reader);
                    }
                    for (int i = 0; i < 4; i++)
                    {
                        boneTransforms[i].transform = boneTransforms[i].transform.Read(reader);
                    }
                }
            }


            if (revision > 34)
            {
                keepMeshData = reader.ReadBoolean();
                if (revision == 37)
                    excludeFromSelfShadow = reader.ReadBoolean();
                else if (revision >= 38)
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

            if (revision == 27)
                Symbol.Write(writer, mat2);

            Symbol.Write(writer, geomOwner);

            if (revision < 13)
                Symbol.Write(writer, altGeomOwner);

            if (revision < 15)
                Symbol.Write(writer, transParent);

            if (revision < 14)
            {
                Symbol.Write(writer, unkTransReference1);
                Symbol.Write(writer, unkTransReference2);
            }

            if (revision < 3)
                unknownVector3.Write(writer);

            if (revision < 15)
                sphere.Write(writer);

            if (revision < 8)
                writer.WriteBoolean(unkBool);

            if (revision < 15)
            {
                Symbol.Write(writer, unkSym1);
                writer.WriteFloat(unkFloat);
            }
            if (revision < 16)
            {
                if (revision > 11)
                {
                    writer.WriteBoolean(unkBool1);
                }
            }
            else
                writer.WriteUInt32((uint)mutable);

            if (revision > 17)
                writer.WriteUInt32((uint)volume);

            if (revision > 18)
                bspNode.Write(writer);

            if (revision == 7)
                writer.WriteBoolean(unkBool2);

            if (revision < 11)
                writer.WriteUInt32(unkInt1);

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
                boneTransform.Write(writer, revision);
            }

            if (revision > 34)
            {
                writer.WriteBoolean(keepMeshData);
                if (revision == 37)
                    writer.WriteBoolean(excludeFromSelfShadow);
                else if (revision >= 38)
                    writer.WriteBoolean(hasAOCalculation);
            }

            if (standalone)
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
        }

    }
}
