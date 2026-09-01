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

                store.Save(metadata);

                var loaded = store.Load();

                Assert.AreEqual(DatabaseSecurityConstants.MetadataVersion, loaded.MetadataVersion);
                Assert.AreEqual(DatabaseSecurityConstants.EncryptionVersion, loaded.EncryptionVersion);
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

                store.Save(DatabaseSecurityMetadata.CreateWindowsCurrentUser(Convert.ToBase64String(new byte[] { 1, 2, 3, 4 })));

                var salt = new byte[DatabaseSecurityConstants.PasswordSaltSizeBytes];

                for (var index = 0; index < salt.Length; index++)
                {
                    salt[index] = (byte)(index + 1);
                }

                store.Save(DatabaseSecurityMetadata.CreateCustomPassword(salt, 1000));

                var loaded = store.Load();

                Assert.AreEqual(DatabaseSecurityMode.CustomPassword, loaded.Mode);
                Assert.AreEqual(1000, loaded.Iterations);
                Assert.AreEqual(DatabaseSecurityConstants.Pbkdf2HmacSha256, loaded.Kdf);
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        private static string CreateTemporaryDirectory()
        {
            var directory = Path.Combine(Path.GetTempPath(), "JasonQuery.Tests", Guid.NewGuid().ToString("N"));

            Directory.CreateDirectory(directory);
            return directory;
        }
    }
}
