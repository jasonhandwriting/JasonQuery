using System;
using System.Linq;
using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.QueryEngine.Editor.Editing;
using JasonQuery.Core.Sql.Lexing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonQuery.Tests.Core.QueryEngine.Editor.Editing
{
    //v0.97 Improvement #6: pre-implementation Golden Tests.
    //Existing behavior tests must PASS on the frozen main baseline.
    //New statement-selection acceptance tests remain explicitly IGNORED
    //until the statement-aware implementation is reviewed and enabled.
    [TestClass]
    public sealed class EditorStatementSelectionGoldenTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("V097StatementSelectionLegacyGolden")]
        [DataRow("SELECT 1;\r\n\r\nSELECT 2;", "SELECT 2;", "SELECT 2;")]
        [DataRow("SELECT 1;\r\n    \r\nSELECT 2;", "SELECT 2;", "SELECT 2;")]
        [DataRow("SELECT 1;\r\n\r\nSELECT 2;\r\n\r\nSELECT 3;", "SELECT 2;", "SELECT 2;")]
        [DataRow("SELECT 1;", "SELECT 1;", "SELECT 1;")]
        [DataRow("SELECT 1\r\nSELECT 2", "SELECT 2", "SELECT 1\r\nSELECT 2")]
        [DataRow("SELECT 1;\r\nSELECT 2;\r\nSELECT 3;", "SELECT 2;", "SELECT 1;\r\nSELECT 2;\r\nSELECT 3;")]
        public void LegacyBlockSelection_Regression(string sql, string caretNeedle, string expectedText)
        {
            expectedText = UnescapeEditorText(expectedText);

            var editor = CreateEditor(sql, caretNeedle);
            var service = new EditorBlockSelectionService(editor);
            var beforeCaret = editor.CurrentPosition;
            var beforeStart = editor.SelectionStart;
            var beforeEnd = editor.SelectionEnd;

            Assert.AreEqual(expectedText, service.SelectCurrentBlock(false));
            Assert.AreEqual(beforeCaret, editor.CurrentPosition, "Read-only selection must not move caret.");
            Assert.AreEqual(beforeStart, editor.SelectionStart, "Read-only selection must not change anchor.");
            Assert.AreEqual(beforeEnd, editor.SelectionEnd, "Read-only selection must not change selection end.");
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("V097StatementSelectionLegacyGolden")]
        public void EmptyEditor_ReturnsEmptyWithoutChangingSelection()
        {
            var editor = new FakeTextEditor { Text = string.Empty };
            var service = new EditorBlockSelectionService(editor);

            Assert.AreEqual(string.Empty, service.SelectCurrentBlock(false));
            Assert.AreEqual(string.Empty, service.SelectCurrentBlock(true));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("V097StatementSelectionLegacyGolden")]
        [DataRow(DataSourceType.Oracle, "SELECT ';' AS v; SELECT 2;", 2)]
        [DataRow(DataSourceType.PostgreSql, "SELECT 'a'';b' AS v; SELECT 2;", 2)]
        [DataRow(DataSourceType.SqlServer, "SELECT [a;b] FROM t; SELECT 2;", 2)]
        [DataRow(DataSourceType.SqlServer, "SELECT \"a;b\" FROM t; SELECT 2;", 2)]
        [DataRow(DataSourceType.MySql, "SELECT `a;b` FROM t; SELECT 2;", 2)]
        [DataRow(DataSourceType.MySql, "SELECT 'a\\';b' AS v; SELECT 2;", 2)]
        [DataRow(DataSourceType.PostgreSql, "SELECT $$a;b$$; SELECT 2;", 2)]
        [DataRow(DataSourceType.PostgreSql, "SELECT $tag$a;b$tag$; SELECT 2;", 2)]
        [DataRow(DataSourceType.Oracle, "SELECT q'[a;b]' FROM dual; SELECT 2;", 2)]
        [DataRow(DataSourceType.Oracle, "SELECT 1 -- ;\r\n; SELECT 2;", 2)]
        [DataRow(DataSourceType.MySql, "SELECT 1 -- ;\r\n; SELECT 2;", 2)]
        [DataRow(DataSourceType.MySql, "SELECT 1 #;\r\n; SELECT 2;", 2)]
        [DataRow(DataSourceType.PostgreSql, "SELECT 1 /* ; */; SELECT 2;", 2)]
        [DataRow(DataSourceType.PostgreSql, "SELECT 1 /* outer /* inner ; */ outer ; */; SELECT 2;", 2)]
        [DataRow(DataSourceType.PostgreSql, "SELECT (1; 2) FROM t; SELECT 2;", 2)]
        [DataRow(DataSourceType.PostgreSql, "SELECT 'unclosed ;", 0)]
        [DataRow(DataSourceType.SqlServer, "SELECT /* unclosed ;", 0)]
        public void Tokenizer_TopLevelSemicolons_AreLexicallySeparated(DataSourceType dataSourceType, string sql, int expected)
        {
            //Deliberately not used as a complete SQL statement parser.
            sql = UnescapeEditorText(sql);

            var tokens = SqlTokenizer.Tokenize(sql, dataSourceType).Tokens;
            var semicolons = tokens.Count(token => token.IsSymbol(";") && token.Depth == 0);

            Assert.AreEqual(expected, semicolons);
        }

        [TestMethod]
        [TestCategory("V097StatementSelectionAcceptanceGolden")]
        [DataRow("SELECT 1;\r\nSELECT 2;\r\nSELECT 3;", "SELECT 2;", "SELECT 2;")]
        [DataRow("SELECT 1; SELECT 2; SELECT 3;", "SELECT 2;", "SELECT 2;")]
        [DataRow("SELECT ';' AS value; SELECT 2;", "SELECT 2;", "SELECT 2;")]
        [DataRow("SELECT 1 /* ; */; SELECT 2;", "SELECT 2;", "SELECT 2;")]
        [DataRow("SELECT 1;\r\n    \r\nSELECT 2;", "SELECT 2;", "SELECT 2;")]
        public void Pending_SemicolonStatementSelection_ResolvesCaretStatement(string sql, string caretNeedle, string expected)
        {
            var editor = CreateEditor(UnescapeEditorText(sql), caretNeedle);
            var service = new EditorBlockSelectionService(editor);

            service.SelectCurrentStatement(DataSourceType.PostgreSql);
            Assert.AreEqual(expected, SelectedInteractiveText(editor).Trim());
        }

        [TestMethod]
        [TestCategory("V097StatementSelectionAcceptanceGolden")]
        public void Pending_AutoCompleteBlock_UsesLegacyRangeWhileInteractiveSelectionUsesStatementRange()
        {
            const string sql = "SELECT 1; SELECT 2;";

            var editor = CreateEditor(sql, "SELECT 2;");
            var service = new EditorBlockSelectionService(editor);
            var legacy = service.SelectCurrentBlock(false);

            Assert.AreEqual(sql, legacy, "AutoComplete must use the original blank-row block.");
            service.SelectCurrentStatement(DataSourceType.PostgreSql);
            Assert.AreEqual("SELECT 2;", SelectedInteractiveText(editor), "Interactive Ctrl+Enter must be statement-aware.");
        }


        //All 19 approved SS-001..SS-019 contract IDs are executable. The
        //"All" provider cases are expanded into four independent dialect runs.
        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("V097StatementSelectionAcceptanceGolden")]
        [DataRow("SS-001", DataSourceType.Oracle, "SELECT 1;\r\nSELECT 2;\r\nSELECT 3;", "SELECT 2;", "SELECT 2;")]
        [DataRow("SS-001", DataSourceType.PostgreSql, "SELECT 1;\r\nSELECT 2;\r\nSELECT 3;", "SELECT 2;", "SELECT 2;")]
        [DataRow("SS-001", DataSourceType.SqlServer, "SELECT 1;\r\nSELECT 2;\r\nSELECT 3;", "SELECT 2;", "SELECT 2;")]
        [DataRow("SS-001", DataSourceType.MySql, "SELECT 1;\r\nSELECT 2;\r\nSELECT 3;", "SELECT 2;", "SELECT 2;")]
        [DataRow("SS-002", DataSourceType.Oracle, "SELECT 1; SELECT 2; SELECT 3;", "SELECT 2;", "SELECT 2;")]
        [DataRow("SS-002", DataSourceType.PostgreSql, "SELECT 1; SELECT 2; SELECT 3;", "SELECT 2;", "SELECT 2;")]
        [DataRow("SS-002", DataSourceType.SqlServer, "SELECT 1; SELECT 2; SELECT 3;", "SELECT 2;", "SELECT 2;")]
        [DataRow("SS-002", DataSourceType.MySql, "SELECT 1; SELECT 2; SELECT 3;", "SELECT 2;", "SELECT 2;")]
        [DataRow("SS-003", DataSourceType.Oracle, "SELECT ';' AS value; SELECT 2;", "SELECT 2;", "SELECT 2;")]
        [DataRow("SS-003", DataSourceType.PostgreSql, "SELECT ';' AS value; SELECT 2;", "SELECT 2;", "SELECT 2;")]
        [DataRow("SS-003", DataSourceType.SqlServer, "SELECT ';' AS value; SELECT 2;", "SELECT 2;", "SELECT 2;")]
        [DataRow("SS-003", DataSourceType.MySql, "SELECT ';' AS value; SELECT 2;", "SELECT 2;", "SELECT 2;")]
        [DataRow("SS-004", DataSourceType.Oracle, "SELECT 1 /* ; */; SELECT 2;", "SELECT 2;", "SELECT 2;")]
        [DataRow("SS-004", DataSourceType.PostgreSql, "SELECT 1 /* ; */; SELECT 2;", "SELECT 2;", "SELECT 2;")]
        [DataRow("SS-004", DataSourceType.SqlServer, "SELECT 1 /* ; */; SELECT 2;", "SELECT 2;", "SELECT 2;")]
        [DataRow("SS-004", DataSourceType.MySql, "SELECT 1 /* ; */; SELECT 2;", "SELECT 2;", "SELECT 2;")]
        [DataRow("SS-005", DataSourceType.PostgreSql, "SELECT $$a;b$$; SELECT 2;", "SELECT 2;", "SELECT 2;")]
        [DataRow("SS-006", DataSourceType.PostgreSql, "SELECT $tag$a;b$tag$; SELECT 2;", "SELECT 2;", "SELECT 2;")]
        [DataRow("SS-007", DataSourceType.Oracle, "SELECT q'[a;b]' FROM dual; SELECT 2;", "SELECT 2;", "SELECT 2;")]
        [DataRow("SS-008", DataSourceType.MySql, "SELECT `a;b` FROM t; SELECT 2;", "SELECT 2;", "SELECT 2;")]
        [DataRow("SS-009", DataSourceType.MySql, "SELECT 1 -- ;\r\n; SELECT 2;", "SELECT 2;", "SELECT 2;")]
        [DataRow("SS-010", DataSourceType.MySql, "SELECT 1 #;\r\n; SELECT 2;", "SELECT 2;", "SELECT 2;")]
        [DataRow("SS-011", DataSourceType.SqlServer, "SELECT [a;b] FROM t; SELECT 2;", "SELECT 2;", "SELECT 2;")]
        [DataRow("SS-012", DataSourceType.Oracle, "SELECT 1;\r\n    \r\nSELECT 2;", "SELECT 2;", "SELECT 2;")]
        [DataRow("SS-012", DataSourceType.PostgreSql, "SELECT 1;\r\n    \r\nSELECT 2;", "SELECT 2;", "SELECT 2;")]
        [DataRow("SS-012", DataSourceType.SqlServer, "SELECT 1;\r\n    \r\nSELECT 2;", "SELECT 2;", "SELECT 2;")]
        [DataRow("SS-012", DataSourceType.MySql, "SELECT 1;\r\n    \r\nSELECT 2;", "SELECT 2;", "SELECT 2;")]
        [DataRow("SS-013", DataSourceType.Oracle, "BEGIN\r\n  UPDATE t SET a=1;\r\n  UPDATE t SET b=2;\r\nEND;\r\n/", "UPDATE t SET b", "BEGIN\r\n  UPDATE t SET a=1;\r\n  UPDATE t SET b=2;\r\nEND;\r\n/")]
        [DataRow("SS-014", DataSourceType.SqlServer, "BEGIN\r\n  UPDATE t SET a=1;\r\n  UPDATE t SET b=2;\r\nEND;", "UPDATE t SET b", "BEGIN\r\n  UPDATE t SET a=1;\r\n  UPDATE t SET b=2;\r\nEND;")]
        [DataRow("SS-015", DataSourceType.MySql, "DELIMITER $$\r\nCREATE PROCEDURE p()\r\nBEGIN\r\n SELECT 1;\r\nEND$$\r\nDELIMITER ;", "SELECT 1;", "DELIMITER $$\r\nCREATE PROCEDURE p()\r\nBEGIN\r\n SELECT 1;\r\nEND$$\r\nDELIMITER ;")]
        [DataRow("SS-016", DataSourceType.PostgreSql, "SELECT 'unterminated ;\r\nSELECT 2;", "SELECT 2;", "SELECT 'unterminated ;\r\nSELECT 2;")]
        [DataRow("SS-017", DataSourceType.Oracle, "SELECT 1;\r\n    \r\nSELECT 2;", "SELECT 2;", "SELECT 2;")]
        [DataRow("SS-017", DataSourceType.PostgreSql, "SELECT 1;\r\n    \r\nSELECT 2;", "SELECT 2;", "SELECT 2;")]
        [DataRow("SS-017", DataSourceType.SqlServer, "SELECT 1;\r\n    \r\nSELECT 2;", "SELECT 2;", "SELECT 2;")]
        [DataRow("SS-017", DataSourceType.MySql, "SELECT 1;\r\n    \r\nSELECT 2;", "SELECT 2;", "SELECT 2;")]
        [DataRow("SS-018", DataSourceType.Oracle, "SELECT 1\r\nSELECT 2", "SELECT 2", "SELECT 1\r\nSELECT 2")]
        [DataRow("SS-018", DataSourceType.PostgreSql, "SELECT 1\r\nSELECT 2", "SELECT 2", "SELECT 1\r\nSELECT 2")]
        [DataRow("SS-018", DataSourceType.SqlServer, "SELECT 1\r\nSELECT 2", "SELECT 2", "SELECT 1\r\nSELECT 2")]
        [DataRow("SS-018", DataSourceType.MySql, "SELECT 1\r\nSELECT 2", "SELECT 2", "SELECT 1\r\nSELECT 2")]
        [DataRow("SS-019", DataSourceType.Oracle, "SELECT 1; SELECT 2;", "SELECT 2;", "SELECT 1; SELECT 2;")]
        [DataRow("SS-019", DataSourceType.PostgreSql, "SELECT 1; SELECT 2;", "SELECT 2;", "SELECT 1; SELECT 2;")]
        [DataRow("SS-019", DataSourceType.SqlServer, "SELECT 1; SELECT 2;", "SELECT 2;", "SELECT 1; SELECT 2;")]
        [DataRow("SS-019", DataSourceType.MySql, "SELECT 1; SELECT 2;", "SELECT 2;", "SELECT 1; SELECT 2;")]
        public void ApprovedStatementSelectionContract(string caseId, DataSourceType dialect, string sql, string caretNeedle, string expected)
        {
            var editor = CreateEditor(sql, caretNeedle);
            var service = new EditorBlockSelectionService(editor);
            var originalText = editor.Text;
            var originalLegacy = service.SelectCurrentBlock(false);

            if (caseId == "SS-019")
            {
                Assert.AreEqual(expected, originalLegacy, "Legacy AutoComplete contract " + caseId);
                service.SelectCurrentStatement(dialect);
                Assert.AreEqual("SELECT 2;", SelectedInteractiveText(editor), "Interactive counterpart " + caseId);

                return;
            }

            service.SelectCurrentStatement(dialect);
            Assert.AreEqual(expected, SelectedInteractiveText(editor), "Golden case " + caseId + "/" + dialect);
            Assert.AreEqual(originalText, editor.Text, "Statement selection must never modify SQL text.");
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("V097StatementSelectionAcceptanceGolden")]
        public void Selection_OnTheSemicolon_BelongsToPrecedingStatement()
        {
            const string text = "SELECT 1; SELECT 2;";

            var editor = CreateEditor(text, "SELECT 1;");

            editor.CurrentPosition = text.IndexOf(';');
            editor.SelectionStart = editor.CurrentPosition;
            editor.SelectionEnd = editor.CurrentPosition;
            new EditorBlockSelectionService(editor).SelectCurrentStatement(DataSourceType.PostgreSql);
            Assert.AreEqual("SELECT 1;", SelectedInteractiveText(editor));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("V097StatementSelectionAcceptanceGolden")]
        public void Selection_WhenWhitespaceFollowsFinalSemicolon_SelectsFinalStatement()
        {
            const string text = "SELECT 1; SELECT 2;   ";

            var editor = CreateEditor(text, "SELECT 2;");

            editor.CurrentPosition = text.Length;
            editor.SelectionStart = editor.CurrentPosition;
            editor.SelectionEnd = editor.CurrentPosition;
            new EditorBlockSelectionService(editor).SelectCurrentStatement(DataSourceType.SqlServer);
            Assert.AreEqual("SELECT 2;", SelectedInteractiveText(editor));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("V097StatementSelectionAcceptanceGolden")]
        public void Selection_UnknownStatementAfterSemicolon_FallsBackToOriginalBlock()
        {
            const string text = "SELECT 1; DROP TABLE t;";

            var editor = CreateEditor(text, "DROP");

            new EditorBlockSelectionService(editor).SelectCurrentStatement(DataSourceType.SqlServer);
            Assert.AreEqual(text, SelectedInteractiveText(editor));
        }

        private static string SelectedInteractiveText(FakeTextEditor editor)
        {
            var start = Math.Min(editor.SelectionStart, editor.CurrentPosition);
            var end = Math.Max(editor.SelectionStart, editor.CurrentPosition);

            return editor.GetTextRange(start, end - start);
        }

        private static string UnescapeEditorText(string sql)
        {
            return (sql ?? string.Empty).Replace("\\r\\n", "\r\n");
        }

        private static FakeTextEditor CreateEditor(string sql, string caretNeedle)
        {
            var index = sql.IndexOf(caretNeedle, StringComparison.Ordinal);

            Assert.IsTrue(index >= 0, "Missing caret needle: " + caretNeedle);

            return new FakeTextEditor
            {
                Text = sql,
                CurrentPosition = index,
                SelectionStart = index,
                SelectionEnd = index
            };
        }

        private sealed class FakeTextEditor : ITextEditor
        {
            public string Text { get; set; } = string.Empty;
            public int SelectionStart { get; set; }
            public int SelectionEnd { get; set; }
            public int CurrentPosition { get; set; }
            public object Tag { get; set; }

            public int CurrentLine
            {
                get
                {
                    var count = 0;
                    var limit = Math.Min(Math.Max(CurrentPosition, 0), Text.Length);

                    for (var i = 0; i < limit; i++)
                    {
                        if (Text[i] == '\n')
                        {
                            count++;
                        }
                    }

                    return count;
                }
            }

            public string SelectedText
            {
                get
                {
                    var start = Math.Min(SelectionStart, SelectionEnd);
                    var end = Math.Max(SelectionStart, SelectionEnd);

                    return Text.Substring(start, end - start);
                }
            }

            public void ReplaceSelection(string text)
            {
                var start = Math.Min(SelectionStart, SelectionEnd);
                var end = Math.Max(SelectionStart, SelectionEnd);

                Text = Text.Substring(0, start) + text + Text.Substring(end);
            }

            public void Select() { }
            public void ScrollCaret() { }
            public string GetTextRange(int start, int length) => Text.Substring(start, length);
        }
    }
}
