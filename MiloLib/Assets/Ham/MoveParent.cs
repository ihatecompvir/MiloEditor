using MiloLib.Assets.Rnd;
using MiloLib.Utils;
using Vector3 = MiloLib.Classes.Vector3;

namespace MiloLib.Assets.Ham
{
    public class MoveParent
    {

        public enum Difficulty
        {
            kDifficultyEasy,
            kDifficultyMedium,
            kDifficultyExpert,
            kDifficultyBeginner,
        }

        public int revision;
        public Symbol name;
        public Difficulty difficulty;

        public List<Symbol> genreFlags = new List<Symbol>();

        public List<Symbol> eraFlags = new List<Symbol>();

        bool unkc;

        public Symbol displayName;

        public List<MoveVariant> moveVariants = new List<MoveVariant>();

        public override string ToString() {
            string str = "MoveParent:\n";
            str += $"\trev {revision}, name {name}, difficulty {difficulty}";
            str += $"\n\tgenre flags ({genreFlags.Count}): ";
            for(int i = 0; i < genreFlags.Count; i++) {
                str += genreFlags[i] + " ";
            }
            str += $"\n\tera flags ({eraFlags.Count}): ";
            for(int i = 0; i < eraFlags.Count; i++) {
                str += eraFlags[i] + " ";
            }
            str += $"\n\tMoveVariants ({moveVariants.Count}):\n";
            for(int i = 0; i < moveVariants.Count; i++) {
                str += moveVariants[i];
            }
            return str;
        }

        public MoveParent Read(EndianReader reader, MoveGraph graph)
        {
            revision = reader.ReadInt32();
            name = Symbol.Read(reader);
            difficulty = (Difficulty)reader.ReadInt32();

            int numGenreFlags = reader.ReadInt32();
            for (int i = 0; i < numGenreFlags; i++)
                genreFlags.Add(Symbol.Read(reader));

            int numEraFlags = reader.ReadInt32();
            for (int i = 0; i < numEraFlags; i++)
                eraFlags.Add(Symbol.Read(reader));

            unkc = reader.ReadBoolean();
            displayName = Symbol.Read(reader);

            int numVariants = reader.ReadInt32();
            for (int i = 0; i < numVariants; i++)
                moveVariants.Add(new MoveVariant().Read(reader, this, graph));

            return this;
        }

        public void Write(EndianWriter writer)
        {
            writer.WriteInt32(revision);
            Symbol.Write(writer, name);
            writer.WriteInt32((int)difficulty);

            writer.WriteInt32(genreFlags.Count);
            foreach (Symbol genreFlag in genreFlags)
                Symbol.Write(writer, genreFlag);

            writer.WriteInt32(eraFlags.Count);
            foreach (Symbol eraFlag in eraFlags)
                Symbol.Write(writer, eraFlag);

            writer.WriteBoolean(unkc);
            Symbol.Write(writer, displayName);

            writer.WriteInt32(moveVariants.Count);
            foreach (MoveVariant variant in moveVariants)
                variant.Write(writer);
        }
    }
}