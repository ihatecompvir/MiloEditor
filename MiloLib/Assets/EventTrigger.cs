using MiloLib.Assets.Rnd;
using MiloLib.Classes;
using MiloLib.Utils;

namespace MiloLib.Assets
{
    [Name("EventTrigger"), Description("Triggers animations, sfx, and responses to game events")]
    public class EventTrigger : Object
    {
        public class Anim
        {
            public Symbol anim = new(0, "");
            public float blend;
            public float delay;
            public bool wait;
            public bool enable;
            public byte rate;
            public float start;
            public float end;
            public float period;
            public float scale;
            public float unknown;
            public Symbol type = new(0, "");

            public Anim Read(EndianReader reader)
            {
                anim = Symbol.Read(reader);
                blend = reader.ReadFloat();
                wait = reader.ReadBoolean();
                delay = reader.ReadFloat();
                enable = reader.ReadBoolean();
                scale = reader.ReadFloat();
                start = reader.ReadFloat();
                end = reader.ReadFloat();
                period = reader.ReadFloat();
                type = Symbol.Read(reader);
                unknown = reader.ReadFloat();
                return this;
            }

            public void Write(EndianWriter writer)
            {
                Symbol.Write(writer, anim);
                writer.WriteFloat(blend);
                writer.WriteBoolean(wait);
                writer.WriteFloat(delay);
                writer.WriteBoolean(enable);
                writer.WriteFloat(scale);
                writer.WriteFloat(start);
                writer.WriteFloat(end);
                writer.WriteFloat(period);
                Symbol.Write(writer, type);
                writer.WriteFloat(unknown);
            }
        }

        public class ProxyCall
        {
            public Symbol proxy = new(0, "");
            public Symbol call = new(0, "");
            public Symbol evt = new(0, "");

            public ProxyCall Read(EndianReader reader)
            {
                proxy = Symbol.Read(reader);
                call = Symbol.Read(reader);
                evt = Symbol.Read(reader);
                return this;
            }

            public void Write(EndianWriter writer)
            {
                Symbol.Write(writer, proxy);
                Symbol.Write(writer, call);
                Symbol.Write(writer, evt);
            }
        }

        public class HideDelay
        {
            public Symbol hide = new(0, "");
            public float delay;
            public int rate;

            public HideDelay Read(EndianReader reader)
            {
                hide = Symbol.Read(reader);
                delay = reader.ReadFloat();
                rate = reader.ReadInt32();
                return this;
            }

            public void Write(EndianWriter writer)
            {
                Symbol.Write(writer, hide);
                writer.WriteFloat(delay);
                writer.WriteInt32(rate);
            }
        }

        private ushort altRevision;
        private ushort revision;

        public RndAnimatable anim = new();

        private uint triggerEventCount;
        public List<Symbol> triggerEvents = new();

        private Symbol unkSym1 = new(0, "");

        private uint animCount;
        public List<Anim> anims = new();

        private uint soundsCount;
        public List<Symbol> sounds = new();

        private uint showsCount;
        public List<Symbol> shows = new();

        private uint hideDelaysCount;
        public List<HideDelay> hideDelays = new();

        private uint enableEventsCount;
        public List<Symbol> enableEvents = new();

        private uint disableEventsCount;
        public List<Symbol> disableEvents = new();

        private uint waitForEventsCount;
        public List<Symbol> waitForEvents = new();

        public Symbol nextLink = new(0, "");

        private uint proxyCallsCount;
        public List<ProxyCall> proxyCalls = new();


        public uint triggerOrder;

        private uint resetTriggersCount;
        public List<Symbol> resetTriggers = new();

        public bool enabledAtStart;

        public uint animTrigger;
        public float animFrame;

        private uint partLauncherCount;
        public List<Symbol> partLaunchers = new();


        public EventTrigger Read(EndianReader reader, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry entry)
        {
            uint combinedRevision = reader.ReadUInt32();
            if (BitConverter.IsLittleEndian) (revision, altRevision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));
            else (altRevision, revision) = ((ushort)(combinedRevision & 0xFFFF), (ushort)((combinedRevision >> 16) & 0xFFFF));

