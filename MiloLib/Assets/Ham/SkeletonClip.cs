using MiloLib.Assets.Rnd;
using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets.Ham
{
    [Name("SkeletonClip"), Description("An animated clip of a skeleton playable in milo")]
    public class SkeletonClip : Object
    {
        private ushort altRevision;
        private ushort revision;

        public RndAnimatable anim = new();

        public Symbol unknownSymbol = new(0, "");
        public Symbol unknownSymbol2 = new(0, "");
        public Symbol unknownSymbol3 = new(0, "");

        public uint unkInt1;
        public uint unkInt2;
        public uint unkInt3;
        public bool unkBool;
        public bool unkBool2;


        // TODO: finish this
        // it seems to be mostly accurate but not 100%
        public SkeletonClip Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)(combinedRevision >> 16 & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)(combinedRevision >> 16 & 0xFFFF));

            anim = anim.Read(reader, parent, entry);

            if (revision != 0)
            {
                unkInt1 = reader.ReadUInt32();
                unkBool = reader.ReadBoolean();
                if (revision < 4)
                {
                    unkBool2 = reader.ReadBoolean();
                }
            }
            if (revision > 1)
            {
                unkInt2 = reader.ReadUInt32();
                unkInt3 = reader.ReadUInt32();
            }

            if (revision > 5)
            {
                unknownSymbol = Symbol.Read(reader);
            }

            unknownSymbol2 = Symbol.Read(reader);
            unknownSymbol3 = Symbol.Read(reader);


            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw new Exception("Got to end of standalone asset but didn't find the expected end bytes, read likely did not succeed");

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)(altRevision << 16 | revision) : (uint)(revision << 16 | altRevision));

            anim.Write(writer);

            if (revision != 0)
            {
                writer.WriteUInt32(unkInt1);
                writer.WriteBoolean(unkBool);
                if (revision < 4)
                {
                    writer.WriteBoolean(unkBool2);
                }
            }

            if (revision > 1)
            {
                writer.WriteUInt32(unkInt2);
                writer.WriteUInt32(unkInt3);
            }

            if (revision > 5)
            {
                Symbol.Write(writer, unknownSymbol);
            }

            Symbol.Write(writer, unknownSymbol2);
            Symbol.Write(writer, unknownSymbol3);

            if (standalone)
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
        }

        public override bool IsDirectory()
        {
            return true;
        }

    }
}
