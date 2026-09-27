using JasonLibrary.Core.Database.Enums;
using JasonLibrary.Core.Text.Formatting;
using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.QueryEngine.Editor.Editing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace JasonQuery.Tests.Core.QueryEngine.Editor.Editing
{
    [TestClass]
    public sealed class SqlFormatterStep5CFlowTests
    {
        [TestMethod]
        [DataRow(SqlFormatterEngineKind.Unknown)]
        [DataRow(SqlFormatterEngineKind.MicrosoftScriptDom)]
        [DataRow(SqlFormatterEngineKind.Hogimn)]
        public void QueryEditor_SqlServerListSetting_MatchesDirectPreviewResult(SqlFormatterEngineKind engineKind)
        {
            const string sql = "SELECT C.A,C.B,C.C,C.D FROM dbo.CUSTOMER C ORDER BY C.A,C.B,C.C,C.D;";

            var options = EditorSqlFormatterOptionsFactory.Create
            (
                4,
                1000,
                1,
                true,
                1,
                2
            );

            var coordinator = new SqlFormatterCoordinator();

            var previewResult = coordinator.Format
            (
                sql,
                DatabaseProviderKind.SqlServer,
                engineKind,
                options
            );

            var editor = new FakeTextEditor
            {
                Text = sql,
                SelectionStart = 0,
                SelectionEnd = sql.Length
            };

            var service = new EditorSqlFormatterService(editor, coordinator.Format);

            var formatted = service.TryFormatSelection
            (
                DataSourceType.SqlServer,
                engineKind,
                options,
                out var errorMessage
            );

            Assert.IsTrue(previewResult.Success, previewResult.ErrorMessage);
            Assert.IsTrue(formatted, errorMessage);
            Assert.AreEqual(previewResult.FormattedSql, editor.Text);
            Assert.AreEqual(0, editor.SelectionStart);
            Assert.AreEqual(editor.Text.Length, editor.SelectionEnd);
            Assert.AreEqual(1, editor.ReplaceSelectionCount);
            Assert.AreEqual(1, editor.ScrollCaretCount);
            Assert.Contains("C.A, C.B,", editor.Text);
            Assert.DoesNotContain("C.A, C.B, C.C", editor.Text);
        }

        [TestMethod]
        public void QueryEditor_ListSettingBoundariesReachBothSqlServerEngines()
        {
            var engineKinds = new[]
            {
                SqlFormatterEngineKind.MicrosoftScriptDom,
                SqlFormatterEngineKind.Hogimn
            };

            foreach (var engineKind in engineKinds)
            {
                var oneItemSql = FormatInEditor(engineKind, 1);
                var tenItemSql = FormatInEditor(engineKind, 10);

                Assert.AreNotEqual(oneItemSql, tenItemSql);
                Assert.DoesNotContain("C.A, C.B", oneItemSql);
                Assert.Contains("C.A, C.B, C.C, C.D", tenItemSql);
            }
        }

        private static string FormatInEditor(SqlFormatterEngineKind engineKind, int listItemsPerLine)
        {
            const string sql = "SELECT C.A,C.B,C.C,C.D FROM dbo.CUSTOMER C ORDER BY C.A,C.B,C.C,C.D;";

            var options = EditorSqlFormatterOptionsFactory.Create
            (
                4,
                1000,
                1,
                true,
                1,
                listItemsPerLine
            );

            var editor = new FakeTextEditor
            {
                Text = sql,
                SelectionStart = 0,
                SelectionEnd = sql.Length
            };

            var coordinator = new SqlFormatterCoordinator();
            var service = new EditorSqlFormatterService(editor, coordinator.Format);

            var formatted = service.TryFormatSelection
            (
                DataSourceType.SqlServer,
                engineKind,
                options,
                out var errorMessage
            );

            Assert.IsTrue(formatted, errorMessage);
            Assert.AreEqual(0, editor.SelectionStart);
            Assert.AreEqual(editor.Text.Length, editor.SelectionEnd);

            return editor.Text;
        }

        private sealed class FakeTextEditor : ITextEditor
        {
            public string Text { get; set; } = string.Empty;

            public string SelectedText
            {
                get
                {
                    var start = Math.Min(SelectionStart, SelectionEnd);
                    var end = Math.Max(SelectionStart, SelectionEnd);

                    return Text.Substring(start, end - start);
                }
            }

            public int SelectionStart { get; set; }

            public int SelectionEnd { get; set; }

            public int CurrentPosition { get; set; }

            public int CurrentLine => 0;

            public object Tag { get; set; }

            public int ReplaceSelectionCount { get; private set; }

            public int ScrollCaretCount { get; private set; }

            public void ReplaceSelection(string text)
            {
                var start = Math.Min(SelectionStart, SelectionEnd);
                var end = Math.Max(SelectionStart, SelectionEnd);

                Text = Text.Substring(0, start) + text + Text.Substring(end);
                SelectionStart = start;
                SelectionEnd = start + text.Length;
                ReplaceSelectionCount++;
            }

            public void Select()
            {
            }

            public void ScrollCaret()
            {
                ScrollCaretCount++;
            }

            public string GetTextRange(int start, int length)
            {
                return Text.Substring(start, length);
            }
        }
    }
}
