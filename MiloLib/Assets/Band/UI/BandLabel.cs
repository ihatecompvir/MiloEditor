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

        public int unkInt1;
        public int unkInt2;
        public int unkInt3;
        public int unkInt4;

        public BandLabel Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            bool b87 = false;

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
                        bool b88 = reader.ReadBoolean();
                        width = reader.ReadFloat();
                        height = reader.ReadFloat();
                        if (b88) fitType = LabelFitTypes.kFitJust;
                        else fitType = 0;
                    }
                    else
                    {
                        int i50 = reader.ReadInt32();
                        width = reader.ReadFloat();
                        height = reader.ReadFloat();
                        fitType = (LabelFitTypes)i50;
                    }
                }
                if (revision > 4) leading = reader.ReadFloat();
                if (revision > 3) alignment = (TextAlignments)reader.ReadInt32();
                if (revision < 2)
                {
                    int i, j, k, l;
                    i = reader.ReadInt32();
                    j = reader.ReadInt32();
                    k = reader.ReadInt32();
                    l = reader.ReadInt32();
                }
                if (revision <= 6)
                {
                    Symbol s = Symbol.Read(reader);
                }
                if (revision != 0) b87 = reader.ReadBoolean();
                else b87 = false;
                if (revision <= 6) textToken = Symbol.Read(reader);
                if (revision < 10) reader.ReadInt32();
                if (revision > 8)
                {
                    float colr = reader.ReadFloat();
                    float colg = reader.ReadFloat();
                    float colb = reader.ReadFloat();
                    float cola = reader.ReadFloat();
                }
                if (revision > 9)
                {
                    kerning = reader.ReadFloat();
                    textSize = reader.ReadFloat();
                }
            }
            else
            {
                base.Read(reader, false, parent, entry);
                if (revision < 0xE)
                {
                    int i6c = reader.ReadInt32();
                    fitType = (LabelFitTypes)i6c;
                    width = reader.ReadFloat();
                    height = reader.ReadFloat();
                    if (fitType == 0)
                    {
                        height = 0;
                        width = 0;
                    }
                }
                if (revision < 0xD)
                {
                    leading = reader.ReadFloat();
                    alignment = (TextAlignments)reader.ReadInt32();
                }
                if (revision < 0xF)
                {
                    int i, j, k, l;
                    i = reader.ReadInt32();
                    j = reader.ReadInt32();
                    k = reader.ReadInt32();
                    l = reader.ReadInt32();
                }
                if (revision < 0xD)
                {
                    b87 = reader.ReadBoolean();
                    kerning = reader.ReadFloat();
                    textSize = reader.ReadFloat();
                }
                if (revision < 0xE) reader.ReadInt32();
                if (revision < 0xF)
                {
                    float colr = reader.ReadFloat();
                    float colg = reader.ReadFloat();
                    float colb = reader.ReadFloat();
                    float cola = reader.ReadFloat();
                }
            }
            if (revision < 0xD)
            {
                int capsmode = 0;
                if (b87) capsmode = 2;
                capsMode = (CapsModes)capsmode;
            }
            if (revision == 0xF) LoadOldBandTextComp(reader);
            if (revision >= 0x11)
            {
                inAnim = Symbol.Read(reader);
                outAnim = Symbol.Read(reader);
            }
            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw new Exception("Got to end of standalone asset but didn't find the expected end bytes, read likely did not succeed");

            return this;
        }
        public void LoadOldBandTextComp(EndianReader reader)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            if (revision > 2)
            {
                return;
            }
            else
            {
                if (revision < 1)
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
                    if (revision >= 2) oldLabelColorNum = reader.ReadUInt32();
                    for (int i = 0; i < oldLabelColorNum; i++) dummy = reader.ReadInt32();
                }
            }
        }
        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            bool b87 = false;

            if (revision < 0xB)
            {
                if (revision <= 6)
                {

                }
                else base.Write(writer, false, parent, entry);
                if (revision > 5)
                {
                    if (revision < 10)
                    {
                        writer.WriteBoolean(false); //b88
                        writer.WriteFloat(width);
                        writer.WriteFloat(height);
                    }
                    else
                    {
                        writer.WriteInt32((int)fitType);
                        writer.WriteFloat(width);
                        writer.WriteFloat(height);
                    }
                }
                if (revision > 4) writer.WriteFloat(leading);
                if (revision > 3) writer.WriteInt32((int)alignment);
                if (revision < 2)
                {
                    writer.WriteInt32(0);
                    writer.WriteInt32(0);
                    writer.WriteInt32(0);
                    writer.WriteInt32(0);
                }
                if (revision <= 6)
                {
                    Symbol.Write(writer, new Symbol(0, ""));
                }
                if (revision != 0) writer.WriteBoolean(b87);
                if (revision <= 6) Symbol.Write(writer, textToken);
                if (revision < 10) writer.WriteInt32(0);
                if (revision > 8)
                {
                    writer.WriteFloat(0);
                    writer.WriteFloat(0);
                    writer.WriteFloat(0);
                    writer.WriteFloat(0);
                }
                if (revision > 9)
                {
                    writer.WriteFloat(kerning);
                    writer.WriteFloat(textSize);
                }
            }
            else
            {
                base.Write(writer, false, parent, entry);
                if (revision < 0xE)
                {
                    writer.WriteInt32((int)fitType);
                    writer.WriteFloat(width);
                    writer.WriteFloat(height);
                }
                if (revision < 0xD)
                {
                    writer.WriteFloat(leading);
                    writer.WriteInt32((int)alignment);
                }
                if (revision < 0xF)
                {
                    writer.WriteInt32(0);
                    writer.WriteInt32(0);
                    writer.WriteInt32(0);
                    writer.WriteInt32(0);
                }
                if (revision < 0xD)
                {
                    writer.WriteBoolean(b87);
                    writer.WriteFloat(kerning);
                    writer.WriteFloat(textSize);
                }
                if (revision < 0xE) writer.WriteInt32(0);
                if (revision < 0xF)
                {
                    writer.WriteFloat(0);
                    writer.WriteFloat(0);
                    writer.WriteFloat(0);
                    writer.WriteFloat(0);
                }
            }
            if (revision < 0xD)
            {
                int capsmode = 0;
                if (b87) capsmode = 2;
                capsMode = (CapsModes)capsmode;
            }

            if (revision == 0xF)
            {
                // todo
            }
            if (revision >= 0x11)
            {
                Symbol.Write(writer, inAnim);
                Symbol.Write(writer, outAnim);
            }
            if (standalone)
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });

        }
    }
}