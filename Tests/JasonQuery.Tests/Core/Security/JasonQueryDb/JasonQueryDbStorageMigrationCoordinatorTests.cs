using JasonQuery.Core.Security.JasonQueryDb;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Text;

namespace JasonQuery.Tests.Core.Security.JasonQueryDb
{
    [TestClass]
    public class JasonQueryDbStorageMigrationCoordinatorTests
    {
        [TestMethod]
        public void Constructor_EmptyDatabasePath_Throws()
        {
            Assert.ThrowsExactly<ArgumentException>
            (
                () => new JasonQueryDbStorageMigrationCoordinator
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
                () => new JasonQueryDbStorageMigrationCoordinator
                (
                    Path.Combine(Path.GetTempPath(), "JasonQuery.db"),
                    " "
                )
            );
        }

        [TestMethod]
        public void Migrate_NullSourcePassword_ThrowsBeforeJournal()
        {
            using (var scope = new TestScope())
            {
                Assert.ThrowsExactly<ArgumentException>
                (
                    () => scope.Coordinator.Migrate
                    (
                        scope.LegacyHelperPath,
                        scope.ModernHelperPath,
                        null,
                        scope.TargetPassword
                    )
                );

                Assert.IsFalse(scope.JournalStore.Exists);
                Assert.AreEqual(0, scope.ProcessCalls);
            }
        }

        [TestMethod]
        public void Migrate_OversizedSourcePassword_ThrowsBeforeJournal()
        {
            using (var scope = new TestScope())
            {
                var password = new byte[ JasonQueryDbStorageMigrationWireProtocol.MaxDatabasePasswordUtf8Bytes + 1 ];

                try
                {
                    Assert.ThrowsExactly<ArgumentException>
                    (
                        () => scope.Coordinator.Migrate
                        (
                            scope.LegacyHelperPath,
                            scope.ModernHelperPath,
                            password,
                            scope.TargetPassword
                        )
                    );
                }
                finally
                {
                    Array.Clear(password, 0, password.Length);
                }

                Assert.IsFalse(scope.JournalStore.Exists);
                Assert.AreEqual(0, scope.ProcessCalls);
            }
        }

        [TestMethod]
        public void Migrate_NullTargetPassword_ThrowsBeforeJournal()
        {
            using (var scope = new TestScope())
            {
                Assert.ThrowsExactly<ArgumentException>
                (
                    () => scope.Coordinator.Migrate
                    (
                        scope.LegacyHelperPath,
                        scope.ModernHelperPath,
                        scope.SourcePassword,
                        null
                    )
                );

                Assert.IsFalse(scope.JournalStore.Exists);
                Assert.AreEqual(0, scope.ProcessCalls);
            }
        }

        [TestMethod]
        public void Migrate_OversizedTargetPassword_ThrowsBeforeJournal()
        {
            using (var scope = new TestScope())
            {
                var password = new byte[ JasonQueryDbStorageV2CandidateWriterProtocol.MaxDatabasePasswordUtf8Bytes + 1 ];

                try
                {
                    Assert.ThrowsExactly<ArgumentException>
                    (
                        () => scope.Coordinator.Migrate
                        (
                            scope.LegacyHelperPath,
                            scope.ModernHelperPath,
                            scope.SourcePassword,
                            password
                        )
                    );
                }
                finally
                {
                    Array.Clear(password, 0, password.Length);
                }

                Assert.IsFalse(scope.JournalStore.Exists);
                Assert.AreEqual(0, scope.ProcessCalls);
            }
        }

        [TestMethod]
        public void Migrate_Success_CompletesEntireDurablePipeline()
        {
            using (var scope = new TestScope())
            {
                var result = scope.Migrate();

                Assert.IsFalse(string.IsNullOrWhiteSpace(result.OperationId));
                Assert.AreEqual(scope.LogicalBytesForwarded, result.LogicalBytesForwarded);
                Assert.AreEqual(scope.CandidateDatabaseSha256, result.TargetDatabaseSha256);
                Assert.AreEqual(scope.TargetMetadataSha256, result.TargetMetadataSha256);
                scope.AssertCompleted();
            }
        }

