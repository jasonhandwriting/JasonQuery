using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace JasonQuery.Tests.UI.QueryEditor
{
    //Source contract guards; actual backup I/O and shutdown require runtime qualification.
    [TestClass]
    [TestCategory("TABRS02B")]
    public sealed class QueryEditorBackupBeforeExitWiringGoldenTests
    {
        private static string Source(string relativePath)
        {
            foreach (var origin in new[] { AppDomain.CurrentDomain.BaseDirectory, Directory.GetCurrentDirectory() })
            {
                var directory = new DirectoryInfo(origin);

                for (var depth = 0; depth < 12 && directory != null; depth++)
                {
                    var path = Path.Combine(directory.FullName, "JasonQuery", relativePath);

                    if (File.Exists(path))
                    {
                        return File.ReadAllText(path);
                    }

                    directory = directory.Parent;
                }
            }

            Assert.Fail("Source file not found: " + relativePath);
            return string.Empty;
        }

        private static string FormSource(string fileName)
        {
            return Source(Path.Combine("UI", "Forms", fileName));
        }

        private static string BlockBody(string source, string anchor)
        {
            var start = source.IndexOf(anchor, StringComparison.Ordinal);

            Assert.IsTrue(start >= 0, "Missing source anchor: " + anchor);

            var open = source.IndexOf('{', start + anchor.Length);

            Assert.IsTrue(open >= 0, "Missing opening brace: " + anchor);

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

            Assert.Fail("Missing closing brace: " + anchor);
            return string.Empty;
        }

        [TestMethod]
        public void TABRS02B01_EnabledBackupMustNotUnconditionallyAuthorizeApplicationExit()
        {
            var close = BlockBody(FormSource("MainForm.cs"), "private bool BeforeCloseApplication()");
            var enabled = BlockBody(close, "if (AppConfigHelper.IsBackupFile)");

            Assert.IsFalse(Regex.IsMatch(enabled, @"\breturn\s+true\s*;"), "Enabled backup is a setting, not evidence of a successful final backup.");
            Assert.IsTrue(Regex.IsMatch(enabled, @"\breturn\s+TryPersistDirtyEditorBackupsBeforeClose\s*\(\s*\)\s*;"), "Application exit must depend on the final backup result.");
            Assert.IsTrue(close.Contains("return CloseTabBeforeCloseApplication();"), "Keep the existing close/save confirmation when backup is disabled.");
        }

        [TestMethod]
        public void TABRS02B02_FinalBackupMustCoverAllDirtyQueryFormsAndStopOnFailure()
        {
            var main = FormSource("MainForm.cs");
            var persist = BlockBody(main, "private bool TryPersistDirtyEditorBackupsBeforeClose()");

            Assert.IsTrue(persist.Contains("tabControl1.TabPages"), "Enumerate all workspace pages, not just the selected editor.");
            Assert.IsTrue(Regex.IsMatch(persist, @"\bis\s+QueryForm\s+query\b"), "Use actual QueryForm identity.");
            Assert.IsFalse(Regex.IsMatch(persist, @"\bSelectedTab\b|\bSelectedIndex\b|\.Title\b|\.Tag\b|\.Visible\b"), "Do not limit final backup to visible pages or derive dirty state from UI projections.");
            Assert.IsTrue(Regex.IsMatch(persist, @"query\.IsEditorDirty\s*&&\s*!query\.TryPersistEditorBackup\s*\(\s*\)"), "Every dirty editor, including retained external content, must successfully persist.");
            Assert.IsTrue(Regex.IsMatch(persist, @"!query\.TryPersistEditorBackup\s*\(\s*\)\s*\)\s*\{\s*return\s+false\s*;"), "Abort application exit if any dirty editor cannot be backed up.");
            Assert.IsTrue(persist.Contains("return true;"), "An empty or fully successful batch may authorize exit.");
        }

        [TestMethod]
        public void TABRS02B03_TimerAndExitMustShareContentBeforeMetadataPersistence()
        {
            var source = FormSource("QueryForm.FileBackup.cs");
            var tick = BlockBody(source, "private void tmrBackup_Tick(");

            Assert.IsTrue(Regex.IsMatch(tick, @"\bTryPersistEditorBackup\s*\(\s*\)"), "Reuse one backup persistence path rather than duplicating file and metadata writes.");
            Assert.IsFalse(Regex.IsMatch(tick, @"File\.WriteAllText\s*\(|UpdateEditorBackupInfoIfNeeded\s*\("), "The timer should delegate persistence to the shared method.");

            var persist = BlockBody(source, "internal bool TryPersistEditorBackup()");
            var write = persist.IndexOf("File.WriteAllText(", StringComparison.Ordinal);
            var metadata = persist.IndexOf("UpdateEditorBackupInfoIfNeeded(", StringComparison.Ordinal);
            var success = persist.IndexOf("return true;", metadata >= 0 ? metadata : 0, StringComparison.Ordinal);

            Assert.IsTrue(write >= 0 && metadata > write && success > metadata, "Report successful dirty backup only after file and metadata persistence.");
            Assert.IsTrue(Regex.IsMatch(persist, @"UpdateEditorBackupInfoIfNeeded\s*\(\s*IsEditorDirty\s*\)"), "Retained external changes must be included in recovery metadata.");
            Assert.IsFalse(persist.Contains("editor.CanUndo"), "Do not bypass the unified dirty state.");
            Assert.IsFalse(Regex.IsMatch(persist, @"DeleteBackupFileInfo\s*\(|EmptyUndoBuffer\s*\(|_requiresSaveWithoutUndo\s*=\s*false"), "Backup is not a Save and must not discard recovery or dirty state.");
        }

        [TestMethod]
        public void TABRS02B04_BackupFailureMustRemainVisibleAndMustNotDisableBackupSetting()
        {
            var source = FormSource("QueryForm.FileBackup.cs");

            Assert.IsFalse(Regex.IsMatch(source, @"AppConfigHelper\.IsBackupFile\s*=\s*false\s*;"), "A directory failure must not silently disable the user's backup preference.");

            var persist = BlockBody(source, "internal bool TryPersistEditorBackup()");
            var failure = BlockBody(persist, "catch (Exception ex)");

            Assert.IsTrue(failure.Contains("ShowExceptionMessage(ex)"), "Report the actual backup failure reason to the user.");
            Assert.IsTrue(Regex.IsMatch(failure, @"\breturn\s+false\s*;"), "A backup exception must deny shutdown authorization.");
        }

        [TestMethod]
        public void TABRS02B05_MetadataLookupAndWriteMustNotSwallowPersistenceFailures()
        {
            var backup = FormSource("QueryForm.FileBackup.cs");
            var lookup = BlockBody(backup, "private bool BackupFileInfoExists(");
            var update = BlockBody(backup, "private void UpdateExistingBackupFileInfo(");
            var insert = BlockBody(backup, "private void InsertBackupFileInfo(");

            Assert.IsTrue(lookup.Contains("JasonQueryRepository.ReadBackupMetadataOrThrow("), "A failed metadata lookup must not be mistaken for an absent record.");
            Assert.IsTrue(update.Contains("JasonQueryRepository.WriteBackupMetadataOrThrow(") && insert.Contains("JasonQueryRepository.WriteBackupMetadataOrThrow("), "Both metadata write paths must propagate persistence failure.");
            Assert.IsFalse(Regex.IsMatch(lookup + update + insert, @"JasonQueryRepository\.Exec(?:Query|NonQuery)\s*\("), "Do not use the existing error-swallowing repository methods for final backup persistence.");

            var repository = Source(Path.Combine("Database", "Internal", "Repositories", "JasonQueryRepository.cs"));
            var read = BlockBody(repository, "internal static DataTable ReadBackupMetadataOrThrow(");
            var write = BlockBody(repository, "internal static void WriteBackupMetadataOrThrow(");

            Assert.IsTrue(read.Contains("GetRuntime().ExecuteQuery") && write.Contains("GetRuntime().ExecuteNonQuery"), "Use the configured internal database runtime without provider fallback.");
            Assert.IsFalse(Regex.IsMatch(read + write, @"\bcatch\b|\bExecQuery\s*\(|\bExecNonQuery\s*\("), "Do not suppress runtime failures at the backup-specific repository boundary.");
        }

        [TestMethod]
        public void TABRS02B06_EditorInputRetainsDebounceAndRenameDoesNotCreateBackup()
        {
            var state = FormSource("QueryForm.EditorContentState.cs");
            var refresh = BlockBody(state, "private void RefreshEditorDirtyState()");
            var rename = BlockBody(FormSource("QueryForm.FileBackup.cs"), "private void UpdateExistingBackupTabTitle(");

            Assert.IsTrue(refresh.Contains("tmrBackup.Stop();") && refresh.Contains("tmrBackup.Interval = 2000;") && refresh.Contains("tmrBackup.Start();"), "Preserve the existing two-second input debounce.");
            Assert.IsFalse(Regex.IsMatch(refresh, @"File\.WriteAllText\s*\(|UpdateEditorBackupInfoIfNeeded\s*\(|TryPersistEditorBackup\s*\("), "Do not introduce synchronous backup I/O during editor input.");
            Assert.IsTrue(rename.Contains("File.Exists(GetBackupFileFullName())"), "Rename may update existing backup metadata only when the file exists.");
            Assert.IsFalse(Regex.IsMatch(rename, @"\bINSERT\b|\bInsertBackupFileInfo\s*\(|File\.WriteAllText\s*\(|TryPersistEditorBackup\s*\("), "Rename alone must not create or rewrite a backup file.");
        }
    }
}
