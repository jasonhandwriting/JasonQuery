using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace JasonQuery.Tests.UI.QueryEditor
{
    //TAB-R-B2: source-level guards for the existing form wiring.
    //These are not runtime tests of WinForms, file I/O or backup recovery.
    [TestClass]
    [TestCategory("TABRB2")]
    public sealed class QueryEditorTabStateWiringGoldenTests
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
        public void TABRB2W01_DirtyRefreshMustNotDeriveDisplayNameFromUiTags()
        {
            var method = MethodBody(FormSource("QueryForm.EditorContentState.cs"), "private string BuildEditorTabTitle(bool isDirty)");

            Assert.IsFalse(Regex.IsMatch(method, @"\bTag\b"), "Dirty-state refresh must use the current display name, not QueryForm.Tag or btnSave.Tag.");
        }

        [TestMethod]
        public void TABRB2W02_BackupMetadataMustNotReadNameOrFilePathFromUiTags()
        {
            var method = MethodBody(FormSource("QueryForm.EditorContentState.cs"), "private void UpdateEditorBackupInfoIfNeeded(bool isDirty)");

            Assert.IsFalse(Regex.IsMatch(method, @"\bTag\b"), "A subsequent backup must not overwrite a renamed display name using an old UI Tag.");
        }

        [TestMethod]
        public void TABRB2W03_RestoreMustNotReplaceDisplayNameWithSavedPath()
        {
            var method = MethodBody(FormSource("QueryForm.FileBackup.cs"), "private void ApplyRestoredBackupFileState(string backupFileName)");

            Assert.IsFalse(method.Contains("{tabTitlePrefix}{BackupRealFullFileName}"), "Restored display name and saved file path must be restored independently.");
            Assert.IsFalse(Regex.IsMatch(method, @"(?<![\w.])Tag\b|\bbtnSave\s*\.\s*Tag\b"), "Restoration must not use QueryForm.Tag or btnSave.Tag as tab or file identity.");
        }

        [TestMethod]
        public void TABRB2W04_TooltipHandlersMustResolveCurrentTabState()
        {
            var source = FormSource("MainForm.cs");
            var mouseMove = MethodBody(source, "private void tabControl1_MouseMove(object sender, MouseEventArgs e)");
            var selectionChanged = MethodBody(source, "private void tabControl1_SelectionChanged(object sender, EventArgs e)");

            Assert.IsFalse(Regex.IsMatch(mouseMove, @"\.Tag\b") || Regex.IsMatch(selectionChanged, @"\.Tag\b"), "Both tooltip handlers must resolve current tab state rather than directly reading stale TabPage.Tag.");
        }

        [TestMethod]
        public void TABRB2W05_SavedFileMenuMustNotInferFileIdentityFromDisplayStrings()
        {
            var method = MethodBody(FormSource("MainForm.cs"), "private void EnableCloseTabMenu(bool bEnable, bool bCheckFavorite = false)");

            Assert.IsFalse(Regex.IsMatch(method, @"\.Title\s*\.\s*Replace\s*\("), "Saved-file detection must not compare a display title stripped of its dirty marker with a Tag value.");
        }

        [TestMethod]
        public void TABRB2W06_RenameDialogMustNotDuplicateOriginalNameInTextBoxTag()
        {
            var source = FormSource("RenameTabDialog.cs");
            var load = MethodBody(source, "private void Form_Load(object sender, EventArgs e)");
            var textChanged = MethodBody(source, "private void txtTabName_TextChanged(object sender, EventArgs e)");

            Assert.IsFalse(load.Contains("txtTabName.Tag") || textChanged.Contains("txtTabName.Tag"), "The existing strongly typed TabTitle property is sufficient for original-name comparison.");
        }

        [TestMethod]
        public void TABRB2W07_BackupTimerWritesContentBeforeMetadata()
        {
            var method = MethodBody(FormSource("QueryForm.FileBackup.cs"), "internal bool TryPersistEditorBackup()");
            var writeContent = method.IndexOf("File.WriteAllText(", StringComparison.Ordinal);
            var updateMetadata = method.IndexOf("UpdateEditorBackupInfoIfNeeded(", StringComparison.Ordinal);

            Assert.IsTrue(writeContent >= 0 && updateMetadata > writeContent, "Backup metadata must follow a successful backup-content write.");
        }

        [TestMethod]
        public void TABRB2W08_BackupMetadataRetainsSeparateDisplayNameAndSavedPathFields()
        {
            var backup = FormSource("QueryForm.FileBackup.cs");
            var main = FormSource("MainForm.cs");

            Assert.IsTrue(backup.Contains("AttributeText = '{escapedTabTitle}'"));
            Assert.IsTrue(backup.Contains("AttributeText2 = '{escapedSavedFullFileName}'"));

            var insert = MethodBody(backup, "private void InsertBackupFileInfo(");

            Assert.IsTrue(insert.Contains("AttributeText, AttributeText2"));
            Assert.IsTrue(insert.Contains("{escapedTabTitle}"));
            Assert.IsTrue(insert.Contains("{escapedSavedFullFileName}"));
            Assert.IsTrue(main.Contains("GetSafeString(\"AttributeText\")"));
            Assert.IsTrue(main.Contains("GetSafeString(\"AttributeText2\")"));
        }
    }
}
