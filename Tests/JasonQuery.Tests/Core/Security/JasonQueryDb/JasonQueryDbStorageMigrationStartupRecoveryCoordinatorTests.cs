using JasonQuery.Core.Security.JasonQueryDb;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Text;

namespace JasonQuery.Tests.Core.Security.JasonQueryDb
{
    [TestClass]
    public class JasonQueryDbStorageMigrationStartupRecoveryCoordinatorTests
    {
        [TestMethod]
        public void Constructor_EmptyDatabasePath_Throws()
        {
            Assert.ThrowsExactly<ArgumentException>
            (
                () => new JasonQueryDbStorageMigrationStartupRecoveryCoordinator
                (
                    " ",
                    Path.Combine(Path.GetTempPath(), "JasonQuery.security.json")
                )
            );
        }

        [TestMethod]
        public void Constructor_EmptyMetadataPath_Throws()
        {
            Assert.ThrowsExactly<ArgumentException>
            (
                () => new JasonQueryDbStorageMigrationStartupRecoveryCoordinator
                (
                    Path.Combine(Path.GetTempPath(), "JasonQuery.db"),
                    " "
                )
            );
        }

        [TestMethod]
        public void RecoverIfNeeded_NoJournalAndNoArtifacts_ReturnsNoRecoveryRequired()
        {
            using (var scope = new TestScope())
            {
                var result = scope.Coordinator.RecoverIfNeeded
                (
                    scope.ModernHelperPath,
                    null
                );

                Assert.AreEqual
                (
                    JasonQueryDbStorageMigrationStartupRecoveryOutcome.NoRecoveryRequired,
                    result.Outcome
                );

                Assert.IsNull(result.OperationId);
                Assert.IsFalse(File.Exists(scope.Paths.LockFilePath));
            }
        }

        [TestMethod]
        public void RecoverIfNeeded_NoJournalWithStaleLock_FailsClosedAndPreservesLock()
        {
            using (var scope = new TestScope())
            {
                File.WriteAllText(scope.Paths.LockFilePath, "STALE");

                Assert.ThrowsExactly<IOException>
                (
                    () => scope.Coordinator.RecoverIfNeeded
                    (
                        scope.ModernHelperPath,
                        null
                    )
                );

                Assert.IsTrue(File.Exists(scope.Paths.LockFilePath));
                Assert.IsFalse(scope.JournalStore.Exists);
            }
        }

        [TestMethod]
        public void RecoverIfNeeded_PreparingCandidate_DeletesUntrustedCandidateAndRestarts()
        {
            using (var scope = new TestScope())
            {
                scope.AdvanceTo(JasonQueryDbStorageMigrationStage.PreparingCandidate);
                File.WriteAllBytes(scope.Paths.CandidateDatabaseFilePath, new byte[] { 9, 8, 7 });
                File.WriteAllText(scope.Paths.CandidateDatabaseFilePath + "-wal", "PARTIAL");

                var result = scope.Coordinator.RecoverIfNeeded
                (
                    scope.ModernHelperPath,
                    null
                );

                Assert.AreEqual
                (
                    JasonQueryDbStorageMigrationStartupRecoveryOutcome.RestartedPreparation,
                    result.Outcome
                );

                Assert.AreEqual(scope.OperationId, result.OperationId);
                Assert.IsFalse(scope.JournalStore.Exists);
                Assert.IsFalse(File.Exists(scope.Paths.CandidateDatabaseFilePath));
                Assert.IsFalse(File.Exists(scope.Paths.CandidateDatabaseFilePath + "-wal"));
                Assert.IsFalse(File.Exists(scope.Paths.LockFilePath));
                Assert.AreEqual(scope.SourceDatabaseSha256, scope.Hash(scope.DatabasePath));
                Assert.AreEqual(scope.SourceMetadataSha256, scope.Hash(scope.MetadataPath));
            }
        }

