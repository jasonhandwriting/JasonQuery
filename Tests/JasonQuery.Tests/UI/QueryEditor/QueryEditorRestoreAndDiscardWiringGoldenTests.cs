using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace JasonQuery.Tests.UI.QueryEditor
{
    //TAB-R-C3-C: source contracts for recovered content and tab identity.
    //These are not runtime tests of WinForms, file I/O or backup recovery.
    [TestClass]
    [TestCategory("TABRC3C")]
    public sealed class QueryEditorRestoreAndDiscardWiringGoldenTests
    {
        private static string FormSource(string fileName)
        {
            foreach (var origin in new[] { AppDomain.CurrentDomain.BaseDirectory, Directory.GetCurrentDirectory() })
            {
                var directory = new DirectoryInfo(origin);

                for (var depth = 0; depth < 12 && directory != null; depth++)
                {
                    var path = Path.Combine(directory.FullName, "JasonQuery", "UI", "Forms", fileName);

                    if (File.Exists(path))
                    {
                        return File.ReadAllText(path);
                    }

                    directory = directory.Parent;
                }
            }

            Assert.Fail("Source file not found: " + fileName);
            return string.Empty;
        }

        private static string MethodBody(string source, string signature)
        {
            var start = source.IndexOf(signature, StringComparison.Ordinal);

            Assert.IsTrue(start >= 0, "Missing source method: " + signature);

            var open = source.IndexOf('{', start + signature.Length);

            Assert.IsTrue(open >= 0, "Missing opening brace: " + signature);

            var depth = 0;

            for (var index = open; index < source.Length; index++)
            {
                if (source[index] == '{')
                {
                    depth++;
                }
                else if (source[index] == '}')
                {
                    depth--;

                    if (depth == 0)
                    {
                        return source.Substring(open, index - open + 1);
                    }
                }
            }

            Assert.Fail("Missing closing brace: " + signature);
            return string.Empty;
        }

        [TestMethod]
        public void TABRC3C01_RestoredDirtyContentDoesNotFabricateUndoHistory()
        {
            var source = FormSource("QueryForm.FileBackup.cs");
            var restore = MethodBody(source, "private void MarkRestoredBackupAsModifiedIfNeeded()");
            var position = MethodBody(source, "private void RestoreBackupEditorPosition()");

            Assert.IsTrue(restore.Contains("_requiresSaveWithoutUndo = true;"));
            Assert.IsTrue(restore.IndexOf("_requiresSaveWithoutUndo = true;", StringComparison.Ordinal) < restore.IndexOf("CheckEditorContent();", StringComparison.Ordinal));
            Assert.IsFalse(Regex.IsMatch(restore, @"editor\.(?:AppendText|InsertText|DeleteRange|ReplaceSelection)\s*\(|editor\.Text\s*="));
            Assert.IsTrue(position.Contains("editor.EmptyUndoBuffer();"));
            Assert.IsTrue(position.Contains("SetEditorPositionSafely(BackupCurrentPosition);"));
        }

        [TestMethod]
        public void TABRC3C02_FileDeletionDiscardResolvesSenderAndRestoresEditor()
        {
            var watch = MethodBody(FormSource("QueryForm.FileWatch.cs"), "private void HandleWatchedFileDeleted(");
            var main = FormSource("MainForm.cs");
            var close = MethodBody(main, "else if (newValue.StartsWith(\"CloseQueryForm`\", StringComparison.Ordinal))");
            var remove = MethodBody(main, "private void RemoveDiscardedQueryTab(");

            Assert.IsTrue(Regex.IsMatch(watch, @"ClearFileWatchBackupState\(\);\s*TransferValueToMainForm\(\$""CloseQueryForm`\{AccessibleDescription\}""\);\s*return;"));
            Assert.IsTrue(close.Contains("sender is QueryForm query") && close.Contains("query.AccessibleDescription") && close.Contains("RemoveDiscardedQueryTab(query);"));
            Assert.IsTrue(remove.Contains("ReferenceEquals(page.Control, query)") && remove.Contains("query.AccessibleDescription"));
            Assert.IsTrue(remove.Contains("RemoveClosedTabPage(tabPage);") && remove.Contains("CreateNewTab(\"Query\", CheckTabNameExist());") && remove.Contains("UpdateTabList();"));
            Assert.IsTrue(remove.Contains("!_isClosingApplication && !_isClosingTabPages"));
        }

        [TestMethod]
        public void TABRC3C03_CancelOpenNeverRemovesAnUnrelatedSelectedTab()
        {
            var main = FormSource("MainForm.cs");
            var close = MethodBody(main, "else if (newValue.StartsWith(\"CloseEmptyTab`\", StringComparison.Ordinal))");

            Assert.IsTrue(close.Contains("sender is QueryForm query && !query.HasSavedFile && !query.IsEditorDirty"));
            Assert.IsTrue(close.Contains("RemoveDiscardedQueryTab(query);"));
            Assert.IsFalse(close.Contains("SelectedTab") || close.Contains("CancelOpenAndCloseTab"));
            Assert.IsFalse(main.Contains("MyGlobal.CancelOpenAndCloseTab") || FormSource("QueryForm.cs").Contains("MyGlobal.CancelOpenAndCloseTab"));
        }

        [TestMethod]
        public void TABRC3C04_GeneratedSqlRoutesOnlyToTheDedicatedLiveEditor()
        {
            var main = FormSource("MainForm.cs");
            var paste = MethodBody(main, "private void PasteFromSchemaBrowser(");
            var find = MethodBody(main, "private TabPage FindSchemaBrowserSqlEditorTab()");
            var ack = MethodBody(main, "else if (MyGlobal.GlobalTemp == \"Rename4SchemaBrowser\")");

            Assert.IsTrue(main.Contains("_schemaBrowserSqlEditor = _queryForm;"));
            Assert.IsTrue(paste.Contains("FindSchemaBrowserSqlEditorTab();") && paste.Contains("tabPage.AccessibleDescription"));
            Assert.IsTrue(find.Contains("ReferenceEquals(page.Control, query)") && find.Contains("query.AccessibleDescription"));
            Assert.IsTrue(find.Contains("query.HasSavedFile") && find.Contains("query.IsDisposed") && find.Contains("!query.IsEditorDirty"));
            Assert.IsTrue(ack.Contains("ReferenceEquals(theTab, generatedEditorTab)"));
        }
    }
}
