using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace JasonQuery.Tests.UI.QueryEditor
{
    //TAB-R-W02-B: source-level regression guards, not runtime file I/O tests.
    //W03 still owns safe replacement of editor content and file-watch timestamps.
    [TestClass]
    [TestCategory("TABRW02B")]
    public sealed class QueryEditorExternalModificationWiringGoldenTests
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

        private static void RequirePattern(string source, string pattern, string message)
        {
            Assert.IsTrue(Regex.IsMatch(source, pattern), message);
        }

        private static void ForbidPattern(string source, string pattern, string message)
        {
            Assert.IsFalse(Regex.IsMatch(source, pattern), message);
        }

        [TestMethod]
        public void TABRW02B01_DirtyStateCombinesUndoAndRetainedExternalChanges()
        {
            var source = FormSource("QueryForm.EditorContentState.cs");

            RequirePattern(source, @"private\s+bool\s+_requiresSaveWithoutUndo\s*(?:=\s*false\s*)?;", "Each QueryForm must own its retained external-change state.");
            RequirePattern(source, @"internal\s+bool\s+IsEditorDirty\s*=>\s*editor\.CanUndo\s*\|\|\s*_requiresSaveWithoutUndo\s*;", "An empty undo buffer must not make retained external content clean.");
        }

        [TestMethod]
        public void TABRW02B02_DecliningReloadRecordsExternalChangesBeforeRefreshingDirtyState()
        {
            var source = FormSource("QueryForm.FileWatch.cs");
            var retain = BlockBody(source, "private void MarkWatchedFileAsExternallyModified(");
            var confirm = BlockBody(source, "private bool ConfirmReloadExternallyModifiedFile(");
            var modified = BlockBody(source, "private void HandleWatchedFileModified(");

            RequirePattern(modified, @"else\s*\{\s*MarkWatchedFileAsExternallyModified\(", "Declining reload must use the retained-content path.");
            RequirePattern(retain, @"_requiresSaveWithoutUndo\s*=\s*true\s*;\s*RefreshEditorDirtyState\s*\(\s*\)\s*;", "Record external divergence before publishing dirty state and scheduling backup.");
            ForbidPattern(retain, @"btnSave(?:Red)?\.Visible\s*=|PublishEditorTabState\s*\(\s*true\s*\)", "Do not simulate dirty state by directly changing UI controls.");
            RequirePattern(confirm, @"\bIsEditorDirty\b", "A later reload prompt must recognize retained content.");
            ForbidPattern(confirm, @"editor\.CanUndo", "Reload confirmation is a data-loss decision, not undo availability.");
        }

        [TestMethod]
        public void TABRW02B03_DirtyRefreshUsesOneStateForButtonsBackupAndTabPublication()
        {
            var body = BlockBody(FormSource("QueryForm.EditorContentState.cs"), "private void RefreshEditorDirtyState()");
            var match = Regex.Match(body, @"var\s+(\w+)\s*=\s*IsEditorDirty\s*;");

            Assert.IsTrue(match.Success, "Capture unified dirty state once per refresh.");

            var dirty = Regex.Escape(match.Groups[1].Value);

            RequirePattern(body, @"ApplyEditorSaveButtonsState\s*\(\s*" + dirty + @"\s*\)", "Save buttons must use unified dirty state.");
            RequirePattern(body, @"if\s*\(\s*AppConfigHelper\.IsBackupFile\s*&&\s*" + dirty + @"\s*\)", "Retained external content must qualify for backup.");
            RequirePattern(body, @"PublishEditorTabState\s*\(\s*" + dirty + @"\s*\)", "Tab title and tooltip must use the same dirty state.");
            ForbidPattern(body, @"editor\.CanUndo", "Dirty refresh must not bypass the unified property.");
        }

        [TestMethod]
        public void TABRW02B04_BackupContentAndMetadataUseUnifiedDirtyState()
        {
            var source = FormSource("QueryForm.FileBackup.cs");
            var persist = BlockBody(source, "internal bool TryPersistEditorBackup()");
            var rename = BlockBody(source, "private void UpdateExistingBackupTabTitle(");
            var write = persist.IndexOf("File.WriteAllText(", StringComparison.Ordinal);
            var metadata = persist.IndexOf("UpdateEditorBackupInfoIfNeeded(", StringComparison.Ordinal);

            RequirePattern(persist, @"!AppConfigHelper\.IsBackupFile\s*\|\|\s*!IsEditorDirty", "Backup eligibility must include retained external changes.");
            RequirePattern(persist, @"UpdateEditorBackupInfoIfNeeded\s*\(\s*IsEditorDirty\s*\)", "Backup metadata must describe retained dirty content.");
            RequirePattern(rename, @"!IsEditorDirty\b", "Existing backup title updates must include retained dirty content.");
            ForbidPattern(persist + rename, @"editor\.CanUndo", "Backup consumers must not rely on undo availability alone.");

            Assert.IsTrue(write >= 0 && metadata > write, "Preserve backup content-before-metadata ordering.");
            Assert.IsTrue(rename.Contains("File.Exists(GetBackupFileFullName())"), "Rename must not create a new backup record.");
        }

        [TestMethod]
        public void TABRW02B05_RenameObtainsDirtyStateFromQueryFormNotPublishedTitle()
        {
            var main = BlockBody(FormSource("MainForm.cs"), "_cMenu.Items[MapColumn.RenameTab].Click += delegate");
            var source = FormSource("QueryForm.EditorContentState.cs");
            var rename = BlockBody(source, "internal bool TryRenameEditorTab(");

            RequirePattern(source, @"internal\s+bool\s+TryRenameEditorTab\s*\(\s*string\s+\w+\s*\)", "Rename must not accept a caller-supplied dirty flag.");
            RequirePattern(main, @"query\.TryRenameEditorTab\s*\(\s*form\.RenamedTabTitle\s*\)", "MainForm must pass only the new display name.");
            ForbidPattern(main, @"\.Title\s*\.\s*StartsWith\s*\(\s*""\*""", "A display marker is not a dirty-state authority.");
            RequirePattern(rename, @"\bIsEditorDirty\b", "Rename publication must use QueryForm's current dirty state.");
            ForbidPattern(rename, @"_requiresSaveWithoutUndo\s*=", "Renaming must not change retained content state.");
        }

        [TestMethod]
        public void TABRW02B06_CloseAndBatchCloseUseActualDirtyState()
        {
            var query = BlockBody(FormSource("QueryForm.cs"), "else if (MyGlobal.GlobalTemp.StartsWith(\"CloseQueryForm`\", StringComparison.Ordinal))");
            var main = FormSource("MainForm.cs");
            var clean = BlockBody(main, "_cMenu.Items[MapColumn.CloseAllSavedFiles].Click += delegate");
            var dirty = BlockBody(main, "_cMenu.Items[MapColumn.CloseAllUnsavedFiles].Click += delegate");
            var exit = BlockBody(main, "private bool CloseTabBeforeCloseApplication()");

            RequirePattern(query, @"if\s*\(\s*IsEditorDirty\s*\)", "Closing a retained editor must still prompt to save.");
            ForbidPattern(query, @"editor\.CanUndo|btnSaveRed\.Visible", "Close authority must not depend on undo or save-button visibility.");
            RequirePattern(clean, @"is\s+QueryForm\s+(\w+)\s*&&\s*!\s*\1\.IsEditorDirty\b", "Preserve Close All Saved as the clean-editor selection.");
            RequirePattern(dirty, @"is\s+QueryForm\s+(\w+)\s*&&\s*\1\.IsEditorDirty\b", "Close All Unsaved must select dirty editors.");
            RequirePattern(exit, @"is\s+QueryForm\s+(\w+)\s*&&\s*\1\.IsEditorDirty\b", "Application close must select retained dirty editors when backup is disabled.");
            ForbidPattern(clean + dirty + exit, @"\.Title\b|\.HasSavedFile\b", "Do not classify dirty content using display text or file identity.");
        }

        [TestMethod]
        public void TABRW02B07_SaveAndReloadClearRetainedChangesOnlyAfterSuccess()
        {
            var source = FormSource("QueryForm.FileSave.cs");
            var write = BlockBody(source, "private bool WriteFile(");
            var apply = BlockBody(source, "private void ApplySavedFileState(");
            var save = BlockBody(source, "private bool Save()");
            var saveAs = BlockBody(source, "private bool SaveAs(string fileName)");
            var reload = BlockBody(FormSource("QueryForm.FileWatch.cs"), "private bool ReloadWatchedFile(");
            var reset = @"_requiresSaveWithoutUndo\s*=\s*false\s*;";

            RequirePattern(write, @"File\.WriteAllText\([\s\S]*?catch\s*\([^)]*\)\s*\{[\s\S]*?return\s+false\s*;\s*\}\s*ApplySavedFileState\s*\(", "The write failure must return before applying successful-save state.");
            ForbidPattern(write + save + saveAs, reset, "Do not clear retained changes in a save attempt or failure path.");
            RequirePattern(apply, reset + @"[\s\S]*?RefreshEditorDirtyState\s*\(\s*\)", "Successful save must clear retained changes before publishing clean state.");
            RequirePattern(reload, @"if\s*\(\s*!LoadFile\(fileName,\s*out\s+_,\s*out\s+wasCancelled\)\s*\)\s*\{\s*return\s+false\s*;\s*\}[\s\S]*?" + reset, "Do not clear retained changes if reload is cancelled or fails.");

            Assert.AreEqual(1, Regex.Matches(reload, reset).Count, "The successful reload path owns its reset.");
        }

        [TestMethod]
        public void TABRW02B08_UndoRedoAvailabilityAndZoomStillUseActualUndoBuffer()
        {
            var menu = BlockBody(FormSource("QueryForm.QueryEditorContextMenuStateApplier.cs"), "private static void ApplyEditorBasicEditMenuState(");
            var zoom = BlockBody(FormSource("QueryForm.cs"), "private void editor_ZoomChanged(");

            RequirePattern(menu, @"Items\[QueryEditorColumn\.Undo\]\.Enabled\s*=\s*owner\.editor\.CanUndo\s*;", "Undo availability is not file dirty state.");
            RequirePattern(menu, @"Items\[QueryEditorColumn\.Redo\]\.Enabled\s*=\s*owner\.editor\.CanRedo\s*;", "Redo availability must remain independent.");
            RequirePattern(zoom, @"var\s+(\w+)\s*=\s*editor\.CanUndo\s*;", "Zoom must capture real undo-buffer state before its temporary text changes.");

            Assert.IsTrue(zoom.Contains("editor.EmptyUndoBuffer();"), "Preserve the existing zoom undo restoration.");

            ForbidPattern(menu + zoom, @"\bIsEditorDirty\b|_requiresSaveWithoutUndo\s*=", "Undo/redo and zoom must not reset retained external changes.");
        }
    }
}
