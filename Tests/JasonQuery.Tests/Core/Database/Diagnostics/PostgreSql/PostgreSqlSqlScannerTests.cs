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
    }
}