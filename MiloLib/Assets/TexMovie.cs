using System.Reflection.Metadata;
using MiloLib.Assets.Rnd;
using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets
{
    [Name("TexMovie"), Description("Draws full screen quad with movie")]
    public class TexMovie : Object
    {
        public ushort altRevision;
        public ushort revision;

        public RndDrawable draw = new();
        public Object obj = new();

        public Symbol outputTexture = new(0, "");

        public bool loop;
        public bool preload;

        public Symbol movieFile = new(0, "");
        public bool drawPreClear;

        public TexMovie Read(EndianReader reader, bool standalone)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            objFields = objFields.Read(reader);

            draw = draw.Read(reader, false, true);
            obj = obj.Read(reader, false);

            outputTexture = Symbol.Read(reader);

            loop = reader.ReadBoolean();
            preload = reader.ReadBoolean();

            movieFile = Symbol.Read(reader);

            drawPreClear = reader.ReadBoolean();

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw new Exception("Got to end of standalone asset but didn't find the expected end bytes, read likely did not succeed");

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));
            objFields.Write(writer);

            draw.Write(writer, false, true);
            obj.Write(writer, false);

            Symbol.Write(writer, outputTexture);

            writer.WriteBoolean(loop);
            writer.WriteBoolean(preload);

            Symbol.Write(writer, movieFile);

            writer.WriteBoolean(drawPreClear);

            if (standalone)
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
        }
    }
}