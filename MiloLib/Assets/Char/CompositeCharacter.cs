using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets.Char
{
    [Name("CompositeCharacter"), Description("Character with outfits and compositing decal texture tech")]
    public class CompositeCharacter : Character
    {
        [HideInInspector]
        public ushort altRevision;
        [HideInInspector]
        public ushort revision;

        public bool unkBool;

        public uint unkInt1;
        public uint unkInt2;
        public uint unkInt3;
        public uint unkInt4;
        public uint unkInt5;
        public uint unkInt6;
        public uint unkInt7;
        public uint unkInt8;

        public uint unkInt9;
        public uint unkInt10;

        public Symbol gender = new(0, "");
        public Symbol unkSym1 = new(0, "");

        public float unkFloat1;
        public float unkFloat2;



        public CompositeCharacter(ushort revision, ushort altRevision = 0) : base(revision, altRevision)
        {
            revision = revision;
            altRevision = altRevision;
            return;
        }

        public CompositeCharacter Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            base.Read(reader, false, parent, entry);

            unkInt1 = reader.ReadUInt32();

            unkBool = reader.ReadBoolean();

            unkInt2 = reader.ReadUInt32();
            unkInt3 = reader.ReadUInt32();
            unkInt4 = reader.ReadUInt32();
            unkInt5 = reader.ReadUInt32();
            unkInt6 = reader.ReadUInt32();
            unkInt7 = reader.ReadUInt32();
            unkInt8 = reader.ReadUInt32();

            gender = Symbol.Read(reader);

            unkInt9 = reader.ReadUInt32();
            unkInt10 = reader.ReadUInt32();

            unkSym1 = Symbol.Read(reader);

            unkFloat1 = reader.ReadFloat();
            unkFloat2 = reader.ReadFloat();

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw MiloLib.Exceptions.MiloAssetReadException.EndBytesNotFound(parent, entry, reader.BaseStream.Position);

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            base.Write(writer, false, parent, entry);

            writer.WriteUInt32(unkInt1);

            writer.WriteBoolean(unkBool);

            writer.WriteUInt32(unkInt2);
            writer.WriteUInt32(unkInt3);
            writer.WriteUInt32(unkInt4);
            writer.WriteUInt32(unkInt5);
            writer.WriteUInt32(unkInt6);
            writer.WriteUInt32(unkInt7);
            writer.WriteUInt32(unkInt8);

            Symbol.Write(writer, gender);

            writer.WriteUInt32(unkInt9);
            writer.WriteUInt32(unkInt10);

            Symbol.Write(writer, unkSym1);

            writer.WriteFloat(unkFloat1);
            writer.WriteFloat(unkFloat2);

            if (standalone)
                writer.WriteEndBytes();
        }

        public override bool IsDirectory()
        {
            return true;
        }

    }
}
