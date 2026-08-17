using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.DmlPreview;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonQuery.Tests.Core.Database.DmlPreview
{
    [TestClass]
    public sealed class DmlPreviewSqlBuilderTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        public void QuoteIdentifier_Oracle_ReturnsIdentifierUnchanged()
        {
            Assert.AreEqual("ABC", DmlPreviewSqlBuilder.QuoteIdentifier(DataSourceType.Oracle, "ABC"));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void QuoteIdentifier_PostgreSql_ReturnsIdentifierUnchanged()
        {
            Assert.AreEqual("countryinfo", DmlPreviewSqlBuilder.QuoteIdentifier(DataSourceType.PostgreSql, "countryinfo"));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void QuoteIdentifier_SqlServer_AddsBrackets()
        {
            Assert.AreEqual("[VendorID]", DmlPreviewSqlBuilder.QuoteIdentifier(DataSourceType.SqlServer, "VendorID"));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void QuoteIdentifier_SqlServer_PreservesExistingBrackets()
        {
            Assert.AreEqual("[VendorID]", DmlPreviewSqlBuilder.QuoteIdentifier(DataSourceType.SqlServer, "[VendorID]"));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void QuoteIdentifier_SqlServer_EscapesClosingBracket()
        {
            Assert.AreEqual("[A]]B]", DmlPreviewSqlBuilder.QuoteIdentifier(DataSourceType.SqlServer, "A]B"));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void QuoteIdentifier_MySql_AddsBackticks()
        {
            Assert.AreEqual("`city_id`", DmlPreviewSqlBuilder.QuoteIdentifier(DataSourceType.MySql, "city_id"));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void QuoteIdentifier_MySql_PreservesExistingBackticks()
        {
            Assert.AreEqual("`city_id`", DmlPreviewSqlBuilder.QuoteIdentifier(DataSourceType.MySql, "`city_id`"));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void QuoteIdentifier_MySql_EscapesBacktick()
        {
            Assert.AreEqual("`A``B`", DmlPreviewSqlBuilder.QuoteIdentifier(DataSourceType.MySql, "A`B"));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void BuildTableName_Oracle_ReturnsTableOnly()
        {
            Assert.AreEqual("ABC", DmlPreviewSqlBuilder.BuildTableName(DataSourceType.Oracle, string.Empty, "ABC"));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void BuildTableName_PostgreSql_ReturnsSchemaAndTable()
        {
            Assert.AreEqual("public.countryinfo", DmlPreviewSqlBuilder.BuildTableName(DataSourceType.PostgreSql, "public", "countryinfo"));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void BuildTableName_SqlServer_QuotesBothParts()
        {
            Assert.AreEqual("[MyDB].[dbo.TS_BoardRequest]", DmlPreviewSqlBuilder.BuildTableName(DataSourceType.SqlServer, "MyDB", "dbo.TS_BoardRequest"));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void BuildTableName_MySql_QuotesBothParts()
        {
            Assert.AreEqual("`sakila`.`city`", DmlPreviewSqlBuilder.BuildTableName(DataSourceType.MySql, "sakila", "city"));
        }

        [TestMethod]
        [TestCategory("Safety")]
        public void BuildInsert_WithEmptyTable_ReturnsEmpty()
        {
            Assert.AreEqual(string.Empty, DmlPreviewSqlBuilder.BuildInsert(string.Empty, "C1", "'a'", true));
        }

        [TestMethod]
        [TestCategory("Safety")]
        public void BuildInsert_WithEmptyFields_ReturnsEmpty()
        {
            Assert.AreEqual(string.Empty, DmlPreviewSqlBuilder.BuildInsert("ABC", string.Empty, "'a'", true));
        }

        [TestMethod]
        [TestCategory("Safety")]
        public void BuildInsert_WithEmptyValues_ReturnsEmpty()
        {
            Assert.AreEqual(string.Empty, DmlPreviewSqlBuilder.BuildInsert("ABC", "C1", string.Empty, true));
        }

        [TestMethod]
        [TestCategory("Safety")]
        public void BuildUpdate_WithEmptySetClause_ReturnsEmpty()
        {
            Assert.AreEqual(string.Empty, DmlPreviewSqlBuilder.BuildUpdate("ABC", string.Empty, "ID = 1", true));
        }

        [TestMethod]
        [TestCategory("Safety")]
        public void BuildUpdate_WithEmptyWhereCondition_ReturnsEmpty()
        {
            Assert.AreEqual(string.Empty, DmlPreviewSqlBuilder.BuildUpdate("ABC", "C1 = 'a'", string.Empty, true));
        }

        [TestMethod]
        [TestCategory("Safety")]
        public void BuildDelete_WithEmptyWhereCondition_ReturnsEmpty()
        {
            Assert.AreEqual(string.Empty, DmlPreviewSqlBuilder.BuildDelete("ABC", string.Empty, true));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void BuildUpdate_WithLowerCaseKeywords_ReturnsLowerCaseTemplate()
        {
            var actual = DmlPreviewSqlBuilder.BuildUpdate("abc", "c1 = 'a'", "id = 1", false);

            Assert.AreEqual("update abc\r\n"
                            + "   set c1 = 'a'\r\n"
                            + " where id = 1;",
                            actual);
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("Oracle")]
        public void BuildUpdate_OracleRowId_ReturnsExpectedSql()
        {
            AssertUpdate("AABBCC",
                         "COLUMN2 = '1231',\r\n"
                         + "       COLUMN3 = '0525'",
                         "ROWID = 'AAATUUAABAAAaefAAF'",
                         "UPDATE AABBCC\r\n"
                         + "   SET COLUMN2 = '1231',\r\n"
                         + "       COLUMN3 = '0525'\r\n"
                         + " WHERE ROWID = 'AAATUUAABAAAaefAAF';");
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("Oracle")]
        public void BuildUpdate_OracleTimestampAndRowId_ReturnsExpectedSql()
        {
            AssertUpdate("AABBCC",
                         "COLUMN4 = '15',\r\n"
                         + "       C5_TIMESTAMP3 = TIMESTAMP '2026-07-1907:43:15.123456'",
                         "ROWID = 'AAATUUAABAAAaefAAJ'",
                         "UPDATE AABBCC\r\n"
                         + "   SET COLUMN4 = '15',\r\n"
                         + "       C5_TIMESTAMP3 = TIMESTAMP '2026-07-1907:43:15.123456'\r\n"
                         + " WHERE ROWID = 'AAATUUAABAAAaefAAJ';");
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("Oracle")]
        public void BuildUpdate_OraclePrimaryKeyAndNull_ReturnsExpectedSql()
        {
            AssertUpdate("ABC",
                         "T2_NUMBER25_5 = NULL,\r\n"
                         + "       T3_VARCHAR2_NEW = '12''31'",
                         "T1_VARCHAR2_PK = 'cc1204ccc'",
                         "UPDATE ABC\r\n"
                         + "   SET T2_NUMBER25_5 = NULL,\r\n"
                         + "       T3_VARCHAR2_NEW = '12''31'\r\n"
                         + " WHERE T1_VARCHAR2_PK = 'cc1204ccc';");
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("Oracle")]
        public void BuildUpdate_OracleModifiedPrimaryKey_UsesOriginalKeyInWhere()
        {
            AssertUpdate("ABC",
                         "T1_VARCHAR2_PK = '5555'",
                         "T1_VARCHAR2_PK = '33335'",
                         "UPDATE ABC\r\n"
                         + "   SET T1_VARCHAR2_PK = '5555'\r\n"
                         + " WHERE T1_VARCHAR2_PK = '33335';");
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("Oracle")]
        public void BuildInsert_Oracle_ReturnsExpectedSql()
        {
            Assert.AreEqual("INSERT INTO ABC_TABLE5\r\n"
                            + "       (COLUMN1, COLUMN2, COLUMN3, COLUMN4, COLUMN5)\r\n"
                            + "VALUES ('a', 'b', 'c', '15b', '10a');",
                            DmlPreviewSqlBuilder.BuildInsert
                            (
                                "ABC_TABLE5",
                                "COLUMN1, COLUMN2, COLUMN3, COLUMN4, COLUMN5",
                                "'a', 'b', 'c', '15b', '10a'",
                                true)
                            );
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("Oracle")]
        public void BuildDelete_OracleRowId_ReturnsExpectedSql()
        {
            AssertDelete("ABC0722",
                         "ROWID = 'AAAU0lAABAAAaNhAAB'",
                         "DELETE FROM ABC0722\r\n"
                         + " WHERE ROWID = 'AAAU0lAABAAAaNhAAB';");
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("Oracle")]
        public void BuildDelete_OraclePrimaryKey_ReturnsExpectedSql()
        {
            AssertDelete("ABC",
                         "T1_VARCHAR2_PK = 'AABBCCDDEEFF'",
                         "DELETE FROM ABC\r\n"
                         + " WHERE T1_VARCHAR2_PK = 'AABBCCDDEEFF';");
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("PostgreSql")]
        public void BuildUpdate_PostgreSqlPrimaryKeyDate_ReturnsExpectedSql()
        {
            AssertUpdate("private.abc_bytea",
                         "t3_date = '2026/07/19'",
                         "t1_charvery = 'T1015'",
                         "UPDATE private.abc_bytea\r\n"
                         + "   SET t3_date = '2026/07/19'\r\n"
                         + " WHERE t1_charvery = 'T1015';");
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("PostgreSql")]
        public void BuildDelete_PostgreSqlPrimaryKey_ReturnsExpectedSql()
        {
            AssertDelete("private.abc_bytea",
                         "t1_charvery = 'T1015'",
                         "DELETE FROM private.abc_bytea\r\n"
                         + " WHERE t1_charvery = 'T1015';");
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("PostgreSql")]
        public void BuildInsert_PostgreSqlDefaults_ReturnsExpectedSql()
        {
            Assert.AreEqual("INSERT INTO public.countryinfo\r\n"
                            + "       (countryid, countrycode, countryname, createddate)\r\n"
                            + "VALUES (DEFAULT, 'JP', '日本', DEFAULT);",
                            DmlPreviewSqlBuilder.BuildInsert
                            (
                                "public.countryinfo",
                                "countryid, countrycode, countryname, createddate",
                                "DEFAULT, 'JP', '日本', DEFAULT",
                                true)
                            );
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("PostgreSql")]
        public void BuildDelete_PostgreSqlCountry_ReturnsExpectedSql()
        {
            AssertDelete("public.countryinfo",
                         "countryid = 1",
                         "DELETE FROM public.countryinfo\r\n"
                         + " WHERE countryid = 1;");
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("PostgreSql")]
        public void BuildUpdate_PostgreSqlEscapedStringAndNull_ReturnsExpectedSql()
        {
            AssertUpdate("public.countryinfo",
                         "countryname = '日本''Japan',\r\n"
                         + "       createddate = NULL",
                         "countryid = 1",
                         "UPDATE public.countryinfo\r\n"
                         + "   SET countryname = '日本''Japan',\r\n"
                         + "       createddate = NULL\r\n"
                         + " WHERE countryid = 1;");
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("PostgreSql")]
        public void BuildUpdate_PostgreSqlModifiedPrimaryKey_UsesOriginalKeyInWhere()
        {
            AssertUpdate("private.abc_bytea",
                         "t1_charvery = 'A1015'",
                         "t1_charvery = 'T1015'",
                         "UPDATE private.abc_bytea\r\n"
                         + "   SET t1_charvery = 'A1015'\r\n"
                         + " WHERE t1_charvery = 'T1015';");
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("PostgreSql")]
        public void BuildUpdate_PostgreSqlCtid_ReturnsExpectedSql()
        {
            AssertUpdate("public.custinfo",
                         "customercode = 'a1',\r\n"
                         + "       customername = 'a3'",
                         "CTID = '(0,6)'",
                         "UPDATE public.custinfo\r\n"
                         + "   SET customercode = 'a1',\r\n"
                         + "       customername = 'a3'\r\n"
                         + " WHERE CTID = '(0,6)';");
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("PostgreSql")]
        public void BuildDelete_PostgreSqlCtid_ReturnsExpectedSql()
        {
            AssertDelete("public.custinfo",
                         "CTID = '(0,8)'",
                         "DELETE FROM public.custinfo\r\n"
                         + " WHERE CTID = '(0,8)';");
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("PostgreSql")]
        public void BuildUpdate_PostgreSqlCtidEscapedStringAndNull_ReturnsExpectedSql()
        {
            AssertUpdate("public.custinfo",
                         "customercode = 'a''b''c',\r\n"
                         + "       email = NULL",
                         "CTID = '(0,12)'",
                         "UPDATE public.custinfo\r\n"
                         + "   SET customercode = 'a''b''c',\r\n"
                         + "       email = NULL\r\n"
                         + " WHERE CTID = '(0,12)';");
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("SqlServer")]
        public void BuildDelete_SqlServerCompositePrimaryKey_ReturnsExpectedSql()
        {
            AssertDelete("[MyDB].[dbo.TS_BoardRequest]",
                         "[BoardRequestID] = 113\r\n"
                         + "   AND [SampleInspectionID] = 1569\r\n"
                         + "   AND [StatusName] = 3",
                         "DELETE FROM [MyDB].[dbo.TS_BoardRequest]\r\n"
                         + " WHERE [BoardRequestID] = 113\r\n"
                         + "   AND [SampleInspectionID] = 1569\r\n"
                         + "   AND [StatusName] = 3;");
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("SqlServer")]
        public void BuildUpdate_SqlServerCompositePrimaryKey_ReturnsExpectedSql()
        {
            AssertUpdate("[MyDB].[dbo.TS_BoardRequest]",
                         "[SampleInspectionID] = 1666,\r\n"
                         + "       [StatusName] = 2,\r\n"
                         + "       [CreatePerson] = N'Winnie',\r\n"
                         + "       [CreateDep] = NULL",
                         "[BoardRequestID] = 124\r\n"
                         + "   AND [SampleInspectionID] = 1667\r\n"
                         + "   AND [StatusName] = 3",
                         "UPDATE [MyDB].[dbo.TS_BoardRequest]\r\n"
                         + "   SET [SampleInspectionID] = 1666,\r\n"
                         + "       [StatusName] = 2,\r\n"
                         + "       [CreatePerson] = N'Winnie',\r\n"
                         + "       [CreateDep] = NULL\r\n"
                         + " WHERE [BoardRequestID] = 124\r\n"
                         + "   AND [SampleInspectionID] = 1667\r\n"
                         + "   AND [StatusName] = 3;");
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("SqlServer")]
        public void BuildInsert_SqlServer_ReturnsExpectedSql()
        {
            Assert.AreEqual("INSERT INTO [MyDB].[dbo.TS_BoardRequest]\r\n"
                            + "       ([BoardRequestID], [SampleInspectionID], [StatusName], [CreatePerson], [CreateDep], [CreateTime], [Applicant], [ApplyTime], [Reviewer], [ReviewTime], [TestLocation], [ReceiveStatus], [IncludeTestLocation])\r\n"
                            + "VALUES (1, 2, 3, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL);",
                            DmlPreviewSqlBuilder.BuildInsert
                            (
                                "[MyDB].[dbo.TS_BoardRequest]",
                                "[BoardRequestID], [SampleInspectionID], [StatusName], [CreatePerson], [CreateDep], [CreateTime], [Applicant], [ApplyTime], [Reviewer], [ReviewTime], [TestLocation], [ReceiveStatus], [IncludeTestLocation]",
                                "1, 2, 3, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL",
                                true)
                            );
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("SqlServer")]
        public void BuildUpdate_SqlServerNStringAndBit_ReturnsExpectedSql()
        {
            AssertUpdate("[MyDB].[dbo.Vendors]",
                         "[VendorName] = N'Taipei''1015',\r\n"
                         + "       [IsActive] = 0",
                         "[VendorID] = 1",
                         "UPDATE [MyDB].[dbo.Vendors]\r\n"
                         + "   SET [VendorName] = N'Taipei''1015',\r\n"
                         + "       [IsActive] = 0\r\n"
                         + " WHERE [VendorID] = 1;");
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("SqlServer")]
        public void BuildInsert_SqlServerNoPrimaryKey_ReturnsExpectedSql()
        {
            Assert.AreEqual("INSERT INTO [MyDB].[dbo.C123]\r\n"
                            + "       ([C1], [C2], [C3])\r\n"
                            + "VALUES (N'a', N'b', N'c');",
                            DmlPreviewSqlBuilder.BuildInsert
                            (
                                "[MyDB].[dbo.C123]",
                                "[C1], [C2], [C3]",
                                "N'a', N'b', N'c'",
                                true)
                            );
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("MySql")]
        public void BuildDelete_MySqlPrimaryKey_ReturnsExpectedSql()
        {
            AssertDelete("`sakila`.`city`",
                         "`city_id` = 6",
                         "DELETE FROM `sakila`.`city`\r\n"
                         + " WHERE `city_id` = 6;");
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("MySql")]
        public void BuildUpdate_MySqlEscapedString_ReturnsExpectedSql()
        {
            AssertUpdate("`sakila`.`city`",
                         "`city` = 'Adoni''r'",
                         "`city_id` = 8",
                         "UPDATE `sakila`.`city`\r\n"
                         + "   SET `city` = 'Adoni''r'\r\n"
                         + " WHERE `city_id` = 8;");
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("MySql")]
        public void BuildUpdate_MySqlCompositePrimaryKey_ReturnsExpectedSql()
        {
            AssertUpdate("`sakila`.`film_actor`",
                         "`actor_id` = 2,\r\n"
                         + "       `film_id` = 666",
                         "`actor_id` = 1\r\n"
                         + "   AND `film_id` = 166",
                         "UPDATE `sakila`.`film_actor`\r\n"
                         + "   SET `actor_id` = 2,\r\n"
                         + "       `film_id` = 666\r\n"
                         + " WHERE `actor_id` = 1\r\n"
                         + "   AND `film_id` = 166;");
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("MySql")]
        public void BuildUpdate_MySqlNull_ReturnsExpectedSql()
        {
            AssertUpdate("`sakila`.`payment`",
                         "`customer_id` = 3,\r\n"
                         + "       `staff_id` = 3,\r\n"
                         + "       `rental_id` = NULL",
                         "`payment_id` = 8",
                         "UPDATE `sakila`.`payment`\r\n"
                         + "   SET `customer_id` = 3,\r\n"
                         + "       `staff_id` = 3,\r\n"
                         + "       `rental_id` = NULL\r\n"
                         + " WHERE `payment_id` = 8;");
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("MySql")]
        public void BuildInsert_MySqlNoPrimaryKey_ReturnsExpectedSql()
        {
            Assert.AreEqual("INSERT INTO `sakila`.`abc_enum_char_set`\r\n"
                            + "       (`a1_char0`)\r\n"
                            + "VALUES ('a');",
                            DmlPreviewSqlBuilder.BuildInsert
                            (
                                "`sakila`.`abc_enum_char_set`",
                                "`a1_char0`",
                                "'a'",
                                true)
                            );
        }

        private static void AssertUpdate(string tableName, string setClause, string whereCondition, string expected)
        {
            Assert.AreEqual(expected, DmlPreviewSqlBuilder.BuildUpdate(tableName, setClause, whereCondition, true));
        }

        private static void AssertDelete(string tableName, string whereCondition, string expected)
        {
            Assert.AreEqual(expected, DmlPreviewSqlBuilder.BuildDelete(tableName, whereCondition, true));
        }
    }
}