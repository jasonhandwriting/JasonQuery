using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using Updater.Core;

namespace JasonLibrary.Tests.Core.Update
{
    [TestClass]
    public sealed class UpdateBackupRetentionPolicyTests
    {
        [TestMethod]
        [TestCategory("Update")]
        public void Prune_KeepsCurrentAndTwoMostRecentVerifiedBackups()
        {
            var root = CreateTestRoot();

            try
            {
                var oldest = CreateVerifiedBackup(root, "20260820-000000-000_0.90_to_0.91", 5);
                var secondOldest = CreateVerifiedBackup(root, "20260821-000000-000_0.91_to_0.92", 4);
                var retainedPrevious2 = CreateVerifiedBackup(root, "20260822-000000-000_0.92_to_0.93", 3);
                var retainedPrevious1 = CreateVerifiedBackup(root, "20260823-000000-000_0.93_to_0.94", 2);
                var current = CreateVerifiedBackup(root, "20260824-000000-000_0.94_to_0.95", 1);
                var result = UpdateBackupRetentionPolicy.Prune(root, current);

                Assert.IsFalse(Directory.Exists(oldest));
                Assert.IsFalse(Directory.Exists(secondOldest));
                Assert.IsTrue(Directory.Exists(retainedPrevious2));
                Assert.IsTrue(Directory.Exists(retainedPrevious1));
                Assert.IsTrue(Directory.Exists(current));
                Assert.AreEqual(2, result.DeletedDirectories.Count);
                Assert.AreEqual(0, result.FailedDirectories.Count);
            }
            finally
            {
                Directory.Delete(Path.GetDirectoryName(root), true);
            }
        }

        [TestMethod]
        [TestCategory("Update")]
        public void Prune_DoesNotDeleteUnknownOrIncompleteDirectories()
        {
            var root = CreateTestRoot();

            try
            {
                var incomplete = Path.Combine(root, "20260801-000000-000_incomplete");
                var unrelated = Path.Combine(root, "MaintainerNotes");

                Directory.CreateDirectory(incomplete);
                Directory.CreateDirectory(unrelated);
                File.WriteAllText(Path.Combine(incomplete, "partial-backup.bin"), "partial");
                File.WriteAllText(Path.Combine(unrelated, "readme.txt"), "unrelated");

                CreateVerifiedBackup(root, "20260821-000000-000_0.91_to_0.92", 4);
                CreateVerifiedBackup(root, "20260822-000000-000_0.92_to_0.93", 3);
                CreateVerifiedBackup(root, "20260823-000000-000_0.93_to_0.94", 2);

                var current = CreateVerifiedBackup(root, "20260824-000000-000_0.94_to_0.95", 1);

                UpdateBackupRetentionPolicy.Prune(root, current);

                Assert.IsTrue(Directory.Exists(incomplete));
                Assert.IsTrue(Directory.Exists(unrelated));
                Assert.IsTrue(File.Exists(Path.Combine(incomplete, "partial-backup.bin")));
                Assert.IsTrue(File.Exists(Path.Combine(unrelated, "readme.txt")));
            }
            finally
            {
                Directory.Delete(Path.GetDirectoryName(root), true);
            }
        }

        [TestMethod]
        [TestCategory("Update")]
        public void Prune_RejectsCurrentBackupOutsideTheBackupParent()
        {
            var testRoot = Path.Combine(Path.GetTempPath(), "JasonQuery-UpdaterTests", Path.GetRandomFileName());
            var expectedParent = Path.Combine(testRoot, "Expected", "UpdateBackups");
            var otherParent = Path.Combine(testRoot, "Other", "UpdateBackups");

            Directory.CreateDirectory(expectedParent);
            Directory.CreateDirectory(otherParent);

            try
            {
                var retained = CreateVerifiedBackup(expectedParent, "20260823-000000-000_0.93_to_0.94", 2);
                var current = CreateVerifiedBackup(otherParent, "20260824-000000-000_0.94_to_0.95", 1);

                Assert.ThrowsException<InvalidOperationException>
                (
                    () => UpdateBackupRetentionPolicy.Prune(expectedParent, current)
                );

                Assert.IsTrue(Directory.Exists(retained));
                Assert.IsTrue(Directory.Exists(current));
            }
            finally
            {
                Directory.Delete(testRoot, true);
            }
        }

        private static string CreateTestRoot()
        {
            var root = Path.Combine
            (
                Path.GetTempPath(),
                "JasonQuery-UpdaterTests",
                Path.GetRandomFileName(),
                "UpdateBackups"
            );

            Directory.CreateDirectory(root);

            return root;
        }

        private static string CreateVerifiedBackup(string backupParent, string folderName, int ageInDays)
        {
            var backupRoot = Path.Combine(backupParent, folderName);

            Directory.CreateDirectory(backupRoot);

            var manifestPath = Path.Combine(backupRoot, FileUpdateTransaction.BackupManifestFileName);
            var installationRoot = EscapeJson(Path.Combine(Path.GetTempPath(), "JasonQuery-Installed"));
            var createdAt = DateTimeOffset.UtcNow.AddDays(-ageInDays).ToString("O");

            var manifestJson = "{"
                               + "\"schema_version\":1,"
                               + $"\"created_at\":\"{createdAt}\","
                               + $"\"installation_root\":\"{installationRoot}\","
                               + "\"installed_version\":\"0.94.0\","
                               + "\"target_version\":\"0.95.0\","
                               + "\"files\":[]"
                               + "}";

            File.WriteAllText(manifestPath, manifestJson);
            Directory.SetCreationTimeUtc(backupRoot, DateTime.UtcNow.AddDays(-ageInDays));

            return backupRoot;
        }

        private static string EscapeJson(string value)
        {
            return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }
    }
}
