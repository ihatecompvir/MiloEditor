using MiloLib.Utils;

namespace MiloLib.Classes
{
    /// <summary>
    /// Represents a 2D vector.
    /// </summary>
    public struct Vector2
    {
        public float x;
        public float y;

        public Vector2()
        {
            this.x = 0.0f;
            this.y = 0.0f;
        }

        public Vector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        /// <summary>
        /// Reads a Vector2 from a stream.
        /// </summary>
        /// <param name="reader">The reader to use.</param>
        /// <returns>The 2D vector read from the stream.</returns>
        public Vector2 Read(EndianReader reader)
        {
            x = reader.ReadFloat();
            y = reader.ReadFloat();
            return this;
        }

        /// <summary>
        /// Writes the Vector2 to a stream.
        /// </summary>
        /// <param name="writer">The writer to use.</param>
        public void Write(EndianWriter writer)
        {
            writer.WriteFloat(x);
            writer.WriteFloat(y);
        }

        public override string ToString()
        {
            return $"({x}, {y})";
        }
    }
}
