using JasonQuery.Core.Security.JasonQueryDb;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Text;

namespace JasonQuery.Tests.Core.Security.JasonQueryDb
{
    [TestClass]
    public class JasonQueryDbStorageMigrationCandidateReadyExecutorTests
    {
        [TestMethod]
        public void Constructor_EmptyDatabasePath_Throws()
        {
            Assert.ThrowsExactly<ArgumentException>
            (
                () => new JasonQueryDbStorageMigrationCandidateReadyExecutor
                (
                    " ",
                    Path.Combine(Path.GetTempPath(), "metadata.json")
                )
            );
        }

        [TestMethod]
        public void Constructor_EmptyMetadataPath_Throws()
        {
            Assert.ThrowsExactly<ArgumentException>
            (
                () => new JasonQueryDbStorageMigrationCandidateReadyExecutor
                (
                    Path.Combine(Path.GetTempPath(), "JasonQuery.db"),
                    " "
                )
            );
        }

        [TestMethod]
        public void PrepareReplace_NullTargetPassword_ThrowsBeforeRecovery()
        {
            using (var scope = new TestScope(true))
            {
                Assert.ThrowsExactly<ArgumentException>
                (
                    () => scope.Executor.PrepareReplace
                    (
                        scope.ModernHelperPath,
                        null
                    )
                );

                Assert.AreEqual(0, scope.ValidationCalls);

                Assert.AreEqual
                (
                    JasonQueryDbStorageMigrationStage.CandidateReady,
                    scope.JournalStore.Load().Stage
                );
            }
        }

        [TestMethod]
        public void PrepareReplace_OversizedTargetPassword_ThrowsBeforeRecovery()
        {
            using (var scope = new TestScope(true))
            {
                var password = new byte[ JasonQueryDbStorageV2ReadOnlyValidatorProtocol.MaxDatabasePasswordUtf8Bytes + 1 ];

                try
                {
                    Assert.ThrowsExactly<ArgumentException>
                    (
                        () => scope.Executor.PrepareReplace
                        (
                            scope.ModernHelperPath,
                            password
                        )
                    );
                }
                finally
                {
                    Array.Clear(password, 0, password.Length);
                }

                Assert.AreEqual(0, scope.ValidationCalls);

                Assert.AreEqual
                (
                    JasonQueryDbStorageMigrationStage.CandidateReady,
                    scope.JournalStore.Load().Stage
                );
            }
        }

        [TestMethod]
        public void PrepareReplace_NoJournal_RejectsNonCandidateReadyState()
        {
            using (var scope = new TestScope(false))
            {
                Assert.ThrowsExactly<InvalidOperationException>
                (
                    () => scope.Executor.PrepareReplace(scope.ModernHelperPath, scope.Password)
                );

                Assert.AreEqual(0, scope.ValidationCalls);
            }
        }

        [TestMethod]
        public void PrepareReplace_CandidateReady_CreatesDurableReplacePreparedBoundary()
        {
            using (var scope = new TestScope(true))
            {
                var databaseBefore = scope.GetDatabaseHash();
                var metadataBefore = scope.GetMetadataHash();
                var candidateBefore = scope.GetCandidateHash();
                var passwordBefore = (byte[])scope.Password.Clone();

                var result = scope.Executor.PrepareReplace
                (
                    scope.ModernHelperPath,
                    scope.Password
                );

                Assert.AreEqual(1, scope.ValidationCalls);
                Assert.AreEqual(JasonQueryDbStorageMigrationStage.ReplacePrepared, result.Journal.Stage);
                Assert.AreEqual(scope.OperationId, result.Journal.OperationId);
                Assert.AreEqual(scope.CandidateDatabaseSha256, result.Journal.CandidateDatabaseSha256);
                Assert.AreEqual(databaseBefore, scope.GetDatabaseHash());
                Assert.AreEqual(metadataBefore, scope.GetMetadataHash());
                Assert.AreEqual(candidateBefore, scope.GetCandidateHash());
                CollectionAssert.AreEqual(passwordBefore, scope.Password);

                Assert.IsTrue(File.Exists(scope.Paths.MetadataTemporaryFilePath));

                Assert.AreEqual
                (
                    scope.TargetMetadataSha256,
                    JasonQueryDbStorageMigrationFileIntegrity.ComputeSha256
                    (
                        scope.Paths.MetadataTemporaryFilePath
                    )
                );

                Assert.AreEqual
                (
                    JasonQueryDbStorageMigrationRecoveryAction.ReplaceDatabase,
                    scope.Recovery.Evaluate().Action
                );

                Assert.IsFalse(File.Exists(scope.Paths.DatabaseBackupFilePath));
                Assert.IsFalse(File.Exists(scope.Paths.MetadataBackupFilePath));
            }
        }

