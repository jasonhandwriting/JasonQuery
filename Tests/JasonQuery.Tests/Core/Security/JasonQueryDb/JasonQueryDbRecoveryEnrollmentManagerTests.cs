using JasonQuery.Core.Security.JasonQueryDb;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Security.Cryptography;

namespace JasonQuery.Tests.Core.Security.JasonQueryDb
{
    [TestClass]
    public class JasonQueryDbRecoveryEnrollmentManagerTests
    {
        [TestMethod]
        public void PrepareEnrollment_LegacyMetadataV1_ProducesVersion2DraftWithoutSaving()
        {
            using (var scope = new TestScope(JasonQueryDbStorageFormatContract.LegacyVersion))
            {
                scope.SetMetadataVersion(JasonQueryDbSecurityConstants.LegacyMetadataVersion);

                using (var draft = scope.Manager.PrepareEnrollment(scope.DatabasePath))
                {
                    Assert.AreEqual(JasonQueryDbSecurityConstants.MetadataVersion, draftKeyMetadataVersion(draft));
                    Assert.IsNull(scope.Store.Load().Recovery);
                    Assert.AreEqual(0, scope.Database.ChangePasswordCalls);
                }
            }
        }

        [TestMethod]
        public void PrepareEnrollment_ModernStorageV2_UsesSameLogicalDatabaseKey()
        {
            using (var scope = new TestScope(JasonQueryDbStorageFormatContract.ModernVersion))
            using (var draft = scope.Manager.PrepareEnrollment(scope.DatabasePath))
            using (var material = JasonQueryDbRecoveryKeyCodec.Decode(draft.RecoveryKeyText))
            {
                var metadata = GetTargetMetadata(draft);
                var recovered = JasonQueryDbRecoveryEnvelopeProtector.Unprotect(metadata.Recovery.ToEnvelope(), material);

                try
                {
                    CollectionAssert.AreEqual(scope.DatabaseKey, recovered);
                    Assert.AreEqual(0, scope.Database.ChangePasswordCalls);
                }
                finally
                {
                    Array.Clear(recovered, 0, recovered.Length);
                }
            }
        }

        [TestMethod]
        public void PrepareEnrollment_ExistingRecovery_RegeneratesDifferentRecoveryKey()
        {
            using (var scope = new TestScope(JasonQueryDbStorageFormatContract.ModernVersion))
            {
                scope.Enroll();

                var previousKeyId = scope.Store.Load().Recovery.KeyId;

                using (var draft = scope.Manager.PrepareEnrollment(scope.DatabasePath))
                {
                    Assert.AreNotEqual(previousKeyId, draft.KeyId);
                }
            }
        }

        [TestMethod]
        public void PrepareEnrollment_CustomPassword_Rejects()
        {
            using (var scope = new TestScope(JasonQueryDbStorageFormatContract.ModernVersion))
            {
                scope.SaveCustomPasswordMetadata();

                Assert.ThrowsExactly<InvalidOperationException>
                (
                    () => scope.Manager.PrepareEnrollment(scope.DatabasePath)
                );
            }
        }

        [TestMethod]
        public void PrepareEnrollment_DpapiFailure_Rejects()
        {
            using (var scope = new TestScope(JasonQueryDbStorageFormatContract.ModernVersion, new ThrowingKeyProtector()))
            {
                Assert.ThrowsExactly<CryptographicException>
                (
                    () => scope.Manager.PrepareEnrollment(scope.DatabasePath)
                );
            }
        }

        [TestMethod]
        public void PrepareEnrollment_InvalidLogicalKeyLength_Rejects()
        {
            using (var scope = new TestScope(JasonQueryDbStorageFormatContract.ModernVersion, new ShortKeyProtector()))
            {
                Assert.ThrowsExactly<InvalidDataException>
                (
                    () => scope.Manager.PrepareEnrollment(scope.DatabasePath)
                );
            }
        }

        [TestMethod]
        public void PrepareEnrollment_DatabaseCannotOpen_Rejects()
        {
            using (var scope = new TestScope(JasonQueryDbStorageFormatContract.ModernVersion))
            {
                scope.Database.CanOpenResult = false;

                Assert.ThrowsExactly<InvalidDataException>
                (
                    () => scope.Manager.PrepareEnrollment(scope.DatabasePath)
                );
            }
        }

