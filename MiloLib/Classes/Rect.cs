using MiloLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiloLib.Classes
{
    /// <summary>
    /// Represents a rectangle.
    /// </summary>
    public class Rect
    {
        public float x;
        public float y;
        public float width;
        public float height;

        /// <summary>
        /// Reads a Rect from the given reader.
        /// </summary>
        public Rect Read(EndianReader reader)
        {
            x = reader.ReadFloat();
            y = reader.ReadFloat();
            width = reader.ReadFloat();
            height = reader.ReadFloat();
            return this;
        }

        /// <summary>
        /// Writes a Rect via the given reader.
        /// </summary>
        /// <param name="writer">The writer to write the Rect.</param>
        public void Write(EndianWriter writer)
        {
            writer.WriteFloat(x);
            writer.WriteFloat(y);
            writer.WriteFloat(width);
            writer.WriteFloat(height);
        }

        override public string ToString()
        {
            return "(x=" + x + ", y=" + y + ", width=" + width + ", height=" + height + ")";
        }
    }
}
