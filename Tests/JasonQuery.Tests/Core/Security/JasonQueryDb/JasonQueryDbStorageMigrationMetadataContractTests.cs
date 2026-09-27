using JasonQuery.Core.Security.JasonQueryDb;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace JasonQuery.Tests.Core.Security.JasonQueryDb
{
    [TestClass]
    public class JasonQueryDbStorageMigrationMetadataContractTests
    {
        [TestMethod]
        public void CreateTargetMetadata_WindowsModePreservesSecurityIdentityAndChangesOnlyStorageFormat()
        {
            var source = JasonQueryDbSecurityMetadata.CreateWindowsCurrentUser
            (
                Convert.ToBase64String(new byte[] { 1, 2, 3, 4, 5, 6 })
            );

            source.StorageFormatVersion = null;

            var target = JasonQueryDbStorageMigrationMetadataContract.CreateTargetMetadata(source);

            Assert.AreEqual(source.MetadataVersion, target.MetadataVersion);
            Assert.AreEqual(source.EncryptionVersion, target.EncryptionVersion);
            Assert.AreEqual(source.Mode, target.Mode);
            Assert.AreEqual(source.Protection, target.Protection);
            Assert.AreEqual(source.ProtectedDatabaseKey, target.ProtectedDatabaseKey);
            Assert.AreEqual(source.Kdf, target.Kdf);
            Assert.AreEqual(source.Iterations, target.Iterations);
            Assert.AreEqual(source.Salt, target.Salt);
            Assert.AreEqual(JasonQueryDbStorageFormatContract.ModernVersion, target.StorageFormatVersion);
        }

        [TestMethod]
        public void CreateTargetMetadata_CustomPasswordModePreservesSecurityIdentity()
        {
            var source = JasonQueryDbSecurityMetadata.CreateCustomPassword
            (
                new byte[JasonQueryDbSecurityConstants.PasswordSaltSizeBytes],
                JasonQueryDbSecurityConstants.DefaultPbkdf2Iterations
            );

            source.StorageFormatVersion = JasonQueryDbStorageFormatContract.LegacyVersion;

            var target = JasonQueryDbStorageMigrationMetadataContract.CreateTargetMetadata(source);

            Assert.AreEqual(source.MetadataVersion, target.MetadataVersion);
            Assert.AreEqual(source.EncryptionVersion, target.EncryptionVersion);
            Assert.AreEqual(source.Mode, target.Mode);
            Assert.AreEqual(source.Kdf, target.Kdf);
            Assert.AreEqual(source.Iterations, target.Iterations);
            Assert.AreEqual(source.Salt, target.Salt);
            Assert.AreEqual(JasonQueryDbStorageFormatContract.ModernVersion, target.StorageFormatVersion);
        }

        [TestMethod]
        public void CreateTargetMetadata_RejectsModernSource()
        {
            var source = CreateWindowsMetadata(JasonQueryDbStorageFormatContract.ModernVersion);

            Assert.ThrowsExactly<InvalidOperationException>
            (
                () => JasonQueryDbStorageMigrationMetadataContract.CreateTargetMetadata(source)
            );
        }

        [TestMethod]
        public void SerializeUtf8WithoutBom_IsDeterministicAndHashMatchesBytes()
        {
            var metadata = CreateWindowsMetadata(JasonQueryDbStorageFormatContract.ModernVersion);
            var first = JasonQueryDbStorageMigrationMetadataContract.SerializeUtf8WithoutBom(metadata);
            var second = JasonQueryDbStorageMigrationMetadataContract.SerializeUtf8WithoutBom(metadata);

            try
            {
                CollectionAssert.AreEqual(first, second);
                Assert.IsGreaterThan(3, first.Length);
                Assert.IsFalse(first[0] == 0xEF && first[1] == 0xBB && first[2] == 0xBF);

                Assert.AreEqual
                (
                    JasonQueryDbStorageMigrationFileIntegrity.ComputeSha256(first),
                    JasonQueryDbStorageMigrationMetadataContract.ComputeSerializedSha256(metadata)
                );
            }
            finally
            {
                Array.Clear(first, 0, first.Length);
                Array.Clear(second, 0, second.Length);
            }
        }

        [TestMethod]
        public void WriteDurablyCreateNew_WritesExactSerializedBytesAndDoesNotOverwrite()
        {
            var directory = CreateTemporaryDirectory();

            try
            {
                var path = Path.Combine(directory, "JasonQuery.security.json.migration.tmp");
                var metadata = CreateWindowsMetadata(JasonQueryDbStorageFormatContract.ModernVersion);
                var expected = JasonQueryDbStorageMigrationMetadataContract.SerializeUtf8WithoutBom(metadata);

                try
                {
                    JasonQueryDbStorageMigrationMetadataContract.WriteDurablyCreateNew(path, metadata);

                    var actual = File.ReadAllBytes(path);

                    try
                    {
                        CollectionAssert.AreEqual(expected, actual);
                    }
                    finally
                    {
                        Array.Clear(actual, 0, actual.Length);
                    }

                    Assert.ThrowsExactly<IOException>
                    (
                        () => JasonQueryDbStorageMigrationMetadataContract.WriteDurablyCreateNew(path, metadata)
                    );
                }
                finally
                {
                    Array.Clear(expected, 0, expected.Length);
                }
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        private static JasonQueryDbSecurityMetadata CreateWindowsMetadata(int storageFormatVersion)
        {
            var metadata = JasonQueryDbSecurityMetadata.CreateWindowsCurrentUser
            (
                Convert.ToBase64String(new byte[] { 9, 8, 7, 6, 5, 4 })
            );

            metadata.StorageFormatVersion = storageFormatVersion;
            return metadata;
        }

        private static string CreateTemporaryDirectory()
        {
            var directory = Path.Combine
            (
                Path.GetTempPath(),
                "JasonQuery-Step389F-R4A-Metadata-" + Guid.NewGuid().ToString("N")
            );

            Directory.CreateDirectory(directory);
            return directory;
        }
    }
}
