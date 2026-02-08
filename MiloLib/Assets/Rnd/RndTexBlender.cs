using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets.Rnd
{
    [Name("TexBlender"), Description("Renderable texture used to composite pieces of texture maps based on the distance between bones or other animatiable objects")]
    public class RndTexBlender : Object
    {
        private ushort altRevision;
        private ushort revision;

        public RndDrawable draw = new();

        [Name("Base Map"), Description("The base texture map")]
        public Symbol baseMap = new(0, "");
        [Name("Near Map"), Description("The texture map to use when the constraints are closer than the default distance")]
        public Symbol nearMap = new(0, "");
        [Name("Far Map"), Description("The texture map to use when the constraints are further than the default distance")]
        public Symbol farMap = new(0, "");
        [Name("Output Texture"), Description("The final result output texture")]
        public Symbol outputTexture = new(0, "");
        private uint controllerCount;
        [Name("Controllers"), Description("The list of controller objects used to render pieces of a mesh to the output texture")]
        public List<Symbol> controllerList = new();
        [Name("Owner"), Description("The owner of this texture blend. This is used to determine if the texture blend is visible. For example, if this texture blend is used in the head object of a character, set the owner to be the head object.")]
        public Symbol owner = new(0, "");
        [Name("Controller Influence"), Description("Global strength of the blending effect for each controller")]
        public float controllerInfluence;

        public RndTexBlender Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            base.Read(reader, false, parent, entry);
            draw = draw.Read(reader, false, parent, entry);

            outputTexture = Symbol.Read(reader);
            baseMap = Symbol.Read(reader);
            nearMap = Symbol.Read(reader);
            farMap = Symbol.Read(reader);
            controllerCount = reader.ReadUInt32();
            for (int i = 0; i < controllerCount; i++)
            {
                controllerList.Add(Symbol.Read(reader));
            }
            owner = Symbol.Read(reader);
            if (revision > 1)
                controllerInfluence = reader.ReadFloat();

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw MiloLib.Exceptions.MiloAssetReadException.EndBytesNotFound(parent, entry, reader.BaseStream.Position);

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            base.Write(writer, false, parent, entry);
            draw.Write(writer, false, parent, null);

            Symbol.Write(writer, outputTexture);
            Symbol.Write(writer, baseMap);
            Symbol.Write(writer, nearMap);
            Symbol.Write(writer, farMap);
            
            controllerCount = (uint)controllerList.Count;
            writer.WriteUInt32(controllerCount);
            for (int i = 0; i < controllerCount; i++)
            {
                Symbol.Write(writer, controllerList[i]);
            }

            Symbol.Write(writer, owner);
            if (revision > 1)
                writer.WriteFloat(controllerInfluence);

            if (standalone)
                writer.WriteEndBytes();
        }

    }
}
