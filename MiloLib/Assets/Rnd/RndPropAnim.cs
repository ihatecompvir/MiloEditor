using MiloLib.Classes;
using MiloLib.Utils;
using System.Reflection.PortableExecutable;

namespace MiloLib.Assets.Rnd
{
    [Name("RndPropAnim"), Description("Animate any properties on target object")]
    public class RndPropAnim : Object
    {
        public class PropKey
        {
            public interface IAnimEvent
            {
                float Pos { get; set; }
            }

            public struct AnimEventFloat : IAnimEvent
            {
                public float Value { get; set; }
                public float Pos { get; set; }
            }

            public struct AnimEventColor : IAnimEvent
            {
                public HmxColor4 Value { get; set; }
                public float Pos { get; set; }
            }

            public struct AnimEventObject : IAnimEvent
            {
                public Symbol Text1 { get; set; }
                public Symbol Text2 { get; set; }
                public float Pos { get; set; }
            }

            public struct AnimEventBool : IAnimEvent
            {
                public bool Value { get; set; }
                public float Pos { get; set; }
            }

            public struct AnimEventQuat : IAnimEvent
            {
                public MiloLib.Classes.Vector4 Value { get; set; }
                public float Pos { get; set; }
            }

            public struct AnimEventVector3 : IAnimEvent
            {
                public MiloLib.Classes.Vector3 Value { get; set; }
                public float Pos { get; set; }
            }

            public struct AnimEventSymbol : IAnimEvent
            {
                public Symbol Text { get; set; }
                public float Pos { get; set; }
            }

            public enum Interpolation : int
            {
                kStep,
                kLinear,
                kSpline,
                kSlerp,
                kHermite,
                kEaseIn,
                kEaseOut
            }

            public enum PropType : int
            {
                kPropFloat,
                kPropColor,
                kPropObject,
                kPropBool,
                kPropQuat,
                kPropVector3,
                kPropSymbol
            }

            public enum ExceptionID : int
            {
                kNoException,
                kTransQuat,
                kTransScale,
                kTransPos,
                kDirEvent,
                kHandleInterp,
                kMacro
            }

            public PropType type1;
            public PropType type2;

            public Symbol target = new(0, "");

            public ObjectFields.DTBParent dtb = new();

            public Interpolation interpolation;
            public Symbol interpHandler = new(0, "");

            public ExceptionID exceptionType;

            public bool unkBool;

            private uint keysCount;
            public List<IAnimEvent> keys = new();

            public PropKey Read(EndianReader reader, uint revision)
            {
                type1 = (PropType)reader.ReadInt32();
                type2 = (PropType)reader.ReadInt32();

                target = Symbol.Read(reader);

                dtb.Read(reader);

                interpolation = (Interpolation)reader.ReadInt32();
                interpHandler = Symbol.Read(reader);

                exceptionType = (ExceptionID)reader.ReadInt32();

                if (revision >= 13)
                    unkBool = reader.ReadBoolean();

                keysCount = reader.ReadUInt32();
                keys.Clear();
                switch (type1)
                {
                    case PropType.kPropFloat:
                        for (int i = 0; i < keysCount; i++)
                        {
                            keys.Add(new AnimEventFloat { Value = reader.ReadFloat(), Pos = reader.ReadFloat() });
                        }
                        break;
                    case PropType.kPropColor:
                        for (int i = 0; i < keysCount; i++)
                        {
                            keys.Add(new AnimEventColor { Value = new HmxColor4().Read(reader), Pos = reader.ReadFloat() });
                        }
                        break;
                    case PropType.kPropObject:
                        for (int i = 0; i < keysCount; i++)
                        {
                            keys.Add(new AnimEventObject { Text1 = Symbol.Read(reader), Text2 = Symbol.Read(reader), Pos = reader.ReadFloat() });
                        }
                        break;
                    case PropType.kPropBool:
                        for (int i = 0; i < keysCount; i++)
                        {
                            keys.Add(new AnimEventBool { Value = reader.ReadBoolean(), Pos = reader.ReadFloat() });
                        }
                        break;
                    case PropType.kPropQuat:
                        for (int i = 0; i < keysCount; i++)
                        {
                            keys.Add(new AnimEventQuat { Value = new MiloLib.Classes.Vector4().Read(reader), Pos = reader.ReadFloat() });
                        }
                        break;
                    case PropType.kPropVector3:
                        for (int i = 0; i < keysCount; i++)
                        {
                            keys.Add(new AnimEventVector3 { Value = new MiloLib.Classes.Vector3().Read(reader), Pos = reader.ReadFloat() });
                        }
                        break;
                    case PropType.kPropSymbol:
                        for (int i = 0; i < keysCount; i++)
                        {
                            keys.Add(new AnimEventSymbol { Text = Symbol.Read(reader), Pos = reader.ReadFloat() });
                        }
                        break;
                }


                return this;
            }

