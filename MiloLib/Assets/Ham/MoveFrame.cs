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

        public bool IsDefault() {
            return unk0 == false && unk4 == -1.0f && unk8 == 1e+30f && unkc == -1.0f && unk10 == 1e+30f;
        }

        public override string ToString() {
            return $"Ham1NodeWeight: bool {unk0} perfectDist1 {unk4} rate1 {unk8} perfectDist2 {unkc} rate2 {unk10}\n";
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

        public override string ToString() {
            return $"Ham2FrameWeight: unk0 {unk0}; unk4 ( {unk4[0]},{unk4[1]},{unk4[2]},{unk4[3]} ); unk14 ( {unk14[0]},{unk14[1]},{unk14[2]},{unk14[3]} )\n";
        }
    }

    public enum NodeCounts {
        kNumHam1Nodes = 16,
        kMaxNumErrorNodes = 33
    }

    public class MoveFrame {
        public float beat; // beat?
        public int unk4; // flags for ErrorNodeType (0x800000 = position, 0x400000 = displacement)
        public int numDC1Nodes;
        public int numHam2Nodes;
        // TODO: this 64 is actually indexed as a 2D array of some sort (16 by 4). 16 = num ham1 error nodes, but what's the 4 represent?
        Ham1NodeWeight[] mHam1NodeWeights = new Ham1NodeWeight[64];
        Classes.Vector3[,] mNodeWeights = new Classes.Vector3[33, 2];
        Classes.Vector3[,] mNodeScales = new Classes.Vector3[33, 2];
        Classes.Vector3[,] mNodesInverseScale = new Classes.Vector3[33, 2];
        Ham2FrameWeight[] mFrameWeights = new Ham2FrameWeight[2];

        public override string ToString() {
            string str = $"MoveFrame:\n";
            str += $"\tBeat: {beat.ToString("0.00")}";
            str += $"\tFlags: 0x{unk4.ToString("X")}";
            str += $"\tDC1 error nodes: {numDC1Nodes}; DC2/3: {numHam2Nodes}\n";
            str += PrintHam1NodeWeights();
            str += PrintHam2NodeWeights();
            str += PrintHam2NodeScales();
            str += $"\tUnmirrored {mFrameWeights[0]}";
            str += $"\tMirrored {mFrameWeights[1]}";
            return str;
        }

        private string PrintHam1NodeWeights() {
            string str = "\tHam1NodeWeights:";
            bool allDefaults = true;
            for(int i = 0; i < 64; i++) {
                if (!mHam1NodeWeights[i].IsDefault()) {
                    allDefaults = false;
                    break;
                }
            }
            if (allDefaults) str += $" ALL DEFAULTS\n";
            else {
                str += " Non-defaults:\n";
                for (int i = 0; i < 64; i++) {
                    if (!mHam1NodeWeights[i].IsDefault())
                        str += $"\t\t{i}: {mHam1NodeWeights[i]}";
                }
            }
            return str;
        }

        private string PrintHam2NodeWeights() {
            string str = "";
            str += $"\tUnmirrored mNodeWeights: ";
            bool allzeroes = true;

            for (int i = 0; i < 33; i++) {
                if (!mNodeWeights[i, 0].IsZero()) {
                    allzeroes = false;
                    break;
                }
            }
            if (allzeroes) str += "ALL ZEROES\n";
            else {
                str += "Non-zeroes:\n";
                for(int i = 0; i < 33; i++) {
                    if (!mNodeWeights[i, 0].IsZero()) {
                        str += $"\t\t{i}: {mNodeWeights[i, 0]}\n";
                    }
                }
            }

            str += $"\tMirrored mNodeWeights: ";
            allzeroes = true;
            for (int i = 0; i < 33; i++) {
                if (!mNodeWeights[i, 1].IsZero()) {
                    allzeroes = false;
                    break;
                }
            }
            if (allzeroes) str += "ALL ZEROES\n";
            else {
                str += "Non-zeroes:\n";
                for (int i = 0; i < 33; i++) {
                    if (!mNodeWeights[i, 1].IsZero()) {
                        str += $"\t\t{i}: {mNodeWeights[i, 1]}\n";
                    }
                }
            }
            return str;
        }
        private string PrintHam2NodeScales() {
            string str = "";
            str += $"\tUnmirrored mNodeScales: ";
            bool allzeroes = true;

            for (int i = 0; i < 33; i++) {
                if (!mNodeScales[i, 0].IsZero()) {
                    allzeroes = false;
                    break;
                }
            }
            if (allzeroes) str += "ALL ZEROES\n";
            else {
                str += "Non-zeroes:\n";
                for (int i = 0; i < 33; i++) {
                    if (!mNodeScales[i, 0].IsZero()) {
                        str += $"\t\t{i}: {mNodeScales[i, 0]}\n";
                    }
                }
            }

            str += $"\tMirrored mNodeScales: ";
            allzeroes = true;
            for (int i = 0; i < 33; i++) {
                if (!mNodeScales[i, 1].IsZero()) {
                    allzeroes = false;
                    break;
                }
            }
            if (allzeroes) str += "ALL ZEROES\n";
            else {
                str += "Non-zeroes:\n";
                for (int i = 0; i < 33; i++) {
                    if (!mNodeScales[i, 1].IsZero()) {
                        str += $"\t\t{i}: {mNodeScales[i, 1]}\n";
                    }
                }
            }
            return str;
        }

        public MoveFrame Read(EndianReader reader) {
            beat = reader.ReadFloat();
            unk4 = reader.ReadInt32();
            numDC1Nodes = reader.ReadInt32(); // expecting this to be 16

            for (int i = 0; i < 64; i++) {
                mHam1NodeWeights[i] = new Ham1NodeWeight().Read(reader);
            }

            for (int i = 0; i < 2; i++) {
                mFrameWeights[i] = new Ham2FrameWeight().Read(reader);
            }

            numHam2Nodes = reader.ReadInt32(); // expecting this to be 33

            for (int i = 0; i < 2; i++) { // unmirrored vs mirrored
                for (int j = 0; j < numHam2Nodes; j++) {
                    mNodeWeights[j, i] = new MiloLib.Classes.Vector3().Read(reader);
                    mNodeScales[j, i] = new MiloLib.Classes.Vector3().Read(reader);
                }
            }

            return this;
        }
    }
}
