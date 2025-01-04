using System.Reflection.Metadata;
using MiloLib.Assets.Rnd;
using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets.Rnd
{
    [Name("AnimFilter"), Description("An AnimFilter object modifies the playing of another animatable object")]
    public class RndAnimFilter : Object
    {
        private Dictionary<Game.MiloGame, uint> gameRevisions = new Dictionary<Game.MiloGame, uint>
        {
            { Game.MiloGame.GuitarHero2_PS2, 1 },
            { Game.MiloGame.GuitarHero2_360, 1 },
            { Game.MiloGame.Phase, 2 },
            { Game.MiloGame.RockBand, 2 },
            { Game.MiloGame.RockBand2, 2 },
            { Game.MiloGame.LegoRockBand, 2 },
            { Game.MiloGame.TheBeatlesRockBand, 2 },
            { Game.MiloGame.GreenDayRockBand, 2 },
            { Game.MiloGame.RockBand3, 2 },
            { Game.MiloGame.DanceCentral, 2 },

            // todo: double check
            { Game.MiloGame.DanceCentral2, 2 },
            { Game.MiloGame.RockBandBlitz, 2 },
            { Game.MiloGame.DanceCentral3, 2 }
        };
        public enum AnimEnum
        {
            kAnimRange,
            kAnimLoop,
            kAnimShuttle
        }

        private ushort altRevision;
        private ushort revision;

        public RndAnimatable anim = new();

        public Symbol animSymbol = new(0, "");

        [Name("Scale"), Description("Multiplier to speed of animation")]
        public float scale;
        [Name("Offset"), Description("Amount to offset frame for animation")]
        public float offset;
        [Name("Start"), Description("Overriden start frame of animation")]
        public float start;
        [Name("End"), Description("Overriden end frame of animation")]
        public float end;

        [Name("Anim Enum"), Description("How to treat the frame outside of start and end")]
        public AnimEnum animEnum;

        [Name("Period"), Description("Alternative to scale, overriden period of animation"), MinVersion(1)]
        public float period;

        [Name("Snap"), Description("Snap frame to nearest multiple"), MinVersion(2)]
        public float snap;
        [Name("Jitter"), Description("Jitter frame randomly up to this amount"), MinVersion(2)]
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