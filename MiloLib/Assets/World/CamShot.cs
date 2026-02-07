using MiloLib.Assets.Rnd;
using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets.World
{
    [Name("CamShot"), Description("A camera shot. This is an animated camera path with keyframed settings.")]
    public class CamShot : Object
    {
        public class SubPart
        {
            [MaxVersion(42)]
            public int dummy;
            public Symbol objName = new(0, "");
            public Symbol partName = new(0, "");

            public SubPart Read(EndianReader reader, ushort camShotRev)
            {
                if (camShotRev < 0x2B) dummy = reader.ReadInt32();
                objName = Symbol.Read(reader);
                partName = Symbol.Read(reader);
                return this;
            }

            public void Write(EndianWriter writer, ushort camShotRev)
            {
                if (camShotRev < 0x2B) writer.WriteInt32(dummy);
                Symbol.Write(writer, objName);
                Symbol.Write(writer, partName);
            }
        }

        public class CamShotFrame
        {
            [Name("Duration"), Description("Duration this keyframe holds steady")]
            public float duration;

            [Name("Blend"), Description("Duration this keyframe blends into the next one")]
            public float blend;

            [Name("Blend Ease"), Description("Amount to ease into this keyframe")]
            public float blendEase;

            [Name("Blend Ease Mode"), Description("Amount to ease out to the next keyframe"), MinVersion(46)]
            public int blendEaseMode;

            [Name("Field of View"), Description("Field of view, in degrees, for this keyframe. Same as setting lens focal length below")]
            public float fov;

            [Name("World Offset"), Description("Camera position for this keyframe")]
            public Matrix worldOffset = new();

            [Name("Screen Offset"), Description("Screen space offset of target for this keyframe")]
            public Vector2 screenOffset = new();

            [Name("Blur Depth"), Description("0 to 1 scale representing the Depth size of the blur valley (offset from the focal target + focus_blur_multiplier) in the Camera Frustrum. Zero puts everything in Blur. 1 puts everything in the Blur falloff valley.")]
            public float blurDepth;

            [MaxVersion(22)]
            public int oldBlurInt;

            [Name("Max Blur"), Description("Maximum blurriness"), MinVersion(24)]
            public float maxBlur;

            [Name("Min Blur"), Description("Minimum blurriness"), MinVersion(29)]
            public float minBlur;

            [Name("Focus Blur Multiplier"), Description("Multiplier of distance from camere to focal target. Offsets focal point of blur."), MinVersion(21)]
            public float focusBlurMultiplier;

            [MaxVersion(22)]
            public int oldFocusInt;

            [Name("Targets"), Description("Target(s) that the camera should look at"), MinVersion(44)]
            public List<Symbol> targets = new();

            [MaxVersion(43)]
            public int oldTargetCount;
            [MaxVersion(43)]
            public List<SubPart> oldTargets = new();

            [Name("Focal Target"), Description("The focal point when calculated oldth of field"), MinVersion(27)]
            public Symbol focusTarget = new(0, "");
            [MinVersion(27), MaxVersion(43)]
            public SubPart oldFocusTarget = new();

            [Name("Parent"), Description("Parent that the camera should attach itself to"), MinVersion(44)]
            public Symbol parent = new(0, "");
            [MaxVersion(43)]
            public SubPart oldParent = new();

            [Name("Use Parent Rotation"), Description("Whether to take the parent object's rotation into account")]
            public bool useParentNotation;

            [Name("Shake Noise Amp"), Description("Noise amplitude for camera shake"), MinVersion(18)]
            public float shakeNoiseAmp;

            [Name("Shake Noise Freq"), Description("Noise frequency for camera shake"), MinVersion(18)]
            public float shakeNoiseFreq;

            [Name("Shake Max Angle"), Description("Maximum angle for camera shake"), MinVersion(18)]
            public Vector2 maxAngularOffset = new();

            [Name("Zoom FOV"), Description("Field of view adjustment (not affected by target reframing"), MinVersion(22)]
            public float zoomFov;

            [Name("Parent First Frame"), Description("Only parent on the first frame"), MinVersion(41)]
            public bool parentFirstFrame;

            public CamShotFrame Read(EndianReader reader, ushort camRev)
            {
                duration = reader.ReadFloat();
                blend = reader.ReadFloat();
                blendEase = reader.ReadFloat();
                if (camRev > 0x2D)
                    blendEaseMode = reader.ReadInt32();
                fov = reader.ReadFloat();
                worldOffset = new Matrix().Read(reader);
                screenOffset = new Vector2().Read(reader);
                blurDepth = reader.ReadFloat();
                if (camRev < 0x17)
                    oldBlurInt = reader.ReadInt32();
                if (camRev > 0x17)
                    maxBlur = reader.ReadFloat();
                if (camRev > 0x1C)
                    minBlur = reader.ReadFloat();
                if (camRev > 0x14)
                    focusBlurMultiplier = reader.ReadFloat();
                if (camRev < 0x17)
                    oldFocusInt = reader.ReadInt32();
                if (camRev > 0x2B)
                {
                    uint count = reader.ReadUInt32();
                    for (int i = 0; i < count; i++)
                        targets.Add(Symbol.Read(reader));
                }
                else
                {
                    oldTargetCount = reader.ReadInt32();
                    for (int i = 0; i < oldTargetCount; i++)
                        oldTargets.Add(new SubPart().Read(reader, camRev));
                }
                if (camRev > 0x1A)
                {
                    if (camRev > 0x2B)
                        focusTarget = Symbol.Read(reader);
                    else
                        oldFocusTarget = new SubPart().Read(reader, camRev);
                }
                if (camRev > 0x2B)
                    parent = Symbol.Read(reader);
                else
                    oldParent = new SubPart().Read(reader, camRev);
                useParentNotation = reader.ReadBoolean();
                if (camRev > 0x11)
                {
                    shakeNoiseAmp = reader.ReadFloat();
                    shakeNoiseFreq = reader.ReadFloat();
                    maxAngularOffset = new Vector2().Read(reader);
                }
                if (camRev > 0x15)
                    zoomFov = reader.ReadFloat();
                if (camRev > 0x28)
                    parentFirstFrame = reader.ReadBoolean();
                return this;
            }

            public void Write(EndianWriter writer, ushort camRev)
            {
                writer.WriteFloat(duration);
                writer.WriteFloat(blend);
                writer.WriteFloat(blendEase);
                if (camRev > 0x2D)
                    writer.WriteInt32(blendEaseMode);
                writer.WriteFloat(fov);
                worldOffset.Write(writer);
                screenOffset.Write(writer);
                writer.WriteFloat(blurDepth);
                if (camRev < 0x17)
                    writer.WriteInt32(oldBlurInt);
                if (camRev > 0x17)
                    writer.WriteFloat(maxBlur);
                if (camRev > 0x1C)
                    writer.WriteFloat(minBlur);
                if (camRev > 0x14)
                    writer.WriteFloat(focusBlurMultiplier);
                if (camRev < 0x17)
                    writer.WriteInt32(oldFocusInt);
                if (camRev > 0x2B)
                {
                    writer.WriteUInt32((uint)targets.Count);
                    foreach (var t in targets)
                        Symbol.Write(writer, t);
                }
                else
                {
                    writer.WriteInt32(oldTargetCount);
                    foreach (var sp in oldTargets)
                        sp.Write(writer, camRev);
                }
                if (camRev > 0x1A)
                {
                    if (camRev > 0x2B)
                        Symbol.Write(writer, focusTarget);
                    else
                        oldFocusTarget.Write(writer, camRev);
                }
                if (camRev > 0x2B)
                    Symbol.Write(writer, parent);
                else
                    oldParent.Write(writer, camRev);
                writer.WriteBoolean(useParentNotation);
                if (camRev > 0x11)
                {
                    writer.WriteFloat(shakeNoiseAmp);
                    writer.WriteFloat(shakeNoiseFreq);
                    maxAngularOffset.Write(writer);
                }
                if (camRev > 0x15)
                    writer.WriteFloat(zoomFov);
                if (camRev > 0x28)
                    writer.WriteBoolean(parentFirstFrame);
            }
        }

        public class CamShotCrowd
        {
            [Name("Crowd"), Description("The crowd to show for this shot")]
            public Symbol crowd = new(0, "");

            [Name("Crowd Rotate"), Description("How to rotate crowd")]
            public int crowdRotate;

            public List<(int, int)> pairData = new();
            public int modifyStamp;

            public CamShotCrowd Read(EndianReader reader)
            {
                crowd = Symbol.Read(reader);
                crowdRotate = reader.ReadInt32();
                uint count = reader.ReadUInt32();
                for (int i = 0; i < count; i++)
                    pairData.Add((reader.ReadInt32(), reader.ReadInt32()));
                modifyStamp = reader.ReadInt32();
                return this;
            }

            public void Write(EndianWriter writer)
            {
                Symbol.Write(writer, crowd);
                writer.WriteInt32(crowdRotate);
                writer.WriteUInt32((uint)pairData.Count);
                foreach (var (a, b) in pairData)
                {
                    writer.WriteInt32(a);
                    writer.WriteInt32(b);
                }
                writer.WriteInt32(modifyStamp);
            }
        }

        private ushort altRevision;
        private ushort revision;

        public RndAnimatable anim = new();

        [Name("Keyframes"), MinVersion(13)]
        public List<CamShotFrame> keyframes = new();

        [Name("Looping"), Description("Whether the animation should loop."), MinVersion(13)]
        public bool looping;

        [Name("Loop Keyframe"), Description("If looping true, which keyframe to loop to."), MinVersion(31)]
        public int loopKeyframe;

        [MaxVersion(39)]
        public float oldSomeFloat;

        [Name("Near Plane"), Description("Near clipping plane for the camera")]
        public float near = 1;

        [Name("Far Plane"), Description("Far clipping plane for the camera")]
        public float far = 1000;

        [Name("Use Depth of Field"), Description("Whether to use oldth-of-field effect on platforms that support it")]
        public bool useDepthOfField = true;

        [Name("Filter"), Description("Filter amount")]
        public float filter = 0.9f;

        [Name("Clamp Height"), Description("Height above target's base at which to clamp camera")]
        public float clampHeight = -1;

        // old path (rev <= 12) oldrecated fields
        [MaxVersion(12)]
        public float oldFov1;
        [MaxVersion(12)]
        public float oldFov2;
        [MaxVersion(12)]
        public Matrix oldTf1 = new();
        [MaxVersion(12)]
        public Matrix oldTf2 = new();
        [MaxVersion(12)]
        public Vector2 oldVec1 = new();
        [MaxVersion(12)]
        public Vector2 oldVec2 = new();
        [MaxVersion(12)]
        public float oldBlendDuration;
        [MinVersion(10), MaxVersion(12)]
        public float oldDof1;
        [MinVersion(10), MaxVersion(12)]
        public float oldDof2;
        [MinVersion(10), MaxVersion(12)]
        public float oldDof3;
        [MaxVersion(3)]
        public bool oldRateBool;
        [MaxVersion(12)]
        public int oldSubPartCount;
        [MaxVersion(12)]
        public List<SubPart> oldSubParts = new();
        [MaxVersion(12)]
        public SubPart oldParentSubPart = new();
        [MinVersion(11), MaxVersion(12)]
        public bool oldUseBool;

        [Name("Path"), Description("Optional camera path to use")]
        public Symbol path = new(0, "");

        [MinVersion(2), MaxVersion(44)]
        public float oldPathFloat;

        [Name("Category"), Description("Category for shot-picking"), MinVersion(3)]
        public Symbol category = new(0, "");

        [MinVersion(3), MaxVersion(37)]
        public float oldCatFloat;

        [Name("Platform Only"), Description("Limit this shot to given platform"), MinVersion(35)]
        public int platformOnly;

        [MinVersion(34), MaxVersion(34)]
        public int oldPlatformState;

        [MinVersion(5), MaxVersion(41)]
        public List<(int, int)> oldCrowdPairData = new();

        [MinVersion(8), MaxVersion(41)]
        public int oldCrowdStamp = -1;

        [Name("Hide List"), Description("List of objects to hide while this camera shot is active, shows them when done"), MinVersion(6)]
        public List<Symbol> hideList = new();

        [Name("Show List"), Description("List of objects to show while this camera shot is active, hides them when done"), MinVersion(48)]
        public List<Symbol> showList = new();

        [Name("Gen Hide List"), Description("Automatically generated list of objects to hide while this camera shot is active, shows them when done.  Not editable"), MinVersion(28)]
        public List<Symbol> unk64 = new();

        [MinVersion(12), MaxVersion(41)]
        public Symbol oldCrowdSym = new(0, "");

        [MinVersion(33), MaxVersion(41)]
        public int oldCrowdRotate;

        [MinVersion(14), MaxVersion(14)]
        public float oldFloat0E_1;
        [MinVersion(14), MaxVersion(14)]
        public float oldFloat0E_2;
        [MinVersion(14), MaxVersion(14)]
        public float oldFloat0E_3;

        [MinVersion(16), MaxVersion(17)]
        public float oldShake1;
        [MinVersion(16), MaxVersion(17)]
        public float oldShake2;

        [MinVersion(17), MaxVersion(17)]
        public Vector2 oldAngOffset = new();

        [Name("Glow Spot"), Description("The spotlight to get glow settings from"), MinVersion(20)]
        public Symbol glowSpot = new(0, "");

        [Name("Draw Overrides"), Description("List of objects to draw in order instead of whole world"), MinVersion(30)]
        public List<Symbol> drawOverrides = new();

        [Name("PostProc Overrides"), Description("List of objects to draw after post-processing"), MinVersion(32)]
        public List<Symbol> postProcOverrides = new();

        [Name("PS3 Per Pixel"), Description("global per-pixel setting for PS3"), MinVersion(36)]
        public bool ps3PerPixel = true;

        [Name("Disabled"), Description("disabled bits"), MinVersion(37)]
        public int flags;

        [MinVersion(40), MaxVersion(42)]
        public Symbol oldSym4042 = new(0, "");

        [Name("Crowds"), MinVersion(42)]
        public List<CamShotCrowd> crowds = new();

        [Name("Anims"), Description("animatables to be driven with the same frame"), MinVersion(43)]
        public List<Symbol> anims = new();

        public List<Symbol> unk74 = new();

        public new CamShot Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            if (revision != 0)
            {
                base.Read(reader, false, parent, entry);
                anim = new RndAnimatable().Read(reader, parent, entry);
            }

            if (revision > 0xC)
            {
                uint keyframeCount = reader.ReadUInt32();
                for (int i = 0; i < keyframeCount; i++)
                    keyframes.Add(new CamShotFrame().Read(reader, revision));
                looping = reader.ReadBoolean();
                if (revision > 0x1E)
                    loopKeyframe = reader.ReadInt32();
                if (revision < 0x28)
                    oldSomeFloat = reader.ReadFloat();
                near = reader.ReadFloat();
                far = reader.ReadFloat();
                useDepthOfField = reader.ReadBoolean();
                filter = reader.ReadFloat();
                clampHeight = reader.ReadFloat();
            }
            else
            {
                oldFov1 = reader.ReadFloat();
                oldFov2 = reader.ReadFloat();
                oldTf1 = new Matrix().Read(reader);
                oldTf2 = new Matrix().Read(reader);
                oldVec1 = new Vector2().Read(reader);
                oldVec2 = new Vector2().Read(reader);
                if (revision < 0x28)
                    oldSomeFloat = reader.ReadFloat();
                oldBlendDuration = reader.ReadFloat();
                near = reader.ReadFloat();
                far = reader.ReadFloat();
                useDepthOfField = reader.ReadBoolean();
                if (revision > 9)
                {
                    oldDof1 = reader.ReadFloat();
                    oldDof2 = reader.ReadFloat();
                    oldDof3 = reader.ReadFloat();
                }
                if (revision < 4)
                    oldRateBool = reader.ReadBoolean();
                filter = reader.ReadFloat();
                clampHeight = reader.ReadFloat();
                oldSubPartCount = reader.ReadInt32();
                for (int i = 0; i < oldSubPartCount; i++)
                    oldSubParts.Add(new SubPart().Read(reader, revision));
                oldParentSubPart = new SubPart().Read(reader, revision);
                if (revision > 10)
                    oldUseBool = reader.ReadBoolean();
            }

            path = Symbol.Read(reader);
            if (revision >= 2 && revision <= 44)
                oldPathFloat = reader.ReadFloat();
            if (revision > 2)
                category = Symbol.Read(reader);
            if (revision > 2 && revision < 0x26)
                oldCatFloat = reader.ReadFloat();
            if (revision > 0x22)
                platformOnly = reader.ReadInt32();
            else if (revision > 0x21)
                oldPlatformState = reader.ReadInt32();

            if (revision < 1)
                anim = new RndAnimatable().Read(reader, parent, entry);

            if (revision >= 5 && revision <= 41)
            {
                uint count = reader.ReadUInt32();
                for (int i = 0; i < count; i++)
                    oldCrowdPairData.Add((reader.ReadInt32(), reader.ReadInt32()));
            }
            if (revision >= 8 && revision <= 41)
                oldCrowdStamp = reader.ReadInt32();

            if (revision > 5)
            {
                if (revision <= 0x2F)
                {
                    uint count = reader.ReadUInt32();
                    for (int i = 0; i < count; i++)
                        hideList.Add(Symbol.Read(reader));
                }
                else
                {
                    uint hideCount = reader.ReadUInt32();
                    for (int i = 0; i < hideCount; i++)
                        hideList.Add(Symbol.Read(reader));
                    uint showCount = reader.ReadUInt32();
                    for (int i = 0; i < showCount; i++)
                        showList.Add(Symbol.Read(reader));
                }
            }

            if (revision > 0x1B)
            {
                uint count = reader.ReadUInt32();
                for (int i = 0; i < count; i++)
                    unk64.Add(Symbol.Read(reader));
            }

            if (revision > 0xB && revision < 0x2A)
                oldCrowdSym = Symbol.Read(reader);
            if (revision >= 33 && revision <= 41)
                oldCrowdRotate = reader.ReadInt32();

            if (revision == 0xE)
            {
                oldFloat0E_1 = reader.ReadFloat();
                oldFloat0E_2 = reader.ReadFloat();
                oldFloat0E_3 = reader.ReadFloat();
            }
            if (revision == 16 || revision == 17)
            {
                oldShake1 = reader.ReadFloat();
                oldShake2 = reader.ReadFloat();
            }
            if (revision > 0x10 && revision < 0x12)
                oldAngOffset = new Vector2().Read(reader);

            if (revision > 0x13)
                glowSpot = Symbol.Read(reader);

            if (revision > 0x1D)
            {
                uint count = reader.ReadUInt32();
                for (int i = 0; i < count; i++)
                    drawOverrides.Add(Symbol.Read(reader));
            }

            if (revision > 0x1F)
            {
                uint count = reader.ReadUInt32();
                for (int i = 0; i < count; i++)
                    postProcOverrides.Add(Symbol.Read(reader));
            }

            if (revision > 0x23 && !(revision >= 47 && revision <= 48))
                ps3PerPixel = reader.ReadBoolean();

            if (revision > 0x24)
                flags = reader.ReadInt32();

            if (revision >= 40 && revision <= 42)
                oldSym4042 = Symbol.Read(reader);

            if (revision >= 0x2A)
            {
                uint crowdCount = reader.ReadUInt32();
                for (int i = 0; i < crowdCount; i++)
                    crowds.Add(new CamShotCrowd().Read(reader));
            }

            if (revision > 0x2A)
            {
                uint count = reader.ReadUInt32();
                for (int i = 0; i < count; i++)
                    anims.Add(Symbol.Read(reader));
            }

            if (altRevision != 0)
            {
                uint count = reader.ReadUInt32();
                for (int i = 0; i < count; i++)
                    unk74.Add(Symbol.Read(reader));
            }

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw MiloLib.Exceptions.MiloAssetReadException.EndBytesNotFound(parent, entry, reader.BaseStream.Position);

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            if (revision != 0)
            {
                base.Write(writer, false, parent, entry);
                anim.Write(writer);
            }

            if (revision > 0xC)
            {
                writer.WriteUInt32((uint)keyframes.Count);
                foreach (var kf in keyframes)
                    kf.Write(writer, revision);
                writer.WriteBoolean(looping);
                if (revision > 0x1E)
                    writer.WriteInt32(loopKeyframe);
                if (revision < 0x28)
                    writer.WriteFloat(oldSomeFloat);
                writer.WriteFloat(near);
                writer.WriteFloat(far);
                writer.WriteBoolean(useDepthOfField);
                writer.WriteFloat(filter);
                writer.WriteFloat(clampHeight);
            }
            else
            {
                writer.WriteFloat(oldFov1);
                writer.WriteFloat(oldFov2);
                oldTf1.Write(writer);
                oldTf2.Write(writer);
                oldVec1.Write(writer);
                oldVec2.Write(writer);
                if (revision < 0x28)
                    writer.WriteFloat(oldSomeFloat);
                writer.WriteFloat(oldBlendDuration);
                writer.WriteFloat(near);
                writer.WriteFloat(far);
                writer.WriteBoolean(useDepthOfField);
                if (revision > 9)
                {
                    writer.WriteFloat(oldDof1);
                    writer.WriteFloat(oldDof2);
                    writer.WriteFloat(oldDof3);
                }
                if (revision < 4)
                    writer.WriteBoolean(oldRateBool);
                writer.WriteFloat(filter);
                writer.WriteFloat(clampHeight);
                writer.WriteInt32(oldSubPartCount);
                foreach (var sp in oldSubParts)
                    sp.Write(writer, revision);
                oldParentSubPart.Write(writer, revision);
                if (revision > 10)
                    writer.WriteBoolean(oldUseBool);
            }

            Symbol.Write(writer, path);
            if (revision >= 2 && revision <= 44)
                writer.WriteFloat(oldPathFloat);
            if (revision > 2)
                Symbol.Write(writer, category);
            if (revision > 2 && revision < 0x26)
                writer.WriteFloat(oldCatFloat);
            if (revision > 0x22)
                writer.WriteInt32(platformOnly);
            else if (revision > 0x21)
                writer.WriteInt32(oldPlatformState);

            if (revision < 1)
                anim.Write(writer);

            if (revision >= 5 && revision <= 41)
            {
                writer.WriteUInt32((uint)oldCrowdPairData.Count);
                foreach (var (a, b) in oldCrowdPairData)
                {
                    writer.WriteInt32(a);
                    writer.WriteInt32(b);
                }
            }
            if (revision >= 8 && revision <= 41)
                writer.WriteInt32(oldCrowdStamp);

            if (revision > 5)
            {
                if (revision <= 0x2F)
                {
                    writer.WriteUInt32((uint)hideList.Count);
                    foreach (var s in hideList)
                        Symbol.Write(writer, s);
                }
                else
                {
                    writer.WriteUInt32((uint)hideList.Count);
                    foreach (var s in hideList)
                        Symbol.Write(writer, s);
                    writer.WriteUInt32((uint)showList.Count);
                    foreach (var s in showList)
                        Symbol.Write(writer, s);
                }
            }

            if (revision > 0x1B)
            {
                writer.WriteUInt32((uint)unk64.Count);
                foreach (var s in unk64)
                    Symbol.Write(writer, s);
            }

            if (revision > 0xB && revision < 0x2A)
                Symbol.Write(writer, oldCrowdSym);
            if (revision >= 33 && revision <= 41)
                writer.WriteInt32(oldCrowdRotate);

            if (revision == 0xE)
            {
                writer.WriteFloat(oldFloat0E_1);
                writer.WriteFloat(oldFloat0E_2);
                writer.WriteFloat(oldFloat0E_3);
            }
            if (revision == 16 || revision == 17)
            {
                writer.WriteFloat(oldShake1);
                writer.WriteFloat(oldShake2);
            }
            if (revision > 0x10 && revision < 0x12)
                oldAngOffset.Write(writer);

            if (revision > 0x13)
                Symbol.Write(writer, glowSpot);

            if (revision > 0x1D)
            {
                writer.WriteUInt32((uint)drawOverrides.Count);
                foreach (var s in drawOverrides)
                    Symbol.Write(writer, s);
            }

            if (revision > 0x1F)
            {
                writer.WriteUInt32((uint)postProcOverrides.Count);
                foreach (var s in postProcOverrides)
                    Symbol.Write(writer, s);
            }

            if (revision > 0x23 && !(revision >= 47 && revision <= 48))
                writer.WriteBoolean(ps3PerPixel);

            if (revision > 0x24)
                writer.WriteInt32(flags);

            if (revision >= 40 && revision <= 42)
                Symbol.Write(writer, oldSym4042);

            if (revision >= 0x2A)
            {
                writer.WriteUInt32((uint)crowds.Count);
                foreach (var c in crowds)
                    c.Write(writer);
            }

            if (revision > 0x2A)
            {
                writer.WriteUInt32((uint)anims.Count);
                foreach (var s in anims)
                    Symbol.Write(writer, s);
            }

            if (altRevision != 0)
            {
                writer.WriteUInt32((uint)unk74.Count);
                foreach (var s in unk74)
                    Symbol.Write(writer, s);
            }

            if (standalone)
            {
                writer.WriteEndBytes();
            }
        }
    }
}
