using MiloLib.Classes;
using MiloLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.PortableExecutable;
using System.Xml.Linq;

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
            public ushort idx1;
            public ushort idx2;
            public ushort idx3;

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
            public uint compressionType;
            public List<Vertex> vertices = new();

            public Vertices Read(EndianReader reader, uint meshVersion)
            {
                count = reader.ReadUInt32();
                if (meshVersion >= 36)
                {
                    isNextGen = reader.ReadBoolean();
                    if (isNextGen)
                    {
                        vertexSize = reader.ReadUInt32();
                        compressionType = reader.ReadUInt32();
                    }
                    ReadVertices(reader, meshVersion, isNextGen, compressionType);
                    return this;
                }
                else
                {
                    ReadVertices(reader, meshVersion, isNextGen, 0);
                }

                return this;
            }

            public void Write(EndianWriter writer, uint meshVersion)
            {
                writer.WriteUInt32((uint)vertices.Count);
                if (meshVersion >= 36)
                {
                    writer.WriteBoolean(isNextGen);
                    if (isNextGen)
                    {
                        writer.WriteUInt32(vertexSize);
                        writer.WriteUInt32(compressionType);
                    }
                    WriteVertices(writer, meshVersion, isNextGen, compressionType);
                    return;
                }
                else
                {
                    WriteVertices(writer, meshVersion, isNextGen, 0);
                }
            }

            // TODO:
            // these read and write vertex functions are genuinely horrible. need to refactor these in the future so converting between Mesh versions can be done more reliably
            public void ReadVertices(EndianReader reader, uint meshVersion, bool isNextGen, uint compressionType)
            {
                vertices = new();
                for (int i = 0; i < count; i++)
                {
                    Vertex newVert = new();
                    newVert.x = reader.ReadFloat();
                    newVert.y = reader.ReadFloat();
                    newVert.z = reader.ReadFloat();

                    if (meshVersion == 34)
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

                        if (meshVersion == 34)
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

                        if (meshVersion >= 33)
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
                                if (meshVersion > 34)
                                {
                                    newVert.unknown1 = reader.ReadFloat();
                                    newVert.unknown2 = reader.ReadFloat();
                                }
                            }
                        }
                    }
                    else
                    {
                        if (compressionType == 1)
                        {
                            uint value = reader.ReadUInt32();
                            newVert.vertexColors.r = (byte)((value >> 24) & 0xFF);
                            newVert.vertexColors.g = (byte)((value >> 16) & 0xFF);
                            newVert.vertexColors.b = (byte)((value >> 8) & 0xFF);
                            newVert.vertexColors.a = (byte)(value & 0xFF);
                            newVert.u = reader.ReadHalfFloat();
                            newVert.v = reader.ReadHalfFloat();

                            Vertex.SignedCompressedVec4 norms = new Vertex.SignedCompressedVec4();
                            norms.Read(reader);
                            newVert.nx = norms.x;
                            newVert.ny = norms.y;
                            newVert.nz = norms.z;
                            newVert.nw = norms.w;

                            Vertex.SignedCompressedVec4 tangents = new Vertex.SignedCompressedVec4();
                            tangents.Read(reader);

                            newVert.tangent0 = tangents.x;
                            newVert.tangent1 = tangents.y;
                            newVert.tangent2 = tangents.z;
                            newVert.tangent3 = tangents.w;

                            Vertex.UnsignedCompressedVec4 weights = new Vertex.UnsignedCompressedVec4();
                            weights.Read(reader);

                            newVert.weight0 = weights.x;
                            newVert.weight1 = weights.y;
                            newVert.weight2 = weights.z;
                            newVert.weight3 = weights.w;

                            newVert.bone0 = reader.ReadByte();
                            newVert.bone1 = reader.ReadByte();
                            newVert.bone2 = reader.ReadByte();
                            newVert.bone3 = reader.ReadByte();
                        }
                        else if (compressionType == 2)
                        {
                            newVert.u = reader.ReadHalfFloat();
                            newVert.v = reader.ReadHalfFloat();

                            Vertex.PS3SignedCompressedVec3 norms = new Vertex.PS3SignedCompressedVec3();
                            norms.Read(reader);
                            newVert.nx = norms.x;
                            newVert.ny = norms.y;
                            newVert.nz = norms.z;
                            newVert.nw = norms.w;

                            Vertex.PS3SignedCompressedVec3 tangents = new Vertex.PS3SignedCompressedVec3();
                            tangents.Read(reader);

                            newVert.tangent0 = tangents.x;
                            newVert.tangent1 = tangents.y;
                            newVert.tangent2 = tangents.z;
                            newVert.tangent3 = tangents.w;

                            Vertex.PS3UnsignedCompressedVec3 weights = new Vertex.PS3UnsignedCompressedVec3();
                            weights.Read(reader);

                            uint value = reader.ReadUInt32();
                            newVert.vertexColors.a = (byte)((value >> 24) & 0xFF);
                            newVert.vertexColors.r = (byte)((value >> 16) & 0xFF);
                            newVert.vertexColors.g = (byte)((value >> 8) & 0xFF);
                            newVert.vertexColors.b = (byte)(value & 0xFF);

                            newVert.weight0 = weights.x;
                            newVert.weight1 = weights.y;
                            newVert.weight2 = weights.z;
                            newVert.weight3 = weights.w;

                            newVert.bone0 = reader.ReadUInt16();
                            newVert.bone1 = reader.ReadUInt16();
                            newVert.bone2 = reader.ReadUInt16();
                            newVert.bone3 = reader.ReadUInt16();
                        }
                    }

                    vertices.Add(newVert);

                }
            }

            public void WriteVertices(EndianWriter writer, uint meshVersion, bool isNextGen, uint compressionType)
            {
                foreach (var vertex in vertices)
                {
                    // Always write position
                    writer.WriteFloat(vertex.x);
                    writer.WriteFloat(vertex.y);
                    writer.WriteFloat(vertex.z);

                    // Possible W
                    if (meshVersion == 34)
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

                        if (meshVersion == 34)
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

                        if (meshVersion >= 33)
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
                                if (meshVersion > 34)
                                {
                                    writer.WriteFloat(vertex.unknown1);
                                    writer.WriteFloat(vertex.unknown2);
                                }


                            }
                        }
                    }
                    else
                    {
                        if (compressionType == 1)
                        {
                            // vertex colors in RGBA?
                            uint value = ((uint)vertex.vertexColors.r << 24) | ((uint)vertex.vertexColors.g << 16) | ((uint)vertex.vertexColors.b << 8) | (uint)vertex.vertexColors.a;
                            writer.WriteUInt32(value);
                            writer.WriteHalfFloat(vertex.u);
                            writer.WriteHalfFloat(vertex.v);

                            Vertex.SignedCompressedVec4 norms = new Vertex.SignedCompressedVec4
                            {
                                x = vertex.nx,
                                y = vertex.ny,
                                z = vertex.nz,
                                w = vertex.nw
                            };
                            norms.Write(writer);

                            Vertex.SignedCompressedVec4 tangents = new Vertex.SignedCompressedVec4
                            {
                                x = vertex.tangent0,
                                y = vertex.tangent1,
                                z = vertex.tangent2,
                                w = vertex.tangent3
                            };
                            tangents.Write(writer);

                            Vertex.UnsignedCompressedVec4 weights = new Vertex.UnsignedCompressedVec4
                            {
                                x = vertex.weight0,
                                y = vertex.weight1,
                                z = vertex.weight2,
                                w = vertex.weight3
                            };
                            weights.Write(writer);

                            writer.WriteByte((byte)vertex.bone0);
                            writer.WriteByte((byte)vertex.bone1);
                            writer.WriteByte((byte)vertex.bone2);
                            writer.WriteByte((byte)vertex.bone3);
                        }
                        else if (compressionType == 2)
                        {
                            writer.WriteHalfFloat(vertex.u);
                            writer.WriteHalfFloat(vertex.v);

                            Vertex.PS3SignedCompressedVec3 norms = new Vertex.PS3SignedCompressedVec3
                            {
                                x = vertex.nx,
                                y = vertex.ny,
                                z = vertex.nz,
                                w = vertex.nw
                            };
                            norms.Write(writer);

                            Vertex.PS3SignedCompressedVec3 tangents = new Vertex.PS3SignedCompressedVec3
                            {
                                x = vertex.tangent0,
                                y = vertex.tangent1,
                                z = vertex.tangent2,
                                w = vertex.tangent3
                            };
                            tangents.Write(writer);

                            Vertex.PS3UnsignedCompressedVec3 weights = new Vertex.PS3UnsignedCompressedVec3
                            {
                                x = vertex.weight0,
                                y = vertex.weight1,
                                z = vertex.weight2,
                                w = vertex.weight3
                            };
                            weights.Write(writer);

                            Vertex.PS3UnsignedCompressedVec3 unknown = new Vertex.PS3UnsignedCompressedVec3
                            {
                                x = vertex.weight0,
                                y = vertex.weight1,
                                z = vertex.weight2,
                                w = vertex.weight3
                            };
                            uint value = ((uint)vertex.vertexColors.a << 24) | ((uint)vertex.vertexColors.r << 16) | ((uint)vertex.vertexColors.g << 8) | (uint)vertex.vertexColors.b;
                            writer.WriteUInt32(value);

                            writer.WriteUInt16((byte)vertex.bone0);
                            writer.WriteUInt16((byte)vertex.bone1);
                            writer.WriteUInt16((byte)vertex.bone2);
                            writer.WriteUInt16((byte)vertex.bone3);
                        }
                    }
                }
            }
        }

        public class GroupSection
        {
            public List<int> sections = new();
            public List<ushort> vertOffsets = new();

            public GroupSection Read(EndianReader reader, uint meshRevision)
            {
                uint sectionCount = reader.ReadUInt32();
                uint vertCount = reader.ReadUInt32();
                for (int i = 0; i < sectionCount; i++)
                {
                    sections.Add(reader.ReadInt32());
                }
                for (int i = 0; i < vertCount; i++)
                {
                    vertOffsets.Add(reader.ReadUInt16());
                }
                return this;
            }

            public void Write(EndianWriter writer, uint meshRevision)
            {
                writer.WriteUInt32((uint)sections.Count);
                writer.WriteUInt32((uint)vertOffsets.Count);
                foreach (var section in sections)
                {
                    writer.WriteInt32(section);
                }
                foreach (var vert in vertOffsets)
                {
                    writer.WriteUInt16(vert);
                }
            }

            public override string ToString()
            {
                return $"{sections.Count} sections, {vertOffsets.Count} vertOffsets";
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

        [HideInInspector]
        public ushort altRevision;
        [HideInInspector]
        public ushort revision;

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
        public bool noQuant;

        public bool unkBool3;

        public bool excludeFromSelfShadow;

        public bool unkBool1;
        public bool unkBool2;
        public uint unkInt1;

        public Sphere sphere = new();

        public bool unkBool;
        public float unkFloat;

        public MiloLib.Classes.Vector3 unknownVector3 = new();

        public List<GroupSection> groupSections = new();


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

            if (revision > 0x17)
            {
                groupSizesCount = reader.ReadUInt32();
                groupSizes = new List<byte>();
                for (int i = 0; i < groupSizesCount; i++)
                {
                    groupSizes.Add(reader.ReadByte());
                }
            }
            else if (revision > 0x15)
            {
                // todo
            }
            else if (revision > 0x10)
            {
                // todo double check this
                groupSizesCount = reader.ReadUInt32();
                groupSizes = new List<byte>();
                for (int i = 0; i < groupSizesCount; i++)
                {
                    groupSizes.Add(reader.ReadByte());
                }
            }



            if (reader.ReadInt32() > 0)
            {
                reader.BaseStream.Position -= 4;
                if (revision >= 33)
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

            if (altRevision > 5)
            {
                // todo: striper stuff
            }

            if (revision != 0 && revision < 4)
            {
                // todo
                // std::vector<std::vector<unsigned short> > usvec;
                // bs >> usvec;
            }

            if (revision == 0)
            {
                /*
                bool bd4;
                int ic0, ic4, ic8, icc;
                bs >> bd4 >> ic0 >> ic4 >> ic8;
                bs >> icc;
                */
            }


            if (revision > 34)
            {
                keepMeshData = reader.ReadBoolean();
            }

            if (revision > 0x25)
            {
                hasAOCalculation = reader.ReadBoolean();
            }

            if (altRevision > 1)
            {
                noQuant = reader.ReadBoolean();
            }

            if (altRevision > 3)
            {
                unkBool3 = reader.ReadBoolean();
            }

            // weird thing on last-gen, from Cisco's notes
            if (groupSizesCount > 0 && groupSizes[0] > 0 && parent.revision < 25)
            {
                for (int i = 0; i < groupSizesCount; i++)
                {
                    GroupSection section = new GroupSection();
                    groupSections.Add(section.Read(reader, revision));
                }
            }



            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw MiloLib.Exceptions.MiloAssetReadException.EndBytesNotFound(parent, entry, reader.BaseStream.Position);

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            base.Write(writer, false, parent, entry);

            trans.Write(writer, false, parent, true);
            draw.Write(writer, false, parent, true);
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

            if (revision > 0x17)
            {
                groupSizesCount = (uint)groupSizes.Count;
                writer.WriteUInt32(groupSizesCount);
                foreach (byte groupSize in groupSizes)
                {
                    writer.WriteByte(groupSize);
                }
            }
            else if (revision > 0x15)
            {
                // todo - matches Read behavior (does nothing)
            }
            else if (revision > 0x10)
            {
                groupSizesCount = (uint)groupSizes.Count;
                writer.WriteUInt32(groupSizesCount);
                foreach (byte groupSize in groupSizes)
                {
                    writer.WriteByte(groupSize);
                }
            }

            
            if (revision >= 33)
            {
                writer.WriteUInt32((uint)boneTransforms.Count);
                foreach (BoneTransform boneTransform in boneTransforms)
                {
                    boneTransform.Write(writer, revision);
                }
            }
            else
            {
                
                if (boneTransforms.Count > 0)
                {
                    while (boneTransforms.Count < 4)
                    {
                        boneTransforms.Add(new BoneTransform());
                    }
                    
                    for (int i = 0; i < 4; i++)
                    {
                        Symbol.Write(writer, boneTransforms[i].name);
                    }
                    for (int i = 0; i < 4; i++)
                    {
                        boneTransforms[i].transform.Write(writer);
                    }
                }
                else
                {
                    writer.WriteUInt32(0);
                }
            }

            if (revision > 34)
                writer.WriteBoolean(keepMeshData);

            if (revision > 0x25)
                writer.WriteBoolean(hasAOCalculation);

            if (altRevision > 1)
                writer.WriteBoolean(noQuant);

            if (altRevision > 3)
                writer.WriteBoolean(unkBool3);

            if (groupSizesCount > 0 && groupSizes.Count > 0 && groupSizes[0] > 0 && parent.revision < 25)
            {
                while (groupSections.Count < groupSizesCount)
                {
                    groupSections.Add(new GroupSection());
                }
                for (int i = 0; i < groupSizesCount; i++)
                {
                    groupSections[i].Write(writer, revision);
                }
            }

            if (standalone)
                writer.WriteEndBytes();
        }

        public static RndMesh New(ushort revision, ushort altRevision, uint vertexCount, uint faceCount)
        {
            RndMesh newRndMesh = new RndMesh();
            newRndMesh.revision = revision;
            newRndMesh.altRevision = altRevision;
            return newRndMesh;
        }

    }
}