        [TestMethod]
        public void Migrate_Success_FinalMetadataPreservesSecurityIdentity()
        {
            using (var scope = new TestScope())
            {
                scope.Migrate();

                var committed = new JasonQueryDbSecurityMetadataStore(scope.MetadataPath).Load();

                var expected = JasonQueryDbStorageMigrationMetadataContract.CreateTargetMetadata
                (
                    scope.SourceMetadata
                );

                Assert.IsTrue
                (
                    JasonQueryDbStorageMigrationMetadataContract.MetadataEquals
                    (
                        expected,
                        committed
                    )
                );
            }
        }

        [TestMethod]
        public void Migrate_Success_DoesNotMutateCallerSecretBuffers()
        {
            using (var scope = new TestScope())
            {
                var sourceBefore = (byte[])scope.SourcePassword.Clone();
                var targetBefore = (byte[])scope.TargetPassword.Clone();

                scope.Migrate();

                CollectionAssert.AreEqual(sourceBefore, scope.SourcePassword);
                CollectionAssert.AreEqual(targetBefore, scope.TargetPassword);

                Array.Clear(sourceBefore, 0, sourceBefore.Length);
                Array.Clear(targetBefore, 0, targetBefore.Length);
            }
        }

        [TestMethod]
        public void Migrate_Success_DelegatesReceivePrivateSecretClones()
        {
            using (var scope = new TestScope())
            {
                scope.Migrate();

                Assert.IsTrue(scope.ProcessObservedSourceSecretClone);
                Assert.IsTrue(scope.ProcessObservedTargetSecretClone);
                Assert.IsTrue(scope.ValidatorObservedTargetSecretClone);
            }
        }

        [TestMethod]
        public void Migrate_Success_UsesPreparedDeterministicCandidatePath()
        {
            using (var scope = new TestScope())
            {
                var result = scope.Migrate();

                Assert.IsTrue(scope.ProcessObservedPreparedSourcePath);
                Assert.IsTrue(scope.ProcessObservedPreparedCandidatePath);

                Assert.IsGreaterThanOrEqualTo(0, scope.LastCandidatePath.IndexOf(".migration." + result.OperationId + ".candidate", StringComparison.Ordinal));
            }
        }

        [TestMethod]
        public void Migrate_ExistingJournal_RejectsNewMigration()
        {
            using (var scope = new TestScope())
            {
                new JasonQueryDbStorageMigrationOrchestrator(scope.DatabasePath, scope.MetadataPath).Prepare();

                Assert.ThrowsExactly<InvalidOperationException>
                (
                    () => scope.Migrate()
                );

                Assert.AreEqual(0, scope.ProcessCalls);

                Assert.AreEqual
                (
                    JasonQueryDbStorageMigrationStage.PreparingCandidate,
                    scope.JournalStore.Load().Stage
                );
            }
        }

        [TestMethod]
        public void Migrate_StaleLockWithoutJournal_FailsClosed()
        {
            using (var scope = new TestScope())
            {
                File.WriteAllText
                (
                    JasonQueryDbStorageMigrationArtifactPaths.GetLockFilePath
                    (
                        scope.DatabasePath
                    ),
                    "STALE",
                    Encoding.UTF8
                );

                Assert.ThrowsExactly<IOException>
                (
                    () => scope.Migrate()
                );

                Assert.IsFalse(scope.JournalStore.Exists);
                Assert.AreEqual(0, scope.ProcessCalls);
            }
        }

        [TestMethod]
        public void Migrate_ConcurrentExecutionLock_FailsBeforePrepare()
        {
            using (var scope = new TestScope())
            using (JasonQueryDbStorageMigrationExecutionLock.AcquireForNewMigration(scope.DatabasePath))
            {
                Assert.ThrowsExactly<IOException>
                (
                    () => scope.Migrate()
                );

                Assert.IsFalse(scope.JournalStore.Exists);
                Assert.AreEqual(0, scope.ProcessCalls);
            }
        }

