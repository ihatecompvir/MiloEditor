using MiloLib.Utils;
using MiloLib.Classes;

namespace MiloLib.Assets.Rnd
{
    [Name("RndDir"), Description("A RndDir specially tracks drawable and animatable objects.")]
    public class RndDir : ObjectDir
    {
        public uint revision;

        public RndAnim anim = new();
        public RndDraw draw = new();
        public RndTrans trans = new();

        public Symbol environ = new(0, "");

        [Name("Test Event"), Description("Test event")]
        public Symbol testEvent = new(0, "");

        public RndDir Read(EndianReader reader, bool standalone)
        {
            revision = reader.ReadUInt32();

            if (revision != 10)
            {
                throw new UnsupportedAssetRevisionException("RndDir", revision);
            }

            base.Read(reader, false);

            anim = anim.Read(reader);
            draw = draw.Read(reader);
            trans = trans.Read(reader, false);

            environ = Symbol.Read(reader);

            testEvent = Symbol.Read(reader);

            if (standalone)
            {
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw new Exception("Got to end of standalone asset but didn't find the expected end bytes, read likely did not succeed");
            }

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone)
        {
            writer.WriteUInt32(revision);

            base.Write(writer, false);

            anim.Write(writer);
            draw.Write(writer);
            trans.Write(writer, false);

            Symbol.Write(writer, environ);
            Symbol.Write(writer, testEvent);

            if (standalone)
            {
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
            }

        }


        public override bool IsDirectory()
        {
            return true;
        }
    }
}
