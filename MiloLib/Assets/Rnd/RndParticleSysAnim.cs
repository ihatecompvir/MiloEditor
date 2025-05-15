using MiloLib.Classes;
using MiloLib.Utils;
using static MiloLib.Assets.Rnd.PropKey;


namespace MiloLib.Assets.Rnd
{
    [Name("RndParticleSysAnim"), Description("Object that animates Particle System properties.")]
    public class RndParticleSysAnim : Object
    {
        private ushort altRevision;
        private ushort revision;

        public RndAnimatable anim = new();

        public Symbol particleSys = new(0, "");
        public Symbol keysOwner = new(0, "");

        private uint startColorKeysCount;
        public List<ColorKey> startColorKeys = new List<ColorKey>();

        private uint endColorKeysCount;
        public List<ColorKey> endColorKeys = new List<ColorKey>();

        private uint emitRateKeysCount;
        [MinVersion(2)]
        public List<Vec2Key> emitRateKeys = new List<Vec2Key>();

        private uint speedKeysCount;
        [MinVersion(2)]
        public List<Vec2Key> speedKeys = new List<Vec2Key>();

        private uint lifeKeysCount;
        [MinVersion(2)]
        public List<Vec2Key> lifeKeys = new List<Vec2Key>();

        private uint startSizeKeysCount;
        [MinVersion(2)]
        public List<Vec2Key> startSizeKeys = new List<Vec2Key>();

        private uint fKeysCount;
        [MaxVersion(1)]
        public List<FloatKey> fKeys = new List<FloatKey>();

        [MinVersion(1), MaxVersion(1)]
        public float unknownFloat;



        public RndParticleSysAnim Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            if (revision > 2)
                base.Read(reader, false, parent, entry);

            anim = anim.Read(reader, parent, entry);

            particleSys = Symbol.Read(reader);

            startColorKeysCount = reader.ReadUInt32();
            for (int i = 0; i < startColorKeysCount; i++)
            {
                ColorKey colorKey = new();
                colorKey.Read(reader);
                startColorKeys.Add(colorKey);
            }

            endColorKeysCount = reader.ReadUInt32();
            for (int i = 0; i < endColorKeysCount; i++)
            {
                ColorKey colorKey = new();
                colorKey.Read(reader);
                endColorKeys.Add(colorKey);
            }

            if (revision < 2)
            {
                fKeysCount = reader.ReadUInt32();
                for (int i = 0; i < fKeysCount; i++)
                {
                    FloatKey floatKey = new();
                    floatKey.Read(reader);
                    fKeys.Add(floatKey);
                }

                if (revision == 1)
                    unknownFloat = reader.ReadFloat();

                keysOwner = Symbol.Read(reader);
            }
            else
            {
                emitRateKeysCount = reader.ReadUInt32();
                for (int i = 0; i < emitRateKeysCount; i++)
                {
                    Vec2Key vec2Key = new();
                    vec2Key.Read(reader);
                    emitRateKeys.Add(vec2Key);
                }
                keysOwner = Symbol.Read(reader);
            }

            if (revision > 1)
            {
                speedKeysCount = reader.ReadUInt32();
                for (int i = 0; i < speedKeysCount; i++)
                {
                    Vec2Key vec2Key = new();
                    vec2Key.Read(reader);
                    speedKeys.Add(vec2Key);
                }
                lifeKeysCount = reader.ReadUInt32();
                for (int i = 0; i < lifeKeysCount; i++)
                {
                    Vec2Key vec2Key = new();
                    vec2Key.Read(reader);
                    lifeKeys.Add(vec2Key);
                }
                startSizeKeysCount = reader.ReadUInt32();
                for (int i = 0; i < startSizeKeysCount; i++)
                {
                    Vec2Key vec2Key = new();
                    vec2Key.Read(reader);
                    startSizeKeys.Add(vec2Key);
                }
            }

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw new Exception("Got to end of standalone asset but didn't find the expected end bytes, read likely did not succeed");

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            if (revision > 2)
                base.Write(writer, false, parent, entry);

            anim.Write(writer);

            Symbol.Write(writer, particleSys);

            writer.WriteUInt32((uint)startColorKeys.Count);
            foreach (ColorKey colorKey in startColorKeys)
            {
                colorKey.Write(writer);
            }

            writer.WriteUInt32((uint)endColorKeys.Count);
            foreach (ColorKey colorKey in endColorKeys)
            {
                colorKey.Write(writer);
            }

            if (revision < 2)
            {
                writer.WriteUInt32((uint)fKeys.Count);
                foreach (FloatKey floatKey in fKeys)
                {
                    floatKey.Write(writer);
                }
                if (revision == 1)
                    writer.WriteFloat(unknownFloat);
                Symbol.Write(writer, keysOwner);
            }
            else
            {
                writer.WriteUInt32((uint)emitRateKeys.Count);
                foreach (Vec2Key vec2Key in emitRateKeys)
                {
                    vec2Key.Write(writer);
                }
                Symbol.Write(writer, keysOwner);
            }

            if (revision > 1)
            {
                writer.WriteUInt32((uint)speedKeys.Count);
                foreach (Vec2Key vec2Key in speedKeys)
                {
                    vec2Key.Write(writer);
                }
                writer.WriteUInt32((uint)lifeKeys.Count);
                foreach (Vec2Key vec2Key in lifeKeys)
                {
                    vec2Key.Write(writer);
                }
                writer.WriteUInt32((uint)startSizeKeys.Count);
                foreach (Vec2Key vec2Key in startSizeKeys)
                {
                    vec2Key.Write(writer);
                }
            }

            if (standalone)
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
        }

    }
}