        [TestMethod]
        public void Migrate_ProcessFailure_CleansUntrustedCandidateAndSidecars()
        {
            using (var scope = new TestScope())
            {
                scope.ProcessFailure = true;
                scope.WritePartialCandidateBeforeProcessFailure = true;

                Assert.ThrowsExactly<InvalidDataException>
                (
                    () => scope.Migrate()
                );

                var journal = scope.JournalStore.Load();

                Assert.AreEqual
                (
                    JasonQueryDbStorageMigrationStage.PreparingCandidate,
                    journal.Stage
                );

                var paths = JasonQueryDbStorageMigrationArtifactPaths.Create
                (
                    scope.DatabasePath,
                    scope.MetadataPath,
                    journal.OperationId
                );

                Assert.IsFalse(File.Exists(paths.CandidateDatabaseFilePath));
                Assert.IsFalse(File.Exists(paths.CandidateDatabaseFilePath + "-journal"));
                Assert.IsFalse(File.Exists(paths.CandidateDatabaseFilePath + "-wal"));
                Assert.IsFalse(File.Exists(paths.CandidateDatabaseFilePath + "-shm"));
                Assert.IsFalse(File.Exists(paths.LockFilePath));
            }
        }

        [TestMethod]
        public void Migrate_ProcessFailure_LeavesSourceDatabaseAndMetadataUnchanged()
        {
            using (var scope = new TestScope())
            {
                scope.ProcessFailure = true;

                var sourceBefore = scope.Hash(scope.DatabasePath);
                var metadataBefore = scope.Hash(scope.MetadataPath);

                Assert.ThrowsExactly<InvalidDataException>
                (
                    () => scope.Migrate()
                );

                Assert.AreEqual(sourceBefore, scope.Hash(scope.DatabasePath));
                Assert.AreEqual(metadataBefore, scope.Hash(scope.MetadataPath));
            }
        }

        [TestMethod]
        public void Migrate_ProcessReturnsNull_CleansCandidateAndLeavesPreparingCandidate()
        {
            using (var scope = new TestScope())
            {
                scope.ReturnNullProcessResult = true;
                scope.WriteCandidateBeforeNullProcessResult = true;

                Assert.ThrowsExactly<InvalidDataException>
                (
                    () => scope.Migrate()
                );

                var journal = scope.JournalStore.Load();

                Assert.AreEqual
                (
                    JasonQueryDbStorageMigrationStage.PreparingCandidate,
                    journal.Stage
                );

                var paths = JasonQueryDbStorageMigrationArtifactPaths.Create
                (
                    scope.DatabasePath,
                    scope.MetadataPath,
                    journal.OperationId
                );

                Assert.IsFalse(File.Exists(paths.CandidateDatabaseFilePath));
            }
        }

        [TestMethod]
        public void Migrate_ProcessReturnsWithoutCandidate_LeavesPreparingCandidate()
        {
            using (var scope = new TestScope())
            {
                scope.SkipCandidateWrite = true;

                Assert.ThrowsExactly<FileNotFoundException>
                (
                    () => scope.Migrate()
                );

                Assert.AreEqual
                (
                    JasonQueryDbStorageMigrationStage.PreparingCandidate,
                    scope.JournalStore.Load().Stage
                );
            }
        }

