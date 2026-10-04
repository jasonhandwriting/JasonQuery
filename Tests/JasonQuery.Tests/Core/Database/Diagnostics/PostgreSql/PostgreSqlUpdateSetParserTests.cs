using JasonQuery.Core.Database.Diagnostics.PostgreSql;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace JasonQuery.Tests.Core.Database.Diagnostics.PostgreSql
{
    [TestClass]
    public sealed class PostgreSqlUpdateSetParserTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void TryParse_WithEmptySql_ReturnsFalse()
        {
            var success = PostgreSqlUpdateSetParser.TryParse(string.Empty, out PostgreSqlUpdateSetParseResult result);

            Assert.IsFalse(success);
            Assert.IsFalse(result.Success);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void TryParse_WithNonUpdateSql_ReturnsFalse()
        {
            var success = PostgreSqlUpdateSetParser.TryParse("insert into a_test (t1) values ('abc')", out PostgreSqlUpdateSetParseResult result);

            Assert.IsFalse(success);
            Assert.IsFalse(result.Success);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        [TestCategory("Regression")]
        public void TryParse_WithSimpleUpdateAndReturning_ParsesStringLiteral()
        {
            const string sql = "update a_test\r\n" +
                               "   set t2 = '123456789012345678901234567890123'\r\n" +
                               " where t1 is null\r\n" +
                               "returning t2";

            var success = PostgreSqlUpdateSetParser.TryParse(sql, out PostgreSqlUpdateSetParseResult result);

            Assert.IsTrue(success);
            Assert.AreEqual("public", result.SchemaName);
            Assert.AreEqual("a_test", result.TableName);
            Assert.HasCount(1, result.SetValues);
            Assert.AreEqual("t2", result.SetValues[0].ColumnName);
            Assert.IsTrue(result.SetValues[0].IsStringLiteral);
            Assert.AreEqual(33, result.SetValues[0].ActualLength);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void TryParse_WithQualifiedTable_UsesSpecifiedSchema()
        {
            const string sql = "update public.a_test\r\n" +
                               "   set t4 = 'abc'\r\n" +
                               " where t1 is null";

            var success = PostgreSqlUpdateSetParser.TryParse(sql, out PostgreSqlUpdateSetParseResult result);

            Assert.IsTrue(success);
            Assert.AreEqual("public", result.SchemaName);
            Assert.AreEqual("a_test", result.TableName);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void TryParse_WithQuotedTableAndColumn_PreservesIdentifierText()
        {
            const string sql = "update \"MySchema\".\"MyTable\"\r\n" +
                               "   set \"MyColumn\" = 'abc'";

            var success = PostgreSqlUpdateSetParser.TryParse(sql, out PostgreSqlUpdateSetParseResult result);

            Assert.IsTrue(success);
            Assert.AreEqual("MySchema", result.SchemaName);
            Assert.AreEqual("MyTable", result.TableName);
            Assert.AreEqual("MyColumn", result.SetValues[0].ColumnName);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        [TestCategory("Regression")]
        public void TryParse_WithAliasUsingAs_FindsSetClauseAfterAlias()
        {
            const string sql = "update a_test as a\r\n" +
                               "   set t4 = '123456755555589012345678901234567890123'\r\n" +
                               " where a.t1 = 'abc'";

            var success = PostgreSqlUpdateSetParser.TryParse(sql, out PostgreSqlUpdateSetParseResult result);

            Assert.IsTrue(success);
            Assert.AreEqual("t4", result.SetValues[0].ColumnName);
            Assert.AreEqual(39, result.SetValues[0].ActualLength);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        [TestCategory("Regression")]
        public void TryParse_WithAliasWithoutAs_FindsSetClauseAfterAlias()
        {
            const string sql = "update a_test a\r\n" +
                               "   set t4 = '12345678922222012345678901234567890123'\r\n" +
                               " where a.t1 = 'abc'";

            var success = PostgreSqlUpdateSetParser.TryParse(sql, out PostgreSqlUpdateSetParseResult result);

            Assert.IsTrue(success);
            Assert.AreEqual("t4", result.SetValues[0].ColumnName);
            Assert.AreEqual(38, result.SetValues[0].ActualLength);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void TryParse_WithOnlyAndAsterisk_ParsesTableAndSetClause()
        {
            const string sql = "update only public.a_test *\r\n" +
                               "   set t1 = 'abc'";

            var success = PostgreSqlUpdateSetParser.TryParse(sql, out PostgreSqlUpdateSetParseResult result);

            Assert.IsTrue(success);
            Assert.AreEqual("public", result.SchemaName);
            Assert.AreEqual("a_test", result.TableName);
            Assert.AreEqual("t1", result.SetValues[0].ColumnName);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        [TestCategory("Regression")]
        public void TryParse_WithMultipleAssignments_PreservesAssignmentOrder()
        {
            const string sql = "update a_test\r\n" +
                               "   set t1 = '12345678901234567890123456789012',\r\n" +
                               "       t2 = '123456789012345678901234567890123'\r\n" +
                               " where t1 is null";

            var success = PostgreSqlUpdateSetParser.TryParse(sql, out PostgreSqlUpdateSetParseResult result);

            Assert.IsTrue(success);
            CollectionAssert.AreEqual(new[] { "t1", "t2" }, result.SetValues.Select(item => item.ColumnName).ToArray());
            Assert.AreEqual(32, result.SetValues[0].ActualLength);
            Assert.AreEqual(33, result.SetValues[1].ActualLength);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        [TestCategory("Regression")]
        public void TryParse_WithEscapedSingleQuote_UnescapesAndCountsActualCharacters()
        {
            const string sql = "update a_test\r\n" +
                               "   set t4 = '1234567sssss890123456789012345678901''23'\r\n" +
                               " where t1 = 'abc'";

            var success = PostgreSqlUpdateSetParser.TryParse(sql, out PostgreSqlUpdateSetParseResult result);

            Assert.IsTrue(success);
            Assert.AreEqual("1234567sssss890123456789012345678901'23", result.SetValues[0].StringValue);
            Assert.AreEqual(39, result.SetValues[0].ActualLength);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        [TestCategory("Regression")]
        public void TryParse_WithFunctionExpression_MarksValueAsNonLiteral()
        {
            const string sql = "update a_test\r\n" +
                               "   set t2 = repeat('1', 33)\r\n" +
                               " where t1 is null";

            var success = PostgreSqlUpdateSetParser.TryParse(sql, out PostgreSqlUpdateSetParseResult result);

            Assert.IsTrue(success);
            Assert.IsFalse(result.SetValues[0].IsStringLiteral);
            Assert.AreEqual("repeat('1', 33)", result.SetValues[0].RawText);
            Assert.AreEqual(0, result.SetValues[0].ActualLength);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void TryParse_WithFunctionContainingComma_DoesNotSplitFunctionArguments()
        {
            const string sql = "update a_test\r\n" +
                               "   set t1 = concat('a', 'b'), t2 = 'c'\r\n" +
                               " where id = 1";

            var success = PostgreSqlUpdateSetParser.TryParse(sql, out PostgreSqlUpdateSetParseResult result);

            Assert.IsTrue(success);
            Assert.HasCount(2, result.SetValues);
            Assert.AreEqual("concat('a', 'b')", result.SetValues[0].RawText);
            Assert.AreEqual("c", result.SetValues[1].StringValue);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void TryParse_WithFromClause_StopsSetContentBeforeFrom()
        {
            const string sql = "update a_test\r\n" +
                               "   set t1 = 'abc'\r\n" +
                               "  from another_table b\r\n" +
                               " where b.id = a_test.id";

            var success = PostgreSqlUpdateSetParser.TryParse(sql, out PostgreSqlUpdateSetParseResult result);

            Assert.IsTrue(success);
            Assert.HasCount(1, result.SetValues);
            Assert.AreEqual("abc", result.SetValues[0].StringValue);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void TryParse_WithWhereKeywordInsideString_StopsAtActualWhereClause()
        {
            const string sql = "update a_test\r\n" +
                               "   set t1 = 'where', t2 = 'abc'\r\n" +
                               " where id = 1";

            var success = PostgreSqlUpdateSetParser.TryParse(sql, out PostgreSqlUpdateSetParseResult result);

            Assert.IsTrue(success);
            Assert.HasCount(2, result.SetValues);
            Assert.AreEqual("where", result.SetValues[0].StringValue);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        [TestCategory("Regression")]
        public void TryParse_WithQualifiedSetTarget_ReturnsLastIdentifierPart()
        {
            const string sql = "update a_test aa\r\n" +
                               "   set aa.t4 = '12345671234567890123'\r\n" +
                               " where aa.t1 = 'abc'";

            var success = PostgreSqlUpdateSetParser.TryParse(sql, out PostgreSqlUpdateSetParseResult result);

            Assert.IsTrue(success);
            Assert.AreEqual("t4", result.SetValues[0].ColumnName);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void TryParse_WithoutSetKeyword_ReturnsFalse()
        {
            var success = PostgreSqlUpdateSetParser.TryParse("update a_test where t1 = 1", out PostgreSqlUpdateSetParseResult result);

            Assert.IsFalse(success);
            Assert.IsFalse(result.Success);
            Assert.Contains("SET", result.FailureReason);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void TryParse_WithNoValidAssignments_ReturnsFalse()
        {
            var success = PostgreSqlUpdateSetParser.TryParse("update a_test set t1 where id = 1", out PostgreSqlUpdateSetParseResult result);

            Assert.IsFalse(success);
            Assert.IsFalse(result.Success);
            Assert.Contains("欄位指定", result.FailureReason);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        [TestCategory("Regression")]
        [TestCategory("SqlLexingMigration")]
        public void TryParse_WithDollarQuotedExpressionContainingWhereCommaAndEqual_ParsesAssignments()
        {
            const string sql = "update a_test\r\n" +
                               "   set t1 = $$where,=()$$, t2 = 'abc'\r\n" +
                               " where id = 1";

            var success = PostgreSqlUpdateSetParser.TryParse(sql, out PostgreSqlUpdateSetParseResult result);

            Assert.IsTrue(success);
            Assert.HasCount(2, result.SetValues);
            Assert.AreEqual("$$where,=()$$", result.SetValues[0].RawText);
            Assert.IsFalse(result.SetValues[0].IsStringLiteral);
            Assert.AreEqual("abc", result.SetValues[1].StringValue);
        }
    }
}
