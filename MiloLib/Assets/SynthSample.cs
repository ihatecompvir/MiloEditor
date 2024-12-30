using MiloLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiloLib.Assets
{
    public class SynthSample : Object
    {
        public class SampleData
        {
            public enum Encoding
            {
                kPCM,
                kBigEndPCM,
                kVAG,
                kXMA,
                kATRAC,
                kMP3,
                kNintendoADPCM
            }
            public ushort altRevision;
            public ushort revision;

            public Encoding encoding;

            public uint sampleCount;
            public uint sampleRate;
            public uint samplesSize;

            public bool readSamples;

            public List<byte> samples = new List<byte>();

            public uint unkInt;

            public SampleData(ushort revision, ushort altRevision = 0)
            {
                this.revision = revision;
                this.altRevision = altRevision;
                return;
            }

            public SampleData Read(EndianReader reader)
            {
                uint combinedRevision = reader.ReadUInt32();
                if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
                else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

                encoding = (Encoding)reader.ReadUInt32();

                sampleCount = reader.ReadUInt32();
                sampleRate = reader.ReadUInt32();
                samplesSize = reader.ReadUInt32();

                readSamples = reader.ReadBoolean();

                if (readSamples)
                {
                    for (int i = 0; i < samplesSize; i++)
                    {
                        samples.Add(reader.ReadByte());
                    }
                }

                if (revision >= 14)
                {
                    unkInt = reader.ReadUInt32();
                }

                return this;
            }

            public void Write(EndianWriter writer)
            {
                writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

                writer.WriteUInt32((uint)encoding);

                writer.WriteUInt32(sampleCount);
                writer.WriteUInt32(sampleRate);
                writer.WriteUInt32(samplesSize);

                writer.WriteBoolean(readSamples);

                if (readSamples)
                {
                    foreach (byte sample in samples)
                    {
                        writer.WriteByte(sample);
                    }
                }

                if (revision >= 14)
                {
                    writer.WriteUInt32(unkInt);
                }
            }
        }
        public ushort altRevision;
        public ushort revision;

        public Symbol file = new(0, "");
        public bool looped;

        public uint loopStartSample;
        public uint loopEndSample;

        public SampleData sampleData = new SampleData(0);

        public SynthSample(ushort revision, ushort altRevision = 0)
        {
            this.revision = revision;
            this.altRevision = altRevision;
            return;
        }

        public SynthSample Read(EndianReader reader, bool standalone)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            if (revision > 1)
                objFields = objFields.Read(reader);

            file = Symbol.Read(reader);
            looped = reader.ReadBoolean();

            loopStartSample = reader.ReadUInt32();
            if (revision > 2)
                loopEndSample = reader.ReadUInt32();

            sampleData = sampleData.Read(reader);

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw new Exception("Got to end of standalone asset but didn't find the expected end bytes, read likely did not succeed");

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            if (revision > 1)
                objFields.Write(writer);

            Symbol.Write(writer, file);
            writer.WriteBoolean(looped);

            writer.WriteUInt32(loopStartSample);
            if (revision > 2)
                writer.WriteUInt32(loopEndSample);

            if (standalone)
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });

            sampleData.Write(writer);
        }
    }
}
