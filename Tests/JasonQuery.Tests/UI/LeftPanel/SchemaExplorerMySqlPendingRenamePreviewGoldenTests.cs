using JasonQuery.UI.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Reflection;

namespace JasonQuery.Tests.UI.LeftPanel
{
    //MySQL Modern Rename Preview: Pending permits only an already-open, read-only VERSION() check.
    //These are pure callback and source-wiring checks; no database or DDL execution.
    [TestClass]
    [TestCategory("F7S1")]
    public sealed class SchemaExplorerMySqlPendingRenamePreviewGoldenTests
    {
        private static readonly Guid OpenConnectionId = new Guid("f8b166ca-a21c-48c3-af28-82a76523f232");

        private static bool CanInspect(bool pending, Guid currentId, Func<bool> ensureConnection)
        {
            var method = typeof(MainForm).GetMethod
            (
                "CanInspectMySqlRenameVersionForPreview",
                BindingFlags.Static | BindingFlags.NonPublic
            );

            Assert.IsNotNull(method, "Golden RED: the pending-safe Preview connection policy does not exist.");
            return (bool)method.Invoke(null, new object[] { pending, currentId, ensureConnection });
        }

        private static string ReadSource(string fileName)
        {
            var folder = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);

            while (folder != null)
            {
                var path = Path.Combine(folder.FullName, "JasonQuery", "UI", "Forms", fileName);

                if (File.Exists(path))
                {
                    return File.ReadAllText(path);
                }

                folder = folder.Parent;
            }

            Assert.Fail("Source not found: " + fileName);
            return string.Empty;
        }

        private static string Section(string source, string start, string end)
        {
            var a = source.IndexOf(start, StringComparison.Ordinal);

            Assert.IsTrue(a >= 0, "Missing section start: " + start);

            var b = source.IndexOf(end, a + start.Length, StringComparison.Ordinal);

            Assert.IsTrue(b > a, "Missing section end: " + end);
            return source.Substring(a, b - a);
        }

        [TestMethod]
        public void M01_PendingOpenReaderAllowsVersionInspectionWithoutReconnect()
        {
            var connects = 0;
            var allowed = CanInspect(true, OpenConnectionId, () => { connects++; return false; });

            Assert.IsTrue(allowed);
            Assert.AreEqual(0, connects, "Pending must never invoke the reconnect callback.");
        }

        [TestMethod]
        public void M02_PendingClosedReaderCannotReconnectForPreview()
        {
            var connects = 0;
            var allowed = CanInspect(true, Guid.Empty, () => { connects++; return true; });

            Assert.IsFalse(allowed);
            Assert.AreEqual(0, connects, "Pending must not reopen a disconnected Reader.");
        }

        [TestMethod]
        public void M03_PendingDoesNotNeedReconnectDelegate()
        {
            Assert.IsTrue(CanInspect(true, OpenConnectionId, null));
            Assert.IsFalse(CanInspect(true, Guid.Empty, null));
        }

        [TestMethod]
        public void M04_CleanPreservesSafeReconnectWhenAvailable()
        {
            var connects = 0;
            var allowed = CanInspect(false, Guid.Empty, () => { connects++; return true; });

            Assert.IsTrue(allowed);
            Assert.AreEqual(1, connects);
        }

        [TestMethod]
        public void M05_CleanReconnectFailureOrAbsentCallbackDoesNotPermitVersionInspection()
        {
            var connects = 0;

            Assert.IsFalse(CanInspect(false, Guid.Empty, () => { connects++; return false; }));
            Assert.AreEqual(1, connects);
            Assert.IsFalse(CanInspect(false, Guid.Empty, null));
        }

        [TestMethod]
        public void M06_ModernRenameBridgeNoLongerRejectsPendingBeforeVersionCheck()
        {
            var source = ReadSource("MainForm.SharedSchemaExplorerObjectCommands.cs");
            var bridge = Section(source, "var modernMySqlRenamePreview = false;", "//B3-B2B: a verified definition receipt");

            Assert.IsFalse(bridge.Contains("&& !AppConfigHelper.IsNotCommitYet"));
            Assert.IsTrue(bridge.Contains("var hasPendingTransaction = AppConfigHelper.IsNotCommitYet;"));
            Assert.IsTrue(bridge.Contains("CanInspectMySqlRenameVersionForPreview"));
            Assert.IsTrue(bridge.Contains("reader.TryReadServerVersionOnVerifiedConnection(previewConnectionId, out serverVersion, out versionError)"));
            Assert.IsTrue(bridge.Contains("EditColumnForm.CanEnableVerifiedMySqlModernRename"));
        }

        [TestMethod]
        public void M07_PreviewEligibilityDoesNotBypassFormExecuteGate()
        {
            var source = ReadSource("MainForm.SharedSchemaExplorerObjectCommands.cs");
            var bridge = Section(source, "var modernMySqlRenamePreview = false;", "//Refresh F7 only after the Add Column transaction is resolved.");

            Assert.IsTrue(bridge.Contains("form.NonOracleF7MySqlModernRenamePreview = modernMySqlRenamePreview;"));
            Assert.IsTrue(bridge.Contains("!modernMySqlRenamePreview && completeDefinitionPreview == null"));
            Assert.IsTrue(bridge.Contains("form.RequireNonOracleF7ExistingColumnGate = true;"));

            var formSource = ReadSource("EditColumnForm.cs");
            var execute = Section(formSource, "private void btnExecute_Click(object sender, EventArgs e)", "var errorMessage = string.Empty;");

            Assert.IsTrue(execute.Contains("if (TryRejectPendingTransactionDdlExecute())"));
        }
    }
}
