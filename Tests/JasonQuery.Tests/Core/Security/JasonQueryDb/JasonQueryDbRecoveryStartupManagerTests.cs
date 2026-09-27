using JasonQuery.Core.Security.JasonQueryDb;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Security.Cryptography;

namespace JasonQuery.Tests.Core.Security.JasonQueryDb
{
    [TestClass]
    public class JasonQueryDbRecoveryStartupManagerTests
    {
        [TestMethod]
        public void Recover_StorageV1_RebindsSameLogicalKeyAndPreservesRecovery()
        {
            using (var scope = new TestScope(JasonQueryDbStorageFormatContract.LegacyVersion))
            {
                var before = scope.Store.Load();
                var result = scope.Manager.Recover(scope.DatabasePath, scope.RecoveryKeyText);
                var after = scope.Store.Load();

                Assert.AreEqual(JasonQueryDbSecurityStartupState.V2Ready, result.State);
                Assert.AreEqual(JasonQueryDbKeyGenerator.ToDatabasePassword(scope.DatabaseKey), result.DatabasePassword);
                Assert.AreEqual(before.StorageFormatVersion, after.StorageFormatVersion);
                Assert.IsTrue(JasonQueryDbRecoveryMetadata.MetadataEquals(before.Recovery, after.Recovery));
                Assert.AreNotEqual(before.ProtectedDatabaseKey, after.ProtectedDatabaseKey);
                Assert.AreEqual(1, scope.ProtectCalls);
                Assert.AreEqual(0, scope.Database.ChangePasswordCalls);
            }
        }

        [TestMethod]
        public void Recover_StorageV2_RebindsSameLogicalKeyAndPreservesRecovery()
        {
            using (var scope = new TestScope(JasonQueryDbStorageFormatContract.ModernVersion))
            {
                var before = scope.Store.Load();
                var result = scope.Manager.Recover(scope.DatabasePath, scope.RecoveryKeyText);
                var after = scope.Store.Load();

                Assert.AreEqual(JasonQueryDbSecurityStartupState.V2Ready, result.State);
                Assert.AreEqual(JasonQueryDbKeyGenerator.ToDatabasePassword(scope.DatabaseKey), result.DatabasePassword);
                Assert.AreEqual(JasonQueryDbStorageFormatContract.ModernVersion, after.StorageFormatVersion);
                Assert.IsTrue(JasonQueryDbRecoveryMetadata.MetadataEquals(before.Recovery, after.Recovery));
                Assert.AreEqual(1, scope.ProtectCalls);
                Assert.AreEqual(0, scope.Database.ChangePasswordCalls);
            }
        }

        [TestMethod]
        public void Recover_RoutesValidatorUsingPersistedStorageFormatVersion()
        {
            using (var scope = new TestScope(JasonQueryDbStorageFormatContract.ModernVersion))
            {
                scope.Manager.Recover(scope.DatabasePath, scope.RecoveryKeyText);

                Assert.AreEqual(JasonQueryDbStorageFormatContract.ModernVersion, scope.LastFactoryStorageFormatVersion);
            }
        }

        [TestMethod]
        public void Recover_WrongRecoveryKey_RejectsWithoutMetadataSave()
        {
            using (var scope = new TestScope(JasonQueryDbStorageFormatContract.ModernVersion))
            using (var wrongRecoveryKey = JasonQueryDbRecoveryKeyCodec.Generate())
            {
                var encodedWrongKey = JasonQueryDbRecoveryKeyCodec.Encode(wrongRecoveryKey);

                Assert.ThrowsExactly<CryptographicException>
                (
                    () => scope.Manager.Recover(scope.DatabasePath, encodedWrongKey)
                );

                Assert.AreEqual(0, scope.Store.SaveCalls);
                Assert.AreEqual(scope.OriginalProtectedDatabaseKey, scope.Store.Load().ProtectedDatabaseKey);
            }
        }

        [TestMethod]
        public void Recover_MalformedRecoveryKey_RejectsWithoutMetadataSave()
        {
            using (var scope = new TestScope(JasonQueryDbStorageFormatContract.ModernVersion))
            {
                Assert.ThrowsExactly<FormatException>
                (
                    () => scope.Manager.Recover(scope.DatabasePath, "not-a-recovery-key")
                );

                Assert.AreEqual(0, scope.Store.SaveCalls);
            }
        }

