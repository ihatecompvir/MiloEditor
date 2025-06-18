using MiloLib.Assets.Rnd;
using MiloLib.Utils;
using static MiloLib.Assets.ObjectFields;
using Vector3 = MiloLib.Classes.Vector3;

namespace MiloLib.Assets.Ham
{
    public class MoveGraph : Object
    {

        public ushort revision;
        public ushort altRevision;

        public Dictionary<Symbol, MoveParent> moveParents = new Dictionary<Symbol, MoveParent>();
        public Dictionary<Symbol, MoveVariant> moveVariants = new Dictionary<Symbol, MoveVariant>();

        public DTBArrayParent moveArray = new DTBArrayParent();

        public override string ToString()
        {
            string str = "MoveGraph:\n";
            str += $"Move parents ({moveParents.Count}):\n\n";
            int count = 1;
            foreach (var parent in moveParents)
            {
                str += $"MoveParent {count} of {moveParents.Count}: (\n";
                str += $"Move symbol: {parent.Key}\n";
                str += parent.Value;
                str += ")\n\n";
                count += 1;
            }

            str += $"Move variants ({moveVariants.Count}):\n\n";
            count = 1;
            foreach (var variant in moveVariants)
            {
                str += $"MoveVariant {count} of {moveVariants.Count}: (\n";
                str += $"Move symbol: {variant.Key}\n";
                str += variant.Value;
                str += ")\n\n";
                count += 1;
            }

            str += $"Move array:\n";
            for (int i = 0; i < moveArray.children.Count; i++)
            {
                str += moveArray.children[i].ToString() + "\n";
            }

            return str;
        }

        public new MoveGraph Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            // Read revision
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            // Base Read
            base.Read(reader, false, parent, entry);

            int numParents = reader.ReadInt32();
            for (int i = 0; i < numParents; i++)
            {
                MoveParent moveParent = new MoveParent();
                moveParent.Read(reader, this);
                moveParents[moveParent.name] = moveParent;
            }

            moveArray.Read(reader);

            if (standalone)
            {
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32())
                    throw new Exception("Got to end of standalone asset but didn't find the expected end bytes, read likely did not succeed");
            }

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            // Revision
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            // Base Write
            base.Write(writer, false, parent, entry);

            // Write number of parents
            writer.WriteInt32(moveParents.Count);

            // Write each MoveParent
            foreach (var moveParent in moveParents.Values)
            {
                moveParent.Write(writer);
            }

            // Write moveArray
            moveArray.Write(writer);

            if (standalone)
            {
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
            }
        }
    }
}