        [TestMethod]
        public void Migrate_CandidateSidecarBeforeCandidateReady_FailsAndCleansSidecar()
        {
            using (var scope = new TestScope())
            {
                scope.WriteCandidateSidecar = true;

                Assert.ThrowsExactly<InvalidDataException>
                (
                    () => scope.Migrate()
                );

                var journal = scope.JournalStore.Load();

                var paths = JasonQueryDbStorageMigrationArtifactPaths.Create
                (
                    scope.DatabasePath,
                    scope.MetadataPath,
                    journal.OperationId
                );

                Assert.AreEqual
                (
                    JasonQueryDbStorageMigrationStage.PreparingCandidate,
                    journal.Stage
                );

                Assert.IsFalse(File.Exists(paths.CandidateDatabaseFilePath));
                Assert.IsFalse(File.Exists(paths.CandidateDatabaseFilePath + "-wal"));
            }
        }

        [TestMethod]
        public void Migrate_ValidatorFailure_PreservesCandidateReadyForRecovery()
        {
            using (var scope = new TestScope())
            {
                scope.ValidatorFailure = true;

                Assert.ThrowsExactly<InvalidDataException>
                (
                    () => scope.Migrate()
                );

                var journal = scope.JournalStore.Load();

                Assert.AreEqual
                (
                    JasonQueryDbStorageMigrationStage.CandidateReady,
                    journal.Stage
                );

                var paths = JasonQueryDbStorageMigrationArtifactPaths.Create
                (
                    scope.DatabasePath,
                    scope.MetadataPath,
                    journal.OperationId
                );

                Assert.IsTrue(File.Exists(paths.CandidateDatabaseFilePath));
                Assert.IsFalse(File.Exists(paths.MetadataTemporaryFilePath));
                Assert.IsFalse(File.Exists(paths.LockFilePath));
            }
        }