        [TestMethod]
        public void RecoverIfNeeded_PreparingCandidate_SourceHashMismatch_FailsClosed()
        {
            using (var scope = new TestScope())
            {
                scope.AdvanceTo(JasonQueryDbStorageMigrationStage.PreparingCandidate);
                File.AppendAllText(scope.DatabasePath, "MUTATED");

                Assert.ThrowsExactly<InvalidDataException>
                (
                    () => scope.Coordinator.RecoverIfNeeded
                    (
                        scope.ModernHelperPath,
                        null
                    )
                );

                Assert.IsTrue(scope.JournalStore.Exists);

                Assert.AreEqual
                (
                    JasonQueryDbStorageMigrationStage.PreparingCandidate,
                    scope.JournalStore.Load().Stage
                );

                Assert.IsFalse(File.Exists(scope.Paths.LockFilePath));
            }
        }

        [TestMethod]
        public void RecoverIfNeeded_PreparingCandidate_UnexpectedBackup_FailsClosed()
        {
            using (var scope = new TestScope())
            {
                scope.AdvanceTo(JasonQueryDbStorageMigrationStage.PreparingCandidate);
                File.WriteAllText(scope.Paths.DatabaseBackupFilePath, "UNEXPECTED");

                Assert.ThrowsExactly<InvalidDataException>
                (
                    () => scope.Coordinator.RecoverIfNeeded
                    (
                        scope.ModernHelperPath,
                        null
                    )
                );

                Assert.IsTrue(File.Exists(scope.Paths.DatabaseBackupFilePath));
                Assert.IsTrue(scope.JournalStore.Exists);
                Assert.IsFalse(File.Exists(scope.Paths.LockFilePath));
            }
        }

        [TestMethod]
        public void RecoverIfNeeded_CandidateReadyWithoutResolver_FailsAndPreservesCandidateReady()
        {
            using (var scope = new TestScope())
            {
                scope.AdvanceTo(JasonQueryDbStorageMigrationStage.CandidateReady);

                Assert.ThrowsExactly<InvalidOperationException>
                (
                    () => scope.Coordinator.RecoverIfNeeded
                    (
                        scope.ModernHelperPath,
                        null
                    )
                );

                Assert.AreEqual
                (
                    JasonQueryDbStorageMigrationStage.CandidateReady,
                    scope.JournalStore.Load().Stage
                );

                Assert.IsTrue(File.Exists(scope.Paths.CandidateDatabaseFilePath));
                Assert.IsFalse(File.Exists(scope.Paths.LockFilePath));
            }
        }

        [TestMethod]
        public void RecoverIfNeeded_CandidateReadyResolverReturnsNull_FailsAndPreservesCandidateReady()
        {
            using (var scope = new TestScope())
            {
                scope.AdvanceTo(JasonQueryDbStorageMigrationStage.CandidateReady);

                Assert.ThrowsExactly<ArgumentException>
                (
                    () => scope.Coordinator.RecoverIfNeeded
                    (
                        scope.ModernHelperPath,
                        (journal, paths) => null
                    )
                );

                Assert.AreEqual
                (
                    JasonQueryDbStorageMigrationStage.CandidateReady,
                    scope.JournalStore.Load().Stage
                );

                Assert.IsTrue(File.Exists(scope.Paths.CandidateDatabaseFilePath));
            }
        }

        [TestMethod]
        public void RecoverIfNeeded_CandidateReadySuccess_CompletesForwardRecovery()
        {
            using (var scope = new TestScope())
            {
                scope.AdvanceTo(JasonQueryDbStorageMigrationStage.CandidateReady);

                var result = scope.RecoverWithCredential();

                Assert.AreEqual
                (
                    JasonQueryDbStorageMigrationStartupRecoveryOutcome.CompletedForwardRecovery,
                    result.Outcome
                );

                Assert.AreEqual(scope.OperationId, result.OperationId);
                scope.AssertCompleted();
            }
        }

        [TestMethod]
        public void RecoverIfNeeded_CandidateReadySuccess_ClearsResolverOwnedCredential()
        {
            using (var scope = new TestScope())
            {
                scope.AdvanceTo(JasonQueryDbStorageMigrationStage.CandidateReady);

                scope.RecoverWithCredential();

                Assert.IsNotNull(scope.LastResolvedCredential);

                foreach (var value in scope.LastResolvedCredential)
                {
                    Assert.AreEqual((byte)0, value);
                }
            }
        }

