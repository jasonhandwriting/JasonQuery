using JasonLibrary.Core.Schema;
using JasonLibrary.Core.Schema.Enums;
using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.DmlPreview;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;

namespace JasonQuery.Tests.Core.Database.DmlPreview
{
    [TestClass]
    public sealed class DmlPreviewWhereConditionSafetyRegressionTests
    {
        private const string RowIdentityColumn = "R_O_W_1_D_P_K_J_Q";

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("Oracle")]
        public void BuildUpdate_OracleModifiedPrimaryKey_UsesOriginalPrimaryKeyInWhere()
        {
            var rows = CreateRows
            (
                new[] { "T1_VARCHAR2_PK" },
                new object[] { "5555" },
                new object[] { "33335" }
            );

            var whereResult = BuildWhere
            (
                DataSourceType.Oracle,
                rows,
                true,
                Key("T1_VARCHAR2_PK", 1, StringColumn("T1_VARCHAR2_PK"))
            );

            var sql = DmlPreviewSqlBuilder.BuildUpdate("ABC", "T1_VARCHAR2_PK = '5555'", whereResult.Condition, true);

            Assert.AreEqual("UPDATE ABC\r\n"
                            + "   SET T1_VARCHAR2_PK = '5555'\r\n"
                            + " WHERE T1_VARCHAR2_PK = '33335';", sql);
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("PostgreSql")]
        public void BuildUpdate_PostgreSqlModifiedPrimaryKey_UsesOriginalPrimaryKeyInWhere()
        {
            var rows = CreateRows
            (
                new[] { "t1_charvery" },
                new object[] { "A1015" },
                new object[] { "T1015" }
            );

            var whereResult = BuildWhere
            (
                DataSourceType.PostgreSql,
                rows,
                true,
                Key("t1_charvery", 1, StringColumn("t1_charvery"))
            );

            var sql = DmlPreviewSqlBuilder.BuildUpdate("private.abc_bytea", "t1_charvery = 'A1015'", whereResult.Condition, true);

            Assert.AreEqual("UPDATE private.abc_bytea\r\n"
                            + "   SET t1_charvery = 'A1015'\r\n"
                            + " WHERE t1_charvery = 'T1015';", sql);
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("SqlServer")]
        public void BuildDelete_SqlServerCompositePrimaryKey_ContainsAllKeysInOrdinalOrder()
        {
            var rows = CreateRows
            (
                new[]
                {
                    "BoardRequestID",
                    "SampleInspectionID",
                    "StatusName"
                },
                new object[] { 113, 1569, 3 },
                new object[] { 113, 1569, 3 }
            );

            var whereResult = BuildWhere
            (
                DataSourceType.SqlServer,
                rows,
                true,
                Key("StatusName", 3, NumberColumn("StatusName")),
                Key("BoardRequestID", 1, NumberColumn("BoardRequestID")),
                Key("SampleInspectionID", 2, NumberColumn("SampleInspectionID"))
            );

            var sql = DmlPreviewSqlBuilder.BuildDelete("[MyDB].[dbo.TS_BoardRequest]", whereResult.Condition, true);

            Assert.AreEqual("DELETE FROM [MyDB].[dbo.TS_BoardRequest]\r\n"
                            + " WHERE [BoardRequestID] = 113\r\n"
                            + "   AND [SampleInspectionID] = 1569\r\n"
                            + "   AND [StatusName] = 3;", sql);
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("SqlServer")]
        public void BuildUpdate_SqlServerCompositePrimaryKey_UsesOriginalValues()
        {
            var rows = CreateRows
            (
                new[]
                {
                    "BoardRequestID",
                    "SampleInspectionID",
                    "StatusName"
                },
                new object[] { 124, 1666, 2 },
                new object[] { 124, 1667, 3 }
            );

            var whereResult = BuildWhere
            (
                DataSourceType.SqlServer,
                rows,
                true,
                Key("BoardRequestID", 1, NumberColumn("BoardRequestID")),
                Key("SampleInspectionID", 2, NumberColumn("SampleInspectionID")),
                Key("StatusName", 3, NumberColumn("StatusName"))
            );

            var sql = DmlPreviewSqlBuilder.BuildUpdate("[MyDB].[dbo.TS_BoardRequest]", "[SampleInspectionID] = 1666,\r\n" + "       [StatusName] = 2", whereResult.Condition, true);

            Assert.AreEqual("UPDATE [MyDB].[dbo.TS_BoardRequest]\r\n"
                            + "   SET [SampleInspectionID] = 1666,\r\n"
                            + "       [StatusName] = 2\r\n"
                            + " WHERE [BoardRequestID] = 124\r\n"
                            + "   AND [SampleInspectionID] = 1667\r\n"
                            + "   AND [StatusName] = 3;", sql);
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("MySql")]
        public void BuildUpdate_MySqlCompositePrimaryKey_UsesOriginalValues()
        {
            var rows = CreateRows
            (
                new[] { "actor_id", "film_id" },
                new object[] { 2, 666 },
                new object[] { 1, 166 }
            );

            var whereResult = BuildWhere
            (
                DataSourceType.MySql,
                rows,
                true,
                Key("actor_id", 1, NumberColumn("actor_id")),
                Key("film_id", 2, NumberColumn("film_id"))
            );

            var sql = DmlPreviewSqlBuilder.BuildUpdate("`sakila`.`film_actor`", "`actor_id` = 2,\r\n" + "       `film_id` = 666", whereResult.Condition, true);

            Assert.AreEqual("UPDATE `sakila`.`film_actor`\r\n"
                            + "   SET `actor_id` = 2,\r\n"
                            + "       `film_id` = 666\r\n"
                            + " WHERE `actor_id` = 1\r\n"
                            + "   AND `film_id` = 166;", sql);
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("Oracle")]
        public void BuildDelete_OracleWithoutPrimaryKey_UsesRowId()
        {
            var rows = CreateRows(new[] { "Value" }, new object[] { "new" }, new object[] { "old" }, "AAAU0lAABAAAaNhAAB");
            var whereResult = BuildWhere(DataSourceType.Oracle, rows, false);
            var sql = DmlPreviewSqlBuilder.BuildDelete("ABC0722", whereResult.Condition, true);

            Assert.AreEqual("DELETE FROM ABC0722\r\n"
                            + " WHERE ROWID = 'AAAU0lAABAAAaNhAAB';", sql);
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("PostgreSql")]
        public void BuildUpdate_PostgreSqlWithoutPrimaryKey_UsesCtid()
        {
            var rows = CreateRows
            (
                new[] { "customercode", "email" },
                new object[] { "a'b'c", DBNull.Value },
                new object[] { "old", "old@example.com" },
                "(0,12)"
            );

            var whereResult = BuildWhere(DataSourceType.PostgreSql, rows, false);
            var sql = DmlPreviewSqlBuilder.BuildUpdate("public.custinfo", "customercode = 'a''b''c',\r\n" + "       email = NULL", whereResult.Condition, true);

            Assert.AreEqual("UPDATE public.custinfo\r\n"
                            + "   SET customercode = 'a''b''c',\r\n"
                            + "       email = NULL\r\n"
                            + " WHERE CTID = '(0,12)';", sql);
        }

