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

        [TestMethod]
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

        [TestMethod]
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
            Assert.Contains(expectedFragment, result.FormattedSql);
            Assert.DoesNotContain("\t", result.FormattedSql);
        }

        [TestMethod]
        public void Format_PostgreSqlConcatenationCorruption_ReturnsOriginalSql()
        {
            const string sql = "SELECT '\"' || value || '\"' FROM sample";

            var result = Format(DatabaseProviderKind.PostgreSql, sql);

            AssertSafetyFailurePreservesOriginal(sql, result);
            Assert.Contains("Operator '||'", result.ErrorMessage);
        }

        [TestMethod]
        public void Format_PostgreSqlEscapeStringPrefixCorruption_ReturnsOriginalSql()
        {
            const string sql = "SELECT E'\\n' FROM sample";

            var result = Format(DatabaseProviderKind.PostgreSql, sql);

            AssertSafetyFailurePreservesOriginal(sql, result);
            Assert.Contains("StringLiteral", result.ErrorMessage);
        }

        [TestMethod]
        public void Format_PostgreSqlCommaMovedIntoLineComment_ReturnsOriginalSql()
        {
            const string sql = "SELECT first_value -- comment before comma\n, second_value FROM sample";

            var result = Format(DatabaseProviderKind.PostgreSql, sql);

            AssertSafetyFailurePreservesOriginal(sql, result);
            Assert.Contains("Comment", result.ErrorMessage);
        }

        [TestMethod]
        public void Format_PostgreSqlCommaBeforeLineComment_RemainsSafe()
        {
            const string sql = "SELECT first_value, -- keep comma active\nsecond_value FROM sample";

            var result = Format(DatabaseProviderKind.PostgreSql, sql);

            Assert.IsTrue(result.Success, result.ErrorMessage);
            Assert.Contains("first_value,", result.FormattedSql);
            Assert.Contains("-- keep comma active", result.FormattedSql);
        }

        [TestMethod]
        public void Format_OracleAlternativeQuotedConcatenation_RemainsSafe()
        {
            const string sql = "SELECT q'[Jason's SQL]' || employee_name FROM employee";

            var result = Format(DatabaseProviderKind.Oracle, sql);

            Assert.IsTrue(result.Success, result.ErrorMessage);
            Assert.Contains("q'[Jason's SQL]' || employee_name", result.FormattedSql);
        }

        [TestMethod]
        [DataRow("A")]
        [DataRow("a")]
        [DataRow("C")]
        [DataRow("c")]
        public void Format_OracleSingleLetterAlias_DoesNotInsertWhitespaceBeforeDot(string alias)
        {
            var sql = $"SELECT {alias}.OWNER,{alias}.TABLE_NAME FROM ALL_TAB_COLUMNS {alias} WHERE {alias}.OWNER=USER";

            var result = Format(DatabaseProviderKind.Oracle, sql);

            Assert.IsTrue(result.Success, result.ErrorMessage);
            Assert.Contains(alias + ".OWNER", result.FormattedSql);
            Assert.Contains(alias + ".TABLE_NAME", result.FormattedSql);
            Assert.DoesNotContain(alias + " .OWNER", result.FormattedSql);
            Assert.DoesNotContain(alias + " .TABLE_NAME", result.FormattedSql);
        }

        [TestMethod]
        public void Format_OracleQualifiedIdentifierTextInsideString_RemainsUntouched()
        {
            const string sql = "SELECT C.OWNER, 'A.OWNER', 'C.OWNER' FROM ALL_TAB_COLUMNS C WHERE C.OWNER = USER";

            var result = Format(DatabaseProviderKind.Oracle, sql);

            Assert.IsTrue(result.Success, result.ErrorMessage);
            Assert.Contains("C.OWNER", result.FormattedSql);
            Assert.Contains("'A.OWNER'", result.FormattedSql);
            Assert.Contains("'C.OWNER'", result.FormattedSql);
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
            Assert.Contains("SELECT DISTINCT C.OWNER,", result.FormattedSql);
            Assert.Contains("\r\n       C.TABLE_NAME", result.FormattedSql);
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
            Assert.Contains("\r\n  FROM ALL_TAB_COLUMNS C,\r\n       ALL_TABLES T", result.FormattedSql);
            Assert.Contains("\r\n WHERE T.OWNER = C.OWNER", result.FormattedSql);
            Assert.Contains("\r\n   AND T.TABLE_NAME = C.TABLE_NAME;", result.FormattedSql);
        }

        [TestMethod]
        public void Format_OracleJoinConditions_AlignsOnAndLogicalOperators()
        {
            const string sql = "SELECT C.OWNER,CC.COMMENTS FROM ALL_TAB_COLUMNS C LEFT JOIN ALL_COL_COMMENTS CC ON CC.OWNER=C.OWNER AND CC.TABLE_NAME=C.TABLE_NAME WHERE C.COLUMN_ID BETWEEN 1 AND 10 OR C.COLUMN_ID IS NULL;";

            var result = Format(DatabaseProviderKind.Oracle, sql);

            Assert.IsTrue(result.Success, result.ErrorMessage);
            Assert.Contains("\r\n  LEFT JOIN ALL_COL_COMMENTS CC", result.FormattedSql);
            Assert.Contains("\r\n    ON CC.OWNER = C.OWNER", result.FormattedSql);
            Assert.Contains("\r\n   AND CC.TABLE_NAME = C.TABLE_NAME", result.FormattedSql);
            Assert.Contains("\r\n WHERE C.COLUMN_ID BETWEEN 1 AND 10", result.FormattedSql);
            Assert.Contains("\r\n    OR C.COLUMN_ID IS NULL;", result.FormattedSql);
            Assert.DoesNotContain("BETWEEN 1\r\n   AND 10", result.FormattedSql);
        }

        [TestMethod]
        public void Format_OracleBetweenDateLiterals_DoesNotTreatRangeAndAsLogicalAnd()
        {
            const string sql = "SELECT C.OWNER FROM ALL_TAB_COLUMNS C WHERE C.CREATED_AT BETWEEN DATE '2026-01-01' AND DATE '2026-12-31' AND C.OWNER=USER;";

            var result = Format(DatabaseProviderKind.Oracle, sql);

            Assert.IsTrue(result.Success, result.ErrorMessage);
            Assert.Contains("C.CREATED_AT BETWEEN DATE '2026-01-01' AND DATE '2026-12-31'", result.FormattedSql);
            Assert.Contains("\r\n   AND C.OWNER = USER;", result.FormattedSql);
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
            Assert.Contains("SELECT NVL(C.DATA_DEFAULT, 'N/A') AS DEFAULT_VALUE,", result.FormattedSql);
            Assert.Contains("\r\n       C.COLUMN_NAME", result.FormattedSql);
            Assert.Contains("INSTR(C.COLUMN_NAME, ',') > 0;", result.FormattedSql);
        }

        [TestMethod]
        public void Format_OracleNestedSelect_UsesNestedQueryBaseIndent()
        {
            const string sql = "SELECT C.OWNER,(SELECT MAX(X.COLUMN_ID) FROM ALL_TAB_COLUMNS X WHERE X.OWNER=C.OWNER) AS MAX_COLUMN_ID FROM ALL_TAB_COLUMNS C;";

            var result = Format(DatabaseProviderKind.Oracle, sql);

            Assert.IsTrue(result.Success, result.ErrorMessage);
            Assert.Contains("SELECT C.OWNER,", result.FormattedSql);
            Assert.Contains("SELECT MAX(X.COLUMN_ID)", result.FormattedSql);
            Assert.Contains("  FROM ALL_TAB_COLUMNS X", result.FormattedSql);
            Assert.Contains(" WHERE X.OWNER = C.OWNER", result.FormattedSql);
            Assert.Contains("\r\n  FROM ALL_TAB_COLUMNS C;", result.FormattedSql);
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
            Assert.Contains("SELECT /*+ INDEX(C IDX_ALL_TAB_COLUMNS) */ C.OWNER,", result.FormattedSql);
            Assert.Contains("\r\n       C.TABLE_NAME", result.FormattedSql);
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
            Assert.Contains("-- selected columns\r\n       C.OWNER,", result.FormattedSql);
            Assert.Contains("\r\n       C.TABLE_NAME", result.FormattedSql);
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

        [TestMethod]
        [DataRow(1, "SELECT C.A,\r\n       C.B,\r\n       C.C,\r\n       C.D")]
        [DataRow(2, "SELECT C.A, C.B,\r\n       C.C, C.D")]
        [DataRow(3, "SELECT C.A, C.B, C.C,\r\n       C.D")]
        [DataRow(10, "SELECT C.A, C.B, C.C, C.D")]
        public void Format_Oracle_ListItemsPerLine_UsesConfiguredUpperBound(int itemsPerLine, string expectedSelect)
        {
            const string sql = "SELECT C.A,C.B,C.C,C.D FROM CUSTOMER C;";

            var options = new SqlFormatOptions(maxLineWidth: 1000, listItemsPerLine: itemsPerLine);
            var result = Format(DatabaseProviderKind.Oracle, sql, options);

            Assert.IsTrue(result.Success, result.ErrorMessage);
            Assert.Contains(expectedSelect, result.FormattedSql);
        }

        [TestMethod]
        public void Format_Oracle_ListItemsPerLine_WrapsBeforeCountWhenMaxWidthIsReached()
        {
            const string sql = "SELECT C.OWNER,C.TABLE_NAME FROM ALL_TAB_COLUMNS C;";

            var options = new SqlFormatOptions(maxLineWidth: 20, listItemsPerLine: 10);
            var result = Format(DatabaseProviderKind.Oracle, sql, options);

            Assert.IsTrue(result.Success, result.ErrorMessage);
            Assert.Contains("SELECT C.OWNER,\r\n       C.TABLE_NAME", result.FormattedSql);
        }

        [TestMethod]
        public void Format_Oracle_ListItemsPerLine_AppliesToGroupByAndOrderBy()
        {
            const string sql = "SELECT C.A,C.B,C.C FROM CUSTOMER C GROUP BY C.A,C.B,C.C ORDER BY C.A,C.B,C.C;";

            var options = new SqlFormatOptions(maxLineWidth: 1000, listItemsPerLine: 2);
            var result = Format(DatabaseProviderKind.Oracle, sql, options);

            Assert.IsTrue(result.Success, result.ErrorMessage);
            Assert.Contains("SELECT C.A, C.B,\r\n       C.C", result.FormattedSql);
            Assert.Contains(" GROUP BY C.A, C.B,\r\n          C.C", result.FormattedSql);
            Assert.Contains(" ORDER BY C.A, C.B,\r\n          C.C;", result.FormattedSql);
        }

        [TestMethod]
        public void Format_Oracle_ListItemsPerLine_DoesNotCountFunctionOrInListCommas()
        {
            const string sql = "SELECT NVL(C.A,C.B) AS VALUE,C.C FROM CUSTOMER C WHERE C.ID IN (1,2,3);";

            var options = new SqlFormatOptions(maxLineWidth: 1000, listItemsPerLine: 2);
            var result = Format(DatabaseProviderKind.Oracle, sql, options);

            Assert.IsTrue(result.Success, result.ErrorMessage);
            Assert.Contains("SELECT NVL(C.A, C.B) AS VALUE, C.C", result.FormattedSql);
            Assert.Contains("C.ID IN (1, 2, 3);", result.FormattedSql);
        }

        [TestMethod]
        public void Format_Oracle_ListItemsPerLine_AppliesIndependentlyInsideNestedSelect()
        {
            const string sql = "SELECT C.A,(SELECT X.A,X.B,X.C FROM CHILD X WHERE X.ID=C.ID) AS CHILD_VALUE FROM CUSTOMER C;";

            var options = new SqlFormatOptions(maxLineWidth: 1000, listItemsPerLine: 2);
            var result = Format(DatabaseProviderKind.Oracle, sql, options);

            Assert.IsTrue(result.Success, result.ErrorMessage);
            Assert.Contains("SELECT X.A, X.B,", result.FormattedSql);
            Assert.Contains("X.C", result.FormattedSql);
            Assert.DoesNotContain("SELECT X.A, X.B, X.C", result.FormattedSql);
        }

        [TestMethod]
        [DataRow(0)]
        [DataRow(11)]
        public void SqlFormatOptions_ListItemsPerLineOutsideRange_Throws(int itemsPerLine)
        {
            Assert.ThrowsExactly<System.ArgumentOutOfRangeException>
            (
                () => new SqlFormatOptions(listItemsPerLine: itemsPerLine)
            );
        }

        [TestMethod]
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
            Assert.Contains("SELECT\r\n", result.FormattedSql);
            Assert.Contains("\r\nFROM\r\n", result.FormattedSql);
            Assert.Contains("\r\nWHERE\r\n", result.FormattedSql);
            Assert.Contains("\r\nORDER BY\r\n", result.FormattedSql);
            Assert.Contains("C.ID, C.NAME", result.FormattedSql);
            Assert.DoesNotContain("SELECT C.ID", result.FormattedSql);
        }

        [TestMethod]
        public void Format_OracleLowerKeywordCase_KeepsStep4ClauseAlignment()
        {
            const string sql = "SELECT C.OWNER FROM ALL_TAB_COLUMNS C WHERE C.OWNER=USER ORDER BY C.OWNER;";

            var options = new SqlFormatOptions(keywordCase: SqlFormatterKeywordCase.Lower);
            var result = Format(DatabaseProviderKind.Oracle, sql, options);

            Assert.IsTrue(result.Success, result.ErrorMessage);
            Assert.Contains("select C.OWNER\r\n  from ALL_TAB_COLUMNS C", result.FormattedSql);
            Assert.Contains("\r\n where C.OWNER = user", result.FormattedSql);
            Assert.Contains("\r\n order by C.OWNER;", result.FormattedSql);
        }

        [TestMethod]
        public void Format_OracleLowerKeywordCase_AllowsUserKeywordCaseChange()
        {
            const string sql = "SELECT C.OWNER FROM ALL_TAB_COLUMNS C WHERE C.OWNER = USER";

            var options = new SqlFormatOptions(keywordCase: SqlFormatterKeywordCase.Lower);
            var result = Format(DatabaseProviderKind.Oracle, sql, options);

            Assert.IsTrue(result.Success, result.ErrorMessage);
            Assert.Contains("select", result.FormattedSql);
            Assert.Contains("user", result.FormattedSql);
            Assert.DoesNotContain("C .", result.FormattedSql);
        }

        [TestMethod]
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
            Assert.Contains(expectedFirstKeyword, result.FormattedSql);
            Assert.Contains(expectedSecondKeyword, result.FormattedSql);
        }

        [TestMethod]
        public void Format_OracleSameWordInKeywordAndQualifiedIdentifier_ChangesOnlyKeywordOccurrence()
        {
            const string sql = "SELECT USER, C.USER FROM CUSTOMER C WHERE C.USER = USER";

            var options = new SqlFormatOptions(keywordCase: SqlFormatterKeywordCase.Lower);
            var result = Format(DatabaseProviderKind.Oracle, sql, options);

            Assert.IsTrue(result.Success, result.ErrorMessage);
            Assert.Contains("select", result.FormattedSql);
            Assert.Contains("user,", result.FormattedSql);
            Assert.Contains("C.USER", result.FormattedSql);
            Assert.Contains("= user", result.FormattedSql);
            Assert.DoesNotContain("C.user", result.FormattedSql);
        }

        [TestMethod]
        public void Format_MultiWordKeyword_AuthorizesEveryContainedSemanticToken()
        {
            const string sql = "SELECT ID FROM CUSTOMER ORDER BY ID";

            var options = new SqlFormatOptions(keywordCase: SqlFormatterKeywordCase.Lower);
            var result = Format(DatabaseProviderKind.SqlServer, sql, options);

            Assert.IsTrue(result.Success, result.ErrorMessage);
            Assert.Contains("order by", result.FormattedSql);
        }

        [TestMethod]
        public void Format_SqliteNamedParameterCorruption_ReturnsOriginalSql()
        {
            const string sql = "SELECT first_name || ' ' || last_name FROM person WHERE id=:id";

            var result = Format(DatabaseProviderKind.Sqlite, sql);

            AssertSafetyFailurePreservesOriginal(sql, result);
            Assert.Contains("Parameter ':id'", result.ErrorMessage);
        }

        [TestMethod]
        [DataRow(SqlFormatterKeywordCase.Upper, "select Id from Customer", "SELECT")]
        [DataRow(SqlFormatterKeywordCase.Lower, "SELECT Id FROM Customer", "select")]
        [DataRow(SqlFormatterKeywordCase.Preserve, "SeLeCt Id FrOm Customer", "SeLeCt")]
        public void Format_KeywordCase_MapsSupportedOption(SqlFormatterKeywordCase keywordCase, string sql, string expectedKeyword)
        {
            var options = new SqlFormatOptions(keywordCase: keywordCase);
            var result = Format(DatabaseProviderKind.SqlServer, sql, options);

            Assert.IsTrue(result.Success, result.ErrorMessage);
            Assert.Contains(expectedKeyword, result.FormattedSql);
        }

        [TestMethod]
        public void Format_ProperKeywordCase_ReturnsFailureAndPreservesOriginalSql()
        {
            const string sql = "SELECT Id FROM Customer";

            var options = new SqlFormatOptions(keywordCase: SqlFormatterKeywordCase.Proper);
            var result = Format(DatabaseProviderKind.SqlServer, sql, options);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(sql, result.FormattedSql);
            Assert.Contains("does not support proper-case", result.ErrorMessage);
        }

        [TestMethod]
        public void Format_CustomIndentSize_UsesSpacesAndCrLf()
        {
            const string sql = "SELECT Id,Name FROM Customer";

            var options = new SqlFormatOptions(indentSize: 6);
            var result = Format(DatabaseProviderKind.SqlServer, sql, options);

            Assert.IsTrue(result.Success, result.ErrorMessage);
            Assert.Contains("\r\n      Id,", result.FormattedSql);
            Assert.DoesNotContain("\t", result.FormattedSql);
            Assert.DoesNotContain("\n", result.FormattedSql.Replace("\r\n", string.Empty));
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
            Assert.Contains("does not support", result.ErrorMessage);
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
            Assert.Contains("does not match", result.ErrorMessage);
        }

        [TestMethod]
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
            Assert.Contains("1;" + new string('\n', blankLines + 1).Replace("\n", "\r\n") + "SELECT", result.FormattedSql);
        }

        [TestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(3)]
        public void Format_RepeatedTopLevelStatementsWithoutSemicolon_UsesConfiguredBlankLineCount(int blankLines)
        {
            const string sql = "SELECT 1\r\nSELECT 2";

            var options = new SqlFormatOptions(linesBetweenStatements: blankLines + 1);
            var result = Format(DatabaseProviderKind.Oracle, sql, options);

            Assert.IsTrue(result.Success, result.ErrorMessage);
            Assert.Contains("1" + new string('\n', blankLines + 1).Replace("\n", "\r\n") + "SELECT", result.FormattedSql);
        }

        [TestMethod]
        public void Format_UnionSelect_DoesNotTreatSelectAsAnotherStatement()
        {
            const string sql = "SELECT 1\r\nUNION\r\nSELECT 2";

            var options = new SqlFormatOptions(linesBetweenStatements: 4);
            var result = Format(DatabaseProviderKind.Oracle, sql, options);

            Assert.IsTrue(result.Success, result.ErrorMessage);
            Assert.Contains("UNION\r\nSELECT", result.FormattedSql);
            Assert.DoesNotContain("UNION\r\n\r\n", result.FormattedSql);
        }

        [TestMethod]
        public void Format_SemicolonInsideString_DoesNotCreateStatementBoundary()
        {
            const string sql = "SELECT ';' AS VALUE; SELECT 2;";

            var options = new SqlFormatOptions(linesBetweenStatements: 3);
            var result = Format(DatabaseProviderKind.Oracle, sql, options);

            Assert.IsTrue(result.Success, result.ErrorMessage);
            Assert.Contains("';'", result.FormattedSql);
            Assert.Contains("VALUE;\r\n\r\n\r\nSELECT", result.FormattedSql);
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
            Assert.Contains("failed token safety validation", result.ErrorMessage);
        }
    }
}
