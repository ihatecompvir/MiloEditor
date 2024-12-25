using MiloLib.Utils;
using MiloLib.Classes;
using MiloLib.Assets.UI;

namespace MiloLib.Assets
{
    [Name("WorldDir"), Description("A WorldDir contains world objects.")]
    public class WorldDir : PanelDir
    {
        [Name("Fake HUD Filename"), Description("HUD Preview Dir")]
        public Symbol fakeHUDFilename = new(0, "");
        public class BitmapOverride
        {
            [Name("Original"), Description("Subdir texture to replace")]
            public Symbol original = new(0, "");
            [Name("Replacement"), Description("Curdir texture to replace with")]
            public Symbol replacement = new(0, "");
        }
        public class MatOverride
        {
            [Name("Mesh"), Description("Subdir mesh to modify")]
            public Symbol mesh = new(0, "");
            [Name("Mat"), Description("Curdir material to set")]
            public Symbol mat = new(0, "");
        }
        public class PresetOverride
        {
            [Name("Preset"), Description("Subdir preset to modify")]
            public Symbol preset = new(0, "");
            [Name("Hue"), Description("Hue texture to use")]
            public Symbol hue = new(0, "");
        }
        public List<PresetOverride> presetOverrides = new List<PresetOverride>();

        public List<BitmapOverride> bitmapOverrides = new List<BitmapOverride>();

        public List<MatOverride> matOverrides = new List<MatOverride>();

        [Name("Hide Overrides"), Description("Subdir objects to hide")]
        public List<Symbol> hideOverrides = new List<Symbol>();
        [Name("Cam Shot Overrides"), Description("Subdir camshots to inhibit")]
        public List<Symbol> camShotOverrides = new List<Symbol>();
        [Name("PS3 Per Pixel Shows"), Description("Things to show when ps3_per_pixel on CamShot")]
        public List<Symbol> PS3PerPixelShows = new List<Symbol>();
        [Name("PS3 Per Pixel Hides"), Description("Things to hide when ps3_per_pixel on CamShot")]
        public List<Symbol> PS3PerPixelHides = new List<Symbol>();

        [Name("Test Light Preset 1"), Description("The first light preset to start")]
        public Symbol mTestPreset1 = new(0, "");
        [Name("Test Light Preset 2"), Description("The second light preset to start")]
        public Symbol mTestPreset2 = new(0, "");
        [Name("Test Animation Time"), Description("animation time in beats")]
        public float mTestAnimationTime;
        [Name("HUD"), Description("hud to be drawn last")]
        public Symbol mHud = new(0, "");


        public WorldDir Read(EndianReader reader, bool standalone)
        {
            revision = reader.ReadUInt32();
            fakeHUDFilename = Symbol.Read(reader);
            base.Read(reader, false);


            uint hideOverrideCount = reader.ReadUInt32();
            for (int i = 0; i < hideOverrideCount; i++)
            {
                var hideOverride = Symbol.Read(reader);
                hideOverrides.Add(hideOverride);
            }

            int bitmapOverrideSize = reader.ReadInt32();
            for (int i = 0; i < bitmapOverrideSize; i++)
            {
                BitmapOverride bitmapOverride = new BitmapOverride();

                bitmapOverrides.Add(bitmapOverride);

            }

            int matOverrideSize = reader.ReadInt32();
            for (int i = 0; i < matOverrideSize; i++)
            {
                MatOverride matOverride = new MatOverride();
                matOverride.mesh = Symbol.Read(reader);
                matOverride.mat = Symbol.Read(reader);
                matOverrides.Add(matOverride);

            }

            int presetOverrideSize = reader.ReadInt32();
            for (int i = 0; i < presetOverrideSize; i++)
            {
                PresetOverride presetOverride = new PresetOverride();
                presetOverride.preset = Symbol.Read(reader);
                presetOverride.hue = Symbol.Read(reader);

                presetOverrides.Add(presetOverride);
            }

            uint camShotOverrideCount = reader.ReadUInt32();
            for (int i = 0; i < camShotOverrideCount; i++)
            {
                Symbol camShot = Symbol.Read(reader);
                camShotOverrides.Add(camShot);
            }

            uint ps3PerPixelHidesCount = reader.ReadUInt32();
            for (int i = 0; i < ps3PerPixelHidesCount; i++)
            {
                Symbol perPixelHide = Symbol.Read(reader);
                PS3PerPixelHides.Add(perPixelHide);
            }

            uint ps3PerPixelShowsCount = reader.ReadUInt32();
            for (int i = 0; i < ps3PerPixelShowsCount; i++)
            {
                Symbol perPixelShow = Symbol.Read(reader);
                PS3PerPixelShows.Add(perPixelShow);
            }

            mTestPreset1 = Symbol.Read(reader);
            mTestPreset2 = Symbol.Read(reader);
            mTestAnimationTime = reader.ReadFloat();

            mHud = Symbol.Read(reader);


            if (standalone)
            {
                reader.BaseStream.Position += 4;
            }

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone)
        {
            writer.WriteUInt32(revision);
            Symbol.Write(writer, fakeHUDFilename);

            base.Write(writer, false);

            writer.WriteUInt32((uint)hideOverrides.Count);
            foreach (var hideOverride in hideOverrides)
            {
                Symbol.Write(writer, hideOverride);
            }

            writer.WriteInt32(bitmapOverrides.Count);
            foreach (var bitmapOverride in bitmapOverrides)
            {
                Symbol.Write(writer, bitmapOverride.original);
                Symbol.Write(writer, bitmapOverride.replacement);
            }

            writer.WriteInt32(matOverrides.Count);
            foreach (var matOverride in matOverrides)
            {
                Symbol.Write(writer, matOverride.mesh);
                Symbol.Write(writer, matOverride.mat);
            }

            writer.WriteInt32(presetOverrides.Count);
            foreach (var presetOverride in presetOverrides)
            {
                Symbol.Write(writer, presetOverride.preset);
                Symbol.Write(writer, presetOverride.hue);
            }

            writer.WriteUInt32((uint)camShotOverrides.Count);
            foreach (var camShot in camShotOverrides)
            {
                Symbol.Write(writer, camShot);
            }

            writer.WriteUInt32((uint)PS3PerPixelHides.Count);
            foreach (var perPixelHide in PS3PerPixelHides)
            {
                Symbol.Write(writer, perPixelHide);
            }

            writer.WriteUInt32((uint)PS3PerPixelShows.Count);
            foreach (var perPixelShow in PS3PerPixelShows)
            {
                Symbol.Write(writer, perPixelShow);
            }

            Symbol.Write(writer, mTestPreset1);
            Symbol.Write(writer, mTestPreset2);
            writer.WriteFloat(mTestAnimationTime);
            Symbol.Write(writer, mHud);

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