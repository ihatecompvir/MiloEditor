using System;

namespace MiloLib.Exceptions
{
    /// <summary>
    /// Exception thrown when an asset fails to read properly, containing contextual information
    /// about which asset failed and where it was located in the milo's hierarchy.
    /// </summary>
    public class MiloAssetReadException : Exception
    {
        public string AssetType { get; }
        public string AssetName { get; }
        public string DirectoryName { get; }
        public string DirectoryType { get; }
        public long StreamPosition { get; }

        public MiloAssetReadException(
            string message,
            string assetType,
            string assetName,
            string directoryName,
            string directoryType,
            long streamPosition,
            Exception innerException = null)
            : base(FormatMessage(message, assetType, assetName, directoryName, directoryType, streamPosition), innerException)
        {
            AssetType = assetType;
            AssetName = assetName;
            DirectoryName = directoryName;
            DirectoryType = directoryType;
            StreamPosition = streamPosition;
        }

        private static string FormatMessage(
            string message,
            string assetType,
            string assetName,
            string directoryName,
            string directoryType,
            long streamPosition)
        {
            return $"{message}\n" +
                   $"Asset: {assetType} '{assetName}'\n" +
                   $"Directory: {directoryType} '{directoryName}'\n" +
                   $"Stream Position: 0x{streamPosition:X}";
        }

        /// <summary>
        /// Creates a MiloAssetReadException for a standalone asset that didn't find expected end bytes.
        /// </summary>
        public static MiloAssetReadException EndBytesNotFound(
            Assets.DirectoryMeta parent,
            Assets.DirectoryMeta.Entry entry,
            long streamPosition)
        {
            return new MiloAssetReadException(
                "Got to end of standalone asset but didn't find the expected end bytes (0xADDEADDE), read likely did not succeed",
                entry.type.value,
                entry.name.value,
                parent.name.value,
                parent.type.value,
                streamPosition);
        }

        /// <summary>
        /// Wraps an existing exception with asset context information.
        /// </summary>
        public static MiloAssetReadException WrapException(
            Exception innerException,
            Assets.DirectoryMeta parent,
            Assets.DirectoryMeta.Entry entry,
            long streamPosition)
        {
            // Don't double-wrap if it's already a MiloAssetReadException
            if (innerException is MiloAssetReadException)
                return (MiloAssetReadException)innerException;

            return new MiloAssetReadException(
                $"Failed to read asset: {innerException.Message}",
                entry.type.value,
                entry.name.value,
                parent.name.value,
                parent.type.value,
                streamPosition,
                innerException);
        }
    }
}
