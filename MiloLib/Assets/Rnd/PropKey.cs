using MiloLib.Classes;
using MiloLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiloLib.Assets.Rnd
{
    public class PropKey
    {
        public class ColorKey
        {
            public HmxColor4 color = new();
            public float pos;

            public ColorKey Read(EndianReader reader)
            {
                color = color.Read(reader);
                pos = reader.ReadFloat();
                return this;
            }

            public void Write(EndianWriter writer)
            {
                color.Write(writer);
                writer.WriteFloat(pos);
            }

            public override string ToString()
            {
                return $"Color: {color}, Position: {pos}";
            }
        }

        public class FloatKey
        {
            public float value;
            public float pos;

            public FloatKey Read(EndianReader reader)
            {
                value = reader.ReadFloat();
                pos = reader.ReadFloat();
                return this;
            }

            public void Write(EndianWriter writer)
            {
                writer.WriteFloat(value);
                writer.WriteFloat(pos);
            }

            public override string ToString()
            {
                return $"Value: {value}, Position: {pos}";
            }
        }

        public class QuatKey
        {
            public Vector4 quat = new();
            public float pos;

            public QuatKey Read(EndianReader reader)
            {
                quat = quat.Read(reader);
                pos = reader.ReadFloat();
                return this;
            }

            public void Write(EndianWriter writer)
            {
                quat.Write(writer);
                writer.WriteFloat(pos);
            }

            public override string ToString()
            {
                return $"Quaternion: {quat}, Position: {pos}";
            }
        }

        public class Vec2Key
        {
            public Vector2 vec = new();
            public float pos;

            public Vec2Key Read(EndianReader reader)
            {
                vec = vec.Read(reader);
                pos = reader.ReadFloat();
                return this;
            }

            public void Write(EndianWriter writer)
            {
                vec.Write(writer);
                writer.WriteFloat(pos);
            }

            public override string ToString()
            {
                return $"Vector: {vec}, Position: {pos}";
            }
        }

        public class Vec3Key
        {
            public Vector3 vec = new();
            public float pos;

            public Vec3Key Read(EndianReader reader)
            {
                vec = vec.Read(reader);
                pos = reader.ReadFloat();
                return this;
            }

            public void Write(EndianWriter writer)
            {
                vec.Write(writer);
                writer.WriteFloat(pos);
            }

            public override string ToString()
            {
                return $"Vector: {vec}, Position: {pos}";
            }
        }

        public class SymbolKey
        {
            public Symbol value = new(0, "");
            public float pos;

            public SymbolKey Read(EndianReader reader)
            {
                value = Symbol.Read(reader);
                pos = reader.ReadFloat();
                return this;
            }

            public void Write(EndianWriter writer)
            {
                Symbol.Write(writer, value);
                writer.WriteFloat(pos);
            }

            public override string ToString()
            {
                return $"Symbol: {value}, Position: {pos}";
            }
        }
    }
}
