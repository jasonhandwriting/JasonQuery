using JasonQuery.Core.Security.JasonQueryDb;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace JasonQuery.Tests.Core.Security.JasonQueryDb
{
    [TestClass]
    public class JasonQueryDbStorageMigrationArtifactPathsTests
    {
        [TestMethod]
        public void Create_ReturnsFrozenArtifactNames()
        {
            var directory = CreateTemporaryDirectory();

            try
            {
                var databasePath = Path.Combine(directory, "JasonQuery.db");
                var metadataPath = Path.Combine(directory, "JasonQuery.security.json");
                const string operationId = "0123456789abcdef0123456789abcdef";

                var paths = JasonQueryDbStorageMigrationArtifactPaths.Create
                (
                    databasePath,
                    metadataPath,
                    operationId
                );

                var fullDatabasePath = Path.GetFullPath(databasePath);
                var fullMetadataPath = Path.GetFullPath(metadataPath);

                Assert.AreEqual(fullDatabasePath, paths.DatabaseFilePath);
                Assert.AreEqual(fullMetadataPath, paths.MetadataFilePath);
                Assert.AreEqual(operationId, paths.OperationId);
                Assert.AreEqual(fullDatabasePath + ".migration.json", paths.JournalFilePath);
                Assert.AreEqual(fullDatabasePath + ".migration." + operationId + ".journal.tmp", paths.JournalTemporaryFilePath);
                Assert.AreEqual(fullDatabasePath + ".migration." + operationId + ".candidate", paths.CandidateDatabaseFilePath);
                Assert.AreEqual(fullDatabasePath + ".migration." + operationId + ".backup", paths.DatabaseBackupFilePath);
                Assert.AreEqual(fullMetadataPath + ".migration." + operationId + ".tmp", paths.MetadataTemporaryFilePath);
                Assert.AreEqual(fullMetadataPath + ".migration." + operationId + ".backup", paths.MetadataBackupFilePath);
                Assert.AreEqual(fullDatabasePath + ".migration.lock", paths.LockFilePath);
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        [TestMethod]
        public void Create_RequiresCanonicalLowercaseGuidNOperationId()
        {
            var directory = CreateTemporaryDirectory();

            try
            {
                var databasePath = Path.Combine(directory, "JasonQuery.db");
                var metadataPath = Path.Combine(directory, "JasonQuery.security.json");
                var upper = Guid.NewGuid().ToString("N").ToUpperInvariant();

                Assert.ThrowsExactly<ArgumentException>
                (
                    () => JasonQueryDbStorageMigrationArtifactPaths.Create(databasePath, metadataPath, upper)
                );

                Assert.ThrowsExactly<ArgumentException>
                (
                    () => JasonQueryDbStorageMigrationArtifactPaths.Create
                    (
                        databasePath,
                        metadataPath,
                        Guid.NewGuid().ToString("D")
                    )
                );
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        [TestMethod]
        public void Create_RequiresDatabaseAndMetadataInSameDirectory()
        {
            var directory = CreateTemporaryDirectory();

            try
            {
                var metadataDirectory = Path.Combine(directory, "metadata");

                Directory.CreateDirectory(metadataDirectory);

                var databasePath = Path.Combine(directory, "JasonQuery.db");
                var metadataPath = Path.Combine(metadataDirectory, "JasonQuery.security.json");

                Assert.ThrowsExactly<InvalidOperationException>
                (
                    () => JasonQueryDbStorageMigrationArtifactPaths.Create
                    (
                        databasePath,
                        metadataPath,
                        Guid.NewGuid().ToString("N")
                    )
                );
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        [TestMethod]
        public void Create_RejectsDatabaseAndMetadataUsingSamePath()
        {
            var directory = CreateTemporaryDirectory();

            try
            {
                var path = Path.Combine(directory, "JasonQuery.db");

                Assert.ThrowsExactly<ArgumentException>
                (
                    () => JasonQueryDbStorageMigrationArtifactPaths.Create
                    (
                        path,
                        path,
                        Guid.NewGuid().ToString("N")
                    )
                );
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        [TestMethod]
        public void CanonicalPaths_NormalizeDatabasePath()
        {
            var directory = CreateTemporaryDirectory();

            try
            {
                var nested = Path.Combine(directory, "nested");

                Directory.CreateDirectory(nested);

                var databasePath = Path.Combine(nested, "..", "JasonQuery.db");
                var fullDatabasePath = Path.GetFullPath(databasePath);
                var operationId = Guid.NewGuid().ToString("N");

                Assert.AreEqual
                (
                    fullDatabasePath + ".migration.json",
                    JasonQueryDbStorageMigrationArtifactPaths.GetJournalFilePath(databasePath)
                );

                Assert.AreEqual
                (
                    fullDatabasePath + ".migration." + operationId + ".journal.tmp",
                    JasonQueryDbStorageMigrationArtifactPaths.GetJournalTemporaryFilePath(databasePath, operationId)
                );

                Assert.AreEqual
                (
                    fullDatabasePath + ".migration.lock",
                    JasonQueryDbStorageMigrationArtifactPaths.GetLockFilePath(databasePath)
                );
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        [TestMethod]
        public void Create_DoesNotCreateMigrationArtifacts()
        {
            var directory = CreateTemporaryDirectory();

            try
            {
                var databasePath = Path.Combine(directory, "JasonQuery.db");
                var metadataPath = Path.Combine(directory, "JasonQuery.security.json");

                var paths = JasonQueryDbStorageMigrationArtifactPaths.Create
                (
                    databasePath,
                    metadataPath,
                    Guid.NewGuid().ToString("N")
                );

                Assert.IsFalse(File.Exists(paths.JournalFilePath));
                Assert.IsFalse(File.Exists(paths.JournalTemporaryFilePath));
                Assert.IsFalse(File.Exists(paths.CandidateDatabaseFilePath));
                Assert.IsFalse(File.Exists(paths.DatabaseBackupFilePath));
                Assert.IsFalse(File.Exists(paths.MetadataTemporaryFilePath));
                Assert.IsFalse(File.Exists(paths.MetadataBackupFilePath));
                Assert.IsFalse(File.Exists(paths.LockFilePath));
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        private static string CreateTemporaryDirectory()
        {
            var directory = Path.Combine
            (
                Path.GetTempPath(),
                "JasonQuery-Step389F-R2-Paths-" + Guid.NewGuid().ToString("N")
            );

            Directory.CreateDirectory(directory);
            return directory;
        }
    }
}