            base.Read(reader, false, parent, entry);

            if (revision > 0xF)
                anim = anim.Read(reader, parent, entry);

            if (revision > 9)
            {
                triggerEventCount = reader.ReadUInt32();
                for (int i = 0; i < triggerEventCount; i++)
                {
                    triggerEvents.Add(Symbol.Read(reader));
                }
            }
            else if (revision > 6)
            {
                unkSym1 = Symbol.Read(reader);
                triggerEvents.Add(unkSym1);
            }

            if (revision > 6)
            {
                animCount = reader.ReadUInt32();
                for (int i = 0; i < animCount; i++)
                {
                    Anim anim = new();
                    anim.Read(reader);
                    anims.Add(anim);
                }

                soundsCount = reader.ReadUInt32();
                for (int i = 0; i < soundsCount; i++)
                {
                    sounds.Add(Symbol.Read(reader));
                }

                showsCount = reader.ReadUInt32();
                for (int i = 0; i < showsCount; i++)
                {
                    shows.Add(Symbol.Read(reader));
                }
            }

            if (revision > 0xC)
            {
                hideDelaysCount = reader.ReadUInt32();
                for (int i = 0; i < hideDelaysCount; i++)
                {
                    HideDelay hideDelay = new();
                    hideDelay.Read(reader);
                    hideDelays.Add(hideDelay);
                }
            }

            if (revision > 2)
            {
                enableEventsCount = reader.ReadUInt32();
                for (int i = 0; i < enableEventsCount; i++)
                {
                    enableEvents.Add(Symbol.Read(reader));
                }

                disableEventsCount = reader.ReadUInt32();
                for (int i = 0; i < disableEventsCount; i++)
                {
                    disableEvents.Add(Symbol.Read(reader));
                }
            }

            if (revision > 5)
            {
                waitForEventsCount = reader.ReadUInt32();
                for (int i = 0; i < waitForEventsCount; i++)
                {
                    waitForEvents.Add(Symbol.Read(reader));
                }
            }

            if (revision > 6)
                nextLink = Symbol.Read(reader);

            if (revision > 7)
            {
                proxyCallsCount = reader.ReadUInt32();

                for (int i = 0; i < proxyCallsCount; i++)
                {
                    ProxyCall proxyCall = new();
                    proxyCall.Read(reader);
                    proxyCalls.Add(proxyCall);
                }
            }

            if (revision > 0xB)
                triggerOrder = reader.ReadUInt32();

            if (revision > 0xD)
            {
                resetTriggersCount = reader.ReadUInt32();
                for (int i = 0; i < resetTriggersCount; i++)
                {
                    resetTriggers.Add(Symbol.Read(reader));
                }
            }

            if (revision > 0xE)
                enabledAtStart = reader.ReadBoolean();

            if (revision > 0xF)
            {
                animTrigger = reader.ReadUInt32();
                animFrame = reader.ReadFloat();
            }

            if (revision > 0x10)
            {
                partLauncherCount = reader.ReadUInt32();
                for (int i = 0; i < partLauncherCount; i++)
                {
                    partLaunchers.Add(Symbol.Read(reader));
                }
            }

            if (standalone)
                if ((reader.Endianness == Endian.BigEndian ? 0xADDEADDE : 0xDEADDEAD) != reader.ReadUInt32()) throw new Exception("Got to end of standalone asset but didn't find the expected end bytes, read likely did not succeed");

            return this;
        }

        public override void Write(EndianWriter writer, bool standalone, DirectoryMeta parent, DirectoryMeta.Entry? entry)
        {
            writer.WriteUInt32(BitConverter.IsLittleEndian ? (uint)((altRevision << 16) | revision) : (uint)((revision << 16) | altRevision));

            base.Write(writer, false, parent, entry);

            if (standalone)
                writer.WriteBlock(new byte[4] { 0xAD, 0xDE, 0xAD, 0xDE });
        }

    }
}
