using MiloLib.Classes;
using MiloLib.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace MiloLib.Assets.Rnd
{
    [Name("Trans"), Description("Base class for transformable perObjs. Trans perObjs have a 3D position, rotation, and scale.")]
    public class RndTrans : Object
    {
        public enum Constraint
        {
            kConstraintNone,
            kConstraintLocalRotate,
            kConstraintParentWorld,
            kConstraintLookAtTarget,
            kConstraintShadowTarget,
            kConstraintBillboardZ,
            kConstraintBillboardXZ,
            kConstraintBillboardXYZ,
            kConstraintFastBillboardXYZ,
        }

        private ushort altRevision;
        private ushort revision;

        [Name("Local Transform"), Description("The local transform of the object.")]
        public Matrix localXfm = new();
        [Name("World Transform"), Description("The world transform of the object.")]
        public Matrix worldXfm = new();

        [Name("Constraint"), Description("Trans constraint for the object."), MinVersion(7)]
        public Constraint constraint;

        [Name("Target"), Description("Target according to the constraint."), MinVersion(6)]
        public Symbol target = new(0, "");

        [Name("Preserve Scale"), Description("Preserve scale if applying dynamic constraint."), MinVersion(7)]
        public bool preserveScale;

        [Name("Parent"), Description("Object this is linked to.")]
        public Symbol parentObj = new(0, "");

        private uint transCount;

        [MaxVersion(8)]
        public List<Symbol> transObjects = new();
        [MaxVersion(8)]
        public List<string> transObjectsNullTerminated = new();

        public RndTrans Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            if (standalone)
                objFields = objFields.Read(reader, parent, entry);

            localXfm = localXfm.Read(reader);
            worldXfm = worldXfm.Read(reader);

            if (revision < 9)
            {
                transCount = reader.ReadUInt32();
                if (transCount > 0)
                {
                    if (parent.revision <= 6)
                    {
                        for (int i = 0; i < transCount; i++)
                        {
                            transObjectsNullTerminated.Add(reader.ReadUTF8());
                        }
                    }
                    else
                    {
                        for (int i = 0; i < transCount; i++)
                        {
                            transObjects.Add(Symbol.Read(reader));
                        }
                    }
                }
            }

            if (revision > 6)
                constraint = (Constraint)reader.ReadUInt32();

            if (revision > 5)
                target = Symbol.Read(reader);

            if (revision > 6)
                preserveScale = reader.ReadBoolean();

            parentObj = Symbol.Read(reader);


            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw MiloLib.Exceptions.MiloAssetReadException.EndBytesNotFound(parent, entry, reader.BaseStream.Position);

            return this;
        }

        public void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, bool skipMetadata = false)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            if (standalone && !skipMetadata)
            {
                base.objFields.Write(writer, parent);
            }

            localXfm.Write(writer);
            worldXfm.Write(writer);

            if (revision < 9)
            {
                if (parent.revision <= 6)
                {
                    if (transObjectsNullTerminated.Count == 0 && transObjects.Count > 0)
                    {
                        foreach (var sym in transObjects)
                        {
                            transObjectsNullTerminated.Add(sym.value);
                        }
                    }
                    transCount = (uint)transObjectsNullTerminated.Count;
                    writer.WriteUInt32(transCount);
                    foreach (var obj in transObjectsNullTerminated)
                    {
                        writer.WriteUTF8(obj);
                    }
                }
                else
                {
                    if (transObjects.Count == 0 && transObjectsNullTerminated.Count > 0)
                    {
                        foreach (var str in transObjectsNullTerminated)
                        {
                            transObjects.Add(new Symbol((uint)str.Length, str));
                        }
                    }
                    transCount = (uint)transObjects.Count;
                    writer.WriteUInt32(transCount);
                    foreach (var obj in transObjects)
                    {
                        Symbol.Write(writer, obj);
                    }
                }
            }

            if (revision > 6)
                writer.WriteUInt32((uint)constraint);

            if (revision > 5)
                Symbol.Write(writer, target);

            if (revision > 6)
                writer.WriteBoolean(preserveScale);

            Symbol.Write(writer, parentObj);

            if (standalone)
            {
                writer.WriteEndBytes();
            }
        }

        public static RndTrans New(ushort revision, ushort altRevision)
        {
            RndTrans trans = new RndTrans();
            trans.revision = revision;
            trans.altRevision = altRevision;

            trans.localXfm = new Matrix();
            trans.localXfm.m11 = 1.0f;
            trans.localXfm.m12 = 0.0f;
            trans.localXfm.m13 = 0.0f;
            trans.localXfm.m21 = 0.0f;
            trans.localXfm.m22 = 1.0f;
            trans.localXfm.m23 = 0.0f;
            trans.localXfm.m31 = 0.0f;
            trans.localXfm.m32 = 0.0f;
            trans.localXfm.m33 = 1.0f;
            trans.localXfm.m41 = 0.0f;
            trans.localXfm.m42 = 0.0f;
            trans.localXfm.m43 = 0.0f;

            trans.worldXfm = new Matrix();
            trans.worldXfm.m11 = 1.0f;
            trans.worldXfm.m12 = 0.0f;
            trans.worldXfm.m13 = 0.0f;
            trans.worldXfm.m21 = 0.0f;
            trans.worldXfm.m22 = 1.0f;
            trans.worldXfm.m23 = 0.0f;
            trans.worldXfm.m31 = 0.0f;
            trans.worldXfm.m32 = 0.0f;
            trans.worldXfm.m33 = 1.0f;
            trans.worldXfm.m41 = 0.0f;
            trans.worldXfm.m42 = 0.0f;
            trans.worldXfm.m43 = 0.0f;
            return trans;
        }
    }
}
