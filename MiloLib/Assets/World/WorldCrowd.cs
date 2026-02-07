using MiloLib.Assets.Rnd;
using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets.World
{
    [Name("WorldCrowd"), Description("A quickly-rendered bunch of instanced characters within an area")]
    public class WorldCrowd : RndDrawable
    {
        // TODO: this needs to be moved into RndMultiMesh when we have that, according to the decomp
        public class OldMultiMeshInstance
        {
            public Matrix oldXfm = new();
            public HmxColor4 oldColor = new HmxColor4();

            public OldMultiMeshInstance Read(EndianReader reader)
            {
                oldXfm = oldXfm.Read(reader);
                oldColor = oldColor.Read(reader);
                return this;
            }

            public void Write(EndianWriter writer)
            {
                oldXfm.Write(writer);
                oldColor.Write(writer);
            }
        }
        public class CharDef
        {

            [Name("Character"), Description("The character to use as the archetype")]
            public Symbol character = new(0, "");
            [Name("Height"), Description("The height at which to render the character")]
            public float height;
            [Name("Density"), Description("Density to place this character")]
            public float density;
            [Name("Radius"), Description("Collision radius of the character - characters won't be placed within this range")]
            public float radius;
            public bool useRandomColor;

            public CharDef Read(EndianReader reader, uint revision)
            {
                character = Symbol.Read(reader);
                height = reader.ReadFloat();
                density = reader.ReadFloat();
                if (revision > 1)
                    radius = reader.ReadFloat();
                if (revision > 8)
                    useRandomColor = reader.ReadBoolean();

                return this;
            }

            public void Write(EndianWriter writer, uint revision)
            {
                Symbol.Write(writer, character);
                writer.WriteFloat(height);
                writer.WriteFloat(density);
                if (revision > 1)
                    writer.WriteFloat(radius);
                if (revision > 8)
                    writer.WriteBoolean(useRandomColor);
            }

            public override string ToString()
            {
                return $"{character.value} - Height: {height}, Density: {density}, Radius: {radius}, RandomColor: {useRandomColor}";
            }
        }

        public class CharData
        {

            public CharDef def = new();

            public CharData Read(EndianReader reader, uint revision)
            {
                def.Read(reader, revision);
                return this;
            }

            public void Write(EndianWriter writer, uint revision)
            {
                def.Write(writer, revision);
            }

            public override string ToString()
            {
                return def.ToString();
            }
        }

        private ushort altRevision;
        private ushort revision;

        [Name("Placement Mesh"), Description("The placement mesh")]
        public Symbol placementMesh = new(0, "");

        public uint unkInt1;

        [Name("Num"), Description("Number of characters to place")]
        public uint num;

        public bool unkBool1;

        public uint modifyStamp;
        [Name("Force 3D Crowd"), Description("Makes crowd be 3D regardless of the CamShot")]
        public bool force3DCrowd;

        [Name("Show 3D Only"), Description("Shows only the 3D crowd, but ONLY in Milo so you can more easily distinguish them from the 2d crowd")]
        public bool show3DOnly;


        [Name("Environ"), Description("The environ to render the imposter billboards with")]
        public Symbol environ = new(0, "");
        [Name("Environ3D"), Description("The environ used when rendering the 3D crowd set by a cam shot")]
        public Symbol environ3D = new(0, "");
        [Name("Focus"), Description("Optional crowd facing focus when rotate is set to kCrowdRotateNone")]
        public Symbol focus = new(0, "");

        private uint charCount;
        public List<CharData> characters = new();

        public List<List<OldMultiMeshInstance>> oldMultiMeshInstances = new();

        private List<uint> transformCount = new();
        public List<List<Matrix>> transforms = new();

        public Object highlightable = new();


        public WorldCrowd Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)(combinedRevision >> 16 & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)(combinedRevision >> 16 & 0xFFFF));

            base.Read(reader, false, parent, entry);

            placementMesh = Symbol.Read(reader);

            if (revision < 3)
                unkInt1 = reader.ReadUInt32();

            num = reader.ReadUInt32();

            if (revision < 8)
                unkBool1 = reader.ReadBoolean();

            charCount = reader.ReadUInt32();
            if (charCount > 0)
            {
                for (int i = 0; i < charCount; i++)
                {
                    characters.Add(new CharData().Read(reader, revision));
                }
            }

            if (revision > 6)
                environ = Symbol.Read(reader);
            if (revision > 9)
                environ3D = Symbol.Read(reader);

            if (revision > 1)
            {
                if (revision < 0xE)
                {
                    for (int i = 0; i < charCount; i++)
                    {
                        uint oldmmCount = reader.ReadUInt32();
                        List<OldMultiMeshInstance> oldmm = new();
                        for (int j = 0; j < oldmmCount; j++)
                        {
                            oldmm.Add(new OldMultiMeshInstance().Read(reader));
                        }
                        oldMultiMeshInstances.Add(oldmm);
                    }
                }
                else
                {
                    // read the count and transforms for every character
                    for (int i = 0; i < charCount; i++)
                    {
                        transformCount.Add(reader.ReadUInt32());
                        if (transformCount[i] > 0)
                        {
                            List<Matrix> transformsList = new();
                            for (int j = 0; j < transformCount[i]; j++)
                            {
                                transformsList.Add(new Matrix().Read(reader));
                            }
                            transforms.Add(transformsList);
                        }
                    }
                }
            }

            if (revision > 4)
                modifyStamp = reader.ReadUInt32();
            if (revision > 0xC)
                force3DCrowd = reader.ReadBoolean();
            if (revision > 5)
                show3DOnly = reader.ReadBoolean();
            if (revision > 0xB)
                focus = Symbol.Read(reader);

            if (revision != 0)
                highlightable = highlightable.Read(reader, false, parent, entry);




            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw MiloLib.Exceptions.MiloAssetReadException.EndBytesNotFound(parent, entry, reader.BaseStream.Position);

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)(altRevision << 16 | revision) : (uint)(revision << 16 | altRevision));

            ((RndDrawable)this).Write(writer, false, parent, true);

            Symbol.Write(writer, placementMesh);

            if (revision < 3)
                writer.WriteUInt32(unkInt1);

            writer.WriteUInt32(num);

            if (revision < 8)
                writer.WriteBoolean(unkBool1);

            charCount = (uint)characters.Count;
            writer.WriteUInt32(charCount);
            foreach (var charData in characters)
            {
                charData.Write(writer, revision);
            }

            if (revision > 6)
                Symbol.Write(writer, environ);
            if (revision > 9)
                Symbol.Write(writer, environ3D);

            if (revision > 1)
            {
                if (revision < 0xE)
                {
                    for (int i = 0; i < charCount; i++)
                    {
                        if (i < oldMultiMeshInstances.Count)
                        {
                            writer.WriteUInt32((uint)oldMultiMeshInstances[i].Count);
                            foreach (var inst in oldMultiMeshInstances[i])
                            {
                                inst.Write(writer);
                            }
                        }
                        else
                        {
                            writer.WriteUInt32(0);
                        }
                    }
                }
                else
                {
                    while (transformCount.Count < characters.Count)
                    {
                        transformCount.Add(0);
                    }
                    while (transforms.Count < characters.Count)
                    {
                        transforms.Add(new List<Matrix>());
                    }
                    
                    for (int i = 0; i < charCount; i++)
                    {
                        if (i < transforms.Count)
                        {
                            transformCount[i] = (uint)transforms[i].Count;
                        }
                        else
                        {
                            transformCount[i] = 0;
                        }
                        
                        writer.WriteUInt32(transformCount[i]);
                        if (transformCount[i] > 0 && i < transforms.Count)
                        {
                            foreach (var transform in transforms[i])
                            {
                                transform.Write(writer);
                            }
                        }
                    }
                }
            }

            if (revision > 4)
                writer.WriteUInt32(modifyStamp);
            if (revision > 0xC)
                writer.WriteBoolean(force3DCrowd);
            if (revision > 5)
                writer.WriteBoolean(show3DOnly);
            if (revision > 0xB)
                Symbol.Write(writer, focus);

            if (revision != 0)
                highlightable.Write(writer, false, parent, entry);

            if (standalone)
                writer.WriteEndBytes();
        }

    }
}
