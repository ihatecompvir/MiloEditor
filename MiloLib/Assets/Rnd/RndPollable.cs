using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets.Rnd
{
    [Name("Pollable"), Description("Abstract base class for pollable RND objects")]
    public class RndPollable : Object
    {
        public Symbol exit = new(0, "");
        public Symbol enter = new(0, "");

        public RndPollable Read(EndianReader reader, bool standalone, DirectoryMeta parent)
        {
            exit = Symbol.Read(reader);
            enter = Symbol.Read(reader);

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw new Exception("Got to end of standalone asset but didn't find the expected end bytes, read likely did not succeed");
            return this;
        }

        public override void Write(EndianWriter writer, bool standalone)
        {
            Symbol.Write(writer, exit);
            Symbol.Write(writer, enter);
            if (standalone)
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
        }
    }
}