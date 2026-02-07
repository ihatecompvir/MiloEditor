using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets.Rnd
{
    [Name("Wind"), Description("Object representing blowing wind, CharHair and Fur can point at them.")]
    public class RndWind : Object
    {
        private ushort altRevision;
        private ushort revision;

        [Name("Prevailing"), Description("Prevailing wind in inches/sec, along each world space axis, adds to random component, 1 mph == 17 inches/sec")]
        public Vector3 prevailing = new();
        [Name("Random"), Description("Random wind speed in inches/sec, along each world axis, adds to prevailing wind, 1 mph == 17 inches/sec")]
        public Vector3 random = new();

        [Name("Time Loop"), Description("how long in seconds before the wind loops, 50 is a nice default")]
        public float timeLoop;
        [Name("Space Loop"), Description("how far in inches before the wind loops, 100 is a nice default")]
        public float spaceLoop;

        [Name("Wind Owner"), Description("Wind owner for the wind, properties shown are not for the owner, however, you must edit it directly")]
        public Symbol windOwner = new(0, "");

        public RndWind Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            base.Read(reader, false, parent, entry);

            prevailing = prevailing.Read(reader);
            random = random.Read(reader);

            timeLoop = reader.ReadFloat();
            spaceLoop = reader.ReadFloat();

            if (revision > 1)
            {
                windOwner = Symbol.Read(reader);
            }

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw MiloLib.Exceptions.MiloAssetReadException.EndBytesNotFound(parent, entry, reader.BaseStream.Position);

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            base.Write(writer, false, parent, entry);

            prevailing.Write(writer);
            random.Write(writer);

            writer.WriteFloat(timeLoop);
            writer.WriteFloat(spaceLoop);

            if (revision > 1)
            {
                Symbol.Write(writer, windOwner);
            }

            if (standalone)
                writer.WriteEndBytes();
        }

    }
}
