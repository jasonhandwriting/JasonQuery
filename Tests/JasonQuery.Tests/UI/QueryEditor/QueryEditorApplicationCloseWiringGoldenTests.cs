using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace JasonQuery.Tests.UI.QueryEditor
{
    //TAB-R-C2b3a: source guards for application-close orchestration.
    //Actual confirmation, transaction and cancellation behavior requires VM GUI qualification.
    [TestClass]
    [TestCategory("TABRC2B3A")]
    public sealed class QueryEditorApplicationCloseWiringGoldenTests
    {
        private static string ReadMainForm()
        {
            foreach (var origin in new[] { AppDomain.CurrentDomain.BaseDirectory, Directory.GetCurrentDirectory() })
            {
                var directory = new DirectoryInfo(origin);

                for (var depth = 0; depth < 12 && directory != null; depth++)
                {
                    var path = Path.Combine(directory.FullName, "JasonQuery", "UI", "Forms", "MainForm.cs");

                    if (File.Exists(path))
                    {
                        return File.ReadAllText(path);
                    }

                    directory = directory.Parent;
                }
            }

            Assert.Fail("MainForm.cs not found.");
            return string.Empty;
        }

        private static string BetweenMethods(string source, string startAnchor, string endAnchor)
        {
            var start = source.IndexOf(startAnchor, StringComparison.Ordinal);

            Assert.IsTrue(start >= 0, "Missing start anchor: " + startAnchor);

            var end = source.IndexOf(endAnchor, start + startAnchor.Length, StringComparison.Ordinal);

            Assert.IsTrue(end > start, "Missing end anchor: " + endAnchor);
            return source.Substring(start, end - start);
        }

        private static string ApplicationCloseSource(string source)
        {
            return BetweenMethods(source, "private bool BeforeCloseApplication()", "private bool CloseTabBeforeCloseApplication()");
        }

        [TestMethod]
        public void TABRC2B3A01_ApplicationCloseOwnsGuardBeforeDispatchAndReleasesItInFinally()
        {
            var body = ApplicationCloseSource(ReadMainForm());

            Assert.IsTrue(Regex.IsMatch(body, @"if\s*\(\s*_isClosingApplication\b[^)]*\)\s*\{\s*return false;"));

            var claim = body.IndexOf("_isClosingApplication = true;", StringComparison.Ordinal);
            var dispatch = body.IndexOf("MyGlobal.GlobalTemp = sb.ToString();", StringComparison.Ordinal);

            Assert.IsTrue(claim >= 0 && dispatch > claim);
            Assert.IsTrue(Regex.IsMatch(body, @"finally\s*\{[^}]*_isClosingApplication\s*=\s*false;"));
        }

        [TestMethod]
        public void TABRC2B3A02_EachAcceptedCloseResetsTransactionDecisionAndRestoresItAfterward()
        {
            var body = ApplicationCloseSource(ReadMainForm());
            var claim = body.IndexOf("_isClosingApplication = true;", StringComparison.Ordinal);
            var reset = body.IndexOf("MyGlobal.CommitRollbackCheck = -1;", StringComparison.Ordinal);
            var dispatch = body.IndexOf("MyGlobal.GlobalTemp = sb.ToString();", StringComparison.Ordinal);

            Assert.IsTrue(claim >= 0 && reset > claim && dispatch > reset);
            Assert.IsTrue(Regex.IsMatch(body, @"finally\s*\{[^}]*MyGlobal\.CommitRollbackCheck\s*=\s*-1;"));
        }

        [TestMethod]
        public void TABRC2B3A03_ShutdownTargetsAreFormTypesNotDisplayTitles()
        {
            var body = ApplicationCloseSource(ReadMainForm());

            Assert.IsTrue(Regex.IsMatch(body, @"\.Control\s+is\s+QueryForm\b"));
            Assert.IsTrue(Regex.IsMatch(body, @"\.Control\s+is\s+SchemaBrowserForm\b"));
            Assert.IsFalse(Regex.IsMatch(body, @"SpecialTabName|\.Title\b"));
        }

        [TestMethod]
        public void TABRC2B3A04_ConflictingCloseAndFileWatchDispatchAreBlockedDuringShutdownPreflight()
        {
            var source = ReadMainForm();
            var closePages = BetweenMethods(source, "private bool CloseTabPages(", "private bool TryCloseTabPage(");
            var fileWatch = BetweenMethods(source, "private void ProcessSelectedFileWatchCheck()", "private void");
            var autoComplete = BetweenMethods(source, "private void HideAutoCompleteList()", "private void LoadPostgreSqlDatabase()");

            Assert.IsTrue(Regex.IsMatch(closePages, @"_isClosingApplication\s*&&\s*restoreEmptyEditor"));
            Assert.IsTrue(Regex.IsMatch(fileWatch, @"if\s*\([^)]*_isClosingApplication[^)]*\)\s*\{\s*return;"));
            Assert.IsTrue(Regex.IsMatch(autoComplete, @"if\s*\([^)]*_isClosingApplication[^)]*\)\s*\{\s*return;"));
            Assert.IsTrue(source.Contains("CloseTabPages(targets, false)"), "Shutdown's own close operation must remain permitted.");
        }
    }
}
