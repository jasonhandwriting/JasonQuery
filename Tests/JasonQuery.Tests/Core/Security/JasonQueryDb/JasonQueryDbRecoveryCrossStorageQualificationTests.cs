using JasonQuery.Core.Security.JasonQueryDb;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace JasonQuery.Tests.Core.Security.JasonQueryDb
{
    [TestClass]
    public class JasonQueryDbRecoveryCrossStorageQualificationTests
    {
        [TestMethod]
        public void StorageMigration_PreparingCandidate_RestartsWithoutCredentialAndPreservesRecoveryEnvelope()
        {
            using (var scope = new StorageMigrationScope())
            {
                scope.AdvanceTo(JasonQueryDbStorageMigrationStage.PreparingCandidate);

                var result = scope.Coordinator.RecoverIfNeeded
                (
                    scope.ModernHelperPath,
                    (journal, paths) =>
                    {
                        scope.ResolverCalled = true;
                        throw new InvalidOperationException("PreparingCandidate must not resolve a database credential.");
                    }
                );

                Assert.AreEqual(JasonQueryDbStorageMigrationStartupRecoveryOutcome.RestartedPreparation, result.Outcome);
                Assert.IsFalse(scope.ResolverCalled);
                Assert.IsFalse(scope.JournalStore.Exists);

                var persisted = new JasonQueryDbSecurityMetadataStore(scope.MetadataPath).Load();

                Assert.AreEqual(JasonQueryDbStorageFormatContract.LegacyVersion, persisted.StorageFormatVersion.Value);
                Assert.IsTrue(JasonQueryDbRecoveryMetadata.MetadataEquals(scope.SourceMetadata.Recovery, persisted.Recovery));
                Assert.AreEqual(scope.SourceDatabaseSha256, scope.Hash(scope.DatabasePath));
                Assert.AreEqual(scope.SourceMetadataSha256, scope.Hash(scope.MetadataPath));
            }
        }

        [TestMethod]
        public void StorageMigration_CandidateReady_CredentialResolverFailure_PreservesDurableBoundary()
        {
            using (var scope = new StorageMigrationScope())
            {
                scope.AdvanceTo(JasonQueryDbStorageMigrationStage.CandidateReady);

                var sourceDatabaseSha256 = scope.Hash(scope.DatabasePath);
                var sourceMetadataSha256 = scope.Hash(scope.MetadataPath);
                var candidateSha256 = scope.Hash(scope.Paths.CandidateDatabaseFilePath);

                Assert.ThrowsExactly<CryptographicException>
                (
                    () => scope.Coordinator.RecoverIfNeeded
                    (
                        scope.ModernHelperPath,
                        (journal, paths) =>
                        {
                            scope.ResolverCalled = true;
                            throw new CryptographicException("Synthetic DPAPI loss during CandidateReady recovery.");
                        }
                    )
                );

                Assert.IsTrue(scope.ResolverCalled);
                Assert.IsTrue(scope.JournalStore.Exists);
                Assert.AreEqual(JasonQueryDbStorageMigrationStage.CandidateReady, scope.JournalStore.Load().Stage);
                Assert.AreEqual(sourceDatabaseSha256, scope.Hash(scope.DatabasePath));
                Assert.AreEqual(sourceMetadataSha256, scope.Hash(scope.MetadataPath));
                Assert.AreEqual(candidateSha256, scope.Hash(scope.Paths.CandidateDatabaseFilePath));
                Assert.IsFalse(File.Exists(scope.Paths.LockFilePath));

                var persisted = new JasonQueryDbSecurityMetadataStore(scope.MetadataPath).Load();

                Assert.IsTrue(JasonQueryDbRecoveryMetadata.MetadataEquals(scope.SourceMetadata.Recovery, persisted.Recovery));
            }
        }

        [TestMethod]
        public void StorageMigration_ReplacePrepared_CompletesWithoutCredentialAndPreservesRecoveryEnvelope()
        {
            using (var scope = new StorageMigrationScope())
            {
                scope.AdvanceTo(JasonQueryDbStorageMigrationStage.ReplacePrepared);

                var result = scope.Coordinator.RecoverIfNeeded
                (
                    scope.ModernHelperPath,
                    (journal, paths) =>
                    {
                        scope.ResolverCalled = true;
                        throw new InvalidOperationException("ReplacePrepared must not resolve a database credential.");
                    }
                );

                Assert.AreEqual(JasonQueryDbStorageMigrationStartupRecoveryOutcome.CompletedForwardRecovery, result.Outcome);
                Assert.IsFalse(scope.ResolverCalled);
                scope.AssertCompleted();

                var persisted = new JasonQueryDbSecurityMetadataStore(scope.MetadataPath).Load();

                Assert.AreEqual(JasonQueryDbStorageFormatContract.ModernVersion, persisted.StorageFormatVersion.Value);
                Assert.IsTrue(JasonQueryDbRecoveryMetadata.MetadataEquals(scope.SourceMetadata.Recovery, persisted.Recovery));
            }
        }

        [TestMethod]
        public void StorageMigration_MetadataCommitted_CleansWithoutCredentialAndPreservesRecoveryEnvelope()
        {
            using (var scope = new StorageMigrationScope())
            {
                scope.AdvanceTo(JasonQueryDbStorageMigrationStage.ReplacePrepared);
                scope.SimulateDatabaseReplace();
                scope.AdvanceJournalStage(JasonQueryDbStorageMigrationStage.DatabaseReplaced);
                scope.SimulateMetadataReplace();
                scope.AdvanceJournalStage(JasonQueryDbStorageMigrationStage.MetadataCommitted);

                var result = scope.Coordinator.RecoverIfNeeded
                (
                    scope.ModernHelperPath,
                    (journal, paths) =>
                    {
                        scope.ResolverCalled = true;
                        throw new InvalidOperationException("MetadataCommitted cleanup must not resolve a database credential.");
                    }
                );

                Assert.AreEqual(JasonQueryDbStorageMigrationStartupRecoveryOutcome.CompletedForwardRecovery, result.Outcome);
                Assert.IsFalse(scope.ResolverCalled);
                scope.AssertCompleted();

                var persisted = new JasonQueryDbSecurityMetadataStore(scope.MetadataPath).Load();

                Assert.AreEqual(JasonQueryDbStorageFormatContract.ModernVersion, persisted.StorageFormatVersion.Value);
                Assert.IsTrue(JasonQueryDbRecoveryMetadata.MetadataEquals(scope.SourceMetadata.Recovery, persisted.Recovery));
            }
        }

        [TestMethod]
        public void StorageMigration_TargetMetadata_PreservesRecoveryEnvelopeAndChangesOnlyStorageVersion()
        {
            using (var recoveryKey = JasonQueryDbRecoveryKeyCodec.Generate())
            {
                var databaseKey = CreateDatabaseKey();

                try
                {
                    var source = CreateWindowsMetadata
                    (
                        JasonQueryDbStorageFormatContract.LegacyVersion,
                        databaseKey,
                        recoveryKey
                    );

                    var target = JasonQueryDbStorageMigrationMetadataContract.CreateTargetMetadata(source);

                    Assert.AreEqual(JasonQueryDbStorageFormatContract.LegacyVersion, source.StorageFormatVersion.Value);
                    Assert.AreEqual(JasonQueryDbStorageFormatContract.ModernVersion, target.StorageFormatVersion.Value);
                    Assert.AreEqual(source.Mode, target.Mode);
                    Assert.AreEqual(source.Protection, target.Protection);
                    Assert.AreEqual(source.ProtectedDatabaseKey, target.ProtectedDatabaseKey);
                    Assert.IsTrue(JasonQueryDbRecoveryMetadata.MetadataEquals(source.Recovery, target.Recovery));
                }
                finally
                {
                    Array.Clear(databaseKey, 0, databaseKey.Length);
                }
            }
        }

        [TestMethod]
        public void StorageMigration_JournalRoundTrip_PreservesSourceAndTargetRecoveryEnvelope()
        {
            using (var scope = new StorageMigrationScope())
            {
                scope.AdvanceTo(JasonQueryDbStorageMigrationStage.CandidateReady);

                var loaded = scope.JournalStore.Load();

                Assert.IsNotNull(loaded.SourceMetadata.Recovery);
                Assert.IsNotNull(loaded.TargetMetadata.Recovery);
                Assert.IsTrue(JasonQueryDbRecoveryMetadata.MetadataEquals(scope.SourceMetadata.Recovery, loaded.SourceMetadata.Recovery));
                Assert.IsTrue(JasonQueryDbRecoveryMetadata.MetadataEquals(scope.TargetMetadata.Recovery, loaded.TargetMetadata.Recovery));
                Assert.AreEqual(JasonQueryDbStorageFormatContract.LegacyVersion, loaded.SourceMetadata.StorageFormatVersion.Value);
                Assert.AreEqual(JasonQueryDbStorageFormatContract.ModernVersion, loaded.TargetMetadata.StorageFormatVersion.Value);
            }
        }

        [TestMethod]
        public void SecurityTransition_DpapiLoss_StorageV1_FailsClosedAndPreservesDurableState()
        {
            using (var scope = new SecurityTransitionScope(JasonQueryDbStorageFormatContract.LegacyVersion))
            {
                scope.CreateInterruptedJournal();

                var databaseSha256 = Hash(scope.DatabasePath);
                var metadataSha256 = Hash(scope.MetadataPath);
                var journalSha256 = Hash(scope.JournalPath);
                var candidateSha256 = Hash(scope.CandidatePath);
                var backupSha256 = Hash(scope.BackupPath);

                Assert.ThrowsExactly<CryptographicException>
                (
                    () => scope.CreateFailingManager().RecoverInterruptedChangeIfNeeded(scope.DatabasePath)
                );

                Assert.AreEqual(JasonQueryDbStorageFormatContract.LegacyVersion, scope.LastResolverStorageFormatVersion.Value);
                Assert.AreEqual(0, scope.Database.CanOpenCalls);
                Assert.AreEqual(databaseSha256, Hash(scope.DatabasePath));
                Assert.AreEqual(metadataSha256, Hash(scope.MetadataPath));
                Assert.AreEqual(journalSha256, Hash(scope.JournalPath));
                Assert.AreEqual(candidateSha256, Hash(scope.CandidatePath));
                Assert.AreEqual(backupSha256, Hash(scope.BackupPath));
                Assert.IsTrue(scope.JournalStore.Exists);

                var persisted = scope.MetadataStore.Load();

                Assert.IsTrue(JasonQueryDbRecoveryMetadata.MetadataEquals(scope.SourceMetadata.Recovery, persisted.Recovery));
            }
        }

        [TestMethod]
        public void SecurityTransition_DpapiLoss_StorageV2_RoutesOnlyByPersistedVersionAndFailsClosed()
        {
            using (var scope = new SecurityTransitionScope(JasonQueryDbStorageFormatContract.ModernVersion))
            {
                scope.CreateInterruptedJournal();

                var databaseSha256 = Hash(scope.DatabasePath);
                var metadataSha256 = Hash(scope.MetadataPath);
                var journalSha256 = Hash(scope.JournalPath);

                Assert.ThrowsExactly<CryptographicException>
                (
                    () => scope.CreateFailingManager().RecoverInterruptedChangeIfNeeded(scope.DatabasePath)
                );

                Assert.AreEqual(JasonQueryDbStorageFormatContract.ModernVersion, scope.LastResolverStorageFormatVersion.Value);
                Assert.AreEqual(1, scope.ResolverCalls);
                Assert.AreEqual(0, scope.Database.CanOpenCalls);
                Assert.AreEqual(databaseSha256, Hash(scope.DatabasePath));
                Assert.AreEqual(metadataSha256, Hash(scope.MetadataPath));
                Assert.AreEqual(journalSha256, Hash(scope.JournalPath));
                Assert.IsTrue(scope.JournalStore.Exists);
            }
        }

        [TestMethod]
        public void SecurityTransition_StorageVersionMismatch_FailsBeforeProviderResolution()
        {
            using (var scope = new SecurityTransitionScope(JasonQueryDbStorageFormatContract.LegacyVersion))
            {
                scope.CreateInterruptedJournal(JasonQueryDbStorageFormatContract.ModernVersion);

                var databaseSha256 = Hash(scope.DatabasePath);
                var metadataSha256 = Hash(scope.MetadataPath);
                var journalSha256 = Hash(scope.JournalPath);

                Assert.ThrowsExactly<InvalidDataException>
                (
                    () => scope.CreateFailingManager().RecoverInterruptedChangeIfNeeded(scope.DatabasePath)
                );

                Assert.AreEqual(0, scope.ResolverCalls);
                Assert.AreEqual(0, scope.Database.CanOpenCalls);
                Assert.AreEqual(databaseSha256, Hash(scope.DatabasePath));
                Assert.AreEqual(metadataSha256, Hash(scope.MetadataPath));
                Assert.AreEqual(journalSha256, Hash(scope.JournalPath));
                Assert.IsTrue(scope.JournalStore.Exists);
            }
        }

        private static JasonQueryDbSecurityMetadata CreateWindowsMetadata(int storageFormatVersion, byte[] databaseKey, JasonQueryDbRecoveryKeyMaterial recoveryKey)
        {
            var metadata = JasonQueryDbSecurityMetadata.CreateWindowsCurrentUser
            (
                Convert.ToBase64String(new byte[] { 0x01, 0x03, 0x05, 0x07 })
            );

            metadata.StorageFormatVersion = storageFormatVersion;

            metadata.Recovery = JasonQueryDbRecoveryMetadata.Create
            (
                JasonQueryDbRecoveryEnvelopeProtector.Protect(databaseKey, recoveryKey)
            );

            return metadata;
        }

        private static byte[] CreateDatabaseKey()
        {
            var result = new byte[JasonQueryDbSecurityConstants.DatabaseKeySizeBytes];

            for (var index = 0; index < result.Length; index++)
            {
                result[index] = (byte)(index + 1);
            }

            return result;
        }

        private static string Hash(string filePath)
        {
            return JasonQueryDbStorageMigrationFileIntegrity.ComputeSha256(filePath);
        }

        private sealed class StorageMigrationScope : IDisposable
        {
            private readonly byte[] _sourceDatabaseBytes;
            private readonly byte[] _candidateDatabaseBytes;
            private readonly byte[] _databaseKey;
            private readonly JasonQueryDbRecoveryKeyMaterial _recoveryKey;

            public StorageMigrationScope()
            {
                DirectoryPath = Path.Combine(Path.GetTempPath(), "JasonQuery-R4F2E5-B5A-Storage-" + Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(DirectoryPath);

                DatabasePath = Path.Combine(DirectoryPath, "JasonQuery.db");
                MetadataPath = Path.Combine(DirectoryPath, JasonQueryDbSecurityConstants.MetadataFileName);
                ModernHelperPath = Path.Combine(DirectoryPath, "JasonQuery.ModernDbMigration.exe");
                OperationId = Guid.NewGuid().ToString("N");

                _sourceDatabaseBytes = Encoding.ASCII.GetBytes("B5A-SOURCE-DATABASE");
                _candidateDatabaseBytes = Encoding.ASCII.GetBytes("B5A-CANDIDATE-DATABASE");
                _databaseKey = CreateDatabaseKey();
                _recoveryKey = JasonQueryDbRecoveryKeyCodec.Generate();

                File.WriteAllBytes(DatabasePath, _sourceDatabaseBytes);

                SourceMetadata = CreateWindowsMetadata
                (
                    JasonQueryDbStorageFormatContract.LegacyVersion,
                    _databaseKey,
                    _recoveryKey
                );

                new JasonQueryDbSecurityMetadataStore(MetadataPath).Save(SourceMetadata);

                TargetMetadata = JasonQueryDbStorageMigrationMetadataContract.CreateTargetMetadata(SourceMetadata);
                SourceDatabaseSha256 = Hash(DatabasePath);
                SourceMetadataSha256 = Hash(MetadataPath);
                CandidateDatabaseSha256 = JasonQueryDbStorageMigrationFileIntegrity.ComputeSha256(_candidateDatabaseBytes);
                TargetMetadataSha256 = JasonQueryDbStorageMigrationMetadataContract.ComputeSerializedSha256(TargetMetadata);
                Paths = JasonQueryDbStorageMigrationArtifactPaths.Create(DatabasePath, MetadataPath, OperationId);
                JournalStore = new JasonQueryDbStorageMigrationJournalStore(DatabasePath);
                Recovery = new JasonQueryDbStorageMigrationRecovery(DatabasePath, MetadataPath);
                Coordinator = new JasonQueryDbStorageMigrationStartupRecoveryCoordinator(DatabasePath, MetadataPath, ValidateCandidate);
            }

            public string DirectoryPath { get; }

            public string DatabasePath { get; }

            public string MetadataPath { get; }

            public string ModernHelperPath { get; }

            public string OperationId { get; }

            public string SourceDatabaseSha256 { get; }

            public string SourceMetadataSha256 { get; }

            public string CandidateDatabaseSha256 { get; }

            public string TargetMetadataSha256 { get; }

            public JasonQueryDbSecurityMetadata SourceMetadata { get; }

            public JasonQueryDbSecurityMetadata TargetMetadata { get; }

            public JasonQueryDbStorageMigrationArtifactPaths Paths { get; }

            public JasonQueryDbStorageMigrationJournalStore JournalStore { get; }

            public JasonQueryDbStorageMigrationRecovery Recovery { get; }

            public JasonQueryDbStorageMigrationStartupRecoveryCoordinator Coordinator { get; }

            public bool ResolverCalled { get; set; }

            public string Hash(string filePath)
            {
                return JasonQueryDbStorageMigrationFileIntegrity.ComputeSha256(filePath);
            }

            public void AdvanceTo(JasonQueryDbStorageMigrationStage targetStage)
            {
                SaveStage(JasonQueryDbStorageMigrationStage.PreparingCandidate);

                if (targetStage == JasonQueryDbStorageMigrationStage.PreparingCandidate)
                {
                    return;
                }

                File.WriteAllBytes(Paths.CandidateDatabaseFilePath, _candidateDatabaseBytes);
                SaveStage(JasonQueryDbStorageMigrationStage.CandidateReady);

                if (targetStage == JasonQueryDbStorageMigrationStage.CandidateReady)
                {
                    return;
                }

                JasonQueryDbStorageMigrationMetadataContract.WriteDurablyCreateNew
                (
                    Paths.MetadataTemporaryFilePath,
                    TargetMetadata
                );

                SaveStage(JasonQueryDbStorageMigrationStage.ReplacePrepared);
            }

            public void AdvanceJournalStage(JasonQueryDbStorageMigrationStage stage)
            {
                SaveStage(stage);
            }

            public void SimulateDatabaseReplace()
            {
                File.Replace
                (
                    Paths.CandidateDatabaseFilePath,
                    Paths.DatabaseFilePath,
                    Paths.DatabaseBackupFilePath
                );
            }

            public void SimulateMetadataReplace()
            {
                File.Replace
                (
                    Paths.MetadataTemporaryFilePath,
                    Paths.MetadataFilePath,
                    Paths.MetadataBackupFilePath
                );
            }

            public void AssertCompleted()
            {
                Assert.IsFalse(JournalStore.Exists);
                Assert.AreEqual(CandidateDatabaseSha256, Hash(DatabasePath));
                Assert.AreEqual(TargetMetadataSha256, Hash(MetadataPath));
                Assert.IsFalse(File.Exists(Paths.CandidateDatabaseFilePath));
                Assert.IsFalse(File.Exists(Paths.DatabaseBackupFilePath));
                Assert.IsFalse(File.Exists(Paths.MetadataTemporaryFilePath));
                Assert.IsFalse(File.Exists(Paths.MetadataBackupFilePath));
                Assert.IsFalse(File.Exists(Paths.LockFilePath));
                Assert.AreEqual(JasonQueryDbStorageMigrationRecoveryAction.NoRecoveryRequired, Recovery.Evaluate().Action);
            }

            private JasonQueryDbStorageV2ReadOnlyValidationResult ValidateCandidate(string modernHelperFilePath, string candidateDatabaseFilePath, byte[] passwordUtf8)
            {
                Assert.AreEqual(ModernHelperPath, modernHelperFilePath);
                Assert.IsNotNull(passwordUtf8);
                Assert.IsNotEmpty(passwordUtf8);

                return new JasonQueryDbStorageV2ReadOnlyValidationResult
                (
                    Hash(candidateDatabaseFilePath)
                );
            }

            private void SaveStage(JasonQueryDbStorageMigrationStage stage)
            {
                JournalStore.Save
                (
                    new JasonQueryDbStorageMigrationJournal
                    {
                        JournalVersion = JasonQueryDbStorageMigrationJournal.CurrentVersion,
                        OperationId = OperationId,
                        Stage = stage,
                        ProtocolVersion = JasonQueryDbStorageMigrationProtocolContract.CurrentVersion,
                        CandidateWriterProtocolVersion = JasonQueryDbStorageV2CandidateWriterProtocol.CurrentVersion,
                        SourceStorageFormatVersion = JasonQueryDbStorageFormatContract.LegacyVersion,
                        TargetStorageFormatVersion = JasonQueryDbStorageFormatContract.ModernVersion,
                        SourceDatabaseSha256 = SourceDatabaseSha256,
                        CandidateDatabaseSha256 = stage == JasonQueryDbStorageMigrationStage.PreparingCandidate ? null : CandidateDatabaseSha256,
                        SourceMetadataSha256 = SourceMetadataSha256,
                        TargetMetadataSha256 = TargetMetadataSha256,
                        SourceMetadata = SourceMetadata,
                        TargetMetadata = TargetMetadata
                    }
                );
            }

            public void Dispose()
            {
                Array.Clear(_sourceDatabaseBytes, 0, _sourceDatabaseBytes.Length);
                Array.Clear(_candidateDatabaseBytes, 0, _candidateDatabaseBytes.Length);
                Array.Clear(_databaseKey, 0, _databaseKey.Length);
                _recoveryKey.Dispose();

                if (Directory.Exists(DirectoryPath))
                {
                    Directory.Delete(DirectoryPath, true);
                }
            }
        }

        private sealed class SecurityTransitionScope : IDisposable
        {
            private readonly PassThroughKeyProtector _journalProtector;
            private readonly byte[] _databaseKey;
            private readonly JasonQueryDbRecoveryKeyMaterial _recoveryKey;

            public SecurityTransitionScope(int storageFormatVersion)
            {
                DirectoryPath = Path.Combine(Path.GetTempPath(), "JasonQuery-R4F2E5-B5A-Transition-" + Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(DirectoryPath);

                DatabasePath = Path.Combine(DirectoryPath, "JasonQuery.db");
                MetadataPath = Path.Combine(DirectoryPath, JasonQueryDbSecurityConstants.MetadataFileName);
                JournalPath = JasonQueryDbSecurityTransitionManager.GetJournalFilePath(DatabasePath);
                CandidatePath = JasonQueryDbSecurityTransitionManager.GetCandidateFilePath(DatabasePath);
                BackupPath = JasonQueryDbSecurityTransitionManager.GetBackupFilePath(DatabasePath);

                _journalProtector = new PassThroughKeyProtector();
                _databaseKey = CreateDatabaseKey();
                _recoveryKey = JasonQueryDbRecoveryKeyCodec.Generate();

                SourcePassword = Convert.ToBase64String(_databaseKey);

                SourceMetadata = CreateWindowsMetadata
                (
                    storageFormatVersion,
                    _databaseKey,
                    _recoveryKey
                );

                MetadataStore = new JasonQueryDbSecurityMetadataStore(MetadataPath);
                MetadataStore.Save(SourceMetadata);

                Database = new FakeMigrationDatabase();
                JournalStore = new JasonQueryDbSecurityTransitionJournalStore(JournalPath);

                File.WriteAllText(DatabasePath, SourcePassword, Encoding.UTF8);
                File.WriteAllText(CandidatePath, "B5A-CANDIDATE", Encoding.UTF8);
                File.WriteAllText(BackupPath, "B5A-BACKUP", Encoding.UTF8);
            }

            public string DirectoryPath { get; }

            public string DatabasePath { get; }

            public string MetadataPath { get; }

            public string JournalPath { get; }

            public string CandidatePath { get; }

            public string BackupPath { get; }

            public string SourcePassword { get; }

            public JasonQueryDbSecurityMetadata SourceMetadata { get; }

            public JasonQueryDbSecurityMetadataStore MetadataStore { get; }

            public JasonQueryDbSecurityTransitionJournalStore JournalStore { get; }

            public FakeMigrationDatabase Database { get; }

            public int ResolverCalls { get; private set; }

            public int? LastResolverStorageFormatVersion { get; private set; }

            public void CreateInterruptedJournal(int? targetStorageFormatVersion = null)
            {
                var targetSalt = new byte[JasonQueryDbSecurityConstants.PasswordSaltSizeBytes];
                var sourcePasswordBytes = Encoding.UTF8.GetBytes(SourcePassword);
                var targetPassword = "B5A-TARGET-PASSWORD";
                var targetPasswordBytes = Encoding.UTF8.GetBytes(targetPassword);

                try
                {
                    for (var index = 0; index < targetSalt.Length; index++)
                    {
                        targetSalt[index] = (byte)(0x41 + (index % 17));
                    }

                    var targetMetadata = JasonQueryDbSecurityMetadata.CreateCustomPassword
                    (
                        targetSalt,
                        JasonQueryDbSecurityConstants.DefaultPbkdf2Iterations
                    );

                    targetMetadata.StorageFormatVersion = targetStorageFormatVersion ?? SourceMetadata.StorageFormatVersion;

                    var protectedSource = _journalProtector.Protect(sourcePasswordBytes);
                    var protectedTarget = _journalProtector.Protect(targetPasswordBytes);

                    try
                    {
                        JournalStore.Save
                        (
                            new JasonQueryDbSecurityTransitionJournal
                            {
                                TransitionVersion = JasonQueryDbSecurityTransitionJournal.CurrentVersion,
                                SourceMetadata = SourceMetadata,
                                TargetMetadata = targetMetadata,
                                ProtectedSourceDatabasePassword = Convert.ToBase64String(protectedSource),
                                ProtectedTargetDatabasePassword = Convert.ToBase64String(protectedTarget)
                            }
                        );
                    }
                    finally
                    {
                        Array.Clear(protectedSource, 0, protectedSource.Length);
                        Array.Clear(protectedTarget, 0, protectedTarget.Length);
                    }
                }
                finally
                {
                    Array.Clear(targetSalt, 0, targetSalt.Length);
                    Array.Clear(sourcePasswordBytes, 0, sourcePasswordBytes.Length);
                    Array.Clear(targetPasswordBytes, 0, targetPasswordBytes.Length);
                }
            }

            public JasonQueryDbSecurityTransitionManager CreateFailingManager()
            {
                return new JasonQueryDbSecurityTransitionManager
                (
                    MetadataStore,
                    new FailingUnprotectKeyProtector(),
                    metadata =>
                    {
                        ResolverCalls++;
                        LastResolverStorageFormatVersion = metadata.StorageFormatVersion;
                        return Database;
                    },
                    JournalStore
                );
            }

            public void Dispose()
            {
                Array.Clear(_databaseKey, 0, _databaseKey.Length);
                _recoveryKey.Dispose();

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

        private sealed class FailingUnprotectKeyProtector : IJasonQueryDbKeyProtector
        {
            public byte[] Protect(byte[] databaseKey)
            {
                throw new NotSupportedException();
            }

            public byte[] Unprotect(byte[] protectedDatabaseKey)
            {
                throw new CryptographicException("Synthetic DPAPI CurrentUser material is unavailable.");
            }
        }

        private sealed class FakeMigrationDatabase : IJasonQueryDbMigrationDatabase
        {
            public int CanOpenCalls { get; private set; }

            public bool CanOpen(string databaseFilePath, string databasePassword)
            {
                CanOpenCalls++;

                return File.Exists(databaseFilePath) && string.Equals(File.ReadAllText(databaseFilePath, Encoding.UTF8), databasePassword, StringComparison.Ordinal);
            }

            public void CreateVerifiedCopy(string sourceDatabaseFilePath, string destinationDatabaseFilePath, string currentDatabasePassword)
            {
                throw new NotSupportedException();
            }

            public void ChangePassword(string databaseFilePath, string currentDatabasePassword, string newDatabasePassword)
            {
                throw new NotSupportedException();
            }
        }
    }
}