        [TestMethod]
        public void Recover_TamperedRecoveryEnvelope_RejectsWithoutMetadataSave()
        {
            using (var scope = new TestScope(JasonQueryDbStorageFormatContract.ModernVersion))
            {
                var metadata = scope.Store.Load();
                var authenticationTag = Convert.FromBase64String(metadata.Recovery.AuthenticationTag);

                authenticationTag[0] ^= 0x5A;
                metadata.Recovery.AuthenticationTag = Convert.ToBase64String(authenticationTag);
                scope.Store.ReplaceWithoutCounting(metadata);

                try
                {
                    Assert.ThrowsExactly<CryptographicException>
                    (
                        () => scope.Manager.Recover(scope.DatabasePath, scope.RecoveryKeyText)
                    );

                    Assert.AreEqual(0, scope.Store.SaveCalls);
                }
                finally
                {
                    Array.Clear(authenticationTag, 0, authenticationTag.Length);
                }
            }
        }

        [TestMethod]
        public void Recover_RecoveredKeyCannotOpenDatabase_RejectsWithoutMetadataSave()
        {
            using (var scope = new TestScope(JasonQueryDbStorageFormatContract.ModernVersion))
            {
                scope.Database.CanOpenResult = false;

                Assert.ThrowsExactly<InvalidDataException>
                (
                    () => scope.Manager.Recover(scope.DatabasePath, scope.RecoveryKeyText)
                );

                Assert.AreEqual(0, scope.Store.SaveCalls);
                Assert.AreEqual(0, scope.ProtectCalls);
            }
        }

        [TestMethod]
        public void Recover_CustomPasswordMetadata_Rejects()
        {
            using (var scope = new TestScope(JasonQueryDbStorageFormatContract.ModernVersion))
            {
                scope.SetCustomPasswordMetadata();

                Assert.ThrowsExactly<InvalidOperationException>
                (
                    () => scope.Manager.Recover(scope.DatabasePath, scope.RecoveryKeyText)
                );

                Assert.AreEqual(0, scope.Store.SaveCalls);
            }
        }

        [TestMethod]
        public void Recover_NoRecoveryEnvelope_Rejects()
        {
            using (var scope = new TestScope(JasonQueryDbStorageFormatContract.ModernVersion))
            {
                var metadata = scope.Store.Load();

                metadata.Recovery = null;
                scope.Store.ReplaceWithoutCounting(metadata);

                Assert.ThrowsExactly<InvalidOperationException>
                (
                    () => scope.Manager.Recover(scope.DatabasePath, scope.RecoveryKeyText)
                );

                Assert.AreEqual(0, scope.Store.SaveCalls);
            }
        }

        [TestMethod]
        public void Recover_MissingDatabase_Rejects()
        {
            using (var scope = new TestScope(JasonQueryDbStorageFormatContract.ModernVersion))
            {
                File.Delete(scope.DatabasePath);

                Assert.ThrowsExactly<FileNotFoundException>
                (
                    () => scope.Manager.Recover(scope.DatabasePath, scope.RecoveryKeyText)
                );

                Assert.AreEqual(0, scope.Store.SaveCalls);
            }
        }

        [TestMethod]
        public void Recover_WindowsProtectFailure_LeavesMetadataUnchanged()
        {
            using (var scope = new TestScope(JasonQueryDbStorageFormatContract.ModernVersion, new ThrowingProtectKeyProtector()))
            {
                Assert.ThrowsExactly<CryptographicException>
                (
                    () => scope.Manager.Recover(scope.DatabasePath, scope.RecoveryKeyText)
                );

                Assert.AreEqual(0, scope.Store.SaveCalls);
                Assert.AreEqual(scope.OriginalProtectedDatabaseKey, scope.Store.Load().ProtectedDatabaseKey);
            }
        }

