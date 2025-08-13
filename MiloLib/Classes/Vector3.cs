using MiloLib.Utils;

namespace MiloLib.Classes
{
    /// <summary>
    /// Represents a 3D vector.
    /// </summary>
    public struct Vector3
    {
        public float x;
        public float y;
        public float z;

        public Vector3()
        {
            this.x = 0.0f;
            this.y = 0.0f;
            this.z = 0.0f;
        }

        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        /// <summary>
        /// Reads a Vector3 from a stream.
        /// </summary>
        /// <param name="reader">The reader to use.</param>
        /// <returns>The 3D vector read from the stream.</returns>
        public Vector3 Read(EndianReader reader)
        {
            x = reader.ReadFloat();
            y = reader.ReadFloat();
            z = reader.ReadFloat();
            return this;
        }

        /// <summary>
        /// Writes the Vector3 to a stream.
        /// </summary>
        /// <param name="writer">The writer to use.</param>
        public void Write(EndianWriter writer)
        {
            writer.WriteFloat(x);
            writer.WriteFloat(y);
            writer.WriteFloat(z);
        }

        public bool IsZero() {
            return x == 0.0f && y == 0.0f && z == 0.0f;
        }
        public override string ToString()
        {
            return $"({x}, {y}, {z})";
        }
    }
}
