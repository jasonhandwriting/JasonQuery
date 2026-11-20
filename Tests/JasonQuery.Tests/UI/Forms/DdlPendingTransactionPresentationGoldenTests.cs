using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace JasonQuery.Tests.UI.Forms
{
    //S1-C-Fix: structural guardrails; true runtime GUI / DB evidence remains necessary.
    [TestClass]
    [TestCategory("F7S1")]
    public sealed class DdlPendingTransactionPresentationGoldenTests
    {
        private static string Source(string fileName)
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

        private static string Section(string source, string start, string end)
        {
            var first = source.IndexOf(start, StringComparison.Ordinal);

            Assert.IsTrue(first >= 0, "Missing start: " + start);

            var last = source.IndexOf(end, first + start.Length, StringComparison.Ordinal);

            Assert.IsTrue(last > first, "Missing end: " + end);
            return source.Substring(first, last - first);
        }

        private static void CheckDeferred(string fileName, string next)
        {
            var source = Source(fileName);
            var locked = Section(source, "private void LockPendingTransactionDdl(bool knownPending)", "private void ShowPendingTransactionWarningIfNeeded()");
            var presenter = Section(source, "private void ShowPendingTransactionWarningIfNeeded()", next);
            var shown = Section(source, "protected override void OnShown(EventArgs e)", "private void ApplyInitialFocus()");

            Assert.IsTrue(locked.Contains("_ddlPendingTransactionLocked = true;"));
            Assert.IsTrue(locked.Contains("btnExecute.Enabled = false;"));
            Assert.IsFalse(locked.Contains("MessageBox.Show"), "Form_Load must not show the warning before form is visible.");
            Assert.IsTrue(locked.Contains("_ddlPendingTransactionWarningMessage = message;"));
            Assert.IsTrue(presenter.Contains("_ddlPendingTransactionWarningShown = true;"));
            Assert.IsTrue(presenter.Contains("MessageBox.Show"));
            Assert.IsTrue(presenter.Contains("this,"), "The form must own its warning dialog.");
            Assert.IsTrue(shown.Contains("_ddlFormShown = true;"));
            Assert.IsTrue(shown.Contains("BeginInvoke(new MethodInvoker(ShowPendingTransactionWarningIfNeeded));"));
        }

        [TestMethod]
        public void P01_TableFormWarningIsDeferredAndOwnerModal()
        {
            CheckDeferred("EditTableForm.cs", "private void Form_ResizeEnd");
        }

        [TestMethod]
        public void P02_ColumnFormWarningIsDeferredAndOwnerModal()
        {
            CheckDeferred("EditColumnForm.cs", "private void ShowMySqlCompleteDefinitionBlockReason()");
        }

        [TestMethod]
        public void P03_TableFormStillLocksBeforeCommentMetadata()
        {
            var source = Source("EditTableForm.cs");
            var load = Section(source, "private void Form_Load(object sender, EventArgs e)", "private void ApplyPendingTransactionProtectionOnOpen()");

            Assert.IsTrue(load.IndexOf("ApplyPendingTransactionProtectionOnOpen();", StringComparison.Ordinal) < load.IndexOf("LoadCommentMetadata();", StringComparison.Ordinal));
        }

        [TestMethod]
        public void P04_ColumnFormStillLocksBeforeCommentMetadata()
        {
            var source = Source("EditColumnForm.cs");
            var load = Section(source, "private void Form_Load(object sender, EventArgs e)", "private void ApplyPendingTransactionProtectionOnOpen()");

            Assert.IsTrue(load.IndexOf("ApplyPendingTransactionProtectionOnOpen();", StringComparison.Ordinal) < load.IndexOf("LoadCommentMetadata();", StringComparison.Ordinal));
        }

        [TestMethod]
        public void P05_MySqlUnavailablePreviewHasDedicatedNonSqlExplanation()
        {
            var source = Source("EditColumnForm.cs");
            var feedback = Section(source, "private void ShowMySqlCompleteDefinitionBlockReason()", "private void ShowMySqlSqlPreviewUnavailable(");
            var overlay = Section(source, "private void ShowMySqlSqlPreviewUnavailable(", "private void Form_ResizeEnd");

            Assert.IsTrue(feedback.Contains("GetReasonKey(NonOracleF7MySqlPreviewBlockReasonCode)"));
            Assert.IsTrue(feedback.Contains("OfflineSqlPreviewOnly && string.IsNullOrWhiteSpace(editorSqlPreview.Text)"));
            Assert.IsTrue(feedback.Contains("ShowMySqlSqlPreviewUnavailable(message);"));
            Assert.IsTrue(overlay.Contains("MySqlSqlPreviewUnavailable"));
            Assert.IsTrue(overlay.Contains("lblMySqlSqlPreviewUnavailable"));
            Assert.IsTrue(overlay.Contains("editorSqlPreview.Parent.Controls.Add(label);"));
            Assert.IsFalse(overlay.Contains("editorSqlPreview.Text ="), "Explanation must not become executable/copyable SQL.");
        }

        [TestMethod]
        public void P06_ColumnSqlPreviewVisibilityFollowsOnlySqlText()
        {
            var source = Source("EditColumnForm.cs");
            var method = Section(source, "private void UpdateSqlPreviewText(string text)", "private void editorSqlPreview_TextChanged");

            Assert.IsTrue(method.Contains("editorSqlPreview.Text = text;"));
            Assert.IsTrue(method.Contains("_mySqlSqlPreviewUnavailableLabel.Visible = string.IsNullOrWhiteSpace(text);"));
            Assert.IsFalse(method.Contains("_ddlPendingTransactionLocked"), "Preview rendering must not be driven by the Execute lock.");
        }

        [TestMethod]
        public void P07_ColumnSqlBuilderDoesNotUseStickyTransactionLock()
        {
            var source = Source("EditColumnForm.cs");
            var method = Section(source, "private string BuildColumnDdlSql(ColumnDdlOperation operation)", "private void UpdateSqlPreviewText(string text)");
            var verifiedPreview = Section(source, "internal static bool TryBuildMySqlVerifiedCommentPreview", "private bool CanEnableNonOracleF7ExistingColumnNow");

            Assert.IsFalse(method.Contains("_ddlPendingTransactionLocked"), "Keep SQL building separate from Execute locking.");
            Assert.IsTrue(method.Contains("TryBuildMySqlVerifiedCommentPreview"), "The builder must use the verified Comment Preview helper.");
            Assert.IsTrue(verifiedPreview.Contains("receipt.HasVerifiedSessionSqlMode"), "Mode evidence must be present.");
            Assert.IsTrue(verifiedPreview.Contains("TryParse(receipt.VerifiedSessionSqlMode"), "The verified mode must be parsed.");
            Assert.IsTrue(verifiedPreview.Contains("MySqlCompleteColumnDefinitionSqlBuilder.TryBuildComment"), "The helper must use the shared SQL Builder.");
            Assert.IsFalse(verifiedPreview.Contains("_ddlPendingTransactionLocked"), "Preview qualification must not depend on the sticky Execute lock.");
            Assert.IsTrue(method.Contains("BuildMySqlRenameColumnSql"));
        }

        [TestMethod]
        public void P08_ExecutionProtectionRemainsFirstInBothForms()
        {
            foreach (var file in new[] { "EditTableForm.cs", "EditColumnForm.cs" })
            {
                var source = Source(file);
                var handler = Section(source, "private void btnExecute_Click(object sender, EventArgs e)", "var errorMessage = string.Empty;");

                Assert.IsTrue(handler.Contains("if (TryRejectPendingTransactionDdlExecute())"), file);
                Assert.IsTrue(handler.Contains("return;"), file);
            }
        }
    }
}
