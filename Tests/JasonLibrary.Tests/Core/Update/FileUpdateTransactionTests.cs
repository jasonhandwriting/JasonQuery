using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Updater.Core;

namespace JasonLibrary.Tests.Core.Update
{
    [TestClass]
    public sealed class FileUpdateTransactionTests
    {
        [TestMethod]
        [TestCategory("Update")]
        public void Execute_UpdatesFilesAndCreatesVerifiedBackup()
        {
            var paths = CreateTransactionPaths();

            try
            {
                var stages = new List<UpdateTransactionStage>();

                WriteFile(paths.PayloadRoot, "JasonQuery.exe", "new exe");
                WriteFile(paths.PayloadRoot, "localization/english.xml", "new language");
                WriteFile(paths.InstallationRoot, "JasonQuery.exe", "old exe");

                var result = new FileUpdateTransaction().Execute
                (
                    paths.PayloadRoot,
                    paths.InstallationRoot,
                    paths.BackupRoot,
                    "0.94.0",
                    "0.95.0",
                    stages.Add
                );

                Assert.AreEqual("new exe", ReadFile(paths.InstallationRoot, "JasonQuery.exe"));
                Assert.AreEqual("new language", ReadFile(paths.InstallationRoot, "localization/english.xml"));
                Assert.IsTrue(File.Exists(result.BackupManifestPath));
                Assert.AreEqual("old exe", ReadFile(Path.Combine(paths.BackupRoot, "files"), "JasonQuery.exe"));
                CollectionAssert.AreEqual
                (
                    new[]
                    {
                        UpdateTransactionStage.CreatingVerifiedBackup,
                        UpdateTransactionStage.ApplyingAndVerifyingUpdate
                    },
                    stages
                );
            }
            finally
            {
                Directory.Delete(paths.Root, true);
            }
        }

