using System;

namespace JasonQuery.Core.Security.Database
{
    /// <summary>
    /// Versioned binary IPC contract for logical Storage V1 -> V2 transfer.
    /// Secrets must never be placed in process arguments or diagnostic logs.
    /// </summary>
    public static class DatabaseStorageMigrationProtocolContract
    {
        public const string Magic = "JQSM";
        public const int CurrentVersion = 1;
        public const int SourceStorageFormatVersion = DatabaseStorageFormatContract.LegacyVersion;
        public const int TargetStorageFormatVersion = DatabaseStorageFormatContract.ModernVersion;

        public static void EnsureSupportedVersion(int protocolVersion)
        {
            if (protocolVersion != CurrentVersion)
            {
                throw new NotSupportedException
                (
                    $"JasonQuery database storage-migration protocol version '{protocolVersion}' is not supported."
                );
            }
        }

        public static void EnsureSupportedRoute(int sourceStorageFormatVersion, int targetStorageFormatVersion)
        {
            if (sourceStorageFormatVersion != SourceStorageFormatVersion || targetStorageFormatVersion != TargetStorageFormatVersion)
            {
                throw new NotSupportedException
                (
                    $"JasonQuery database storage migration route " +
                    $"'{sourceStorageFormatVersion} -> {targetStorageFormatVersion}' is not supported."
                );
            }
        }
    }
}
