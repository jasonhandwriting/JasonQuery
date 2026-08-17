using JasonQuery.Core.QueryEngine.Editor.AutoComplete.Resolvers;
using JasonQuery.Core.QueryEngine.Editor.Editing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace JasonQuery.Tests.Core.QueryEngine.Editor.AutoComplete
{
    [TestClass]
    public sealed class QueryEditorCtrlJPeriodResolverTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        [DataRow("select * from table1 a where a.", -1, "")]
        [DataRow("select * from table1 a where a.column1", -1, "column1")]
        [DataRow("select * from table1 a where a.column1 = 1", 34, "column1")]
        [DataRow("select a.column1, b.column2", -1, "column2")]
        [DataRow("select schema1.table1", -1, "table1")]
        public void Resolve_WithValidPeriodContext_ReturnsExpectedTrigger(string text, int currentPosition, string expectedKeyword)
        {
            var editor = new FakeTextEditor
            {
                Text = text,
                CurrentPosition = currentPosition == -2 ? text.IndexOf('.') + 2 : currentPosition < 0 ? text.Length : currentPosition
            };

            var resolver = new QueryEditorCtrlJPeriodResolver(editor);
            var result = resolver.Resolve(false, false);
            var expectedPeriodPosition = FindNearestPeriodPosition(text, editor.CurrentPosition);

            Assert.IsTrue(result.CanTrigger);
            Assert.AreEqual(expectedPeriodPosition, result.PeriodPosition);
            Assert.AreEqual(expectedKeyword, result.Keyword);
            Assert.AreEqual(!string.IsNullOrEmpty(expectedKeyword), result.HasKeyword);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        public void Resolve_WhenCaretIsOnPeriod_ReturnsPositionAfterPeriod()
        {
            const string text = "select * from table1 a where a.column1";
            var periodIndex = text.IndexOf(".", StringComparison.Ordinal);

            var editor = new FakeTextEditor
            {
                Text = text,
                CurrentPosition = periodIndex
            };

            var result = new QueryEditorCtrlJPeriodResolver(editor).Resolve(false, false);

            Assert.IsTrue(result.CanTrigger);
            Assert.AreEqual(periodIndex + 1, result.PeriodPosition);
            Assert.AreEqual("column1", result.Keyword);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        public void Resolve_WhenCaretIsInKeyword_ReturnsKeywordStartAfterPeriod()
        {
            const string text = "select * from table1 a where a.column_name";
            var periodPosition = text.IndexOf(".", StringComparison.Ordinal) + 1;

            var editor = new FakeTextEditor
            {
                Text = text,
                CurrentPosition = text.IndexOf("name", StringComparison.Ordinal)
            };

            var result = new QueryEditorCtrlJPeriodResolver(editor).Resolve(false, false);

            Assert.IsTrue(result.CanTrigger);
            Assert.AreEqual(periodPosition, result.PeriodPosition);
            Assert.AreEqual("column_name", result.Keyword);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        [DataRow("select 'a.b' from table1", -2)]
        [DataRow("select 'a''b.c' from table1", -2)]
        [DataRow("select \"a.b\" from table1", -2)]
        [DataRow("select [a.b] from table1", -2)]
        [DataRow("select `a.b` from table1", -2)]
        [DataRow("select 1 -- a.b", -2)]
        [DataRow("select 1 # a.b", -2)]
        [DataRow("select /* a.b */ 1", 13)]
        [DataRow("select 1.25", -1)]
        [DataRow("select 1.", -1)]
        [DataRow("select .5", -1)]
        public void Resolve_InsideQuotedTextCommentOrNumber_CannotTrigger(string text, int currentPosition)
        {
            var editor = new FakeTextEditor
            {
                Text = text,
                CurrentPosition = currentPosition == -2 ? text.IndexOf('.') + 2 : currentPosition < 0 ? text.Length : currentPosition
            };

            var result = new QueryEditorCtrlJPeriodResolver(editor).Resolve(false, false);

            Assert.IsFalse(result.CanTrigger);
            Assert.AreEqual(0, result.PeriodPosition);
            Assert.AreEqual(string.Empty, result.Keyword);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        [DataRow(null, 0)]
        [DataRow("", 0)]
        [DataRow("   ", 3)]
        [DataRow("select column1", -1)]
        [DataRow("select a.column1 + value1", -1)]
        [DataRow("select a.column1\r\nwhere value1 = 1", -1)]
        public void Resolve_WithoutUsablePeriodContext_CannotTrigger(string text, int currentPosition)
        {
            var editor = new FakeTextEditor
            {
                Text = text,
                CurrentPosition = currentPosition < 0 ? (text ?? string.Empty).Length : currentPosition
            };

            var result = new QueryEditorCtrlJPeriodResolver(editor).Resolve(false, false);

            Assert.IsFalse(result.CanTrigger);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        public void Resolve_WithCurrentPositionBeyondTextLength_UsesSafeTextEnd()
        {
            const string text = "select * from table1 a where a.column1";

            var editor = new FakeTextEditor
            {
                Text = text,
                CurrentPosition = text.Length + 100
            };

            var result = new QueryEditorCtrlJPeriodResolver(editor).Resolve(false, false);

            Assert.IsTrue(result.CanTrigger);
            Assert.AreEqual("column1", result.Keyword);
        }

        private static int FindNearestPeriodPosition(string text, int currentPosition)
        {
            var safePosition = Math.Min(Math.Max(currentPosition, 0), text.Length);
            var searchStart = Math.Min(safePosition - 1, text.Length - 1);
            var periodIndex = text.LastIndexOf('.', searchStart);

            return periodIndex + 1;
        }

        private sealed class FakeTextEditor : ITextEditor
        {
            public string Text { get; set; }

            public string SelectedText
            {
                get
                {
                    var start = Math.Min(SelectionStart, SelectionEnd);
                    var end = Math.Max(SelectionStart, SelectionEnd);

                    if (string.IsNullOrEmpty(Text) || start < 0 || end > Text.Length || end <= start)
                    {
                        return string.Empty;
                    }

                    return Text.Substring(start, end - start);
                }
            }

            public int SelectionStart { get; set; }

            public int SelectionEnd { get; set; }

            public int CurrentPosition { get; set; }

            public int CurrentLine
            {
                get { return 0; }
            }

            public object Tag { get; set; }

            public void ReplaceSelection(string text)
            {
                var source = Text ?? string.Empty;
                var start = Math.Min(SelectionStart, SelectionEnd);
                var end = Math.Max(SelectionStart, SelectionEnd);

                Text = source.Substring(0, start) + (text ?? string.Empty) + source.Substring(end);
            }

            public void Select()
            {
            }

            public void ScrollCaret()
            {
            }

            public string GetTextRange(int start, int length)
            {
                return (Text ?? string.Empty).Substring(start, length);
            }
        }
    }
}