        [TestMethod]
        [TestCategory("Update")]
        public void Execute_WhenLaterTargetIsLocked_RestoresEarlierFiles()
        {
            var paths = CreateTransactionPaths();

            try
            {
                var stages = new List<UpdateTransactionStage>();

                WriteFile(paths.PayloadRoot, "A.dll", "new A");
                WriteFile(paths.PayloadRoot, "JasonQuery.exe", "new exe");
                WriteFile(paths.PayloadRoot, "ZLocked.dll", "new locked");
                WriteFile(paths.InstallationRoot, "A.dll", "old A");
                WriteFile(paths.InstallationRoot, "JasonQuery.exe", "old exe");
                WriteFile(paths.InstallationRoot, "ZLocked.dll", "old locked");

                using (var locked = new FileStream(Path.Combine(paths.InstallationRoot, "ZLocked.dll"), FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var exception = Assert.ThrowsException<UpdateTransactionException>
                    (
                        () => new FileUpdateTransaction().Execute
                        (
                            paths.PayloadRoot,
                            paths.InstallationRoot,
                            paths.BackupRoot,
                            "0.94.0",
                            "0.95.0",
                            stages.Add
                        )
                    );

                    Assert.IsTrue(exception.RecoverySucceeded, exception.RecoveryException?.Message);
                }

                Assert.AreEqual("old A", ReadFile(paths.InstallationRoot, "A.dll"));
                Assert.AreEqual("old exe", ReadFile(paths.InstallationRoot, "JasonQuery.exe"));
                Assert.AreEqual("old locked", ReadFile(paths.InstallationRoot, "ZLocked.dll"));
                CollectionAssert.AreEqual
                (
                    new[]
                    {
                        UpdateTransactionStage.CreatingVerifiedBackup,
                        UpdateTransactionStage.ApplyingAndVerifyingUpdate,
                        UpdateTransactionStage.RestoringAndVerifyingPreviousVersion
                    },
                    stages
                );
            }
            finally
            {
                Directory.Delete(paths.Root, true);
            }
        }

        [TestMethod]
        [TestCategory("Update")]
        public void RestoreBackup_RejectsTamperedBackupBeforeRestoring()
        {
            var paths = CreateTransactionPaths();

            try
            {
                WriteFile(paths.PayloadRoot, "JasonQuery.exe", "new exe");
                WriteFile(paths.InstallationRoot, "JasonQuery.exe", "old exe");

                var transaction = new FileUpdateTransaction();
                var result = transaction.Execute
                (
                    paths.PayloadRoot,
                    paths.InstallationRoot,
                    paths.BackupRoot,
                    "0.94.0",
                    "0.95.0"
                );

                WriteFile(Path.Combine(paths.BackupRoot, "files"), "JasonQuery.exe", "tampered backup");

                Assert.ThrowsException<InvalidDataException>(() => transaction.RestoreBackup(result.BackupManifestPath));
                Assert.AreEqual("new exe", ReadFile(paths.InstallationRoot, "JasonQuery.exe"));
            }
            finally
            {
                Directory.Delete(paths.Root, true);
            }
        }

        [TestMethod]
        [TestCategory("Update")]
        public void Execute_RejectsPackageThatContainsJasonQueryDatabase()
        {
            var paths = CreateTransactionPaths();

            try
            {
                WriteFile(paths.PayloadRoot, "JasonQuery.exe", "new exe");
                WriteFile(paths.PayloadRoot, "JasonQuery.db", "must not be updated");
                WriteFile(paths.InstallationRoot, "JasonQuery.exe", "old exe");

                var exception = Assert.ThrowsException<ProtectedUpdatePathException>
                (
                    () => new FileUpdateTransaction().Execute
                    (
                        paths.PayloadRoot,
                        paths.InstallationRoot,
                        paths.BackupRoot,
                        "0.94.0",
                        "0.95.0"
                    )
                );

                Assert.AreEqual("JasonQuery.db", exception.RelativePath);
                Assert.AreEqual("old exe", ReadFile(paths.InstallationRoot, "JasonQuery.exe"));
            }
            finally
            {
                Directory.Delete(paths.Root, true);
            }
        }

        [TestMethod]
        [TestCategory("Update")]
        public void Execute_RejectsPackageThatContainsApplicationBackupFile()
        {
            var paths = CreateTransactionPaths();

            try
            {
                WriteFile(paths.PayloadRoot, "JasonQuery.exe", "new exe");
                WriteFile(paths.PayloadRoot, "backup/query-backup.sql", "must not be updated");
                WriteFile(paths.InstallationRoot, "JasonQuery.exe", "old exe");

                var exception = Assert.ThrowsException<ProtectedUpdatePathException>
                (
                    () => new FileUpdateTransaction().Execute
                    (
                        paths.PayloadRoot,
                        paths.InstallationRoot,
                        paths.BackupRoot,
                        "0.94.0",
                        "0.95.0"
                    )
                );

                Assert.AreEqual(Path.Combine("backup", "query-backup.sql"), exception.RelativePath);
                Assert.AreEqual("old exe", ReadFile(paths.InstallationRoot, "JasonQuery.exe"));
            }
            finally
            {
                Directory.Delete(paths.Root, true);
            }
        }

        private static TransactionPaths CreateTransactionPaths()
        {
            var root = Path.Combine(Path.GetTempPath(), "JasonQuery-UpdaterTests", Path.GetRandomFileName());

            var paths = new TransactionPaths
            {
                Root = root,
                PayloadRoot = Path.Combine(root, "payload"),
                InstallationRoot = Path.Combine(root, "installation"),
                BackupRoot = Path.Combine(root, "backup")
            };

            Directory.CreateDirectory(paths.PayloadRoot);
            Directory.CreateDirectory(paths.InstallationRoot);

            return paths;
        }

        private static void WriteFile(string root, string relativePath, string content)
        {
            var path = Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar));
            var directory = Path.GetDirectoryName(path);

            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(path, content, new UTF8Encoding(false));
        }

        private static string ReadFile(string root, string relativePath)
        {
            return File.ReadAllText(Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar)), Encoding.UTF8);
        }

        private sealed class TransactionPaths
        {
            public string Root { get; set; }

            public string PayloadRoot { get; set; }

            public string InstallationRoot { get; set; }

            public string BackupRoot { get; set; }
        }
    }
}