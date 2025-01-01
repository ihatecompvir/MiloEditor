using MiloLib.Utils;
using MiloLib.Classes;

namespace MiloLib.Assets.Band
{
    public enum CharInstrumentType
    {
        guitar,
        bass,
        drum,
        mic,
        keyboard,
        numInstruments
    }

    public enum PatchCategory
    {
        //  kNone,
        //  kTattoo,
        //  kLogo,
        //  kAccessory,
        //  kFacePaint
    }

    [Name("Outfit Piece"), Description("A piece of an outfit. Defines the color and the Milo scene for the outfit.")]
    public class OutfitPiece
    {
        [Name("Name"), Description("The Milo scene that the outfit piece uses.")]
        public Symbol name = new(0, "");
        [Name("Colors"), Description("The indices of the color palettes the outfit piece uses.")]
        public int[] colors = new int[3];

        public OutfitPiece Read(EndianReader reader)
        {
            name = Symbol.Read(reader);
            colors[0] = reader.ReadByte();
            colors[1] = reader.ReadByte();
            colors[2] = reader.ReadByte();

            return this;
        }

        public void Write(EndianWriter writer)
        {
            Symbol.Write(writer, name);
            writer.WriteByte((byte)colors[0]);
            writer.WriteByte((byte)colors[1]);
            writer.WriteByte((byte)colors[2]);
        }
    }

    [Name("Head"), Description("The configuration of the character's head.")]
    public class Head
    {
        [Name("Hide"), Description("Hide the head.")]
        public bool hide;
        [Name("Eye Color"), Description("eye color index")]
        public int eyeColor;
        [Name("Shape"), Description("shape of the head index")]
        public int shape;
        [Name("Chin"), Description("chin index 0-2")]
        public int chin;
        [Name("Chin Width"), Description("chin length 0-1")]
        public float chinWidth;
        [Name("Chin Height"), Description("chin height 0-1")]
        public float chinHeight;
        [Name("Jaw Width"), Description("jaw length 0-1")]
        public float jawWidth;
        [Name("Jaw Height"), Description("jaw height 0-1")]
        public float jawHeight;
        [Name("Nose"), Description("nose index")]
        public int nose;
        [Name("Nose Width"), Description("nose width 0-1")]
        public float noseWidth;
        [Name("Nose Height"), Description("nose height 0-1")]
        public float noseHeight;
        [Name("Eye"), Description("eye index")]
        public int eye;
        [Name("Eye Separation"), Description("eye separation 0-1")]
        public float eyeSeparation;
        [Name("Eye Height"), Description("eye height 0-1")]
        public float eyeHeight;
        [Name("Eye Rotation"), Description("eye rotation 0-1")]
        public float eyeRotation;
        [Name("Mouth"), Description("mouth index")]
        public int mouth;
        [Name("Mouth Width"), Description("mouth width 0-1")]
        public float mouthWidth;
        [Name("Mouth Height"), Description("mouth height 0-1")]
        public float mouthHeight;
        [Name("Brow Separation"), Description("eyebrow separation 0-1")]
        public float browSeparation;
        [Name("Brow Height"), Description("eyebrow height 0-1")]
        public float browHeight;

        public void Read(EndianReader reader)
        {
            hide = reader.ReadBoolean();
            eyeColor = reader.ReadInt32();
            shape = reader.ReadInt32();
            chin = reader.ReadInt32();
            chinWidth = reader.ReadFloat();
            chinHeight = reader.ReadFloat();
            jawWidth = reader.ReadFloat();
            jawHeight = reader.ReadFloat();
            nose = reader.ReadInt32();
            noseWidth = reader.ReadFloat();
            noseHeight = reader.ReadFloat();
            eye = reader.ReadInt32();
            eyeSeparation = reader.ReadFloat();
            eyeHeight = reader.ReadFloat();
            eyeRotation = reader.ReadFloat();
            mouth = reader.ReadInt32();
            mouthWidth = reader.ReadFloat();
            mouthHeight = reader.ReadFloat();
            browSeparation = reader.ReadFloat();
            browHeight = reader.ReadFloat();
        }

