using MiloLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiloLib.Classes
{
    /// <summary>
    /// Represents a 3x4 matrix.
    /// </summary>
    public class Matrix
    {
        public float m11;
        public float m12;
        public float m13;
        public float m21;
        public float m22;
        public float m23;
        public float m31;
        public float m32;
        public float m33;
        public float m41;
        public float m42;
        public float m43;

        /// <summary>
        /// Reads a matrix from the given reader.
        /// </summary>
        public Matrix Read(EndianReader reader)
        {
            m11 = reader.ReadFloat();
            m12 = reader.ReadFloat();
            m13 = reader.ReadFloat();
            m21 = reader.ReadFloat();
            m22 = reader.ReadFloat();
            m23 = reader.ReadFloat();
            m31 = reader.ReadFloat();
            m32 = reader.ReadFloat();
            m33 = reader.ReadFloat();
            m41 = reader.ReadFloat();
            m42 = reader.ReadFloat();
            m43 = reader.ReadFloat();
            return this;
        }

        /// <summary>
        /// Writes the matrix to the given writer.
        /// </summary>
        /// <param name="writer">The writer to write with.</param>
        public void Write(EndianWriter writer)
        {
            writer.WriteFloat(m11);
            writer.WriteFloat(m12);
            writer.WriteFloat(m13);
            writer.WriteFloat(m21);
            writer.WriteFloat(m22);
            writer.WriteFloat(m23);
            writer.WriteFloat(m31);
            writer.WriteFloat(m32);
            writer.WriteFloat(m33);
            writer.WriteFloat(m41);
            writer.WriteFloat(m42);
            writer.WriteFloat(m43);
        }

        override public string ToString()
        {
            return $"[{m11}, {m12}, {m13}],[{m21}, {m22}, {m23}],[{m31}, {m32}, {m33}],[{m41}, {m42}, {m43}]";
        }
    }
}
