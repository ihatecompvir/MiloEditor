using MiloLib.Classes;
using MiloLib.Utils;
using System;
using System.Collections.Generic;

namespace MiloLib.Assets.Synth
{
    [Name("MidiInstrument"), Description("Basic sound effect object.  Plays several samples with a given volume, pan, transpose, and envelope settings.")]
    public class MidiInstrument : Object
    {
        public class SampleZone
        {
            [Name("Sample"), Description("Which sample to play")]
            public Symbol sample = new(0, "");

            [Name("Volume"), Description("Volume in dB (0 is full volume, -96 is silence)")]
            public float volume;

            [Name("Pan"), Description("Surround pan, between -4 and 4")]
            public float pan;

            [Name("Center Note"), Description("note at which sample pays without pitch change")]
            public int centerNote = 0x24;

            [Name("Min Note"), Description("Lowest zone note")]
            public int minNote;

            [Name("Max Note"), Description("Highest zone note")]
            public int maxNote = 0x7f;

            [Name("FX Core"), Description("Which core's digital FX should be used in playing this sample")]
            public int fxCore = -1;

            [Name("ADSR")]
            public ADSR adsr = new();

            [Name("Min Velocity"), Description("Lowest zone velocity"), MinVersion(2)]
            public int minVel;

            [Name("Max Velocity"), Description("Highest zone velocity"), MinVersion(2)]
            public int maxVel = 0x7f;

            public void Read(EndianReader reader, ushort rev)
            {
                sample = Symbol.Read(reader);
                volume = reader.ReadFloat();
                pan = reader.ReadFloat();
                centerNote = reader.ReadInt32();
                minNote = reader.ReadInt32();
                maxNote = reader.ReadInt32();
                fxCore = reader.ReadInt32();
                adsr = new ADSR();
                adsr.Read(reader);
                if (rev >= 2)
                {
                    minVel = reader.ReadInt32();
                    maxVel = reader.ReadInt32();
                }
            }

            public void Write(EndianWriter writer, ushort rev)
            {
                Symbol.Write(writer, sample);
                writer.WriteFloat(volume);
                writer.WriteFloat(pan);
                writer.WriteInt32(centerNote);
                writer.WriteInt32(minNote);
                writer.WriteInt32(maxNote);
                writer.WriteInt32(fxCore);
                adsr.Write(writer);
                if (rev >= 2)
                {
                    writer.WriteInt32(minVel);
                    writer.WriteInt32(maxVel);
                }
            }

            public override string ToString()
            {
                return $"{sample}, vol: {volume}, pan: {pan}, centerNote: {centerNote}, range: {minNote}-{maxNote}";
            }
        }

        private ushort altRevision;
        private ushort revision;

        [Name("Multi Sample Maps")]
        public List<SampleZone> multiSampleMaps = new();

        [Name("Send"), Description("Effect chain to use")]
        public Symbol send = new(0, "");

        [Name("Patch Number")]
        public int patchNumber;

        [Name("Faders"), Description("Faders affecting this sound effect")]
        public FaderGroup faderGroup = new();

        [Name("Reverb Mix DB"), Description("Reverb send for this instrument"), MinVersion(3)]
        public float reverbMixDb = -96.0f;

        [Name("Reverb Enable"), Description("Enable reverb send"), MinVersion(3)]
        public bool reverbEnable;

        public MidiInstrument Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            base.Read(reader, false, parent, entry);

            uint count = reader.ReadUInt32();
            if (count > 1000)
            {
                throw new InvalidDataException("SampleZone count is too high, MidiInstrument is invalid");
            }
            for (int i = 0; i < count; i++)
            {
                SampleZone zone = new();
                zone.Read(reader, revision);
                multiSampleMaps.Add(zone);
            }

            send = Symbol.Read(reader);
            patchNumber = reader.ReadInt32();
            faderGroup = faderGroup.Read(reader);

            if (revision >= 3)
            {
                reverbMixDb = reader.ReadFloat();
                reverbEnable = reader.ReadBoolean();
            }

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw MiloLib.Exceptions.MiloAssetReadException.EndBytesNotFound(parent, entry, reader.BaseStream.Position);

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            base.Write(writer, false, parent, entry);

            writer.WriteUInt32((uint)multiSampleMaps.Count);
            foreach (var zone in multiSampleMaps)
            {
                zone.Write(writer, revision);
            }

            Symbol.Write(writer, send);
            writer.WriteInt32(patchNumber);
            faderGroup.Write(writer);

            if (revision >= 3)
            {
                writer.WriteFloat(reverbMixDb);
                writer.WriteBoolean(reverbEnable);
            }

            if (standalone)
            {
                writer.WriteEndBytes();
            }
        }
    }
}
