﻿using MiloLib.Assets.Rnd;
using MiloLib.Utils;
using MiloLib.Classes;

namespace MiloLib.Assets.UI
{
    [Name("PanelDir"), Description("Top-level UI Object, contains UI components and an optional camera")]
    public class PanelDir : RndDir
    {
        [Name("Camera"), Description("Camera to use in game, else standard UI cam")]
        public Symbol cam = new(0, ""); // 0x194

        [Name("Use Specified Camera"), Description("Forces the usage of the 'cam' property to render in milo. This is a milo only feature.")]
        public bool useSpecifiedCam; // 0x1b1

        [Name("Back View Only Panels"), Description("Additional panels to display behind this panel.")]
        public List<Symbol> backPanels = new List<Symbol>();
        public List<Symbol> backFilenames = new List<Symbol>();
        [Name("Front View Only Panels"), Description("Additional panels to display in front of this panel.")]
        public List<Symbol> frontPanels = new List<Symbol>();
        public List<Symbol> frontFilenames = new List<Symbol>();
        [Name("Post Processes Before Draw"), Description("Trigger postprocs before drawing this panel. If checked, this panel will not be affected by the postprocs.")]

        public bool postProcsBeforeDraw; // 0x1d4
        [Name("Show View Only Panels"), Description("Whether or no this panel displays its view only panels")]
        public bool showViewOnlyPanels; // 0x1d5

        public PanelDir Read(EndianReader reader, bool standalone)
        {
            revision = reader.ReadUInt32();

            base.Read(reader, false);

            cam = Symbol.Read(reader);

            useSpecifiedCam = reader.ReadBoolean();

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

            postProcsBeforeDraw = reader.ReadBoolean();
            showViewOnlyPanels = reader.ReadBoolean();

            if (standalone)
            {
                reader.BaseStream.Position += 4;
            }

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone)
        {
            writer.WriteUInt32(revision);

            base.Write(writer, false);

            Symbol.Write(writer, cam);

            writer.WriteBoolean(useSpecifiedCam);

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

            writer.WriteBoolean(postProcsBeforeDraw);
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