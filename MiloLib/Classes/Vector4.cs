using MiloLib.Utils;

namespace MiloLib.Classes
{
    /// <summary>
    /// Represents a 4D vector.
    /// </summary>
    public class Vector4
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public Vector4()
        {
            this.x = 0.0f;
            this.y = 0.0f;
            this.z = 0.0f;
            this.w = 0.0f;
        }

        public Vector4(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = 0.0f;
        }

        /// <summary>
        /// Reads a Vector4 from a stream.
        /// </summary>
        /// <param name="reader">The reader to use.</param>
        /// <returns>The 4D vector read from the stream.</returns>
        public Vector4 Read(EndianReader reader)
        {
            x = reader.ReadFloat();
            y = reader.ReadFloat();
            z = reader.ReadFloat();
            w = reader.ReadFloat();
            return this;
        }

        /// <summary>
        /// Writes the Vector4 to a stream.
        /// </summary>
        /// <param name="writer">The writer to use.</param>
        public void Write(EndianWriter writer)
        {
            writer.WriteFloat(x);
            writer.WriteFloat(y);
            writer.WriteFloat(z);
            writer.WriteFloat(w);
        }

        public override string ToString()
        {
            return $"({x}, {y}, {z}, {w})";
        }
    }
}