        [TestMethod]
        public void PrepareReplace_ExistingUncommittedMetadataTemp_IsRecreatedDurably()
        {
            using (var scope = new TestScope(true))
            {
                File.WriteAllText
                (
                    scope.Paths.MetadataTemporaryFilePath,
                    "STALE-UNCOMMITTED-TEMP",
                    Encoding.UTF8
                );

                var result = scope.Executor.PrepareReplace
                (
                    scope.ModernHelperPath,
                    scope.Password
                );

                Assert.AreEqual(JasonQueryDbStorageMigrationStage.ReplacePrepared, result.Journal.Stage);

                Assert.AreEqual
                (
                    scope.TargetMetadataSha256,
                    JasonQueryDbStorageMigrationFileIntegrity.ComputeSha256
                    (
                        scope.Paths.MetadataTemporaryFilePath
                    )
                );
            }
        }

        [TestMethod]
        public void PrepareReplace_MetadataTempDirectory_FailsClosedBeforeJournalAdvance()
        {
            using (var scope = new TestScope(true))
            {
                Directory.CreateDirectory(scope.Paths.MetadataTemporaryFilePath);

                Assert.ThrowsExactly<InvalidDataException>
                (
                    () => scope.Executor.PrepareReplace
                    (
                        scope.ModernHelperPath,
                        scope.Password
                    )
                );

                Assert.AreEqual
                (
                    JasonQueryDbStorageMigrationStage.CandidateReady,
                    scope.JournalStore.Load().Stage
                );

                Assert.IsTrue(Directory.Exists(scope.Paths.MetadataTemporaryFilePath));
            }
        }

        [TestMethod]
        public void PrepareReplace_ValidatorThrows_DoesNotPrepareMetadataOrAdvanceJournal()
        {
            using (var scope = new TestScope(true, throwValidationFailure: true))
            {
                Assert.ThrowsExactly<InvalidDataException>
                (
                    () => scope.Executor.PrepareReplace
                    (
                        scope.ModernHelperPath,
                        scope.Password
                    )
                );

                Assert.AreEqual
                (
                    JasonQueryDbStorageMigrationStage.CandidateReady,
                    scope.JournalStore.Load().Stage
                );

                Assert.IsFalse(File.Exists(scope.Paths.MetadataTemporaryFilePath));
            }
        }

        [TestMethod]
        public void PrepareReplace_ValidatorReturnsWrongHash_FailsClosed()
        {
            using (var scope = new TestScope(true, validationHashOverride: new string('A', 64)))
            {
                Assert.ThrowsExactly<InvalidDataException>
                (
                    () => scope.Executor.PrepareReplace
                    (
                        scope.ModernHelperPath,
                        scope.Password
                    )
                );

                Assert.AreEqual
                (
                    JasonQueryDbStorageMigrationStage.CandidateReady,
                    scope.JournalStore.Load().Stage
                );

                Assert.IsFalse(File.Exists(scope.Paths.MetadataTemporaryFilePath));
            }
        }

