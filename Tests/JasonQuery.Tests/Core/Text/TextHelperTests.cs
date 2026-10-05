using JasonQuery.Core.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace JasonQuery.Tests.Core.Text
{
    [TestClass]
    public sealed class TextHelperTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        public void GetSafeSubstring_WithNull_ReturnsEmptyString()
        {
            Assert.AreEqual(string.Empty, TextHelper.GetSafeSubstring(null, 0));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void GetSafeSubstring_WithEmptyString_ReturnsEmptyString()
        {
            Assert.AreEqual(string.Empty, TextHelper.GetSafeSubstring(string.Empty, 0));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void GetSafeSubstring_WithNegativeStart_ReturnsEmptyString()
        {
            Assert.AreEqual(string.Empty, TextHelper.GetSafeSubstring("abcdef", -1));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void GetSafeSubstring_WithStartAtLength_ReturnsEmptyString()
        {
            Assert.AreEqual(string.Empty, TextHelper.GetSafeSubstring("abcdef", 6));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void GetSafeSubstring_WithNegativeLength_ReturnsEmptyString()
        {
            Assert.AreEqual(string.Empty, TextHelper.GetSafeSubstring("abcdef", 1, -1));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void GetSafeSubstring_WithoutLength_ReturnsToEnd()
        {
            Assert.AreEqual("cdef", TextHelper.GetSafeSubstring("abcdef", 2));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void GetSafeSubstring_WithLength_ReturnsRequestedText()
        {
            Assert.AreEqual("cde", TextHelper.GetSafeSubstring("abcdef", 2, 3));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void GetSafeSubstring_WithExcessiveLength_ClampsToEnd()
        {
            Assert.AreEqual("cdef", TextHelper.GetSafeSubstring("abcdef", 2, 100));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void GetSafeSubstring_WithZeroLength_ReturnsEmptyString()
        {
            Assert.AreEqual(string.Empty, TextHelper.GetSafeSubstring("abcdef", 2, 0));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [DataRow("0.90", "0.90.0", 0)]
        [DataRow("0.90.1", "0.90", 1)]
        [DataRow("1.0", "1.0.1", -1)]
        [DataRow("1.10", "1.2", 1)]
        [DataRow("0.91.2", "0.91.10", -1)]
        public void CompareVersions_WithNumericVersions_ReturnsExpectedResult(string version1, string version2, int expected)
        {
            Assert.AreEqual(expected, TextHelper.CompareVersions(version1, version2));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void TruncateDecimal_WithNull_ReturnsNull()
        {
            Assert.IsNull(TextHelper.TruncateDecimal(null, 2));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void TruncateDecimal_WithWhiteSpace_ReturnsOriginalText()
        {
            Assert.AreEqual("   ", TextHelper.TruncateDecimal("   ", 2));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void TruncateDecimal_WithoutDecimalPoint_ReturnsOriginalText()
        {
            Assert.AreEqual("123", TextHelper.TruncateDecimal("123", 2));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void TruncateDecimal_WithNegativeScale_TreatsScaleAsZero()
        {
            Assert.AreEqual("123", TextHelper.TruncateDecimal("123.456", -1));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void TruncateDecimal_WithZeroScale_RemovesFraction()
        {
            Assert.AreEqual("123", TextHelper.TruncateDecimal("123.456", 0));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void TruncateDecimal_WithScale_TruncatesWithoutRounding()
        {
            Assert.AreEqual("123.45", TextHelper.TruncateDecimal("123.456", 2));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void TruncateDecimal_WithScaleLongerThanFraction_ReturnsOriginal()
        {
            Assert.AreEqual("123.4", TextHelper.TruncateDecimal("123.4", 5));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void JoinNonEmpty_WithNullEnumerable_ReturnsEmptyString()
        {
            Assert.AreEqual
            (
                string.Empty,
                TextHelper.JoinNonEmpty
                (
                    ",",
                    (IEnumerable<string>)null
                )
            );
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void JoinNonEmpty_WithOnlyEmptyValues_ReturnsEmptyString()
        {
            Assert.AreEqual
            (
                string.Empty,
                TextHelper.JoinNonEmpty
                (
                    ",",
                    null,
                    string.Empty,
                    "   "
                )
            );
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void JoinNonEmpty_WithMixedValues_IgnoresEmptyEntries()
        {
            Assert.AreEqual
            (
                "a,b,c",
                TextHelper.JoinNonEmpty
                (
                    ",",
                    "a",
                    null,
                    " ",
                    "b",
                    "c"
                )
            );
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void JoinNonEmpty_WithNullSeparator_ConcatenatesValues()
        {
            Assert.AreEqual
            (
                "abc",
                TextHelper.JoinNonEmpty
                (
                    null,
                    "a",
                    "b",
                    "c"
                )
            );
        }

        [TestMethod]
        [TestCategory("Unit")]
        [DataRow(null, false, "")]
        [DataRow("", false, "")]
        [DataRow("(getdate())", false, "getdate()")]
        [DataRow("((1))", false, "1")]
        [DataRow("()", false, "")]
        [DataRow("(1)+(2)", false, "(1)+(2)")]
        [DataRow("(abc", false, "(abc")]
        [DataRow("abc)", false, "abc)")]
        [DataRow("((a)+(b))", false, "(a)+(b)")]
        [DataRow("  ((1))  ", false, "  ((1))  ")]
        [DataRow("  ((1))  ", true, "1")]
        public void RemoveEnclosingParentheses_WithDifferentInputs_ReturnsExpectedText(string source, bool trimOuterWhiteSpace, string expected)
        {
            Assert.AreEqual
            (
                expected,
                TextHelper.RemoveEnclosingParentheses
                (
                    source,
                    trimOuterWhiteSpace
                )
            );
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void GetValueFromDictionary_WithNullDictionary_ReturnsEmptyString()
        {
            Assert.AreEqual
            (
                string.Empty,
                TextHelper.GetValueFromDictionary(null, "A")
            );
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void GetValueFromDictionary_WithEmptyKey_ReturnsEmptyString()
        {
            var values = new Dictionary<string, string>
            {
                { "A", "Alpha" }
            };

            Assert.AreEqual
            (
                string.Empty,
                TextHelper.GetValueFromDictionary(values, string.Empty)
            );
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void GetValueFromDictionary_WithMissingKey_ReturnsEmptyString()
        {
            var values = new Dictionary<string, string>
            {
                { "A", "Alpha" }
            };

            Assert.AreEqual
            (
                string.Empty,
                TextHelper.GetValueFromDictionary(values, "B")
            );
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void GetValueFromDictionary_WithNullValue_ReturnsEmptyString()
        {
            var values = new Dictionary<string, string>
            {
                { "A", null }
            };

            Assert.AreEqual
            (
                string.Empty,
                TextHelper.GetValueFromDictionary(values, "A")
            );
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void GetValueFromDictionary_WithExistingValue_ReturnsValue()
        {
            var values = new Dictionary<string, string>
            {
                { "A", "Alpha" }
            };

            Assert.AreEqual
            (
                "Alpha",
                TextHelper.GetValueFromDictionary(values, "A")
            );
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("SqlLexingMigration")]
        [TestCategory("TextHelperTransferString")]
        [DataRow("select * from t where name = 'MixedCase'", true, false, false)]
        [DataRow("SELECT * FROM T WHERE NAME = 'MixedCase'", false, false, false)]
        [DataRow("select \"MixedCase\" from t", true, false, false)]
        [DataRow("select 'a--b' as x from t", true, false, false)]
        [DataRow("select 'a/*b*/c' as x from t", true, false, false)]
        [DataRow("select \"a--b\" from t", true, false, false)]
        [DataRow("select 'it''s Mixed' as x from t", true, false, false)]
        [DataRow("select \"a\"\"b\" from t", true, false, false)]
        [DataRow("select 1 -- MixedCase\r\nfrom t", true, false, false)]
        [DataRow("select /* MixedCase */ 1 from t", true, false, false)]
        [DataRow("select /* outer /* inner */ tail */ 1", true, false, false)]
        [DataRow("select [MixedCase] from t", true, false, false)]
        [DataRow("select `MixedCase` from t", true, false, false)]
        [DataRow("select $$MixedCase$$ from t", true, false, false)]
        [DataRow("select 1 -- MixedCase\r\nfrom t", true, true, false)]
        [DataRow("with x as (select 1 /* MixedCase */) select * from x", true, true, true)]
        public void GetTransferString_SharedTokenizer_MatchesLegacy(string sql, bool toUppercase, bool isComment, bool isReplace)
        {
            var shared = TextHelper.GetTransferString(toUppercase, sql, isComment, isReplace);
            var legacy = TextHelper.GetTransferStringLegacyForParityTest(toUppercase, sql, isComment, isReplace);

            Assert.AreEqual(legacy, shared);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Regression")]
        [TestCategory("SqlLexingMigration")]
        [TestCategory("TextHelperTransferString")]
        public void GetTransferString_LineCommentContainingUnmatchedQuote_DoesNotPoisonFollowingSql()
        {
            const string sql = "select 1 -- 'MiXeD\r\nwhere Name = 1";

            Assert.AreEqual
            (
                "SELECT 1 -- 'MiXeD\r\nWHERE NAME = 1",
                TextHelper.GetTransferString(true, sql)
            );
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Regression")]
        [TestCategory("SqlLexingMigration")]
        [TestCategory("TextHelperTransferString")]
        public void GetTransferString_BlockCommentContainingUnmatchedQuote_DoesNotPoisonFollowingSql()
        {
            const string sql = "select /* 'MiXeD */ Name from t";

            Assert.AreEqual
            (
                "SELECT /* 'MiXeD */ NAME FROM T",
                TextHelper.GetTransferString(true, sql)
            );
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Regression")]
        [TestCategory("SqlLexingMigration")]
        [TestCategory("TextHelperTransferString")]
        public void GetTransferString_LineCommentContainingBlockCommentOpener_DoesNotPoisonFollowingSql()
        {
            const string sql = "select 1 -- /* MiXeD\r\nfrom TableName";

            Assert.AreEqual
            (
                "SELECT 1 -- /* MiXeD\r\nFROM TABLENAME",
                TextHelper.GetTransferString(true, sql)
            );
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Regression")]
        [TestCategory("SqlLexingMigration")]
        [TestCategory("TextHelperTransferString")]
        public void GetTransferString_BlockCommentContainingLineCommentMarker_DoesNotPoisonFollowingSql()
        {
            const string sql = "select /* -- MiXeD */ ColumnName from t";

            Assert.AreEqual
            (
                "SELECT /* -- MiXeD */ COLUMNNAME FROM T",
                TextHelper.GetTransferString(true, sql)
            );
        }

    }
}
