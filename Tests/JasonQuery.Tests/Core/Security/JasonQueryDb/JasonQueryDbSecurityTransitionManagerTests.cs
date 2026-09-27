using JasonQuery.Core.Security.JasonQueryDb;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Text;

namespace JasonQuery.Tests.Core.Security.JasonQueryDb
{
    [TestClass]
    public class JasonQueryDbSecurityTransitionManagerTests
    {
        [TestMethod]
        public void ChangeToCustomPassword_WindowsProtectedSource_CommitsTarget()
        {
            using (var scope = new TestScope())
            {
                const string sourcePassword = "SOURCE-WINDOWS-KEY";

                scope.CreateWindowsSource(sourcePassword);

                var result = scope.Manager.ChangeToCustomPassword(scope.DatabasePath, sourcePassword, "New Custom Password");

                Assert.AreEqual(JasonQueryDbSecurityMode.CustomPassword, result.Metadata.Mode);
                Assert.IsTrue(result.Metadata.StorageFormatVersion.HasValue);
                Assert.AreEqual(JasonQueryDbStorageFormatContract.LegacyVersion, result.Metadata.StorageFormatVersion.Value);
                Assert.IsTrue(scope.Database.CanOpen(scope.DatabasePath, result.DatabasePassword));
                Assert.IsFalse(scope.Database.CanOpen(scope.DatabasePath, sourcePassword));
                Assert.AreEqual(JasonQueryDbSecurityMode.CustomPassword, scope.MetadataStore.Load().Mode);
                scope.AssertNoTransitionArtifacts();
            }
        }

        [TestMethod]
        public void ChangeToWindowsCurrentUser_CustomPasswordSource_CommitsTarget()
        {
            using (var scope = new TestScope())
            {
                var sourcePassword = scope.CreateCustomSource("Current Custom Password");
                var result = scope.Manager.ChangeToWindowsCurrentUser(scope.DatabasePath, sourcePassword);

                Assert.AreEqual(JasonQueryDbSecurityMode.WindowsCurrentUser, result.Metadata.Mode);
                Assert.IsTrue(result.Metadata.StorageFormatVersion.HasValue);
                Assert.AreEqual(JasonQueryDbStorageFormatContract.LegacyVersion, result.Metadata.StorageFormatVersion.Value);
                Assert.IsTrue(scope.Database.CanOpen(scope.DatabasePath, result.DatabasePassword));
                Assert.IsFalse(scope.Database.CanOpen(scope.DatabasePath, sourcePassword));
                scope.AssertNoTransitionArtifacts();
            }
        }

        [TestMethod]
        public void ChangeToCustomPassword_CustomPasswordSource_RotatesSaltAndKey()
        {
            using (var scope = new TestScope())
            {
                var sourcePassword = scope.CreateCustomSource("Same User Password");
                var oldSalt = scope.MetadataStore.Load().Salt;
                var result = scope.Manager.ChangeToCustomPassword(scope.DatabasePath, sourcePassword, "Same User Password");

                Assert.AreNotEqual(oldSalt, result.Metadata.Salt);
                Assert.AreNotEqual(sourcePassword, result.DatabasePassword);
                Assert.IsTrue(scope.Database.CanOpen(scope.DatabasePath, result.DatabasePassword));
                scope.AssertNoTransitionArtifacts();
            }
        }

        [TestMethod]
        public void ChangeToCustomPassword_MissingStorageMarker_CanonicalizesLegacyVersion()
        {
            using (var scope = new TestScope())
            {
                const string sourcePassword = "SOURCE-WINDOWS-KEY";

                scope.CreateWindowsSource(sourcePassword);

                var sourceMetadata = scope.MetadataStore.Load();

                sourceMetadata.StorageFormatVersion = null;

                scope.MetadataStore.Save(sourceMetadata);

                var persistedLegacyMetadata = scope.MetadataStore.Load();

                Assert.IsFalse(persistedLegacyMetadata.StorageFormatVersion.HasValue);

                var result = scope.Manager.ChangeToCustomPassword
                (
                    scope.DatabasePath,
                    sourcePassword,
                    "New Custom Password"
                );

                Assert.IsTrue
                (
                    result.Metadata.StorageFormatVersion.HasValue
                );

                Assert.AreEqual
                (
                    JasonQueryDbStorageFormatContract.LegacyVersion,
                    result.Metadata.StorageFormatVersion.Value
                );

                var committedMetadata = scope.MetadataStore.Load();

                Assert.IsTrue
                (
                    committedMetadata.StorageFormatVersion.HasValue
                );

                Assert.AreEqual
                (
                    JasonQueryDbStorageFormatContract.LegacyVersion,
                    committedMetadata.StorageFormatVersion.Value
                );

                Assert.IsTrue
                (
                    scope.Database.CanOpen
                    (
                        scope.DatabasePath,
                        result.DatabasePassword
                    )
                );

                scope.AssertNoTransitionArtifacts();
            }
        }

