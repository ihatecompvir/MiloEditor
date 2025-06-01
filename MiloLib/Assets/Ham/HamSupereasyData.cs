using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using MiloLib.Assets.Rnd;
using MiloLib.Classes;
using MiloLib.Utils;
using static MiloLib.Assets.Ham.HamBattleData;

namespace MiloLib.Assets.Ham {
    public class HamSupereasyData : Object {

        private ushort altRevision;
        private ushort revision;

        public class HamSupereasyMeasure {
            [Name("First"), Description("MoveVariant to use for transition into measure")]
            Symbol first = new(0, "");
            [Name("Second"), Description("MoveVariant to use for transition out of measure")]
            Symbol second = new(0, "");
            [Name("Preferred"), Description("Preferred MoveVariant for this measure")]
            Symbol preferred = new(0, "");

            public HamSupereasyMeasure Read(EndianReader reader) {
                first = Symbol.Read(reader);
                second = Symbol.Read(reader);
                preferred = Symbol.Read(reader);
                return this;
            }
        }

        private uint numSuperEasyMeasures;
        public List<HamSupereasyMeasure> mRoutine = new();

        public HamSupereasyData Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry) {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            base.Read(reader, false, parent, entry);

            numSuperEasyMeasures = reader.ReadUInt32();
            for(int i = 0; i < numSuperEasyMeasures; i++) {
                mRoutine.Add(new HamSupereasyMeasure().Read(reader));
            }

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw new Exception("Got to end of standalone asset but didn't find the expected end bytes, read likely did not succeed");

            return this;
        }
    }
}