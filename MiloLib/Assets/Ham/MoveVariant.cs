using MiloLib.Assets.Rnd;
using MiloLib.Utils;
using Vector3 = MiloLib.Classes.Vector3;

namespace MiloLib.Assets.Ham
{
    public class MoveVariant
    {
        public int revision;
        public Vector3 positionOffset;
        MoveParent moveParent;

        Symbol index;

        public List<MoveCandidate> prevCandidates = new List<MoveCandidate>();
        public List<MoveCandidate> nextCandidates = new List<MoveCandidate>();

        public Symbol hamMoveName = new(0,"");
        public Symbol hamMoveMiloname = new(0, "");
        public Symbol linkedTo = new(0, "");
        public Symbol linkedFrom = new(0, "");
        public Symbol genre = new(0, "");
        public Symbol era = new(0, "");
        public Symbol songName = new(0, "");
        public float avgBeatsPerSecond;

        uint flags;

        public override string ToString() {
            string str = "MoveVariant";
            str += $"\trev: {revision} position offset: {positionOffset}\n";
            str += $"\tindex: {index} hamMoveName: {hamMoveName} hamMoveMiloName: {hamMoveMiloname}\n";
            str += $"\tgenre: {genre} era: {era} songName: {songName} avgBeatsPerSec: {avgBeatsPerSecond} flags: {flags}\n";
            str += $"\tLinked to: {linkedTo}; Linked from: {linkedFrom}\n";
            str += $"\tPrev candidates ({prevCandidates.Count})\n";
            for(int i = 0; i < prevCandidates.Count; i++) {
                str += $"\t\t{prevCandidates[i]}\n";
            }
            str += $"\tNext candidates ({nextCandidates.Count})\n";
            for (int i = 0; i < nextCandidates.Count; i++) {
                str += $"\t\t{nextCandidates[i]}\n";
            }
            return str;
        }

        public MoveVariant Read(EndianReader reader, MoveParent parent, MoveGraph moveGraph)
        {
            revision = reader.ReadInt32();
            moveParent = parent;
            positionOffset = new Vector3().Read(reader);
            index = Symbol.Read(reader);
            hamMoveName = Symbol.Read(reader);
            hamMoveMiloname = Symbol.Read(reader);
            genre = Symbol.Read(reader);
            era = Symbol.Read(reader);
            songName = Symbol.Read(reader);
            avgBeatsPerSecond = reader.ReadFloat();
            flags = reader.ReadUInt32();

            bool hasLinksTo = reader.ReadBoolean();
            if (hasLinksTo)
            {
                linkedTo = Symbol.Read(reader);
            }

            bool hasLinksFrom = reader.ReadBoolean();
            if (hasLinksFrom)
            {
                linkedFrom = Symbol.Read(reader);
            }

            int numPrevCandidates = reader.ReadInt32();
            for (int i = 0; i < numPrevCandidates; i++)
            {
                prevCandidates.Add(new MoveCandidate().Read(reader));
            }

            int numNextCandidates = reader.ReadInt32();
            for (int i = 0; i < numNextCandidates; i++)
            {
                nextCandidates.Add(new MoveCandidate().Read(reader));
            }

            moveGraph.moveVariants.Add(index, this);

            return this;
        }

        public void Write(EndianWriter writer)
        {
            writer.WriteInt32(revision);
            positionOffset.Write(writer);
            Symbol.Write(writer, index);
            Symbol.Write(writer, hamMoveName);
            Symbol.Write(writer, hamMoveMiloname);
            Symbol.Write(writer, genre);
            Symbol.Write(writer, era);
            Symbol.Write(writer, songName);
            writer.WriteFloat(avgBeatsPerSecond);
            writer.WriteUInt32(flags);

            writer.WriteBoolean(linkedTo != null);
            if (linkedTo != null)
                Symbol.Write(writer, linkedTo);

            writer.WriteBoolean(linkedFrom != null);
            if (linkedFrom != null)
                Symbol.Write(writer, linkedFrom);

            writer.WriteInt32(prevCandidates.Count);
            foreach (var candidate in prevCandidates)
                candidate.Write(writer);

            writer.WriteInt32(nextCandidates.Count);
            foreach (var candidate in nextCandidates)
                candidate.Write(writer);
        }
    }
}