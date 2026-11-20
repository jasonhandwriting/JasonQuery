using JasonQuery.UI.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace JasonQuery.Tests.UI.Forms
{
    //F7-S1-B2A: source/wiring Golden RED, not a substitute for runtime DB or GUI evidence.
    //Expected against the reviewed B1 state: 10 RED, 0 GREEN. No DB connection or DDL dispatch.
    [TestClass]
    [TestCategory("F7S1")]
    public sealed class DdlPendingTransactionFormIntegrationGoldenTests
    {
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

            Assert.Fail("Source file not found: " + fileName);
            return string.Empty;
        }

        private static string Between(string source, string startMarker, string endMarker)
        {
            var start = source.IndexOf(startMarker, StringComparison.Ordinal);

            Assert.IsTrue(start >= 0, "Missing source entry: " + startMarker);

            var end = source.IndexOf(endMarker, start + startMarker.Length, StringComparison.Ordinal);

            Assert.IsTrue(end > start, "Missing source end marker: " + endMarker);
            return source.Substring(start, end - start);
        }

        private static void AssertLoadGuardBeforeCommentMetadata(string fileName, string nextMethod)
        {
            var body = Between
            (
                ReadSource(fileName),
                "private void Form_Load(object sender, EventArgs e)",
                nextMethod
            );

            var gate = body.IndexOf("ApplyPendingTransactionProtectionOnOpen();", StringComparison.Ordinal);
            var metadata = body.IndexOf("LoadCommentMetadata();", StringComparison.Ordinal);

            Assert.IsTrue(gate >= 0, "Expected RED: Form_Load must apply the S1 protection before loading metadata.");
            Assert.IsTrue(metadata >= 0, "Original Comment metadata path must remain present.");
            Assert.IsTrue(gate < metadata, "S1 must inspect transaction safety before any comment metadata read/reconnect.");
        }

        private static void AssertExecuteGuardRunsFirst(string fileName)
        {
            var source = ReadSource(fileName);
            var start = source.IndexOf("private void btnExecute_Click(object sender, EventArgs e)", StringComparison.Ordinal);

            Assert.IsTrue(start >= 0, "Missing Execute handler.");

            var end = source.IndexOf("var errorMessage = string.Empty;", start, StringComparison.Ordinal);

            Assert.IsTrue(end > start, "Missing Execute handler pre-dispatch initialization.");

            var prefix = source.Substring(start, end - start);

            Assert.IsTrue
            (
                Regex.IsMatch(prefix, @"if\s*\(\s*TryRejectPendingTransactionDdlExecute\s*\(\s*\)\s*\)", RegexOptions.CultureInvariant),
                "Expected RED: Execute must reject pending/unknown transaction before confirmation, reconnect or Reader dispatch."
            );
        }

        private static void AssertEveryPreviewEnablementHonorsStickyLock(string fileName, int expectedCount)
        {
            var source = ReadSource(fileName);

            var matches = Regex.Matches
            (
                source,
                @"btnExecute\.Enabled\s*=\s*!OfflineSqlPreviewOnly[^;]*;",
                RegexOptions.CultureInvariant
            );

            Assert.AreEqual(expectedCount, matches.Count, "Unexpected number of preview-driven button enablement sites; review all sites before changing this Golden.");

            foreach (Match match in matches)
            {
                Assert.IsTrue
                (
                    match.Value.Contains("!_ddlPendingTransactionLocked"),
                    "Expected RED: a SQL Preview change must never re-enable Execute when the form was locked: " + match.Value
                );
            }
        }

        [TestMethod]
        public void B2A01_TableFormDeclaresStickyTransactionLock()
        {
            var field = typeof(EditTableForm).GetField("_ddlPendingTransactionLocked", BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.IsNotNull(field, "Expected RED: the Table form must retain an irreversible per-open transaction lock.");
            Assert.AreEqual(typeof(bool), field.FieldType);
        }

        [TestMethod]
        public void B2A02_ColumnFormDeclaresStickyTransactionLock()
        {
            var field = typeof(EditColumnForm).GetField("_ddlPendingTransactionLocked", BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.IsNotNull(field, "Expected RED: the Column form must retain an irreversible per-open transaction lock.");
            Assert.AreEqual(typeof(bool), field.FieldType);
        }

        [TestMethod]
        public void B2A03_TableFormChecksTransactionBeforeMetadata()
        {
            AssertLoadGuardBeforeCommentMetadata("EditTableForm.cs", "private void Form_ResizeEnd");
        }

        [TestMethod]
        public void B2A04_ColumnFormChecksTransactionBeforeMetadata()
        {
            AssertLoadGuardBeforeCommentMetadata("EditColumnForm.cs", "private void ShowMySqlCompleteDefinitionBlockReason");
        }

        [TestMethod]
        public void B2A05_TableFormBlocksAtStartOfExecuteHandler()
        {
            AssertExecuteGuardRunsFirst("EditTableForm.cs");
        }

        [TestMethod]
        public void B2A06_ColumnFormBlocksAtStartOfExecuteHandler()
        {
            AssertExecuteGuardRunsFirst("EditColumnForm.cs");
        }

        [TestMethod]
        public void B2A07_TableSqlPreviewCannotReenableLockedExecute()
        {
            AssertEveryPreviewEnablementHonorsStickyLock("EditTableForm.cs", 2);
        }

        [TestMethod]
        public void B2A08_ColumnSqlPreviewCannotReenableLockedExecute()
        {
            AssertEveryPreviewEnablementHonorsStickyLock("EditColumnForm.cs", 2);
        }

        [TestMethod]
        public void B2A09_TableFormHasExecuteTimeProtectionMethod()
        {
            var method = typeof(EditTableForm).GetMethod("TryRejectPendingTransactionDdlExecute", BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.IsNotNull(method, "Expected RED: missing Table Execute-Time last-mile S1 check.");
            Assert.AreEqual(typeof(bool), method.ReturnType);
        }

        [TestMethod]
        public void B2A10_ColumnFormHasExecuteTimeProtectionMethod()
        {
            var method = typeof(EditColumnForm).GetMethod("TryRejectPendingTransactionDdlExecute", BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.IsNotNull(method, "Expected RED: missing Column Execute-Time last-mile S1 check.");
            Assert.AreEqual(typeof(bool), method.ReturnType);
        }
    }
}
