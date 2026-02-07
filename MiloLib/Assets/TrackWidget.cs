using MiloLib.Assets.Rnd;
using MiloLib.Classes; // Added for attributes
using MiloLib.Utils;
using System; // Added for Exception, BitConverter
using System.Collections.Generic; // Added for List<>

namespace MiloLib.Assets
{
    [Name("TrackWidget"), Description("Any object that is placed on the track and scrolls towards the player. Can have any number of meshes and an environment. Drawn efficiently and pruned automatically by TrackDir.")]
    public class TrackWidget : Object
    {
        public enum WidgetType
        {
            kImmediateWidget = 0,
            kMultiMeshWidget = 1,
            kTextWidget = 2,
            kMatWidget = 3
        };

        private ushort altRevision;
        private ushort revision;

        [MinVersion(1)]
        public RndDrawable draw = new();

        private uint meshCount;
        [Name("meshes"), Description("Meshes used to draw widgets, drawn in order")]
        public List<Symbol> meshes = new();

        private uint meshLeftCount;
        [MinVersion(5)]
        public List<Symbol> meshesLeft = new();
        private uint meshSpanCount;
        [MinVersion(5)]
        public List<Symbol> meshesSpan = new();
        private uint meshRightCount;
        [MinVersion(5)]
        public List<Symbol> meshesRight = new();

        [MinVersion(5)]
        public bool wideWidget;

        [Name("environ"), Description("Environment used to draw widget")]
        public Symbol environ = new(0, "");

        [Name("base_length"), Description("Length of unscaled geometry, should be 0 if no duration"), MinVersion(3)]
        float baseLength;
        [Name("base_width"), Description("Width of unscaled geometry, should be 0 if no scaling"), MinVersion(9)]
        float baseWidth;
        [Name("x_offset"), Description("X offset to be applied to all widget instances"), MinVersion(13)]
        float xOffset;
        [Name("y_offset"), Description("Y offset to be applied to all widget instances"), MinVersion(12)]
        float yOffset;
        [Name("z_offset"), Description("Z offset to be applied to all widget instances"), MinVersion(13)]
        float zOffset;


        [MinVersion(2), MaxVersion(7)]
        public bool shouldUseWideWidget;
        [MinVersion(6), MaxVersion(7)]
        public bool shouldBeTextWidget;

        [Name("allow_rotation"), Description("Allow meshes to be rotated/scaled"), MinVersion(4)]
        public bool allowRotation;

        [Name("font"), MinVersion(6)]
        public Symbol font = new(0, "");
        [Name("text_obj"), MinVersion(7)]
        public Symbol text = new(0, "");

        [Name("chars_per_inst"), MinVersion(7)]
        public int charsPerInst;
        [Name("max_text_instances"), MinVersion(7)]
        public int maxTextInstances;

        [Name("widget_type"), MinVersion(8)]
        public WidgetType widgetType;
        [Name("mat"), MinVersion(8)]
        public Symbol mat = new(0, "");

        [Name("text_alignment"), MinVersion(10)]
        public RndText.Alignment textAlignment;

        [Name("text_color"), Description("Primary color for text instances"), MinVersion(11)]
        public HmxColor4 textColor = new HmxColor4();
        [Name("alt_text_color"), Description("Secondary color for text instances"), MinVersion(11)]
        public HmxColor4 altTextColor = new HmxColor4();

        [Name("allow_shift"), Description("Allow widget instances to shift their X/Z coordinates in coordination with their smasher during a keyboard lane shift"), MinVersion(14)] // 0xD + 1
        public bool allowShift;
        [Name("allow_line_rotation"), Description("Individual lines can have different rotations"), MinVersion(15)] // 0xE + 1
        public bool allowLineRotation;


        public TrackWidget Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            base.Read(reader, false, parent, entry);

            if (revision != 0)
                draw.Read(reader, false, parent, entry);

            meshCount = reader.ReadUInt32();
            meshes.Clear();
            for (int i = 0; i < meshCount; i++)
            {
                Symbol mesh = Symbol.Read(reader);
                meshes.Add(mesh);
            }

            if (revision > 4)
            {
                wideWidget = reader.ReadBoolean();
                meshLeftCount = reader.ReadUInt32();
                meshesLeft.Clear();
                for (int i = 0; i < meshLeftCount; i++)
                {
                    Symbol mesh = Symbol.Read(reader);
                    meshesLeft.Add(mesh);
                }
                meshSpanCount = reader.ReadUInt32();
                meshesSpan.Clear();
                for (int i = 0; i < meshSpanCount; i++)
                {
                    Symbol mesh = Symbol.Read(reader);
                    meshesSpan.Add(mesh);
                }
                meshRightCount = reader.ReadUInt32();
                meshesRight.Clear();
                for (int i = 0; i < meshRightCount; i++)
                {
                    Symbol mesh = Symbol.Read(reader);
                    meshesRight.Add(mesh);
                }
            }
            else
            {
                wideWidget = false;
                meshesLeft.Clear();
                meshesSpan.Clear();
                meshesRight.Clear();
            }

            environ = Symbol.Read(reader);

            if (revision > 2)
                baseLength = reader.ReadFloat();
            else baseLength = 0f;

