using JasonQuery.Core.Security.JasonQueryDb;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Text;

namespace JasonQuery.Tests.Core.Security.JasonQueryDb
{
    [TestClass]
    public class JasonQueryDbStorageMigrationOrchestratorTests
    {
        [TestMethod]
        public void Prepare_MissingStorageMarkerCreatesDurablePreparingCandidateJournal()
        {
            using (var scope = new TestScope(null))
            {
                var result = scope.Orchestrator.Prepare();

                Assert.IsTrue(scope.JournalStore.Exists);
                Assert.AreEqual(JasonQueryDbStorageMigrationStage.PreparingCandidate, result.Journal.Stage);
                Assert.IsNull(result.Journal.CandidateDatabaseSha256);
                Assert.AreEqual(JasonQueryDbStorageFormatContract.LegacyVersion, result.Journal.SourceStorageFormatVersion);
                Assert.AreEqual(JasonQueryDbStorageFormatContract.ModernVersion, result.Journal.TargetStorageFormatVersion);
                Assert.IsNull(result.Journal.SourceMetadata.StorageFormatVersion);
                Assert.AreEqual(JasonQueryDbStorageFormatContract.ModernVersion, result.Journal.TargetMetadata.StorageFormatVersion);
                Assert.AreEqual(scope.SourceDatabaseSha256, result.Journal.SourceDatabaseSha256);
                Assert.AreEqual(scope.SourceMetadataSha256, result.Journal.SourceMetadataSha256);

                Assert.AreEqual
                (
                    JasonQueryDbStorageMigrationMetadataContract.ComputeSerializedSha256(result.Journal.TargetMetadata),
                    result.Journal.TargetMetadataSha256
                );

                Assert.IsFalse(File.Exists(result.Paths.CandidateDatabaseFilePath));
                Assert.IsFalse(File.Exists(result.Paths.DatabaseBackupFilePath));
                Assert.IsFalse(File.Exists(result.Paths.MetadataTemporaryFilePath));
                Assert.IsFalse(File.Exists(result.Paths.MetadataBackupFilePath));
                Assert.IsFalse(File.Exists(result.Paths.LockFilePath));
            }
        }

        [TestMethod]
        public void Prepare_ExplicitLegacyStorageMarkerIsAccepted()
        {
            using (var scope = new TestScope(JasonQueryDbStorageFormatContract.LegacyVersion))
            {
                var result = scope.Orchestrator.Prepare();

                Assert.AreEqual
                (
                    JasonQueryDbStorageFormatContract.LegacyVersion,
                    result.Journal.SourceMetadata.StorageFormatVersion
                );

                Assert.AreEqual
                (
                    JasonQueryDbStorageMigrationRecoveryAction.RestartPreparation,
                    new JasonQueryDbStorageMigrationRecovery(scope.DatabasePath, scope.MetadataPath).Evaluate().Action
                );
            }
        }

        [TestMethod]
        public void Prepare_RejectsModernSourceMetadata()
        {
            using (var scope = new TestScope(JasonQueryDbStorageFormatContract.ModernVersion))
            {
                Assert.ThrowsExactly<InvalidOperationException>
                (
                    () => scope.Orchestrator.Prepare()
                );

                Assert.IsFalse(scope.JournalStore.Exists);
            }
        }

        [TestMethod]
        public void Prepare_OrphanMigrationArtifactFailsClosed()
        {
            using (var scope = new TestScope(null))
            {
                var orphan = scope.DatabasePath + ".migration." + Guid.NewGuid().ToString("N") + ".candidate";

                File.WriteAllText(orphan, "ORPHAN", Encoding.UTF8);

                Assert.ThrowsExactly<InvalidDataException>
                (
                    () => scope.Orchestrator.Prepare()
                );

                Assert.IsFalse(scope.JournalStore.Exists);
                Assert.IsTrue(File.Exists(orphan));
            }
        }

        [TestMethod]
        public void Prepare_SourceSqliteSidecarFailsClosed()
        {
            using (var scope = new TestScope(null))
            {
                var sidecar = scope.DatabasePath + "-wal";

                File.WriteAllText(sidecar, "ACTIVE", Encoding.UTF8);

                Assert.ThrowsExactly<InvalidDataException>
                (
                    () => scope.Orchestrator.Prepare()
                );

                Assert.IsFalse(scope.JournalStore.Exists);
                Assert.IsTrue(File.Exists(sidecar));
            }
        }

        private sealed class TestScope : IDisposable
        {
            public TestScope(int? storageFormatVersion)
            {
                DirectoryPath = Path.Combine
                (
                    Path.GetTempPath(),
                    "JasonQuery-Step389F-R4A-Orchestrator-" + Guid.NewGuid().ToString("N")
                );

                Directory.CreateDirectory(DirectoryPath);

                DatabasePath = Path.Combine(DirectoryPath, "JasonQuery.db");
                MetadataPath = Path.Combine(DirectoryPath, JasonQueryDbSecurityConstants.MetadataFileName);

                File.WriteAllBytes(DatabasePath, Encoding.ASCII.GetBytes("SOURCE-DATABASE-R4A"));

                var metadata = JasonQueryDbSecurityMetadata.CreateWindowsCurrentUser
                (
                    Convert.ToBase64String(new byte[] { 1, 3, 5, 7, 9, 11 })
                );

                metadata.StorageFormatVersion = storageFormatVersion;

                var metadataStore = new JasonQueryDbSecurityMetadataStore(MetadataPath);

                metadataStore.Save(metadata);

                SourceDatabaseSha256 = JasonQueryDbStorageMigrationFileIntegrity.ComputeSha256(DatabasePath);
                SourceMetadataSha256 = JasonQueryDbStorageMigrationFileIntegrity.ComputeSha256(MetadataPath);

                JournalStore = new JasonQueryDbStorageMigrationJournalStore(DatabasePath);
                Orchestrator = new JasonQueryDbStorageMigrationOrchestrator(DatabasePath, MetadataPath);
            }

            public string DirectoryPath { get; }

            public string DatabasePath { get; }

            public string MetadataPath { get; }

            public string SourceDatabaseSha256 { get; }

            public string SourceMetadataSha256 { get; }

            public JasonQueryDbStorageMigrationJournalStore JournalStore { get; }

            public JasonQueryDbStorageMigrationOrchestrator Orchestrator { get; }

            public void Dispose()
            {
                if (Directory.Exists(DirectoryPath))
                {
                    Directory.Delete(DirectoryPath, true);
                }
            }
        }
    }
}
