using MiloLib.Assets.Rnd;
using MiloLib.Assets.UI;
using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets
{
    [Name("GHTrackDir"), Description("")]
    public class GHTrackDir : RndDir
    {
        public enum PlayerSettings : uint
        {
            kPlayer1,
            kPlayer2,
            kPlayerNone,
            kPlayerShared,
        }
        private ushort altRevision;
        private ushort revision;

        public PlayerSettings playerSettings;

        public Symbol spCam = new(0, "");

        private uint mpCamCount;
        public List<Symbol> mpCams = new();

        private uint gemWidgetCount;
        public List<Symbol> gemWidgets = new();

        private uint hopoWidgetCount;
        public List<Symbol> hopoWidgets = new();

        private uint starWidgetCount;
        public List<Symbol> starWidgets = new();

        private uint starHopoWidgetCount;
        public List<Symbol> starHopoWidgets = new();


        public GHTrackDir(ushort revision, ushort altRevision = 0) : base(revision, altRevision)
        {
            revision = revision;
            altRevision = altRevision;
            return;
        }

        public GHTrackDir Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            playerSettings = (PlayerSettings)reader.ReadUInt32();

            spCam = Symbol.Read(reader);

            mpCamCount = reader.ReadUInt32();
            for (int i = 0; i < mpCamCount; i++)
            {
                mpCams.Add(Symbol.Read(reader));
            }

            gemWidgetCount = reader.ReadUInt32();
            for (int i = 0; i < gemWidgetCount; i++)
            {
                gemWidgets.Add(Symbol.Read(reader));
            }

            hopoWidgetCount = reader.ReadUInt32();
            for (int i = 0; i < hopoWidgetCount; i++)
            {
                hopoWidgets.Add(Symbol.Read(reader));
            }

            starWidgetCount = reader.ReadUInt32();
            for (int i = 0; i < starWidgetCount; i++)
            {
                starWidgets.Add(Symbol.Read(reader));
            }

            starHopoWidgetCount = reader.ReadUInt32();
            for (int i = 0; i < starHopoWidgetCount; i++)
            {
                starHopoWidgets.Add(Symbol.Read(reader));
            }

            base.Read(reader, false, parent, entry);

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw MiloLib.Exceptions.MiloAssetReadException.EndBytesNotFound(parent, entry, reader.BaseStream.Position);

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            writer.WriteUInt32((uint)playerSettings);

            Symbol.Write(writer, spCam);

            writer.WriteUInt32(mpCamCount);
            foreach (Symbol mpCam in mpCams)
            {
                Symbol.Write(writer, mpCam);
            }

            writer.WriteUInt32(gemWidgetCount);
            foreach (Symbol gemWidget in gemWidgets)
            {
                Symbol.Write(writer, gemWidget);
            }

            writer.WriteUInt32(hopoWidgetCount);
            foreach (Symbol hopoWidget in hopoWidgets)
            {
                Symbol.Write(writer, hopoWidget);
            }

            writer.WriteUInt32(starWidgetCount);
            foreach (Symbol starWidget in starWidgets)
            {
                Symbol.Write(writer, starWidget);
            }

            writer.WriteUInt32(starHopoWidgetCount);
            foreach (Symbol starHopoWidget in starHopoWidgets)
            {
                Symbol.Write(writer, starHopoWidget);
            }

            base.Write(writer, false, parent, entry);

            if (standalone)
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
        }

        public override bool IsDirectory()
        {
            return true;
        }

    }
}
