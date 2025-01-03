using System.Numerics;
using MiloLib.Utils;

namespace MiloLib.Assets.Ham
{
    public class HamMove : Object
    {
        public class Language
        {
            public Symbol locale = new(0, "");
            public Symbol name = new(0, "");

            public Language Read(EndianReader reader)
            {
                locale = Symbol.Read(reader);
                name = Symbol.Read(reader);
                return this;
            }

            public void Write(EndianWriter writer)
            {
                Symbol.Write(writer, locale);
                Symbol.Write(writer, name);
            }

            public override string ToString()
            {
                return $"{locale} - {name}";
            }
        }

        public class LargeMoveFrame
        {
            public uint unk;
            public uint unk2;

            public Vector3 vec;

            public uint unk3;

            public byte unk4;

            // TODO: there is much more here
        }

        public class MoveFrame
        {
            public float position;

            public uint unk1;
            public uint unk2;
            public uint unk3;
            public uint unk4;
            public uint unk5;
        }

        private ushort altRevision;
        private ushort revision;

        // TODO: add propanim template
        // public PropAnim propAnim;

        public uint unk;

        public Symbol tex = new(0, "");

        public bool unkBool;

        public bool unkBool2;

        private uint languageCount;

        public List<Language> languages = new();

        public uint unk2;
        public uint unk3;
        public uint unk4;
        public uint unk5;

        private uint frameCount;

        public List<LargeMoveFrame> largeFrames = new();
        public List<MoveFrame> frames = new();

        public uint unk6;

        public HamMove Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            // propAnim = PropAnim.Read(reader);

            unk = reader.ReadUInt32();
            tex = Symbol.Read(reader);

            unkBool = reader.ReadBoolean();
            unkBool2 = reader.ReadBoolean();

            languageCount = reader.ReadUInt32();
            for (int i = 0; i < languageCount; i++)
            {
                languages.Add(new Language().Read(reader));
            }

            unk2 = reader.ReadUInt32();
            unk3 = reader.ReadUInt32();
            unk4 = reader.ReadUInt32();
            unk5 = reader.ReadUInt32();

            frameCount = reader.ReadUInt32();
            if (frameCount > 0 && revision >= 28)
            {
                for (int i = 0; i < frameCount; i++)
                {
                    largeFrames.Add(new LargeMoveFrame
                    {
                        unk = reader.ReadUInt32(),
                        unk2 = reader.ReadUInt32(),
                        vec = new Vector3(reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat()),
                        unk3 = reader.ReadUInt32(),
                        unk4 = reader.ReadByte()
                        // TODO: there is much more here
                    });
                }
            }
            else if (frameCount > 0)
            {
                for (int i = 0; i < frameCount; i++)
                {
                    frames.Add(new MoveFrame
                    {
                        position = reader.ReadFloat(),
                        unk1 = reader.ReadUInt32(),
                        unk2 = reader.ReadUInt32(),
                        unk3 = reader.ReadUInt32(),
                        unk4 = reader.ReadUInt32(),
                        unk5 = reader.ReadUInt32()
                    });
                }
            }

            unk6 = reader.ReadUInt32();

            return this;
        }




    }
}