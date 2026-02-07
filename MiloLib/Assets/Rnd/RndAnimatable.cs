using MiloLib.Classes;
using MiloLib.Utils;
using System.Text;

namespace MiloLib.Assets.Rnd
{
    [Name("Animatable"), Description("Base class for animatable perObjs. Anim perObjs change their state or other perObjs.")]
    public class RndAnimatable
    {
        private Dictionary<Game.MiloGame, uint> gameRevisions = new Dictionary<Game.MiloGame, uint>
        {
            { Game.MiloGame.Amplitude, 0 },
            { Game.MiloGame.EyeToyAntiGrav, 0 },
            { Game.MiloGame.GuitarHero, 0 },
            { Game.MiloGame.GuitarHero2_PS2, 4 },
            { Game.MiloGame.GuitarHero2_360, 4 },
            { Game.MiloGame.Phase, 4 },
            { Game.MiloGame.RockBand, 4 },
            { Game.MiloGame.RockBand2, 4 },
            { Game.MiloGame.LegoRockBand, 4 },
            { Game.MiloGame.TheBeatlesRockBand, 4 },
            { Game.MiloGame.GreenDayRockBand, 4 },
            { Game.MiloGame.RockBand3, 4 },
            { Game.MiloGame.DanceCentral, 4 },
            { Game.MiloGame.DanceCentral2, 4 },
            { Game.MiloGame.RockBandBlitz, 4 },
            { Game.MiloGame.DanceCentral3, 4 }
        };
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

        private ushort altRevision;
        private ushort revision;

        [Name("Frame"), Description("Frame of animation"), MinVersion(2)]
        public float frame;

        [Name("Rate"), Description("Rate to animate")]
        public Rate rate;

        private uint animEntryCount;
        [Name("Anim Entries"), Description("List of animation entries"), MaxVersion(1)]
        public List<AnimEntry> animEntries = new();

        private uint animCount;
        [Name("Anims"), Description("List of animations"), MaxVersion(1)]
        public List<Symbol> anims = new();

        public override string ToString() {
            return $"RndAnimatable: revs({revision}, {altRevision})\tframe: {frame}, rate {rate}\n";
        }

        public RndAnimatable Read(EndianReader reader, DirectoryMeta parent, DirectoryMeta.Entry entry)
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
                    AnimEntry animEntry = new();
                    animEntry.Read(reader);
                    animEntries.Add(animEntry);
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

            if (revision < 1)
            {
                writer.WriteUInt32((uint)animEntries.Count);
                foreach (var entry in animEntries)
                {
                    entry.Write(writer);
                }

                writer.WriteUInt32((uint)anims.Count);
                foreach (var anim in anims)
                {
                    Symbol.Write(writer, anim);
                }
            }
        }

        public static RndAnimatable New(ushort revision, ushort altRevision)
        {
            RndAnimatable anim = new RndAnimatable();
            anim.revision = revision;
            anim.altRevision = altRevision;
            return anim;
        }
    }
}