        [TestMethod]
        public void Migrate_ValidatorWrongHash_PreservesCandidateReadyForRecovery()
        {
            using (var scope = new TestScope())
            {
                scope.ValidatorReturnsWrongHash = true;

                Assert.ThrowsExactly<InvalidDataException>
                (
                    () => scope.Migrate()
                );

                var journal = scope.JournalStore.Load();

                Assert.AreEqual
                (
                    JasonQueryDbStorageMigrationStage.CandidateReady,
                    journal.Stage
                );

                var paths = JasonQueryDbStorageMigrationArtifactPaths.Create
                (
                    scope.DatabasePath,
                    scope.MetadataPath,
                    journal.OperationId
                );

                Assert.IsTrue(File.Exists(paths.CandidateDatabaseFilePath));
                Assert.IsFalse(File.Exists(paths.MetadataTemporaryFilePath));
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
                    "JasonQuery-Step389F-R4D-" + Guid.NewGuid().ToString("N")
                );

                Directory.CreateDirectory(DirectoryPath);

                DatabasePath = Path.Combine(DirectoryPath, "JasonQuery.db");

                MetadataPath = Path.Combine
                (
                    DirectoryPath,
                    JasonQueryDbSecurityConstants.MetadataFileName
                );

                LegacyHelperPath = Path.Combine(DirectoryPath, "LegacyHelper.exe");
                ModernHelperPath = Path.Combine(DirectoryPath, "ModernHelper.exe");

                _sourceDatabaseBytes = Encoding.ASCII.GetBytes
                (
                    "SOURCE-DATABASE-R4D"
                );

                _candidateDatabaseBytes = Encoding.ASCII.GetBytes
                (
                    "CANDIDATE-DATABASE-R4D"
                );

                File.WriteAllBytes(DatabasePath, _sourceDatabaseBytes);
                File.WriteAllBytes(LegacyHelperPath, new byte[] { 1, 2, 3 });
                File.WriteAllBytes(ModernHelperPath, new byte[] { 4, 5, 6 });

                SourceMetadata = JasonQueryDbSecurityMetadata.CreateWindowsCurrentUser
                (
                    Convert.ToBase64String(new byte[] { 2, 4, 6, 8, 10, 12 })
                );

                SourceMetadata.StorageFormatVersion = JasonQueryDbStorageFormatContract.LegacyVersion;

                new JasonQueryDbSecurityMetadataStore(MetadataPath).Save(SourceMetadata);

                SourcePassword = Encoding.UTF8.GetBytes("legacy-source-password");

                TargetPassword = Encoding.UTF8.GetBytes
                (
                    Convert.ToBase64String(new byte[32])
                );

                CandidateDatabaseSha256 = JasonQueryDbStorageMigrationFileIntegrity.ComputeSha256
                (
                    _candidateDatabaseBytes
                );

                TargetMetadataSha256 = JasonQueryDbStorageMigrationMetadataContract.ComputeSerializedSha256
                (
                    JasonQueryDbStorageMigrationMetadataContract.CreateTargetMetadata
                    (
                        SourceMetadata
                    )
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

                Coordinator = new JasonQueryDbStorageMigrationCoordinator
                (
                    DatabasePath,
                    MetadataPath,
                    CreateCandidate,
                    ValidateCandidate
                );
            }

            public string DirectoryPath { get; }

            public string DatabasePath { get; }

            public string MetadataPath { get; }

            public string LegacyHelperPath { get; }

            public string ModernHelperPath { get; }

            public JasonQueryDbSecurityMetadata SourceMetadata { get; }

            public byte[] SourcePassword { get; }

            public byte[] TargetPassword { get; }

            public string CandidateDatabaseSha256 { get; }

            public string TargetMetadataSha256 { get; }

            public JasonQueryDbStorageMigrationJournalStore JournalStore { get; }

            public JasonQueryDbStorageMigrationRecovery Recovery { get; }

            public JasonQueryDbStorageMigrationCoordinator Coordinator { get; }

            public long LogicalBytesForwarded { get; } = 987654;

            public int ProcessCalls { get; private set; }

            public int ValidatorCalls { get; private set; }

            public bool ProcessFailure { get; set; }

            public bool WritePartialCandidateBeforeProcessFailure { get; set; }

            public bool ReturnNullProcessResult { get; set; }

            public bool WriteCandidateBeforeNullProcessResult { get; set; }

            public bool SkipCandidateWrite { get; set; }

            public bool WriteCandidateSidecar { get; set; }

            public bool ValidatorFailure { get; set; }

            public bool ValidatorReturnsWrongHash { get; set; }

            public bool ProcessObservedSourceSecretClone { get; private set; }

            public bool ProcessObservedTargetSecretClone { get; private set; }

            public bool ValidatorObservedTargetSecretClone { get; private set; }

            public bool ProcessObservedPreparedSourcePath { get; private set; }

            public bool ProcessObservedPreparedCandidatePath { get; private set; }

            public string LastCandidatePath { get; private set; }

            public JasonQueryDbStorageMigrationCoordinatorResult Migrate()
            {
                return Coordinator.Migrate
                (
                    LegacyHelperPath,
                    ModernHelperPath,
                    SourcePassword,
                    TargetPassword
                );
            }

            public string Hash(string filePath)
            {
                return JasonQueryDbStorageMigrationFileIntegrity.ComputeSha256
                (
                    filePath
                );
            }

            public void AssertCompleted()
            {
                Assert.IsFalse(JournalStore.Exists);
                Assert.AreEqual(CandidateDatabaseSha256, Hash(DatabasePath));
                Assert.AreEqual(TargetMetadataSha256, Hash(MetadataPath));

                Assert.AreEqual
                (
                    JasonQueryDbStorageMigrationRecoveryAction.NoRecoveryRequired,
                    Recovery.Evaluate().Action
                );

                Assert.IsFalse
                (
                    File.Exists
                    (
                        JasonQueryDbStorageMigrationArtifactPaths.GetLockFilePath
                        (
                            DatabasePath
                        )
                    )
                );
            }

            private JasonQueryDbStorageMigrationProcessResult CreateCandidate(string legacyHelperFilePath, string modernHelperFilePath,
                                                                              string sourceDatabaseFilePath, string candidateDatabaseFilePath,
                                                                              byte[] sourcePassword, byte[] targetPassword)
            {
                ProcessCalls++;

                Assert.AreEqual(LegacyHelperPath, legacyHelperFilePath);
                Assert.AreEqual(ModernHelperPath, modernHelperFilePath);

                ProcessObservedPreparedSourcePath = string.Equals(DatabasePath, sourceDatabaseFilePath, StringComparison.OrdinalIgnoreCase);

                ProcessObservedPreparedCandidatePath = candidateDatabaseFilePath.IndexOf(".migration.", StringComparison.Ordinal) >= 0
                                                       && candidateDatabaseFilePath.EndsWith(".candidate", StringComparison.Ordinal);

                LastCandidatePath = candidateDatabaseFilePath;
                ProcessObservedSourceSecretClone = !ReferenceEquals(SourcePassword, sourcePassword) && BytesEqual(SourcePassword, sourcePassword);
                ProcessObservedTargetSecretClone = !ReferenceEquals(TargetPassword, targetPassword) && BytesEqual(TargetPassword, targetPassword);

                if (WritePartialCandidateBeforeProcessFailure)
                {
                    File.WriteAllBytes
                    (
                        candidateDatabaseFilePath,
                        new byte[] { 9, 9, 9 }
                    );

                    File.WriteAllText
                    (
                        candidateDatabaseFilePath + "-wal",
                        "PARTIAL"
                    );
                }

                if (ProcessFailure)
                {
                    throw new InvalidDataException
                    (
                        "Synthetic process-runner failure."
                    );
                }

                if (WriteCandidateBeforeNullProcessResult)
                {
                    File.WriteAllBytes
                    (
                        candidateDatabaseFilePath,
                        _candidateDatabaseBytes
                    );
                }

                if (ReturnNullProcessResult)
                {
                    return null;
                }

                if (!SkipCandidateWrite)
                {
                    File.WriteAllBytes
                    (
                        candidateDatabaseFilePath,
                        _candidateDatabaseBytes
                    );
                }

                if (WriteCandidateSidecar)
                {
                    File.WriteAllText
                    (
                        candidateDatabaseFilePath + "-wal",
                        "ACTIVE"
                    );
                }

                return new JasonQueryDbStorageMigrationProcessResult
                (
                    LogicalBytesForwarded
                );
            }

            private JasonQueryDbStorageV2ReadOnlyValidationResult ValidateCandidate(string modernHelperFilePath, string candidateDatabaseFilePath, byte[] targetPassword)
            {
                ValidatorCalls++;

                Assert.AreEqual(ModernHelperPath, modernHelperFilePath);
                ValidatorObservedTargetSecretClone = !ReferenceEquals(TargetPassword, targetPassword) && BytesEqual(TargetPassword, targetPassword);

                if (ValidatorFailure)
                {
                    throw new InvalidDataException
                    (
                        "Synthetic read-only validation failure."
                    );
                }

                if (ValidatorReturnsWrongHash)
                {
                    return new JasonQueryDbStorageV2ReadOnlyValidationResult
                    (
                        new string('A', 64)
                    );
                }

                return new JasonQueryDbStorageV2ReadOnlyValidationResult
                (
                    Hash(candidateDatabaseFilePath)
                );
            }

            private static bool BytesEqual(byte[] left, byte[] right)
            {
                if (left == null || right == null || left.Length != right.Length)
                {
                    return false;
                }

                var difference = 0;

                for (var index = 0; index < left.Length; index++)
                {
                    difference |= left[index] ^ right[index];
                }

                return difference == 0;
            }

            public void Dispose()
            {
                Array.Clear(_sourceDatabaseBytes, 0, _sourceDatabaseBytes.Length);
                Array.Clear(_candidateDatabaseBytes, 0, _candidateDatabaseBytes.Length);
                Array.Clear(SourcePassword, 0, SourcePassword.Length);
                Array.Clear(TargetPassword, 0, TargetPassword.Length);

                if (Directory.Exists(DirectoryPath))
                {
                    Directory.Delete(DirectoryPath, true);
                }
            }
        }
    }
}
