using MiloLib.Assets.Rnd;
using MiloLib.Utils;
using Vector3 = MiloLib.Classes.Vector3;

namespace MiloLib.Assets.Ham
{
    public class DancerSequence : Object
    {

        public ushort revision;
        public ushort altRevision;

        public RndAnimatable rndAnimatable = new RndAnimatable();

        public class DancerFrame
        {
            public short unk0;
            public short unk2;

            public DancerSkeleton mSkeleton = new DancerSkeleton();
        }

        public List<DancerFrame> mDancerFrames = new List<DancerFrame>();

        public override string ToString()
        {
            string str = $"DancerSequence: revs: ({revision}, {altRevision})\n";
            str += $"DancerFrames ({mDancerFrames.Count}):\n\n";
            str += $"Showing the first 50 frames for performance purposes:\n";
            for (int i = 0; i < 50; i++)
            {
                str += $"DancerFrame {i} of {mDancerFrames.Count}:\n";
                str += $"unk0 {mDancerFrames[i].unk0} unk2 {mDancerFrames[i].unk2}\n";
                str += mDancerFrames[i].mSkeleton + "\n";
            }
            return str;
        }

        public new DancerSequence Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            // Read revision
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            // Base Read
            base.Read(reader, false, parent, entry);
            rndAnimatable.Read(reader, parent, entry);

            // DancerSequence length
            int numFrames = reader.ReadInt32();

            for (int i = 0; i < numFrames; i++)
            {
                DancerFrame curFrame = new DancerFrame();
                mDancerFrames.Add(curFrame);

                // Unknown data
                if (revision == 0)
                {
                    reader.ReadInt32(); // Dummy stuff
                    curFrame.unk2 = -1;
                    curFrame.unk0 = -1;
                }
                else
                {
                    curFrame.unk0 = reader.ReadInt16();
                    curFrame.unk2 = reader.ReadInt16();
                }

                DancerSkeleton skeleton = curFrame.mSkeleton;
                if (revision < 7)
                {
                    // non-relevant stuff here, ignore lol
                }
                else
                {
                    for (int k = 0; k < 20; k++)
                    {
                        Vector3 vpos = new Vector3().Read(reader);
                        Vector3 vdisp = new Vector3().Read(reader);
                        skeleton.mCamJointPositions.Add(vpos);
                        skeleton.mCamJointDisplacements.Add(vdisp);
                        if (revision < 8)
                        {
                            reader.ReadInt32(); // Dummy stuff
                        }
                    }
                    int elapsed = reader.ReadInt32();
                    skeleton.mElapsedMs = elapsed;
                }
            }



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
            rndAnimatable.Write(writer);

            // DancerSequence length
            writer.WriteInt32(mDancerFrames.Count);

            foreach (DancerFrame curFrame in mDancerFrames)
            {
                // Unknown data
                if (revision == 0)
                {
                    writer.WriteInt32(0); // Dummy stuff
                    // Note: We don't write unk0 and unk2 for revision 0, as they're set to -1 in Read
                }
                else
                {
                    writer.WriteInt16(curFrame.unk0);
                    writer.WriteInt16(curFrame.unk2);
                }

                DancerSkeleton skeleton = curFrame.mSkeleton;
                if (revision < 7)
                {
                    // non-relevant stuff here, ignore lol
                }
                else
                {
                    for (int k = 0; k < 20; k++)
                    {
                        skeleton.mCamJointPositions[k].Write(writer);
                        skeleton.mCamJointDisplacements[k].Write(writer);
                        if (revision < 8)
                        {
                            writer.WriteInt32(0); // Dummy stuff
                        }
                    }
                    writer.WriteInt32(skeleton.mElapsedMs);
                }
            }

            if (standalone)
            {
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
            }
        }
    }
}