        [TestMethod]
        [TestCategory("Safety")]
        [TestCategory("SqlServer")]
        public void BuildDelete_SqlServerIncompleteCompositeKeyMetadata_ProducesNoSql()
        {
            var rows = CreateRows
            (
                new[] { "K1", "K2" },
                new object[] { 1, 2 },
                new object[] { 1, 2 }
            );

            var whereResult = BuildWhere
            (
                DataSourceType.SqlServer,
                rows,
                true,
                Key("K1", 1, NumberColumn("K1")),
                Key("K2", 2, null)
            );

            var sql = DmlPreviewSqlBuilder.BuildDelete
            (
                "[dbo].[T]",
                whereResult.Condition,
                true
            );

            Assert.IsFalse(whereResult.Success);
            Assert.AreEqual(string.Empty, sql);
        }

        [TestMethod]
        [TestCategory("Safety")]
        public void BuildUpdate_WhenOriginalRowIsMissing_ProducesNoSql()
        {
            var currentTable = CreateTable("PK");
            var originalTable = CreateTable("PK");
            var currentRow = AddRow(currentTable, "001", new[] { "PK" }, new object[] { "new" });

            AddRow(originalTable, "002", new[] { "PK" }, new object[] { "old" });

            var whereResult = DmlPreviewWhereConditionBuilder.Build
            (
                new DmlPreviewWhereConditionRequest
                {
                    DataSourceType = DataSourceType.Oracle,
                    CurrentRow = currentRow,
                    OriginalTable = originalTable,
                    RowIdentityColumnName = RowIdentityColumn,
                    HasDeclaredPrimaryKey = true,
                    PrimaryKeyColumns = new[]
                    {
                        Key("PK", 1, StringColumn("PK"))
                    }
                }
            );

            var sql = DmlPreviewSqlBuilder.BuildUpdate("T", "PK = 'new'", whereResult.Condition, true);

            Assert.IsFalse(whereResult.Success);
            Assert.AreEqual(string.Empty, sql);
        }

