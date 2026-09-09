using JasonQuery.Core.Security.Database;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace JasonQuery.Tests.Core.Security.Database
{
    [TestClass]
    public class DatabaseSecurityMetadataStoreTests
    {
        [TestMethod]
        public void SaveAndLoad_WindowsCurrentUser_RoundTripsMetadata()
        {
            var directory = CreateTemporaryDirectory();

            try
            {
                var metadataPath = Path.Combine(directory, DatabaseSecurityConstants.MetadataFileName);
                var store = new DatabaseSecurityMetadataStore(metadataPath);
                var metadata = DatabaseSecurityMetadata.CreateWindowsCurrentUser(Convert.ToBase64String(new byte[] { 1, 2, 3, 4 }));

                Assert.IsFalse
                (
                    metadata.StorageFormatVersion.HasValue
                );

                metadata.StorageFormatVersion = DatabaseStorageFormatContract.LegacyVersion;

                store.Save(metadata);

                var json = File.ReadAllText(metadataPath);
                var loaded = store.Load();

                StringAssert.Contains(json, "\"storageFormatVersion\": 1");

                Assert.AreEqual(DatabaseSecurityConstants.MetadataVersion, loaded.MetadataVersion);
                Assert.AreEqual(DatabaseSecurityConstants.EncryptionVersion, loaded.EncryptionVersion);

                Assert.IsTrue
                (
                    loaded.StorageFormatVersion.HasValue
                );

                Assert.AreEqual
                (
                    DatabaseStorageFormatContract.LegacyVersion,
                    loaded.StorageFormatVersion.Value
                );

                Assert.AreEqual(DatabaseSecurityMode.WindowsCurrentUser, loaded.Mode);
                Assert.AreEqual("DPAPI-CurrentUser", loaded.Protection);
                Assert.AreEqual(metadata.ProtectedDatabaseKey, loaded.ProtectedDatabaseKey);
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        [TestMethod]
        public void Save_ExistingMetadata_ReplacesAtomically()
        {
            var directory = CreateTemporaryDirectory();

            try
            {
                var metadataPath = Path.Combine(directory, DatabaseSecurityConstants.MetadataFileName);
                var store = new DatabaseSecurityMetadataStore(metadataPath);

                var windowsMetadata = DatabaseSecurityMetadata.CreateWindowsCurrentUser
                (
                    Convert.ToBase64String(new byte[] { 1, 2, 3, 4 })
                );

                Assert.IsFalse
                (
                    windowsMetadata.StorageFormatVersion.HasValue
                );

                windowsMetadata.StorageFormatVersion = DatabaseStorageFormatContract.LegacyVersion;

                store.Save(windowsMetadata);

                var salt = new byte[DatabaseSecurityConstants.PasswordSaltSizeBytes];

                for (var index = 0; index < salt.Length; index++)
                {
                    salt[index] = (byte)(index + 1);
                }

                var customMetadata = DatabaseSecurityMetadata.CreateCustomPassword(salt, 1000);

                Assert.IsFalse
                (
                    customMetadata.StorageFormatVersion.HasValue
                );

                customMetadata.StorageFormatVersion = DatabaseStorageFormatContract.LegacyVersion;

                store.Save(customMetadata);

                var loaded = store.Load();

                Assert.AreEqual(DatabaseSecurityMode.CustomPassword, loaded.Mode);
                Assert.AreEqual(1000, loaded.Iterations);
                Assert.AreEqual(DatabaseSecurityConstants.Pbkdf2HmacSha256, loaded.Kdf);

                Assert.IsTrue
                (
                    loaded.StorageFormatVersion.HasValue
                );

                Assert.AreEqual
                (
                    DatabaseStorageFormatContract.LegacyVersion,
                    loaded.StorageFormatVersion.Value
                );
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        [TestMethod]
        public void Load_MetadataWithoutStorageFormatVersion_ResolvesAsLegacy()
        {
            var directory = CreateTemporaryDirectory();

            try
            {
                var metadataPath = Path.Combine(directory, DatabaseSecurityConstants.MetadataFileName);

                var json =
                    "{" + Environment.NewLine +
                    $"  \"metadataVersion\": {DatabaseSecurityConstants.MetadataVersion}," + Environment.NewLine +
                    $"  \"encryptionVersion\": {DatabaseSecurityConstants.EncryptionVersion}," + Environment.NewLine +
                    "  \"mode\": \"WindowsCurrentUser\"," + Environment.NewLine +
                    "  \"protection\": \"DPAPI-CurrentUser\"," + Environment.NewLine +
                    "  \"protectedDatabaseKey\": \"AQIDBA==\"" + Environment.NewLine +
                    "}";

                File.WriteAllText
                (
                    metadataPath,
                    json
                );

                var store = new DatabaseSecurityMetadataStore(metadataPath);
                var loaded = store.Load();

                Assert.IsFalse
                (
                    loaded.StorageFormatVersion.HasValue
                );

                Assert.AreEqual
                (
                    DatabaseStorageFormatVersion.LegacySystemDataSQLiteCryptoApi,
                    DatabaseStorageFormatContract.ResolveVersion
                    (
                        loaded.StorageFormatVersion
                    )
                );
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        [TestMethod]
        public void Load_UnknownStorageFormatVersion_ThrowsNotSupportedException()
        {
            var directory = CreateTemporaryDirectory();

            try
            {
                var metadataPath = Path.Combine(directory, DatabaseSecurityConstants.MetadataFileName);

                var json =
                    "{" + Environment.NewLine +
                    $"  \"metadataVersion\": {DatabaseSecurityConstants.MetadataVersion}," + Environment.NewLine +
                    $"  \"encryptionVersion\": {DatabaseSecurityConstants.EncryptionVersion}," + Environment.NewLine +
                    "  \"storageFormatVersion\": 2," + Environment.NewLine +
                    "  \"mode\": \"WindowsCurrentUser\"," + Environment.NewLine +
                    "  \"protection\": \"DPAPI-CurrentUser\"," + Environment.NewLine +
                    "  \"protectedDatabaseKey\": \"AQIDBA==\"" + Environment.NewLine +
                    "}";

                File.WriteAllText
                (
                    metadataPath,
                    json
                );

                var store = new DatabaseSecurityMetadataStore(metadataPath);

                Assert.ThrowsException<NotSupportedException>
                (
                    () => store.Load()
                );
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        private static string CreateTemporaryDirectory()
        {
            var directory = Path.Combine
            (
                Path.GetTempPath(),
                "JasonQuery.Tests",
                Guid.NewGuid().ToString("N")
            );

            Directory.CreateDirectory(directory);
            return directory;
        }
    }
}
