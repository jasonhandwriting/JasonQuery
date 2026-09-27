using JasonQuery.Core.Security.JasonQueryDb;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Text;

namespace JasonQuery.Tests.Core.Security.JasonQueryDb
{
    [TestClass]
    public class JasonQueryDbLegacySecurityMigratorTests
    {
        private const string LegacyDefaultPassword = "ytec1688";

        [TestMethod]
        public void MigrateToWindowsCurrentUser_LegacyDefault_RekeysDatabaseAndCreatesMetadata()
        {
            var directory = CreateTemporaryDirectory();

            try
            {
                var databasePath = CreateFakeDatabase(directory, LegacyDefaultPassword);
                var metadataStore = CreateMetadataStore(directory);
                var migrator = CreateMigrator(metadataStore, new PassThroughKeyProtector(), new FakeMigrationDatabase());
                var result = migrator.MigrateToWindowsCurrentUser(databasePath);

                Assert.AreEqual(JasonQueryDbSecurityMode.WindowsCurrentUser, result.Metadata.Mode);
                Assert.IsTrue(metadataStore.Exists);
                Assert.AreEqual(result.DatabasePassword, File.ReadAllText(databasePath, Encoding.UTF8));
                Assert.IsFalse(new FakeMigrationDatabase().CanOpen(databasePath, LegacyDefaultPassword));
                Assert.IsFalse(File.Exists(JasonQueryDbLegacySecurityMigrator.GetBackupFilePath(databasePath)));
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        [TestMethod]
        public void MigrateToWindowsCurrentUser_LegacyCustom_FailsClosedWithoutMutation()
        {
            var directory = CreateTemporaryDirectory();

            try
            {
                var legacyCustomPassword = JasonQueryDbLegacySecurity.CreateCustomDatabasePassword("Legacy-Custom-Test");
                var databasePath = CreateFakeDatabase(directory, legacyCustomPassword);
                var metadataStore = CreateMetadataStore(directory);
                var migrator = CreateMigrator(metadataStore, new PassThroughKeyProtector(), new FakeMigrationDatabase());
                var ex = Assert.ThrowsExactly<JasonQueryDbSecurityStartupException>(() => migrator.MigrateToWindowsCurrentUser(databasePath));

                Assert.AreEqual(JasonQueryDbSecurityStartupErrorKind.MissingSecurityInformationOrLegacyCustomPassword, ex.ErrorKind);
                Assert.AreEqual(legacyCustomPassword, File.ReadAllText(databasePath, Encoding.UTF8));
                Assert.IsFalse(metadataStore.Exists);
                Assert.IsFalse(File.Exists(JasonQueryDbLegacySecurityMigrator.GetBackupFilePath(databasePath)));
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        [TestMethod]
        public void MigrateToCustomPassword_LegacyCustom_RekeysDatabaseAndCreatesCustomMetadata()
        {
            var directory = CreateTemporaryDirectory();

            try
            {
                const string customPassword = "Legacy-Custom-保留-123!";

                var legacyDatabasePassword = JasonQueryDbLegacySecurity.CreateCustomDatabasePassword(customPassword);
                var databasePath = CreateFakeDatabase(directory, legacyDatabasePassword);
                var metadataStore = CreateMetadataStore(directory);
                var migrationDatabase = new FakeMigrationDatabase();
                var migrator = CreateMigrator(metadataStore, new PassThroughKeyProtector(), migrationDatabase);
                var result = migrator.MigrateToCustomPassword(databasePath, customPassword);

                Assert.AreEqual(JasonQueryDbSecurityMode.CustomPassword, result.Metadata.Mode);
                Assert.AreEqual(JasonQueryDbSecurityConstants.Pbkdf2HmacSha256, result.Metadata.Kdf);
                Assert.AreEqual(JasonQueryDbSecurityConstants.DefaultPbkdf2Iterations, result.Metadata.Iterations);
                Assert.IsFalse(string.IsNullOrWhiteSpace(result.Metadata.Salt));
                Assert.IsTrue(metadataStore.Exists);

                var salt = Convert.FromBase64String(result.Metadata.Salt);

                try
                {
                    var expectedDatabasePassword = JasonQueryDbCustomPasswordKeyDeriver.DeriveDatabasePassword
                    (
                        customPassword,
                        salt,
                        result.Metadata.Iterations
                    );

                    Assert.AreEqual(expectedDatabasePassword, result.DatabasePassword);
                    Assert.AreEqual(expectedDatabasePassword, File.ReadAllText(databasePath, Encoding.UTF8));
                }
                finally
                {
                    Array.Clear(salt, 0, salt.Length);
                }

                Assert.IsFalse(migrationDatabase.CanOpen(databasePath, legacyDatabasePassword));
                Assert.IsFalse(File.Exists(JasonQueryDbLegacySecurityMigrator.GetBackupFilePath(databasePath)));
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        [TestMethod]
        public void MigrateToCustomPassword_WrongPassword_FailsClosedWithoutMutation()
        {
            var directory = CreateTemporaryDirectory();

            try
            {
                const string customPassword = "Legacy-Custom-Test";

                var legacyDatabasePassword = JasonQueryDbLegacySecurity.CreateCustomDatabasePassword(customPassword);
                var databasePath = CreateFakeDatabase(directory, legacyDatabasePassword);
                var metadataStore = CreateMetadataStore(directory);
                var migrator = CreateMigrator(metadataStore, new PassThroughKeyProtector(), new FakeMigrationDatabase());

                var ex = Assert.ThrowsExactly<JasonQueryDbSecurityStartupException>
                (
                    () => migrator.MigrateToCustomPassword(databasePath, "Wrong-Password")
                );

                Assert.AreEqual(JasonQueryDbSecurityStartupErrorKind.MissingSecurityInformationOrLegacyCustomPassword, ex.ErrorKind);
                Assert.AreEqual(legacyDatabasePassword, File.ReadAllText(databasePath, Encoding.UTF8));
                Assert.IsFalse(metadataStore.Exists);
                Assert.IsFalse(File.Exists(JasonQueryDbLegacySecurityMigrator.GetBackupFilePath(databasePath)));
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        [TestMethod]
        public void MigrateToCustomPassword_MetadataSaveFailure_RestoresLegacyDatabase()
        {
            var directory = CreateTemporaryDirectory();

            try
            {
                const string customPassword = "Legacy-Custom-Test";

                var legacyDatabasePassword = JasonQueryDbLegacySecurity.CreateCustomDatabasePassword(customPassword);
                var databasePath = CreateFakeDatabase(directory, legacyDatabasePassword);
                var metadataPath = Path.Combine(directory, JasonQueryDbSecurityConstants.MetadataFileName);
                var metadataStore = new ThrowingMetadataStore(metadataPath);
                var migrator = CreateMigrator(metadataStore, new PassThroughKeyProtector(), new FakeMigrationDatabase());

                var ex = Assert.ThrowsExactly<InvalidDataException>
                (
                    () => migrator.MigrateToCustomPassword(databasePath, customPassword)
                );

                Assert.Contains("previous legacy JasonQuery.db was restored", ex.Message);
                Assert.AreEqual(legacyDatabasePassword, File.ReadAllText(databasePath, Encoding.UTF8));
                Assert.IsFalse(metadataStore.Exists);
                Assert.IsFalse(File.Exists(JasonQueryDbLegacySecurityMigrator.GetBackupFilePath(databasePath)));
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        [TestMethod]
        public void MigrateToCustomPassword_InterruptedMigrationBackup_RestoresAndCompletesMigration()
        {
            var directory = CreateTemporaryDirectory();

            try
            {
                const string customPassword = "Legacy-Custom-Recovery";

                var legacyDatabasePassword = JasonQueryDbLegacySecurity.CreateCustomDatabasePassword(customPassword);
                var databasePath = CreateFakeDatabase(directory, "Interrupted-V2-Candidate");
                var backupPath = JasonQueryDbLegacySecurityMigrator.GetBackupFilePath(databasePath);

                File.WriteAllText(backupPath, legacyDatabasePassword, Encoding.UTF8);

                var metadataStore = CreateMetadataStore(directory);
                var migrationDatabase = new FakeMigrationDatabase();
                var migrator = CreateMigrator(metadataStore, new PassThroughKeyProtector(), migrationDatabase);
                var result = migrator.MigrateToCustomPassword(databasePath, customPassword);

                Assert.AreEqual(JasonQueryDbSecurityMode.CustomPassword, result.Metadata.Mode);
                Assert.AreEqual(result.DatabasePassword, File.ReadAllText(databasePath, Encoding.UTF8));
                Assert.IsTrue(metadataStore.Exists);
                Assert.IsFalse(migrationDatabase.CanOpen(databasePath, legacyDatabasePassword));
                Assert.IsFalse(File.Exists(backupPath));
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        [TestMethod]
        public void IsCustomPasswordValid_InterruptedBackupExists_ValidatesBackupCredential()
        {
            var directory = CreateTemporaryDirectory();

            try
            {
                const string customPassword = "Legacy-Custom-Recovery";

                var legacyDatabasePassword = JasonQueryDbLegacySecurity.CreateCustomDatabasePassword(customPassword);
                var databasePath = CreateFakeDatabase(directory, "Interrupted-V2-Candidate");
                var backupPath = JasonQueryDbLegacySecurityMigrator.GetBackupFilePath(databasePath);

                File.WriteAllText(backupPath, legacyDatabasePassword, Encoding.UTF8);

                var metadataStore = CreateMetadataStore(directory);
                var migrator = CreateMigrator(metadataStore, new PassThroughKeyProtector(), new FakeMigrationDatabase());

                Assert.IsTrue(migrator.IsCustomPasswordValid(databasePath, customPassword));
                Assert.IsFalse(migrator.IsCustomPasswordValid(databasePath, "Wrong-Password"));
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        [TestMethod]
        public void CanRecoverInterruptedMigrationWithDefaultPassword_DefaultBackup_ReturnsTrue()
        {
            var directory = CreateTemporaryDirectory();

            try
            {
                var databasePath = CreateFakeDatabase(directory, "Interrupted-V2-Candidate");
                var backupPath = JasonQueryDbLegacySecurityMigrator.GetBackupFilePath(databasePath);

                File.WriteAllText(backupPath, LegacyDefaultPassword, Encoding.UTF8);

                var metadataStore = CreateMetadataStore(directory);
                var migrator = CreateMigrator(metadataStore, new PassThroughKeyProtector(), new FakeMigrationDatabase());

                Assert.IsTrue(migrator.HasInterruptedMigrationBackup(databasePath));
                Assert.IsTrue(migrator.CanRecoverInterruptedMigrationWithDefaultPassword(databasePath));
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        [TestMethod]
        public void MigrateToWindowsCurrentUser_RekeyFailure_LeavesOriginalDatabaseUntouched()
        {
            var directory = CreateTemporaryDirectory();

            try
            {
                var databasePath = CreateFakeDatabase(directory, LegacyDefaultPassword);
                var metadataStore = CreateMetadataStore(directory);
                var migrationDatabase = new FakeMigrationDatabase { ThrowOnChangePassword = true };
                var migrator = CreateMigrator(metadataStore, new PassThroughKeyProtector(), migrationDatabase);

                Assert.ThrowsExactly<InvalidOperationException>(() => migrator.MigrateToWindowsCurrentUser(databasePath));

                Assert.AreEqual(LegacyDefaultPassword, File.ReadAllText(databasePath, Encoding.UTF8));
                Assert.IsFalse(metadataStore.Exists);
                Assert.IsFalse(File.Exists(JasonQueryDbLegacySecurityMigrator.GetBackupFilePath(databasePath)));
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        [TestMethod]
        public void MigrateToWindowsCurrentUser_MetadataSaveFailure_RestoresLegacyDatabase()
        {
            var directory = CreateTemporaryDirectory();

            try
            {
                var databasePath = CreateFakeDatabase(directory, LegacyDefaultPassword);
                var metadataPath = Path.Combine(directory, JasonQueryDbSecurityConstants.MetadataFileName);
                var metadataStore = new ThrowingMetadataStore(metadataPath);
                var migrator = CreateMigrator(metadataStore, new PassThroughKeyProtector(), new FakeMigrationDatabase());
                var ex = Assert.ThrowsExactly<InvalidDataException>(() => migrator.MigrateToWindowsCurrentUser(databasePath));

                Assert.Contains("previous legacy JasonQuery.db was restored", ex.Message);
                Assert.AreEqual(LegacyDefaultPassword, File.ReadAllText(databasePath, Encoding.UTF8));
                Assert.IsFalse(metadataStore.Exists);
                Assert.IsFalse(File.Exists(JasonQueryDbLegacySecurityMigrator.GetBackupFilePath(databasePath)));
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        [TestMethod]
        public void RecoverInterruptedMigrationIfNeeded_BackupExists_RestoresLegacyDatabase()
        {
            var directory = CreateTemporaryDirectory();

            try
            {
                var databasePath = CreateFakeDatabase(directory, "V2-Temporary-Password");
                var backupPath = JasonQueryDbLegacySecurityMigrator.GetBackupFilePath(databasePath);

                File.WriteAllText(backupPath, LegacyDefaultPassword, Encoding.UTF8);

                var metadataStore = CreateMetadataStore(directory);
                var migrator = CreateMigrator(metadataStore, new PassThroughKeyProtector(), new FakeMigrationDatabase());
                var recovered = migrator.RecoverInterruptedMigrationIfNeeded(databasePath);

                Assert.IsTrue(recovered);
                Assert.AreEqual(LegacyDefaultPassword, File.ReadAllText(databasePath, Encoding.UTF8));
                Assert.IsFalse(File.Exists(backupPath));
                Assert.IsFalse(metadataStore.Exists);
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        [TestMethod]
        public void RecoverInterruptedMigrationIfNeeded_NoBackup_ReturnsFalse()
        {
            var directory = CreateTemporaryDirectory();

            try
            {
                var databasePath = CreateFakeDatabase(directory, LegacyDefaultPassword);
                var metadataStore = CreateMetadataStore(directory);
                var migrator = CreateMigrator(metadataStore, new PassThroughKeyProtector(), new FakeMigrationDatabase());

                Assert.IsFalse(migrator.RecoverInterruptedMigrationIfNeeded(databasePath));
                Assert.AreEqual(LegacyDefaultPassword, File.ReadAllText(databasePath, Encoding.UTF8));
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        [TestMethod]
        public void CleanupCompletedMigrationBackup_MetadataExists_DeletesLegacyBackup()
        {
            var directory = CreateTemporaryDirectory();

            try
            {
                var databasePath = CreateFakeDatabase(directory, "V2-Database-Password");
                var backupPath = JasonQueryDbLegacySecurityMigrator.GetBackupFilePath(databasePath);

                File.WriteAllText(backupPath, LegacyDefaultPassword, Encoding.UTF8);

                var metadataStore = CreateMetadataStore(directory);
                var databaseKey = JasonQueryDbKeyGenerator.Generate();

                try
                {
                    metadataStore.Save
                    (
                        JasonQueryDbSecurityMetadata.CreateWindowsCurrentUser(Convert.ToBase64String(databaseKey))
                    );
                }
                finally
                {
                    Array.Clear(databaseKey, 0, databaseKey.Length);
                }

                var migrator = CreateMigrator(metadataStore, new PassThroughKeyProtector(), new FakeMigrationDatabase());

                migrator.CleanupCompletedMigrationBackup(databasePath);
                Assert.IsFalse(File.Exists(backupPath));
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        [TestMethod]
        public void RecoverInterruptedMigrationIfNeeded_InvalidBackup_DoesNotOverwriteCurrentDatabase()
        {
            var directory = CreateTemporaryDirectory();

            try
            {
                var currentPassword = "Current-Database-Password";
                var databasePath = CreateFakeDatabase(directory, currentPassword);
                var backupPath = JasonQueryDbLegacySecurityMigrator.GetBackupFilePath(databasePath);

                File.WriteAllText(backupPath, "Invalid-Backup-Password", Encoding.UTF8);

                var metadataStore = CreateMetadataStore(directory);
                var migrator = CreateMigrator(metadataStore, new PassThroughKeyProtector(), new FakeMigrationDatabase());

                Assert.ThrowsExactly<InvalidDataException>(() => migrator.RecoverInterruptedMigrationIfNeeded(databasePath));

                Assert.AreEqual(currentPassword, File.ReadAllText(databasePath, Encoding.UTF8));
                Assert.IsTrue(File.Exists(backupPath));
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        private static JasonQueryDbLegacySecurityMigrator CreateMigrator(IJasonQueryDbSecurityMetadataStore metadataStore, IJasonQueryDbKeyProtector keyProtector, IJasonQueryDbMigrationDatabase migrationDatabase)
        {
            return new JasonQueryDbLegacySecurityMigrator(metadataStore, keyProtector, migrationDatabase);
        }

        private static JasonQueryDbSecurityMetadataStore CreateMetadataStore(string directory)
        {
            return new JasonQueryDbSecurityMetadataStore
            (
                Path.Combine(directory, JasonQueryDbSecurityConstants.MetadataFileName)
            );
        }

        private static string CreateFakeDatabase(string directory, string password)
        {
            var databasePath = Path.Combine(directory, "JasonQuery.db");

            File.WriteAllText(databasePath, password, Encoding.UTF8);
            return databasePath;
        }

        private static string CreateTemporaryDirectory()
        {
            var directory = Path.Combine(Path.GetTempPath(), "JasonQuery.Tests", Guid.NewGuid().ToString("N"));

            Directory.CreateDirectory(directory);
            return directory;
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

        private sealed class FakeMigrationDatabase : IJasonQueryDbMigrationDatabase
        {
            public bool ThrowOnChangePassword { get; set; }

            public bool CanOpen(string databaseFilePath, string databasePassword)
            {
                if (!File.Exists(databaseFilePath))
                {
                    return false;
                }

                return string.Equals
                (
                    File.ReadAllText(databaseFilePath, Encoding.UTF8),
                    databasePassword,
                    StringComparison.Ordinal
                );
            }

            public void CreateVerifiedCopy(string sourceDatabaseFilePath, string destinationDatabaseFilePath, string databasePassword)
            {
                if (!CanOpen(sourceDatabaseFilePath, databasePassword))
                {
                    throw new InvalidDataException("The fake source database could not be opened.");
                }

                File.Copy(sourceDatabaseFilePath, destinationDatabaseFilePath, false);

                if (!CanOpen(destinationDatabaseFilePath, databasePassword))
                {
                    throw new InvalidDataException("The fake database copy could not be validated.");
                }
            }

            public void ChangePassword(string databaseFilePath, string currentDatabasePassword, string newDatabasePassword)
            {
                if (ThrowOnChangePassword)
                {
                    throw new InvalidOperationException("Simulated rekey failure.");
                }

                if (!CanOpen(databaseFilePath, currentDatabasePassword))
                {
                    throw new InvalidDataException("The fake database could not be opened with the current password.");
                }

                File.WriteAllText(databaseFilePath, newDatabasePassword, Encoding.UTF8);
            }
        }

        private sealed class ThrowingMetadataStore : IJasonQueryDbSecurityMetadataStore
        {
            public ThrowingMetadataStore(string metadataFilePath)
            {
                MetadataFilePath = metadataFilePath;
            }

            public string MetadataFilePath { get; }

            public bool Exists => File.Exists(MetadataFilePath);

            public JasonQueryDbSecurityMetadata Load()
            {
                throw new FileNotFoundException();
            }

            public void Save(JasonQueryDbSecurityMetadata metadata)
            {
                throw new IOException("Simulated metadata save failure.");
            }

            public void Delete()
            {
                if (File.Exists(MetadataFilePath))
                {
                    File.Delete(MetadataFilePath);
                }
            }
        }
    }
}
