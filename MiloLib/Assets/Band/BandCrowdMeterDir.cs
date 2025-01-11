using MiloLib.Assets.Rnd;
using MiloLib.Classes;
using MiloLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiloLib.Assets.Band
{
    [Name("BandCrowdMeterDir"), Description("Crowd meter hud element, has a needle")]
    public class BandCrowdMeterDir : RndDir
    {
        private Dictionary<Game.MiloGame, uint> gameRevisions = new Dictionary<Game.MiloGame, uint>
        {
            // no dirs before this
            { Game.MiloGame.GuitarHero2_PS2, 3 },
            { Game.MiloGame.GuitarHero2_360, 3 },
            //{ Game.MiloGame.RockBand, 19 },
            //{ Game.MiloGame.RockBand2, 20 },
            //{ Game.MiloGame.LegoRockBand, 20 },
            //{ Game.MiloGame.TheBeatlesRockBand, 22 },
            //{ Game.MiloGame.GreenDayRockBand, 22 },
            { Game.MiloGame.RockBand3, 3 },
        };
        private ushort altRevision;
        private ushort revision;

        private uint colorCount;

        [Name("Colors"), Description("The colors that will be shown as the meter decreases."), MinVersion(2)]
        public List<HmxColor4> colors = new();

        [Name("Peak Value"), Description("Peak state value"), MinVersion(1)]
        public float peakValue;

        private float groupCount;

        [MaxVersion(2)]
        private List<Symbol> groups = new();

        public uint unkInt;

        [Name("Needle Anim"), Description("anim to drive the needle")]
        public Symbol needleAnim = new(0, "");
        [Name("Warning Anim"), Description("animation that is played when below the warning level")]
        public Symbol warningAnim = new(0, "");
        [Name("Red Anim"), Description("animation that is played when in the red state")]
        public Symbol redAnim = new(0, "");
        [Name("Yellow Anim"), Description("animation that is played when in the yellow state")]
        public Symbol yellowAnim = new(0, "");
        [Name("Green Anim"), Description("animation that is played when in the green state")]
        public Symbol greenAnim = new(0, "");


        public BandCrowdMeterDir(ushort revision, ushort altRevision = 0) : base(revision, altRevision)
        {
            revision = revision;
            altRevision = altRevision;
            return;
        }

        public BandCrowdMeterDir Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            // despite being the same revision this is a way different asset in GH2, so try to detect if its being loaded out of a GH2-versioned scene, so check if the version is 25 or earlier
            if (parent.revision <= 25)
            {
                unkInt = reader.ReadUInt32();

                warningAnim = Symbol.Read(reader);
                redAnim = Symbol.Read(reader);
                yellowAnim = Symbol.Read(reader);
                greenAnim = Symbol.Read(reader);
                needleAnim = Symbol.Read(reader);

            }
            else
            {
                // fields only read when dir is not proxied
                if (!entry.isProxy)
                {
                    if (revision < 3)
                    {
                        groupCount = reader.ReadUInt32();
                        for (int i = 0; i < groupCount; i++)
                        {
                            groups.Add(Symbol.Read(reader));
                        }
                    }

                    if (revision >= 2)
                    {
                        colorCount = reader.ReadUInt32();
                        for (int i = 0; i < colorCount; i++)
                        {
                            colors.Add(new HmxColor4().Read(reader));
                        }
                    }
                }

                if (revision >= 1)
                    peakValue = reader.ReadFloat();
            }

            base.Read(reader, false, parent, entry);

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw new Exception("Got to end of standalone asset but didn't find the expected end bytes, read likely did not succeed");

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            if (parent.revision <= 25)
            {
                writer.WriteUInt32(unkInt);
                Symbol.Write(writer, warningAnim);
                Symbol.Write(writer, redAnim);
                Symbol.Write(writer, yellowAnim);
                Symbol.Write(writer, greenAnim);
                Symbol.Write(writer, needleAnim);

                base.Write(writer, false, parent, entry);

                if (standalone)
                {
                    writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
                }
                return;
            }
            else
            {
                if (!entry.isProxy)
                {
                    if (revision < 3)
                    {
                        writer.WriteUInt32((uint)groups.Count);
                        foreach (var group in groups)
                        {
                            Symbol.Write(writer, group);
                        }
                    }

                    if (revision >= 2)
                    {
                        writer.WriteUInt32((uint)colors.Count);
                        foreach (var color in colors)
                        {
                            color.Write(writer);
                        }
                    }

                }
                if (revision >= 1)
                    writer.WriteFloat(peakValue);

                base.Write(writer, false, parent, entry);

                if (standalone)
                {
                    writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
                }
            }
        }


        public override bool IsDirectory()
        {
            return true;
        }
    }
}