            public void Write(EndianWriter writer, uint revision)
            {
                writer.WriteInt32((int)type1);
                writer.WriteInt32((int)type2);
                Symbol.Write(writer, target);
                dtb.Write(writer);
                writer.WriteInt32((int)interpolation);
                Symbol.Write(writer, interpHandler);
                writer.WriteInt32((int)exceptionType);
                if (revision >= 13)
                    writer.WriteBoolean(unkBool);
                writer.WriteUInt32(keysCount);
                switch (type1)
                {
                    case PropType.kPropFloat:
                        foreach (AnimEventFloat key in keys)
                        {
                            writer.WriteFloat(key.Value);
                            writer.WriteFloat(key.Pos);
                        }
                        break;
                    case PropType.kPropColor:
                        foreach (AnimEventColor key in keys)
                        {
                            key.Value.Write(writer);
                            writer.WriteFloat(key.Pos);
                        }
                        break;
                    case PropType.kPropObject:
                        foreach (AnimEventObject key in keys)
                        {
                            Symbol.Write(writer, key.Text1);
                            Symbol.Write(writer, key.Text2);
                            writer.WriteFloat(key.Pos);
                        }
                        break;
                    case PropType.kPropBool:
                        foreach (AnimEventBool key in keys)
                        {
                            writer.WriteBoolean(key.Value);
                            writer.WriteFloat(key.Pos);
                        }
                        break;
                    case PropType.kPropQuat:
                        foreach (AnimEventQuat key in keys)
                        {
                            key.Value.Write(writer);
                            writer.WriteFloat(key.Pos);
                        }
                        break;
                    case PropType.kPropVector3:
                        foreach (AnimEventVector3 key in keys)
                        {
                            key.Value.Write(writer);
                            writer.WriteFloat(key.Pos);
                        }
                        break;
                    case PropType.kPropSymbol:
                        foreach (AnimEventSymbol key in keys)
                        {
                            Symbol.Write(writer, key.Text);
                            writer.WriteFloat(key.Pos);
                        }
                        break;
                }
            }

            public override string ToString()
            {
                if (interpHandler.value == "")
                    return $"target: {target} type: {type1} interp: {interpolation} exceptionType: {exceptionType} numKeys: {keysCount}";
                else
                    return $"target: {target} type: {type1} interp: {interpolation} interpHandler: {interpHandler} exceptionType: {exceptionType} numKeys: {keysCount}";
            }


        }
        private ushort altRevision;
        private ushort revision;

        public RndAnimatable anim = new();

        public bool unkBool;
        public bool unkBool2;

        [Name("Loop"), Description("Do I self loop on SetFrame?"), MinVersion(12)]
        public bool mLoop;

        private uint propKeysCount;
        public List<PropKey> propKeys = new();

        [Name("Flow Labels"), Description("the names of possible flow labels you can place on this timeline (i.e. 'footstep')"), MinVersion(14)]
        private uint numFlowLabels;
        public List<Symbol> flowLabels = new();

        [Name("Intensity"), Description("Scales all animation keyframe values by this #"), MinVersion(15)]
        public float mIntensity;

        public RndPropAnim Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            base.Read(reader, false, parent, entry);

            anim = anim.Read(reader, parent, entry);

            // TODO: if revision < 7, there's some nasty involved keys loading routine
            // else, do the following:

            propKeysCount = reader.ReadUInt32();

            for (int i = 0; i < propKeysCount; i++)
            {
                PropKey propKey = new();
                propKey.Read(reader, revision);
                propKeys.Add(propKey);
            }

            if (revision > 11)
                mLoop = reader.ReadBoolean();

            // if rev > 13, set flowlabels, a list<String>s
            if (revision > 13) {
                numFlowLabels = reader.ReadUInt32();
                for(int i = 0; i < numFlowLabels; i++) {
                    Symbol sym = Symbol.Read(reader);
                    flowLabels.Add(sym);
                }
            }

            // if rev > 14, load float mIntensity
            if (revision > 14)
                mIntensity = reader.ReadFloat();

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw new Exception("Got to end of standalone asset but didn't find the expected end bytes, read likely did not succeed");

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            base.Write(writer, false, parent, entry);

            anim.Write(writer);

            writer.WriteUInt32((uint)propKeys.Count);

            foreach (PropKey propKey in propKeys)
            {
                propKey.Write(writer, revision);
            }

            if (revision > 11)
                writer.WriteBoolean(mLoop);

            writer.WriteUInt32((uint)flowLabels.Count);
            foreach(Symbol flowLabel in flowLabels) {
                Symbol.Write(writer, flowLabel);
            }

            if(revision > 14)
                writer.WriteFloat(mIntensity);

            if (standalone)
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
        }
    }
}
