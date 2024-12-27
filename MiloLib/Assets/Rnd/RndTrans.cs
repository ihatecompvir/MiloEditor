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
    [Name("Trans"), Description("Base class for transformable objects. Trans objects have a 3D position, rotation, and scale.")]
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

        public ushort altRevision;
        public ushort revision;

        [Name("Local Transform"), Description("The local transform of the object.")]
        public Matrix localXfm = new();
        [Name("World Transform"), Description("The world transform of the object.")]
        public Matrix worldXfm = new();

        [Name("Constraint"), Description("Trans constraint for the object.")]
        public Constraint constraint;

        [Name("Target"), Description("Target according to the constraint.")]
        public Symbol target = new(0, "");

        [Name("Preserve Scale"), Description("Preserve scale if applying dynamic constraint.")]
        public bool preserveScale;

        [Name("Parent"), Description("Object this is linked to.")]
        public Symbol parent = new(0, "");

        public RndTrans Read(EndianReader reader, bool standalone)
        {
            altRevision = reader.ReadUInt16();
            revision = reader.ReadUInt16();

            if (revision != 9)
            {
                throw new UnsupportedAssetRevisionException("RndTrans", revision);
            }

            if (standalone)
            {
                base.objFields.Read(reader);
            }

            localXfm = localXfm.Read(reader);
            worldXfm = worldXfm.Read(reader);

            constraint = (Constraint)reader.ReadUInt32();

            target = Symbol.Read(reader);

            preserveScale = reader.ReadBoolean();

            parent = Symbol.Read(reader);

            if (standalone)
            {
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw new Exception("Got to end of standalone asset but didn't find the expected end bytes, read likely did not succeed");
            }

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone)
        {
            writer.WriteUInt16(altRevision);
            writer.WriteUInt16(revision);

            if (standalone)
            {
                base.objFields.Write(writer);
            }

            localXfm.Write(writer);
            worldXfm.Write(writer);

            writer.WriteUInt32((uint)constraint);

            Symbol.Write(writer, target);

            writer.WriteByte((byte)(preserveScale ? 1 : 0));

            Symbol.Write(writer, parent);

            if (standalone)
            {
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
            }
        }
    }
}
