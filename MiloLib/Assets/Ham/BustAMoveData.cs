using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using MiloLib.Assets.Rnd;
using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets.Ham
{
    public class BustAMoveData : Object
    {

        private ushort altRevision;
        private ushort revision;

        public class BAMPhrase
        {
            // "How many times this bar phrasing repeats". Ranges from 1 to 100.
            int count;
            // "How many bars per phrase". Ranges from 1 to 100.
            int bars;

            public BAMPhrase Read(EndianReader reader)
            {
                count = reader.ReadInt32();
                bars = reader.ReadInt32();
                return this;
            }

            public BAMPhrase Write(EndianWriter writer)
            {
                writer.WriteInt32(count);
                writer.WriteInt32(bars);
                return this;
            }
        };

        private uint numPhrases;
        public List<BAMPhrase> mPhrases = new();

        public BustAMoveData Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            base.Read(reader, false, parent, entry);

            numPhrases = reader.ReadUInt32();
            for (int i = 0; i < numPhrases; i++)
            {
                mPhrases.Add(new BAMPhrase().Read(reader));
            }

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw new Exception("Got to end of standalone asset but didn't find the expected end bytes, read likely did not succeed");

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            base.Write(writer, false, parent, entry);

            writer.WriteUInt32(numPhrases);
            foreach (BAMPhrase phrase in mPhrases)
            {
                phrase.Write(writer);
            }

            if (standalone)
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
        }
    }
}