        [TestMethod]
        public void RecoverIfNeeded_CandidateReadyValidationFailure_PreservesCandidateReady()
        {
            using (var scope = new TestScope())
            {
                scope.AdvanceTo(JasonQueryDbStorageMigrationStage.CandidateReady);
                scope.ValidationFailure = true;

                Assert.ThrowsExactly<InvalidDataException>
                (
                    () => scope.RecoverWithCredential()
                );

                Assert.AreEqual
                (
                    JasonQueryDbStorageMigrationStage.CandidateReady,
                    scope.JournalStore.Load().Stage
                );

                Assert.IsTrue(File.Exists(scope.Paths.CandidateDatabaseFilePath));
                Assert.IsFalse(File.Exists(scope.Paths.MetadataTemporaryFilePath));
                Assert.IsFalse(File.Exists(scope.Paths.LockFilePath));
            }
        }

        [TestMethod]
        public void RecoverIfNeeded_ReplacePreparedStateA_CompletesWithoutCredentialResolver()
        {
            using (var scope = new TestScope())
            {
                scope.AdvanceTo(JasonQueryDbStorageMigrationStage.ReplacePrepared);

                var result = scope.Coordinator.RecoverIfNeeded
                (
                    scope.ModernHelperPath,
                    (journal, paths) =>
                    {
                        scope.ResolverCalled = true;
                        return scope.CreateCredential();
                    }
                );

                Assert.IsFalse(scope.ResolverCalled);

                Assert.AreEqual
                (
                    JasonQueryDbStorageMigrationStartupRecoveryOutcome.CompletedForwardRecovery,
                    result.Outcome
                );

                scope.AssertCompleted();
            }
        }

        [TestMethod]
        public void RecoverIfNeeded_ReplacePreparedStateB_RecognizesDatabaseReplacement()
        {
            using (var scope = new TestScope())
            {
                scope.AdvanceTo(JasonQueryDbStorageMigrationStage.ReplacePrepared);
                scope.SimulateDatabaseReplace();

                Assert.AreEqual
                (
                    JasonQueryDbStorageMigrationRecoveryAction.RecognizeDatabaseReplaced,
                    scope.Recovery.Evaluate().Action
                );

                var result = scope.Coordinator.RecoverIfNeeded
                (
                    scope.ModernHelperPath,
                    null
                );

                Assert.AreEqual
                (
                    JasonQueryDbStorageMigrationStartupRecoveryOutcome.CompletedForwardRecovery,
                    result.Outcome
                );

                scope.AssertCompleted();
            }
        }

        [TestMethod]
        public void RecoverIfNeeded_DatabaseReplacedSourceMetadata_Completes()
        {
            using (var scope = new TestScope())
            {
                scope.AdvanceTo(JasonQueryDbStorageMigrationStage.ReplacePrepared);
                scope.SimulateDatabaseReplace();
                scope.AdvanceJournalStage(JasonQueryDbStorageMigrationStage.DatabaseReplaced);

                Assert.AreEqual
                (
                    JasonQueryDbStorageMigrationRecoveryAction.CommitMetadata,
                    scope.Recovery.Evaluate().Action
                );

                scope.Coordinator.RecoverIfNeeded(scope.ModernHelperPath, null);
                scope.AssertCompleted();
            }
        }

        [TestMethod]
        public void RecoverIfNeeded_DatabaseReplacedTargetMetadata_Completes()
        {
            using (var scope = new TestScope())
            {
                scope.AdvanceTo(JasonQueryDbStorageMigrationStage.ReplacePrepared);
                scope.SimulateDatabaseReplace();
                scope.AdvanceJournalStage(JasonQueryDbStorageMigrationStage.DatabaseReplaced);
                scope.SimulateMetadataReplace();

                Assert.AreEqual
                (
                    JasonQueryDbStorageMigrationRecoveryAction.RecognizeMetadataCommitted,
                    scope.Recovery.Evaluate().Action
                );

                scope.Coordinator.RecoverIfNeeded(scope.ModernHelperPath, null);
                scope.AssertCompleted();
            }
        }

