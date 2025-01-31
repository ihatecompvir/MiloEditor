using MiloLib.Assets.Rnd;
using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets
{
    [Name("StreakMeterDir"), Description("streak meter for band tracks")]
    public class StreakMeterDir : RndDir
    {
        private ushort altRevision;
        private ushort revision;

        public int streakMultiplier;
        public int bandMultiplier;
        public int maxMultiplier;

        public Symbol newStreakTrig = new(0, "");
        public Symbol endStreakTrig = new(0, "");
        public Symbol unkTrig = new(0, "");
        public Symbol multiMeterAnim = new(0, "");
        public Symbol multiplierLabel = new(0, "");
        public Symbol textObject = new(0, "");
        public Symbol meterWipeAnim = new(0, "");
        public Symbol matObject = new(0, "");
        public Symbol starDeployTrig = new(0, "");
        public Symbol endOverdriveTrig = new(0, "");
        public Symbol resetTrig = new(0, "");

        public StreakMeterDir(ushort revision, ushort altRevision = 0) : base(revision, altRevision)
        {
            revision = revision;
            altRevision = altRevision;
            return;
        }

        public StreakMeterDir Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            streakMultiplier = reader.ReadInt32();
            bandMultiplier = reader.ReadInt32();
            maxMultiplier = reader.ReadInt32();
            if (!entry.isProxy)
            {
                newStreakTrig = Symbol.Read(reader);
                endStreakTrig = Symbol.Read(reader);
                if (revision < 3)
                    unkTrig = Symbol.Read(reader);
                multiMeterAnim = Symbol.Read(reader);
                if (revision >= 1)
                    multiplierLabel = Symbol.Read(reader);
                else
                    textObject = Symbol.Read(reader);
                if (revision >= 2)
                    meterWipeAnim = Symbol.Read(reader);
                else
                    matObject = Symbol.Read(reader);
                starDeployTrig = Symbol.Read(reader);
                endOverdriveTrig = Symbol.Read(reader);
                resetTrig = Symbol.Read(reader);
            }

            base.Read(reader, false, parent, entry);

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw new Exception("Got to end of standalone asset but didn't find the expected end bytes, read likely did not succeed");

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            writer.WriteInt32(streakMultiplier);
            writer.WriteInt32(bandMultiplier);
            writer.WriteInt32(maxMultiplier);
            if (!entry.isProxy)
            {
                Symbol.Write(writer, newStreakTrig);
                Symbol.Write(writer, endStreakTrig);
                if (revision < 3)
                    Symbol.Write(writer, unkTrig);
                Symbol.Write(writer, multiMeterAnim);
                if (revision >= 1)
                    Symbol.Write(writer, multiplierLabel);
                else
                    Symbol.Write(writer, textObject);
                if (revision >= 2)
                    Symbol.Write(writer, meterWipeAnim);
                else
                    Symbol.Write(writer, matObject);
                Symbol.Write(writer, starDeployTrig);
                Symbol.Write(writer, endOverdriveTrig);
                Symbol.Write(writer, resetTrig);
            }

            base.Write(writer, false, parent, entry);

            if (standalone)
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
        }

        public override bool IsDirectory()
        {
            return true;
        }

    }
}
