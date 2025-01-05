using MiloLib.Assets.Rnd;
using MiloLib.Utils;
using MiloLib.Classes;

namespace MiloLib.Assets.UI
{
    [Name("PanelDir"), Description("Top-level UI Object, contains UI components and an optional camera")]
    public class PanelDir : RndDir
    {
        private Dictionary<Game.MiloGame, uint> gameRevisions = new Dictionary<Game.MiloGame, uint>
        {
            // no dirs before this
            { Game.MiloGame.GuitarHero2_PS2, 2 },
            { Game.MiloGame.GuitarHero2_360, 2 },
            { Game.MiloGame.Phase, 2 },
            { Game.MiloGame.RockBand, 2 },
            { Game.MiloGame.RockBand2, 7 },
            { Game.MiloGame.LegoRockBand, 7 },
            { Game.MiloGame.TheBeatlesRockBand, 7 },
            { Game.MiloGame.GreenDayRockBand, 7 },
            { Game.MiloGame.RockBand3, 8 },
            { Game.MiloGame.DanceCentral, 8 },
            { Game.MiloGame.DanceCentral2, 8 },
            { Game.MiloGame.RockBandBlitz, 8 },
            { Game.MiloGame.DanceCentral3, 8 }
        };
        private ushort altRevision;
        private ushort revision;
        [Name("Camera"), Description("Camera to use in game, else standard UI cam")]
        public Symbol cam = new(0, "");

        [Name("Use Specified Camera"), Description("Forces the usage of the 'cam' property to render in milo. This is a milo only feature.")]
        public bool useSpecifiedCam;

        [Name("Back View Only Panels"), Description("Additional panels to display behind this panel.")]
        public List<Symbol> backPanels = new List<Symbol>();
        public List<Symbol> backFilenames = new List<Symbol>();
        [Name("Front View Only Panels"), Description("Additional panels to display in front of this panel.")]
        public List<Symbol> frontPanels = new List<Symbol>();
        public List<Symbol> frontFilenames = new List<Symbol>();
        [Name("Post Processes Before Draw"), Description("Trigger postprocs before drawing this panel. If checked, this panel will not be affected by the postprocs.")]

        public bool postProcsBeforeDraw;
        [Name("Show View Only Panels"), Description("Whether or no this panel displays its view only panels")]
        public bool showViewOnlyPanels;

        public bool canEndWorld;

        public Symbol testEvent = new(0, "");

        public Symbol unknownSymbol = new(0, "");

        public PanelDir(ushort revision, ushort altRevision = 0) : base(revision, altRevision)
        {
            revision = revision;
            altRevision = altRevision;
            return;
        }

        public PanelDir Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            base.Read(reader, false, parent, entry);

            // if this is not an entry inside another directory (as in, not an inlined subdir), read the camera
            if (!entry.isEntryInRootDir)
            {
                if (revision != 0)
                    cam = Symbol.Read(reader);
            }

            if (revision <= 1)
            {
                return this;
            }

            if (revision == 2)
            {
                testEvent = Symbol.Read(reader);
                return this;
            }
            else if (revision <= 7)
            {
                canEndWorld = reader.ReadBoolean();
            }
            else
            {
                useSpecifiedCam = reader.ReadBoolean();
            }

            if (revision > 4)
            {
                int frontPanelCount = reader.ReadInt32();
                frontPanels = new List<Symbol>();
                for (int i = 0; i < frontPanelCount; i++)
                {
                    frontPanels.Add(Symbol.Read(reader));
                }

                int backPanelCount = reader.ReadInt32();
                backPanels = new List<Symbol>();
                for (int i = 0; i < backPanelCount; i++)
                {
                    backPanels.Add(Symbol.Read(reader));
                }
            }

            if (revision >= 8)
                postProcsBeforeDraw = reader.ReadBoolean();

            if (revision > 5)
                showViewOnlyPanels = reader.ReadBoolean();

            if (standalone)
            {
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw new Exception("Got to end of standalone asset but didn't find the expected end bytes, read likely did not succeed");
            }

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            base.Write(writer, false, parent, entry);

            if (!entry.isEntryInRootDir)
            {
                if (revision != 0)
                    Symbol.Write(writer, cam);
                if (revision == 2)
                    Symbol.Write(writer, unknownSymbol);
            }

            if (revision <= 1)
            {
                if (standalone)
                {
                    writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
                }
                return;
            }

            if (revision == 2)
            {
                Symbol.Write(writer, testEvent);
                if (standalone)
                {
                    writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
                }
                return;
            }
            else if (revision <= 7)
            {
                writer.WriteBoolean(canEndWorld);
            }
            else
            {
                writer.WriteBoolean(useSpecifiedCam);
            }

            if (revision > 4)
            {
                writer.WriteInt32(frontPanels.Count);
                foreach (var panel in frontPanels)
                {
                    Symbol.Write(writer, panel);
                }

                writer.WriteInt32(backPanels.Count);
                foreach (var panel in backPanels)
                {
                    Symbol.Write(writer, panel);
                }
            }

            if (revision >= 8)
                writer.WriteBoolean(postProcsBeforeDraw);

            if (revision > 5)
                writer.WriteBoolean(showViewOnlyPanels);

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
