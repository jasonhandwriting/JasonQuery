using JasonQuery.Core.Security.JasonQueryDb;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Text;

namespace JasonQuery.Tests.Core.Security.JasonQueryDb
{
    [TestClass]
    public class JasonQueryDbStorageMigrationJournalStoreTests
    {
        private const string SourceDatabaseSha256 = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";
        private const string CandidateDatabaseSha256 = "BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB";
        private const string SourceMetadataSha256 = "CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC";
        private const string TargetMetadataSha256 = "DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD";

        [TestMethod]
        public void Save_NewPreparingCandidate_RoundTrips()
        {
            using (var scope = new TestScope())
            {
                var journal = scope.CreateJournal(JasonQueryDbStorageMigrationStage.PreparingCandidate);

                scope.Store.Save(journal);

                Assert.IsTrue(scope.Store.Exists);

                var loaded = scope.Store.Load();

                Assert.AreEqual(journal.OperationId, loaded.OperationId);
                Assert.AreEqual(JasonQueryDbStorageMigrationStage.PreparingCandidate, loaded.Stage);
                Assert.AreEqual(SourceDatabaseSha256, loaded.SourceDatabaseSha256);
                Assert.IsNull(loaded.CandidateDatabaseSha256);
                Assert.AreEqual(SourceMetadataSha256, loaded.SourceMetadataSha256);
                Assert.AreEqual(TargetMetadataSha256, loaded.TargetMetadataSha256);
            }
        }

        [TestMethod]
        public void Save_InitialJournalMustStartAtPreparingCandidate()
        {
            using (var scope = new TestScope())
            {
                var journal = scope.CreateJournal(JasonQueryDbStorageMigrationStage.CandidateReady);

                Assert.ThrowsExactly<InvalidOperationException>(() => scope.Store.Save(journal));
                Assert.IsFalse(scope.Store.Exists);
            }
        }

        [TestMethod]
        public void Save_AllowsSingleForwardStageTransition()
        {
            using (var scope = new TestScope())
            {
                var preparing = scope.CreateJournal(JasonQueryDbStorageMigrationStage.PreparingCandidate);

                scope.Store.Save(preparing);

                var ready = scope.CreateJournal(JasonQueryDbStorageMigrationStage.CandidateReady);

                scope.Store.Save(ready);

                var loaded = scope.Store.Load();

                Assert.AreEqual(JasonQueryDbStorageMigrationStage.CandidateReady, loaded.Stage);
                Assert.AreEqual(CandidateDatabaseSha256, loaded.CandidateDatabaseSha256);
            }
        }

        [TestMethod]
        public void Save_RejectsStageRegression()
        {
            using (var scope = new TestScope())
            {
                scope.Store.Save(scope.CreateJournal(JasonQueryDbStorageMigrationStage.PreparingCandidate));
                scope.Store.Save(scope.CreateJournal(JasonQueryDbStorageMigrationStage.CandidateReady));

                Assert.ThrowsExactly<InvalidOperationException>
                (
                    () => scope.Store.Save(scope.CreateJournal(JasonQueryDbStorageMigrationStage.PreparingCandidate))
                );
            }
        }

        [TestMethod]
        public void Save_RejectsStageSkip()
        {
            using (var scope = new TestScope())
            {
                scope.Store.Save(scope.CreateJournal(JasonQueryDbStorageMigrationStage.PreparingCandidate));

                Assert.ThrowsExactly<InvalidOperationException>
                (
                    () => scope.Store.Save(scope.CreateJournal(JasonQueryDbStorageMigrationStage.ReplacePrepared))
                );
            }
        }

        [TestMethod]
        public void Save_RejectsOperationIdChange()
        {
            using (var scope = new TestScope())
            {
                scope.Store.Save(scope.CreateJournal(JasonQueryDbStorageMigrationStage.PreparingCandidate));

                var next = scope.CreateJournal(JasonQueryDbStorageMigrationStage.CandidateReady);

                next.OperationId = Guid.NewGuid().ToString("N");
                Assert.ThrowsExactly<InvalidOperationException>(() => scope.Store.Save(next));
            }
        }

        [TestMethod]
        public void Save_RejectsSourceIdentityMutation()
        {
            using (var scope = new TestScope())
            {
                scope.Store.Save(scope.CreateJournal(JasonQueryDbStorageMigrationStage.PreparingCandidate));

                var next = scope.CreateJournal(JasonQueryDbStorageMigrationStage.CandidateReady);

                next.SourceDatabaseSha256 = "EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE";
                Assert.ThrowsExactly<InvalidOperationException>(() => scope.Store.Save(next));
            }
        }

        [TestMethod]
        public void Save_RejectsCandidateIdentityMutationAfterCandidateReady()
        {
            using (var scope = new TestScope())
            {
                scope.Store.Save(scope.CreateJournal(JasonQueryDbStorageMigrationStage.PreparingCandidate));
                scope.Store.Save(scope.CreateJournal(JasonQueryDbStorageMigrationStage.CandidateReady));

                var next = scope.CreateJournal(JasonQueryDbStorageMigrationStage.ReplacePrepared);

                next.CandidateDatabaseSha256 = "EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE";
                Assert.ThrowsExactly<InvalidOperationException>(() => scope.Store.Save(next));
            }
        }

