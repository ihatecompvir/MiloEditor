using System.Diagnostics;
using System.Numerics;
using MiloLib.Assets.Rnd;
using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets.Ham {
    public class OldNodeWeight {
        float unk0, unk4, unk8, unkc, unk10;
    }

    public class Ham1NodeWeight {
        bool unk0;
        float unk4, unk8, unkc, unk10;

        public Ham1NodeWeight Read(EndianReader reader) {
            unk4 = reader.ReadFloat();
            unk8 = reader.ReadFloat();
            unkc = reader.ReadFloat();
            unk10 = reader.ReadFloat();
            unk0 = reader.ReadBoolean();
            return this;
        }
    }

    public class Ham2FrameWeight {
        float unk0;
        float[] unk4 = new float[4];
        float[] unk14 = new float[4];
        public Ham2FrameWeight Read(EndianReader reader) {
            unk0 = reader.ReadFloat();
            for (int i = 0; i < 4; i++) {
                unk4[i] = reader.ReadFloat();
                unk14[i] = reader.ReadFloat();
            }
            return this;
        }
    }

    public enum NodeCounts {
        kNumHam1Nodes = 16,
        kMaxNumErrorNodes = 33
    }

    public class MoveFrame {
        public float unk0; // beat?
        public int unk4;
        // TODO: this 64 is actually indexed as a 2D array of some sort (16 by 4 maybe?)
        Ham1NodeWeight[] mHam1NodeWeights = new Ham1NodeWeight[64];
        Classes.Vector3[,] mNodeWeights = new Classes.Vector3[33, 2];
        Classes.Vector3[,] mNodeScales = new Classes.Vector3[33, 2];
        Classes.Vector3[,] mNodesInverseScale = new Classes.Vector3[33, 2];
        Ham2FrameWeight[] mFrameWeights = new Ham2FrameWeight[2];

        public MoveFrame Read(EndianReader reader) {
            unk0 = reader.ReadFloat();
            unk4 = reader.ReadInt32();
            int always16 = reader.ReadInt32(); // expecting this to be 16

            for (int i = 0; i < 64; i++) {
                mHam1NodeWeights[i] = new Ham1NodeWeight().Read(reader);
            }

            for (int i = 0; i < 2; i++) {
                mFrameWeights[i] = new Ham2FrameWeight().Read(reader);
            }

            int numHam2Nodes = reader.ReadInt32(); // expecting this to be 33

            for (int i = 0; i < 2; i++) {
                for (int j = 0; j < numHam2Nodes; j++) {
                    mNodeWeights[j, i] = new MiloLib.Classes.Vector3().Read(reader);
                    mNodeScales[j, i] = new MiloLib.Classes.Vector3().Read(reader);
                }
            }

            return this;
        }
    }
}
