using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.DdlPreview;
using JasonQuery.UI.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;

namespace JasonQuery.Tests.Core.Database.DdlPreview
{
    //F7-U2-C98: pure policy + opt-in contract. Never connect or execute DDL.
    [TestClass]
    [TestCategory("F7U2C98")]
    public sealed class NonOracleDdlFormIntentGoldenTests
    {
        private const string Sql = "ALTER TABLE \"public\".\"CaseTable\" ADD COLUMN \"New\" integer NULL;";

        private static string Verify
        (
            DataSourceType provider,
            string expectedDb = "TestDb",
            string expectedSchema = "public",
            string expectedTable = "CaseTable",
            string formDb = "TestDb",
            string formSchema = "public",
            string formTable = "CaseTable",
            string currentPostgreSqlDb = "TestDb",
            string preview = Sql,
            string rebuilt = Sql
        )
        {
            var type = typeof(ColumnDdlRequest).Assembly.GetType
            (
                "JasonQuery.Core.Database.DdlPreview.NonOracleSchemaExplorerDdlExecuteGate",
                true
            );

            var method = type.GetMethod("ValidateFormIntent", BindingFlags.Public | BindingFlags.Static);

            Assert.IsNotNull(method);

            return (string)method.Invoke
            (
                null,
                new object[]
                {
                    provider, expectedDb, expectedSchema, expectedTable,
                    formDb, formSchema, formTable, currentPostgreSqlDb, preview, rebuilt
                }
            );
        }

        [TestMethod]
        public void C9801_PostgreSqlSameDatabaseSchemaTableAndSqlPasses()
        {
            Assert.AreEqual(string.Empty, Verify(DataSourceType.PostgreSql, formDb: string.Empty));
        }

        [TestMethod]
        public void C9802_PostgreSqlDifferentLiveDatabaseRejects()
        {
            Assert.AreEqual("F7U2C2_DATABASE_MISMATCH", Verify(DataSourceType.PostgreSql, formDb: string.Empty, currentPostgreSqlDb: "OtherDb"));
        }

        [TestMethod]
        public void C9803_SqlServerExactFormTargetPassesWithoutObjectIdRequirement()
        {
            Assert.AreEqual(string.Empty, Verify(DataSourceType.SqlServer, expectedSchema: "dbo", formSchema: "dbo"));
        }

        [TestMethod]
        public void C9804_SqlServerChangedDatabaseOrSchemaRejects()
        {
            Assert.AreEqual("F7U2C2_DATABASE_MISMATCH", Verify(DataSourceType.SqlServer, formDb: "WrongDb"));
            Assert.AreEqual("F7U2C2_SCHEMA_MISMATCH", Verify(DataSourceType.SqlServer, expectedSchema: "dbo", formSchema: "audit"));
        }

        [TestMethod]
        public void C9805_MySqlChangedDatabaseOrTableRejects()
        {
            Assert.AreEqual("F7U2C2_DATABASE_MISMATCH", Verify(DataSourceType.MySql, expectedSchema: string.Empty, formSchema: string.Empty, formDb: "OtherDb"));
            Assert.AreEqual("F7U2C2_TABLE_MISMATCH", Verify(DataSourceType.MySql, expectedSchema: string.Empty, formSchema: string.Empty, formTable: "AnotherTable"));
        }

        [TestMethod]
        public void C9806_AnyChangedOrEmptySqlRejects()
        {
            Assert.AreEqual("F7U2C2_SQL_CHANGED", Verify(DataSourceType.PostgreSql, preview: Sql + " "));
            Assert.AreEqual("F7U2C2_SQL_EMPTY", Verify(DataSourceType.MySql, expectedSchema: string.Empty, formSchema: string.Empty, preview: " "));
        }

        [TestMethod]
        public void C9807_FormRequiresExplicitExecuteTimeGateOptIn()
        {
            var property = typeof(EditColumnForm).GetProperty
            (
                "RequireNonOracleF7ExecuteGate", BindingFlags.Instance | BindingFlags.NonPublic
            );

            Assert.IsNotNull(property);
            Assert.AreEqual(typeof(bool), property.PropertyType);
            Assert.IsNotNull(property.GetGetMethod(true));
            Assert.IsNotNull(property.GetSetMethod(true));
        }
    }
}