        [TestMethod]
        public void Recover_WindowsProtectRoundTripMismatch_LeavesMetadataUnchanged()
        {
            using (var scope = new TestScope(JasonQueryDbStorageFormatContract.ModernVersion, new MismatchingKeyProtector()))
            {
                Assert.ThrowsExactly<CryptographicException>
                (
                    () => scope.Manager.Recover(scope.DatabasePath, scope.RecoveryKeyText)
                );

                Assert.AreEqual(0, scope.Store.SaveCalls);
                Assert.AreEqual(scope.OriginalProtectedDatabaseKey, scope.Store.Load().ProtectedDatabaseKey);
            }
        }

        [TestMethod]
        public void Recover_MetadataChangesDuringOperation_RejectsBeforeSave()
        {
            using (var scope = new TestScope(JasonQueryDbStorageFormatContract.ModernVersion))
            {
                scope.Store.ChangeOnSecondLoad = true;

                Assert.ThrowsExactly<InvalidOperationException>
                (
                    () => scope.Manager.Recover(scope.DatabasePath, scope.RecoveryKeyText)
                );

                Assert.AreEqual(0, scope.Store.SaveCalls);
            }
        }

        private sealed class TestScope : IDisposable
        {
            public TestScope(int storageFormatVersion, IJasonQueryDbKeyProtector protector = null)
            {
                DirectoryPath = Path.Combine(Path.GetTempPath(), "JasonQuery-R4F2E5-B4-" + Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(DirectoryPath);

                DatabasePath = Path.Combine(DirectoryPath, "JasonQuery.db");
                File.WriteAllText(DatabasePath, "synthetic");

                DatabaseKey = new byte[JasonQueryDbSecurityConstants.DatabaseKeySizeBytes];

                for (var index = 0; index < DatabaseKey.Length; index++)
                {
                    DatabaseKey[index] = (byte)(index + 1);
                }

                using (var recoveryKey = JasonQueryDbRecoveryKeyCodec.Generate())
                {
                    RecoveryKeyText = JasonQueryDbRecoveryKeyCodec.Encode(recoveryKey);

                    var metadata = JasonQueryDbSecurityMetadata.CreateWindowsCurrentUser
                    (
                        Convert.ToBase64String(new byte[] { 0x11, 0x22, 0x33, 0x44 })
                    );

                    metadata.StorageFormatVersion = storageFormatVersion;

                    metadata.Recovery = JasonQueryDbRecoveryMetadata.Create
                    (
                        JasonQueryDbRecoveryEnvelopeProtector.Protect(DatabaseKey, recoveryKey)
                    );

                    OriginalProtectedDatabaseKey = metadata.ProtectedDatabaseKey;
                    Store = new InMemoryMetadataStore(metadata);
                }

                if (protector == null)
                {
                    RebindingProtector = new RebindingKeyProtector();
                    Protector = RebindingProtector;
                }
                else
                {
                    Protector = protector;
                }

                Database = new FakeMigrationDatabase(DatabaseKey);
                Manager = new JasonQueryDbRecoveryStartupManager
                (
                    Store,
                    Protector,
                    metadata =>
                    {
                        LastFactoryStorageFormatVersion = metadata.StorageFormatVersion;
                        return Database;
                    }
                );
            }

            public string DirectoryPath { get; }

            public string DatabasePath { get; }

            public byte[] DatabaseKey { get; }

            public string RecoveryKeyText { get; }

            public string OriginalProtectedDatabaseKey { get; }

            public InMemoryMetadataStore Store { get; }

            public IJasonQueryDbKeyProtector Protector { get; }

            public int ProtectCalls => RebindingProtector?.ProtectCalls ?? 0;

            private RebindingKeyProtector RebindingProtector { get; }

            public FakeMigrationDatabase Database { get; }

            public JasonQueryDbRecoveryStartupManager Manager { get; }

            public int? LastFactoryStorageFormatVersion { get; set; }

            public void SetCustomPasswordMetadata()
            {
                var salt = new byte[JasonQueryDbSecurityConstants.PasswordSaltSizeBytes];

                try
                {
                    for (var index = 0; index < salt.Length; index++)
                    {
                        salt[index] = (byte)(index + 17);
                    }

                    var metadata = JasonQueryDbSecurityMetadata.CreateCustomPassword(salt, 1000);

                    metadata.StorageFormatVersion = JasonQueryDbStorageFormatContract.ModernVersion;
                    Store.ReplaceWithoutCounting(metadata);
                }
                finally
                {
                    Array.Clear(salt, 0, salt.Length);
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

        private sealed class InMemoryMetadataStore : IJasonQueryDbSecurityMetadataStore
        {
            private JasonQueryDbSecurityMetadata _metadata;
            private int _loadCalls;

            public InMemoryMetadataStore(JasonQueryDbSecurityMetadata metadata)
            {
                _metadata = Clone(metadata);
            }

            public string MetadataFilePath => "memory://JasonQuery.security.json";

            public bool Exists => _metadata != null;

            public int SaveCalls { get; private set; }

            public bool ChangeOnSecondLoad { get; set; }

            public JasonQueryDbSecurityMetadata Load()
            {
                _loadCalls++;

                if (ChangeOnSecondLoad && _loadCalls == 2)
                {
                    _metadata.ProtectedDatabaseKey = Convert.ToBase64String(new byte[] { 0xAA, 0xBB, 0xCC });
                }

                return Clone(_metadata);
            }

            public void Save(JasonQueryDbSecurityMetadata metadata)
            {
                SaveCalls++;
                _metadata = Clone(metadata);
            }

            public void Delete()
            {
                _metadata = null;
            }

            public void ReplaceWithoutCounting(JasonQueryDbSecurityMetadata metadata)
            {
                _metadata = Clone(metadata);
                _loadCalls = 0;
            }

            private static JasonQueryDbSecurityMetadata Clone(JasonQueryDbSecurityMetadata source)
            {
                if (source == null)
                {
                    return null;
                }

                return new JasonQueryDbSecurityMetadata
                {
                    MetadataVersion = source.MetadataVersion,
                    EncryptionVersion = source.EncryptionVersion,
                    StorageFormatVersion = source.StorageFormatVersion,
                    Mode = source.Mode,
                    Protection = source.Protection,
                    ProtectedDatabaseKey = source.ProtectedDatabaseKey,
                    Kdf = source.Kdf,
                    Iterations = source.Iterations,
                    Salt = source.Salt,
                    Recovery = JasonQueryDbRecoveryMetadata.Clone(source.Recovery)
                };
            }
        }

        private sealed class RebindingKeyProtector : IJasonQueryDbKeyProtector
        {
            public int ProtectCalls { get; private set; }

            public byte[] Protect(byte[] databaseKey)
            {
                ProtectCalls++;

                var result = new byte[databaseKey.Length + 1];

                result[0] = 0xA5;
                Buffer.BlockCopy(databaseKey, 0, result, 1, databaseKey.Length);
                return result;
            }

            public byte[] Unprotect(byte[] protectedDatabaseKey)
            {
                if (protectedDatabaseKey == null || protectedDatabaseKey.Length != JasonQueryDbSecurityConstants.DatabaseKeySizeBytes + 1 || protectedDatabaseKey[0] != 0xA5)
                {
                    throw new CryptographicException("Synthetic Windows protection envelope is invalid.");
                }

                var result = new byte[JasonQueryDbSecurityConstants.DatabaseKeySizeBytes];

                Buffer.BlockCopy(protectedDatabaseKey, 1, result, 0, result.Length);
                return result;
            }
        }

        private sealed class ThrowingProtectKeyProtector : IJasonQueryDbKeyProtector
        {
            public byte[] Protect(byte[] databaseKey)
            {
                throw new CryptographicException("Synthetic Windows protect failure.");
            }

            public byte[] Unprotect(byte[] protectedDatabaseKey)
            {
                throw new NotSupportedException();
            }
        }

        private sealed class MismatchingKeyProtector : IJasonQueryDbKeyProtector
        {
            public byte[] Protect(byte[] databaseKey)
            {
                return new byte[] { 0x10, 0x20, 0x30 };
            }

            public byte[] Unprotect(byte[] protectedDatabaseKey)
            {
                return new byte[JasonQueryDbSecurityConstants.DatabaseKeySizeBytes];
            }
        }

        public sealed class FakeMigrationDatabase : IJasonQueryDbMigrationDatabase
        {
            private readonly string _expectedPassword;

            public FakeMigrationDatabase(byte[] databaseKey)
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
                throw new InvalidOperationException("Startup recovery must not rekey the database.");
            }
        }
    }
}
