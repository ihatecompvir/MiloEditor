using System.Reflection.Metadata;
using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets.Rnd
{
    [Name("TexMovie"), Description("Draws full screen quad with movie")]
    public class RndTexMovie : Object
    {
        public class Movie
        {
            [Name("Filename"), Description("The filename of the movie.")]
            public Symbol name = new(0, "");
            public uint unknown;
            public uint unknown2;

            public bool unkBool;

            private uint byteCount;

            [Name("Movie Bytes"), Description("The bytes of the movie file. Usually an unencrypted Bink movie, but may differ on platform.")]
            public List<byte> bytes = new();

            public Movie Read(EndianReader reader, uint revision)
            {
                name = Symbol.Read(reader);
                if (revision > 1 && revision < 3)
                {
                    unkBool = reader.ReadBoolean();
                    return this;
                }
                unknown = reader.ReadUInt32();
                unknown2 = reader.ReadUInt32();

                byteCount = reader.ReadUInt32();
                for (int i = 0; i < byteCount; i++)
                {
                    bytes.Add(reader.ReadByte());
                }

                return this;
            }

            public void Write(EndianWriter writer, uint revision)
            {
                Symbol.Write(writer, name);

                if (revision > 1 && revision < 3)
                {
                    writer.WriteBoolean(unkBool);
                    return;
                }

                writer.WriteUInt32(unknown);
                writer.WriteUInt32(unknown2);

                writer.WriteUInt32((uint)bytes.Count);
                foreach (var b in bytes)
                {
                    writer.WriteByte(b);
                }
            }
        }

        private ushort altRevision;
        private ushort revision;

        public RndDrawable draw = new();
        public Object obj = new();

        public Symbol outputTexture = new(0, "");

        public bool loop;

        public Movie movie = new();

        public bool fillWidth;

        public byte unk;

        public RndTexMovie Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)(combinedRevision >> 16 & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)(combinedRevision >> 16 & 0xFFFF));

            objFields = objFields.Read(reader, parent, entry);

            draw = draw.Read(reader, false, parent, entry);
            obj = obj.Read(reader, false, parent, entry);

            if (altRevision != 0)
                fillWidth = reader.ReadBoolean();

            outputTexture = Symbol.Read(reader);
            loop = reader.ReadBoolean();

            if (revision < 4)
                unk = reader.ReadByte();

            movie = movie.Read(reader, revision);

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw MiloLib.Exceptions.MiloAssetReadException.EndBytesNotFound(parent, entry, reader.BaseStream.Position);

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)(altRevision << 16 | revision) : (uint)(revision << 16 | altRevision));
            objFields.Write(writer, parent);

            draw.Write(writer, false, parent, true);
            obj.Write(writer, false, parent, entry);

            if (altRevision != 0)
                writer.WriteBoolean(fillWidth);

            Symbol.Write(writer, outputTexture);

            writer.WriteBoolean(loop);

            if (revision < 4)
                writer.WriteByte(unk);

            movie.Write(writer, revision);

            if (standalone)
                writer.WriteEndBytes();
        }
    }
}