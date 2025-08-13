using MiloLib.Utils;
using MiloLib.Classes;
using MiloLib.Assets.UI;
using MiloLib.Assets.Rnd;

namespace MiloLib.Assets.World
{
    [Name("WorldDir"), Description("A WorldDir contains world perObjs.")]
    public class WorldDir : PanelDir
    {
        private ushort altRevision;
        private ushort revision;
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

        [Name("Hide Overrides"), Description("Subdir perObjs to hide")]
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
        public Symbol hud = new(0, "");
        public Symbol camReference = new(0, "");

        public Matrix xfm = new();
        public RndTrans camTrans = new();

        public uint unkInt1;
        public float unkFloat;

        public Symbol unkSym = new(0, "");


        public WorldDir(ushort revision, ushort altRevision = 0) : base(revision, altRevision)
        {
            revision = revision;
            altRevision = altRevision;
            return;
        }


        public WorldDir Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)(combinedRevision >> 16 & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)(combinedRevision >> 16 & 0xFFFF));

            if (revision != 0 && revision < 5)
            {
                cam = Symbol.Read(reader);
            }

            if (revision >= 2 && revision <= 20)
            {
                unkInt1 = reader.ReadUInt32();
                unkFloat = reader.ReadFloat();
            }

            if (revision > 9)
            {
                fakeHUDFilename = Symbol.Read(reader);
            }

            if (revision < 9)
            {
                if (revision > 7)
                {
                    // OldLoadProxies
                }
                else if (revision > 2)
                {
                    // OldLoadChars
                }
            }

            base.Read(reader, false, parent, entry);

            if (revision == 5)
            {
                camReference = Symbol.Read(reader);
            }

            if (revision < 0x19)
            {
                if (revision > 0xA)
                {
                    xfm = xfm.Read(reader);
                }
                else if (revision > 6)
                    camTrans = camTrans.Read(reader, false, parent, entry);
            }



            if (revision > 0xB)
            {
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
                    bitmapOverride.original = Symbol.Read(reader);
                    bitmapOverride.replacement = Symbol.Read(reader);

                    bitmapOverrides.Add(bitmapOverride);
                }
            }

            if (revision > 0xD)
            {
                int matOverrideSize = reader.ReadInt32();
                for (int i = 0; i < matOverrideSize; i++)
                {
                    MatOverride matOverride = new MatOverride();
                    matOverride.mesh = Symbol.Read(reader);
                    matOverride.mat = Symbol.Read(reader);
                    matOverrides.Add(matOverride);
                }
            }

            if (revision > 0xE)
            {
                int presetOverrideSize = reader.ReadInt32();
                for (int i = 0; i < presetOverrideSize; i++)
                {
                    PresetOverride presetOverride = new PresetOverride();
                    presetOverride.preset = Symbol.Read(reader);
                    presetOverride.hue = Symbol.Read(reader);

                    presetOverrides.Add(presetOverride);
                }
            }

            if (revision > 0xF)
            {
                uint camShotOverrideCount = reader.ReadUInt32();
                for (int i = 0; i < camShotOverrideCount; i++)
                {
                    Symbol camShot = Symbol.Read(reader);
                    camShotOverrides.Add(camShot);
                }
            }

            if (revision > 0x10 && revision != 0x17)
            {
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
            }

            if (revision == 0x12 || revision == 0x13 || revision == 0x14 || revision == 0x15)
            {
                unkSym = Symbol.Read(reader);
            }

            if (revision > 0x12)
            {
                mTestPreset1 = Symbol.Read(reader);
                mTestPreset2 = Symbol.Read(reader);
                mTestAnimationTime = reader.ReadFloat();
            }

            if (revision > 0x13)
            {
                hud = Symbol.Read(reader);
            }


            if (standalone)
            {
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw new Exception("Got to end of standalone asset but didn't find the expected end bytes, read likely did not succeed");
            }

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)(altRevision << 16 | revision) : (uint)(revision << 16 | altRevision));

            if (revision != 0 && revision < 5)
            {
                Symbol.Write(writer, cam);
            }

            if (revision >= 2 && revision <= 20)
            {
                writer.WriteUInt32(unkInt1);
                writer.WriteFloat(unkFloat);
            }

            if (revision > 9)
            {
                Symbol.Write(writer, fakeHUDFilename);
            }

            if (revision < 9)
            {
                if (revision > 7)
                {
                    // OldLoadProxies
                }
                else if (revision > 2)
                {
                    // OldLoadChars
                }
            }

            base.Write(writer, false, parent, entry);

            if (revision == 5)
            {
                Symbol.Write(writer, camReference);
            }

            if (revision < 0x19)
            {
                if (revision > 0xA)
                {
                    xfm.Write(writer);
                }
                else if (revision > 6)
                    camTrans.Write(writer, false, true);
            }

            if (revision > 0xB)
            {
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
            }

            if (revision > 0xD)
            {
                writer.WriteInt32(matOverrides.Count);
                foreach (var matOverride in matOverrides)
                {
                    Symbol.Write(writer, matOverride.mesh);
                    Symbol.Write(writer, matOverride.mat);
                }
            }

            if (revision > 0xE)
            {
                writer.WriteInt32(presetOverrides.Count);
                foreach (var presetOverride in presetOverrides)
                {
                    Symbol.Write(writer, presetOverride.preset);
                    Symbol.Write(writer, presetOverride.hue);
                }
            }

            if (revision > 0xF)
            {
                writer.WriteUInt32((uint)camShotOverrides.Count);
                foreach (var camShot in camShotOverrides)
                {
                    Symbol.Write(writer, camShot);
                }
            }

            if (revision > 0x10 && revision != 0x17)
            {
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
            }

            if (revision == 0x12 || revision == 0x13 || revision == 0x14 || revision == 0x15)
            {
                Symbol.Write(writer, unkSym);
            }

            if (revision > 0x12)
            {
                Symbol.Write(writer, mTestPreset1);
                Symbol.Write(writer, mTestPreset2);
                writer.WriteFloat(mTestAnimationTime);
            }

            if (revision > 0x13)
            {
                Symbol.Write(writer, hud);
            }

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