        [TestMethod]
        public void PrepareEnrollment_MissingDatabase_Rejects()
        {
            using (var scope = new TestScope(JasonQueryDbStorageFormatContract.ModernVersion))
            {
                File.Delete(scope.DatabasePath);

                Assert.ThrowsExactly<FileNotFoundException>
                (
                    () => scope.Manager.PrepareEnrollment(scope.DatabasePath)
                );
            }
        }

        [TestMethod]
        public void CommitEnrollment_PersistsRecoveryAndPreservesDatabaseSecurityIdentity()
        {
            using (var scope = new TestScope(JasonQueryDbStorageFormatContract.ModernVersion))
            using (var draft = scope.Manager.PrepareEnrollment(scope.DatabasePath))
            {
                var before = scope.Store.Load();
                var committed = scope.Manager.CommitEnrollment(scope.DatabasePath, draft);

                Assert.IsNotNull(committed.Recovery);
                Assert.AreEqual(JasonQueryDbSecurityConstants.MetadataVersion, committed.MetadataVersion);
                Assert.AreEqual(before.EncryptionVersion, committed.EncryptionVersion);
                Assert.AreEqual(before.StorageFormatVersion, committed.StorageFormatVersion);
                Assert.AreEqual(before.Mode, committed.Mode);
                Assert.AreEqual(before.Protection, committed.Protection);
                Assert.AreEqual(before.ProtectedDatabaseKey, committed.ProtectedDatabaseKey);
                Assert.AreEqual(0, scope.Database.ChangePasswordCalls);
            }
        }

        [TestMethod]
        public void CommitEnrollment_RecoveryKeyUnwrapsOriginalLogicalDatabaseKey()
        {
            using (var scope = new TestScope(JasonQueryDbStorageFormatContract.ModernVersion))
            using (var draft = scope.Manager.PrepareEnrollment(scope.DatabasePath))
            {
                var encoded = draft.RecoveryKeyText;

                scope.Manager.CommitEnrollment(scope.DatabasePath, draft);

                using (var material = JasonQueryDbRecoveryKeyCodec.Decode(encoded))
                {
                    var metadata = scope.Store.Load();
                    var recovered = JasonQueryDbRecoveryEnvelopeProtector.Unprotect(metadata.Recovery.ToEnvelope(), material);

                    try
                    {
                        CollectionAssert.AreEqual(scope.DatabaseKey, recovered);
                    }
                    finally
                    {
                        Array.Clear(recovered, 0, recovered.Length);
                    }
                }
            }
        }

        [TestMethod]
        public void CommitEnrollment_StaleMetadata_RejectsWithoutOverwriting()
        {
            using (var scope = new TestScope(JasonQueryDbStorageFormatContract.ModernVersion))
            using (var draft = scope.Manager.PrepareEnrollment(scope.DatabasePath))
            {
                var changed = scope.Store.Load();

                changed.ProtectedDatabaseKey = Convert.ToBase64String(new byte[JasonQueryDbSecurityConstants.DatabaseKeySizeBytes]);
                scope.Store.Save(changed);

                Assert.ThrowsExactly<InvalidOperationException>
                (
                    () => scope.Manager.CommitEnrollment(scope.DatabasePath, draft)
                );

                Assert.IsNull(scope.Store.Load().Recovery);
            }
        }

        [TestMethod]
        public void CommitEnrollment_DisposedDraft_Rejects()
        {
            using (var scope = new TestScope(JasonQueryDbStorageFormatContract.ModernVersion))
            {
                var draft = scope.Manager.PrepareEnrollment(scope.DatabasePath);

                draft.Dispose();

                Assert.ThrowsExactly<ObjectDisposedException>
                (
                    () => scope.Manager.CommitEnrollment(scope.DatabasePath, draft)
                );
            }
        }

        [TestMethod]
        public void CommitEnrollment_DraftFromDifferentManager_Rejects()
        {
            using (var scope = new TestScope(JasonQueryDbStorageFormatContract.ModernVersion))
            using (var draft = scope.Manager.PrepareEnrollment(scope.DatabasePath))
            {
                var other = new JasonQueryDbRecoveryEnrollmentManager(scope.Store, scope.Protector, metadata => scope.Database);

                Assert.ThrowsExactly<InvalidOperationException>
                (
                    () => other.CommitEnrollment(scope.DatabasePath, draft)
                );
            }
        }

