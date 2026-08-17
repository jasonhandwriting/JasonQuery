using JasonLibrary.Core.Database.Enums;
using JasonLibrary.Core.Text.Formatting;
using JasonLibrary.Core.Text.Formatting.Engines;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonLibrary.Tests.Core.Text.Formatting
{
    [TestClass]
    public sealed class HogimnSqlFormatterEngineTests
    {
        private readonly HogimnSqlFormatterEngine _engine = new HogimnSqlFormatterEngine();

        [DataTestMethod]
        [DataRow(DatabaseProviderKind.Oracle)]
        [DataRow(DatabaseProviderKind.PostgreSql)]
        [DataRow(DatabaseProviderKind.SqlServer)]
        [DataRow(DatabaseProviderKind.MySql)]
        [DataRow(DatabaseProviderKind.Sqlite)]
        public void Supports_AllPlannedProviders(DatabaseProviderKind providerKind)
        {
            Assert.IsTrue(_engine.Supports(providerKind));
        }

        [TestMethod]
        public void Supports_UnknownProvider_ReturnsFalse()
        {
            Assert.IsFalse(_engine.Supports(DatabaseProviderKind.Unknown));
        }

        [DataTestMethod]
        [DataRow(DatabaseProviderKind.Oracle,
                 "SELECT e.employee_id,e.employee_name FROM hr.employee e WHERE e.employee_id=:employee_id ORDER BY e.employee_name",
                 "e.employee_id = :employee_id")]
        [DataRow(DatabaseProviderKind.PostgreSql,
                 "SELECT u.id,u.name FROM public.users u WHERE u.enabled=true ORDER BY u.name LIMIT 100",
                 "u.enabled = true")]
        [DataRow(DatabaseProviderKind.SqlServer,
                 "SELECT TOP (100) c.Id,c.Name FROM dbo.Customer c WHERE c.IsActive=1 ORDER BY c.Name;",
                 "TOP (100) c.Id")]
        [DataRow(DatabaseProviderKind.MySql,
                 "SELECT u.id,u.name FROM `user` u WHERE u.enabled=1 ORDER BY u.name LIMIT 100;",
                 "`user` u")]
        [DataRow(DatabaseProviderKind.Sqlite,
                 "SELECT i.id,i.name FROM item i WHERE i.deleted_at IS NULL ORDER BY i.name LIMIT 100;",
                 "i.deleted_at IS NULL")]
        public void Format_BasicSql_UsesProviderDialectAndPassesSafetyValidation(DatabaseProviderKind providerKind, string sql, string expectedFragment)
        {
            var result = Format(providerKind, sql);

            Assert.IsTrue(result.Success, result.ErrorMessage);
            Assert.AreEqual(SqlFormatterEngineKind.Hogimn, result.EngineKind);
            StringAssert.Contains(result.FormattedSql, expectedFragment);
            Assert.IsFalse(result.FormattedSql.Contains("\t"));
        }

        [TestMethod]
        public void Format_PostgreSqlConcatenationCorruption_ReturnsOriginalSql()
        {
            const string sql = "SELECT '\"' || value || '\"' FROM sample";

            var result = Format(DatabaseProviderKind.PostgreSql, sql);

            AssertSafetyFailurePreservesOriginal(sql, result);
            StringAssert.Contains(result.ErrorMessage, "Operator '||'");
        }

        [TestMethod]
        public void Format_PostgreSqlEscapeStringPrefixCorruption_ReturnsOriginalSql()
        {
            const string sql = "SELECT E'\\n' FROM sample";

            var result = Format(DatabaseProviderKind.PostgreSql, sql);

            AssertSafetyFailurePreservesOriginal(sql, result);
            StringAssert.Contains(result.ErrorMessage, "StringLiteral");
        }

        [TestMethod]
        public void Format_PostgreSqlCommaMovedIntoLineComment_ReturnsOriginalSql()
        {
            const string sql = "SELECT first_value -- comment before comma\n, second_value FROM sample";

            var result = Format(DatabaseProviderKind.PostgreSql, sql);

            AssertSafetyFailurePreservesOriginal(sql, result);
            StringAssert.Contains(result.ErrorMessage, "Comment");
        }

        [TestMethod]
        public void Format_PostgreSqlCommaBeforeLineComment_RemainsSafe()
        {
            const string sql = "SELECT first_value, -- keep comma active\nsecond_value FROM sample";

            var result = Format(DatabaseProviderKind.PostgreSql, sql);

            Assert.IsTrue(result.Success, result.ErrorMessage);
            StringAssert.Contains(result.FormattedSql, "first_value,");
            StringAssert.Contains(result.FormattedSql, "-- keep comma active");
        }

        [TestMethod]
        public void Format_OracleAlternativeQuotedConcatenation_RemainsSafe()
        {
            const string sql = "SELECT q'[Jason's SQL]' || employee_name FROM employee";

            var result = Format(DatabaseProviderKind.Oracle, sql);

            Assert.IsTrue(result.Success, result.ErrorMessage);
            StringAssert.Contains(result.FormattedSql, "q'[Jason's SQL]' || employee_name");
        }

        [DataTestMethod]
        [DataRow("A")]
        [DataRow("a")]
        [DataRow("C")]
        [DataRow("c")]
        public void Format_OracleSingleLetterAlias_DoesNotInsertWhitespaceBeforeDot(string alias)
        {
            var sql = $"SELECT {alias}.OWNER,{alias}.TABLE_NAME FROM ALL_TAB_COLUMNS {alias} WHERE {alias}.OWNER=USER";

            var result = Format(DatabaseProviderKind.Oracle, sql);

            Assert.IsTrue(result.Success, result.ErrorMessage);
            StringAssert.Contains(result.FormattedSql, alias + ".OWNER");
            StringAssert.Contains(result.FormattedSql, alias + ".TABLE_NAME");
            Assert.IsFalse(result.FormattedSql.Contains(alias + " .OWNER"));
            Assert.IsFalse(result.FormattedSql.Contains(alias + " .TABLE_NAME"));
        }

        [TestMethod]
        public void Format_OracleQualifiedIdentifierTextInsideString_RemainsUntouched()
        {
            const string sql = "SELECT C.OWNER, 'A.OWNER', 'C.OWNER' FROM ALL_TAB_COLUMNS C WHERE C.OWNER = USER";

            var result = Format(DatabaseProviderKind.Oracle, sql);

            Assert.IsTrue(result.Success, result.ErrorMessage);
            StringAssert.Contains(result.FormattedSql, "C.OWNER");
            StringAssert.Contains(result.FormattedSql, "'A.OWNER'");
            StringAssert.Contains(result.FormattedSql, "'C.OWNER'");
        }

        [TestMethod]
        public void Format_OracleSelect_UsesStep4ClauseAlignment()
        {
            const string sql = "SELECT C.OWNER,C.TABLE_NAME,C.COLUMN_NAME FROM ALL_TAB_COLUMNS C WHERE C.OWNER=USER ORDER BY C.TABLE_NAME,C.COLUMN_ID;";
            const string expected = "SELECT C.OWNER,\r\n"
                                    + "       C.TABLE_NAME,\r\n"
                                    + "       C.COLUMN_NAME\r\n"
                                    + "  FROM ALL_TAB_COLUMNS C\r\n"
                                    + " WHERE C.OWNER = USER\r\n"
                                    + " ORDER BY C.TABLE_NAME,\r\n"
                                    + "          C.COLUMN_ID;";

            var result = Format
            (
                DatabaseProviderKind.Oracle,
                sql,
                new SqlFormatOptions(listItemsPerLine: 1)
            );

            Assert.IsTrue(result.Success, result.ErrorMessage);
            Assert.AreEqual(expected, result.FormattedSql);
        }

        [TestMethod]
        public void Format_OracleDistinct_KeepsModifierAndFirstItemOnSelectLine()
        {
            const string sql = "SELECT DISTINCT C.OWNER,C.TABLE_NAME FROM ALL_TAB_COLUMNS C;";

            var result = Format
            (
                DatabaseProviderKind.Oracle,
                sql,
                new SqlFormatOptions(listItemsPerLine: 1)
            );

            Assert.IsTrue(result.Success, result.ErrorMessage);
            StringAssert.Contains(result.FormattedSql, "SELECT DISTINCT C.OWNER,");
            StringAssert.Contains(result.FormattedSql, "\r\n       C.TABLE_NAME");
        }

        [TestMethod]
        public void Format_OracleGroupHavingAndOrderBy_AlignsEachClause()
        {
            const string sql = "SELECT C.OWNER,COUNT(*) AS COLUMN_COUNT FROM ALL_TAB_COLUMNS C WHERE C.OWNER IS NOT NULL GROUP BY C.OWNER HAVING COUNT(*)>1 ORDER BY C.OWNER;";
            const string expected = "SELECT C.OWNER,\r\n"
                                    + "       COUNT(*) AS COLUMN_COUNT\r\n"
                                    + "  FROM ALL_TAB_COLUMNS C\r\n"
                                    + " WHERE C.OWNER IS NOT NULL\r\n"
                                    + " GROUP BY C.OWNER\r\n"
                                    + "HAVING COUNT(*) > 1\r\n"
                                    + " ORDER BY C.OWNER;";

            var result = Format
            (
                DatabaseProviderKind.Oracle,
                sql,
                new SqlFormatOptions(listItemsPerLine: 1)
            );

            Assert.IsTrue(result.Success, result.ErrorMessage);
            Assert.AreEqual(expected, result.FormattedSql);
        }

        [TestMethod]
        public void Format_OracleCommaSeparatedFromItems_AlignsWithFirstTable()
        {
            const string sql = "SELECT C.OWNER,T.TABLE_NAME FROM ALL_TAB_COLUMNS C,ALL_TABLES T WHERE T.OWNER=C.OWNER AND T.TABLE_NAME=C.TABLE_NAME;";

            var result = Format(DatabaseProviderKind.Oracle, sql);

            Assert.IsTrue(result.Success, result.ErrorMessage);
            StringAssert.Contains(result.FormattedSql, "\r\n  FROM ALL_TAB_COLUMNS C,\r\n       ALL_TABLES T");
            StringAssert.Contains(result.FormattedSql, "\r\n WHERE T.OWNER = C.OWNER");
            StringAssert.Contains(result.FormattedSql, "\r\n   AND T.TABLE_NAME = C.TABLE_NAME;");
        }

        [TestMethod]
        public void Format_OracleJoinConditions_AlignsOnAndLogicalOperators()
        {
            const string sql = "SELECT C.OWNER,CC.COMMENTS FROM ALL_TAB_COLUMNS C LEFT JOIN ALL_COL_COMMENTS CC ON CC.OWNER=C.OWNER AND CC.TABLE_NAME=C.TABLE_NAME WHERE C.COLUMN_ID BETWEEN 1 AND 10 OR C.COLUMN_ID IS NULL;";

            var result = Format(DatabaseProviderKind.Oracle, sql);

            Assert.IsTrue(result.Success, result.ErrorMessage);
            StringAssert.Contains(result.FormattedSql, "\r\n  LEFT JOIN ALL_COL_COMMENTS CC");
            StringAssert.Contains(result.FormattedSql, "\r\n    ON CC.OWNER = C.OWNER");
            StringAssert.Contains(result.FormattedSql, "\r\n   AND CC.TABLE_NAME = C.TABLE_NAME");
            StringAssert.Contains(result.FormattedSql, "\r\n WHERE C.COLUMN_ID BETWEEN 1 AND 10");
            StringAssert.Contains(result.FormattedSql, "\r\n    OR C.COLUMN_ID IS NULL;");
            Assert.IsFalse(result.FormattedSql.Contains("BETWEEN 1\r\n   AND 10"));
        }

        [TestMethod]
        public void Format_OracleBetweenDateLiterals_DoesNotTreatRangeAndAsLogicalAnd()
        {
            const string sql = "SELECT C.OWNER FROM ALL_TAB_COLUMNS C WHERE C.CREATED_AT BETWEEN DATE '2026-01-01' AND DATE '2026-12-31' AND C.OWNER=USER;";

            var result = Format(DatabaseProviderKind.Oracle, sql);

            Assert.IsTrue(result.Success, result.ErrorMessage);

            StringAssert.Contains
            (
                result.FormattedSql,
                "C.CREATED_AT BETWEEN DATE '2026-01-01' AND DATE '2026-12-31'"
            );

            StringAssert.Contains(result.FormattedSql, "\r\n   AND C.OWNER = USER;");
        }

        [TestMethod]
        public void Format_OracleFunctionArguments_DoesNotTreatNestedCommasAsSelectItems()
        {
            const string sql = "SELECT NVL(C.DATA_DEFAULT,'N/A') AS DEFAULT_VALUE,C.COLUMN_NAME FROM ALL_TAB_COLUMNS C WHERE INSTR(C.COLUMN_NAME,',')>0;";

            var result = Format
            (
                DatabaseProviderKind.Oracle,
                sql,
                new SqlFormatOptions(listItemsPerLine: 1)
            );

            Assert.IsTrue(result.Success, result.ErrorMessage);
            StringAssert.Contains(result.FormattedSql, "SELECT NVL(C.DATA_DEFAULT, 'N/A') AS DEFAULT_VALUE,");
            StringAssert.Contains(result.FormattedSql, "\r\n       C.COLUMN_NAME");
            StringAssert.Contains(result.FormattedSql, "INSTR(C.COLUMN_NAME, ',') > 0;");
        }

        [TestMethod]
        public void Format_OracleNestedSelect_UsesNestedQueryBaseIndent()
        {
            const string sql = "SELECT C.OWNER,(SELECT MAX(X.COLUMN_ID) FROM ALL_TAB_COLUMNS X WHERE X.OWNER=C.OWNER) AS MAX_COLUMN_ID FROM ALL_TAB_COLUMNS C;";

            var result = Format(DatabaseProviderKind.Oracle, sql);

            Assert.IsTrue(result.Success, result.ErrorMessage);
            StringAssert.Contains(result.FormattedSql, "SELECT C.OWNER,");
            StringAssert.Contains(result.FormattedSql, "SELECT MAX(X.COLUMN_ID)");
            StringAssert.Contains(result.FormattedSql, "  FROM ALL_TAB_COLUMNS X");
            StringAssert.Contains(result.FormattedSql, " WHERE X.OWNER = C.OWNER");
            StringAssert.Contains(result.FormattedSql, "\r\n  FROM ALL_TAB_COLUMNS C;");
        }

        [TestMethod]
        public void Format_OracleHint_RemainsAttachedToSelectClause()
        {
            const string sql = "SELECT /*+ INDEX(C IDX_ALL_TAB_COLUMNS) */ C.OWNER,C.TABLE_NAME FROM ALL_TAB_COLUMNS C;";

            var result = Format
            (
                DatabaseProviderKind.Oracle,
                sql,
                new SqlFormatOptions(listItemsPerLine: 1)
            );

            Assert.IsTrue(result.Success, result.ErrorMessage);
            StringAssert.Contains(result.FormattedSql, "SELECT /*+ INDEX(C IDX_ALL_TAB_COLUMNS) */ C.OWNER,");
            StringAssert.Contains(result.FormattedSql, "\r\n       C.TABLE_NAME");
        }

        [TestMethod]
        public void Format_OracleLineCommentBeforeFirstItem_KeepsFollowingItemAligned()
        {
            const string sql = "SELECT -- selected columns\r\nC.OWNER,C.TABLE_NAME FROM ALL_TAB_COLUMNS C;";

            var result = Format
            (
                DatabaseProviderKind.Oracle,
                sql,
                new SqlFormatOptions(listItemsPerLine: 1)
            );

            Assert.IsTrue(result.Success, result.ErrorMessage);
            StringAssert.Contains(result.FormattedSql, "-- selected columns\r\n       C.OWNER,");
            StringAssert.Contains(result.FormattedSql, "\r\n       C.TABLE_NAME");
        }

        [TestMethod]
        public void Format_Oracle_ListItemsPerLine_DefaultsToThree()
        {
            const string sql = "SELECT C.OWNER,C.TABLE_NAME,C.COLUMN_NAME FROM ALL_TAB_COLUMNS C WHERE C.OWNER=USER ORDER BY C.TABLE_NAME,C.COLUMN_ID;";
            const string expected = "SELECT C.OWNER, C.TABLE_NAME, C.COLUMN_NAME\r\n"
                                    + "  FROM ALL_TAB_COLUMNS C\r\n"
                                    + " WHERE C.OWNER = USER\r\n"
                                    + " ORDER BY C.TABLE_NAME, C.COLUMN_ID;";

            var result = Format(DatabaseProviderKind.Oracle, sql);

            Assert.IsTrue(result.Success, result.ErrorMessage);
            Assert.AreEqual(SqlFormatOptions.DefaultListItemsPerLine, new SqlFormatOptions().ListItemsPerLine);
            Assert.AreEqual(expected, result.FormattedSql);
        }

        [DataTestMethod]
        [DataRow(1, "SELECT C.A,\r\n       C.B,\r\n       C.C,\r\n       C.D")]
        [DataRow(2, "SELECT C.A, C.B,\r\n       C.C, C.D")]
        [DataRow(3, "SELECT C.A, C.B, C.C,\r\n       C.D")]
        [DataRow(10, "SELECT C.A, C.B, C.C, C.D")]
        public void Format_Oracle_ListItemsPerLine_UsesConfiguredUpperBound(int itemsPerLine,
                                                                          string expectedSelect)
        {
            const string sql = "SELECT C.A,C.B,C.C,C.D FROM CUSTOMER C;";

            var options = new SqlFormatOptions(maxLineWidth: 1000, listItemsPerLine: itemsPerLine);
            var result = Format(DatabaseProviderKind.Oracle, sql, options);

            Assert.IsTrue(result.Success, result.ErrorMessage);
            StringAssert.Contains(result.FormattedSql, expectedSelect);
        }

        [TestMethod]
        public void Format_Oracle_ListItemsPerLine_WrapsBeforeCountWhenMaxWidthIsReached()
        {
            const string sql = "SELECT C.OWNER,C.TABLE_NAME FROM ALL_TAB_COLUMNS C;";

            var options = new SqlFormatOptions(maxLineWidth: 20, listItemsPerLine: 10);
            var result = Format(DatabaseProviderKind.Oracle, sql, options);

            Assert.IsTrue(result.Success, result.ErrorMessage);
            StringAssert.Contains(result.FormattedSql, "SELECT C.OWNER,\r\n       C.TABLE_NAME");
        }

        [TestMethod]
        public void Format_Oracle_ListItemsPerLine_AppliesToGroupByAndOrderBy()
        {
            const string sql = "SELECT C.A,C.B,C.C FROM CUSTOMER C GROUP BY C.A,C.B,C.C ORDER BY C.A,C.B,C.C;";

            var options = new SqlFormatOptions(maxLineWidth: 1000, listItemsPerLine: 2);
            var result = Format(DatabaseProviderKind.Oracle, sql, options);

            Assert.IsTrue(result.Success, result.ErrorMessage);
            StringAssert.Contains(result.FormattedSql, "SELECT C.A, C.B,\r\n       C.C");
            StringAssert.Contains(result.FormattedSql, " GROUP BY C.A, C.B,\r\n          C.C");
            StringAssert.Contains(result.FormattedSql, " ORDER BY C.A, C.B,\r\n          C.C;");
        }

        [TestMethod]
        public void Format_Oracle_ListItemsPerLine_DoesNotCountFunctionOrInListCommas()
        {
            const string sql = "SELECT NVL(C.A,C.B) AS VALUE,C.C FROM CUSTOMER C WHERE C.ID IN (1,2,3);";

            var options = new SqlFormatOptions(maxLineWidth: 1000, listItemsPerLine: 2);
            var result = Format(DatabaseProviderKind.Oracle, sql, options);

            Assert.IsTrue(result.Success, result.ErrorMessage);
            StringAssert.Contains(result.FormattedSql, "SELECT NVL(C.A, C.B) AS VALUE, C.C");
            StringAssert.Contains(result.FormattedSql, "C.ID IN (1, 2, 3);");
        }

        [TestMethod]
        public void Format_Oracle_ListItemsPerLine_AppliesIndependentlyInsideNestedSelect()
        {
            const string sql = "SELECT C.A,(SELECT X.A,X.B,X.C FROM CHILD X WHERE X.ID=C.ID) AS CHILD_VALUE FROM CUSTOMER C;";

            var options = new SqlFormatOptions(maxLineWidth: 1000, listItemsPerLine: 2);
            var result = Format(DatabaseProviderKind.Oracle, sql, options);

            Assert.IsTrue(result.Success, result.ErrorMessage);
            StringAssert.Contains(result.FormattedSql, "SELECT X.A, X.B,");
            StringAssert.Contains(result.FormattedSql, "X.C");
            Assert.IsFalse(result.FormattedSql.Contains("SELECT X.A, X.B, X.C"));
        }

        [DataTestMethod]
        [DataRow(0)]
        [DataRow(11)]
        public void SqlFormatOptions_ListItemsPerLineOutsideRange_Throws(int itemsPerLine)
        {
            Assert.ThrowsException<System.ArgumentOutOfRangeException>
            (
                () => new SqlFormatOptions(listItemsPerLine: itemsPerLine)
            );
        }

        [DataTestMethod]
        [DataRow(DatabaseProviderKind.PostgreSql)]
        [DataRow(DatabaseProviderKind.SqlServer)]
        [DataRow(DatabaseProviderKind.MySql)]
        [DataRow(DatabaseProviderKind.Sqlite)]
        public void Format_NonOracleProvider_KeepsStep3ClauseLayoutAndAppliesListPacking(DatabaseProviderKind providerKind)
        {
            const string sql = "SELECT C.ID,C.NAME FROM CUSTOMER C WHERE C.ID=1 ORDER BY C.NAME;";

            var options = new SqlFormatOptions(listItemsPerLine: 10);
            var result = Format(providerKind, sql, options);

            Assert.IsTrue(result.Success, result.ErrorMessage);
            StringAssert.Contains(result.FormattedSql, "SELECT\r\n");
            StringAssert.Contains(result.FormattedSql, "\r\nFROM\r\n");
            StringAssert.Contains(result.FormattedSql, "\r\nWHERE\r\n");
            StringAssert.Contains(result.FormattedSql, "\r\nORDER BY\r\n");
            StringAssert.Contains(result.FormattedSql, "C.ID, C.NAME");
            Assert.IsFalse(result.FormattedSql.Contains("SELECT C.ID"));
        }

        [TestMethod]
        public void Format_OracleLowerKeywordCase_KeepsStep4ClauseAlignment()
        {
            const string sql = "SELECT C.OWNER FROM ALL_TAB_COLUMNS C WHERE C.OWNER=USER ORDER BY C.OWNER;";
            var options = new SqlFormatOptions(keywordCase: SqlFormatterKeywordCase.Lower);

            var result = Format(DatabaseProviderKind.Oracle, sql, options);

            Assert.IsTrue(result.Success, result.ErrorMessage);
            StringAssert.Contains(result.FormattedSql, "select C.OWNER\r\n  from ALL_TAB_COLUMNS C");
            StringAssert.Contains(result.FormattedSql, "\r\n where C.OWNER = user");
            StringAssert.Contains(result.FormattedSql, "\r\n order by C.OWNER;");
        }

        [TestMethod]
        public void Format_OracleLowerKeywordCase_AllowsUserKeywordCaseChange()
        {
            const string sql = "SELECT C.OWNER FROM ALL_TAB_COLUMNS C WHERE C.OWNER = USER";

            var options = new SqlFormatOptions(keywordCase: SqlFormatterKeywordCase.Lower);
            var result = Format(DatabaseProviderKind.Oracle, sql, options);

            Assert.IsTrue(result.Success, result.ErrorMessage);
            StringAssert.Contains(result.FormattedSql, "select");
            StringAssert.Contains(result.FormattedSql, "user");
            Assert.IsFalse(result.FormattedSql.Contains("C ."));
        }

        [DataTestMethod]
        [DataRow(DatabaseProviderKind.Oracle, "SELECT CAST(1 AS INTEGER) FROM DUAL", "integer", "select")]
        [DataRow(DatabaseProviderKind.PostgreSql, "SELECT CURRENT_USER", "current_user", "select")]
        [DataRow(DatabaseProviderKind.SqlServer, "SELECT CURRENT_USER", "current_user", "select")]
        [DataRow(DatabaseProviderKind.MySql, "SELECT CURRENT_USER()", "current_user", "select")]
        [DataRow(DatabaseProviderKind.Sqlite, "SELECT CAST(1 AS INTEGER)", "cast", "integer")]
        public void Format_LowerKeywordCase_AllowsDialectTokensWithoutManualCatalog(DatabaseProviderKind providerKind, string sql, string expectedFirstKeyword, string expectedSecondKeyword)
        {
            var options = new SqlFormatOptions(keywordCase: SqlFormatterKeywordCase.Lower);
            var result = Format(providerKind, sql, options);

            Assert.IsTrue(result.Success, result.ErrorMessage);
            StringAssert.Contains(result.FormattedSql, expectedFirstKeyword);
            StringAssert.Contains(result.FormattedSql, expectedSecondKeyword);
        }

        [TestMethod]
        public void Format_OracleSameWordInKeywordAndQualifiedIdentifier_ChangesOnlyKeywordOccurrence()
        {
            const string sql = "SELECT USER, C.USER FROM CUSTOMER C WHERE C.USER = USER";

            var options = new SqlFormatOptions(keywordCase: SqlFormatterKeywordCase.Lower);
            var result = Format(DatabaseProviderKind.Oracle, sql, options);

            Assert.IsTrue(result.Success, result.ErrorMessage);
            StringAssert.Contains(result.FormattedSql, "select");
            StringAssert.Contains(result.FormattedSql, "user,");
            StringAssert.Contains(result.FormattedSql, "C.USER");
            StringAssert.Contains(result.FormattedSql, "= user");
            Assert.IsFalse(result.FormattedSql.Contains("C.user"));
        }

        [TestMethod]
        public void Format_MultiWordKeyword_AuthorizesEveryContainedSemanticToken()
        {
            const string sql = "SELECT ID FROM CUSTOMER ORDER BY ID";

            var options = new SqlFormatOptions(keywordCase: SqlFormatterKeywordCase.Lower);
            var result = Format(DatabaseProviderKind.SqlServer, sql, options);

            Assert.IsTrue(result.Success, result.ErrorMessage);
            StringAssert.Contains(result.FormattedSql, "order by");
        }

        [TestMethod]
        public void Format_SqliteNamedParameterCorruption_ReturnsOriginalSql()
        {
            const string sql = "SELECT first_name || ' ' || last_name FROM person WHERE id=:id";

            var result = Format(DatabaseProviderKind.Sqlite, sql);

            AssertSafetyFailurePreservesOriginal(sql, result);
            StringAssert.Contains(result.ErrorMessage, "Parameter ':id'");
        }

        [DataTestMethod]
        [DataRow(SqlFormatterKeywordCase.Upper, "select Id from Customer", "SELECT")]
        [DataRow(SqlFormatterKeywordCase.Lower, "SELECT Id FROM Customer", "select")]
        [DataRow(SqlFormatterKeywordCase.Preserve, "SeLeCt Id FrOm Customer", "SeLeCt")]
        public void Format_KeywordCase_MapsSupportedOption(SqlFormatterKeywordCase keywordCase, string sql, string expectedKeyword)
        {
            var options = new SqlFormatOptions(keywordCase: keywordCase);
            var result = Format(DatabaseProviderKind.SqlServer, sql, options);

            Assert.IsTrue(result.Success, result.ErrorMessage);
            StringAssert.Contains(result.FormattedSql, expectedKeyword);
        }

        [TestMethod]
        public void Format_ProperKeywordCase_ReturnsFailureAndPreservesOriginalSql()
        {
            const string sql = "SELECT Id FROM Customer";

            var options = new SqlFormatOptions(keywordCase: SqlFormatterKeywordCase.Proper);
            var result = Format(DatabaseProviderKind.SqlServer, sql, options);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(sql, result.FormattedSql);
            StringAssert.Contains(result.ErrorMessage, "does not support proper-case");
        }

        [TestMethod]
        public void Format_CustomIndentSize_UsesSpacesAndCrLf()
        {
            const string sql = "SELECT Id,Name FROM Customer";

            var options = new SqlFormatOptions(indentSize: 6);
            var result = Format(DatabaseProviderKind.SqlServer, sql, options);

            Assert.IsTrue(result.Success, result.ErrorMessage);
            StringAssert.Contains(result.FormattedSql, "\r\n      Id,");
            Assert.IsFalse(result.FormattedSql.Contains("\t"));
            Assert.IsFalse(result.FormattedSql.Replace("\r\n", string.Empty).Contains("\n"));
        }

        [TestMethod]
        public void Format_WhitespaceOnly_ReturnsNormalizedInputWithoutCallingFormatter()
        {
            var result = Format(DatabaseProviderKind.Sqlite, "\t\n");

            Assert.IsTrue(result.Success, result.ErrorMessage);
            Assert.AreEqual("    ", result.FormattedSql);
        }

        [TestMethod]
        public void Format_UnknownProvider_ReturnsFailureAndPreservesOriginalSql()
        {
            const string sql = "SELECT 1";

            var result = Format(DatabaseProviderKind.Unknown, sql);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(sql, result.FormattedSql);
            StringAssert.Contains(result.ErrorMessage, "does not support");
        }

        [TestMethod]
        public void Format_MismatchedEngineKind_ReturnsFailureAndPreservesOriginalSql()
        {
            const string sql = "SELECT 1";

            var request = new SqlFormatRequest
            (
                sql,
                DatabaseProviderKind.SqlServer,
                SqlFormatterEngineKind.MicrosoftScriptDom
            );

            var result = _engine.Format(request);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(sql, result.FormattedSql);
            StringAssert.Contains(result.ErrorMessage, "does not match");
        }

        [DataTestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        [DataRow(3)]
        [DataRow(4)]
        public void Format_SemicolonSeparatedStatements_UsesConfiguredBlankLineCount(int blankLines)
        {
            const string sql = "SELECT 1; SELECT 2;";

            var options = new SqlFormatOptions(linesBetweenStatements: blankLines + 1);
            var result = Format(DatabaseProviderKind.Sqlite, sql, options);

            Assert.IsTrue(result.Success, result.ErrorMessage);

            StringAssert.Contains
            (
                result.FormattedSql,
                "1;" + new string('\n', blankLines + 1).Replace("\n", "\r\n") + "SELECT"
            );
        }

        [DataTestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(3)]
        public void Format_RepeatedTopLevelStatementsWithoutSemicolon_UsesConfiguredBlankLineCount(int blankLines)
        {
            const string sql = "SELECT 1\r\nSELECT 2";

            var options = new SqlFormatOptions(linesBetweenStatements: blankLines + 1);
            var result = Format(DatabaseProviderKind.Oracle, sql, options);

            Assert.IsTrue(result.Success, result.ErrorMessage);

            StringAssert.Contains
            (
                result.FormattedSql,
                "1" + new string('\n', blankLines + 1).Replace("\n", "\r\n") + "SELECT"
            );
        }

        [TestMethod]
        public void Format_UnionSelect_DoesNotTreatSelectAsAnotherStatement()
        {
            const string sql = "SELECT 1\r\nUNION\r\nSELECT 2";

            var options = new SqlFormatOptions(linesBetweenStatements: 4);
            var result = Format(DatabaseProviderKind.Oracle, sql, options);

            Assert.IsTrue(result.Success, result.ErrorMessage);
            StringAssert.Contains(result.FormattedSql, "UNION\r\nSELECT");
            Assert.IsFalse(result.FormattedSql.Contains("UNION\r\n\r\n"));
        }

        [TestMethod]
        public void Format_SemicolonInsideString_DoesNotCreateStatementBoundary()
        {
            const string sql = "SELECT ';' AS VALUE; SELECT 2;";

            var options = new SqlFormatOptions(linesBetweenStatements: 3);
            var result = Format(DatabaseProviderKind.Oracle, sql, options);

            Assert.IsTrue(result.Success, result.ErrorMessage);
            StringAssert.Contains(result.FormattedSql, "';'");
            StringAssert.Contains(result.FormattedSql, "VALUE;\r\n\r\n\r\nSELECT");
        }

        private SqlFormatResult Format(DatabaseProviderKind providerKind, string sql, SqlFormatOptions options = null)
        {
            return _engine.Format
            (
                new SqlFormatRequest
                (
                    sql,
                    providerKind,
                    SqlFormatterEngineKind.Hogimn,
                    options
                )
            );
        }

        private static void AssertSafetyFailurePreservesOriginal(string originalSql, SqlFormatResult result)
        {
            Assert.IsFalse(result.Success);
            Assert.AreEqual(originalSql, result.FormattedSql);
            StringAssert.Contains(result.ErrorMessage, "failed token safety validation");
        }
    }
}
