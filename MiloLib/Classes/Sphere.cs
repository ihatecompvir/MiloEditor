using MiloLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiloLib.Classes
{
    /// <summary>
    /// Represents a sphere.
    /// </summary>
    public struct Sphere
    {
        public float x;
        public float y;
        public float z;
        public float radius;

        /// <summary>
        /// Reads a sphere from the given reader.
        /// </summary>
        public Sphere Read(EndianReader reader)
        {
            x = reader.ReadFloat();
            y = reader.ReadFloat();
            z = reader.ReadFloat();
            radius = reader.ReadFloat();
            return this;
        }

        public void Write(EndianWriter writer)
        {
            writer.WriteFloat(x);
            writer.WriteFloat(y);
            writer.WriteFloat(z);
            writer.WriteFloat(radius);
        }

        override public string ToString()
        {
            return "(x=" + x + ", y=" + y + ", z=" + z + "), r=" + radius;
        }
    }
}
