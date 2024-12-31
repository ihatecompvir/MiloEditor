using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets.Rnd
{
    [Name("Animatable"), Description("Base class for animatable objects. Anim objects change their state or other objects.")]
    public class RndAnimatable
    {
        public class AnimEntry
        {
            public Symbol name = new(0, "");
            public float frame1;
            public float frame2;

            public AnimEntry Read(EndianReader reader)
            {
                name = Symbol.Read(reader);
                frame1 = reader.ReadFloat();
                frame2 = reader.ReadFloat();
                return this;
            }

            public void Write(EndianWriter writer)
            {
                Symbol.Write(writer, name);
                writer.WriteFloat(frame1);
                writer.WriteFloat(frame2);
            }
        }
        public enum Rate
        {
            k30_fps,
            k480_fpb,
            k30_fps_ui,
            k1_fpb,
            k30_fps_tutorial
        }

        public ushort altRevision;
        public ushort revision;

        [Name("Frame"), Description("Frame of animation"), MinVersion(2)]
        public float frame;

        [Name("Rate"), Description("Rate to animate")]
        public Rate rate;

        private uint animEntryCount;
        public List<AnimEntry> animEntries = new();

        private uint animCount;
        public List<Symbol> anims = new();

        public RndAnimatable Read(EndianReader reader, DirectoryMeta parent)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            if (revision > 1)
            {
                frame = reader.ReadFloat();
            }

            if (revision < 4)
            {
                if (revision > 2)
                {
                    byte uc = reader.ReadByte();
                    rate = uc == 0 ? Rate.k30_fps : Rate.k480_fpb;
                }
            }
            else
            {
                rate = (Rate)reader.ReadUInt32();
                return this;
            }

            if (revision < 1)
            {
                animEntryCount = reader.ReadUInt32();
                for (int i = 0; i < animEntryCount; i++)
                {
                    AnimEntry entry = new();
                    entry.Read(reader);
                    animEntries.Add(entry);
                }

                animCount = reader.ReadUInt32();
                for (int i = 0; i < animCount; i++)
                {
                    anims.Add(Symbol.Read(reader));
                }
            }

            return this;
        }

        public void Write(EndianWriter writer)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));
            if (revision > 1)
                writer.WriteFloat(frame);

            if (revision < 4)
            {
                if (revision > 2)
                {
                    writer.WriteByte((byte)(rate == Rate.k30_fps ? 0 : 1));
                }
            }
            else
            {
                writer.WriteUInt32((uint)rate);
                return;
            }

            writer.WriteUInt32(animEntryCount);
            foreach (var entry in animEntries)
            {
                entry.Write(writer);
            }

            writer.WriteUInt32(animCount);
            foreach (var anim in anims)
            {
                Symbol.Write(writer, anim);
            }
        }
    }
}
