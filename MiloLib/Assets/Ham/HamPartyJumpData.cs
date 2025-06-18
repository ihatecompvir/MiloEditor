using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using MiloLib.Assets.Rnd;
using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets.Ham
{
    public class HamPartyJumpData : Object
    {

        private ushort altRevision;
        private ushort revision;

        private uint numJumps;
        // pair<int from_measure, int to_measure>
        // "When song reaches 'from_measure', jump to start of 'to_measure'. 'from_measure' is not played."
        public List<Tuple<int, int>> mJumps = new List<Tuple<int, int>>();

        public HamPartyJumpData Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            base.Read(reader, false, parent, entry);

            numJumps = reader.ReadUInt32();
            for (int i = 0; i < numJumps; i++)
            {
                mJumps.Add(new Tuple<int, int>(reader.ReadInt32(), reader.ReadInt32()));
            }

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw new Exception("Got to end of standalone asset but didn't find the expected end bytes, read likely did not succeed");

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            uint combinedRevision = BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision);
            writer.WriteUInt32(combinedRevision);

            base.Write(writer, false, parent, entry);

            writer.WriteUInt32(numJumps);
            foreach (var jump in mJumps)
            {
                writer.WriteInt32(jump.Item1);
                writer.WriteInt32(jump.Item2);
            }

            if (standalone)
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
        }

    }
}