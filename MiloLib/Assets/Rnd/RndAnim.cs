using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets.Rnd
{
    [Name("Anim"), Description("Base class for animatable objects. Anim objects change their state or other objects.")]
    public class RndAnim
    {
        public enum Rate
        {
            k30_fps,
            k480_fpb,
            k30_fps_ui,
            k1_fpb,
            k30_fps_tutorial
        }

        public uint revision;

        [Name("Frame"), Description("Frame of animation")]
        public float frame;

        [Name("Rate"), Description("Rate to animate")]
        public Rate rate;

        public RndAnim Read(EndianReader reader)
        {
            revision = reader.ReadUInt32();

            if (revision != 4)
            {
                throw new UnsupportedAssetRevisionException("RndAnim", revision);
            }

            frame = reader.ReadFloat();
            rate = (Rate)reader.ReadUInt32();
            return this;
        }

        public void Write(EndianWriter writer)
        {
            writer.WriteUInt32(revision);
            writer.WriteFloat(frame);
            writer.WriteUInt32((uint)rate);
        }
    }
}
