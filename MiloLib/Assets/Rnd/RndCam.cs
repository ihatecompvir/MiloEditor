using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets.Rnd
{
    [Name("Cam"), Description("A Camera object is drawable and transformable. When drawn it sets up projection and clipping parameters for subsequent draw siblings.")]
    public class RndCam : Object
    {
        private ushort altRevision;
        private ushort revision;

        public RndTrans trans = new();

        [MaxVersion(9)]
        public RndDrawable draw = new();

        private uint objectsCount;
        [MinVersion(8), MaxVersion(8)]
        public List<Symbol> objects = new List<Symbol>();

        [Name("Near Plane"), Description("The distance in world coordinates to the near clipping plane. The near/far ratio is limited to 1:1000 to preserve Z-buffer resolution.")]
        public float nearPlane;
        [Name("Far Plane"), Description("The distance in world coordinates to the far clipping plane. The near/far ratio is limited to 1:1000 to preserve Z-buffer resolution. Note that on the PS2, object polys are culled rather than clipped to the far plane.")]
        public float farPlane;
        [Name("Y FOV"), Description("The vertical field of view in degrees.")]
        public float yFOV;

        [MaxVersion(1)]
        public uint unkInt1;
        public uint unkInt2;
        [MinVersion(6), MaxVersion(6)]
        public uint unkInt3;

        [Name("Z Range"), Description("The part of the Z-buffer to use, in normalized coordinates. It can be useful to draw a scene where the near and far planes must exceed the 1:1000 ratio (so multiple cameras are used to draw farthest to nearest perObjs, each using a closer range of the z-buffer) or to leave some z-buffer for HUD overlay perObjs."), MinVersion(4)]
        public Vector2 zRange = new();

        [Name("Target Texture"), MinVersion(5)]
        public Symbol targetTex = new(0, "");

        [Name("Screen Rect"), Description("The area of the screen in normalized coordinates (0 to 1) to draw into.")]
        public Rect screenRect = new();

        public RndCam Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)(combinedRevision >> 16 & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)(combinedRevision >> 16 & 0xFFFF));

            if (revision > 10)
            {
                base.Read(reader, false, parent, entry);
            }

            trans = trans.Read(reader, false, parent, entry);

            if (revision < 10)
            {
                draw = draw.Read(reader, false, parent, entry);
            }

            if (revision == 8)
            {
                objectsCount = reader.ReadUInt32();
                for (int i = 0; i < objectsCount; i++)
                {
                    objects.Add(Symbol.Read(reader));
                }
            }

            nearPlane = reader.ReadFloat();
            farPlane = reader.ReadFloat();
            yFOV = reader.ReadFloat();

            if (revision < 2)
            {
                unkInt1 = reader.ReadUInt32();
            }

            screenRect = screenRect.Read(reader);

            if ((revision - 1) <= 1)
            {
                unkInt2 = reader.ReadUInt32();
            }

            if (revision > 3)
            {
                zRange = zRange.Read(reader);
            }

            if (revision > 4)
            {
                targetTex = Symbol.Read(reader);
            }

            if (revision == 6)
            {
                unkInt3 = reader.ReadUInt32();
            }

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw MiloLib.Exceptions.MiloAssetReadException.EndBytesNotFound(parent, entry, reader.BaseStream.Position);

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)(altRevision << 16 | revision) : (uint)(revision << 16 | altRevision));

            if (revision > 10)
            {
                base.Write(writer, false, parent, entry);
            }

            trans.Write(writer, false, parent, null);

            if (revision < 10)
            {
                draw.Write(writer, false, parent, null);
            }

            if (revision == 8)
            {
                writer.WriteUInt32((uint)objects.Count);
                foreach (var obj in objects)
                {
                    Symbol.Write(writer, obj);
                }
            }

            writer.WriteFloat(nearPlane);

            writer.WriteFloat(farPlane);

            writer.WriteFloat(yFOV);

            if (revision < 2)
            {
                writer.WriteUInt32(unkInt1);
            }

            screenRect.Write(writer);

            if ((revision - 1) <= 1)
            {
                writer.WriteUInt32(unkInt2);
            }

            if (revision > 3)
            {
                zRange.Write(writer);
            }

            if (revision > 4)
            {
                Symbol.Write(writer, targetTex);
            }

            if (revision == 6)
            {
                writer.WriteUInt32(unkInt3);
            }

            if (standalone)
                writer.WriteEndBytes();
        }

    }
}
