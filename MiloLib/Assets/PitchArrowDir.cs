using MiloLib.Assets.Rnd;
using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets
{
    [Name("PitchArrowDir"), Description("singer's vocal HUD pitch guide arrow")]
    public class PitchArrowDir : RndDir
    {

        private ushort altRevision;
        private ushort revision;

        public bool unk18c;
        [Name("Score"), Description("the score for the current frame, normalized between 0 and 1")]
        public float score;
        [Name("Harmony FX"), Description("extra bonus fx that play when nailing harmony with another part")]
        public float harmonyFX;
        [Name("Volume"), Description("arrow alpha driven by the mic volume in the current frame, normalized between 0 and 1")]
        public float volume;
        [Name("Tilt"), Description("the tilt of the arrow direction in degrees")]
        public float tilt;
        [Name("Color Fade"), Description("controls the saturation of the arrow color: 0 = max saturation, 1 = no saturation")]
        public float colorFade;
        public bool spotlight;
        public bool deploying;
        public bool pitched;
        public bool unk1ab;
        public Symbol testColor;
        public int arrowStyle;
        public Symbol scoreAnim;
        public Symbol harmonyFXAnim;
        public Symbol volumeAnim;
        public Symbol tiltAnim;
        public Symbol colorAnim;
        public Symbol colorFadeAnim;
        public Symbol splitAnim;
        public Symbol arrowStyleAnim;
        public Symbol setPitchedTrig;
        public Symbol setUnpitchedTrig;
        public Symbol spotlightStartTrig;
        public Symbol spotlightEndTrig;
        public Symbol deployStartTrig;
        public Symbol deployEndTrig;
        public Symbol ghostGrp;
        public Symbol ghostFadeAnim;
        public Symbol arrowFXGrp;
        public bool unk280;
        [Name("Spin Speed"), Description("controls the spin speed for helix particle effects.  Can be animated")]
        public float spinSpeed;
        [Name("Spin Anim"), Description("this anim will loop its loop range according to the value of spin_speed")]
        public Symbol spinAnim;
        [Name("Spin Rest Frame"), Description("frame where spin_anim should be when spin_speed is zero")]
        public float spinRestFrame;
        [Name("Spin Begin Frame"), Description("loop start frame for spin_anim")]
        public float spinBeginFrame;
        [Name("Spin End Frame"), Description("loop end frame for spin_anim")]
        public float spinEndFrame;
        public uint arrowFXGrpCount;
        public List<Symbol> arrowFXGrpList;
        public uint ghostGrpCount;
        public List<Symbol> ghostGrpList;


        public PitchArrowDir(ushort revision, ushort altRevision = 0) : base(revision, altRevision)
        {
            revision = revision;
            altRevision = altRevision;
            return;
        }

        public PitchArrowDir Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            pitched = reader.ReadBoolean();
            spotlight = reader.ReadBoolean();
            deploying = reader.ReadBoolean();
            score = reader.ReadFloat();
            harmonyFX = reader.ReadFloat();
            volume = reader.ReadFloat();
            tilt = reader.ReadFloat();
            if (revision < 2)
                testColor = Symbol.Read(reader);
            colorFade = reader.ReadFloat();
            if (revision >= 1 && !entry.isProxy)
            {
                spinSpeed = reader.ReadFloat();
                spinAnim = Symbol.Read(reader);
                spinBeginFrame = reader.ReadFloat();
                spinEndFrame = reader.ReadFloat();
                spinRestFrame = reader.ReadFloat();
            }
            if (revision >= 2)
            {
                arrowStyle = reader.ReadInt32();
                testColor = Symbol.Read(reader);
            }

            base.Read(reader, false, parent, entry);

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw new Exception("Got to end of standalone asset but didn't find the expected end bytes, read likely did not succeed");

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            writer.WriteBoolean(pitched);
            writer.WriteBoolean(spotlight);
            writer.WriteBoolean(deploying);
            writer.WriteFloat(score);
            writer.WriteFloat(harmonyFX);
            writer.WriteFloat(volume);
            writer.WriteFloat(tilt);
            if (revision < 2)
            {
                Symbol.Write(writer, testColor);
            }
            writer.WriteFloat(colorFade);
            if (revision >= 1 && !entry.isProxy)
            {
                writer.WriteFloat(spinSpeed);
                Symbol.Write(writer, spinAnim);
                writer.WriteFloat(spinBeginFrame);
                writer.WriteFloat(spinEndFrame);
                writer.WriteFloat(spinRestFrame);
            }
            if (revision >= 2)
            {
                writer.WriteInt32(arrowStyle);
                Symbol.Write(writer, testColor);
            }

            base.Write(writer, false, parent, entry);

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
