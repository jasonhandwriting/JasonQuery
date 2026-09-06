using JasonQuery.Core.Security.Database;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Text;

namespace JasonQuery.Tests.Core.Security.Database
{
    [TestClass]
    public class DatabaseSecurityTransitionManagerTests
    {
        [TestMethod]
        public void ChangeToCustomPassword_WindowsProtectedSource_CommitsTarget()
        {
            using (var scope = new TestScope())
            {
                const string sourcePassword = "SOURCE-WINDOWS-KEY";

                scope.CreateWindowsSource(sourcePassword);

                var result = scope.Manager.ChangeToCustomPassword(scope.DatabasePath, sourcePassword, "New Custom Password");

                Assert.AreEqual(DatabaseSecurityMode.CustomPassword, result.Metadata.Mode);
                Assert.IsTrue(scope.Database.CanOpen(scope.DatabasePath, result.DatabasePassword));
                Assert.IsFalse(scope.Database.CanOpen(scope.DatabasePath, sourcePassword));
                Assert.AreEqual(DatabaseSecurityMode.CustomPassword, scope.MetadataStore.Load().Mode);
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

                Assert.AreEqual(DatabaseSecurityMode.WindowsCurrentUser, result.Metadata.Mode);
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
        public void RecoverInterruptedChange_SourcePair_RollsBackCandidate()
        {
            using (var scope = new TestScope())
            {
                const string sourcePassword = "SOURCE-KEY";

                scope.CreateWindowsSource(sourcePassword);

                var target = scope.CreateWindowsTarget();

                scope.CreateInterruptedJournal(sourcePassword, target.Password, target.Metadata);
                File.WriteAllText(DatabaseSecurityTransitionManager.GetCandidateFilePath(scope.DatabasePath), target.Password, Encoding.UTF8);

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
                File.Copy(scope.DatabasePath, DatabaseSecurityTransitionManager.GetBackupFilePath(scope.DatabasePath));
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
                File.Copy(scope.DatabasePath, DatabaseSecurityTransitionManager.GetBackupFilePath(scope.DatabasePath));

                Assert.ThrowsException<InvalidDataException>
                (
                    () => scope.Manager.RecoverInterruptedChangeIfNeeded(scope.DatabasePath)
                );
            }
        }

        private sealed class TargetInfo
        {
            public string Password { get; set; }
            public DatabaseSecurityMetadata Metadata { get; set; }
            public byte[] Key { get; set; }
            public byte[] ProtectedKey { get; set; }

            public void Clear()
            {
                if (Key != null) Array.Clear(Key, 0, Key.Length);
                if (ProtectedKey != null) Array.Clear(ProtectedKey, 0, ProtectedKey.Length);
            }
        }

        private sealed class TestScope : IDisposable
        {
            public TestScope()
            {
                DirectoryPath = Path.Combine(Path.GetTempPath(), "JasonQuery.Tests", Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(DirectoryPath);
                DatabasePath = Path.Combine(DirectoryPath, "JasonQuery.db");
                MetadataStore = new DatabaseSecurityMetadataStore(Path.Combine(DirectoryPath, DatabaseSecurityConstants.MetadataFileName));
                Protector = new PassThroughKeyProtector();
                Database = new FakeMigrationDatabase();
                JournalStore = new DatabaseSecurityTransitionJournalStore(DatabaseSecurityTransitionManager.GetJournalFilePath(DatabasePath));
                Manager = new DatabaseSecurityTransitionManager(MetadataStore, Protector, Database, JournalStore);
            }

            public string DirectoryPath { get; }
            public string DatabasePath { get; }
            public DatabaseSecurityMetadataStore MetadataStore { get; }
            public PassThroughKeyProtector Protector { get; }
            public FakeMigrationDatabase Database { get; }
            public DatabaseSecurityTransitionJournalStore JournalStore { get; }
            public DatabaseSecurityTransitionManager Manager { get; }

            public void CreateWindowsSource(string databasePassword)
            {
                var key = Encoding.UTF8.GetBytes(databasePassword);

                try
                {
                    var protectedKey = Protector.Protect(key);

                    try
                    {
                        MetadataStore.Save(DatabaseSecurityMetadata.CreateWindowsCurrentUser(Convert.ToBase64String(protectedKey)));
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
                var salt = CustomPasswordDatabaseKeyDeriver.CreateSalt();

                try
                {
                    var metadata = DatabaseSecurityMetadata.CreateCustomPassword(salt, DatabaseSecurityConstants.DefaultPbkdf2Iterations);
                    MetadataStore.Save(metadata);
                    var databasePassword = CustomPasswordDatabaseKeyDeriver.DeriveDatabasePassword(userPassword, salt, metadata.Iterations);
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
                var key = DatabaseKeyGenerator.Generate();
                var protectedKey = Protector.Protect(key);

                return new TargetInfo
                {
                    Key = key,
                    ProtectedKey = protectedKey,
                    Password = DatabaseKeyGenerator.ToDatabasePassword(key),
                    Metadata = DatabaseSecurityMetadata.CreateWindowsCurrentUser(Convert.ToBase64String(protectedKey))
                };
            }

            public void CreateInterruptedJournal(string sourcePassword, string targetPassword, DatabaseSecurityMetadata targetMetadata)
            {
                var sourceBytes = Encoding.UTF8.GetBytes(sourcePassword);
                var targetBytes = Encoding.UTF8.GetBytes(targetPassword);

                try
                {
                    var protectedSource = Protector.Protect(sourceBytes);
                    var protectedTarget = Protector.Protect(targetBytes);

                    try
                    {
                        JournalStore.Save(new DatabaseSecurityTransitionJournal
                        {
                            TransitionVersion = DatabaseSecurityTransitionJournal.CurrentVersion,
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
                Assert.IsFalse(File.Exists(DatabaseSecurityTransitionManager.GetCandidateFilePath(DatabasePath)));
                Assert.IsFalse(File.Exists(DatabaseSecurityTransitionManager.GetBackupFilePath(DatabasePath)));
                Assert.IsFalse(File.Exists(DatabaseSecurityTransitionManager.GetRollbackScratchFilePath(DatabasePath)));
            }

            public void Dispose()
            {
                if (Directory.Exists(DirectoryPath)) Directory.Delete(DirectoryPath, true);
            }
        }

        private sealed class PassThroughKeyProtector : IDatabaseKeyProtector
        {
            public byte[] Protect(byte[] databaseKey) => (byte[])databaseKey.Clone();
            public byte[] Unprotect(byte[] protectedDatabaseKey) => (byte[])protectedDatabaseKey.Clone();
        }

        private sealed class FakeMigrationDatabase : IDatabaseSecurityMigrationDatabase
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
