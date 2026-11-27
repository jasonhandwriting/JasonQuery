using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace JasonQuery.Tests.UI.QueryEditor
{
    //Source-level regression guards. Runtime read/reload failures require GUI qualification.
    [TestClass]
    [TestCategory("TABRW03B")]
    public sealed class QueryEditorFileReloadSafetyWiringGoldenTests
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

        [TestMethod]
        public void TABRW03B01_LoadMustNotClearEditorBeforeSuccessfulRead()
        {
            var load = BlockBody(FormSource("QueryForm.FileLoad.cs"), "private bool LoadFile(string fileName, out bool isLargeFile, out bool wasCancelled)");
            var read = load.IndexOf("TextFileLoadHelper.Load(", StringComparison.Ordinal);
            var reject = load.IndexOf("if (!loadResult.IsSuccess)", StringComparison.Ordinal);
            var apply = load.IndexOf("ApplyLoadedFileText(loadResult.Text)", StringComparison.Ordinal);

            Assert.IsTrue(read >= 0 && reject > read && apply > reject, "Read and reject unsuccessful results before replacing editor content.");
            Assert.IsTrue(Regex.IsMatch(BlockBody(load, "if (!loadResult.IsSuccess)"), @"\breturn\s+false\s*;"), "An unsuccessful load must return without applying content.");
            Assert.IsFalse(Regex.IsMatch(load, @"editor\s*\.\s*Text\s*=(?!=)|editor\s*\.\s*Clear(?:All)?\s*\("), "LoadFile must not clear or directly replace editor text before the success boundary.");
            Assert.AreEqual(1, Regex.Matches(load, @"ApplyLoadedFileText\s*\(").Count, "Use a single successful text replacement path.");
        }

        [TestMethod]
        public void TABRW03B02_LoadStatusMustOnlyBePublishedAfterContentReplacement()
        {
            var load = BlockBody(FormSource("QueryForm.FileLoad.cs"), "private bool LoadFile(string fileName, out bool isLargeFile, out bool wasCancelled)");
            var apply = load.IndexOf("ApplyLoadedFileText(loadResult.Text)", StringComparison.Ordinal);
            var encoding = load.IndexOf("ApplyFileLoadEncodingStatus(", StringComparison.Ordinal);
            var execution = load.IndexOf("ResetFileLoadExecutionStatus(", StringComparison.Ordinal);
            var success = load.IndexOf("return true;", StringComparison.Ordinal);
            var cleanup = BlockBody(load, "finally");

            Assert.IsTrue(apply >= 0 && encoding > apply && execution > apply && success > encoding && success > execution, "Publish encoding, EOL and execution status only after a successful editor replacement.");
            Assert.IsFalse(Regex.IsMatch(cleanup, @"ApplyFileLoadEncodingStatus\s*\(|ResetFileLoadExecutionStatus\s*\("), "The unconditional cleanup must not overwrite status on failed or cancelled loads.");
            Assert.IsTrue(Regex.IsMatch(cleanup, @"Application\.UseWaitCursor\s*=\s*false\s*;"), "Preserve wait-cursor cleanup on every exit.");
        }

        [TestMethod]
        public void TABRW03B03_ReloadMustReturnFailureBeforeClearingRecoveryState()
        {
            var source = FormSource("QueryForm.FileWatch.cs");
            var reload = BlockBody(source, "ReloadWatchedFile(string fileName, out bool wasCancelled)");
            var reject = BlockBody(reload, "if (!LoadFile(fileName, out _, out wasCancelled))");
            var backup = reload.IndexOf("ClearFileWatchBackupState()", StringComparison.Ordinal);
            var dirty = reload.IndexOf("_requiresSaveWithoutUndo = false;", StringComparison.Ordinal);
            var undo = reload.IndexOf("editor.EmptyUndoBuffer()", StringComparison.Ordinal);
            var refresh = reload.IndexOf("RefreshEditorDirtyState()", StringComparison.Ordinal);
            var success = reload.IndexOf("return true;", StringComparison.Ordinal);

            Assert.IsTrue(Regex.IsMatch(source, @"private\s+bool\s+ReloadWatchedFile\s*\("), "The caller must receive an explicit success/failure result.");
            Assert.IsTrue(Regex.IsMatch(reject, @"\breturn\s+false\s*;"), "Failed or cancelled loads must report failure before clearing recovery state.");
            Assert.IsFalse(Regex.IsMatch(reject, @"ClearFileWatchBackupState\s*\(|_requiresSaveWithoutUndo\s*=\s*false|EmptyUndoBuffer\s*\("), "A rejected load must not clear backup, retained changes or undo history.");

            var failure = reload.IndexOf("return false;", StringComparison.Ordinal);

            Assert.IsTrue(failure >= 0 && backup > failure && dirty > failure && undo > failure && refresh > dirty && refresh > undo && success > backup && success > refresh, "Only a successful load may clear recovery state and publish the refreshed editor.");
        }

        [TestMethod]
        public void TABRW03B04_WatchAcknowledgesOnlyProcessedTimestamp()
        {
            var modified = BlockBody(FormSource("QueryForm.FileWatch.cs"), "private void HandleWatchedFileModified(");
            var timestamp = modified.IndexOf("btnSaveRed.Tag = currentLastWriteTime;", StringComparison.Ordinal);
            var failedReload = Regex.Match(modified, @"if\s*\(\s*!ReloadWatchedFile\(fileName,\s*out\s+var\s+wasCancelled\)\s*\)");
            var failure = BlockBody(modified, "if (!ReloadWatchedFile(fileName, out var wasCancelled))");
            var reject = Regex.Match(failure, @"if\s*\(\s*!wasCancelled\s*\)\s*\{\s*return\s*;\s*\}");
            var retained = failure.IndexOf("MarkWatchedFileAsExternallyModified(fileName);", StringComparison.Ordinal);
            var directDecline = Regex.Match(modified, @"else\s*\{\s*MarkWatchedFileAsExternallyModified\(fileName\);\s*\}");

            Assert.IsTrue(failedReload.Success && reject.Success && retained > reject.Index && timestamp > failedReload.Index + failedReload.Length, "A real read failure must return before acknowledgement; cancellation must retain editor content.");
            Assert.IsTrue(directDecline.Success && timestamp > directDecline.Index + directDecline.Length, "Direct decline must retain content before acknowledging the observed timestamp.");
            Assert.IsFalse(failure.Contains("btnSaveRed.Tag ="), "Neither reload failure nor cancellation may bypass the common acknowledgement path.");
            Assert.AreEqual(1, Regex.Matches(modified, @"GetFileLastWriteTimeText\s*\(").Count, "Do not acknowledge a newer, unprocessed timestamp after a modal confirmation.");
            Assert.AreEqual(1, Regex.Matches(modified, @"btnSaveRed\.Tag\s*=").Count, "Use a single timestamp acknowledgement path.");
        }

        [TestMethod]
        public void TABRW03B05_SuccessfulEmptyTextStillUsesExistingReplacementContract()
        {
            var apply = BlockBody(FormSource("QueryForm.FileLoad.cs"), "private void ApplyLoadedFileText(");

            Assert.IsTrue(Regex.IsMatch(apply, @"editor\.Text\s*=\s*text\s*;"), "A successful empty file must replace the previous editor text with empty text.");
            Assert.IsFalse(Regex.IsMatch(apply, @"string\.IsNullOr(?:Empty|WhiteSpace)\s*\(|\breturn\s*;"), "Do not reject a successfully loaded empty file.");
            Assert.IsTrue(apply.Contains("editor.SelectionStart = 0;") && apply.Contains("editor.SelectionEnd = 0;") && apply.Contains("editor.ScrollCaret();"), "Preserve the existing successful-load selection and caret behavior.");
        }

        [TestMethod]
        public void TABRW03B06_OnlyExplicitLargeFileDeclineReportsCancellation()
        {
            var source = FormSource("QueryForm.FileLoad.cs");
            var load = BlockBody(source, "private bool LoadFile(string fileName, out bool isLargeFile, out bool wasCancelled)");
            var decline = BlockBody(load, "if (!TryConfirmOpenLargeFile(fileName, out isLargeFile))");
            var wrapper = BlockBody(source, "private bool LoadFile(string fileName, out bool isLargeFile)");
            var confirm = BlockBody(source, "private bool TryConfirmOpenLargeFile(");
            var reject = BlockBody(confirm, "if (!ConfirmOpenLargeFile(fileName, fileLimitAlarm))");

            Assert.IsTrue(Regex.IsMatch(load, @"^\s*\{\s*isLargeFile\s*=\s*false\s*;\s*wasCancelled\s*=\s*false\s*;"), "Cancellation must default to false, including on read errors.");
            Assert.IsTrue(Regex.IsMatch(decline, @"^\s*\{\s*wasCancelled\s*=\s*true\s*;\s*return\s+false\s*;\s*\}$"), "Only an explicit large-file decline marks the load cancelled.");
            Assert.AreEqual(2, Regex.Matches(load, @"\bwasCancelled\s*=").Count, "No other path may change the cancellation classification.");
            Assert.AreEqual(1, Regex.Matches(confirm, @"\breturn\s+false\s*;").Count, "Only an explicit refusal of the large-file warning may return false from the confirmation helper.");
            Assert.IsTrue(Regex.IsMatch(reject, @"^\s*\{\s*return\s+false\s*;\s*\}$"), "Preserve the explicit refusal boundary.");
            Assert.IsTrue(wrapper.Contains("return LoadFile(fileName, out isLargeFile, out _);"), "Existing non-watch load callers retain their boolean contract.");
        }
    }
}
