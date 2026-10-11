using JasonQuery.Core.Database.Diagnostics.PostgreSql;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace JasonQuery.Tests.Core.Database.Diagnostics.PostgreSql
{
    [TestClass]
    public sealed class PostgreSqlSqlScannerTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void NormalizeSql_WithCommentsAndTerminator_RemovesCommentsAndSemicolon()
        {
            var sql = "select 1 /* block comment */\r\n" +
                      "-- line comment\r\n" +
                      ";";

            var actual = PostgreSqlSqlScanner.NormalizeSql(sql);

            Assert.AreEqual("select 1", actual);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void NormalizeSql_WithCommentMarkersInsideString_PreservesStringContent()
        {
            var sql = "select '--not comment', '/*not comment*/';";
            var actual = PostgreSqlSqlScanner.NormalizeSql(sql);

            Assert.AreEqual("select '--not comment', '/*not comment*/'", actual);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void NormalizeSql_WithCommentMarkersInsideQuotedIdentifier_PreservesIdentifier()
        {
            var sql = "select \"--Column\", \"/*Column*/\";";
            var actual = PostgreSqlSqlScanner.NormalizeSql(sql);

            Assert.AreEqual("select \"--Column\", \"/*Column*/\"", actual);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void ReadKeyword_WithDifferentCaseAndLeadingWhitespace_ReturnsTrueAndAdvancesIndex()
        {
            var text = "   UpDaTe public.a_test";
            var index = 0;
            var success = PostgreSqlSqlScanner.ReadKeyword(text, ref index, "update");

            Assert.IsTrue(success);
            Assert.AreEqual(9, index);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void ReadKeyword_WhenKeywordIsIdentifierPrefix_ReturnsFalse()
        {
            var text = "update_data";
            var index = 0;
            var success = PostgreSqlSqlScanner.ReadKeyword(text, ref index, "update");

            Assert.IsFalse(success);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void FindKeywordAtTopLevel_IgnoresKeywordsInsideStringsAndFunctions()
        {
            var text = "update a_test " +
                       "set t1 = 'where', t2 = replace('set', 's', 'x') " +
                       "where id = 1";

            var actual = PostgreSqlSqlScanner.FindKeywordAtTopLevel(text, "where", 0);

            Assert.AreEqual(text.IndexOf("where id"), actual);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void FindKeywordAtTopLevel_IgnoresKeywordInsideQuotedIdentifier()
        {
            var text = "select \"where\" from public.a_test where t1 = 1";
            var actual = PostgreSqlSqlScanner.FindKeywordAtTopLevel(text, "where", 0);

            Assert.AreEqual(text.LastIndexOf("where"), actual);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void FindKeywordAtTopLevel_WhenMissing_ReturnsMinusOne()
        {
            var actual = PostgreSqlSqlScanner.FindKeywordAtTopLevel("select 1", "where", 0);

            Assert.AreEqual(-1, actual);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void TryReadQualifiedTableName_WithUnquotedName_NormalizesToLowerCase()
        {
            var text = " Public.A_Test ";
            var index = 0;
            var success = PostgreSqlSqlScanner.TryReadQualifiedTableName(text, ref index, out string schemaName, out string tableName);

            Assert.IsTrue(success);
            Assert.AreEqual("public", schemaName);
            Assert.AreEqual("a_test", tableName);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void TryReadQualifiedTableName_WithQuotedName_PreservesIdentifierCase()
        {
            var text = "\"MySchema\".\"MyTable\"";
            var index = 0;
            var success = PostgreSqlSqlScanner.TryReadQualifiedTableName(text, ref index, out string schemaName, out string tableName);

            Assert.IsTrue(success);
            Assert.AreEqual("MySchema", schemaName);
            Assert.AreEqual("MyTable", tableName);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void TryReadQualifiedTableName_WithThreeParts_UsesLastTwoParts()
        {
            var text = "catalog.public.a_test";
            var index = 0;
            var success = PostgreSqlSqlScanner.TryReadQualifiedTableName(text, ref index, out string schemaName, out string tableName);

            Assert.IsTrue(success);
            Assert.AreEqual("public", schemaName);
            Assert.AreEqual("a_test", tableName);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void TryReadQualifiedColumnName_WithAlias_ReturnsLastPart()
        {
            var text = "a.t2";
            var index = 0;
            var success = PostgreSqlSqlScanner.TryReadQualifiedColumnName(text, ref index, out string columnName);

            Assert.IsTrue(success);
            Assert.AreEqual("t2", columnName);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void TryReadIdentifier_WithEscapedDoubleQuote_UnescapesIdentifier()
        {
            var text = "\"My\"\"Column\"";
            var index = 0;
            var success = PostgreSqlSqlScanner.TryReadIdentifier(text, ref index, out string identifier);

            Assert.IsTrue(success);
            Assert.AreEqual("My\"Column", identifier);
            Assert.AreEqual(text.Length, index);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void TryReadIdentifier_WithInvalidStart_ReturnsFalse()
        {
            var text = ".column";
            var index = 0;
            var success = PostgreSqlSqlScanner.TryReadIdentifier(text, ref index, out string identifier);

            Assert.IsFalse(success);
            Assert.AreEqual(string.Empty, identifier);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void TryReadParenthesizedContent_WithNestedFunctionAndQuotedComma_ReturnsWholeContent()
        {
            var text = " (repeat('a,b', 2), func(1, 2)) tail";
            var index = 0;
            var success = PostgreSqlSqlScanner.TryReadParenthesizedContent(text, ref index, out string content);

            Assert.IsTrue(success);
            Assert.AreEqual("repeat('a,b', 2), func(1, 2)", content);
            Assert.AreEqual(text.IndexOf(" tail"), index);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void TryReadParenthesizedContent_WithUnclosedParenthesis_ReturnsFalse()
        {
            var text = "(func(1, 2)";
            var index = 0;
            var success = PostgreSqlSqlScanner.TryReadParenthesizedContent(text, ref index, out string content);

            Assert.IsFalse(success);
            Assert.AreEqual(string.Empty, content);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void SplitTopLevelComma_WithFunctionsAndQuotedComma_SplitsOnlyTopLevel()
        {
            var actual = PostgreSqlSqlScanner.SplitTopLevelComma("t1 = 'a,b', t2 = repeat('x,y', 2), t3 = func(1, 2)");

            CollectionAssert.AreEqual
            (
                new[]
                {
                    "t1 = 'a,b'",
                    "t2 = repeat('x,y', 2)",
                    "t3 = func(1, 2)"
                },
                actual.ToArray()
            );
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void SplitTopLevelComma_WithQuotedIdentifierComma_DoesNotSplitIdentifier()
        {
            var actual = PostgreSqlSqlScanner.SplitTopLevelComma("\"a,b\", c");

            CollectionAssert.AreEqual(new[] { "\"a,b\"", "c" }, actual.ToArray());
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void FindTopLevelEqualSign_IgnoresEqualSignsInsideStringAndFunction()
        {
            var text = "t1 = replace('a=b', '=', '-')";
            var actual = PostgreSqlSqlScanner.FindTopLevelEqualSign(text);

            Assert.AreEqual(text.IndexOf(" = ") + 1, actual);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void FindTopLevelEqualSign_WhenMissing_ReturnsMinusOne()
        {
            Assert.AreEqual(-1, PostgreSqlSqlScanner.FindTopLevelEqualSign("repeat('=', 2)"));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void TryParseSingleQuotedStringLiteral_WithNormalText_ReturnsValue()
        {
            var success = PostgreSqlSqlScanner.TryParseSingleQuotedStringLiteral("  'abc'  ", out string value);

            Assert.IsTrue(success);
            Assert.AreEqual("abc", value);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void TryParseSingleQuotedStringLiteral_WithEscapedQuote_UnescapesValue()
        {
            var success = PostgreSqlSqlScanner.TryParseSingleQuotedStringLiteral("'abc''def'", out string value);

            Assert.IsTrue(success);
            Assert.AreEqual("abc'def", value);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void TryParseSingleQuotedStringLiteral_WithEmptyString_ReturnsEmptyValue()
        {
            var success = PostgreSqlSqlScanner.TryParseSingleQuotedStringLiteral("''", out string value);

            Assert.IsTrue(success);
            Assert.AreEqual(string.Empty, value);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void TryParseSingleQuotedStringLiteral_WithCastAfterLiteral_ReturnsFalse()
        {
            var success = PostgreSqlSqlScanner.TryParseSingleQuotedStringLiteral("'abc'::text", out string value);

            Assert.IsFalse(success);
            Assert.AreEqual(string.Empty, value);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void TryParseSingleQuotedStringLiteral_WithUnclosedLiteral_ReturnsFalse()
        {
            var success = PostgreSqlSqlScanner.TryParseSingleQuotedStringLiteral("'abc", out string value);

            Assert.IsFalse(success);
            Assert.AreEqual(string.Empty, value);
        }
        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        [TestCategory("SqlLexingMigration")]
        [DataRow("select 1 /* note */;", "c2VsZWN0IDE=")]
        [DataRow("select '--not comment', '/*still text*/';", "c2VsZWN0ICctLW5vdCBjb21tZW50JywgJy8qc3RpbGwgdGV4dCovJw==")]
        [DataRow("select \"--Column\", \"/*Column*/\";", "c2VsZWN0ICItLUNvbHVtbiIsICIvKkNvbHVtbiovIg==")]
        public void NormalizeSql_SharedTokenizer_MatchesGolden(string sql, string expectedBase64)
        {
            var expected = System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(expectedBase64));

            Assert.AreEqual(expected, PostgreSqlSqlScanner.NormalizeSql(sql));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        [TestCategory("SqlLexingMigration")]
        [DataRow("update a_test set t1 = 'where' where id = 1", "where", 31)]
        [DataRow("select \"where\" from public.a_test where id = 1", "where", 34)]
        [DataRow("update a_test set t1 = func(1, 2) returning t1", "returning", 34)]
        public void FindKeywordAtTopLevel_SharedTokenizer_MatchesGolden(string text, string keyword, int expectedPosition)
        {
            Assert.AreEqual(expectedPosition, PostgreSqlSqlScanner.FindKeywordAtTopLevel(text, keyword, 0));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        [TestCategory("SqlLexingMigration")]
        [DataRow("(repeat('a,b', 2), func(1, 2)) tail", true, 30, "cmVwZWF0KCdhLGInLCAyKSwgZnVuYygxLCAyKQ==")]
        [DataRow("(\"a)b\", ('x')) tail", true, 14, "ImEpYiIsICgneCcp")]
        public void TryReadParenthesizedContent_SharedTokenizer_MatchesGolden(string text, bool expectedSuccess, int expectedIndex, string expectedContentBase64)
        {
            var index = 0;
            var success = PostgreSqlSqlScanner.TryReadParenthesizedContent(text, ref index, out string actualContent);
            var expectedContent = System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(expectedContentBase64));

            Assert.AreEqual(expectedSuccess, success);
            Assert.AreEqual(expectedIndex, index);
            Assert.AreEqual(expectedContent, actualContent);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        [TestCategory("SqlLexingMigration")]
        [DataRow("t1 = 'a,b', t2 = repeat('x,y', 2), t3 = func(1, 2)", "dDEgPSAnYSxiJw==|dDIgPSByZXBlYXQoJ3gseScsIDIp|dDMgPSBmdW5jKDEsIDIp")]
        [DataRow("\"a,b\", c", "ImEsYiI=|Yw==")]
        public void SplitTopLevelComma_SharedTokenizer_MatchesGolden(string text, string expectedItemsBase64)
        {
            var expected = System.Array.ConvertAll(expectedItemsBase64.Split('|'), value => System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(value)));
            var actual = PostgreSqlSqlScanner.SplitTopLevelComma(text);

            CollectionAssert.AreEqual(expected, actual.ToArray());
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        [TestCategory("SqlLexingMigration")]
        [DataRow("t1 = replace('a=b', '=', '-')", 3)]
        [DataRow("t1 = func(1, 2)", 3)]
        public void FindTopLevelEqualSign_SharedTokenizer_MatchesGolden(string text, int expectedPosition)
        {
            Assert.AreEqual(expectedPosition, PostgreSqlSqlScanner.FindTopLevelEqualSign(text));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        [TestCategory("SqlLexingMigration")]
        [DataRow("'abc'", true, "YWJj")]
        [DataRow("'a''b'", true, "YSdi")]
        public void TryParseSingleQuotedStringLiteral_SharedTokenizer_MatchesGolden(string text, bool expectedSuccess, string expectedValueBase64)
        {
            var actualSuccess = PostgreSqlSqlScanner.TryParseSingleQuotedStringLiteral(text, out string actualValue);
            var expectedValue = System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(expectedValueBase64));

            Assert.AreEqual(expectedSuccess, actualSuccess);
            Assert.AreEqual(expectedValue, actualValue);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        [TestCategory("SqlLexingMigration")]
        public void FindKeywordAtTopLevel_DollarQuotedKeyword_IgnoresProtectedText()
        {
            const string text = "update a_test set t1 = $$where$$ where id = 1";

            var actual = PostgreSqlSqlScanner.FindKeywordAtTopLevel(text, "where", 0);

            Assert.AreEqual(text.LastIndexOf("where"), actual);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        [TestCategory("SqlLexingMigration")]
        public void TryReadParenthesizedContent_DollarQuotedParenthesis_ReturnsWholeContent()
        {
            const string text = "($$value ) still quoted$$, func(1, 2)) tail";

            var index = 0;
            var success = PostgreSqlSqlScanner.TryReadParenthesizedContent(text, ref index, out string content);

            Assert.IsTrue(success);
            Assert.AreEqual("$$value ) still quoted$$, func(1, 2)", content);
            Assert.AreEqual(text.IndexOf(" tail"), index);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        [TestCategory("SqlLexingMigration")]
        public void SplitTopLevelComma_DollarQuotedComma_DoesNotSplitProtectedText()
        {
            var actual = PostgreSqlSqlScanner.SplitTopLevelComma("t1 = $$a,b$$, t2 = 'c'");

            CollectionAssert.AreEqual(new[] { "t1 = $$a,b$$", "t2 = 'c'" }, actual.ToArray());
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        [TestCategory("SqlLexingMigration")]
        public void FindTopLevelEqualSign_DollarQuotedEqual_IgnoresProtectedText()
        {
            const string text = "t1 = $$a=b$$";

            var actual = PostgreSqlSqlScanner.FindTopLevelEqualSign(text);

            Assert.AreEqual(text.IndexOf(" = ") + 1, actual);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        [TestCategory("SqlLexingMigration")]
        public void NormalizeSql_NestedBlockComment_RemovesWholePostgreSqlComment()
        {
            const string sql = "select 1 /* outer /* inner */ outer */;";

            var actual = PostgreSqlSqlScanner.NormalizeSql(sql);

            Assert.AreEqual("select 1", actual);
        }
    }
}
