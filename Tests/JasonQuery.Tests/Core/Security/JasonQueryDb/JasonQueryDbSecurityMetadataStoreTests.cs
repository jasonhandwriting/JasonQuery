using JasonQuery.Core.Security.JasonQueryDb;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace JasonQuery.Tests.Core.Security.JasonQueryDb
{
    [TestClass]
    public class JasonQueryDbSecurityMetadataStoreTests
    {
        [TestMethod]
        public void SaveAndLoad_WindowsCurrentUser_RoundTripsMetadata()
        {
            var directory = CreateTemporaryDirectory();

            try
            {
                var metadataPath = Path.Combine(directory, JasonQueryDbSecurityConstants.MetadataFileName);
                var store = new JasonQueryDbSecurityMetadataStore(metadataPath);
                var metadata = JasonQueryDbSecurityMetadata.CreateWindowsCurrentUser(Convert.ToBase64String(new byte[] { 1, 2, 3, 4 }));

                Assert.IsFalse
                (
                    metadata.StorageFormatVersion.HasValue
                );

                metadata.StorageFormatVersion = JasonQueryDbStorageFormatContract.LegacyVersion;

                store.Save(metadata);

                var json = File.ReadAllText(metadataPath);
                var loaded = store.Load();

                Assert.Contains("\"storageFormatVersion\": 1", json);

                Assert.AreEqual(JasonQueryDbSecurityConstants.MetadataVersion, loaded.MetadataVersion);
                Assert.AreEqual(JasonQueryDbSecurityConstants.EncryptionVersion, loaded.EncryptionVersion);

                Assert.IsTrue
                (
                    loaded.StorageFormatVersion.HasValue
                );

                Assert.AreEqual
                (
                    JasonQueryDbStorageFormatContract.LegacyVersion,
                    loaded.StorageFormatVersion.Value
                );

                Assert.AreEqual(JasonQueryDbSecurityMode.WindowsCurrentUser, loaded.Mode);
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
                var metadataPath = Path.Combine(directory, JasonQueryDbSecurityConstants.MetadataFileName);
                var store = new JasonQueryDbSecurityMetadataStore(metadataPath);

                var windowsMetadata = JasonQueryDbSecurityMetadata.CreateWindowsCurrentUser
                (
                    Convert.ToBase64String(new byte[] { 1, 2, 3, 4 })
                );

                Assert.IsFalse
                (
                    windowsMetadata.StorageFormatVersion.HasValue
                );

                windowsMetadata.StorageFormatVersion = JasonQueryDbStorageFormatContract.LegacyVersion;

                store.Save(windowsMetadata);

                var salt = new byte[JasonQueryDbSecurityConstants.PasswordSaltSizeBytes];

                for (var index = 0; index < salt.Length; index++)
                {
                    salt[index] = (byte)(index + 1);
                }

                var customMetadata = JasonQueryDbSecurityMetadata.CreateCustomPassword(salt, 1000);

                Assert.IsFalse
                (
                    customMetadata.StorageFormatVersion.HasValue
                );

                customMetadata.StorageFormatVersion = JasonQueryDbStorageFormatContract.LegacyVersion;

                store.Save(customMetadata);

                var loaded = store.Load();

                Assert.AreEqual(JasonQueryDbSecurityMode.CustomPassword, loaded.Mode);
                Assert.AreEqual(1000, loaded.Iterations);
                Assert.AreEqual(JasonQueryDbSecurityConstants.Pbkdf2HmacSha256, loaded.Kdf);

                Assert.IsTrue
                (
                    loaded.StorageFormatVersion.HasValue
                );

                Assert.AreEqual
                (
                    JasonQueryDbStorageFormatContract.LegacyVersion,
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
                var metadataPath = Path.Combine(directory, JasonQueryDbSecurityConstants.MetadataFileName);

                var json = "{" + Environment.NewLine
                           + $"  \"metadataVersion\": {JasonQueryDbSecurityConstants.MetadataVersion}," + Environment.NewLine
                           + $"  \"encryptionVersion\": {JasonQueryDbSecurityConstants.EncryptionVersion}," + Environment.NewLine
                           + "  \"mode\": \"WindowsCurrentUser\"," + Environment.NewLine
                           + "  \"protection\": \"DPAPI-CurrentUser\"," + Environment.NewLine
                           + "  \"protectedDatabaseKey\": \"AQIDBA==\"" + Environment.NewLine
                           + "}";

                File.WriteAllText
                (
                    metadataPath,
                    json
                );

                var store = new JasonQueryDbSecurityMetadataStore(metadataPath);
                var loaded = store.Load();

                Assert.IsFalse
                (
                    loaded.StorageFormatVersion.HasValue
                );

                Assert.AreEqual
                (
                    JasonQueryDbStorageFormatVersion.LegacySystemDataSQLiteCryptoApi,
                    JasonQueryDbStorageFormatContract.ResolveVersion
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
        public void Load_ModernStorageFormatVersion_ResolvesAsModern()
        {
            var directory = CreateTemporaryDirectory();

            try
            {
                var metadataPath = Path.Combine(directory, JasonQueryDbSecurityConstants.MetadataFileName);

                var json = "{" + Environment.NewLine
                           + $"  \"metadataVersion\": {JasonQueryDbSecurityConstants.MetadataVersion}," + Environment.NewLine
                           + $"  \"encryptionVersion\": {JasonQueryDbSecurityConstants.EncryptionVersion}," + Environment.NewLine
                           + "  \"storageFormatVersion\": 2," + Environment.NewLine
                           + "  \"mode\": \"WindowsCurrentUser\"," + Environment.NewLine
                           + "  \"protection\": \"DPAPI-CurrentUser\"," + Environment.NewLine
                           + "  \"protectedDatabaseKey\": \"AQIDBA==\"" + Environment.NewLine
                           + "}";

                File.WriteAllText
                (
                    metadataPath,
                    json
                );

                var store = new JasonQueryDbSecurityMetadataStore(metadataPath);
                var loaded = store.Load();

                Assert.IsTrue
                (
                    loaded.StorageFormatVersion.HasValue
                );

                Assert.AreEqual
                (
                    JasonQueryDbStorageFormatContract.ModernVersion,
                    loaded.StorageFormatVersion.Value
                );

                Assert.AreEqual
                (
                    JasonQueryDbStorageFormatVersion.SqlCipherCompatibility4,
                    JasonQueryDbStorageFormatContract.ResolveVersion
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
                var metadataPath = Path.Combine(directory, JasonQueryDbSecurityConstants.MetadataFileName);

                var json = "{" + Environment.NewLine
                           + $"  \"metadataVersion\": {JasonQueryDbSecurityConstants.MetadataVersion}," + Environment.NewLine
                           + $"  \"encryptionVersion\": {JasonQueryDbSecurityConstants.EncryptionVersion}," + Environment.NewLine
                           + "  \"storageFormatVersion\": 3," + Environment.NewLine
                           + "  \"mode\": \"WindowsCurrentUser\"," + Environment.NewLine
                           + "  \"protection\": \"DPAPI-CurrentUser\"," + Environment.NewLine
                           + "  \"protectedDatabaseKey\": \"AQIDBA==\"" + Environment.NewLine
                           + "}";

                File.WriteAllText
                (
                    metadataPath,
                    json
                );

                var store = new JasonQueryDbSecurityMetadataStore(metadataPath);

                Assert.ThrowsExactly<NotSupportedException>
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
