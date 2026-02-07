using MiloLib.Assets.Rnd;
using MiloLib.Classes;
using MiloLib.Utils;
using System.Reflection.PortableExecutable;

namespace MiloLib.Assets.UI
{
    [Name("BandLabel"), Description("Label with color presets")]
    public class BandLabel : UILabel
    {
        private ushort altRevision;
        private ushort revision;

        public RndTrans trans = new();
        public RndDrawable draw = new();

        public float unk1dcKey1Value;
        public float unk1dcKey1Frame;
        public float unk1dcKey2Value;
        public float unk1dcKey2Frame;
        public Symbol unk1e4 = new(0, "");
        public string unk1e8 = "";
        public bool unk1f4;

        public Symbol inAnim;
        public Symbol outAnim;

        public Symbol oldLabelType = new(0, "");

        public int dummy;

        private uint oldLabelColorNum;

        private ushort oldBandTextCompRevision;
        private ushort oldBandTextCompAltRevision;

        public int unkInt1;
        public int unkInt2;
        public int unkInt3;
        public int unkInt4;

        public bool unkBool87;
        public bool unkBool88;

        public int unkIntI;
        public int unkIntJ;
        public int unkIntK;
        public int unkIntL;

        public int unkInt50;
        public int unkInt6C;

        public float unkFloatWidth;
        public float unkFloatHeight;
        public float unkFloatColR;
        public float unkFloatColG;
        public float unkFloatColB;
        public float unkFloatColA;
        public float unkFloatKerning;
        public float unkFloatTextSize;
        public float unkFloatLeading;

        public BandLabel Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            if (revision < 0xB)
            {
                if (revision <= 6)
                {
                    trans = trans.Read(reader, false, parent, entry);
                    draw = draw.Read(reader, false, parent, entry);
                }
                else base.Read(reader, false, parent, entry);

                if (revision > 5)
                {
                    if (revision < 10)
                    {
                        unkBool88 = reader.ReadBoolean();
                        unkFloatWidth = reader.ReadFloat();
                        unkFloatHeight = reader.ReadFloat();
                        fitType = unkBool88 ? LabelFitTypes.kFitJust : 0;
                    }
                    else
                    {
                        unkInt50 = reader.ReadInt32();
                        unkFloatWidth = reader.ReadFloat();
                        unkFloatHeight = reader.ReadFloat();
                        fitType = (LabelFitTypes)unkInt50;
                    }
                }

                if (revision > 4) unkFloatLeading = reader.ReadFloat();
                if (revision > 3) alignment = (TextAlignments)reader.ReadInt32();

                if (revision < 2)
                {
                    unkIntI = reader.ReadInt32();
                    unkIntJ = reader.ReadInt32();
                    unkIntK = reader.ReadInt32();
                    unkIntL = reader.ReadInt32();
                }

                if (revision <= 6)
                {
                    Symbol s = Symbol.Read(reader);
                }

                if (revision != 0) unkBool87 = reader.ReadBoolean();
                else unkBool87 = false;

                if (revision <= 6) textToken = Symbol.Read(reader);

                if (revision < 10) reader.ReadInt32();

                if (revision > 8)
                {
                    unkFloatColR = reader.ReadFloat();
                    unkFloatColG = reader.ReadFloat();
                    unkFloatColB = reader.ReadFloat();
                    unkFloatColA = reader.ReadFloat();
                }

                if (revision > 9)
                {
                    unkFloatKerning = reader.ReadFloat();
                    unkFloatTextSize = reader.ReadFloat();
                }
            }
            else
            {
                base.Read(reader, false, parent, entry);

                if (revision < 0xE)
                {
                    unkInt6C = reader.ReadInt32();
                    fitType = (LabelFitTypes)unkInt6C;
                    unkFloatWidth = reader.ReadFloat();
                    unkFloatHeight = reader.ReadFloat();

                    if (fitType == 0)
                    {
                        unkFloatHeight = 0;
                        unkFloatWidth = 0;
                    }
                }

                if (revision < 0xD)
                {
                    unkFloatLeading = reader.ReadFloat();
                    alignment = (TextAlignments)reader.ReadInt32();
                }

                if (revision < 0xF)
                {
                    unkIntI = reader.ReadInt32();
                    unkIntJ = reader.ReadInt32();
                    unkIntK = reader.ReadInt32();
                    unkIntL = reader.ReadInt32();
                }

                if (revision < 0xD)
                {
                    unkBool87 = reader.ReadBoolean();
                    unkFloatKerning = reader.ReadFloat();
                    unkFloatTextSize = reader.ReadFloat();
                }

                if (revision < 0xE) reader.ReadInt32();

                if (revision < 0xF)
                {
                    unkFloatColR = reader.ReadFloat();
                    unkFloatColG = reader.ReadFloat();
                    unkFloatColB = reader.ReadFloat();
                    unkFloatColA = reader.ReadFloat();
                }
            }

            if (revision < 0xD)
            {
                int capsmode = 0;
                if (unkBool87) capsmode = 2;
                capsMode = (CapsModes)capsmode;
            }

            if (revision == 0xF) LoadOldBandTextComp(reader);

