using System.Reflection.Metadata;
using MiloLib.Assets.Rnd;
using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets.Rnd
{
    [Name("AnimFilter"), Description("An AnimFilter object modifies the playing of another animatable object")]
    public class RndAnimFilter : Object
    {
        public enum AnimEnum
        {
            kAnimRange,
            kAnimLoop,
            kAnimShuttle
        }

        public ushort altRevision;
        public ushort revision;

        public RndAnimatable anim = new();

        public Symbol animSymbol = new(0, "");

        public float scale;
        public float offset;
        public float start;
        public float end;

        public AnimEnum animEnum;

        [MinVersion(1)]
        public float period;

        [MinVersion(2)]
        public float snap;
        [MinVersion(2)]
        public float jitter;

        public RndAnimFilter Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            objFields = objFields.Read(reader, parent, entry);

            anim = anim.Read(reader, parent, entry);

            animSymbol = Symbol.Read(reader);

            scale = reader.ReadFloat();
            offset = reader.ReadFloat();
            start = reader.ReadFloat();
            end = reader.ReadFloat();

            if (revision < 1)
            {
                animEnum = (AnimEnum)reader.ReadByte();
            }
            else
            {
                animEnum = (AnimEnum)reader.ReadUInt32();
                period = reader.ReadFloat();
            }

            if (revision > 1)
            {
                snap = reader.ReadFloat();
                jitter = reader.ReadFloat();
            }

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw new Exception("Got to end of standalone asset but didn't find the expected end bytes, read likely did not succeed");

            return this;
        }

        public void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            objFields.Write(writer);

            anim.Write(writer);

            Symbol.Write(writer, animSymbol);

            writer.WriteFloat(scale);
            writer.WriteFloat(offset);
            writer.WriteFloat(start);
            writer.WriteFloat(end);

            if (revision < 1)
            {
                writer.WriteByte((byte)animEnum);
            }
            else
            {
                writer.WriteUInt32((uint)animEnum);
                writer.WriteFloat(period);
            }

            if (revision > 1)
            {
                writer.WriteFloat(snap);
                writer.WriteFloat(jitter);
            }

            if (standalone)
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
        }
    }
}