        [TestMethod]
        public void RecoverIfNeeded_MetadataCommitted_CleansAndCompletes()
        {
            using (var scope = new TestScope())
            {
                scope.AdvanceTo(JasonQueryDbStorageMigrationStage.ReplacePrepared);
                scope.SimulateDatabaseReplace();
                scope.AdvanceJournalStage(JasonQueryDbStorageMigrationStage.DatabaseReplaced);
                scope.SimulateMetadataReplace();
                scope.AdvanceJournalStage(JasonQueryDbStorageMigrationStage.MetadataCommitted);

                Assert.AreEqual
                (
                    JasonQueryDbStorageMigrationRecoveryAction.CleanupCommittedMigration,
                    scope.Recovery.Evaluate().Action
                );

                scope.Coordinator.RecoverIfNeeded(scope.ModernHelperPath, null);
                scope.AssertCompleted();
            }
        }

        private sealed class TestScope : IDisposable
        {
            private readonly byte[] _sourceDatabaseBytes;
            private readonly byte[] _candidateDatabaseBytes;

            public TestScope()
            {
                DirectoryPath = Path.Combine
                (
                    Path.GetTempPath(),
                    "JasonQuery-Step389F-R4E-" + Guid.NewGuid().ToString("N")
                );

                Directory.CreateDirectory(DirectoryPath);
                DatabasePath = Path.Combine(DirectoryPath, "JasonQuery.db");

                MetadataPath = Path.Combine
                (
                    DirectoryPath,
                    JasonQueryDbSecurityConstants.MetadataFileName
                );

                ModernHelperPath = Path.Combine
                (
                    DirectoryPath,
                    "JasonQuery.ModernDbMigration.exe"
                );

                OperationId = Guid.NewGuid().ToString("N");

                _sourceDatabaseBytes = Encoding.ASCII.GetBytes
                (
                    "R4E-SOURCE-DATABASE"
                );

                _candidateDatabaseBytes = Encoding.ASCII.GetBytes
                (
                    "R4E-CANDIDATE-DATABASE"
                );

                File.WriteAllBytes(DatabasePath, _sourceDatabaseBytes);

                SourceMetadata = JasonQueryDbSecurityMetadata.CreateWindowsCurrentUser
                (
                    Convert.ToBase64String(new byte[] { 2, 4, 6, 8, 10, 12 })
                );

                SourceMetadata.StorageFormatVersion = JasonQueryDbStorageFormatContract.LegacyVersion;

                new JasonQueryDbSecurityMetadataStore(MetadataPath).Save(SourceMetadata);

                TargetMetadata = JasonQueryDbStorageMigrationMetadataContract.CreateTargetMetadata
                (
                    SourceMetadata
                );

                SourceDatabaseSha256 = Hash(DatabasePath);
                SourceMetadataSha256 = Hash(MetadataPath);

                CandidateDatabaseSha256 = JasonQueryDbStorageMigrationFileIntegrity.ComputeSha256
                (
                    _candidateDatabaseBytes
                );

                TargetMetadataSha256 = JasonQueryDbStorageMigrationMetadataContract.ComputeSerializedSha256
                (
                    TargetMetadata
                );

                Paths = JasonQueryDbStorageMigrationArtifactPaths.Create
                (
                    DatabasePath,
                    MetadataPath,
                    OperationId
                );

                JournalStore = new JasonQueryDbStorageMigrationJournalStore
                (
                    DatabasePath
                );

                Recovery = new JasonQueryDbStorageMigrationRecovery
                (
                    DatabasePath,
                    MetadataPath
                );

                Coordinator = new JasonQueryDbStorageMigrationStartupRecoveryCoordinator
                (
                    DatabasePath,
                    MetadataPath,
                    ValidateCandidate
                );
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

            public bool ValidationFailure { get; set; }

            public bool ResolverCalled { get; set; }

            public byte[] LastResolvedCredential { get; private set; }

            public string Hash(string filePath)
            {
                return JasonQueryDbStorageMigrationFileIntegrity.ComputeSha256
                (
                    filePath
                );
            }

            public byte[] CreateCredential()
            {
                LastResolvedCredential = Encoding.UTF8.GetBytes
                (
                    Convert.ToBase64String(new byte[32])
                );

                return LastResolvedCredential;
            }

            public JasonQueryDbStorageMigrationStartupRecoveryResult RecoverWithCredential()
            {
                return Coordinator.RecoverIfNeeded
                (
                    ModernHelperPath,
                    (journal, paths) =>
                    {
                        ResolverCalled = true;
                        return CreateCredential();
                    }
                );
            }

            public void AdvanceTo(JasonQueryDbStorageMigrationStage targetStage)
            {
                SaveStage(JasonQueryDbStorageMigrationStage.PreparingCandidate);

                if (targetStage == JasonQueryDbStorageMigrationStage.PreparingCandidate)
                {
                    return;
                }

                File.WriteAllBytes
                (
                    Paths.CandidateDatabaseFilePath,
                    _candidateDatabaseBytes
                );

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

                Assert.AreEqual
                (
                    JasonQueryDbStorageMigrationRecoveryAction.NoRecoveryRequired,
                    Recovery.Evaluate().Action
                );
            }

            private JasonQueryDbStorageV2ReadOnlyValidationResult ValidateCandidate(string modernHelperFilePath, string candidateDatabaseFilePath, byte[] passwordUtf8)
            {
                Assert.AreEqual(ModernHelperPath, modernHelperFilePath);
                Assert.IsNotEmpty(passwordUtf8);

                if (ValidationFailure)
                {
                    throw new InvalidDataException
                    (
                        "Synthetic R4E candidate validation failure."
                    );
                }

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

                if (LastResolvedCredential != null)
                {
                    Array.Clear
                    (
                        LastResolvedCredential,
                        0,
                        LastResolvedCredential.Length
                    );
                }

                if (Directory.Exists(DirectoryPath))
                {
                    Directory.Delete(DirectoryPath, true);
                }
            }
        }
    }

