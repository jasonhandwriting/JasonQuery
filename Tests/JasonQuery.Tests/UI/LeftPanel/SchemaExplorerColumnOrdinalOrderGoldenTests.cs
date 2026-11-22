using JasonQuery.Core.Database.SqlBuilder.MySql.Metadata.Table;
using JasonQuery.Core.Database.SqlBuilder.Oracle.Metadata.Table;
using JasonQuery.Core.Database.SqlBuilder.PostgreSql.Metadata.Table;
using JasonQuery.Core.Database.SqlBuilder.SqlServer.Metadata.Table;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace JasonQuery.Tests.UI.LeftPanel
{
    [TestClass]
    [TestCategory("F7U4CA2")]
    public sealed class SchemaExplorerColumnOrdinalOrderGoldenTests
    {
        [TestMethod]
        public void G01_Oracle_UsesColumnIdOrder()
        {
            var sql = OracleTableSqlBuilder.BuildGetTableInfoSql("JQ_OWNER", false);

            Assert.IsTrue(sql.EndsWith("ORDER BY c.Table_Name, c.Column_ID", StringComparison.Ordinal));
        }

        [TestMethod]
        public void G02_PostgreSql_UsesAttributeNumberOrder()
        {
            var sql = PostgreSqlTableSchemaMetadataSqlBuilder.Build(false);

            Assert.IsTrue(sql.EndsWith("ORDER BY t.DbName, t.TableName, a.attnum", StringComparison.Ordinal));
        }

        [TestMethod]
        public void G03_SqlServer_UsesOrdinalPositionOrder()
        {
            var sql = new SqlServerTableColumnMetadataSqlBuilder().Build("JQ_TEST", "Ordinal_Position;");

            Assert.IsTrue(sql.IndexOf("ORDER BY o.Table_Name, Ordinal_Position", StringComparison.Ordinal) >= 0);
        }

        [TestMethod]
        public void G04_MySql_UsesOrdinalPositionOrder()
        {
            var sql = MySqlTableColumnMetadataSqlBuilder.Build("'jq_test'", false);

            Assert.IsTrue(sql.EndsWith("ORDER BY Table_Schema, Table_Name, Ordinal_Position", StringComparison.Ordinal));
        }
    }
}
