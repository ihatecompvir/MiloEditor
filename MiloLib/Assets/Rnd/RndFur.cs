using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets.Rnd
{
    [Name("Fur"), Description("Parameters for fur shading, to be set on a material")]
    public class RndFur : Object
    {
        private ushort altRevision;
        private ushort revision;


        [Name("Roots Tint"), Description("Tint at hair roots")]
        public HmxColor4 rootsTint = new(1, 1, 1, 1);
        [Name("Ends Tint"), Description("Tint at hair ends")]
        public HmxColor4 endsTint = new(1, 1, 1, 1);

        [Name("Fur Tiling"), Description("Tiling for fur detail map. UVs of fur_detail are multiplied by this value.")]
        public float furTiling;
        [Name("Fluidity"), Description("Langor of motion")]
        public float fluidity;
        [Name("Gravity"), Description("Strength of gravity")]
        public float gravity;
        [Name("Slide"), Description("Maximum lateral motion"), MinVersion(2)]
        public float slide;
        [Name("Stretch"), Description("Maximum stretch"), MinVersion(2)]
        public float stretch;
        [Name("Alpha Falloff"), Description("Bunch opacity towards surface")]
        public float alphaFalloff;
        [Name("Shell Out"), Description("Bunch shells towards surface")]
        public float shellOut;
        [Name("Curvature"), Description("Curvature exponent")]
        public float curvature;
        [Name("Thickness"), Description("Length of fur")]
        public float thickness;
        public float unknown;

        [Name("Fur Detail Map"), Description("Detail map for finer fur.  Only the alpha channel is used.")]
        public Symbol furDetailMap = new(0, "");

        [Name("Wind"), Description("Wind Object, if set, blows on the fur."), MinVersion(3)]
        public Symbol wind = new(0, "");



        public RndFur Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            base.Read(reader, false, parent, entry);

            furTiling = reader.ReadFloat();
            fluidity = reader.ReadFloat();
            gravity = reader.ReadFloat();
            if (revision > 1)
            {
                slide = reader.ReadFloat();
                stretch = reader.ReadFloat();
            }

            alphaFalloff = reader.ReadFloat();
            shellOut = reader.ReadFloat();
            curvature = reader.ReadFloat();
            thickness = reader.ReadFloat();

            rootsTint = new HmxColor4().Read(reader);
            endsTint = new HmxColor4().Read(reader);

            furDetailMap = Symbol.Read(reader);
            unknown = reader.ReadFloat();

            if (revision > 2)
            {
                wind = Symbol.Read(reader);
            }


            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw MiloLib.Exceptions.MiloAssetReadException.EndBytesNotFound(parent, entry, reader.BaseStream.Position);

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            base.Write(writer, false, parent, entry);

            writer.WriteFloat(furTiling);
            writer.WriteFloat(fluidity);
            writer.WriteFloat(gravity);
            if (revision > 1)
            {
                writer.WriteFloat(slide);
                writer.WriteFloat(stretch);
            }
            writer.WriteFloat(alphaFalloff);
            writer.WriteFloat(shellOut);
            writer.WriteFloat(curvature);
            writer.WriteFloat(thickness);

            rootsTint.Write(writer);
            endsTint.Write(writer);

            Symbol.Write(writer, furDetailMap);
            writer.WriteFloat(unknown);

            if (revision > 2)
            {
                Symbol.Write(writer, wind);
            }

            if (standalone)
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
        }

    }
}
