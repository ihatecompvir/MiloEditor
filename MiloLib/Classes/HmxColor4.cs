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
    public class HmxColor4
    {
        public float r; public float g; public float b; public float a;

        public HmxColor4()
        {
            this.r = 1.0f;
            this.g = 1.0f;
            this.b = 1.0f;
            this.a = 1.0f;
        }

        public HmxColor4(float r, float g, float b, float a)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }

        public HmxColor4 Read(EndianReader reader)
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

    /// <summary>
    /// Represents a color with red, green, and blue components.
    /// </summary>
    public class HmxColor3
    {
        public float r; public float g; public float b;

        public HmxColor3()
        {
            this.r = 1.0f;
            this.g = 1.0f;
            this.b = 1.0f;
        }

        public HmxColor3(float r, float g, float b, float a)
        {
            this.r = r;
            this.g = g;
            this.b = b;
        }

        public HmxColor3 Read(EndianReader reader)
        {
            r = reader.ReadFloat();
            g = reader.ReadFloat();
            b = reader.ReadFloat();
            return this;
        }

        public void Write(EndianWriter writer)
        {
            writer.WriteFloat(r);
            writer.WriteFloat(g);
            writer.WriteFloat(b);
        }

        public override string ToString()
        {
            return $"R: {r}, G: {g}, B: {b}";
        }
    }
}