        [TestMethod]
        [TestCategory("Safety")]
        [TestCategory("SqlServer")]
        public void BuildUpdate_SqlServerWithoutPrimaryKey_ProducesNoSql()
        {
            var rows = CreateRows
            (
                new[] { "Value" },
                new object[] { "new" },
                new object[] { "old" }
            );

            var whereResult = BuildWhere(DataSourceType.SqlServer, rows, false);
            var sql = DmlPreviewSqlBuilder.BuildUpdate("[dbo].[NoPk]", "[Value] = N'new'", whereResult.Condition, true);

            Assert.IsFalse(whereResult.Success);
            Assert.AreEqual(string.Empty, sql);
        }

        [TestMethod]
        [TestCategory("Safety")]
        [TestCategory("MySql")]
        public void BuildDelete_MySqlWithoutPrimaryKey_ProducesNoSql()
        {
            var rows = CreateRows
            (
                new[] { "Value" },
                new object[] { "new" },
                new object[] { "old" }
            );

            var whereResult = BuildWhere(DataSourceType.MySql, rows, false);
            var sql = DmlPreviewSqlBuilder.BuildDelete("`sakila`.`no_pk`", whereResult.Condition, true);

            Assert.IsFalse(whereResult.Success);
            Assert.AreEqual(string.Empty, sql);
        }

        [TestMethod]
        [TestCategory("Safety")]
        public void BuildDelete_WithDuplicateOriginalIdentity_ProducesNoSql()
        {
            var currentTable = CreateTable("PK");
            var originalTable = CreateTable("PK");

            var currentRow = AddRow
            (
                currentTable,
                "001",
                new[] { "PK" },
                new object[] { 1 }
            );

            AddRow
            (
                originalTable,
                "001",
                new[] { "PK" },
                new object[] { 1 }
            );

            AddRow
            (
                originalTable,
                "001",
                new[] { "PK" },
                new object[] { 2 }
            );

            var whereResult = DmlPreviewWhereConditionBuilder.Build
            (
                new DmlPreviewWhereConditionRequest
                {
                    DataSourceType = DataSourceType.SqlServer,
                    CurrentRow = currentRow,
                    OriginalTable = originalTable,
                    RowIdentityColumnName = RowIdentityColumn,
                    HasDeclaredPrimaryKey = true,
                    PrimaryKeyColumns = new[]
                    {
                        Key("PK", 1, NumberColumn("PK"))
                    }
                }
            );

            var sql = DmlPreviewSqlBuilder.BuildDelete("[dbo].[T]", whereResult.Condition, true);

            Assert.IsFalse(whereResult.Success);
            Assert.AreEqual(string.Empty, sql);
        }

        [TestMethod]
        [TestCategory("Regression")]
        public void BuildDelete_NullPrimaryKey_UsesIsNullWithoutDroppingCondition()
        {
            var rows = CreateRows
            (
                new[] { "K1", "K2" },
                new object[] { 1, DBNull.Value },
                new object[] { 1, DBNull.Value }
            );

            var whereResult = BuildWhere
            (
                DataSourceType.PostgreSql,
                rows,
                true,
                Key("K1", 1, NumberColumn("K1")),
                Key("K2", 2, StringColumn("K2"))
            );

            var sql = DmlPreviewSqlBuilder.BuildDelete("public.t", whereResult.Condition, true);

            Assert.AreEqual("DELETE FROM public.t\r\n"
                            + " WHERE K1 = 1\r\n"
                            + "   AND K2 IS NULL;", sql);
        }

