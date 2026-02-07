using MiloLib.Utils;
using MiloLib.Classes;

namespace MiloLib.Assets
{

    [Name("SynthSample"), Description("A single mono waveform residing in a bank.")]
    public class SynthSample : Object
    {
        public class SampleData
        {
            public class SampleMarker
            {
                public Symbol name = new(0, "");
                public int sample;

                public SampleMarker Read(EndianReader reader)
                {
                    name = Symbol.Read(reader);
                    sample = reader.ReadInt32();
                    return this;
                }

                public void Write(EndianWriter writer)
                {
                    Symbol.Write(writer, name);
                    writer.WriteInt32(sample);
                }

                public override string ToString()
                {
                    return $"{name} {sample}";
                }
            }

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
            private ushort altRevision;
            private ushort revision;

            [Name("TextureEncoding"), Description("The format of the sample data.")]
            public Encoding encoding;

            public uint sampleCount;

            [Name("Sample Rate"), Description("Sample rate, in Hz")]
            public uint sampleRate;

            private uint samplesSize;

            [Name("Should Read Samples"), Description("Whether or not the game should read the sample data. This must be checked if sample data is present.")]
            public bool readSamples;

            [Name("Samples"), Description("The raw samples of the audio file. These are not playable on their own if extracted.")]
            public List<byte> samples = new List<byte>();

            private uint markerCount;
            [Name("Markers"), MinVersion(14)]
            public List<SampleMarker> markers = new List<SampleMarker>();

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
                    markerCount = reader.ReadUInt32();
                    for (int i = 0; i < markerCount; i++)
                    {
                        markers.Add(new SampleMarker().Read(reader));
                    }
                }

                return this;
            }

            public void Write(EndianWriter writer)
            {
                writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

                writer.WriteUInt32((uint)encoding);

                writer.WriteUInt32(sampleCount);
                writer.WriteUInt32(sampleRate);
                writer.WriteUInt32((uint)samples.Count);

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
                    writer.WriteUInt32((uint)markers.Count);
                    foreach (SampleMarker marker in markers)
                    {
                        marker.Write(writer);
                    }
                }
            }
        }
        private ushort altRevision;
        private ushort revision;

        [Name("File Name"), Description("Mono, 16-bit sample file")]
        public Symbol file = new(0, "");

        [Name("Looped"), Description("Loop this sample")]
        public bool looped;

        [Name("Loop Start Sample"), Description("Start of the loop, in samples. Ignored if \"looped\" is unchecked.")]
        public uint loopStartSample;
        [Name("Loop End Sample"), Description("End of the loop, in samples.  Use -1 for the end of the sample."), MinVersion(3)]
        public int loopEndSample;

        public uint unkInt;

        public SampleData sampleData = new SampleData();

        public SynthSample Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            if (revision > 1)
                objFields = objFields.Read(reader, parent, entry);

            file = Symbol.Read(reader);

            if (revision < 6)
            {
                looped = reader.ReadBoolean();
                loopStartSample = reader.ReadUInt32();
            }

            if (revision > 2)
                loopEndSample = reader.ReadInt32();

            sampleData = sampleData.Read(reader);

            if (revision >= 6)
            {
                unkInt = reader.ReadUInt32();
            }


            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw MiloLib.Exceptions.MiloAssetReadException.EndBytesNotFound(parent, entry, reader.BaseStream.Position);

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            if (revision > 1)
                objFields.Write(writer, parent);

            Symbol.Write(writer, file);

            if (revision < 6)
            {
                writer.WriteBoolean(looped);
                writer.WriteUInt32(loopStartSample);
            }


            if (revision > 2)
                writer.WriteInt32(loopEndSample);


            sampleData.Write(writer);

            if (revision >= 6)
            {
                writer.WriteUInt32(unkInt);
            }

            if (standalone)
                writer.WriteEndBytes();
        }
    }
}
