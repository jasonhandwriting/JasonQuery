using JasonQuery.Core.Security.JasonQueryDb;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Text;

namespace JasonQuery.Tests.Core.Security.JasonQueryDb
{
    [TestClass]
    public class JasonQueryDbStorageMigrationRecoveryTests
    {
        [TestMethod]
        public void Evaluate_NoJournalAndNoArtifacts_ReturnsNoRecoveryRequired()
        {
            using (var scope = new TestScope())
            {
                var plan = scope.Recovery.Evaluate();

                Assert.AreEqual(JasonQueryDbStorageMigrationRecoveryAction.NoRecoveryRequired, plan.Action);
                Assert.IsNull(plan.Journal);
                Assert.IsNull(plan.Paths);
            }
        }

        [TestMethod]
        public void Evaluate_NoJournalWithOrphanArtifact_FailsClosed()
        {
            using (var scope = new TestScope())
            {
                var orphan = scope.DatabasePath + ".migration." + scope.OperationId + ".backup";

                File.WriteAllText(orphan, "ORPHAN", Encoding.UTF8);

                Assert.ThrowsExactly<InvalidDataException>(() => scope.Recovery.Evaluate());
                Assert.IsTrue(File.Exists(orphan));
            }
        }

        [TestMethod]
        public void Evaluate_PreparingCandidate_ReturnsRestartPreparation()
        {
            using (var scope = new TestScope())
            {
                scope.AdvanceJournalTo(JasonQueryDbStorageMigrationStage.PreparingCandidate);

                File.WriteAllText(scope.Paths.CandidateDatabaseFilePath, "UNTRUSTED", Encoding.UTF8);

                var plan = scope.Recovery.Evaluate();

                Assert.AreEqual(JasonQueryDbStorageMigrationRecoveryAction.RestartPreparation, plan.Action);
                Assert.AreEqual(JasonQueryDbStorageMigrationStage.PreparingCandidate, plan.Journal.Stage);
            }
        }

        [TestMethod]
        public void Evaluate_CandidateReadyWithExactArtifacts_ReturnsValidateCandidate()
        {
            using (var scope = new TestScope())
            {
                scope.WriteCandidate();
                scope.AdvanceJournalTo(JasonQueryDbStorageMigrationStage.CandidateReady);

                var plan = scope.Recovery.Evaluate();

                Assert.AreEqual
                (
                    JasonQueryDbStorageMigrationRecoveryAction.ValidateCandidateAndPrepareReplace,
                    plan.Action
                );
            }
        }

        [TestMethod]
        public void Evaluate_CandidateReadyMissingCandidate_FailsClosed()
        {
            using (var scope = new TestScope())
            {
                scope.AdvanceJournalTo(JasonQueryDbStorageMigrationStage.CandidateReady);

                Assert.ThrowsExactly<InvalidDataException>(() => scope.Recovery.Evaluate());
            }
        }

        [TestMethod]
        public void Evaluate_ReplacePreparedBeforeReplace_ReturnsReplaceDatabase()
        {
            using (var scope = new TestScope())
            {
                scope.WriteCandidate();
                scope.WriteTargetMetadataTemporary();
                scope.AdvanceJournalTo(JasonQueryDbStorageMigrationStage.ReplacePrepared);

                var plan = scope.Recovery.Evaluate();

                Assert.AreEqual(JasonQueryDbStorageMigrationRecoveryAction.ReplaceDatabase, plan.Action);
            }
        }

        [TestMethod]
        public void Evaluate_ReplacePreparedAfterDatabaseReplace_ReturnsRecognizeDatabaseReplaced()
        {
            using (var scope = new TestScope())
            {
                scope.WriteCandidate();
                scope.WriteTargetMetadataTemporary();
                scope.AdvanceJournalTo(JasonQueryDbStorageMigrationStage.ReplacePrepared);
                scope.SimulateDatabaseReplace();

                var plan = scope.Recovery.Evaluate();

                Assert.AreEqual
                (
                    JasonQueryDbStorageMigrationRecoveryAction.RecognizeDatabaseReplaced,
                    plan.Action
                );
            }
        }

        [TestMethod]
        public void Evaluate_ReplacePreparedAmbiguousDatabaseState_FailsClosed()
        {
            using (var scope = new TestScope())
            {
                scope.WriteCandidate();
                scope.WriteTargetMetadataTemporary();
                scope.AdvanceJournalTo(JasonQueryDbStorageMigrationStage.ReplacePrepared);

                File.Copy(scope.DatabasePath, scope.Paths.DatabaseBackupFilePath, false);
                Assert.ThrowsExactly<InvalidDataException>(() => scope.Recovery.Evaluate());
            }
        }

        [TestMethod]
        public void Evaluate_DatabaseReplacedWithSourceMetadata_ReturnsCommitMetadata()
        {
            using (var scope = new TestScope())
            {
                scope.WriteCandidate();
                scope.WriteTargetMetadataTemporary();
                scope.AdvanceJournalTo(JasonQueryDbStorageMigrationStage.DatabaseReplaced);
                scope.SimulateDatabaseReplace();

                var plan = scope.Recovery.Evaluate();

                Assert.AreEqual(JasonQueryDbStorageMigrationRecoveryAction.CommitMetadata, plan.Action);
            }
        }

        [TestMethod]
        public void Evaluate_DatabaseReplacedAfterMetadataReplace_ReturnsRecognizeMetadataCommitted()
        {
            using (var scope = new TestScope())
            {
                scope.WriteCandidate();
                scope.WriteTargetMetadataTemporary();
                scope.AdvanceJournalTo(JasonQueryDbStorageMigrationStage.DatabaseReplaced);
                scope.SimulateDatabaseReplace();
                scope.SimulateMetadataReplace();

                var plan = scope.Recovery.Evaluate();

                Assert.AreEqual
                (
                    JasonQueryDbStorageMigrationRecoveryAction.RecognizeMetadataCommitted,
                    plan.Action
                );
            }
        }