        [TestMethod]
        public void PrepareReplace_CandidateChangedDuringValidation_FailsClosed()
        {
            using (var scope = new TestScope(true, mutateCandidateDuringValidation: true))
            {
                Assert.ThrowsExactly<InvalidDataException>
                (
                    () => scope.Executor.PrepareReplace
                    (
                        scope.ModernHelperPath,
                        scope.Password
                    )
                );

                Assert.AreEqual
                (
                    JasonQueryDbStorageMigrationStage.CandidateReady,
                    scope.JournalStore.Load().Stage
                );

                Assert.IsFalse(File.Exists(scope.Paths.MetadataTemporaryFilePath));
            }
        }

        [TestMethod]
        public void PrepareReplace_SourceDatabaseChangedDuringValidation_FailsClosed()
        {
            using (var scope = new TestScope(true, mutateSourceDatabaseDuringValidation: true))
            {
                Assert.ThrowsExactly<InvalidDataException>
                (
                    () => scope.Executor.PrepareReplace
                    (
                        scope.ModernHelperPath,
                        scope.Password
                    )
                );

                Assert.AreEqual
                (
                    JasonQueryDbStorageMigrationStage.CandidateReady,
                    scope.JournalStore.Load().Stage
                );

                Assert.IsFalse(File.Exists(scope.Paths.MetadataTemporaryFilePath));
            }
        }

        [TestMethod]
        public void PrepareReplace_SourceMetadataChangedDuringValidation_FailsClosed()
        {
            using (var scope = new TestScope(true, mutateSourceMetadataDuringValidation: true))
            {
                Assert.ThrowsExactly<InvalidDataException>
                (
                    () => scope.Executor.PrepareReplace
                    (
                        scope.ModernHelperPath,
                        scope.Password
                    )
                );

                Assert.AreEqual
                (
                    JasonQueryDbStorageMigrationStage.CandidateReady,
                    scope.JournalStore.Load().Stage
                );

                Assert.IsFalse(File.Exists(scope.Paths.MetadataTemporaryFilePath));
            }
        }

        [TestMethod]
        public void PrepareReplace_CandidateSidecar_FailsBeforeValidation()
        {
            using (var scope = new TestScope(true))
            {
                File.WriteAllText
                (
                    scope.Paths.CandidateDatabaseFilePath + "-wal",
                    "ACTIVE",
                    Encoding.UTF8
                );

                Assert.ThrowsExactly<InvalidDataException>
                (
                    () => scope.Executor.PrepareReplace
                    (
                        scope.ModernHelperPath,
                        scope.Password
                    )
                );

                Assert.AreEqual(0, scope.ValidationCalls);

                Assert.AreEqual
                (
                    JasonQueryDbStorageMigrationStage.CandidateReady,
                    scope.JournalStore.Load().Stage
                );
            }
        }

        [TestMethod]
        public void PrepareReplace_DatabaseBackupAlreadyExists_FailsBeforeValidation()
        {
            using (var scope = new TestScope(true))
            {
                File.WriteAllText
                (
                    scope.Paths.DatabaseBackupFilePath,
                    "UNEXPECTED-BACKUP",
                    Encoding.UTF8
                );

                Assert.ThrowsExactly<InvalidDataException>
                (
                    () => scope.Executor.PrepareReplace
                    (
                        scope.ModernHelperPath,
                        scope.Password
                    )
                );

                Assert.AreEqual(0, scope.ValidationCalls);

                Assert.AreEqual
                (
                    JasonQueryDbStorageMigrationStage.CandidateReady,
                    scope.JournalStore.Load().Stage
                );
            }
        }

