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

            // if a RndTrans is read as a standalone object, it has the Object fields
            // otherwise it does not
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
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw new Exception("Got to end of standalone asset but didn't find the expected end bytes, read likely did not succeed");

            return this;
        }

        public void Write(EndianWriter writer, bool standalone, bool skipMetadata = false)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            if (standalone && !skipMetadata)
            {
                base.objFields.Write(writer);
            }

            localXfm.Write(writer);
            worldXfm.Write(writer);

            if (revision < 9)
            {
                writer.WriteUInt32((uint)transObjects.Count);
                foreach (var obj in transObjects)
                {
                    Symbol.Write(writer, obj);
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
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
            }
        }
    }
}
