using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace JasonQuery.Tests.UI.QueryEditor
{
    //TAB-R-C2b1: source-level regression guards for relative tab closing.
    //Runtime close/cancel, Options cleanup and backup safety require GUI qualification.
    [TestClass]
    [TestCategory("TABRC2B1")]
    public sealed class QueryEditorTabCloseIdentityWiringGoldenTests
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

        private static void AssertRelativeCloseUsesReference(string menuName, bool closeLeft, bool closeRight)
        {
            var source = FormSource("MainForm.cs");
            var handler = BlockBody(source, "_cMenu.Items[MapColumn." + menuName + "].Click += delegate");

            Assert.IsFalse(Regex.IsMatch(handler, @"\.Title\b"), "The relative close target must not be selected by a mutable display title: " + menuName);

            var expectedCall = @"\bCloseRelativeTabs\s*\(\s*" + (closeLeft ? "true" : "false") + @"\s*,\s*" + (closeRight ? "true" : "false") + @"\s*\)";

            Assert.IsTrue(Regex.IsMatch(handler, expectedCall), "The handler must route through the shared relative-close operation with the correct direction: " + menuName);

            var operation = BlockBody(source, "private void CloseRelativeTabs(bool closeLeft, bool closeRight)");

            Assert.IsTrue(Regex.IsMatch(operation, @"\bReferenceEquals\s*\("), "The original selected page must be identified by object reference.");
            Assert.IsTrue(Regex.IsMatch(operation, @"\.ToArray\s*\("), "Capture the original page order before any close can change the collection.");
            Assert.IsFalse(Regex.IsMatch(operation, @"\.Title\b"), "The shared close operation must not compare display titles.");
        }

        [TestMethod]
        public void TABRC2B101_CloseAllButThisUsesSelectedPageIdentity()
        {
            AssertRelativeCloseUsesReference("CloseAllButThis", true, true);
        }

        [TestMethod]
        public void TABRC2B102_CloseAllLeftUsesOriginalSelectedPageBoundary()
        {
            AssertRelativeCloseUsesReference("CloseAllLeft", true, false);
        }

        [TestMethod]
        public void TABRC2B103_CloseAllRightUsesOriginalSelectedPageBoundary()
        {
            AssertRelativeCloseUsesReference("CloseAllRight", false, true);
        }

        [TestMethod]
        public void TABRC2B104_CloseTabIndexDoesNotReuseMutableIndexAfterDispatch()
        {
            var method = BlockBody(FormSource("MainForm.cs"), "private void CloseTabIndex(int tabIndex)");
            var indexReads = Regex.Matches(method, @"tabControl1\s*\.\s*TabPages\s*\[\s*tabIndex\s*\]");

            Assert.IsTrue(indexReads.Count <= 1, "Resolve the requested page once; do not resolve a mutable index again after a nested message loop or close confirmation.");
        }

        [TestMethod]
        public void TABRC2B105_OptionsCloseCallbackResolvesActualSenderPage()
        {
            var method = BlockBody(FormSource("MainForm.cs"), "else if (newValue.StartsWith(\"CloseOptionsTab`\", StringComparison.Ordinal))");

            Assert.IsTrue(Regex.IsMatch(method, @"sender\s+is\s+OptionsForm\b"), "Only the actual OptionsForm sender can authorize removal of its page.");
            Assert.IsTrue(Regex.IsMatch(method, @"\bReferenceEquals\s*\("), "Resolve the current page using the sender instance, not its former index or a display title.");
            Assert.IsFalse(Regex.IsMatch(method, @"\.Title\b|TabPages\s*\[\s*tabIndexValue\s*\]"), "Options close must not remove an unrelated page through stale display or index state.");
        }

        [TestMethod]
        public void TABRC2B106_FailedSaveDuringCloseMustAcknowledgeCancellation()
        {
            var source = FormSource("QueryForm.cs");
            var close = BlockBody(source, "else if (MyGlobal.GlobalTemp.StartsWith(\"CloseQueryForm`\", StringComparison.Ordinal))");
            var failedSave = BlockBody(close, "if (!Save())");

            Assert.IsTrue(failedSave.Contains("|CANCEL;"), "While the legacy close request protocol remains in use, failed or cancelled Save must acknowledge cancellation before returning; MainForm must not wait forever or remove the backup.");
        }
    }
}
