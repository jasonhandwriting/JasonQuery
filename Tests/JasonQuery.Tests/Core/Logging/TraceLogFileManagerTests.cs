using JasonQuery.Core.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace JasonQuery.Tests.Core.Logging
{
    [TestClass]
    public class TraceLogFileManagerTests
    {
        private string _tempDirectory;

        [TestInitialize]
        public void TestInitialize()
        {
            _tempDirectory = Path.Combine(Path.GetTempPath(), "JasonQuery-TraceLogFileManagerTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDirectory);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            if (Directory.Exists(_tempDirectory))
            {
                Directory.Delete(_tempDirectory, true);
            }
        }

        [TestMethod]
        public void CreateSessionLogFilePath_UsesTimestampProcessIdAndCsvExtension()
        {
            var now = new DateTimeOffset(2026, 8, 30, 10, 11, 12, 345, TimeSpan.FromHours(8));
            var result = TraceLogFileManager.CreateSessionLogFilePath(_tempDirectory, now, 12345);

            Assert.AreEqual(Path.Combine(_tempDirectory, "JasonQuery_20260830_101112_345_P12345.csv"), result);
        }

        [TestMethod]
        public void CreateSessionLogFilePath_WhenNameExists_AddsNumericSuffix()
        {
            var now = new DateTimeOffset(2026, 8, 30, 10, 11, 12, 345, TimeSpan.FromHours(8));
            var firstPath = TraceLogFileManager.CreateSessionLogFilePath(_tempDirectory, now, 12345);

            File.WriteAllText(firstPath, string.Empty);

            var result = TraceLogFileManager.CreateSessionLogFilePath(_tempDirectory, now, 12345);

            Assert.AreEqual(Path.Combine(_tempDirectory, "JasonQuery_20260830_101112_345_P12345_02.csv"), result);
        }

        [TestMethod]
        public void CleanupOldLogFiles_DeletesOnlyExpiredMatchingTopLevelFiles()
        {
            var nowUtc = new DateTimeOffset(2026, 8, 30, 2, 0, 0, TimeSpan.Zero);
            var expiredCsv = CreateFile("JasonQuery_20260820_010000_000_P1.csv", nowUtc.AddDays(-8));
            var expiredLegacyLog = CreateFile("JasonQuery.log", nowUtc.AddDays(-8));
            var recentCsv = CreateFile("JasonQuery_20260829_010000_000_P1.csv", nowUtc.AddDays(-1));
            var unrelatedFile = CreateFile("notes.log", nowUtc.AddDays(-30));
            var similarlyNamedFile = CreateFile("JasonQueryNotes.log", nowUtc.AddDays(-30));
            var subdirectory = Path.Combine(_tempDirectory, "archive");

            Directory.CreateDirectory(subdirectory);

            var nestedCsv = Path.Combine(subdirectory, "JasonQuery_20260820_010000_000_P1.csv");

            File.WriteAllText(nestedCsv, string.Empty);
            File.SetLastWriteTimeUtc(nestedCsv, nowUtc.AddDays(-8).UtcDateTime);

            var deletedCount = TraceLogFileManager.CleanupOldLogFiles(_tempDirectory, nowUtc, 7);

            Assert.AreEqual(2, deletedCount);
            Assert.IsFalse(File.Exists(expiredCsv));
            Assert.IsFalse(File.Exists(expiredLegacyLog));
            Assert.IsTrue(File.Exists(recentCsv));
            Assert.IsTrue(File.Exists(unrelatedFile));
            Assert.IsTrue(File.Exists(similarlyNamedFile));
            Assert.IsTrue(File.Exists(nestedCsv));
        }

        [TestMethod]
        public void CleanupOldLogFiles_DoesNotDeleteActiveFile()
        {
            var nowUtc = new DateTimeOffset(2026, 8, 30, 2, 0, 0, TimeSpan.Zero);
            var activeFile = CreateFile("JasonQuery_20260820_010000_000_P1.csv", nowUtc.AddDays(-8));
            var deletedCount = TraceLogFileManager.CleanupOldLogFiles(_tempDirectory, nowUtc, 7, activeFile);

            Assert.AreEqual(0, deletedCount);
            Assert.IsTrue(File.Exists(activeFile));
        }

        private string CreateFile(string fileName, DateTimeOffset lastWriteTimeUtc)
        {
            var filePath = Path.Combine(_tempDirectory, fileName);

            File.WriteAllText(filePath, string.Empty);
            File.SetLastWriteTimeUtc(filePath, lastWriteTimeUtc.UtcDateTime);

            return filePath;
        }
    }
}
