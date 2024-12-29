using MiloLib.Classes;
using MiloLib.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace MiloLib.Assets
{
    [Name("Sfx"), Description("Basic sound effect object.  Plays several samples with a given volume, pan, transpose, and envelope settings.")]
    public class Sfx : Object
    {
        public ushort altRevision;
        public ushort revision;

        [MinVersion(4), Name("Send"), Description("Effect chain to use")]
        public Symbol sendObj;

        public FaderGroup faderGroup = new();

        private uint moggClipCount;
        [MinVersion(9), Name("Mogg Clips"), Description("List of mogg clips to play")]
        public List<Symbol> MoggClips;

        [MinVersion(12), Name("Reverb Mix DB"), Description("Reverb send for this sfx")]
        public float reverbMixDb;

        [MinVersion(12), Name("Reverb Enable"), Description("Enable reverb send")]
        public bool reverbSendEnable;

        private uint sfxMapsCount;
        public List<SfxMap> sfxMaps;

        public Sequence sequence = new();

        public Sfx()
        {
            sfxMaps = new List<SfxMap>();
            MoggClips = new List<Symbol>();
        }

        public Sfx Read(EndianReader reader, bool standalone)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            sequence = sequence.Read(reader);

            sfxMapsCount = reader.ReadUInt32();

            // sanity check on sfxMaps count
            if (sfxMapsCount > 100)
            {
                throw new InvalidDataException("SfxMap count is too high, Sfx is invalid");
            }

            for (int i = 0; i < sfxMapsCount; i++)
            {
                SfxMap map = new SfxMap();
                map.Read(reader, revision);
                sfxMaps.Add(map);
            }

            moggClipCount = reader.ReadUInt32();

            // sanity check on moggClip count
            if (moggClipCount > 100)
            {
                throw new InvalidDataException("MoggClip count is too high, Sfx is invalid");
            }

            for (int i = 0; i < moggClipCount; i++)
            {
                MoggClips.Add(Symbol.Read(reader));
            }

            sendObj = Symbol.Read(reader);
            faderGroup = faderGroup.Read(reader);

            reverbMixDb = reader.ReadFloat();
            reverbSendEnable = reader.ReadBoolean();

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw new Exception("Got to end of standalone asset but didn't find the expected end bytes, read likely did not succeed");

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));
            base.Write(writer, standalone);

            sequence.Write(writer, standalone);

            writer.WriteInt32(sfxMaps.Count);
            foreach (var map in sfxMaps)
            {
                map.Write(writer, revision);
            }

            var fields = this.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (var field in fields)
            {
                var minVersionAttr = field.GetCustomAttribute<MinVersionAttribute>();
                if (minVersionAttr == null || revision >= minVersionAttr.Version)
                {
                    if (field.FieldType == typeof(Symbol) && field.Name == nameof(sendObj))
                        Symbol.Write(writer, (Symbol)field.GetValue(this));
                    else if (field.FieldType == typeof(FaderGroup) && field.Name == nameof(FaderGroup))
                        ((FaderGroup)field.GetValue(this))?.Write(writer);
                    else if (field.FieldType == typeof(List<Symbol>) && field.Name == nameof(MoggClips))
                    {
                        var clips = (List<Symbol>)field.GetValue(this);
                        writer.WriteInt32(clips?.Count ?? 0);
                        foreach (var clip in clips)
                            Symbol.Write(writer, clip);
                    }
                    else if (field.FieldType == typeof(float) && field.Name == nameof(reverbMixDb))
                        writer.WriteFloat((float)field.GetValue(this));
                    else if (field.FieldType == typeof(bool) && field.Name == nameof(reverbSendEnable))
                        writer.WriteByte((byte)field.GetValue(this));
                }
            }
        }
    }

    public class Sequence : Object
    {
        public ushort altRevision;
        public ushort revision;
        public float avgVol;
        public float volSpread;
        public float avgTranspose;
        public float transposeSpread;
        public float avgPan;
        public float panSpread;
        public bool canStop;

        public Sequence Read(EndianReader reader)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            if (2 < revision)
                base.Read(reader, false);

            avgVol = reader.ReadFloat();
            volSpread = reader.ReadFloat();
            avgTranspose = reader.ReadFloat();
            transposeSpread = reader.ReadFloat();
            avgPan = reader.ReadFloat();
            panSpread = reader.ReadFloat();

            if (1 < revision)
                canStop = reader.ReadBoolean();

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            if (2 < revision)
                base.Write(writer, standalone);

            writer.WriteFloat(avgVol);
            writer.WriteFloat(volSpread);
            writer.WriteFloat(avgTranspose);
            writer.WriteFloat(transposeSpread);
            writer.WriteFloat(avgPan);
            writer.WriteFloat(panSpread);

            if (1 < revision)
                writer.WriteBoolean(canStop);

            if (standalone)
            {
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
            }
        }
    }
    public class SfxMap
    {
        [Name("Sample Name"), Description("Which sample to play")]
        public Symbol sampleName = new(0, "");
        [Name("Volume"), Description("Volume in dB (0 is full volume, -96 is silence)")]
        public float volume { get; set; }
        [Name("Pan"), Description("Surround pan, between -4 and 4")]
        public float pan { get; set; }
        [Name("Transpose"), Description("Transpose in half steps")]
        public float transpose { get; set; }
        [Name("FX Core"), Description("Which core's digital FX should be used in playing this sample")]
        public uint fxCore { get; set; }
        [Name("ADSR"), Description("Envelope settings")]
        public ADSR ADSR = new ADSR();

        public void Read(EndianReader reader, uint version)
        {
            sampleName = Symbol.Read(reader);

            volume = reader.ReadFloat();
            pan = reader.ReadFloat();
            transpose = reader.ReadFloat();
            fxCore = reader.ReadUInt32();
            ADSR = new ADSR();
            ADSR.Read(reader);
        }

        public void Write(EndianWriter writer, uint version)
        {
            Symbol.Write(writer, sampleName);
            writer.WriteFloat(volume);
            writer.WriteFloat(pan);
            writer.WriteFloat(transpose);
            writer.WriteUInt32(fxCore);

            ADSR.Write(writer);
        }
    }
}

