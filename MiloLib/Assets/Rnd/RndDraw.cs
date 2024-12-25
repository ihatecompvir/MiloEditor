using MiloLib.Classes;
using MiloLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiloLib.Assets.Rnd
{
    [Name("Draw"), Description("Base class for drawable objects. Draw objects either render polys or determine rendering state.")]
    public class RndDraw
    {
        public uint revision;

        public bool showing;

        public Sphere sphere = new();

        public float drawOrder;

        public RndDraw Read(EndianReader reader)
        {
            revision = reader.ReadUInt32();

            if (revision != 3)
            {
                throw new UnsupportedAssetRevisionException("Draw", revision);
            }

            showing = reader.ReadBoolean();
            sphere = new Sphere().Read(reader);
            drawOrder = reader.ReadFloat();
            return this;
        }

        public void Write(EndianWriter writer)
        {
            writer.WriteUInt32(revision);
            writer.WriteBoolean(showing);
            sphere.Write(writer);
            writer.WriteFloat(drawOrder);
        }
    }
}
