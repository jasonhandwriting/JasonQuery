using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.Execution;
using JasonQuery.Core.SchemaExplorer.Selection;
using JasonQuery.UI.LeftPanel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Data;
using System.Reflection;

namespace JasonQuery.Tests.Core.SchemaExplorer.Selection
{
    [TestClass]
    public sealed class OracleOwnerRowProvenanceTests
    {
        private static object InvokeInternal(string typeName, string methodName, params object[] parameters)
        {
            var type = typeof(DatabaseSqlExecutor).Assembly.GetType(typeName, true);
            var method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
            Assert.IsNotNull(method, typeName + "." + methodName);
            return method.Invoke(null, parameters);
        }

        [TestMethod]
        [TestCategory("D1B3F02B2G1")]
        public void F02B2G1_TableInfoSql_ProjectsExactDictionaryOwner()
        {
            var sql = (string)InvokeInternal("JasonQuery.Core.Database.SqlBuilder.Oracle.Metadata.Table.OracleTableSqlBuilder", "BuildGetTableInfoSql", "SYS", false);
            Assert.Contains("c.Owner AS OracleMetadataOwner", sql);
        }

        [TestMethod]
        [TestCategory("D1B3F02B2G1")]
        public void F02B2G1_ViewInfoSql_ProjectsExactDictionaryOwner()
        {
            var sql = (string)InvokeInternal("JasonQuery.Core.Database.SqlBuilder.Oracle.Metadata.View.OracleViewSqlBuilder", "BuildGetViewInfoSql", "SYS");
            Assert.Contains("o.Owner AS OracleMetadataOwner", sql);
        }

        [TestMethod]
        [TestCategory("D1B3F02B2G1")]
        public void F02B2G1_ViewColumnSql_ProjectsExactDictionaryOwner()
        {
            var sql = (string)InvokeInternal("JasonQuery.Core.Database.SqlBuilder.Oracle.Metadata.View.OracleViewSqlBuilder", "BuildGetViewColumnInfoSql", "SYS");
            Assert.Contains("c.Owner AS OracleMetadataOwner", sql);
        }


        [TestMethod]
        [TestCategory("D1B3F02B2G1")]
        public void F02B2G1_ViewColumnMap_SeparatesSameNamedViewsAcrossOwners()
        {
            var rows = new DataTable();
            rows.Columns.Add("ViewName");
            rows.Columns.Add("OracleMetadataOwner");
            rows.Rows.Add("V_REPORT", "MIXED");
            rows.Rows.Add("V_REPORT", "MiXeD");
            var type = typeof(DatabaseSqlExecutor).Assembly.GetType("JasonQuery.Core.Database.Metadata.Oracle.View.OracleViewMetadataOrganizer", true);
            var method = type.GetMethod("BuildViewColumnInfoMap", BindingFlags.Static | BindingFlags.NonPublic);
            Assert.IsNotNull(method);
            var map = (IDictionary)method.Invoke(null, new object[] { rows });
            Assert.AreEqual(2, map.Count);
        }

        private static DataTable CreateTable()
        {
            var table = new DataTable();
            table.Columns.Add("SchemaObject");
            table.Columns.Add("SchemaType");
            table.Columns.Add("SchemaName");
            table.Columns.Add("ColumnInfo");
            table.Columns.Add("OracleMetadataOwner");
            return table;
        }

        [TestMethod]
        [TestCategory("D1B3F02B2G1")]
        public void F02B2G1_TableRowBuilder_PropagatesSysOwner()
        {
            var row = (DataRow)InvokeInternal("JasonQuery.Core.Database.Metadata.Oracle.Table.OracleTableSchemaRowBuilder", "Build", CreateTable(), "local", "Tables", "AABBCC", "COL_A, NUMBER", "SYS");
            Assert.AreEqual("SYS", row["OracleMetadataOwner"]);
        }

        [TestMethod]
        [TestCategory("D1B3F02B2G1")]
        public void F02B2G1_TableRowBuilder_PreservesMixedCaseOwner()
        {
            var row = (DataRow)InvokeInternal("JasonQuery.Core.Database.Metadata.Oracle.Table.OracleTableSchemaRowBuilder", "Build", CreateTable(), "local", "Tables", "AABBCC", "COL_A, NUMBER", "MiXeD");
            Assert.AreEqual("MiXeD", row["OracleMetadataOwner"]);
        }

        [TestMethod]
        [TestCategory("D1B3F02B2G1")]
        public void F02B2G1_OracleSnapshot_HidesInternalOwnerField()
        {
            var table = CreateTable();
            table.Rows.Add("local", "Tables", "AABBCC", "COL_A, NUMBER", "MiXeD");
            SharedSchemaMetadataSnapshot snapshot;
            Assert.IsTrue(SharedSchemaMetadataSnapshotPolicy.TryCapture(table, DataSourceType.Oracle, false, 1, out snapshot));
            Assert.IsTrue(Array.IndexOf(snapshot.HiddenColumns, "OracleMetadataOwner") >= 0);
        }

        [TestMethod]
        [TestCategory("D1B3F02B2G1")]
        public void F02B2G1_OracleSnapshot_CopyRetainsExactOwnerAndDoesNotMutateSource()
        {
            var table = CreateTable();
            table.Rows.Add("local", "Tables", "AABBCC", "COL_A, NUMBER", "MiXeD");
            SharedSchemaMetadataSnapshot snapshot;
            Assert.IsTrue(SharedSchemaMetadataSnapshotPolicy.TryCapture(table, DataSourceType.Oracle, false, 1, out snapshot));
            Assert.AreEqual("MiXeD", snapshot.Table.Rows[0]["OracleMetadataOwner"]);
            Assert.AreEqual("MiXeD", table.Rows[0]["OracleMetadataOwner"]);
            Assert.AreNotSame(table, snapshot.Table);
        }
    }
}
