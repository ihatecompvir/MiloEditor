﻿using MiloLib.Utils;
using MiloLib.Classes;
using MiloLib.Assets;

namespace MiloLib.Assets.Char
{
    [Name("CharClipSet"), Description("A CharClip container.")]
    public class CharClipSet : ObjectDir
    {
        [Name("Character File Path"), Description("Preview base character to use- for example, char/male/male_guitar.milo for male guitarist")]
        public Symbol charFilePath = new(0, "");

        [Name("Preview Clip"), Description("Pick a clip to play")]
        public Symbol previewClip = new(0, "");

        [Name("Filter Flags"), Description("Flags for filtering preview clip")]
        public uint filterFlags;

        [Name("BPM"), Description("bpm for clip playing")]
        public uint bpm;

        [Name("Preview Walk"), Description("Allow preview character to move around and walk?")]
        public bool previewWalk;

        [Name("Still Clip"), Description("Set this to view drummer play anims")]
        public Symbol stillClip = new(0, "");

        public CharClipSet Read(EndianReader reader, bool standalone)
        {
            revision = reader.ReadUInt32();

            if (revision != 25)
            {
                throw new UnsupportedAssetRevisionException("CharClipSet", revision);
            }

            base.Read(reader, false);

            charFilePath = Symbol.Read(reader);
            previewClip = Symbol.Read(reader);
            filterFlags = reader.ReadUInt32();
            bpm = reader.ReadUInt32();
            previewWalk = reader.ReadBoolean();
            stillClip = Symbol.Read(reader);

            if (standalone)
            {
                reader.BaseStream.Position += 4;
            }

            return this;
        }
    }
}