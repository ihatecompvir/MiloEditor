using MiloLib.Assets.UI;
using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets.Band.UI
{
    [Name("BandButton"), Description("Button with app-specific features")]
    public class BandButton : UIButton
    {
        private ushort altRevision;
        private ushort revision;

        [Name("Focus Anim")]
        public Symbol focusAnim = new(0, "");
        [Name("Pulse Anim")]
        public Symbol pulseAnim = new(0, "");

        public BandButton Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            if (revision < 8)
            {
                if (revision <= 4)
                {
                    trans = trans.Read(reader, false, parent, entry);
                    draw = draw.Read(reader, false, parent, entry);
                }
                else
                {
                    base.Read(reader, false, parent, entry);
                }
                if (revision > 2)
                {
                    reader.ReadInt32();
                    reader.ReadInt32();
                    reader.ReadInt32();
                    reader.ReadInt32();
                }
                if (revision <= 4)
                {
                    Symbol.Read(reader);
                }

                if (revision < 8)
                {
                    reader.ReadBoolean();
                }
                if (revision != 0)
                    reader.ReadBoolean();

                width = reader.ReadFloat();
                height = reader.ReadFloat();

                if (revision <= 4)
                {
                    textToken = Symbol.Read(reader);
                }
                if (revision > 5)
                {
                    alignment = (UILabel.TextAlignments)reader.ReadInt32();
                }
            }
            else if (revision == 8)
            {
                base.Read(reader, false, parent, entry);

                fitType = (UILabel.LabelFitTypes)reader.ReadInt32();
                width = reader.ReadFloat();
                height = reader.ReadFloat();
                leading = reader.ReadFloat();
                alignment = (UILabel.TextAlignments)reader.ReadInt32();
                reader.ReadInt32();
                reader.ReadInt32();
                reader.ReadInt32();
                reader.ReadInt32();
                reader.ReadBoolean();
                HmxColor4 color = new HmxColor4().Read(reader);
                kerning = reader.ReadFloat();
                textSize = reader.ReadFloat();
            }
            else
            {
                base.Read(reader, false, parent, entry);
                if (revision < 0xC)
                {
                    fitType = (UILabel.LabelFitTypes)reader.ReadInt32();
                    width = reader.ReadFloat();
                    height = reader.ReadFloat();
                }

                if (revision < 0xB)
                {
                    leading = reader.ReadFloat();
                    alignment = (UILabel.TextAlignments)reader.ReadInt32();
                }

                if (revision < 0xE)
                {
                    reader.ReadInt32();
                    reader.ReadInt32();
                    reader.ReadInt32();
                    reader.ReadInt32();
                }

                if (revision < 0xB)
                {
                    reader.ReadBoolean();
                    kerning = reader.ReadFloat();
                    textSize = reader.ReadFloat();
                }
            }

            if (revision < 0xC)
            {
                reader.ReadInt32();
            }

            if (revision == 0xE)
            {
                // todo

            }

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw new Exception("Got to end of standalone asset but didn't find the expected end bytes, read likely did not succeed");

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            if (revision < 8)
            {
                if (revision <= 4)
                {
                    trans.Write(writer, false, parent, entry);
                    draw.Write(writer, false, parent, entry);
                }
                else
                {
                    base.Write(writer, false, parent, entry);
                }
                if (revision > 2)
                {
                    writer.WriteInt32(0);
                    writer.WriteInt32(0);
                    writer.WriteInt32(0);
                    writer.WriteInt32(0);
                }
                if (revision <= 4)
                {
                    Symbol.Write(writer, new Symbol(0, ""));
                }
                if (revision < 8)
                {
                    writer.WriteBoolean(false);
                }
                if (revision != 0)
                    writer.WriteBoolean(false);

                writer.WriteFloat(width);
                writer.WriteFloat(height);

                if (revision <= 4)
                {
                    Symbol.Write(writer, textToken);
                }
                if (revision > 5)
                {
                    writer.WriteInt32((int)alignment);
                }
            }
            else if (revision == 8)
            {
                base.Write(writer, false, parent, entry);


                writer.WriteInt32((int)fitType);
                writer.WriteFloat(width);
                writer.WriteFloat(height);
                writer.WriteFloat(leading);
                writer.WriteInt32((int)alignment);
                writer.WriteInt32(0);
                writer.WriteInt32(0);
                writer.WriteInt32(0);
                writer.WriteInt32(0);
                writer.WriteBoolean(false);
                new HmxColor4().Write(writer);
                writer.WriteFloat(kerning);
                writer.WriteFloat(textSize);
            }
            else
            {
                base.Write(writer, false, parent, entry);
                if (revision < 0xC)
                {
                    writer.WriteInt32((int)fitType);
                    writer.WriteFloat(width);
                    writer.WriteFloat(height);
                }

                if (revision < 0xB)
                {
                    writer.WriteFloat(leading);
                    writer.WriteInt32((int)alignment);
                }

                if (revision < 0xE)
                {
                    writer.WriteInt32(0);
                    writer.WriteInt32(0);
                    writer.WriteInt32(0);
                    writer.WriteInt32(0);
                }

                if (revision < 0xB)
                {
                    writer.WriteBoolean(false);
                    writer.WriteFloat(kerning);
                    writer.WriteFloat(textSize);
                }

            }

            if (revision < 0xC)
            {
                writer.WriteInt32(0);
            }

            if (revision == 0xE)
            {
                // todo

            }

            if (standalone)
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
        }

    }
}
