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
        public RndAnimatable anim = new();
        [Name("Draw")]
        public RndDrawable draw = new();
        [Name("Trans")]
        public RndTrans trans = new();

        [Name("Environ"), MinVersion(9)]
        public Symbol environ = new(0, "");

        [Name("Test Event"), Description("Test event"), MinVersion(10)]
        public Symbol testEvent = new(0, "");

        [Name("Unknown Floats"), Description("Unknown floats only found in the GH2 4-song demo."), MinVersion(6), MaxVersion(6)]
        public List<float> unknownFloats = new();

        [MinVersion(0), MaxVersion(8)]
        public RndPollable poll = new();
        [MinVersion(0), MaxVersion(8)]
        public Symbol unkSymbol1 = new(0, "");
        [MinVersion(0), MaxVersion(8)]
        public Symbol unkSymbol2 = new(0, "");

        public RndDir Read(EndianReader reader, bool standalone)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            base.Read(reader, false);

            anim = anim.Read(reader);
            draw = draw.Read(reader, false, true);
            trans = trans.Read(reader, false, true);

            if (revision < 9)
            {
                poll = poll.Read(reader, false);
                unkSymbol1 = Symbol.Read(reader);
                unkSymbol2 = Symbol.Read(reader);
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
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            base.Write(writer, false);

            anim.Write(writer);
            draw.Write(writer, false);
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
