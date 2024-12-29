namespace MiloLib.Classes
{
    public class UnsupportedAssetRevisionException : Exception
    {
        public uint Revision { get; }

        public string ClassName { get; }

        public UnsupportedAssetRevisionException(string className, uint revision) : base($"Unsupported {className} revision: {revision}")
        {
            Revision = revision;
            ClassName = className;
        }
    }

    public class UnsupportedMiloSceneRevision : Exception
    {
        public uint Revision { get; }

        public UnsupportedMiloSceneRevision(uint version) : base($"Unsupported Milo scene revision: {version}")
        {
            Revision = version;
        }
    }
}