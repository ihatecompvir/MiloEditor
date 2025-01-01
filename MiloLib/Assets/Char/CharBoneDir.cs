using MiloLib.Classes;
using MiloLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiloLib.Assets.Char
{
    [Name("CharBoneDir"), Description("A CharBone container, acts as a resource file, storing skeleton and DOF for particular contexts")]
    public class CharBoneDir : ObjectDir
    {
        public class Recenter
        {
            private uint targetCount;
            [Name("Targets"), Description("bones to recenter, ie, bone_pelvis")]
            public List<Symbol> targets = new();

            private uint averageCount;
            [Name("Averages"), Description("bones to average to find the new center")]
            public List<Symbol> averages = new();

            [Name("Slide"), Description("Slide the character over the course of the clip.  If false, just uses the start of the clip")]
            public bool slide;

            public Recenter Read(EndianReader reader)
            {
                targetCount = reader.ReadUInt32();
                for (int i = 0; i < targetCount; i++)
                {
                    targets.Add(Symbol.Read(reader));
                }
                averageCount = reader.ReadUInt32();
                for (int i = 0; i < averageCount; i++)
                {
                    averages.Add(Symbol.Read(reader));
                }
                slide = reader.ReadBoolean();
                return this;
            }

            public void Write(EndianWriter writer)
            {
                writer.WriteUInt32((uint)targets.Count);
                foreach (Symbol target in targets)
                {
                    Symbol.Write(writer, target);
                }
                writer.WriteUInt32((uint)averages.Count);
                foreach (Symbol average in averages)
                {
                    Symbol.Write(writer, average);
                }
                writer.WriteBoolean(slide);
            }
        }

        public ushort altRevision;
        public ushort revision;

        [Name("Move Context"), Description("context in which character should move itself around via bone_facing.pos and bone_facing.rotz bones")]
        public uint moveContext;

        [Name("Bake Out Facing"), Description("if false, won't bake out facing, will just bake out position")]
        public bool bakeOutFacing;

        public bool unkBool;

        [Name("Recenter"), Description("Used to limit travel.  Moves [targets] bones so that the average of the [average] bones will be at (0,0,0) at the start of the clip.  If slide is true evaluates the [average] bones and the start of the clip and end of the clip, and recenters [targets] smoothly between those.")]
        public Recenter recenter = new();

        public CharBoneDir(ushort revision, ushort altRevision = 0) : base(revision, altRevision)
        {
            revision = revision;
            altRevision = altRevision;
            return;
        }

        public CharBoneDir Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            base.Read(reader, false, parent, entry);

            if (revision < 2)
            {
                moveContext = reader.ReadBoolean() ? 1u : 0u;
            }
            else
            {
                moveContext = reader.ReadUInt32();
            }

            if (revision < 3)
            {
                unkBool = reader.ReadBoolean();
            }

            recenter = recenter.Read(reader);

            if (revision > 3)
            {
                bakeOutFacing = reader.ReadBoolean();
            }

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw new Exception("Got to end of standalone asset but didn't find the expected end bytes, read likely did not succeed");

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            base.Write(writer, false);

            if (revision < 2)
            {
                writer.WriteBoolean(moveContext == 1);
            }
            else
            {
                writer.WriteUInt32(moveContext);
            }

            if (revision < 3)
            {
                writer.WriteBoolean(unkBool);
            }

            recenter.Write(writer);

            if (revision > 3)
            {
                writer.WriteBoolean(bakeOutFacing);
            }

            if (standalone)
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
        }

        public override bool IsDirectory()
        {
            return true;
        }
    }
}
