using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

namespace JasonQuery.Core.Security.JasonQueryDb
{
    /// <summary>
    /// Deterministic metadata snapshot/serialization rules for physical storage migration.
    /// The security identity is preserved; only storageFormatVersion changes from V1 to V2.
    /// </summary>
    internal static class JasonQueryDbStorageMigrationMetadataContract
    {
        private static readonly UTF8Encoding Utf8WithoutBom = new UTF8Encoding(false);

        public static JasonQueryDbSecurityMetadata CreateTargetMetadata(JasonQueryDbSecurityMetadata sourceMetadata)
        {
            if (sourceMetadata == null)
            {
                throw new ArgumentNullException(nameof(sourceMetadata));
            }

            sourceMetadata.Validate();

            var sourceStorageVersion = (int)JasonQueryDbStorageFormatContract.ResolveVersion
            (
                sourceMetadata.StorageFormatVersion
            );

            if (sourceStorageVersion != JasonQueryDbStorageFormatContract.LegacyVersion)
            {
                throw new InvalidOperationException
                (
                    "Physical storage migration requires Legacy Storage V1 source metadata."
                );
            }

            var targetMetadata = new JasonQueryDbSecurityMetadata
            {
                MetadataVersion = sourceMetadata.MetadataVersion,
                EncryptionVersion = sourceMetadata.EncryptionVersion,
                StorageFormatVersion = JasonQueryDbStorageFormatContract.ModernVersion,
                Mode = sourceMetadata.Mode,
                Protection = sourceMetadata.Protection,
                ProtectedDatabaseKey = sourceMetadata.ProtectedDatabaseKey,
                Kdf = sourceMetadata.Kdf,
                Iterations = sourceMetadata.Iterations,
                Salt = sourceMetadata.Salt,
                Recovery = JasonQueryDbRecoveryMetadata.Clone(sourceMetadata.Recovery)
            };

            targetMetadata.Validate();
            EnsureSecurityIdentityPreserved(sourceMetadata, targetMetadata);

            return targetMetadata;
        }

        public static byte[] SerializeUtf8WithoutBom(JasonQueryDbSecurityMetadata metadata)
        {
            if (metadata == null)
            {
                throw new ArgumentNullException(nameof(metadata));
            }

            metadata.Validate();

            var json = JsonConvert.SerializeObject(metadata, Formatting.Indented);

            return Utf8WithoutBom.GetBytes(json);
        }

        public static string ComputeSerializedSha256(JasonQueryDbSecurityMetadata metadata)
        {
            var bytes = SerializeUtf8WithoutBom(metadata);

            try
            {
                return JasonQueryDbStorageMigrationFileIntegrity.ComputeSha256(bytes);
            }
            finally
            {
                Array.Clear(bytes, 0, bytes.Length);
            }
        }

        public static void WriteDurablyCreateNew(string filePath, JasonQueryDbSecurityMetadata metadata)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("A metadata temporary file path is required.", nameof(filePath));
            }

            var normalizedPath = Path.GetFullPath(filePath);
            var directory = Path.GetDirectoryName(normalizedPath);

            if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
            {
                throw new DirectoryNotFoundException
                (
                    "The metadata temporary file directory does not exist."
                );
            }

            var bytes = SerializeUtf8WithoutBom(metadata);

            try
            {
                using (var stream = new FileStream(normalizedPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, FileOptions.WriteThrough))
                {
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Flush(true);
                }
            }
            finally
            {
                Array.Clear(bytes, 0, bytes.Length);
            }
        }

        public static bool MetadataEquals(JasonQueryDbSecurityMetadata left, JasonQueryDbSecurityMetadata right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (left == null || right == null)
            {
                return false;
            }

            return left.MetadataVersion == right.MetadataVersion
                   && left.EncryptionVersion == right.EncryptionVersion
                   && left.StorageFormatVersion == right.StorageFormatVersion
                   && left.Mode == right.Mode
                   && string.Equals(left.Protection, right.Protection, StringComparison.Ordinal)
                   && string.Equals(left.ProtectedDatabaseKey, right.ProtectedDatabaseKey, StringComparison.Ordinal)
                   && string.Equals(left.Kdf, right.Kdf, StringComparison.Ordinal)
                   && left.Iterations == right.Iterations
                   && string.Equals(left.Salt, right.Salt, StringComparison.Ordinal)
                   && JasonQueryDbRecoveryMetadata.MetadataEquals(left.Recovery, right.Recovery);
        }

        private static void EnsureSecurityIdentityPreserved(JasonQueryDbSecurityMetadata sourceMetadata, JasonQueryDbSecurityMetadata targetMetadata)
        {
            if (sourceMetadata.MetadataVersion != targetMetadata.MetadataVersion
                || sourceMetadata.EncryptionVersion != targetMetadata.EncryptionVersion
                || sourceMetadata.Mode != targetMetadata.Mode
                || !string.Equals(sourceMetadata.Protection, targetMetadata.Protection, StringComparison.Ordinal)
                || !string.Equals(sourceMetadata.ProtectedDatabaseKey, targetMetadata.ProtectedDatabaseKey, StringComparison.Ordinal)
                || !string.Equals(sourceMetadata.Kdf, targetMetadata.Kdf, StringComparison.Ordinal)
                || sourceMetadata.Iterations != targetMetadata.Iterations
                || !string.Equals(sourceMetadata.Salt, targetMetadata.Salt, StringComparison.Ordinal))
            {
                throw new InvalidOperationException
                (
                    "Physical storage migration must preserve the existing database security identity."
                );
            }
        }
    }
}