        [TestMethod]
        public void DisableRecovery_RemovesRecoveryWithoutDatabaseRekey()
        {
            using (var scope = new TestScope(JasonQueryDbStorageFormatContract.ModernVersion))
            {
                scope.Enroll();

                var before = scope.Store.Load();
                var committed = scope.Manager.DisableRecovery(scope.DatabasePath);

                Assert.IsNull(committed.Recovery);
                Assert.AreEqual(before.ProtectedDatabaseKey, committed.ProtectedDatabaseKey);
                Assert.AreEqual(before.StorageFormatVersion, committed.StorageFormatVersion);
                Assert.AreEqual(0, scope.Database.ChangePasswordCalls);
            }
        }

        [TestMethod]
        public void DisableRecovery_NoRecoveryConfigured_Rejects()
        {
            using (var scope = new TestScope(JasonQueryDbStorageFormatContract.ModernVersion))
            {
                Assert.ThrowsExactly<InvalidOperationException>
                (
                    () => scope.Manager.DisableRecovery(scope.DatabasePath)
                );
            }
        }

        [TestMethod]
        public void DisableRecovery_CustomPassword_Rejects()
        {
            using (var scope = new TestScope(JasonQueryDbStorageFormatContract.ModernVersion))
            {
                scope.SaveCustomPasswordMetadata();

                Assert.ThrowsExactly<InvalidOperationException>
                (
                    () => scope.Manager.DisableRecovery(scope.DatabasePath)
                );
            }
        }

        [TestMethod]
        public void IsRecoveryConfigured_TracksCommittedState()
        {
            using (var scope = new TestScope(JasonQueryDbStorageFormatContract.ModernVersion))
            {
                Assert.IsFalse(scope.Manager.IsRecoveryConfigured());

                scope.Enroll();

                Assert.IsTrue(scope.Manager.IsRecoveryConfigured());

                scope.Manager.DisableRecovery(scope.DatabasePath);

                Assert.IsFalse(scope.Manager.IsRecoveryConfigured());
            }
        }

        [TestMethod]
        public void Draft_Dispose_RejectsRecoveryKeyAccess()
        {
            using (var scope = new TestScope(JasonQueryDbStorageFormatContract.ModernVersion))
            {
                var draft = scope.Manager.PrepareEnrollment(scope.DatabasePath);

                draft.Dispose();

                Assert.ThrowsExactly<ObjectDisposedException>(() => _ = draft.RecoveryKeyText);
                Assert.ThrowsExactly<ObjectDisposedException>(() => _ = draft.KeyId);
            }
        }

        private static int draftKeyMetadataVersion(JasonQueryDbRecoveryEnrollmentDraft draft)
        {
            return GetTargetMetadata(draft).MetadataVersion;
        }

        private static JasonQueryDbSecurityMetadata GetTargetMetadata(JasonQueryDbRecoveryEnrollmentDraft draft)
        {
            var property = typeof(JasonQueryDbRecoveryEnrollmentDraft).GetProperty
            (
                "TargetMetadata",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic
            );

            return (JasonQueryDbSecurityMetadata)property.GetValue(draft, null);
        }

