using JasonQuery.Core.QueryEngine.Editor.AutoComplete.Support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace JasonQuery.Tests.Core.QueryEngine.Editor.AutoComplete
{
    [TestClass]
    public sealed class QueryEditorAutoCompleteTriggerPolicyTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        [DataRow("select * from table1 a where a.", "a.")]
        [DataRow("select * from table1 a where a.column1", "a.")]
        [DataRow("select schema1.table1", "schema1.")]
        [DataRow("select (a).column1", "a).")]
        [DataRow("select a.\r\ncolumn1", "a.")]
        public void IsPeriodTriggerAllowed_WithCodePeriod_ReturnsTrue(string sql, string periodAnchor)
        {
            var periodIndex = sql.IndexOf(periodAnchor, StringComparison.Ordinal) + periodAnchor.LastIndexOf('.');

            Assert.IsTrue(QueryEditorAutoCompleteTriggerPolicy.IsPeriodTriggerAllowed(sql, periodIndex + 1));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        [DataRow("select 'a.b' from table1", "a.b")]
        [DataRow("select 'a''b.c' from table1", "b.c")]
        [DataRow("select \"a.b\" from table1", "a.b")]
        [DataRow("select [a.b] from table1", "a.b")]
        [DataRow("select `a.b` from table1", "a.b")]
        [DataRow("select 1 -- a.b\r\nfrom table1", "a.b")]
        [DataRow("select 1 # a.b\r\nfrom table1", "a.b")]
        [DataRow("select /* a.b */ 1 from table1", "a.b")]
        [DataRow("select 1.25 from table1", "1.")]
        [DataRow("select 1. from table1", "1.")]
        [DataRow("select .5 from table1", ".5")]
        public void IsPeriodTriggerAllowed_InsideNonCodeOrNumericLiteral_ReturnsFalse(string sql, string periodAnchor)
        {
            var periodIndex = sql.IndexOf(periodAnchor, StringComparison.Ordinal) + periodAnchor.LastIndexOf('.');

            Assert.IsFalse(QueryEditorAutoCompleteTriggerPolicy.IsPeriodTriggerAllowed(sql, periodIndex + 1));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        [DataRow(null, 0)]
        [DataRow("", 0)]
        [DataRow("select a", 0)]
        [DataRow("select a", 100)]
        [DataRow("select a", -1)]
        public void IsPeriodTriggerAllowed_WithInvalidInput_ReturnsFalse(string sql, int periodPosition)
        {
            Assert.IsFalse(QueryEditorAutoCompleteTriggerPolicy.IsPeriodTriggerAllowed(sql, periodPosition));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        [DataRow("select a.column1", "a.")]
        [DataRow("select 1\r\nfrom table1 a\r\nwhere a.column1 = 1", "a.")]
        [DataRow("select /* x */ a.column1", "a.")]
        [DataRow("select 'x' || a.column1", "a.")]
        public void IsCodePosition_WithCodePeriod_ReturnsTrue(string sql, string anchor)
        {
            var position = sql.IndexOf(anchor, StringComparison.Ordinal) + anchor.LastIndexOf('.');

            Assert.IsTrue(QueryEditorAutoCompleteTriggerPolicy.IsCodePosition(sql, position));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        [DataRow("select 'a.b'", "a.")]
        [DataRow("select \"a.b\"", "a.")]
        [DataRow("select [a.b]", "a.")]
        [DataRow("select `a.b`", "a.")]
        [DataRow("select -- a.b\r\n1", "a.")]
        [DataRow("select # a.b\r\n1", "a.")]
        [DataRow("select /* a.b */ 1", "a.")]
        public void IsCodePosition_InsideQuotedTextOrComment_ReturnsFalse(string sql, string anchor)
        {
            var position = sql.IndexOf(anchor, StringComparison.Ordinal) + anchor.LastIndexOf('.');

            Assert.IsFalse(QueryEditorAutoCompleteTriggerPolicy.IsCodePosition(sql, position));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        [DataRow("select a, b", " b")]
        [DataRow("select a,\r\nb", "\r\nb")]
        [DataRow("select a, \"Column Name\"", " \"")]
        [DataRow("select a, `column_name`", " `")]
        [DataRow("select a, [Column Name]", " [")]
        [DataRow("select a, 'text'", " '")]
        [DataRow("select a, function1()", " f")]
        [DataRow("select a, :parameter1", " :")]
        [DataRow("select a, @parameter1", " @")]
        [DataRow("select a, ?parameter1", " ?")]
        public void ShouldSuppressSpaceAfterComma_WithExistingNextToken_ReturnsTrue(string sql, string caretAnchor)
        {
            var caretPosition = sql.IndexOf(caretAnchor, StringComparison.Ordinal) + 1;

            Assert.IsTrue(QueryEditorAutoCompleteTriggerPolicy.ShouldSuppressSpaceAfterComma(sql, caretPosition, false));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        [DataRow("select a, ", false)]
        [DataRow("select a b", false)]
        [DataRow("select 'a, b'", false)]
        [DataRow("select /* a, b */ 1", false)]
        [DataRow("select a, )", false)]
        [DataRow("select a, ;", false)]
        [DataRow("select a, ,", false)]
        [DataRow("select a, b", true)]
        public void ShouldSuppressSpaceAfterComma_WhenSuppressionDoesNotApply_ReturnsFalse(string sql, bool isAnyPopupVisible)
        {
            var caretPosition = ResolveCaretPosition(sql);

            Assert.IsFalse(QueryEditorAutoCompleteTriggerPolicy.ShouldSuppressSpaceAfterComma(sql, caretPosition, isAnyPopupVisible));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        [DataRow(null, 0)]
        [DataRow("", 0)]
        [DataRow("select a, b", -1)]
        [DataRow("select a, b", 100)]
        public void ShouldSuppressSpaceAfterComma_WithInvalidInput_ReturnsFalse(string sql, int caretPosition)
        {
            Assert.IsFalse(QueryEditorAutoCompleteTriggerPolicy.ShouldSuppressSpaceAfterComma(sql, caretPosition, false));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        [TestCategory("SqlLexingMigration")]
        [TestCategory("AutoCompleteTriggerPolicy")]
        [DataRow("a.b", 1, true)]
        [DataRow("'a'.", 0, true)]
        [DataRow("'a'.", 1, false)]
        [DataRow("'a'.", 2, false)]
        [DataRow("'a'.", 3, true)]
        [DataRow("'a''b'.", 3, true)]
        [DataRow("'a''b'.", 4, false)]
        [DataRow("'a''b'.", 6, true)]
        [DataRow("\"a\"\"b\".", 3, true)]
        [DataRow("\"a\"\"b\".", 4, false)]
        [DataRow("[a]]b].", 3, true)]
        [DataRow("[a]]b].", 4, false)]
        [DataRow("`a``b`.", 3, true)]
        [DataRow("`a``b`.", 4, false)]
        [DataRow("--x\r\n.", 1, true)]
        [DataRow("--x\r\n.", 2, false)]
        [DataRow("--x\r\n.", 4, true)]
        [DataRow("abc#x\n.", 4, false)]
        [DataRow("/*x*/.", 4, false)]
        [DataRow("/*x*/.", 5, true)]
        public void IsCodePosition_SharedTokenizer_MatchesGolden(string text, int position, bool expected)
        {
            Assert.AreEqual(expected, QueryEditorAutoCompleteTriggerPolicy.IsCodePosition(text, position));
        }

        private static int ResolveCaretPosition(string sql)
        {
            if (string.IsNullOrEmpty(sql))
            {
                return 0;
            }

            var commaIndex = sql.IndexOf(',');

            if (commaIndex < 0)
            {
                return sql.Length;
            }

            var position = commaIndex + 1;

            while (position < sql.Length && char.IsWhiteSpace(sql[position]))
            {
                position++;
            }

            return position;
        }
    }
}