        [TestMethod]
        public void RecoverInterruptedChange_SourcePair_RollsBackCandidate()
        {
            using (var scope = new TestScope())
            {
                const string sourcePassword = "SOURCE-KEY";

                scope.CreateWindowsSource(sourcePassword);

                var target = scope.CreateWindowsTarget();

                scope.CreateInterruptedJournal(sourcePassword, target.Password, target.Metadata);
                File.WriteAllText(JasonQueryDbSecurityTransitionManager.GetCandidateFilePath(scope.DatabasePath), target.Password, Encoding.UTF8);

                var result = scope.Manager.RecoverInterruptedChangeIfNeeded(scope.DatabasePath);

                Assert.AreEqual(sourcePassword, result.DatabasePassword);
                Assert.IsTrue(scope.Database.CanOpen(scope.DatabasePath, sourcePassword));
                scope.AssertNoTransitionArtifacts();
                target.Clear();
            }
        }

        [TestMethod]
        public void RecoverInterruptedChange_TargetDatabaseSourceMetadata_CompletesCommit()
        {
            using (var scope = new TestScope())
            {
                const string sourcePassword = "SOURCE-KEY";

                scope.CreateWindowsSource(sourcePassword);

                var target = scope.CreateWindowsTarget();

                scope.CreateInterruptedJournal(sourcePassword, target.Password, target.Metadata);
                File.Copy(scope.DatabasePath, JasonQueryDbSecurityTransitionManager.GetBackupFilePath(scope.DatabasePath));
                File.WriteAllText(scope.DatabasePath, target.Password, Encoding.UTF8);

                var result = scope.Manager.RecoverInterruptedChangeIfNeeded(scope.DatabasePath);

                Assert.AreEqual(target.Password, result.DatabasePassword);
                Assert.AreEqual(target.Metadata.ProtectedDatabaseKey, scope.MetadataStore.Load().ProtectedDatabaseKey);
                scope.AssertNoTransitionArtifacts();
                target.Clear();
            }
        }

        [TestMethod]
        public void RecoverInterruptedChange_BackupWithoutJournal_FailsClosed()
        {
            using (var scope = new TestScope())
            {
                scope.CreateWindowsSource("SOURCE-KEY");
                File.Copy(scope.DatabasePath, JasonQueryDbSecurityTransitionManager.GetBackupFilePath(scope.DatabasePath));

                Assert.ThrowsExactly<InvalidDataException>
                (
                    () => scope.Manager.RecoverInterruptedChangeIfNeeded(scope.DatabasePath)
                );
            }
        }

        private sealed class TargetInfo
        {
            public string Password { get; set; }
            public JasonQueryDbSecurityMetadata Metadata { get; set; }
            public byte[] Key { get; set; }
            public byte[] ProtectedKey { get; set; }

            public void Clear()
            {
                if (Key != null)
                {
                    Array.Clear(Key, 0, Key.Length);
                }

                if (ProtectedKey != null)
                {
                    Array.Clear(ProtectedKey, 0, ProtectedKey.Length);
                }
            }
        }

