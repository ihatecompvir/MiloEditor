﻿using MiloLib.Utils;
using MiloLib.Classes;

namespace MiloLib.Assets.Rnd
{

    // this is a misleading description from Harmonix lol, I guess I could add this though so it is no longer false
    [Name("Tex"), Description("Tex objects represent bitmaps used by materials. These can be created automatically with 'import tex' on the file menu.")]
    public class RndTex : Object
    {
        public uint revision;

        [Name("Width"), Description("Width of the texture in pixels.")]
        public uint width;
        [Name("Height"), Description("Height of the texture in pixels.")]
        public uint height;

        [Name("BPP"), Description("Bits per pixel.")]
        public uint bpp;

        [Name("External Path"), Description("Path to the texture to be loaded externally.")]
        public Symbol externalPath = new(0, "");

        public float index;
        public uint index2;

        public bool unk;

        [Name("Use External Path"), Description("Whether or not to use the external path.")]
        public bool useExternalPath;

        [Name("Bitmap"), Description("The bitmap data.")]
        public RndBitmap bitmap = new();

        public RndTex Read(EndianReader reader, bool standalone)
        {
            revision = reader.ReadUInt32();

            if (revision != 11)
            {
                throw new UnsupportedAssetRevisionException("RndTex", revision);
            }


            base.Read(reader, false);

            width = reader.ReadUInt32();
            height = reader.ReadUInt32();

            bpp = reader.ReadUInt32();

            externalPath = Symbol.Read(reader);

            index = reader.ReadFloat();
            index2 = reader.ReadUInt32();

            unk = reader.ReadBoolean();
            useExternalPath = reader.ReadBoolean();

            bitmap = new RndBitmap().Read(reader, false);

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

            writer.WriteUInt32(width);
            writer.WriteUInt32(height);

            writer.WriteUInt32(bpp);

            Symbol.Write(writer, externalPath);

            writer.WriteFloat(index);
            writer.WriteUInt32(index2);

            writer.WriteByte(unk ? (byte)1 : (byte)0);
            writer.WriteByte(useExternalPath ? (byte)1 : (byte)0);

            bitmap.Write(writer, false);

            if (standalone)
            {
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
            }

        }

    }
}