    [TestClass]
    public class JasonQueryDbStorageRuntimeStartupGateTests
    {
        [TestMethod]
        public void EnsureNormalRuntimeReady_MissingMetadata_AllowsLegacyStartup()
        {
            using (var scope = new MetadataScope())
            {
                JasonQueryDbStorageRuntimeStartupGate.EnsureNormalRuntimeReady
                (
                    scope.Store
                );
            }
        }

        [TestMethod]
        public void EnsureNormalRuntimeReady_LegacyMetadata_AllowsStartup()
        {
            using (var scope = new MetadataScope())
            {
                scope.Save(JasonQueryDbStorageFormatContract.LegacyVersion);

                JasonQueryDbStorageRuntimeStartupGate.EnsureNormalRuntimeReady
                (
                    scope.Store
                );
            }
        }

        [TestMethod]
        public void EnsureNormalRuntimeReady_ModernMetadata_BlocksLegacyRuntime()
        {
            using (var scope = new MetadataScope())
            {
                scope.Save(JasonQueryDbStorageFormatContract.ModernVersion);

                Assert.ThrowsExactly<InvalidOperationException>
                (
                    () => JasonQueryDbStorageRuntimeStartupGate.EnsureNormalRuntimeReady
                    (
                        scope.Store
                    )
                );
            }
        }

        [TestMethod]
        public void EnsureNormalRuntimeReady_UnknownStorageVersion_FailsClosed()
        {
            using (var scope = new MetadataScope())
            {
                scope.Save(JasonQueryDbStorageFormatContract.LegacyVersion);

                var json = File.ReadAllText(scope.MetadataPath).Replace
                (
                    "\"storageFormatVersion\": 1",
                    "\"storageFormatVersion\": 3"
                );

                File.WriteAllText(scope.MetadataPath, json);

                Assert.ThrowsExactly<NotSupportedException>
                (
                    () => JasonQueryDbStorageRuntimeStartupGate.EnsureNormalRuntimeReady
                    (
                        scope.Store
                    )
                );
            }
        }