            if (revision > 8)
                baseWidth = reader.ReadFloat();
            else baseWidth = 0f;

            if (revision >= 2 && revision <= 7)
            {
                shouldUseWideWidget = reader.ReadBoolean();
            }
            else shouldUseWideWidget = false;

            if (revision > 3)
                allowRotation = reader.ReadBoolean();
            else allowRotation = false;

            if (revision > 5)
            {
                font = Symbol.Read(reader);
                if (revision < 8)
                {
                    shouldBeTextWidget = reader.ReadBoolean();
                }
                else shouldBeTextWidget = false;
            }
            else
            {
                font = new(0, "");
                shouldBeTextWidget = false;
            }

            if (revision > 6)
            {
                text = Symbol.Read(reader);
                charsPerInst = reader.ReadInt32();
                maxTextInstances = reader.ReadInt32();
            }
            else
            {
                text = new(0, "");
                charsPerInst = 0;
                maxTextInstances = 0;
            }

            if (revision > 7)
            {
                widgetType = (WidgetType)reader.ReadInt32();
                mat = Symbol.Read(reader);
            }

            if (revision > 9)
            {
                textAlignment = (RndText.Alignment)reader.ReadInt32();
            }
            else textAlignment = RndText.Alignment.kMiddleCenter;

            if (revision > 10)
            {
                textColor = new HmxColor4().Read(reader);
                altTextColor = new HmxColor4().Read(reader);
            }
            else
            {
                textColor = new HmxColor4(1, 1, 1, 1);
                altTextColor = new HmxColor4(1, 1, 1, 1);
            }

            if (revision > 0xB)
            {
                yOffset = reader.ReadFloat();
            }
            else yOffset = 0f;

            if (revision > 0xC)
            {
                xOffset = reader.ReadFloat();
                zOffset = reader.ReadFloat();
            }
            else
            {
                xOffset = 0f;
                zOffset = 0f;
            }

            if (revision > 0xD)
            {
                allowShift = reader.ReadBoolean();
            }
            else allowShift = false;

            if (revision > 0xE)
            {
                allowLineRotation = reader.ReadBoolean();
            }
            else allowLineRotation = false;


            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw MiloLib.Exceptions.MiloAssetReadException.EndBytesNotFound(parent, entry, reader.BaseStream.Position);

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            meshCount = (uint)meshes.Count;
            meshLeftCount = (uint)meshesLeft.Count;
            meshSpanCount = (uint)meshesSpan.Count;
            meshRightCount = (uint)meshesRight.Count;

            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            base.Write(writer, false, parent, entry);

            if (revision != 0)
                draw.Write(writer, false, parent, true);

            writer.WriteUInt32(meshCount);
            for (int i = 0; i < meshCount; i++)
            {
                Symbol.Write(writer, meshes[i]);
            }

            if (revision > 4)
            {
                writer.WriteBoolean(wideWidget);
                writer.WriteUInt32(meshLeftCount);
                for (int i = 0; i < meshLeftCount; i++)
                {
                    Symbol.Write(writer, meshesLeft[i]);
                }
                writer.WriteUInt32(meshSpanCount);
                for (int i = 0; i < meshSpanCount; i++)
                {
                    Symbol.Write(writer, meshesSpan[i]);
                }
                writer.WriteUInt32(meshRightCount);
                for (int i = 0; i < meshRightCount; i++)
                {
                    Symbol.Write(writer, meshesRight[i]);
                }
            }

            Symbol.Write(writer, environ);

            if (revision > 2)
                writer.WriteFloat(baseLength);

            if (revision > 8)
                writer.WriteFloat(baseWidth);

            if (revision >= 2 && revision <= 7)
            {
                writer.WriteBoolean(shouldUseWideWidget);
            }

            if (revision > 3)
                writer.WriteBoolean(allowRotation);

            if (revision > 5)
            {
                Symbol.Write(writer, font);
                if (revision < 8)
                {
                    writer.WriteBoolean(shouldBeTextWidget);
                }
            }

            if (revision > 6)
            {
                Symbol.Write(writer, text);
                writer.WriteInt32(charsPerInst);
                writer.WriteInt32(maxTextInstances);
            }

            if (revision > 7)
            {
                writer.WriteInt32((int)widgetType);
                Symbol.Write(writer, mat);
            }

            if (revision > 9)
            {
                writer.WriteInt32((int)textAlignment);
            }

            if (revision > 10)
            {
                textColor.Write(writer);
                altTextColor.Write(writer);
            }

            if (revision > 0xB)
            {
                writer.WriteFloat(yOffset);
            }

            if (revision > 0xC)
            {
                writer.WriteFloat(xOffset);
                writer.WriteFloat(zOffset);
            }

            if (revision > 0xD)
            {
                writer.WriteBoolean(allowShift);
            }

            if (revision > 0xE)
            {
                writer.WriteBoolean(allowLineRotation);
            }


            if (standalone)
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
        }

        public static TrackWidget New(ushort revision, ushort altRevision)
        {
            TrackWidget widget = new TrackWidget();
            widget.revision = revision;
            widget.altRevision = altRevision;
            return widget;
        }
    }
}