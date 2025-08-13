using MiloLib.Utils;
using Vector3 = MiloLib.Classes.Vector3;

namespace MiloLib.Assets.Ham
{
    public class DancerSkeleton
    {
        public List<Vector3> mCamJointDisplacements = new List<Vector3>(20);
        public List<Vector3> mCamJointPositions = new List<Vector3>(20);
        public List<float> mCamBoneLengths = new List<float>(19);
        public int mElapsedMs;
        public bool mTracked;

        public override string ToString() {
            string str = "DancerSkeleton:\n";
            str += "Cam joint displacements:\n";
            for (int i = 0; i < 20; i++) {
                str += $"{(SkeletonJoint)i}: {mCamJointDisplacements[i]}\n";
            }
            str += "Cam joint positions:\n";
            for(int i = 0; i < 20; i++) {
                str += $"{(SkeletonJoint)i}: {mCamJointPositions[i]}\n";
            }
            //str += "Cam bone lengths:\n";
            //for(int i = 0; i < 19; i++) {
            //    str += $"{(SkeletonBone)i}: {mCamBoneLengths[i]}\n";
            //}
            str += $"Elapsed ms: {mElapsedMs}\n";
            str += $"Tracked: {mTracked}\n";
            return str;
        }
        public DancerSkeleton Read(EndianReader reader)
        {
            for (int i = 0; i < 20; i++)
            {
                mCamJointPositions.Add(new Vector3().Read(reader));
                mCamJointDisplacements.Add(new Vector3().Read(reader));
            }

            mElapsedMs = reader.ReadInt32();
            mTracked = reader.ReadBoolean();

            return this;
        }

        public void Write(EndianWriter writer)
        {
            for (int i = 0; i < 20; i++)
            {
                mCamJointPositions[i].Write(writer);
                mCamJointDisplacements[i].Write(writer);
            }

            writer.WriteInt32(mElapsedMs);
            writer.WriteBoolean(mTracked);
        }
    }
}