﻿using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets.Rnd
{
    [Name("MatAnim"), Description("MatAnim objects animate material properties.")]
    public class RndMatAnim : Object
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

        public class RndMatAnimStage
        {
			public List<Vec3Key> transKeys = new List<Vec3Key>();
			public List<Vec3Key> scaleKeys = new List<Vec3Key>();
			public List<Vec3Key> rotKeys = new List<Vec3Key>();
			public List<SymbolKey> texKeys = new List<SymbolKey>();

			public RndMatAnimStage Read(EndianReader reader)
            {
                uint transKeysCount = reader.ReadUInt32();
                for (uint i = 0; i < transKeysCount; i++)
                {
                    Vec3Key transKey = new();
                    transKey.Read(reader);
                    transKeys.Add(transKey);
                }
				uint scaleKeysCount = reader.ReadUInt32();
                for (uint i = 0; i < scaleKeysCount; i++)
                {
                    Vec3Key scaleKey = new();
                    scaleKey.Read(reader);
                    scaleKeys.Add(scaleKey);
                }
				uint rotKeysCount = reader.ReadUInt32();
                for (uint i = 0; i < rotKeysCount; i++)
                {
                    Vec3Key rotKey = new();
                    rotKey.Read(reader);
                    rotKeys.Add(rotKey);
                }
				uint texKeysCount = reader.ReadUInt32();
                for (uint i = 0; i < texKeysCount; i++)
                {
                    SymbolKey texKey = new();
                    texKey.Read(reader);
                    texKeys.Add(texKey);
                }
                return this;
            }

            public void Write(EndianWriter writer)
            {
				writer.WriteUInt32((uint)transKeys.Count);
                foreach (Vec3Key transKey in transKeys)
                {
                    transKey.Write(writer);
                }
				writer.WriteUInt32((uint)scaleKeys.Count);
                foreach (Vec3Key scaleKey in scaleKeys)
                {
                    scaleKey.Write(writer);
                }
				writer.WriteUInt32((uint)rotKeys.Count);
                foreach (Vec3Key rotKey in rotKeys)
                {
                    rotKey.Write(writer);
                }
				writer.WriteUInt32((uint)texKeys.Count);
                foreach (SymbolKey texKey in texKeys)
                {
                    texKey.Write(writer);
                }
            }
        }

        private ushort altRevision;
        private ushort revision;

        public RndAnimatable anim = new();

        [Name("Material"), Description("The material to animate.")]
        public Symbol material = new(0, "");

        [Name("Keys Owner"), Description("The owner of the keys.")]
        public Symbol keysOwner = new(0, "");

        private uint colorKeysCount;
        [Name("Color Keys"), Description("The color keys of the material animation.")]
        public List<ColorKey> colorKeys = new List<ColorKey>();

        private uint alphaKeysCount;
        [Name("Alpha Keys"), Description("The alpha keys of the material animation.")]
        public List<FloatKey> alphaKeys = new List<FloatKey>();

        private uint transKeysCount;
        [Name("Transform Keys"), Description("The transform keys of the material animation."), MinVersion(7)]
        public List<Vec3Key> transKeys = new List<Vec3Key>();

        private uint scaleKeysCount;
        [Name("Scale Keys"), Description("The scale keys of the material animation."), MinVersion(7)]
        public List<Vec3Key> scaleKeys = new List<Vec3Key>();

        private uint rotKeysCount;
        [Name("Rotation Keys"), Description("The rotation keys of the material animation."), MinVersion(7)]
        public List<Vec3Key> rotKeys = new List<Vec3Key>();

        private uint texKeysCount;
        [Name("Texture Keys"), Description("The texture keys of the material animation."), MinVersion(7)]
        public List<SymbolKey> texKeys = new List<SymbolKey>();

        private uint colorKeysCount1;
        [MaxVersion(4)]
        public List<ColorKey> colorKeys1 = new();
        private uint colorKeysCount2;
        [MaxVersion(2)]
        public List<ColorKey> colorKeys2 = new();
        private uint colorKeysCount3;
        [MaxVersion(3)]
        public List<ColorKey> colorKeys3 = new();
		[MaxVersion(3)]
		public List<RndMatAnimStage> stages = new();



        public RndMatAnim Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            if (revision > 5)
            {
                base.objFields.Read(reader, parent, entry);
            }

            anim = anim.Read(reader, parent, entry);

            material = Symbol.Read(reader);

			if (revision <= 3) 
			{
				uint stage_ct = reader.ReadUInt32();
				for (uint i = 0; i < stage_ct; i++) {
                    RndMatAnimStage stage = new();
                    stage.Read(reader);
                    stages.Add(stage);
				}
			}

            keysOwner = Symbol.Read(reader);

            if (revision > 1)
            {
                if (revision < 5)
                {
                    colorKeysCount1 = reader.ReadUInt32();
                    for (int i = 0; i < colorKeysCount1; i++)
                    {
                        ColorKey colorKey = new();
                        colorKey.Read(reader);
                        colorKeys1.Add(colorKey);
                    }
                }
                if (revision < 3)
                {
                    colorKeysCount2 = reader.ReadUInt32();
                    for (int i = 0; i < colorKeysCount2; i++)
                    {
                        ColorKey colorKey = new();
                        colorKey.Read(reader);
                        colorKeys2.Add(colorKey);
                    }
                }
                colorKeysCount = reader.ReadUInt32();
                for (int i = 0; i < colorKeysCount; i++)
                {
                    ColorKey colorKey = new();
                    colorKey.Read(reader);
                    colorKeys.Add(colorKey);
                }

                if (revision < 4)
                {
                    colorKeysCount3 = reader.ReadUInt32();
                    for (int i = 0; i < colorKeysCount3; i++)
                    {
                        ColorKey colorKey = new();
                        colorKey.Read(reader);
                        colorKeys3.Add(colorKey);
                    }
                }

                alphaKeysCount = reader.ReadUInt32();
                for (int i = 0; i < alphaKeysCount; i++)
                {
                    FloatKey alphaKey = new();
                    alphaKey.Read(reader);
                    alphaKeys.Add(alphaKey);
                }
            }

            if (revision > 6)
            {
                transKeysCount = reader.ReadUInt32();
                for (int i = 0; i < transKeysCount; i++)
                {
                    Vec3Key transKey = new();
                    transKey.Read(reader);
                    transKeys.Add(transKey);
                }

                scaleKeysCount = reader.ReadUInt32();
                for (int i = 0; i < scaleKeysCount; i++)
                {
                    Vec3Key scaleKey = new();
                    scaleKey.Read(reader);
                    scaleKeys.Add(scaleKey);
                }

                rotKeysCount = reader.ReadUInt32();
                for (int i = 0; i < rotKeysCount; i++)
                {
                    Vec3Key rotKey = new();
                    rotKey.Read(reader);
                    rotKeys.Add(rotKey);
                }

                texKeysCount = reader.ReadUInt32();
                for (int i = 0; i < texKeysCount; i++)
                {
                    SymbolKey texKey = new();
                    texKey.Read(reader);
                    texKeys.Add(texKey);
                }
            }

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw new Exception("Got to end of standalone asset but didn't find the expected end bytes, read likely did not succeed");

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            if (revision > 5)
            {
                base.objFields.Write(writer);
            }

            anim.Write(writer);

            Symbol.Write(writer, material);

            Symbol.Write(writer, keysOwner);

            if (revision > 1)
            {
                if (revision < 5)
                {
                    colorKeysCount1 = (uint)colorKeys1.Count;
                    writer.WriteUInt32(colorKeysCount1);
                    foreach (ColorKey colorKey in colorKeys1)
                    {
                        colorKey.Write(writer);
                    }
                }
                if (revision < 3)
                {
                    colorKeysCount2 = (uint)colorKeys2.Count;
                    writer.WriteUInt32(colorKeysCount2);
                    foreach (ColorKey colorKey in colorKeys2)
                    {
                        colorKey.Write(writer);
                    }
                }
                colorKeysCount = (uint)colorKeys.Count;
                writer.WriteUInt32(colorKeysCount);
                foreach (ColorKey colorKey in colorKeys)
                {
                    colorKey.Write(writer);
                }

                if (revision < 4)
                {
                    colorKeysCount3 = (uint)colorKeys3.Count;
                    writer.WriteUInt32(colorKeysCount3);
                    foreach (ColorKey colorKey in colorKeys3)
                    {
                        colorKey.Write(writer);
                    }
                }
                
                alphaKeysCount = (uint)alphaKeys.Count;
                writer.WriteUInt32(alphaKeysCount);
                foreach (FloatKey alphaKey in alphaKeys)
                {
                    alphaKey.Write(writer);
                }
            }

            if (revision > 6)
            {
                transKeysCount = (uint)transKeys.Count;
                writer.WriteUInt32(transKeysCount);
                foreach (Vec3Key transKey in transKeys)
                {
                    transKey.Write(writer);
                }

                scaleKeysCount = (uint)scaleKeys.Count;
                writer.WriteUInt32(scaleKeysCount);
                foreach (Vec3Key scaleKey in scaleKeys)
                {
                    scaleKey.Write(writer);
                }

                rotKeysCount = (uint)rotKeys.Count;
                writer.WriteUInt32(rotKeysCount);
                foreach (Vec3Key rotKey in rotKeys)
                {
                    rotKey.Write(writer);
                }

                texKeysCount = (uint)texKeys.Count;
                writer.WriteUInt32(texKeysCount);
                foreach (SymbolKey texKey in texKeys)
                {
                    texKey.Write(writer);
                }
            }

            if (standalone)
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
        }

    }
}