        [TestMethod]
        public void PrepareReplace_JournalChangedDuringValidation_FailsClosed()
        {
            using (var scope = new TestScope(true, advanceJournalDuringValidation: true))
            {
                Assert.ThrowsExactly<InvalidDataException>
                (
                    () => scope.Executor.PrepareReplace
                    (
                        scope.ModernHelperPath,
                        scope.Password
                    )
                );

                Assert.AreEqual
                (
                    JasonQueryDbStorageMigrationStage.ReplacePrepared,
                    scope.JournalStore.Load().Stage
                );

                Assert.IsFalse(File.Exists(scope.Paths.MetadataTemporaryFilePath));
            }
        }

        private sealed class TestScope : IDisposable
        {
            private readonly bool _throwValidationFailure;
            private readonly string _validationHashOverride;
            private readonly bool _mutateCandidateDuringValidation;
            private readonly bool _mutateSourceDatabaseDuringValidation;
            private readonly bool _mutateSourceMetadataDuringValidation;
            private readonly bool _advanceJournalDuringValidation;
            private readonly byte[] _candidateBytes;

            public TestScope(bool createCandidateReadyJournal, bool throwValidationFailure = false, string validationHashOverride = null,
                             bool mutateCandidateDuringValidation = false, bool mutateSourceDatabaseDuringValidation = false,
                             bool mutateSourceMetadataDuringValidation = false, bool advanceJournalDuringValidation = false)
            {
                _throwValidationFailure = throwValidationFailure;
                _validationHashOverride = validationHashOverride;
                _mutateCandidateDuringValidation = mutateCandidateDuringValidation;
                _mutateSourceDatabaseDuringValidation = mutateSourceDatabaseDuringValidation;
                _mutateSourceMetadataDuringValidation = mutateSourceMetadataDuringValidation;
                _advanceJournalDuringValidation = advanceJournalDuringValidation;

                DirectoryPath = Path.Combine
                (
                    Path.GetTempPath(),
                    "JasonQuery-Step389F-R4C1-" + Guid.NewGuid().ToString("N")
                );

                Directory.CreateDirectory(DirectoryPath);
                DatabasePath = Path.Combine(DirectoryPath, "JasonQuery.db");

                MetadataPath = Path.Combine
                (
                    DirectoryPath,
                    JasonQueryDbSecurityConstants.MetadataFileName
                );

                ModernHelperPath = Path.Combine(DirectoryPath, "ModernHelper.exe");
                OperationId = Guid.NewGuid().ToString("N");

                File.WriteAllBytes
                (
                    DatabasePath,
                    Encoding.ASCII.GetBytes("SOURCE-DATABASE-R4C1")
                );

                File.WriteAllBytes
                (
                    ModernHelperPath,
                    new byte[] { 1, 2, 3, 4 }
                );

                SourceMetadata = JasonQueryDbSecurityMetadata.CreateWindowsCurrentUser
                (
                    Convert.ToBase64String(new byte[] { 2, 4, 6, 8, 10, 12 })
                );

                SourceMetadata.StorageFormatVersion = JasonQueryDbStorageFormatContract.LegacyVersion;

                var metadataStore = new JasonQueryDbSecurityMetadataStore
                (
                    MetadataPath
                );

                metadataStore.Save(SourceMetadata);

                TargetMetadata = JasonQueryDbStorageMigrationMetadataContract.CreateTargetMetadata
                (
                    SourceMetadata
                );

                SourceDatabaseSha256 = JasonQueryDbStorageMigrationFileIntegrity.ComputeSha256(DatabasePath);
                SourceMetadataSha256 = JasonQueryDbStorageMigrationFileIntegrity.ComputeSha256(MetadataPath);
                TargetMetadataSha256 = JasonQueryDbStorageMigrationMetadataContract.ComputeSerializedSha256(TargetMetadata);

                _candidateBytes = Encoding.ASCII.GetBytes
                (
                    "CANDIDATE-DATABASE-R4C1"
                );

                CandidateDatabaseSha256 = JasonQueryDbStorageMigrationFileIntegrity.ComputeSha256(_candidateBytes);

                Paths = JasonQueryDbStorageMigrationArtifactPaths.Create
                (
                    DatabasePath,
                    MetadataPath,
                    OperationId
                );

                if (createCandidateReadyJournal)
                {
                    File.WriteAllBytes
                    (
                        Paths.CandidateDatabaseFilePath,
                        _candidateBytes
                    );
                }

                Password = Encoding.UTF8.GetBytes
                (
                    Convert.ToBase64String(new byte[32])
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

                if (createCandidateReadyJournal)
                {
                    JournalStore.Save
                    (
                        CreateJournal(JasonQueryDbStorageMigrationStage.PreparingCandidate)
                    );

                    JournalStore.Save
                    (
                        CreateJournal(JasonQueryDbStorageMigrationStage.CandidateReady)
                    );
                }

                Executor = new JasonQueryDbStorageMigrationCandidateReadyExecutor
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

            public string CandidateDatabaseSha256 { get; }

            public string SourceMetadataSha256 { get; }

            public string TargetMetadataSha256 { get; }

            public JasonQueryDbSecurityMetadata SourceMetadata { get; }

            public JasonQueryDbSecurityMetadata TargetMetadata { get; }

            public JasonQueryDbStorageMigrationArtifactPaths Paths { get; }

            public JasonQueryDbStorageMigrationJournalStore JournalStore { get; }

            public JasonQueryDbStorageMigrationRecovery Recovery { get; }

            public JasonQueryDbStorageMigrationCandidateReadyExecutor Executor { get; }

            public byte[] Password { get; }

            public int ValidationCalls { get; private set; }

            public string GetDatabaseHash()
            {
                return JasonQueryDbStorageMigrationFileIntegrity.ComputeSha256(DatabasePath);
            }

            public string GetMetadataHash()
            {
                return JasonQueryDbStorageMigrationFileIntegrity.ComputeSha256(MetadataPath);
            }

            public string GetCandidateHash()
            {
                return JasonQueryDbStorageMigrationFileIntegrity.ComputeSha256
                (
                    Paths.CandidateDatabaseFilePath
                );
            }

            private JasonQueryDbStorageV2ReadOnlyValidationResult ValidateCandidate(string modernHelperFilePath, string candidateDatabaseFilePath, byte[] databasePasswordUtf8)
            {
                ValidationCalls++;

                Assert.AreEqual(ModernHelperPath, modernHelperFilePath);
                Assert.AreEqual(Paths.CandidateDatabaseFilePath, candidateDatabaseFilePath);
                Assert.AreSame(Password, databasePasswordUtf8);

                if (_throwValidationFailure)
                {
                    throw new InvalidDataException
                    (
                        "Synthetic validation failure."
                    );
                }

                if (_mutateCandidateDuringValidation)
                {
                    File.AppendAllText
                    (
                        Paths.CandidateDatabaseFilePath,
                        "MUTATED",
                        Encoding.UTF8
                    );
                }

                if (_mutateSourceDatabaseDuringValidation)
                {
                    File.AppendAllText
                    (
                        DatabasePath,
                        "MUTATED",
                        Encoding.UTF8
                    );
                }

                if (_mutateSourceMetadataDuringValidation)
                {
                    File.AppendAllText
                    (
                        MetadataPath,
                        " ",
                        Encoding.UTF8
                    );
                }

                if (_advanceJournalDuringValidation)
                {
                    JournalStore.Save
                    (
                        CreateJournal(JasonQueryDbStorageMigrationStage.ReplacePrepared)
                    );
                }

                return new JasonQueryDbStorageV2ReadOnlyValidationResult
                (
                    _validationHashOverride ?? CandidateDatabaseSha256
                );
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
                Array.Clear(Password, 0, Password.Length);

                if (Directory.Exists(DirectoryPath))
                {
                    Directory.Delete(DirectoryPath, true);
                }
            }
        }
    }
}
