using MiloLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiloLib.Classes
{
    /// <summary>
    /// Represents a color with red, green, blue, and alpha components.
    /// </summary>
    public class HmxColor
    {
        public float r; public float g; public float b; public float a;

        public HmxColor()
        {
            this.r = 1.0f;
            this.g = 1.0f;
            this.b = 1.0f;
            this.a = 1.0f;
        }

        public HmxColor(float r, float g, float b, float a)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }

        public HmxColor Read(EndianReader reader)
        {
            r = reader.ReadFloat();
            g = reader.ReadFloat();
            b = reader.ReadFloat();
            a = reader.ReadFloat();
            return this;
        }

        public void Write(EndianWriter writer)
        {
            writer.WriteFloat(r);
            writer.WriteFloat(g);
            writer.WriteFloat(b);
            writer.WriteFloat(a);
        }

        public override string ToString()
        {
            return $"R: {r}, G: {g}, B: {b}, A: {a}";
        }
    }
}