        public void Write(EndianWriter writer)
        {
            writer.WriteByte((byte)(hide ? 1 : 0));
            writer.WriteInt32(eyeColor);
            writer.WriteInt32(shape);
            writer.WriteInt32(chin);
            writer.WriteFloat(chinWidth);
            writer.WriteFloat(chinHeight);
            writer.WriteFloat(jawWidth);
            writer.WriteFloat(jawHeight);
            writer.WriteInt32(nose);
            writer.WriteFloat(noseWidth);
            writer.WriteFloat(noseHeight);
            writer.WriteInt32(eye);
            writer.WriteFloat(eyeSeparation);
            writer.WriteFloat(eyeHeight);
            writer.WriteFloat(eyeRotation);
            writer.WriteInt32(mouth);
            writer.WriteFloat(mouthWidth);
            writer.WriteFloat(mouthHeight);
            writer.WriteFloat(browSeparation);
            writer.WriteFloat(browHeight);
        }
    }

    [Name("Outfit"), Description("The clothes the character is wearing.")]
    public class Outfit
    {
        [Name("Eyebrows"), Description("The name of the eyebrows Milo scene that this character uses.")]
        public OutfitPiece eyebrows;
        [Name("Face Hair"), Description("The name of the face hair Milo scene that this character uses.")]
        public OutfitPiece faceHair;
        [Name("Hair"), Description("The name of the hair Milo scene that this character uses.")]
        public OutfitPiece hair;
        [Name("Earrings"), Description("The name of the earrings Milo scene that this character uses.")]
        public OutfitPiece earrings;
        [Name("Glasses"), Description("The name of the glasses Milo scene that this character uses.")]
        public OutfitPiece glasses;
        [Name("Piercings"), Description("The name of the piercings Milo scene that this character uses.")]
        public OutfitPiece piercings;
        [Name("Feet"), Description("The name of the feet Milo scene that this character uses.")]
        public OutfitPiece feet;
        [Name("Hands"), Description("The name of the hands Milo scene that this character uses.")]
        public OutfitPiece hands;
        [Name("Legs"), Description("The name of the legs Milo scene that this character uses.")]
        public OutfitPiece legs;
        [Name("Rings"), Description("The name of the rings Milo scene that this character uses.")]
        public OutfitPiece rings;
        [Name("Torso"), Description("The name of the torso Milo scene that this character uses.")]
        public OutfitPiece torso;
        [Name("Wrist"), Description("The name of the wrist Milo scene that this character uses.")]
        public OutfitPiece wrist;


        public void Read(EndianReader reader)
        {
            eyebrows = new OutfitPiece().Read(reader);
            earrings = new OutfitPiece().Read(reader);
            faceHair = new OutfitPiece().Read(reader);
            glasses = new OutfitPiece().Read(reader);
            hair = new OutfitPiece().Read(reader);
            piercings = new OutfitPiece().Read(reader);
            feet = new OutfitPiece().Read(reader);
            hands = new OutfitPiece().Read(reader);
            legs = new OutfitPiece().Read(reader);
            rings = new OutfitPiece().Read(reader);
            torso = new OutfitPiece().Read(reader);
            wrist = new OutfitPiece().Read(reader);
        }
        public void Write(EndianWriter writer)
        {
            eyebrows.Write(writer);
            earrings.Write(writer);
            faceHair.Write(writer);
            glasses.Write(writer);
            hair.Write(writer);
            piercings.Write(writer);
            feet.Write(writer);
            hands.Write(writer);
            legs.Write(writer);
            rings.Write(writer);
            torso.Write(writer);
            wrist.Write(writer);
        }
    }

    [Name("Instrument Outfit"), Description("The instruments a character will use")]
    public class InstrumentOutfit
    {
        public OutfitPiece guitar;
        public OutfitPiece bass;
        public OutfitPiece drum;
        public OutfitPiece mic;
        public OutfitPiece keyboard;
        public InstrumentOutfit Read(EndianReader reader)
        {
            guitar = new OutfitPiece().Read(reader);
            bass = new OutfitPiece().Read(reader);
            drum = new OutfitPiece().Read(reader);
            mic = new OutfitPiece().Read(reader);
            keyboard = new OutfitPiece().Read(reader);
            return this;
        }

        public void Write(EndianWriter writer)
        {
            guitar.Write(writer);
            bass.Write(writer);
            drum.Write(writer);
            mic.Write(writer);
            keyboard.Write(writer);
        }
    }

