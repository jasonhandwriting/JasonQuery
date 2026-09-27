using System;
using System.IO;

namespace JasonQuery.Core.Security.JasonQueryDb
{
    /// <summary>
    /// Immutable routing decision from one persisted storage-format family to
    /// the production runtime family that is allowed to open it.
    /// </summary>
    internal sealed class JasonQueryDbStorageRuntimeRoute
    {
        public JasonQueryDbStorageRuntimeRoute(JasonQueryDbStorageFormatVersion storageFormatVersion, JasonQueryDbStorageRuntimeKind runtimeKind)
        {
            EnsureValidPair
            (
                storageFormatVersion,
                runtimeKind
            );

            StorageFormatVersion = storageFormatVersion;
            RuntimeKind = runtimeKind;
        }

        public JasonQueryDbStorageFormatVersion StorageFormatVersion { get; }

        public JasonQueryDbStorageRuntimeKind RuntimeKind { get; }

        public int PersistedStorageFormatVersion => (int)StorageFormatVersion;

        public bool IsLegacy => RuntimeKind == JasonQueryDbStorageRuntimeKind.LegacySystemDataSQLite;

        public bool IsModern => RuntimeKind == JasonQueryDbStorageRuntimeKind.ModernSqlCipher;

        private static void EnsureValidPair(JasonQueryDbStorageFormatVersion storageFormatVersion, JasonQueryDbStorageRuntimeKind runtimeKind)
        {
            if (storageFormatVersion == JasonQueryDbStorageFormatVersion.LegacySystemDataSQLiteCryptoApi
                && runtimeKind == JasonQueryDbStorageRuntimeKind.LegacySystemDataSQLite)
            {
                return;
            }

            if (storageFormatVersion == JasonQueryDbStorageFormatVersion.SqlCipherCompatibility4
                && runtimeKind == JasonQueryDbStorageRuntimeKind.ModernSqlCipher)
            {
                return;
            }

            throw new InvalidDataException
            (
                $"Storage format '{(int)storageFormatVersion}' cannot be routed " +
                $"to runtime '{runtimeKind}'."
            );
        }
    }
}
