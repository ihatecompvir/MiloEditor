using System.Diagnostics;
using System.Numerics;
using MiloLib.Assets.Rnd;
using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets.Ham {
    public enum HamDifficulty {
        kDifficultyEasy,
        kDifficultyMedium,
        kDifficultyExpert,
        kDifficultyBeginner
    };

    public enum MoveRating {
        kMoveRatingSuperPerfect = 0,
        kMoveRatingPerfect = 1,
        kMoveRatingAwesome = 2,
        kMoveRatingOk = 3,
        kNumMoveRatings = 4
    };

    public enum SkeletonJoint {
        kJointHipCenter = 0,
        kJointSpine = 1,
        kJointShoulderCenter = 2,
        kJointHead = 3,
        kJointShoulderLeft = 4,
        kJointElbowLeft = 5,
        kJointWristLeft = 6,
        kJointHandLeft = 7,
        kJointShoulderRight = 8,
        kJointElbowRight = 9,
        kJointWristRight = 10,
        kJointHandRight = 11,
        kJointHipLeft = 12,
        kJointKneeLeft = 13,
        kJointAnkleLeft = 14,
        kJointHipRight = 15,
        kJointKneeRight = 16,
        kJointAnkleRight = 17,
        kJointFootLeft = 18,
        kJointFootRight = 19,
        kNumJoints = 20
    }

    public enum SkeletonCoordSys {
        kCoordCamera = 0,
        kCoordLeftArm = 1,
        kCoordRightArm = 2,
        kCoordLeftLeg = 3,
        kCoordRightLeg = 4
    };

    public enum SkeletonBone {
        kBoneHead = 0,
        kBoneCollarRight = 1,
        kBoneArmUpperRight = 2,
        kBoneArmLowerRight = 3,
        kBoneHandRight = 4,
        kBoneCollarLeft = 5,
        kBoneArmUpperLeft = 6,
        kBoneArmLowerLeft = 7,
        kBoneHandLeft = 8,
        kBoneLegUpperRight = 9,
        kBoneLegLowerRight = 10,
        kBoneLegUpperLeft = 11,
        kBoneLegLowerLeft = 12,
        kBoneBackUpper = 13,
        kBoneBackLower = 14,
        kBoneHipRight = 15,
        kBoneHipLeft = 16,
        kBoneFootLeft = 17,
        kBoneFootRight = 18,
        kNumBones = 19
    };

    public enum MoveMirrored {
        kMirroredNo = 0,
        kMirroredYes = 1,
        kNumMoveMirrored = 2
    };
}