        [TestMethod]
        public void Save_OrphanInitialJournalTemporaryFile_FailsClosed()
        {
            using (var scope = new TestScope())
            {
                var journal = scope.CreateJournal(JasonQueryDbStorageMigrationStage.PreparingCandidate);

                var temporaryPath = JasonQueryDbStorageMigrationArtifactPaths.GetJournalTemporaryFilePath
                (
                    scope.DatabasePath,
                    journal.OperationId
                );

                File.WriteAllText(temporaryPath, "UNCOMMITTED", Encoding.UTF8);

                Assert.ThrowsExactly<InvalidDataException>(() => scope.Store.Save(journal));
                Assert.IsFalse(scope.Store.Exists);
                Assert.IsTrue(File.Exists(temporaryPath));
            }
        }

        [TestMethod]
        public void Save_ExistingCanonicalJournal_DiscardsOwnedUncommittedTemporaryFile()
        {
            using (var scope = new TestScope())
            {
                var preparing = scope.CreateJournal(JasonQueryDbStorageMigrationStage.PreparingCandidate);

                scope.Store.Save(preparing);

                var temporaryPath = JasonQueryDbStorageMigrationArtifactPaths.GetJournalTemporaryFilePath
                (
                    scope.DatabasePath,
                    preparing.OperationId
                );

                File.WriteAllText(temporaryPath, "UNCOMMITTED", Encoding.UTF8);

                scope.Store.Save(scope.CreateJournal(JasonQueryDbStorageMigrationStage.CandidateReady));

                Assert.IsFalse(File.Exists(temporaryPath));
                Assert.AreEqual(JasonQueryDbStorageMigrationStage.CandidateReady, scope.Store.Load().Stage);
            }
        }

        [TestMethod]
        public void Load_InvalidJson_FailsClosed()
        {
            using (var scope = new TestScope())
            {
                File.WriteAllText(scope.Store.JournalFilePath, "{invalid-json", Encoding.UTF8);

                Assert.ThrowsExactly<InvalidDataException>(() => scope.Store.Load());
            }
        }

        [TestMethod]
        public void Save_JournalUsesUtf8WithoutBom()
        {
            using (var scope = new TestScope())
            {
                scope.Store.Save(scope.CreateJournal(JasonQueryDbStorageMigrationStage.PreparingCandidate));

                var bytes = File.ReadAllBytes(scope.Store.JournalFilePath);

                Assert.IsGreaterThan(3, bytes.Length);
                Assert.IsFalse(bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF);
            }
        }

        [TestMethod]
        public void Delete_RemovesOwnedTemporaryFileAndCanonicalJournal()
        {
            using (var scope = new TestScope())
            {
                var journal = scope.CreateJournal(JasonQueryDbStorageMigrationStage.PreparingCandidate);

                scope.Store.Save(journal);

                var temporaryPath = JasonQueryDbStorageMigrationArtifactPaths.GetJournalTemporaryFilePath
                (
                    scope.DatabasePath,
                    journal.OperationId
                );

                File.WriteAllText(temporaryPath, "UNCOMMITTED", Encoding.UTF8);

                scope.Store.Delete();

                Assert.IsFalse(File.Exists(temporaryPath));
                Assert.IsFalse(scope.Store.Exists);
            }
        }

        private sealed class TestScope : IDisposable
        {
            public TestScope()
            {
                DirectoryPath = Path.Combine
                (
                    Path.GetTempPath(),
                    "JasonQuery-Step389F-R2-Journal-" + Guid.NewGuid().ToString("N")
                );

                Directory.CreateDirectory(DirectoryPath);

                DatabasePath = Path.Combine(DirectoryPath, "JasonQuery.db");
                MetadataPath = Path.Combine(DirectoryPath, "JasonQuery.security.json");
                OperationId = Guid.NewGuid().ToString("N");
                Store = new JasonQueryDbStorageMigrationJournalStore(DatabasePath);
            }

            public string DirectoryPath { get; }

            public string DatabasePath { get; }

            public string MetadataPath { get; }

            public string OperationId { get; }

            public JasonQueryDbStorageMigrationJournalStore Store { get; }

            public JasonQueryDbStorageMigrationJournal CreateJournal(JasonQueryDbStorageMigrationStage stage)
            {
                var sourceMetadata = JasonQueryDbSecurityMetadata.CreateWindowsCurrentUser
                (
                    Convert.ToBase64String(new byte[] { 1, 2, 3, 4 })
                );

                sourceMetadata.StorageFormatVersion = JasonQueryDbStorageFormatContract.LegacyVersion;

                var targetMetadata = JasonQueryDbSecurityMetadata.CreateWindowsCurrentUser
                (
                    sourceMetadata.ProtectedDatabaseKey
                );

                targetMetadata.StorageFormatVersion = JasonQueryDbStorageFormatContract.ModernVersion;

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
                    SourceMetadata = sourceMetadata,
                    TargetMetadata = targetMetadata
                };
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
}
