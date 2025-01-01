using System.Reflection.Metadata;
using MiloLib.Assets.Rnd;
using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets
{
    [Name("TexMovie"), Description("Draws full screen quad with movie")]
    public class TexMovie : Object
    {
        public class Movie
        {
            [Name("Filename"), Description("The filename of the movie.")]
            public Symbol name = new(0, "");
            public uint unknown;
            public uint unknown2;

            private uint byteCount;

            [Name("Movie Bytes"), Description("The bytes of the movie file. Usually an unencrypted Bink movie, but may differ on platform.")]
            public List<byte> bytes = new();

            public Movie Read(EndianReader reader)
            {
                name = Symbol.Read(reader);
                unknown = reader.ReadUInt32();
                unknown2 = reader.ReadUInt32();

                byteCount = reader.ReadUInt32();
                for (int i = 0; i < byteCount; i++)
                {
                    bytes.Add(reader.ReadByte());
                }

                return this;
            }

            public void Write(EndianWriter writer)
            {
                Symbol.Write(writer, name);

                writer.WriteUInt32(unknown);
                writer.WriteUInt32(unknown2);

                writer.WriteUInt32((uint)bytes.Count);
                foreach (var b in bytes)
                {
                    writer.WriteByte((byte)b);
                }
            }
        }

        public ushort altRevision;
        public ushort revision;

        public RndDrawable draw = new();
        public Object obj = new();

        public Symbol outputTexture = new(0, "");

        public bool loop;

        public Movie movie = new();

        public TexMovie Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            objFields = objFields.Read(reader, parent, entry);

            draw = draw.Read(reader, false, parent, entry);
            obj = obj.Read(reader, false, parent, entry);

            outputTexture = Symbol.Read(reader);

            loop = reader.ReadBoolean();
            movie = movie.Read(reader);

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw new Exception("Got to end of standalone asset but didn't find the expected end bytes, read likely did not succeed");

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));
            objFields.Write(writer);

            draw.Write(writer, false, true);
            obj.Write(writer, false, parent, entry);

            Symbol.Write(writer, outputTexture);

            writer.WriteBoolean(loop);

            movie.Write(writer);

            if (standalone)
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
        }
    }
}