        private sealed class TestScope : IDisposable
        {
            public TestScope(int storageVersion, IJasonQueryDbKeyProtector protector = null)
            {
                DirectoryPath = Path.Combine(Path.GetTempPath(), "JasonQuery-R4F2E5-B3-" + Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(DirectoryPath);

                DatabasePath = Path.Combine(DirectoryPath, "JasonQuery.db");
                File.WriteAllText(DatabasePath, "synthetic");

                DatabaseKey = new byte[JasonQueryDbSecurityConstants.DatabaseKeySizeBytes];

                for (var index = 0; index < DatabaseKey.Length; index++)
                {
                    DatabaseKey[index] = (byte)(index + 1);
                }

                Protector = protector ?? new PassThroughKeyProtector();
                Store = new JasonQueryDbSecurityMetadataStore(Path.Combine(DirectoryPath, JasonQueryDbSecurityConstants.MetadataFileName));
                Database = new FakeDatabase(DatabaseKey);

                var metadata = JasonQueryDbSecurityMetadata.CreateWindowsCurrentUser(Convert.ToBase64String(DatabaseKey));

                metadata.StorageFormatVersion = storageVersion;
                Store.Save(metadata);

                Manager = new JasonQueryDbRecoveryEnrollmentManager(Store, Protector, value => Database);
            }

            public string DirectoryPath { get; }

            public string DatabasePath { get; }

            public byte[] DatabaseKey { get; }

            public IJasonQueryDbKeyProtector Protector { get; }

            public JasonQueryDbSecurityMetadataStore Store { get; }

            public FakeDatabase Database { get; }

            public JasonQueryDbRecoveryEnrollmentManager Manager { get; }

            public void SetMetadataVersion(int version)
            {
                var metadata = Store.Load();

                metadata.MetadataVersion = version;
                Store.Save(metadata);
            }

            public void SaveCustomPasswordMetadata()
            {
                var salt = new byte[JasonQueryDbSecurityConstants.PasswordSaltSizeBytes];

                try
                {
                    for (var index = 0; index < salt.Length; index++)
                    {
                        salt[index] = (byte)(index + 11);
                    }

                    var metadata = JasonQueryDbSecurityMetadata.CreateCustomPassword(salt, 1000);

                    metadata.StorageFormatVersion = JasonQueryDbStorageFormatContract.ModernVersion;
                    Store.Save(metadata);
                }
                finally
                {
                    Array.Clear(salt, 0, salt.Length);
                }
            }

            public void Enroll()
            {
                using (var draft = Manager.PrepareEnrollment(DatabasePath))
                {
                    Manager.CommitEnrollment(DatabasePath, draft);
                }
            }

            public void Dispose()
            {
                Array.Clear(DatabaseKey, 0, DatabaseKey.Length);

                if (Directory.Exists(DirectoryPath))
                {
                    Directory.Delete(DirectoryPath, true);
                }
            }
        }

        private sealed class PassThroughKeyProtector : IJasonQueryDbKeyProtector
        {
            public byte[] Protect(byte[] databaseKey)
            {
                return (byte[])databaseKey.Clone();
            }

            public byte[] Unprotect(byte[] protectedDatabaseKey)
            {
                return (byte[])protectedDatabaseKey.Clone();
            }
        }

        private sealed class ThrowingKeyProtector : IJasonQueryDbKeyProtector
        {
            public byte[] Protect(byte[] databaseKey)
            {
                throw new NotSupportedException();
            }

            public byte[] Unprotect(byte[] protectedDatabaseKey)
            {
                throw new CryptographicException("Synthetic DPAPI failure.");
            }
        }

        private sealed class ShortKeyProtector : IJasonQueryDbKeyProtector
        {
            public byte[] Protect(byte[] databaseKey)
            {
                throw new NotSupportedException();
            }

            public byte[] Unprotect(byte[] protectedDatabaseKey)
            {
                return new byte[JasonQueryDbSecurityConstants.DatabaseKeySizeBytes - 1];
            }
        }

        public sealed class FakeDatabase : IJasonQueryDbMigrationDatabase
        {
            private readonly string _expectedPassword;

            public FakeDatabase(byte[] databaseKey)
            {
                _expectedPassword = Convert.ToBase64String(databaseKey);
            }

            public bool CanOpenResult { get; set; } = true;

            public int ChangePasswordCalls { get; private set; }

            public bool CanOpen(string databaseFilePath, string databasePassword)
            {
                return CanOpenResult && string.Equals(_expectedPassword, databasePassword, StringComparison.Ordinal);
            }

            public void CreateVerifiedCopy(string sourceDatabaseFilePath, string destinationDatabaseFilePath, string databasePassword)
            {
                throw new NotSupportedException();
            }

            public void ChangePassword(string databaseFilePath, string currentDatabasePassword, string newDatabasePassword)
            {
                ChangePasswordCalls++;
                throw new InvalidOperationException("Recovery enrollment must not rekey the database.");
            }
        }
    }
}
