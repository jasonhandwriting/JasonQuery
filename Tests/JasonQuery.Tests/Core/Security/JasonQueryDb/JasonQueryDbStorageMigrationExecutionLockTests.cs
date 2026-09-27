using JasonQuery.Core.Security.JasonQueryDb;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace JasonQuery.Tests.Core.Security.JasonQueryDb
{
    [TestClass]
    public class JasonQueryDbStorageMigrationExecutionLockTests
    {
        [TestMethod]
        public void Acquire_EmptyDatabasePath_Throws()
        {
            Assert.ThrowsExactly<ArgumentException>
            (
                () => JasonQueryDbStorageMigrationExecutionLock.Acquire(" ")
            );
        }

        [TestMethod]
        public void Acquire_CreatesExclusiveSameDirectoryLockFile()
        {
            using (var scope = new TestScope())
            using (var migrationLock = JasonQueryDbStorageMigrationExecutionLock.Acquire(scope.DatabasePath))
            {
                Assert.AreEqual(scope.LockPath, migrationLock.LockFilePath);
                Assert.IsTrue(File.Exists(scope.LockPath));

                Assert.ThrowsExactly<IOException>
                (
                    () =>
                    {
                        using (new FileStream(scope.LockPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                        {
                        }
                    }
                );
            }
        }

        [TestMethod]
        public void Dispose_DeletesLockFile()
        {
            using (var scope = new TestScope())
            {
                using (JasonQueryDbStorageMigrationExecutionLock.Acquire(scope.DatabasePath))
                {
                    Assert.IsTrue(File.Exists(scope.LockPath));
                }

                Assert.IsFalse(File.Exists(scope.LockPath));
            }
        }

        [TestMethod]
        public void Acquire_SecondLockForSameDatabase_FailsClosed()
        {
            using (var scope = new TestScope())
            using (JasonQueryDbStorageMigrationExecutionLock.Acquire(scope.DatabasePath))
            {
                Assert.ThrowsExactly<IOException>
                (
                    () => JasonQueryDbStorageMigrationExecutionLock.Acquire(scope.DatabasePath)
                );
            }
        }

        private sealed class TestScope : IDisposable
        {
            public TestScope()
            {
                DirectoryPath = Path.Combine
                (
                    Path.GetTempPath(),
                    "JasonQuery-Step389F-R4C2-Lock-" + Guid.NewGuid().ToString("N")
                );

                Directory.CreateDirectory(DirectoryPath);
                DatabasePath = Path.Combine(DirectoryPath, "JasonQuery.db");
                LockPath = JasonQueryDbStorageMigrationArtifactPaths.GetLockFilePath(DatabasePath);

                File.WriteAllBytes(DatabasePath, new byte[] { 1, 2, 3 });
            }

            public string DirectoryPath { get; }

            public string DatabasePath { get; }

            public string LockPath { get; }

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