        [TestMethod]
        [TestCategory("Safety")]
        public void BuildDelete_WhenOneCompositeKeyColumnIsMissing_ProducesNoPartialWhere()
        {
            var currentTable = CreateTable("K1");
            var originalTable = CreateTable("K1");

            var currentRow = AddRow
            (
                currentTable,
                "001",
                new[] { "K1" },
                new object[] { 1 }
            );

            AddRow
            (
                originalTable,
                "001",
                new[] { "K1" },
                new object[] { 1 }
            );

            var whereResult = DmlPreviewWhereConditionBuilder.Build
            (
                new DmlPreviewWhereConditionRequest
                {
                    DataSourceType = DataSourceType.SqlServer,
                    CurrentRow = currentRow,
                    OriginalTable = originalTable,
                    RowIdentityColumnName = RowIdentityColumn,
                    HasDeclaredPrimaryKey = true,
                    PrimaryKeyColumns = new[]
                    {
                        Key("K1", 1, NumberColumn("K1")),
                        Key("K2", 2, NumberColumn("K2"))
                    }
                }
            );

            var sql = DmlPreviewSqlBuilder.BuildDelete("[dbo].[T]", whereResult.Condition, true);

            Assert.AreEqual(DmlPreviewWhereConditionFailureKind.PrimaryKeyValueColumnMissing, whereResult.FailureKind);
            Assert.AreEqual(string.Empty, sql);
        }

        private static DmlPreviewWhereConditionResult BuildWhere(DataSourceType dataSourceType, RowPair rows, bool hasDeclaredPrimaryKey, params DmlPreviewPrimaryKeyColumn[] primaryKeyColumns)
        {
            return DmlPreviewWhereConditionBuilder.Build
            (
                new DmlPreviewWhereConditionRequest
                {
                    DataSourceType = dataSourceType,
                    CurrentRow = rows.CurrentRow,
                    OriginalTable = rows.OriginalTable,
                    RowIdentityColumnName = RowIdentityColumn,
                    HasDeclaredPrimaryKey = hasDeclaredPrimaryKey,
                    PrimaryKeyColumns = primaryKeyColumns,
                    UseUpperCaseKeywords = true
                }
            );
        }

        private static DmlPreviewPrimaryKeyColumn Key(string columnName, int ordinalPosition, ColumnInfo columnInfo)
        {
            return new DmlPreviewPrimaryKeyColumn
            {
                ColumnName = columnName,
                OrdinalPosition = ordinalPosition,
                ColumnInfo = columnInfo
            };
        }

        private static ColumnInfo StringColumn(string columnName)
        {
            return new ColumnInfo
            {
                ColumnName = columnName,
                CategoryDataTypeKind = CategoryDataTypeKind.String,
                IsNullable = true
            };
        }

        private static ColumnInfo NumberColumn(string columnName)
        {
            return new ColumnInfo
            {
                ColumnName = columnName,
                CategoryDataTypeKind = CategoryDataTypeKind.Number,
                IsNullable = true
            };
        }

        private static RowPair CreateRows(string[] columnNames, object[] currentValues, object[] originalValues, string rowIdentity = "001")
        {
            var currentTable = CreateTable(columnNames);
            var originalTable = CreateTable(columnNames);
            var currentRow = AddRow(currentTable, rowIdentity, columnNames, currentValues);

            AddRow(originalTable, rowIdentity, columnNames, originalValues);

            return new RowPair
            {
                CurrentRow = currentRow,
                OriginalTable = originalTable
            };
        }

        private static DataTable CreateTable(params string[] columnNames)
        {
            var table = new DataTable();

            table.Columns.Add(RowIdentityColumn, typeof(object));

            foreach (var columnName in columnNames)
            {
                table.Columns.Add(columnName, typeof(object));
            }

            return table;
        }

        private static DataRow AddRow(DataTable table, object rowIdentity, string[] columnNames, object[] values)
        {
            var row = table.NewRow();

            row[RowIdentityColumn] = rowIdentity;

            for (var index = 0; index < columnNames.Length; index++)
            {
                row[columnNames[index]] = values[index] ?? (object)DBNull.Value;
            }

            table.Rows.Add(row);
            return row;
        }

        private sealed class RowPair
        {
            public DataRow CurrentRow { get; set; }

            public DataTable OriginalTable { get; set; }
        }
    }
}
