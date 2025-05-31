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