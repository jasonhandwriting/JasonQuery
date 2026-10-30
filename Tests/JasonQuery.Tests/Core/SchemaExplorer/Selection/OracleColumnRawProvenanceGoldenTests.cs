using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.Execution;
using JasonQuery.UI.LeftPanel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;
using System.Reflection;

namespace JasonQuery.Tests.Core.SchemaExplorer.Selection
{
    [TestClass]
    public sealed class OracleColumnRawProvenanceGoldenTests
    {
        private static Type RowBuilder()
        {
            var type = typeof(DatabaseSqlExecutor).Assembly.GetType("JasonQuery.Core.Database.Metadata.Oracle.Table.OracleTableSchemaRowBuilder");

            Assert.IsNotNull(type);
            return type;
        }

        private static DataTable RawSchema()
        {
            var table = new DataTable();

            table.Columns.Add("SchemaObject", typeof(string));
            table.Columns.Add("SchemaType", typeof(string));
            table.Columns.Add("SchemaName", typeof(string));
            table.Columns.Add("ColumnInfo", typeof(string));
            table.Columns.Add("OracleMetadataOwner", typeof(string));
            table.Columns.Add("OracleMetadataTableName", typeof(string));
            table.Columns.Add("OracleMetadataColumnName", typeof(string));

            return table;
        }

        private static DataRow BuildRaw(DataTable table, string owner, string rawTable, string rawColumn)
        {
            var method = RowBuilder().GetMethod("BuildWithRawIdentifiers", BindingFlags.Static | BindingFlags.Public);

            Assert.IsNotNull(method, "Missing B3-B2-C1 raw dictionary identifier propagation");

            return (DataRow)method.Invoke(null, new object[]
            {
                table, "Oracle", "Tables", "DISPLAY_TABLE(11)", "COLUMN, VARCHAR2(50)",
                owner, rawTable, rawColumn
            });
        }

        [TestMethod, TestCategory("F02B3B2C1")]
        public void G01_OracleMetadataQueryAlreadyProjectsRawTableAndColumnNames()
        {
            var type = typeof(DatabaseSqlExecutor).Assembly.GetType("JasonQuery.Core.Database.SqlBuilder.Oracle.Metadata.Table.OracleTableSqlBuilder");

            Assert.IsNotNull(type);

            var method = type.GetMethod("BuildGetTableInfoSql", BindingFlags.Static | BindingFlags.Public);

            Assert.IsNotNull(method);

            var sql = (string)method.Invoke(null, new object[] { "SYS", false });

            Assert.Contains("c.Table_Name AS TableName", sql);
            Assert.Contains("c.Column_Name AS ColumnName", sql);
            Assert.Contains("c.Owner AS OracleMetadataOwner", sql);
        }

        [TestMethod, TestCategory("F02B3B2C1")]
        public void G02_LegacySixArgumentRowBuilderRemainsCompatible()
        {
            var method = RowBuilder().GetMethod("Build", BindingFlags.Static | BindingFlags.Public);

            Assert.IsNotNull(method);

            var row = (DataRow)method.Invoke(null, new object[]
            {
                RawSchema(), "Oracle", "Tables", "ABC(11)", "COL, NUMBER", "SYS"
            });

            Assert.AreEqual("SYS", row["OracleMetadataOwner"]);
        }

        [TestMethod, TestCategory("F02B3B2C1")]
        public void G03_SnapshotCopiesRawColumnsWithoutModifyingSource()
        {
            var source = RawSchema();

            source.Rows.Add("Oracle", "Tables", "ABC(11)", "ColumnName, NUMBER", "MiXeD", "ABC", "ColumnName");

            SharedSchemaMetadataSnapshot snapshot;

            Assert.IsTrue(SharedSchemaMetadataSnapshotPolicy.TryCapture(source, DataSourceType.Oracle, true, 1, out snapshot, "MiXeD", Guid.NewGuid()));
            Assert.AreNotSame(source, snapshot.Table);
            Assert.AreEqual("ColumnName", snapshot.Table.Rows[0]["OracleMetadataColumnName"]);
        }

        [TestMethod, TestCategory("F02B3B2C1")]
        public void G04_NonOracleSnapshotDoesNotHideOracleIdentifiers()
        {
            var source = RawSchema();

            source.Columns.Add("SchemaNode", typeof(string));

            SharedSchemaMetadataSnapshot snapshot;

            Assert.IsTrue(SharedSchemaMetadataSnapshotPolicy.TryCapture(source, DataSourceType.PostgreSql, false, 1, out snapshot));
            Assert.IsFalse(Array.Exists(snapshot.HiddenColumns, name => name == "OracleMetadataColumnName"));
        }

        [TestMethod, TestCategory("F02B3B2C1")]
        public void G05_RawTableAndColumnFieldsExistInNewRowBuilder()
        {
            var row = BuildRaw(RawSchema(), "SYS", "ABC", "RAW_COLUMN");

            Assert.AreEqual("ABC", row["OracleMetadataTableName"]);
            Assert.AreEqual("RAW_COLUMN", row["OracleMetadataColumnName"]);
        }

        [TestMethod, TestCategory("F02B3B2C1")]
        public void G06_MixedCaseDictionaryIdentifiersRemainExact()
        {
            var row = BuildRaw(RawSchema(), "MiXeD", "MixedTable", "MiXeD_Col");

            Assert.AreEqual("MiXeD", row["OracleMetadataOwner"]);
            Assert.AreEqual("MixedTable", row["OracleMetadataTableName"]);
            Assert.AreEqual("MiXeD_Col", row["OracleMetadataColumnName"]);
        }

        [TestMethod, TestCategory("F02B3B2C1")]
        public void G07_QuotedNamePunctuationIsNotParsedFromColumnInfo()
        {
            var row = BuildRaw(RawSchema(), "SYS", "Table, One", "A, B\"C");

            Assert.AreEqual("Table, One", row["OracleMetadataTableName"]);
            Assert.AreEqual("A, B\"C", row["OracleMetadataColumnName"]);
        }

        [TestMethod, TestCategory("F02B3B2C1")]
        public void G08_OracleSnapshotHidesRawIdentifiersFromGrid()
        {
            var source = RawSchema();

            source.Rows.Add("Oracle", "Tables", "ABC(11)", "COL, NUMBER", "SYS", "ABC", "COL");

            SharedSchemaMetadataSnapshot snapshot;

            Assert.IsTrue(SharedSchemaMetadataSnapshotPolicy.TryCapture(source, DataSourceType.Oracle, true, 1, out snapshot, "SYS", Guid.NewGuid()));

            CollectionAssert.Contains(snapshot.HiddenColumns, "OracleMetadataOwner");
            CollectionAssert.Contains(snapshot.HiddenColumns, "OracleMetadataTableName");
            CollectionAssert.Contains(snapshot.HiddenColumns, "OracleMetadataColumnName");
        }
    }
}
