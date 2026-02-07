using MiloLib.Classes;
using MiloLib.Utils;
using static MiloLib.Assets.Rnd.PropKey;

namespace MiloLib.Assets.Rnd
{
    [Name("RndTransAnim"), Description("TransAnim objects animate the position, rotation, and scale of transformable objects.")]
    public class RndTransAnim : Object
    {
        private ushort altRevision;
        private ushort revision;

        public RndAnimatable anim = new();

        public Symbol trans = new(0, "");
        public Symbol keysOwner = new(0, "");

        [MaxVersion(5)]
        public RndDrawable draw = new();

        [MinVersion(2)]
        public List<QuatKey> rotKeys = new List<QuatKey>();

        [MinVersion(2)]
        public List<Vec3Key> transKeys = new List<Vec3Key>();

        [MinVersion(2)]
        public List<Vec3Key> scaleKeys = new List<Vec3Key>();

        public bool transSpline;
        public bool scaleSpline;
        public bool rotSlerp;
        public bool rotSpline;
        public bool repeatTrans;
        public bool followPath;

        public RndTransAnim Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            if (revision > 4)
                base.Read(reader, false, parent, entry);

            anim = anim.Read(reader, parent, entry);

            if (revision < 6)
                draw = draw.Read(reader, false, parent, entry);

            trans = Symbol.Read(reader);

            if (revision > 2)
            {
                uint rotKeyCount = reader.ReadUInt32();
                for (int i = 0; i < rotKeyCount; i++)
                {
                    QuatKey qk = new();
                    qk.Read(reader);
                    rotKeys.Add(qk);
                }

                uint transKeyCount = reader.ReadUInt32();
                for (int i = 0; i < transKeyCount; i++)
                {
                    Vec3Key vk = new();
                    vk.Read(reader);
                    transKeys.Add(vk);
                }
            }

            keysOwner = Symbol.Read(reader);

            if (revision < 3)
            {
                int transKeyCount = reader.ReadInt32();
                for (int i = 0; i < transKeyCount; i++)
                {
                    Vec3Key vk = new();
                    vk.Read(reader);
                    transKeys.Add(vk);
                }

                int rotKeyCount = reader.ReadInt32();
                for (int i = 0; i < rotKeyCount; i++)
                {
                    QuatKey qk = new();
                    qk.Read(reader);
                    rotKeys.Add(qk);
                }

                int unused = reader.ReadInt32();
            }

            if (revision > 3)
                transSpline = reader.ReadBoolean();
            else
                transSpline = reader.ReadInt32() != 0;

            repeatTrans = reader.ReadBoolean();

            if (revision > 3)
            {
                uint scaleKeyCount = reader.ReadUInt32();
                for (int i = 0; i < scaleKeyCount; i++)
                {
                    Vec3Key vk = new();
                    vk.Read(reader);
                    scaleKeys.Add(vk);
                }
                scaleSpline = reader.ReadBoolean();
            }
            else if (revision > 0)
            {
                if (revision != 2)
                {
                    uint scaleKeyCount = reader.ReadUInt32();
                    for (int i = 0; i < scaleKeyCount; i++)
                    {
                        Vec3Key vk = new();
                        vk.Read(reader);
                        scaleKeys.Add(vk);
                    }
                }
                if (revision < 3)
                {
                    // todo
                }
                scaleSpline = reader.ReadInt32() != 0;
            }

            if (revision > 1)
                followPath = reader.ReadBoolean();
            else
                followPath = (rotKeys.Count == 0 && transKeys.Count > 1);

            if (revision > 3)
                rotSlerp = reader.ReadBoolean();

            if (revision > 6)
                rotSpline = reader.ReadBoolean();

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw new Exception("Did not find end bytes, read likely did not succeed");

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            if (revision > 4)
                base.Write(writer, false, parent, entry);

            anim.Write(writer);

            if (revision < 6)
                draw.Write(writer, false, parent);

            Symbol.Write(writer, trans);

            if (revision > 2)
            {
                writer.WriteUInt32((uint)rotKeys.Count);
                foreach (var qk in rotKeys) qk.Write(writer);

                writer.WriteUInt32((uint)transKeys.Count);
                foreach (var vk in transKeys) vk.Write(writer);
            }

            Symbol.Write(writer, keysOwner);

            if (revision < 3)
            {
                writer.WriteInt32(transKeys.Count);
                foreach (var vk in transKeys) vk.Write(writer);

                writer.WriteInt32(rotKeys.Count);
                foreach (var qk in rotKeys) qk.Write(writer);

                writer.WriteInt32(0);
            }

            if (revision > 3)
                writer.WriteBoolean(transSpline);
            else
                writer.WriteInt32(transSpline ? 1 : 0);

            writer.WriteBoolean(repeatTrans);

            if (revision > 3)
            {
                writer.WriteUInt32((uint)scaleKeys.Count);
                foreach (var vk in scaleKeys) vk.Write(writer);
                writer.WriteBoolean(scaleSpline);
            }
            else if (revision > 0)
            {
                if (revision != 2)
                {
                    writer.WriteUInt32((uint)scaleKeys.Count);
                    foreach (var vk in scaleKeys) vk.Write(writer);
                }
                if (revision < 3)
                {
                    // todo
                }
                writer.WriteInt32(scaleSpline ? 1 : 0);
            }

            if (revision > 1)
                writer.WriteBoolean(followPath);

            if (revision > 3)
                writer.WriteBoolean(rotSlerp);

            if (revision > 6)
                writer.WriteBoolean(rotSpline);

            if (standalone)
                writer.WriteEndBytes();
        }
    }
}
