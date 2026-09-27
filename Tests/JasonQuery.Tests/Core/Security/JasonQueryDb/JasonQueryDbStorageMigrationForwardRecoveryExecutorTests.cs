using JasonQuery.Core.Security.JasonQueryDb;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace JasonQuery.Tests.Core.Security.JasonQueryDb
{
    [TestClass]
    public class JasonQueryDbStorageMigrationForwardRecoveryExecutorTests
    {
        [TestMethod]
        public void Constructor_EmptyDatabasePath_Throws()
        {
            Assert.ThrowsExactly<ArgumentException>
            (
                () => new JasonQueryDbStorageMigrationForwardRecoveryExecutor
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
                () => new JasonQueryDbStorageMigrationForwardRecoveryExecutor
                (
                    Path.Combine(Path.GetTempPath(), "JasonQuery.db"),
                    " "
                )
            );
        }

        [TestMethod]
        public void Execute_NoJournal_IsIdempotent()
        {
            using (var scope = new TestScope())
            {
                var result = scope.Executor.Execute();

                Assert.IsNull(result.OperationId);

                Assert.AreEqual
                (
                    JasonQueryDbStorageMigrationRecoveryAction.NoRecoveryRequired,
                    scope.Recovery.Evaluate().Action
                );
            }
        }

        [TestMethod]
        public void Execute_PreparingCandidate_RejectsEarlyStage()
        {
            using (var scope = new TestScope())
            {
                scope.AdvanceTo(JasonQueryDbStorageMigrationStage.PreparingCandidate);

                Assert.ThrowsExactly<InvalidOperationException>
                (
                    () => scope.Executor.Execute()
                );

                Assert.AreEqual
                (
                    JasonQueryDbStorageMigrationStage.PreparingCandidate,
                    scope.JournalStore.Load().Stage
                );
            }
        }

        [TestMethod]
        public void Execute_CandidateReady_RejectsEarlyStage()
        {
            using (var scope = new TestScope())
            {
                scope.AdvanceTo(JasonQueryDbStorageMigrationStage.CandidateReady);

                Assert.ThrowsExactly<InvalidOperationException>
                (
                    () => scope.Executor.Execute()
                );

                Assert.AreEqual
                (
                    JasonQueryDbStorageMigrationStage.CandidateReady,
                    scope.JournalStore.Load().Stage
                );
            }
        }

        [TestMethod]
        public void Execute_ReplacePreparedStateA_CompletesForwardMigrationAndCleanup()
        {
            using (var scope = new TestScope())
            {
                scope.AdvanceTo(JasonQueryDbStorageMigrationStage.ReplacePrepared);

                var sourceHash = scope.SourceDatabaseSha256;
                var candidateHash = scope.CandidateDatabaseSha256;
                var sourceMetadataHash = scope.SourceMetadataSha256;
                var targetMetadataHash = scope.TargetMetadataSha256;
                var result = scope.Executor.Execute();

                Assert.AreEqual(scope.OperationId, result.OperationId);
                Assert.IsFalse(scope.JournalStore.Exists);
                Assert.AreEqual(candidateHash, scope.Hash(scope.DatabasePath));
                Assert.AreEqual(targetMetadataHash, scope.Hash(scope.MetadataPath));
                Assert.IsFalse(File.Exists(scope.Paths.CandidateDatabaseFilePath));
                Assert.IsFalse(File.Exists(scope.Paths.MetadataTemporaryFilePath));
                Assert.IsFalse(File.Exists(scope.Paths.DatabaseBackupFilePath));
                Assert.IsFalse(File.Exists(scope.Paths.MetadataBackupFilePath));

                Assert.AreEqual
                (
                    JasonQueryDbStorageMigrationRecoveryAction.NoRecoveryRequired,
                    scope.Recovery.Evaluate().Action
                );

                Assert.AreNotEqual(sourceHash, scope.Hash(scope.DatabasePath));
                Assert.AreNotEqual(sourceMetadataHash, scope.Hash(scope.MetadataPath));
            }
        }

        [TestMethod]
        public void Execute_ReplacePreparedStateB_RecognizesDatabaseReplaceAndCompletes()
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

                var result = scope.Executor.Execute();

                Assert.AreEqual(scope.OperationId, result.OperationId);
                scope.AssertCompleted();
            }
        }

        [TestMethod]
        public void Execute_DatabaseReplacedSourceMetadata_CommitsMetadataAndCompletes()
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

                var result = scope.Executor.Execute();

                Assert.AreEqual(scope.OperationId, result.OperationId);
                scope.AssertCompleted();
            }
        }

        [TestMethod]
        public void Execute_DatabaseReplacedTargetMetadata_RecognizesMetadataCommitAndCompletes()
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

                var result = scope.Executor.Execute();

                Assert.AreEqual(scope.OperationId, result.OperationId);
                scope.AssertCompleted();
            }
        }

        [TestMethod]
        public void Execute_MetadataCommitted_WithBothBackups_CleansAndDeletesJournalLast()
        {
            using (var scope = new TestScope())
            {
                scope.AdvanceTo(JasonQueryDbStorageMigrationStage.ReplacePrepared);
                scope.SimulateDatabaseReplace();
                scope.AdvanceJournalStage(JasonQueryDbStorageMigrationStage.DatabaseReplaced);
                scope.SimulateMetadataReplace();
                scope.AdvanceJournalStage(JasonQueryDbStorageMigrationStage.MetadataCommitted);

                Assert.IsTrue(File.Exists(scope.Paths.DatabaseBackupFilePath));
                Assert.IsTrue(File.Exists(scope.Paths.MetadataBackupFilePath));

                var result = scope.Executor.Execute();

                Assert.AreEqual(scope.OperationId, result.OperationId);
                scope.AssertCompleted();
            }
        }

        [TestMethod]
        public void Execute_MetadataCommitted_WithDatabaseBackupAlreadyRemoved_Completes()
        {
            using (var scope = new TestScope())
            {
                scope.AdvanceTo(JasonQueryDbStorageMigrationStage.ReplacePrepared);
                scope.SimulateDatabaseReplace();
                scope.AdvanceJournalStage(JasonQueryDbStorageMigrationStage.DatabaseReplaced);
                scope.SimulateMetadataReplace();
                scope.AdvanceJournalStage(JasonQueryDbStorageMigrationStage.MetadataCommitted);

                File.Delete(scope.Paths.DatabaseBackupFilePath);

                var result = scope.Executor.Execute();

                Assert.AreEqual(scope.OperationId, result.OperationId);
                scope.AssertCompleted();
            }
        }

        [TestMethod]
        public void Execute_MetadataCommitted_WithMetadataBackupAlreadyRemoved_Completes()
        {
            using (var scope = new TestScope())
            {
                scope.AdvanceTo(JasonQueryDbStorageMigrationStage.ReplacePrepared);
                scope.SimulateDatabaseReplace();
                scope.AdvanceJournalStage(JasonQueryDbStorageMigrationStage.DatabaseReplaced);
                scope.SimulateMetadataReplace();
                scope.AdvanceJournalStage(JasonQueryDbStorageMigrationStage.MetadataCommitted);

                File.Delete(scope.Paths.MetadataBackupFilePath);

                var result = scope.Executor.Execute();

                Assert.AreEqual(scope.OperationId, result.OperationId);
                scope.AssertCompleted();
            }
        }

        [TestMethod]
        public void Execute_ReplacePrepared_CandidateHashMismatch_FailsBeforeDatabaseReplace()
        {
            using (var scope = new TestScope())
            {
                scope.AdvanceTo(JasonQueryDbStorageMigrationStage.ReplacePrepared);
                File.AppendAllText(scope.Paths.CandidateDatabaseFilePath, "MUTATED");

                Assert.ThrowsExactly<InvalidDataException>
                (
                    () => scope.Executor.Execute()
                );

                Assert.AreEqual(scope.SourceDatabaseSha256, scope.Hash(scope.DatabasePath));
                Assert.IsFalse(File.Exists(scope.Paths.DatabaseBackupFilePath));

                Assert.AreEqual
                (
                    JasonQueryDbStorageMigrationStage.ReplacePrepared,
                    scope.JournalStore.Load().Stage
                );
            }
        }

        [TestMethod]
        public void Execute_ReplacePrepared_MetadataTempHashMismatch_FailsBeforeDatabaseReplace()
        {
            using (var scope = new TestScope())
            {
                scope.AdvanceTo(JasonQueryDbStorageMigrationStage.ReplacePrepared);
                File.AppendAllText(scope.Paths.MetadataTemporaryFilePath, " ");

                Assert.ThrowsExactly<InvalidDataException>
                (
                    () => scope.Executor.Execute()
                );

                Assert.AreEqual(scope.SourceDatabaseSha256, scope.Hash(scope.DatabasePath));
                Assert.IsFalse(File.Exists(scope.Paths.DatabaseBackupFilePath));
            }
        }

        [TestMethod]
        public void Execute_ReplacePrepared_UnexpectedDatabaseBackup_FailsClosed()
        {
            using (var scope = new TestScope())
            {
                scope.AdvanceTo(JasonQueryDbStorageMigrationStage.ReplacePrepared);
                File.WriteAllText(scope.Paths.DatabaseBackupFilePath, "UNEXPECTED");

                Assert.ThrowsExactly<InvalidDataException>
                (
                    () => scope.Executor.Execute()
                );

                Assert.AreEqual(scope.SourceDatabaseSha256, scope.Hash(scope.DatabasePath));

                Assert.AreEqual
                (
                    JasonQueryDbStorageMigrationStage.ReplacePrepared,
                    scope.JournalStore.Load().Stage
                );
            }
        }

        [TestMethod]
        public void Execute_DatabaseReplaced_MetadataTempHashMismatch_DoesNotCommitMetadata()
        {
            using (var scope = new TestScope())
            {
                scope.AdvanceTo(JasonQueryDbStorageMigrationStage.ReplacePrepared);
                scope.SimulateDatabaseReplace();
                scope.AdvanceJournalStage(JasonQueryDbStorageMigrationStage.DatabaseReplaced);
                File.AppendAllText(scope.Paths.MetadataTemporaryFilePath, " ");

                Assert.ThrowsExactly<InvalidDataException>
                (
                    () => scope.Executor.Execute()
                );

                Assert.AreEqual(scope.SourceMetadataSha256, scope.Hash(scope.MetadataPath));
                Assert.IsFalse(File.Exists(scope.Paths.MetadataBackupFilePath));

                Assert.AreEqual
                (
                    JasonQueryDbStorageMigrationStage.DatabaseReplaced,
                    scope.JournalStore.Load().Stage
                );
            }
        }

        [TestMethod]
        public void Execute_MetadataCommitted_DatabaseBackupHashMismatch_FailsBeforeDelete()
        {
            using (var scope = new TestScope())
            {
                scope.AdvanceTo(JasonQueryDbStorageMigrationStage.ReplacePrepared);
                scope.SimulateDatabaseReplace();
                scope.AdvanceJournalStage(JasonQueryDbStorageMigrationStage.DatabaseReplaced);
                scope.SimulateMetadataReplace();
                scope.AdvanceJournalStage(JasonQueryDbStorageMigrationStage.MetadataCommitted);

                File.AppendAllText(scope.Paths.DatabaseBackupFilePath, "MUTATED");

                Assert.ThrowsExactly<InvalidDataException>
                (
                    () => scope.Executor.Execute()
                );

                Assert.IsTrue(File.Exists(scope.Paths.DatabaseBackupFilePath));
                Assert.IsTrue(scope.JournalStore.Exists);
            }
        }

        [TestMethod]
        public void Execute_MetadataCommitted_MetadataBackupHashMismatch_FailsBeforeDelete()
        {
            using (var scope = new TestScope())
            {
                scope.AdvanceTo(JasonQueryDbStorageMigrationStage.ReplacePrepared);
                scope.SimulateDatabaseReplace();
                scope.AdvanceJournalStage(JasonQueryDbStorageMigrationStage.DatabaseReplaced);
                scope.SimulateMetadataReplace();
                scope.AdvanceJournalStage(JasonQueryDbStorageMigrationStage.MetadataCommitted);

                File.AppendAllText(scope.Paths.MetadataBackupFilePath, "MUTATED");

                Assert.ThrowsExactly<InvalidDataException>
                (
                    () => scope.Executor.Execute()
                );

                Assert.IsTrue(File.Exists(scope.Paths.MetadataBackupFilePath));
                Assert.IsTrue(scope.JournalStore.Exists);
            }
        }

        [TestMethod]
        public void Execute_MetadataCommitted_DatabaseBackupDirectory_FailsClosed()
        {
            using (var scope = new TestScope())
            {
                scope.AdvanceTo(JasonQueryDbStorageMigrationStage.ReplacePrepared);
                scope.SimulateDatabaseReplace();
                scope.AdvanceJournalStage(JasonQueryDbStorageMigrationStage.DatabaseReplaced);
                scope.SimulateMetadataReplace();
                scope.AdvanceJournalStage(JasonQueryDbStorageMigrationStage.MetadataCommitted);

                File.Delete(scope.Paths.DatabaseBackupFilePath);
                Directory.CreateDirectory(scope.Paths.DatabaseBackupFilePath);

                Assert.ThrowsExactly<InvalidDataException>
                (
                    () => scope.Executor.Execute()
                );

                Assert.IsTrue(Directory.Exists(scope.Paths.DatabaseBackupFilePath));
                Assert.IsTrue(scope.JournalStore.Exists);
            }
        }

        [TestMethod]
        public void Execute_MetadataCommitted_CandidateArtifact_FailsBeforeCleanup()
        {
            using (var scope = new TestScope())
            {
                scope.AdvanceTo(JasonQueryDbStorageMigrationStage.ReplacePrepared);
                scope.SimulateDatabaseReplace();
                scope.AdvanceJournalStage(JasonQueryDbStorageMigrationStage.DatabaseReplaced);
                scope.SimulateMetadataReplace();
                scope.AdvanceJournalStage(JasonQueryDbStorageMigrationStage.MetadataCommitted);

                File.WriteAllText(scope.Paths.CandidateDatabaseFilePath, "UNEXPECTED");

                Assert.ThrowsExactly<InvalidDataException>
                (
                    () => scope.Executor.Execute()
                );

                Assert.IsTrue(scope.JournalStore.Exists);
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
                    "JasonQuery-Step389F-R4C2-" + Guid.NewGuid().ToString("N")
                );

                Directory.CreateDirectory(DirectoryPath);

                DatabasePath = Path.Combine(DirectoryPath, "JasonQuery.db");

                MetadataPath = Path.Combine
                (
                    DirectoryPath,
                    JasonQueryDbSecurityConstants.MetadataFileName
                );

                OperationId = Guid.NewGuid().ToString("N");

                _sourceDatabaseBytes = new byte[] { 10, 20, 30, 40, 50 };
                _candidateDatabaseBytes = new byte[] { 90, 80, 70, 60, 50, 40 };

                File.WriteAllBytes(DatabasePath, _sourceDatabaseBytes);

                SourceMetadata = JasonQueryDbSecurityMetadata.CreateWindowsCurrentUser
                (
                    Convert.ToBase64String(new byte[] { 2, 4, 6, 8, 10, 12 })
                );

                SourceMetadata.StorageFormatVersion = JasonQueryDbStorageFormatContract.LegacyVersion;

                new JasonQueryDbSecurityMetadataStore(MetadataPath).Save(SourceMetadata);

                TargetMetadata = JasonQueryDbStorageMigrationMetadataContract.CreateTargetMetadata(SourceMetadata);

                SourceDatabaseSha256 = Hash(DatabasePath);
                SourceMetadataSha256 = Hash(MetadataPath);
                CandidateDatabaseSha256 = JasonQueryDbStorageMigrationFileIntegrity.ComputeSha256(_candidateDatabaseBytes);
                TargetMetadataSha256 = JasonQueryDbStorageMigrationMetadataContract.ComputeSerializedSha256(TargetMetadata);

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

                Executor = new JasonQueryDbStorageMigrationForwardRecoveryExecutor
                (
                    DatabasePath,
                    MetadataPath
                );
            }

            public string DirectoryPath { get; }

            public string DatabasePath { get; }

            public string MetadataPath { get; }

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

            public JasonQueryDbStorageMigrationForwardRecoveryExecutor Executor { get; }

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

                if (targetStage == JasonQueryDbStorageMigrationStage.ReplacePrepared)
                {
                    return;
                }

                SimulateDatabaseReplace();
                SaveStage(JasonQueryDbStorageMigrationStage.DatabaseReplaced);

                if (targetStage == JasonQueryDbStorageMigrationStage.DatabaseReplaced)
                {
                    return;
                }

                SimulateMetadataReplace();
                SaveStage(JasonQueryDbStorageMigrationStage.MetadataCommitted);
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
                Assert.IsFalse(File.Exists(Paths.MetadataTemporaryFilePath));
                Assert.IsFalse(File.Exists(Paths.DatabaseBackupFilePath));
                Assert.IsFalse(File.Exists(Paths.MetadataBackupFilePath));

                Assert.AreEqual
                (
                    JasonQueryDbStorageMigrationRecoveryAction.NoRecoveryRequired,
                    Recovery.Evaluate().Action
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

                if (Directory.Exists(DirectoryPath))
                {
                    Directory.Delete(DirectoryPath, true);
                }
            }
        }
    }
}
