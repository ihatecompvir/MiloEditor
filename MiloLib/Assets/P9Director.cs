using MiloLib.Assets.Rnd;
using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets
{
    [Name("P9 Director"), Description("P9 Director, sits in each song file and manages camera + scene changes")]
    public class P9Director : Object
    {
        public uint revision;

        public ObjectFields objFields1 = new();
        public ObjectFields objFields2 = new();

        public RndDraw draw = new();

        public Symbol venue = new(0, "");

        public P9Director Read(EndianReader reader, bool standalone)
        {
            revision = reader.ReadUInt32();

            if (revision != 5)
                throw new UnsupportedAssetRevisionException("P9Director", revision);

            objFields1 = objFields1.Read(reader);
            objFields2 = objFields2.Read(reader);

            draw = new RndDraw().Read(reader);
            venue = Symbol.Read(reader);

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw new Exception("Got to end of standalone asset but didn't find the expected end bytes, read likely did not succeed");
            return this;
        }

        public override void Write(EndianWriter writer, bool standalone)
        {
            writer.WriteUInt32(revision);

            objFields1.Write(writer);
            objFields2.Write(writer);

            draw.Write(writer);
            Symbol.Write(writer, venue);

            if (standalone)
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
        }
    }
}