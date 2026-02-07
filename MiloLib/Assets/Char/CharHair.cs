using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets.Char
{
    [Name("CharHair"), Description("Hair physics, deals with strands of hair")]
    public class CharHair : Object
    {
        public class Point
        {
            public Vector3 pos = new();
            public Vector3 force = new();
            public Vector3 lastFriction = new();
            public Vector3 lastZ = new();
            [Name("Bone"), Description("hair bone we set the transform of")]
            public Symbol bone = new(0, "");
            [Name("Length"), Description("the length of this strand bone")]
            public float length;
            private uint collidesCount;
            [Name("Collides"), Description("things to collide against")]
            public List<Symbol> collides = new();
            [Name("Radius"), Description("collision radius")]
            public float radius;
            [Name("Outer Radius"), Description("if > radius, is the distance the hair bone should start aligning itself with the collision primitive, so that once touching it, it will be totally flattened against it.")]
            public float outerRadius;
            [Name("Side Length"), Description("if >= 0 the base length to the side modified by min_slack and max_slack")]
            public float sideLength;
            public Vector3 unk5c = new();

            public Symbol unkSym = new(0, "");
            public Symbol unkSym2 = new(0, "");

            public int unkInt1;
            public int unkInt2;
            public int unkInt3;

            public float addToRadius;

            public bool unkBool;


            public Point Read(EndianReader reader, uint revision)
            {
                pos.Read(reader);
                bone = Symbol.Read(reader);
                length = reader.ReadFloat();
                if (revision < 3)
                {
                    unkInt3 = reader.ReadInt32();
                    unkSym = Symbol.Read(reader);
                }
                else if (revision == 3)
                {
                    unkInt1 = reader.ReadInt32();
                }

                radius = reader.ReadFloat();

                if (revision > 1)
                    outerRadius = reader.ReadFloat();

                if (revision == 6 || revision == 7 || revision == 8)
                {
                    addToRadius = reader.ReadFloat();
                }

                if (revision == 6)
                {
                    unkSym2 = Symbol.Read(reader);
                }

                if (revision < 8)
                {
                    if (revision > 5)
                    {
                        unkInt2 = reader.ReadInt32();
                    }
                }
                else
                {
                    if (revision < 9)
                        unkBool = reader.ReadBoolean();
                    sideLength = reader.ReadFloat();
                }
                if (revision > 9)
                    unk5c = unk5c.Read(reader);

                return this;
            }

            public void Write(EndianWriter writer, uint revision)
            {
                pos.Write(writer);
                Symbol.Write(writer, bone);
                writer.WriteFloat(length);
                if (revision < 3)
                {
                    writer.WriteInt32(unkInt3);
                    Symbol.Write(writer, unkSym);
                }
                else if (revision == 3)
                {
                    writer.WriteInt32(unkInt1);
                }

                writer.WriteFloat(radius);

                if (revision > 1)
                    writer.WriteFloat(outerRadius);

                if (revision == 6 || revision == 7 || revision == 8)
                {
                    writer.WriteFloat(addToRadius);
                }

                if (revision == 6)
                {
                    Symbol.Write(writer, unkSym2);
                }

                if (revision < 8)
                {
                    if (revision > 5)
                    {
                        writer.WriteInt32(unkInt2);
                    }
                }
                else
                {
                    if (revision < 9)
                        writer.WriteBoolean(unkBool);
                    writer.WriteFloat(sideLength);
                }
                if (revision > 9)
                    unk5c.Write(writer);
            }
        }

        public class Strand
        {
            [Name("Root"), Description("The root Trans for the hair strand")]
            public Symbol root = new(0, "");
            [Name("Angle"), Description("Angle in degrees of starting flip")]
            public float angle;
            private uint pointCount;
            public List<Point> points = new();
            public Matrix3 baseMat = new();
            public Matrix3 rootMat = new();
            public int hookupFlags;

            public Strand Read(EndianReader reader, uint revision)
            {
                root = Symbol.Read(reader);
                angle = reader.ReadFloat();
                pointCount = reader.ReadUInt32();
                for (int i = 0; i < pointCount; i++)
                {
                    Point point = new Point();
                    point.Read(reader, revision);
                    points.Add(point);
                }
                baseMat = baseMat.Read(reader);
                rootMat = rootMat.Read(reader);
                if (revision > 2)
                    hookupFlags = reader.ReadInt32();

                return this;
            }

            public void Write(EndianWriter writer, uint revision)
            {
                Symbol.Write(writer, root);
                writer.WriteFloat(angle);
                writer.WriteUInt32((uint)points.Count);
                foreach (var point in points)
                {
                    point.Write(writer, revision);
                }
                baseMat.Write(writer);
                rootMat.Write(writer);
                if (revision > 2)
                    writer.WriteInt32(hookupFlags);
            }
        }
        private ushort altRevision;
        private ushort revision;

        [Name("Stiffness"), Description("stiffness of each strand")]
        public float stiffness;
        [Name("Torsion"), Description("rotational stiffness of each strand")]
        public float torsion;
        [Name("Inertia"), Description("Inertia of the hair, zero means none")]
        public float inertia;
        [Name("Gravity"), Description("Gravity of the hair, one is normal")]
        public float gravity;
        [Name("Weight"), Description("Weight of the hair, one is normal")]
        public float weight;
        [Name("Friction"), Description("Hair friction against each other")]
        public float friction;
        [Name("Min Slack"), Description("If using sides, determines how far in it could go"), MinVersion(9)]
        public float minSlack;
        [Name("Max Slack"), Description("If using sides, determines how far out it could go"), MinVersion(9)]
        public float maxSlack;
        private uint strandCount;
        [Name("Hair Strands")]
        public List<Strand> strands = new();
        [Name("Simulate"), Description("Simulate physics or not")]
        public bool simulate;

        [Name("Wind"), Description("wind object to use"), MinVersion(10)]
        public Symbol wind = new(0, "");

        public CharHair Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            base.Read(reader, false, parent, entry);

            stiffness = reader.ReadFloat();
            torsion = reader.ReadFloat();
            inertia = reader.ReadFloat();
            gravity = reader.ReadFloat();
            weight = reader.ReadFloat();
            friction = reader.ReadFloat();
            if (revision > 8)
            {
                minSlack = reader.ReadFloat();
                maxSlack = reader.ReadFloat();
            }

            strandCount = reader.ReadUInt32();
            for (int i = 0; i < strandCount; i++)
            {
                Strand strand = new Strand();
                strand.Read(reader, revision);
                strands.Add(strand);
            }

            simulate = reader.ReadBoolean();

            if (revision > 10)
                wind = Symbol.Read(reader);

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw MiloLib.Exceptions.MiloAssetReadException.EndBytesNotFound(parent, entry, reader.BaseStream.Position);

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            base.Write(writer, false, parent, entry);

            writer.WriteFloat(stiffness);
            writer.WriteFloat(torsion);
            writer.WriteFloat(inertia);
            writer.WriteFloat(gravity);
            writer.WriteFloat(weight);
            writer.WriteFloat(friction);
            if (revision > 8)
            {
                writer.WriteFloat(minSlack);
                writer.WriteFloat(maxSlack);
            }

            writer.WriteUInt32((uint)strands.Count);
            foreach (var strand in strands)
            {
                strand.Write(writer, revision);
            }

            writer.WriteBoolean(simulate);

            if (revision > 10)
                Symbol.Write(writer, wind);

            if (standalone)
                writer.WriteEndBytes();
        }

    }
}