public class FaderGroup
{
    public ushort altRevision;
    public ushort revision;
    private int fadersCount;
    [Name("Faders"), Description("Faders affecting this sound effect")]
    public List<Symbol> faders = new List<Symbol>();

    public FaderGroup Read(EndianReader reader)
    {
        altRevision = reader.ReadUInt16();
        revision = reader.ReadUInt16();
        fadersCount = reader.ReadInt32();

        // sanity check on faders count
        if (fadersCount > 100)
        {
            throw new InvalidDataException("Fader count is too high, FaderGroup is invalid");
        }

        faders = new List<Symbol>();
        for (int i = 0; i < fadersCount; i++)
            faders.Add(Symbol.Read(reader));

        return this;
    }

    public void Write(EndianWriter writer)
    {

        writer.WriteUInt16(altRevision);
        writer.WriteUInt16(revision);
        writer.WriteInt32(faders.Count);
        foreach (var fader in faders)
            Symbol.Write(writer, fader);
    }
}

public class ADSR
{
    public ushort altRevision;
    public ushort revision;
    [Name("Sustain Level"), Description("Level of sustain volume (0-1)")]
    public float sustainLevel;
    [Name("Release Rate"), Description("Duration of release in seconds")]
    public float releaseRate;
    [Name("Sustain Rate"), Description("Duration of sustain in seconds")]
    public float sustainRate;
    [Name("Decay Rate"), Description("Duration of decay in seconds")]
    public float decayRate;
    [Name("Attack Rate"), Description("Duration of attack in seconds")]
    public float attackRate;
    [Name("Release Mode"), Description("Release mode")]
    public uint releaseMode;
    [Name("Sustain Mode"), Description("Sustain mode")]
    public uint sustainMode;
    [Name("Attack Mode"), Description("Attack mode")]
    public uint attackMode;

    public void Read(EndianReader reader)
    {
        altRevision = reader.ReadUInt16();
        revision = reader.ReadUInt16();

        if (revision != 1)
        {
            throw new UnsupportedAssetRevisionException("Sfx::ADSR", revision);
        }

        sustainLevel = reader.ReadFloat();
        releaseRate = reader.ReadFloat();
        sustainRate = reader.ReadFloat();
        decayRate = reader.ReadFloat();
        attackRate = reader.ReadFloat();
        releaseMode = reader.ReadUInt32();
        sustainMode = reader.ReadUInt32();
        attackMode = reader.ReadUInt32();

    }

    public void Write(EndianWriter writer)
    {
        writer.WriteUInt16(altRevision);
        writer.WriteUInt16(revision);
        writer.WriteFloat(sustainLevel);
        writer.WriteFloat(releaseRate);
        writer.WriteFloat(sustainRate);
        writer.WriteFloat(decayRate);
        writer.WriteFloat(attackRate);
        writer.WriteUInt32(releaseMode);
        writer.WriteUInt32(sustainMode);
        writer.WriteUInt32(attackMode);
    }
}

