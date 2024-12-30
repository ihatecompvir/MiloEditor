using System.Numerics;
using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets.Rnd
{
    [Name("ScreenMask"), Description("Draws full screen quad with material and color.")]
    public class RndScreenMask : Object
    {
        public ushort altRevision;
        public ushort revision;

        public RndDrawable draw = new();

        public Symbol material = new(0, "");
        public HmxColor4 color = new();
        public float alpha;

        public Rect rect = new();

        public bool useCamRect;

        public RndScreenMask Read(EndianReader reader, bool standalone)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            base.Read(reader, false);

            draw = draw.Read(reader, false, true);

            material = Symbol.Read(reader);
            color = color.Read(reader);

            rect = rect.Read(reader);

            useCamRect = reader.ReadBoolean();

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw new Exception("Got to end of standalone asset but didn't find the expected end bytes, read likely did not succeed");

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            base.Write(writer, false);

            draw.Write(writer, false);

            Symbol.Write(writer, material);
            color.Write(writer);
            writer.WriteFloat(alpha);

            rect.Write(writer);

            writer.WriteBoolean(useCamRect);

            if (standalone)
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
        }
    }
}