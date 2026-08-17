using JasonQuery.Core.Database.Diagnostics.PostgreSql;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace JasonQuery.Tests.Core.Database.Diagnostics.PostgreSql
{
    [TestClass]
    public sealed class PostgreSqlInsertValuesParserTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void TryParse_WithEmptySql_ReturnsFalse()
        {
            var success = PostgreSqlInsertValuesParser.TryParse(string.Empty, out PostgreSqlInsertValuesParseResult result);

            Assert.IsFalse(success);
            Assert.IsFalse(result.Success);
            Assert.IsFalse(string.IsNullOrWhiteSpace(result.FailureReason));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void TryParse_WithNonInsertSql_ReturnsFalse()
        {
            var success = PostgreSqlInsertValuesParser.TryParse("update a_test set t1 = 'abc'", out PostgreSqlInsertValuesParseResult result);

            Assert.IsFalse(success);
            Assert.IsFalse(result.Success);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void TryParse_WithSimpleInsert_UsesPublicSchema()
        {
            const string sql = "insert into a_test (t1)\r\n" +
                               "values ('abc')";

            var success = PostgreSqlInsertValuesParser.TryParse(sql, out PostgreSqlInsertValuesParseResult result);

            Assert.IsTrue(success);
            Assert.IsTrue(result.Success);
            Assert.AreEqual("public", result.SchemaName);
            Assert.AreEqual("a_test", result.TableName);
            Assert.AreEqual("public.a_test", result.FullTableName);
            CollectionAssert.AreEqual(new[] { "t1" }, result.ColumnNames.ToArray());
            Assert.AreEqual(1, result.Values.Count);
            Assert.AreEqual("abc", result.Values[0].StringValue);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void TryParse_WithQualifiedTable_UsesSpecifiedSchema()
        {
            const string sql = "insert into public.a_test (t4)\r\n" +
                               "values ('abc')";

            var success = PostgreSqlInsertValuesParser.TryParse(sql, out PostgreSqlInsertValuesParseResult result);

            Assert.IsTrue(success);
            Assert.AreEqual("public", result.SchemaName);
            Assert.AreEqual("a_test", result.TableName);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void TryParse_WithQuotedIdentifiers_PreservesIdentifierText()
        {
            const string sql = "insert into \"MySchema\".\"MyTable\" (\"MyColumn\")\r\n" +
                               "values ('abc')";

            var success = PostgreSqlInsertValuesParser.TryParse(sql, out PostgreSqlInsertValuesParseResult result);

            Assert.IsTrue(success);
            Assert.AreEqual("MySchema", result.SchemaName);
            Assert.AreEqual("MyTable", result.TableName);
            CollectionAssert.AreEqual(new[] { "MyColumn" }, result.ColumnNames.ToArray());
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        [TestCategory("Regression")]
        public void TryParse_WithCommentBetweenColumnListAndValues_ParsesSuccessfully()
        {
            const string sql = "insert into a_test (t2)\r\n" +
                               "-- comment\r\n" +
                               "values ('123456789012345678901234567890123')";

            var success = PostgreSqlInsertValuesParser.TryParse(sql, out PostgreSqlInsertValuesParseResult result);

            Assert.IsTrue(success);
            Assert.AreEqual(1, result.Values.Count);
            Assert.AreEqual(33, result.Values[0].ActualLength);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        [TestCategory("Regression")]
        public void TryParse_WithEscapedSingleQuote_UnescapesAndCountsActualCharacters()
        {
            const string sql = "insert into a_test (t2)\r\n" +
                               "values ('1234567890123456789012345678901''23')";

            var success = PostgreSqlInsertValuesParser.TryParse(sql, out PostgreSqlInsertValuesParseResult result);

            Assert.IsTrue(success);
            Assert.AreEqual("1234567890123456789012345678901'23", result.Values[0].StringValue);
            Assert.AreEqual(34, result.Values[0].ActualLength);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void TryParse_WithExpressionValue_MarksValueAsNonLiteral()
        {
            const string sql = "insert into a_test (t2)\r\n" +
                               "values (repeat('1', 33))";

            var success = PostgreSqlInsertValuesParser.TryParse(sql, out PostgreSqlInsertValuesParseResult result);

            Assert.IsTrue(success);
            Assert.AreEqual(1, result.Values.Count);
            Assert.IsFalse(result.Values[0].IsStringLiteral);
            Assert.AreEqual("repeat('1', 33)", result.Values[0].RawText);
            Assert.AreEqual(0, result.Values[0].ActualLength);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        [TestCategory("Regression")]
        public void TryParse_WithMultipleValuesRows_AssignsOneBasedRowIndexes()
        {
            const string sql = "insert into a_test (t2)\r\n" +
                               "values ('first'),\r\n" +
                               "       ('second'),\r\n" +
                               "       ('third')";

            var success = PostgreSqlInsertValuesParser.TryParse(sql, out PostgreSqlInsertValuesParseResult result);

            Assert.IsTrue(success);
            Assert.AreEqual(3, result.Values.Count);
            Assert.AreEqual(1, result.Values[0].ValuesRowIndex);
            Assert.AreEqual(2, result.Values[1].ValuesRowIndex);
            Assert.AreEqual(3, result.Values[2].ValuesRowIndex);
            Assert.AreEqual("third", result.Values[2].StringValue);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void TryParse_WithMultipleColumns_MapsColumnIndexesInEachRow()
        {
            const string sql = "insert into a_test (t1, t2)\r\n" +
                               "values ('a', 'b'), ('c', 'd')";

            var success = PostgreSqlInsertValuesParser.TryParse(sql, out PostgreSqlInsertValuesParseResult result);

            Assert.IsTrue(success);
            Assert.AreEqual(4, result.Values.Count);

            Assert.AreEqual(1, result.Values[0].ValuesRowIndex);
            Assert.AreEqual(0, result.Values[0].ColumnIndex);
            Assert.AreEqual("a", result.Values[0].StringValue);

            Assert.AreEqual(1, result.Values[1].ValuesRowIndex);
            Assert.AreEqual(1, result.Values[1].ColumnIndex);
            Assert.AreEqual("b", result.Values[1].StringValue);

            Assert.AreEqual(2, result.Values[2].ValuesRowIndex);
            Assert.AreEqual(0, result.Values[2].ColumnIndex);
            Assert.AreEqual("c", result.Values[2].StringValue);

            Assert.AreEqual(2, result.Values[3].ValuesRowIndex);
            Assert.AreEqual(1, result.Values[3].ColumnIndex);
            Assert.AreEqual("d", result.Values[3].StringValue);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void TryParse_WithReturningClause_ParsesSuccessfully()
        {
            const string sql = "insert into a_test (t1)\r\n" +
                               "values ('abc')\r\n" +
                               "returning t1";

            var success = PostgreSqlInsertValuesParser.TryParse(sql, out PostgreSqlInsertValuesParseResult result);

            Assert.IsTrue(success);
            Assert.IsTrue(result.Success);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void TryParse_WithUnsupportedTail_ReturnsFalse()
        {
            const string sql = "insert into a_test (t1)\r\n" +
                               "values ('abc')\r\n" +
                               "    on conflict do nothing";

            var success = PostgreSqlInsertValuesParser.TryParse(sql, out PostgreSqlInsertValuesParseResult result);

            Assert.IsFalse(success);
            Assert.IsFalse(result.Success);
            StringAssert.Contains(result.FailureReason, "不支援");
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void TryParse_WithoutColumnList_ReturnsFalse()
        {
            const string sql = "insert into a_test\r\n" +
                               "values ('abc')";

            var success = PostgreSqlInsertValuesParser.TryParse(sql, out PostgreSqlInsertValuesParseResult result);

            Assert.IsFalse(success);
            Assert.IsFalse(result.Success);
            StringAssert.Contains(result.FailureReason, "column list");
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        [TestCategory("Regression")]
        public void TryParse_WhenColumnCountExceedsValueCount_ReturnsFalse()
        {
            const string sql = "insert into a_test (t1, t2)\r\n" +
                               "values ('123456789012345678901234567890123')";

            var success = PostgreSqlInsertValuesParser.TryParse(sql, out PostgreSqlInsertValuesParseResult result);

            Assert.IsFalse(success);
            Assert.IsFalse(result.Success);
            StringAssert.Contains(
                result.FailureReason,
                "VALUES 數量");
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void TryParse_WhenSecondValuesRowHasWrongCount_ReturnsFalseAndIdentifiesRow()
        {
            const string sql = "insert into a_test (t1, t2)\r\n" +
                               "values ('a', 'b'),\r\n" +
                               "       ('c')";

            var success = PostgreSqlInsertValuesParser.TryParse(sql, out PostgreSqlInsertValuesParseResult result);

            Assert.IsFalse(success);
            Assert.IsFalse(result.Success);
            StringAssert.Contains(result.FailureReason, "第 2 組");
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void TryParse_WithSemicolonAndWhitespace_ParsesSuccessfully()
        {
            const string sql = "  insert into a_test (t1) values ('abc');  \r\n";

            var success = PostgreSqlInsertValuesParser.TryParse(sql, out PostgreSqlInsertValuesParseResult result);

            Assert.IsTrue(success);
            Assert.AreEqual("abc", result.Values[0].StringValue);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void TryParseSingleRow_WithMultipleRows_RemainsCompatibleAndParsesAllRows()
        {
            const string sql = "insert into a_test (t2)\r\n" +
                               "values ('a'), ('b')";

            var success = PostgreSqlInsertValuesParser.TryParseSingleRow(sql, out PostgreSqlInsertValuesParseResult result);

            Assert.IsTrue(success);
            Assert.AreEqual(2, result.Values.Count);
        }
    }
}