        private sealed class MetadataScope : IDisposable
        {
            public MetadataScope()
            {
                DirectoryPath = Path.Combine
                (
                    Path.GetTempPath(),
                    "JasonQuery-Step389F-R4E-RuntimeGate-" + Guid.NewGuid().ToString("N")
                );

                Directory.CreateDirectory(DirectoryPath);

                MetadataPath = Path.Combine
                (
                    DirectoryPath,
                    JasonQueryDbSecurityConstants.MetadataFileName
                );

                Store = new JasonQueryDbSecurityMetadataStore(MetadataPath);
            }

            public string DirectoryPath { get; }

            public string MetadataPath { get; }

            public JasonQueryDbSecurityMetadataStore Store { get; }

            public void Save(int storageFormatVersion)
            {
                var metadata = JasonQueryDbSecurityMetadata.CreateWindowsCurrentUser
                (
                    Convert.ToBase64String(new byte[] { 1, 3, 5, 7 })
                );

                metadata.StorageFormatVersion = storageFormatVersion;
                Store.Save(metadata);
            }

            public void Dispose()
            {
                if (Directory.Exists(DirectoryPath))
                {
                    Directory.Delete(DirectoryPath, true);
                }
            }
        }
    }

    [TestClass]
    public class JasonQueryDbStorageMigrationCredentialEncoderTests
    {
        [TestMethod]
        public void EncodeDatabaseKey_NullKey_Throws()
        {
            Assert.ThrowsExactly<ArgumentNullException>
            (
                () => JasonQueryDbStorageMigrationCredentialEncoder.EncodeDatabaseKey(null)
            );
        }

        [TestMethod]
        public void EncodeDatabaseKey_WrongLength_Throws()
        {
            Assert.ThrowsExactly<ArgumentException>
            (
                () => JasonQueryDbStorageMigrationCredentialEncoder.EncodeDatabaseKey
                (
                    new byte[JasonQueryDbSecurityConstants.DatabaseKeySizeBytes - 1]
                )
            );
        }

        [TestMethod]
        public void EncodeDatabaseKey_ProducesCanonicalBase64Utf8()
        {
            var databaseKey = new byte[JasonQueryDbSecurityConstants.DatabaseKeySizeBytes];

            for (var index = 0; index < databaseKey.Length; index++)
            {
                databaseKey[index] = (byte)index;
            }

            var expected = Encoding.UTF8.GetBytes
            (
                Convert.ToBase64String(databaseKey)
            );

            var actual = JasonQueryDbStorageMigrationCredentialEncoder.EncodeDatabaseKey
            (
                databaseKey
            );

            try
            {
                CollectionAssert.AreEqual(expected, actual);
            }
            finally
            {
                Array.Clear(databaseKey, 0, databaseKey.Length);
                Array.Clear(expected, 0, expected.Length);
                Array.Clear(actual, 0, actual.Length);
            }
        }

        [TestMethod]
        public void EncodeDatabaseKey_DoesNotMutateInputKey()
        {
            var databaseKey = new byte[JasonQueryDbSecurityConstants.DatabaseKeySizeBytes];

            for (var index = 0; index < databaseKey.Length; index++)
            {
                databaseKey[index] = (byte)(255 - index);
            }

            var before = (byte[])databaseKey.Clone();

            var encoded = JasonQueryDbStorageMigrationCredentialEncoder.EncodeDatabaseKey
            (
                databaseKey
            );

            try
            {
                CollectionAssert.AreEqual(before, databaseKey);
            }
            finally
            {
                Array.Clear(databaseKey, 0, databaseKey.Length);
                Array.Clear(before, 0, before.Length);
                Array.Clear(encoded, 0, encoded.Length);
            }
        }
    }
}
