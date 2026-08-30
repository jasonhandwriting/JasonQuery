using JasonQuery.Core.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace JasonQuery.Tests.Core.Logging
{
    [TestClass]
    [DoNotParallelize]
    public class TraceLoggerTests
    {
        private string _tempDirectory;

        [TestInitialize]
        public void TestInitialize()
        {
            TraceLogger.Shutdown();
            TraceLogger.SetContextProvider(null);

            _tempDirectory = Path.Combine(Path.GetTempPath(), "JasonQuery-TraceLoggerTests", Guid.NewGuid().ToString("N"));

            Directory.CreateDirectory(_tempDirectory);
            TraceLogger.Initialize(_tempDirectory);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            TraceLogger.Shutdown();
            TraceLogger.SetContextProvider(null);

            if (Directory.Exists(_tempDirectory))
            {
                Directory.Delete(_tempDirectory, true);
            }
        }

        [TestMethod]
        public void SetEnabled_CreatesPerSessionCsvWithUtf8BomAndHeader()
        {
            Assert.IsTrue(TraceLogger.SetEnabled(true));

            var filePath = TraceLogger.CurrentLogFilePath;

            TraceLogger.Shutdown();

            var bytes = File.ReadAllBytes(filePath);
            var firstLine = File.ReadLines(filePath).First();

            Assert.IsTrue(Regex.IsMatch(Path.GetFileName(filePath), @"^JasonQuery_\d{8}_\d{6}_\d{3}_P\d+\.csv$"));
            Assert.IsTrue(bytes.Length >= 3);
            Assert.AreEqual((byte)0xEF, bytes[0]);
            Assert.AreEqual((byte)0xBB, bytes[1]);
            Assert.AreEqual((byte)0xBF, bytes[2]);
            Assert.AreEqual(TraceCsvFormatter.Header, firstLine);
        }

        [TestMethod]
        public void Time_WhenCreatedWhileDisabled_DoesNotLogAfterLoggerIsEnabled()
        {
            var scope = TraceLogger.Time("CreatedBeforeEnable");

            Assert.IsTrue(TraceLogger.SetEnabled(true));

            var filePath = TraceLogger.CurrentLogFilePath;

            scope.Dispose();
            TraceLogger.Shutdown();

            var content = File.ReadAllText(filePath);

            Assert.IsFalse(content.Contains("CreatedBeforeEnable"));
        }

        [TestMethod]
        public void Time_WhenLoggerStartsANewSession_DoesNotWriteEndWithoutMatchingStart()
        {
            Assert.IsTrue(TraceLogger.SetEnabled(true));

            var oldScope = TraceLogger.Time("OldSessionOperation");

            TraceLogger.Shutdown();
            Assert.IsTrue(TraceLogger.SetEnabled(true));

            var newFilePath = TraceLogger.CurrentLogFilePath;

            oldScope.Dispose();
            TraceLogger.Shutdown();

            var content = File.ReadAllText(newFilePath);

            Assert.IsFalse(content.Contains("OldSessionOperation"));
        }

        [TestMethod]
        public void Time_NestedScopes_WriteParentOperationIdAndDepth()
        {
            Assert.IsTrue(TraceLogger.SetEnabled(true));

            var filePath = TraceLogger.CurrentLogFilePath;

            using (TraceLogger.Time("Outer"))
            {
                using (TraceLogger.Time("Inner"))
                {
                }
            }

            TraceLogger.Shutdown();

            var lines = File.ReadAllLines(filePath);
            var outerStart = lines.Single(line => line.Contains("\"Start\"") && line.Contains("\"Outer\""));
            var innerStart = lines.Single(line => line.Contains("\"Start\"") && line.Contains("\"Inner\""));
            var outerMatch = Regex.Match(outerStart, ",\"(?<operation>OP\\d{6})\",\"(?<parent>[^\"]*)\",(?<depth>\\d+),");
            var innerMatch = Regex.Match(innerStart, ",\"(?<operation>OP\\d{6})\",\"(?<parent>[^\"]*)\",(?<depth>\\d+),");

            Assert.IsTrue(outerMatch.Success);
            Assert.IsTrue(innerMatch.Success);
            Assert.AreEqual(string.Empty, outerMatch.Groups["parent"].Value);
            Assert.AreEqual("0", outerMatch.Groups["depth"].Value);
            Assert.AreEqual(outerMatch.Groups["operation"].Value, innerMatch.Groups["parent"].Value);
            Assert.AreEqual("1", innerMatch.Groups["depth"].Value);
        }

        [TestMethod]
        public void LogStage_WhenCalledConcurrently_WritesEveryEntry()
        {
            Assert.IsTrue(TraceLogger.SetEnabled(true));

            var filePath = TraceLogger.CurrentLogFilePath;

            Parallel.For(0, 40, index => TraceLogger.LogStage($"Concurrent-{index}"));
            TraceLogger.Shutdown();

            var stageCount = File.ReadAllLines(filePath).Count(line => line.Contains("\"Stage\""));

            Assert.AreEqual(40, stageCount);
        }

        [TestMethod]
        public void LogStage_UsesConfiguredDatabaseContext()
        {
            TraceLogger.SetContextProvider(() => new TraceLogContext
            {
                DatabaseType = "SqlServer",
                DatabaseVersion = "SQL Server 2025",
                ConnectionName = "Test Connection"
            });

            Assert.IsTrue(TraceLogger.SetEnabled(true));

            var filePath = TraceLogger.CurrentLogFilePath;

            TraceLogger.LogStage("LoadMetadata");
            TraceLogger.Shutdown();

            var content = File.ReadAllText(filePath);

            Assert.IsTrue(content.Contains("\"SqlServer\""));
            Assert.IsTrue(content.Contains("\"SQL Server 2025\""));
            Assert.IsTrue(content.Contains("\"Test Connection\""));
        }

        [TestMethod]
        public void LogError_WritesExceptionTypeMessageAndFailedStatus()
        {
            Assert.IsTrue(TraceLogger.SetEnabled(true));

            var filePath = TraceLogger.CurrentLogFilePath;

            TraceLogger.LogError("TestFailure", new InvalidOperationException("Failure, with comma"));
            TraceLogger.Shutdown();

            var content = File.ReadAllText(filePath);

            Assert.IsTrue(content.Contains("\"Error\""));
            Assert.IsTrue(content.Contains("\"Failed\""));
            Assert.IsTrue(content.Contains("System.InvalidOperationException"));
            Assert.IsTrue(content.Contains("Failure, with comma"));
        }
    }
}
