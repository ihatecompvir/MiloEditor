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
        public enum OverrideIncludeInDepthOnlyPass
        {
            kOverrideIncludeInDepthOnlyPass_None,
            kOverrideIncludeInDepthOnlyPass_Include,
            kOverrideIncludeInDepthOnlyPass_DontInclude
        }

        public uint revision;

        public bool showing;

        public Sphere sphere = new();

        public float drawOrder;
        public OverrideIncludeInDepthOnlyPass overrideIncludeInDepthOnlyPass;
        private uint drawableCount;
        public List<Symbol> drawables = new();
        public List<string> drawablesNullTerminated = new();

        public RndDraw Read(EndianReader reader)
        {
            revision = reader.ReadUInt32();

            showing = reader.ReadBoolean();

            if (revision < 2)
            {
                drawableCount = reader.ReadUInt32();
                if (drawableCount > 0)
                {
                    if (revision <= 6)
                    {
                        for (int i = 0; i < drawableCount; i++)
                        {
                            drawablesNullTerminated.Add(reader.ReadUTF8());
                        }
                    }
                    else
                    {
                        for (int i = 0; i < drawableCount; i++)
                        {
                            drawables.Add(Symbol.Read(reader));
                        }
                    }
                }
            }

            if (revision > 0)
            {
                sphere = new Sphere().Read(reader);
            }
            if (revision > 2)
            {
                drawOrder = reader.ReadFloat();
            }

            if (revision >= 4)
            {
                overrideIncludeInDepthOnlyPass = (OverrideIncludeInDepthOnlyPass)reader.ReadUInt32();
            }

            return this;
        }

        public void Write(EndianWriter writer)
        {
            writer.WriteUInt32(revision);

            writer.WriteBoolean(showing);

            if (revision < 2)
            {
                writer.WriteUInt32((uint)drawables.Count);
                if (drawables.Count > 0)
                {
                    if (revision <= 6)
                    {
                        foreach (var drawable in drawablesNullTerminated)
                        {
                            writer.WriteUTF8(drawable);
                        }
                    }
                    else
                    {
                        foreach (var drawable in drawables)
                        {
                            Symbol.Write(writer, drawable);
                        }
                    }
                }
            }

            if (revision > 0)
            {
                sphere.Write(writer);
            }
            if (revision > 2)
            {
                writer.WriteFloat(drawOrder);
            }

            if (revision >= 4)
            {
                writer.WriteUInt32((uint)overrideIncludeInDepthOnlyPass);
            }
        }
    }
}
