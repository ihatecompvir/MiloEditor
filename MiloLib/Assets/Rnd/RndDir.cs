using MiloLib.Utils;
using MiloLib.Classes;

namespace MiloLib.Assets.Rnd
{
    [Name("RndDir"), Description("A RndDir specially tracks drawable and animatable objects.")]
    public class RndDir : ObjectDir
    {
        public ushort altRevision;
        public ushort revision;

        [Name("Anim")]
        public RndAnim anim = new();
        [Name("Draw")]
        public RndDraw draw = new();
        [Name("Trans")]
        public RndTrans trans = new();

        [Name("Environ"), MinVersion(9)]
        public Symbol environ = new(0, "");

        [Name("Test Event"), Description("Test event"), MinVersion(10)]
        public Symbol testEvent = new(0, "");

        [Name("Unknown Floats"), Description("Unknown floats only found in the GH2 4-song demo."), MinVersion(6), MaxVersion(6)]
        public List<float> unknownFloats = new();

        public RndDir Read(EndianReader reader, bool standalone)
        {
            altRevision = reader.ReadUInt16();
            revision = reader.ReadUInt16();

            base.Read(reader, false);

            anim = anim.Read(reader);
            draw = draw.Read(reader);
            trans = trans.Read(reader, false);

            if (revision < 9)
            {
                // TODO: add Poll and read it here
            }
            else
            {
                environ = Symbol.Read(reader);
                if (revision >= 10)
                    testEvent = Symbol.Read(reader);
            }

            if (revision == 6)
            {
                for (int i = 0; i < 8; i++)
                {
                    unknownFloats.Add(reader.ReadFloat());
                }
            }

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw new Exception("Got to end of standalone asset but didn't find the expected end bytes, read likely did not succeed");

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone)
        {
            writer.WriteUInt16(altRevision);
            writer.WriteUInt16(revision);

            base.Write(writer, false);

            anim.Write(writer);
            draw.Write(writer);
            trans.Write(writer, false);

            if (revision < 9)
            {
                // TODO: add Poll and write it here
            }
            else
            {
                Symbol.Write(writer, environ);
                if (revision >= 10)
                    Symbol.Write(writer, testEvent);
            }

            if (revision == 6)
            {
                for (int i = 0; i < 8; i++)
                {
                    writer.WriteFloat(unknownFloats[i]);
                }
            }

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