    [Name("Patch"), Description("A patch is a texture that is applied to a mesh.")]
    public class Patch
    {
        [Name("Texture Index"), Description("texture index of profile, -1 means to interpret mesh_name as the actual patch mesh, which would live in color_palettes.milo")]
        public int texture;
        [Name("Category"), Description("Category of this patch")]
        public PatchCategory category;
        [Name("Mesh Name"), Description("name of placement mesh or mapping mesh. Valid placement meshes:\nplacement_legs_L-back.mesh placement_legs_L-front.mesh placement_legs_R-back.mesh placement_legs_R-front.mesh placement_torso_back.mesh placement_torso_front.mesh placement_torso_L-lowerArm.mesh placement_torso_L-shoulder.mesh placement_torso_R-lowerArm.mesh placement_torso_R-shoulder.mesh")]
        public string meshName;
        [Name("Rotation"), Description("in radians, about the uv space z axis.")]
        public float rotation;
        [Name("UV"), Description("UV in the mesh, u == -1 means mesh is mapping mesh.")]
        public Classes.Vector2 uv;
        [Name("Scale"), Description("local x and y scale factors.")]
        public Classes.Vector2 scale;
        public Patch Read(EndianReader reader)
        {
            texture = reader.ReadInt32();
            category = (PatchCategory)reader.ReadInt32();
            meshName = Symbol.Read(reader);
            uv = new Classes.Vector2().Read(reader);
            rotation = reader.ReadFloat();
            scale = new Classes.Vector2().Read(reader);

            return this;
        }
        public void Write(EndianWriter writer)
        {
            writer.WriteInt32(texture);
            writer.WriteInt32((int)category);
            Symbol.Write(writer, meshName);
            uv.Write(writer);
            writer.WriteFloat(rotation);
            scale.Write(writer);
        }
    }
    [Name("BandCharDesc"), Description("Band Character Description, contains all physical appearance attributes.")]
    public class BandCharDesc : Object
    {
        public ushort altRevision;
        public ushort revision; // Constant for revision 0x11 (17)

        [Name("Prefab"), Description("Prefab name if this is a non-editable prefab")]
        public Symbol prefab;

        [Name("Gender"), Description("take a wild guess")]
        public Symbol gender;

        [Name("skin_color"), Description("skin color, taken from skin.pal")]
        public int skinColor;

        public Head head;

        [Name("instruments"), Description("instruments")]
        public InstrumentOutfit instruments;

        [Name("outfit"), Description("clothing")]
        public Outfit outfit;

        public List<Patch> patches = new List<Patch>();

        [Name("height"), Description("Height of character, 0 - 1")]
        public float height;
        [Name("weight"), Description("Weight of character, 0 - 1")]
        public float weight;
        [Name("muscle"), Description("Muscle of character, 0 - 1")]
        public float muscle;

        public int unk224;

        public uint head1;
        public uint head2;


        public BandCharDesc Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            base.Read(reader, false, parent, entry);

            if (revision > 0x10)
                prefab = Symbol.Read(reader);


            gender = Symbol.Read(reader);

            if (revision != 0)
            {
                skinColor = reader.ReadInt32();
                if (revision < 5)
                {
                    head1 = reader.ReadUInt32();
                    head2 = reader.ReadUInt32();
                }
                else
                {
                    head = new Head();
                    head.Read(reader);
                }
            }

            outfit = new Outfit();
            outfit.Read(reader);

            height = reader.ReadFloat();
            weight = reader.ReadFloat();
            muscle = reader.ReadFloat();
            instruments = new InstrumentOutfit();
            instruments.Read(reader);
            int count = reader.ReadInt32();

            // sanity check on the number of patches, there are only a certain number allowed
            if (count < 0 || count > 100)
            {
                throw new InvalidDataException("There are an invalid number of patches in the BandCharDesc, cannot read.");
            }

            patches = new List<Patch>();
            for (int i = 0; i < count; i++)
            {
                Patch patch = new Patch();
                patch.Read(reader);
                patches.Add(patch);
            }

            if (standalone)
            {
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw new Exception("Got to end of standalone asset but didn't find the expected end bytes, read likely did not succeed");
            }
            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));
            base.Write(writer, false, parent, entry);
            Symbol.Write(writer, prefab);
            Symbol.Write(writer, gender);
            writer.WriteInt32(skinColor);

            head.Write(writer);
            outfit.Write(writer);

            writer.WriteFloat(height);
            writer.WriteFloat(weight);
            writer.WriteFloat(muscle);
            instruments.Write(writer);
            writer.WriteInt32(patches.Count);
            foreach (Patch patch in patches)
            {
                patch.Write(writer);
            }

            if (standalone)
            {
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
            }
        }
    }
}