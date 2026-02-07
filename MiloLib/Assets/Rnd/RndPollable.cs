using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets.Rnd
{
    [Name("Pollable"), Description("Abstract base class for pollable RND perObjs")]
    public class RndPollable : Object
    {
        public Symbol exit = new(0, "");
        public Symbol enter = new(0, "");

        public RndPollable Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            exit = Symbol.Read(reader);
            enter = Symbol.Read(reader);

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw MiloLib.Exceptions.MiloAssetReadException.EndBytesNotFound(parent, entry, reader.BaseStream.Position);
            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            Symbol.Write(writer, exit);
            Symbol.Write(writer, enter);
            if (standalone)
                writer.WriteEndBytes();
        }
    }
}