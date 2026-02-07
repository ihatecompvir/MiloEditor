using MiloLib.Assets.Rnd;
using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets.Char
{
    // this description will always sound very metal
    [Name("CharCollide"), Description("Feeds the bones when executed.")]
    public class CharCollide : Object
    {
        public enum Shape
        {
            kPlane = 0,
            kSphere = 1,
            kInsideSphere = 2,
            kCigar = 3,
            kInsideCigar = 4,
        };

        public struct CharCollideStruct
        {
            public int unk0;
            public Vector3 vec;

            public void Read(EndianReader reader)
            {
                unk0 = reader.ReadInt32();
                vec = new Vector3().Read(reader);
            }

            public void Write(EndianWriter writer)
            {
                writer.WriteInt32(unk0);
                vec.Write(writer);
            }
        };

        private ushort altRevision;
        private ushort revision;

        public RndTrans trans = new();

        [Name("Shape"), Description("Type of collision")]
        public Shape shape;

        [Name("Radius0"), Description("Radius of the sphere, or of length0 hemisphere if cigar")]
        public float[] origRadius = new float[2] { 0, 0 };
        [Name("Length0"), Description("cigar: placement of radius0 hemisphere along X axis, must be < than length0, not used for sphere shapes")]
        public float[] origLength = new float[2] { 0, 0 };
        [Name("Radius1"), Description("cigar: Radius of length1 hemisphere")]
        public float[] curRadius = new float[2] { 0, 0 };
        [Name("Length1"), Description("cigar: placement of radius1 hemisphere along X axis, must be >= length0")]
        public float[] curLength = new float[2] { 0, 0 };

        [MinVersion(2)]
        public int flags;

        [MinVersion(6)]
        public Matrix unknownTransform = new();

        [Name("Mesh"), Description("Optional mesh that will deform, used to resize ourselves.  If this is set, make sure you are not parented to any bone with scale, such as an exo bone"), MinVersion(6)]
        public Symbol mesh = new(0, "");

        [MinVersion(6)]
        public List<CharCollideStruct> structs = new(8);

        [MinVersion(6)]
        public byte[] sha1Digest = new byte[20];

        [Name("Mesh Y Bias"), Description("For spheres + cigars, finds mesh points along positive y axis (the green one), makes a better fit for spheres where only one side should be the fit, like for chest and back collision volumes"), MinVersion(6)]
        public bool meshYBias;

        public CharCollide Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            base.Read(reader, false, parent, entry);
            trans.Read(reader, false, parent, entry);

            shape = (Shape)reader.ReadUInt32();

            origRadius[0] = reader.ReadFloat();

            if (revision > 4)
                origLength[0] = reader.ReadFloat();
            if (revision > 2)
                origLength[1] = reader.ReadFloat();
            if (revision > 1)
                flags = reader.ReadInt32();
            if (revision > 3)
                curRadius[0] = reader.ReadFloat();

            if (revision > 5)
            {
                origRadius[1] = reader.ReadFloat();
                curRadius[1] = reader.ReadFloat();
                curLength[0] = reader.ReadFloat();
                curLength[1] = reader.ReadFloat();
                unknownTransform.Read(reader);
                mesh = Symbol.Read(reader);
                for (int i = 0; i < 8; i++)
                {
                    CharCollideStruct unkStruct = new CharCollideStruct();
                    unkStruct.Read(reader);
                    structs.Add(unkStruct);
                }

                sha1Digest = reader.ReadBlock(20);
                meshYBias = reader.ReadBoolean();
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

            writer.WriteUInt32((uint)shape);

            writer.WriteFloat(origRadius[0]);

            if (revision > 4)
                writer.WriteFloat(origLength[0]);
            if (revision > 2)
                writer.WriteFloat(origLength[1]);
            if (revision > 1)
                writer.WriteInt32(flags);
            if (revision > 3)
                writer.WriteFloat(curRadius[0]);

            if (revision > 5)
            {
                writer.WriteFloat(origRadius[1]);
                writer.WriteFloat(curRadius[1]);
                writer.WriteFloat(curLength[0]);
                writer.WriteFloat(curLength[1]);
                unknownTransform.Write(writer);
                Symbol.Write(writer, mesh);
                
                while (structs.Count < 8)
                {
                    structs.Add(new CharCollideStruct());
                }
                for (int i = 0; i < 8; i++)
                {
                    structs[i].Write(writer);
                }
                
                writer.WriteBlock(sha1Digest);
                writer.WriteBoolean(meshYBias);
            }

            if (standalone)
                writer.WriteEndBytes();
        }

    }
}