        [TestMethod]
        public void Evaluate_MetadataCommitted_ReturnsCleanupCommittedMigration()
        {
            using (var scope = new TestScope())
            {
                scope.WriteCandidate();
                scope.WriteTargetMetadataTemporary();
                scope.AdvanceJournalTo(JasonQueryDbStorageMigrationStage.MetadataCommitted);
                scope.SimulateDatabaseReplace();
                scope.SimulateMetadataReplace();

                var plan = scope.Recovery.Evaluate();

                Assert.AreEqual
                (
                    JasonQueryDbStorageMigrationRecoveryAction.CleanupCommittedMigration,
                    plan.Action
                );
            }
        }

        private sealed class TestScope : IDisposable
        {
            private readonly byte[] _candidateBytes;

            public TestScope()
            {
                DirectoryPath = Path.Combine
                (
                    Path.GetTempPath(),
                    "JasonQuery-Step389F-R4A-Recovery-" + Guid.NewGuid().ToString("N")
                );

                Directory.CreateDirectory(DirectoryPath);

                DatabasePath = Path.Combine(DirectoryPath, "JasonQuery.db");
                MetadataPath = Path.Combine(DirectoryPath, JasonQueryDbSecurityConstants.MetadataFileName);
                OperationId = Guid.NewGuid().ToString("N");

                File.WriteAllBytes(DatabasePath, Encoding.ASCII.GetBytes("SOURCE-DATABASE-R4A-RECOVERY"));

                SourceMetadata = JasonQueryDbSecurityMetadata.CreateWindowsCurrentUser
                (
                    Convert.ToBase64String(new byte[] { 2, 4, 6, 8, 10, 12 })
                );

                SourceMetadata.StorageFormatVersion = JasonQueryDbStorageFormatContract.LegacyVersion;

                var metadataStore = new JasonQueryDbSecurityMetadataStore(MetadataPath);

                metadataStore.Save(SourceMetadata);

                TargetMetadata = JasonQueryDbStorageMigrationMetadataContract.CreateTargetMetadata
                (
                    SourceMetadata
                );

                SourceDatabaseSha256 = JasonQueryDbStorageMigrationFileIntegrity.ComputeSha256(DatabasePath);
                SourceMetadataSha256 = JasonQueryDbStorageMigrationFileIntegrity.ComputeSha256(MetadataPath);
                TargetMetadataSha256 = JasonQueryDbStorageMigrationMetadataContract.ComputeSerializedSha256(TargetMetadata);

                _candidateBytes = Encoding.ASCII.GetBytes("CANDIDATE-DATABASE-R4A-RECOVERY");
                CandidateDatabaseSha256 = JasonQueryDbStorageMigrationFileIntegrity.ComputeSha256(_candidateBytes);

                Paths = JasonQueryDbStorageMigrationArtifactPaths.Create
                (
                    DatabasePath,
                    MetadataPath,
                    OperationId
                );

                JournalStore = new JasonQueryDbStorageMigrationJournalStore(DatabasePath);
                Recovery = new JasonQueryDbStorageMigrationRecovery(DatabasePath, MetadataPath);
            }

            public string DirectoryPath { get; }

            public string DatabasePath { get; }

            public string MetadataPath { get; }

            public string OperationId { get; }

            public string SourceDatabaseSha256 { get; }

            public string CandidateDatabaseSha256 { get; }

            public string SourceMetadataSha256 { get; }

            public string TargetMetadataSha256 { get; }

            public JasonQueryDbSecurityMetadata SourceMetadata { get; }

            public JasonQueryDbSecurityMetadata TargetMetadata { get; }

            public JasonQueryDbStorageMigrationArtifactPaths Paths { get; }

            public JasonQueryDbStorageMigrationJournalStore JournalStore { get; }

            public JasonQueryDbStorageMigrationRecovery Recovery { get; }

            public void WriteCandidate()
            {
                File.WriteAllBytes(Paths.CandidateDatabaseFilePath, _candidateBytes);
            }

            public void WriteTargetMetadataTemporary()
            {
                JasonQueryDbStorageMigrationMetadataContract.WriteDurablyCreateNew
                (
                    Paths.MetadataTemporaryFilePath,
                    TargetMetadata
                );
            }

            public void SimulateDatabaseReplace()
            {
                File.Copy(DatabasePath, Paths.DatabaseBackupFilePath, false);
                File.Delete(DatabasePath);
                File.Move(Paths.CandidateDatabaseFilePath, DatabasePath);
            }

            public void SimulateMetadataReplace()
            {
                File.Copy(MetadataPath, Paths.MetadataBackupFilePath, false);
                File.Delete(MetadataPath);
                File.Move(Paths.MetadataTemporaryFilePath, MetadataPath);
            }

            public void AdvanceJournalTo(JasonQueryDbStorageMigrationStage targetStage)
            {
                for (var value = (int)JasonQueryDbStorageMigrationStage.PreparingCandidate; value <= (int)targetStage; value++)
                {
                    JournalStore.Save
                    (
                        CreateJournal((JasonQueryDbStorageMigrationStage)value)
                    );
                }
            }

            private JasonQueryDbStorageMigrationJournal CreateJournal(JasonQueryDbStorageMigrationStage stage)
            {
                return new JasonQueryDbStorageMigrationJournal
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
                };
            }

            public void Dispose()
            {
                Array.Clear(_candidateBytes, 0, _candidateBytes.Length);

                if (Directory.Exists(DirectoryPath))
                {
                    Directory.Delete(DirectoryPath, true);
                }
            }
        }
    }
}