            if (revision >= 0x11)
            {
                inAnim = Symbol.Read(reader);
                outAnim = Symbol.Read(reader);
            }

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw MiloLib.Exceptions.MiloAssetReadException.EndBytesNotFound(parent, entry, reader.BaseStream.Position);

            return this;
        }
        public void LoadOldBandTextComp(EndianReader reader)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (oldBandTextCompRevision, oldBandTextCompAltRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (oldBandTextCompAltRevision, oldBandTextCompRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            if (oldBandTextCompRevision > 2)
            {
                return;
            }
            else
            {
                if (oldBandTextCompRevision < 1)
                {
                    unkInt1 = reader.ReadInt32();
                    unkInt1 = reader.ReadInt32();
                    unkInt1 = reader.ReadInt32();
                    unkInt1 = reader.ReadInt32();
                }
                oldLabelType = Symbol.Read(reader);
                if (oldLabelType == "custom_colors")
                {
                    oldLabelColorNum = 4;
                    if (oldBandTextCompRevision >= 2) oldLabelColorNum = reader.ReadUInt32();
                    for (int i = 0; i < oldLabelColorNum; i++) dummy = reader.ReadInt32();
                }
            }
        }

        public void WriteOldBandTextComp(EndianWriter writer)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((oldBandTextCompAltRevision << 16) | oldBandTextCompRevision) : (uint)((oldBandTextCompRevision << 16) | oldBandTextCompAltRevision));
            if (oldBandTextCompRevision > 2)
            {
                return;
            }
            else
            {
                if (oldBandTextCompRevision < 1)
                {
                    writer.WriteInt32(unkInt1);
                    writer.WriteInt32(unkInt1);
                    writer.WriteInt32(unkInt1);
                    writer.WriteInt32(unkInt1);
                }
                Symbol.Write(writer, oldLabelType);
                if (oldLabelType == "custom_colors")
                {
                    if (oldBandTextCompRevision >= 2) writer.WriteUInt32(oldLabelColorNum);
                    for (int i = 0; i < oldLabelColorNum; i++) writer.WriteInt32(dummy);
                }
            }
        }
        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            if (revision < 0xB)
            {
                if (revision <= 6)
                {
                    // Write Trans and Drawable members
                    trans.Write(writer, false, parent, true);
                    draw.Write(writer, false, parent, true);
                }
                else
                {
                    base.Write(writer, false, parent, entry);
                }

                if (revision > 5)
                {
                    if (revision < 10)
                    {
                        writer.WriteBoolean(unkBool88);
                        writer.WriteFloat(unkFloatWidth);
                        writer.WriteFloat(unkFloatHeight);
                    }
                    else
                    {
                        writer.WriteInt32((int)fitType);
                        writer.WriteFloat(unkFloatWidth);
                        writer.WriteFloat(unkFloatHeight);
                    }
                }

                if (revision > 4) writer.WriteFloat(unkFloatLeading);
                if (revision > 3) writer.WriteInt32((int)alignment);

                if (revision < 2)
                {
                    writer.WriteInt32(unkIntI);
                    writer.WriteInt32(unkIntJ);
                    writer.WriteInt32(unkIntK);
                    writer.WriteInt32(unkIntL);
                }

                if (revision <= 6) Symbol.Write(writer, new Symbol(0, ""));
                if (revision != 0) writer.WriteBoolean(unkBool87);

                if (revision <= 6) Symbol.Write(writer, textToken);

                if (revision < 10) writer.WriteInt32(0);

                if (revision > 8)
                {
                    writer.WriteFloat(unkFloatColR);
                    writer.WriteFloat(unkFloatColG);
                    writer.WriteFloat(unkFloatColB);
                    writer.WriteFloat(unkFloatColA);
                }

                if (revision > 9)
                {
                    writer.WriteFloat(unkFloatKerning);
                    writer.WriteFloat(unkFloatTextSize);
                }
            }
            else
            {
                base.Write(writer, false, parent, entry);

                if (revision < 0xE)
                {
                    writer.WriteInt32((int)fitType);
                    writer.WriteFloat(unkFloatWidth);
                    writer.WriteFloat(unkFloatHeight);
                }

                if (revision < 0xD)
                {
                    writer.WriteFloat(unkFloatLeading);
                    writer.WriteInt32((int)alignment);
                }

                if (revision < 0xF)
                {
                    writer.WriteInt32(unkIntI);
                    writer.WriteInt32(unkIntJ);
                    writer.WriteInt32(unkIntK);
                    writer.WriteInt32(unkIntL);
                }

                if (revision < 0xD)
                {
                    writer.WriteBoolean(unkBool87);
                    writer.WriteFloat(unkFloatKerning);
                    writer.WriteFloat(unkFloatTextSize);
                }

                if (revision < 0xE) writer.WriteInt32(0);

                if (revision < 0xF)
                {
                    writer.WriteFloat(unkFloatColR);
                    writer.WriteFloat(unkFloatColG);
                    writer.WriteFloat(unkFloatColB);
                    writer.WriteFloat(unkFloatColA);
                }
            }

            if (revision == 0xF)
            {
                WriteOldBandTextComp(writer);
            }

            if (revision >= 0x11)
            {
                Symbol.Write(writer, inAnim);
                Symbol.Write(writer, outAnim);
            }

            if (standalone)
                writer.WriteEndBytes();
        }

    }
}