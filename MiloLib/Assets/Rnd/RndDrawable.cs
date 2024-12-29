using MiloLib.Classes;
using MiloLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiloLib.Assets.Rnd
{
    [Name("Drawable"), Description("Base class for drawable objects. Draw objects either render polys or determine rendering state.")]
    public class RndDrawable : Object
    {
        public enum OverrideIncludeInDepthOnlyPass
        {
            kOverrideIncludeInDepthOnlyPass_None,
            kOverrideIncludeInDepthOnlyPass_Include,
            kOverrideIncludeInDepthOnlyPass_DontInclude
        }

        public ushort altRevision;
        public ushort revision;

        [Name("Showing"), Description("Whether the object and its Draw children are drawn or collided with.")]
        public bool showing;

        [Name("Sphere"), Description("Represents a bounding sphere around this object and its drawn children, which is used for culling of draw and collision commands. X, Y, Z are the sphere center in local coordinates, R is the sphere radius in world coordinates. Culling is not performed when the radius is zero. The world transform of the object must be baked into the radius."), MinVersion(1)]
        public Sphere sphere = new();

        [Name("Draw Order"), Description("Draw order within proxies, lower numbers are drawn first, so assign numbers from the outside-in (unless translucent), to minimize overdraw.  In groups, draw_order will be ignored unless you explicitly click the sort button."), MinVersion(3)]
        public float drawOrder;

        [Name("Override Include In Depth Only Pass"), MinVersion(4)]
        public OverrideIncludeInDepthOnlyPass overrideIncludeInDepthOnlyPass;

        private uint drawableCount;
        [Name("Drawables"), MaxVersion(1)]
        public List<Symbol> drawables = new();
        public List<string> drawablesNullTerminated = new();

        public RndDrawable Read(EndianReader reader, bool standalone, bool skipMetadata = false)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            if (revision != 1 && !skipMetadata)
            {
                objFields = objFields.Read(reader);
            }

            showing = reader.ReadBoolean();

            if (revision < 2)
            {
                drawableCount = reader.ReadUInt32();
                if (drawableCount > 0)
                {
                    /*
                    if (revision <= 6)
                    {
                        for (int i = 0; i < drawableCount; i++)
                        {
                            drawablesNullTerminated.Add(reader.ReadUTF8());
                        }
                    }
                    else
                    {
                    */
                    for (int i = 0; i < drawableCount; i++)
                    {
                        drawables.Add(Symbol.Read(reader));
                    }
                    //}
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

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw new Exception("Got to end of standalone asset but didn't find the expected end bytes, read likely did not succeed");


            return this;
        }

        public void Write(EndianWriter writer, bool standalone, bool skipMetadata = false)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            if (revision != 1 && !skipMetadata)
            {
                objFields.Write(writer);
            }

            writer.WriteBoolean(showing);

            if (revision < 2)
            {
                writer.WriteUInt32((uint)drawables.Count);
                if (drawables.Count > 0)
                {
                    /*
                    if (revision <= 6)
                    {
                        foreach (var drawable in drawablesNullTerminated)
                        {
                            writer.WriteUTF8(drawable);
                        }
                    }
                    else
                    {
                    */
                    foreach (var drawable in drawables)
                    {
                        Symbol.Write(writer, drawable);
                    }
                    //}
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

            if (standalone)
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
        }
    }
}