        private sealed class TestScope : IDisposable
        {
            public TestScope()
            {
                DirectoryPath = Path.Combine(Path.GetTempPath(), "JasonQuery.Tests", Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(DirectoryPath);
                DatabasePath = Path.Combine(DirectoryPath, "JasonQuery.db");
                MetadataStore = new JasonQueryDbSecurityMetadataStore(Path.Combine(DirectoryPath, JasonQueryDbSecurityConstants.MetadataFileName));
                Protector = new PassThroughKeyProtector();
                Database = new FakeMigrationDatabase();
                JournalStore = new JasonQueryDbSecurityTransitionJournalStore(JasonQueryDbSecurityTransitionManager.GetJournalFilePath(DatabasePath));
                Manager = new JasonQueryDbSecurityTransitionManager(MetadataStore, Protector, Database, JournalStore);
            }

            public string DirectoryPath { get; }
            public string DatabasePath { get; }
            public JasonQueryDbSecurityMetadataStore MetadataStore { get; }
            public PassThroughKeyProtector Protector { get; }
            public FakeMigrationDatabase Database { get; }
            public JasonQueryDbSecurityTransitionJournalStore JournalStore { get; }
            public JasonQueryDbSecurityTransitionManager Manager { get; }

            public void CreateWindowsSource(string databasePassword)
            {
                var key = Encoding.UTF8.GetBytes(databasePassword);

                try
                {
                    var protectedKey = Protector.Protect(key);

                    try
                    {
                        MetadataStore.Save(JasonQueryDbSecurityMetadata.CreateWindowsCurrentUser(Convert.ToBase64String(protectedKey)));
                    }
                    finally
                    {
                        Array.Clear(protectedKey, 0, protectedKey.Length);
                    }
                }
                finally
                {
                    Array.Clear(key, 0, key.Length);
                }

                File.WriteAllText(DatabasePath, databasePassword, Encoding.UTF8);
            }

            public string CreateCustomSource(string userPassword)
            {
                var salt = JasonQueryDbCustomPasswordKeyDeriver.CreateSalt();

                try
                {
                    var metadata = JasonQueryDbSecurityMetadata.CreateCustomPassword(salt, JasonQueryDbSecurityConstants.DefaultPbkdf2Iterations);

                    MetadataStore.Save(metadata);

                    var databasePassword = JasonQueryDbCustomPasswordKeyDeriver.DeriveDatabasePassword(userPassword, salt, metadata.Iterations);

                    File.WriteAllText(DatabasePath, databasePassword, Encoding.UTF8);
                    return databasePassword;
                }
                finally
                {
                    Array.Clear(salt, 0, salt.Length);
                }
            }

            public TargetInfo CreateWindowsTarget()
            {
                var key = JasonQueryDbKeyGenerator.Generate();
                var protectedKey = Protector.Protect(key);

                return new TargetInfo
                {
                    Key = key,
                    ProtectedKey = protectedKey,
                    Password = JasonQueryDbKeyGenerator.ToDatabasePassword(key),
                    Metadata = JasonQueryDbSecurityMetadata.CreateWindowsCurrentUser(Convert.ToBase64String(protectedKey))
                };
            }

            public void CreateInterruptedJournal(string sourcePassword, string targetPassword, JasonQueryDbSecurityMetadata targetMetadata)
            {
                var sourceBytes = Encoding.UTF8.GetBytes(sourcePassword);
                var targetBytes = Encoding.UTF8.GetBytes(targetPassword);

                try
                {
                    var protectedSource = Protector.Protect(sourceBytes);
                    var protectedTarget = Protector.Protect(targetBytes);

                    try
                    {
                        JournalStore.Save(new JasonQueryDbSecurityTransitionJournal
                        {
                            TransitionVersion = JasonQueryDbSecurityTransitionJournal.CurrentVersion,
                            SourceMetadata = MetadataStore.Load(),
                            TargetMetadata = targetMetadata,
                            ProtectedSourceDatabasePassword = Convert.ToBase64String(protectedSource),
                            ProtectedTargetDatabasePassword = Convert.ToBase64String(protectedTarget)
                        });
                    }
                    finally
                    {
                        Array.Clear(protectedSource, 0, protectedSource.Length);
                        Array.Clear(protectedTarget, 0, protectedTarget.Length);
                    }
                }
                finally
                {
                    Array.Clear(sourceBytes, 0, sourceBytes.Length);
                    Array.Clear(targetBytes, 0, targetBytes.Length);
                }
            }

            public void AssertNoTransitionArtifacts()
            {
                Assert.IsFalse(JournalStore.Exists);
                Assert.IsFalse(File.Exists(JasonQueryDbSecurityTransitionManager.GetCandidateFilePath(DatabasePath)));
                Assert.IsFalse(File.Exists(JasonQueryDbSecurityTransitionManager.GetBackupFilePath(DatabasePath)));
                Assert.IsFalse(File.Exists(JasonQueryDbSecurityTransitionManager.GetRollbackScratchFilePath(DatabasePath)));
            }

            public void Dispose()
            {
                if (Directory.Exists(DirectoryPath)) Directory.Delete(DirectoryPath, true);
            }
        }

        private sealed class PassThroughKeyProtector : IJasonQueryDbKeyProtector
        {
            public byte[] Protect(byte[] databaseKey) => (byte[])databaseKey.Clone();
            public byte[] Unprotect(byte[] protectedDatabaseKey) => (byte[])protectedDatabaseKey.Clone();
        }

        private sealed class FakeMigrationDatabase : IJasonQueryDbMigrationDatabase
        {
            public bool CanOpen(string databaseFilePath, string databasePassword)
            {
                return File.Exists(databaseFilePath) && string.Equals(File.ReadAllText(databaseFilePath, Encoding.UTF8), databasePassword, StringComparison.Ordinal);
            }

            public void CreateVerifiedCopy(string sourceDatabaseFilePath, string destinationDatabaseFilePath, string currentDatabasePassword)
            {
                if (!CanOpen(sourceDatabaseFilePath, currentDatabasePassword))
                {
                    throw new InvalidDataException("Fake source password mismatch.");
                }

                File.Copy(sourceDatabaseFilePath, destinationDatabaseFilePath);
            }

            public void ChangePassword(string databaseFilePath, string currentDatabasePassword, string newDatabasePassword)
            {
                if (!CanOpen(databaseFilePath, currentDatabasePassword))
                {
                    throw new InvalidDataException("Fake current password mismatch.");
                }

                File.WriteAllText(databaseFilePath, newDatabasePassword, Encoding.UTF8);
            }
        }
    }
}
