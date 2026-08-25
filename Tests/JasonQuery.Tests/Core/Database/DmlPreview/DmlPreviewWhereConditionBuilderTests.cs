using JasonLibrary.Core.Schema;
using JasonLibrary.Core.Schema.Enums;
using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.DmlPreview;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data;

namespace JasonQuery.Tests.Core.Database.DmlPreview
{
    [TestClass]
    public sealed class DmlPreviewWhereConditionBuilderTests
    {
        private const string RowIdentityColumn = "R_O_W_1_D_P_K_J_Q";

        [TestMethod]
        [TestCategory("Safety")]
        public void Build_WithNullRequest_ReturnsInvalidRequest()
        {
            AssertFailure(DmlPreviewWhereConditionBuilder.Build(null), DmlPreviewWhereConditionFailureKind.InvalidRequest);
        }

        [TestMethod]
        [TestCategory("Safety")]
        public void Build_WithNullCurrentRow_ReturnsInvalidRequest()
        {
            AssertFailure
            (
                DmlPreviewWhereConditionBuilder.Build
                (
                    new DmlPreviewWhereConditionRequest
                    {
                        OriginalTable = CreateTable("PK"),
                        RowIdentityColumnName = RowIdentityColumn
                    }
                ),
                DmlPreviewWhereConditionFailureKind.InvalidRequest
            );
        }

        [TestMethod]
        [TestCategory("Safety")]
        public void Build_WithNullOriginalTable_ReturnsInvalidRequest()
        {
            var currentTable = CreateTable("PK");
            var currentRow = AddRow(currentTable, "001", "PK", "A");

            AssertFailure
            (
                DmlPreviewWhereConditionBuilder.Build
                (
                    new DmlPreviewWhereConditionRequest
                    {
                        CurrentRow = currentRow,
                        RowIdentityColumnName = RowIdentityColumn
                    }
                ),
                DmlPreviewWhereConditionFailureKind.InvalidRequest
            );
        }

        [TestMethod]
        [TestCategory("Safety")]
        public void Build_WithBlankIdentityColumn_ReturnsInvalidRequest()
        {
            var rows = CreateCurrentAndOriginalRows("PK", "new", "old");

            AssertFailure
            (
                DmlPreviewWhereConditionBuilder.Build
                (
                    new DmlPreviewWhereConditionRequest
                    {
                        CurrentRow = rows.CurrentRow,
                        OriginalTable = rows.OriginalTable,
                        RowIdentityColumnName = " "
                    }
                ),
                DmlPreviewWhereConditionFailureKind.InvalidRequest
            );
        }

        [TestMethod]
        [TestCategory("Safety")]
        public void Build_WhenOriginalRowCannotBeResolved_ReturnsOriginalRowNotFound()
        {
            var currentTable = CreateTable("PK");
            var originalTable = CreateTable("PK");
            var currentRow = AddRow(currentTable, "001", "PK", "new");

            AddRow(originalTable, "002", "PK", "old");

            AssertFailure
            (
                Build
                (
                    DataSourceType.Oracle,
                    currentRow,
                    originalTable,
                    true,
                    Key("PK", 1, StringColumn("PK"))
                ),
                DmlPreviewWhereConditionFailureKind.OriginalRowNotFound
            );
        }

        [TestMethod]
        [TestCategory("Safety")]
        public void Build_WithDeclaredPrimaryKeyButNoMetadata_ReturnsMetadataMissing()
        {
            var rows = CreateCurrentAndOriginalRows("PK", "new", "old");

            AssertFailure
            (
                Build
                (
                    DataSourceType.Oracle,
                    rows.CurrentRow,
                    rows.OriginalTable,
                    true
                ),
                DmlPreviewWhereConditionFailureKind.PrimaryKeyMetadataMissing
            );
        }

        [TestMethod]
        [TestCategory("Safety")]
        public void Build_WithNullPrimaryKeyItem_ReturnsColumnNameMissing()
        {
            var rows = CreateCurrentAndOriginalRows("PK", "new", "old");

            AssertFailure
            (
                Build
                (
                    DataSourceType.Oracle,
                    rows.CurrentRow,
                    rows.OriginalTable,
                    true,
                    (DmlPreviewPrimaryKeyColumn)null
                ),
                DmlPreviewWhereConditionFailureKind.PrimaryKeyColumnNameMissing
            );
        }

        [TestMethod]
        [TestCategory("Safety")]
        public void Build_WithBlankPrimaryKeyName_ReturnsColumnNameMissing()
        {
            var rows = CreateCurrentAndOriginalRows("PK", "new", "old");

            AssertFailure
            (
                Build
                (
                    DataSourceType.Oracle,
                    rows.CurrentRow,
                    rows.OriginalTable,
                    true,
                    Key(" ", 1, StringColumn("PK"))
                ),
                DmlPreviewWhereConditionFailureKind.PrimaryKeyColumnNameMissing
            );
        }

        [TestMethod]
        [TestCategory("Safety")]
        public void Build_WithMissingColumnInfo_ReturnsColumnInfoMissing()
        {
            var rows = CreateCurrentAndOriginalRows("PK", "new", "old");

            AssertFailure
            (
                Build
                (
                    DataSourceType.Oracle,
                    rows.CurrentRow,
                    rows.OriginalTable,
                    true,
                    Key("PK", 1, null)
                ),
                DmlPreviewWhereConditionFailureKind.PrimaryKeyColumnInfoMissing
            );
        }

        [TestMethod]
        [TestCategory("Safety")]
        public void Build_WhenOriginalRowDoesNotContainKeyColumn_ReturnsValueColumnMissing()
        {
            var currentTable = CreateTable("PK");
            var originalTable = CreateTable("OTHER");
            var currentRow = AddRow(currentTable, "001", "PK", "new");

            AddRow(originalTable, "001", "OTHER", "old");

            AssertFailure
            (
                Build
                (
                    DataSourceType.Oracle,
                    currentRow,
                    originalTable,
                    true,
                    Key("PK", 1, StringColumn("PK"))
                ),
                DmlPreviewWhereConditionFailureKind.PrimaryKeyValueColumnMissing
            );
        }

        [TestMethod]
        [TestCategory("Safety")]
        public void Build_WithDuplicatePrimaryKeyColumns_ReturnsDuplicateFailure()
        {
            var rows = CreateCurrentAndOriginalRows("PK", "new", "old");

            AssertFailure
            (
                Build
                (
                    DataSourceType.SqlServer,
                    rows.CurrentRow,
                    rows.OriginalTable,
                    true,
                    Key("PK", 1, NumberColumn("PK")),
                    Key("pk", 2, NumberColumn("pk"))
                ),
                DmlPreviewWhereConditionFailureKind.DuplicatePrimaryKeyColumn
            );
        }

        [TestMethod]
        [TestCategory("Safety")]
        public void Build_WithInvalidPrimaryKeyDoesNotFallBackToOracleRowId()
        {
            var rows = CreateCurrentAndOriginalRows("PK", "new", "old", "AAATUUAABAAAaefAAF");

            AssertFailure
            (
                Build
                (
                    DataSourceType.Oracle,
                    rows.CurrentRow,
                    rows.OriginalTable,
                    true,
                    Key("MISSING", 1, StringColumn("MISSING"))
                ),
                DmlPreviewWhereConditionFailureKind.PrimaryKeyValueColumnMissing
            );
        }

        [TestMethod]
        [TestCategory("Safety")]
        public void Build_WithoutPrimaryKeyForSqlServer_ReturnsPhysicalIdentifierNotSupported()
        {
            var rows = CreateCurrentAndOriginalRows("Value", "new", "old");

            AssertFailure
            (
                Build
                (
                    DataSourceType.SqlServer,
                    rows.CurrentRow,
                    rows.OriginalTable,
                    false
                ),
                DmlPreviewWhereConditionFailureKind.PhysicalRowIdentifierNotSupported
            );
        }

        [TestMethod]
        [TestCategory("Safety")]
        public void Build_WithoutPrimaryKeyForMySql_ReturnsPhysicalIdentifierNotSupported()
        {
            var rows = CreateCurrentAndOriginalRows("Value", "new", "old");

            AssertFailure
            (
                Build
                (
                    DataSourceType.MySql,
                    rows.CurrentRow,
                    rows.OriginalTable,
                    false
                ),
                DmlPreviewWhereConditionFailureKind.PhysicalRowIdentifierNotSupported
            );
        }

        [TestMethod]
        [TestCategory("Safety")]
        public void Build_WithoutPrimaryKeyForUnknownProvider_ReturnsPhysicalIdentifierNotSupported()
        {
            var rows = CreateCurrentAndOriginalRows("Value", "new", "old");

            AssertFailure
            (
                Build
                (
                    DataSourceType.None,
                    rows.CurrentRow,
                    rows.OriginalTable,
                    false
                ),
                DmlPreviewWhereConditionFailureKind.PhysicalRowIdentifierNotSupported
            );
        }

        [TestMethod]
        [TestCategory("Safety")]
        public void Build_OracleWithoutPhysicalIdentifier_ReturnsMissingFailure()
        {
            var currentTable = CreateTable("Value");
            var originalTable = CreateTable("Value");
            var currentRow = AddRow(currentTable, "001", "Value", "new");
            var originalRow = AddRow(originalTable, "001", "Value", "old");

            originalRow[RowIdentityColumn] = string.Empty;

            AssertFailure
            (
                Build
                (
                    DataSourceType.Oracle,
                    currentRow,
                    originalTable,
                    false
                ),
                DmlPreviewWhereConditionFailureKind.OriginalRowNotFound
            );
        }

        [TestMethod]
        [TestCategory("Safety")]
        public void Build_PostgreSqlWithoutPhysicalIdentifier_ReturnsMissingFailure()
        {
            var currentTable = CreateTable("Value");
            var originalTable = CreateTable("Value");
            var currentRow = AddRow(currentTable, "001", "Value", "new");
            var originalRow = AddRow(originalTable, "001", "Value", "old");

            originalRow[RowIdentityColumn] = DBNull.Value;

            AssertFailure
            (
                Build
                (
                    DataSourceType.PostgreSql,
                    currentRow,
                    originalTable,
                    false
                ),
                DmlPreviewWhereConditionFailureKind.OriginalRowNotFound
            );
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("Oracle")]
        public void Build_OracleModifiedPrimaryKey_UsesOriginalValue()
        {
            var rows = CreateCurrentAndOriginalRows("T1_VARCHAR2_PK", "5555", "33335");

            var result = Build
            (
                DataSourceType.Oracle,
                rows.CurrentRow,
                rows.OriginalTable,
                true,
                Key("T1_VARCHAR2_PK", 1, StringColumn("T1_VARCHAR2_PK"))
            );

            AssertSuccess(result, "T1_VARCHAR2_PK = '33335'", DmlPreviewWhereConditionSource.PrimaryKey);
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("PostgreSql")]
        public void Build_PostgreSqlModifiedPrimaryKey_UsesOriginalValue()
        {
            var rows = CreateCurrentAndOriginalRows("t1_charvery", "A1015", "T1015");

            var result = Build
            (
                DataSourceType.PostgreSql,
                rows.CurrentRow,
                rows.OriginalTable,
                true,
                Key("t1_charvery", 1, StringColumn("t1_charvery"))
            );

            AssertSuccess(result, "t1_charvery = 'T1015'", DmlPreviewWhereConditionSource.PrimaryKey);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("SqlServer")]
        public void Build_SqlServerUnicodePrimaryKey_UsesNStringLiteral()
        {
            var rows = CreateCurrentAndOriginalRows("Code", "新", "舊");

            var result = Build
            (
                DataSourceType.SqlServer,
                rows.CurrentRow,
                rows.OriginalTable,
                true,
                Key("Code", 1, NStringColumn("Code"))
            );

            AssertSuccess(result, "[Code] = N'舊'", DmlPreviewWhereConditionSource.PrimaryKey);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("MySql")]
        public void Build_MySqlNumberPrimaryKey_UsesUnquotedNumber()
        {
            var rows = CreateCurrentAndOriginalRows("city_id", 8, 6);

            var result = Build
            (
                DataSourceType.MySql,
                rows.CurrentRow,
                rows.OriginalTable,
                true,
                Key("city_id", 1, NumberColumn("city_id") )
            );

            AssertSuccess(result, "`city_id` = 6", DmlPreviewWhereConditionSource.PrimaryKey);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Build_DbNullPrimaryKey_UsesIsNull()
        {
            var rows = CreateCurrentAndOriginalRows("PK", "new", DBNull.Value);

            var result = Build
            (
                DataSourceType.PostgreSql,
                rows.CurrentRow,
                rows.OriginalTable,
                true,
                Key("PK", 1, StringColumn("PK"))
            );

            AssertSuccess(result, "PK IS NULL", DmlPreviewWhereConditionSource.PrimaryKey);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Build_DbNullPrimaryKeyWithLowerCaseKeywords_UsesLowerCaseIsNull()
        {
            var rows = CreateCurrentAndOriginalRows("PK", "new", DBNull.Value);

            var result = Build
            (
                DataSourceType.PostgreSql,
                rows.CurrentRow,
                rows.OriginalTable,
                true,
                false,
                Key("PK", 1, StringColumn("PK"))
            );

            AssertSuccess(result, "PK is null", DmlPreviewWhereConditionSource.PrimaryKey);
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("SqlServer")]
        public void Build_SqlServerCompositeKey_SortsByOrdinalPosition()
        {
            var rows = CreateRows
            (
                new[]
                {
                    "BoardRequestID",
                    "SampleInspectionID",
                    "StatusName"
                },
                new object[] { 999, 999, 999 },
                new object[] { 113, 1569, 3 }
            );

            var result = Build
            (
                DataSourceType.SqlServer,
                rows.CurrentRow,
                rows.OriginalTable,
                true,
                Key("StatusName", 3, NumberColumn("StatusName")),
                Key("BoardRequestID", 1, NumberColumn("BoardRequestID")),
                Key("SampleInspectionID", 2, NumberColumn("SampleInspectionID"))
            );

            AssertSuccess(result, "[BoardRequestID] = 113\r\n" + "   AND [SampleInspectionID] = 1569\r\n" + "   AND [StatusName] = 3", DmlPreviewWhereConditionSource.PrimaryKey);
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("MySql")]
        public void Build_MySqlCompositeKey_SortsByOrdinalPosition()
        {
            var rows = CreateRows(new[] { "actor_id", "film_id" }, new object[] { 2, 666 }, new object[] { 1, 166 });

            var result = Build
            (
                DataSourceType.MySql,
                rows.CurrentRow,
                rows.OriginalTable,
                true,
                Key("film_id", 2, NumberColumn("film_id")),
                Key("actor_id", 1, NumberColumn("actor_id"))
            );

            AssertSuccess(result, "`actor_id` = 1\r\n" + "   AND `film_id` = 166", DmlPreviewWhereConditionSource.PrimaryKey);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Oracle")]
        public void Build_OracleCompositeKey_SortsByOrdinalPosition()
        {
            var rows = CreateRows(new[] { "K1", "K2" }, new object[] { "new1", "new2" }, new object[] { "old1", "old2" });

            var result = Build
            (
                DataSourceType.Oracle,
                rows.CurrentRow,
                rows.OriginalTable,
                true,
                Key("K2", 2, StringColumn("K2")),
                Key("K1", 1, StringColumn("K1"))
            );

            AssertSuccess(result, "K1 = 'old1'\r\n" + "   AND K2 = 'old2'", DmlPreviewWhereConditionSource.PrimaryKey);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Build_WithEqualOrdinals_SortsByColumnName()
        {
            var rows = CreateRows(new[] { "B", "A" }, new object[] { 2, 1 }, new object[] { 20, 10 });

            var result = Build
            (
                DataSourceType.PostgreSql,
                rows.CurrentRow,
                rows.OriginalTable,
                true,
                Key("B", 1, NumberColumn("B")),
                Key("A", 1, NumberColumn("A"))
            );

            AssertSuccess(result, "A = 10\r\n" + "   AND B = 20", DmlPreviewWhereConditionSource.PrimaryKey);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("SqlServer")]
        public void Build_SqlServerIdentifierContainingBracket_IsEscaped()
        {
            var rows = CreateCurrentAndOriginalRows("A]B", 2, 1);

            var result = Build
            (
                DataSourceType.SqlServer,
                rows.CurrentRow,
                rows.OriginalTable,
                true,
                Key("A]B", 1, NumberColumn("A]B"))
            );

            AssertSuccess(result, "[A]]B] = 1", DmlPreviewWhereConditionSource.PrimaryKey);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("MySql")]
        public void Build_MySqlIdentifierContainingBacktick_IsEscaped()
        {
            var rows = CreateCurrentAndOriginalRows("A`B", 2, 1);

            var result = Build
            (
                DataSourceType.MySql,
                rows.CurrentRow,
                rows.OriginalTable,
                true,
                Key("A`B", 1, NumberColumn("A`B"))
            );

            AssertSuccess(result, "`A``B` = 1", DmlPreviewWhereConditionSource.PrimaryKey);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Build_WithLowerCaseKeywords_UsesLowerCaseAnd()
        {
            var rows = CreateRows(new[] { "K1", "K2" }, new object[] { 1, 2 }, new object[] { 10, 20 });

            var result = Build
            (
                DataSourceType.PostgreSql,
                rows.CurrentRow,
                rows.OriginalTable,
                true,
                false,
                Key("K1", 1, NumberColumn("K1")),
                Key("K2", 2, NumberColumn("K2"))
            );

            AssertSuccess(result, "K1 = 10\r\n" + "   and K2 = 20", DmlPreviewWhereConditionSource.PrimaryKey);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Build_WithCustomNullIndicator_UsesIsNull()
        {
            var rows = CreateCurrentAndOriginalRows("PK", "new", "(null)");

            var result = Build
            (
                DataSourceType.PostgreSql,
                rows.CurrentRow,
                rows.OriginalTable,
                true,
                true,
                new[] { "(null)" },
                Key("PK", 1, StringColumn("PK"))
            );

            AssertSuccess(result, "PK IS NULL", DmlPreviewWhereConditionSource.PrimaryKey);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Build_StringPrimaryKey_EscapesSingleQuote()
        {
            var rows = CreateCurrentAndOriginalRows("PK", "new", "old'value");

            var result = Build
            (
                DataSourceType.PostgreSql,
                rows.CurrentRow,
                rows.OriginalTable,
                true,
                Key("PK", 1, StringColumn("PK"))
            );

            AssertSuccess(result, "PK = 'old''value'", DmlPreviewWhereConditionSource.PrimaryKey);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Oracle")]
        public void Build_OracleDateTimePrimaryKey_UsesTimestampLiteral()
        {
            var rows = CreateCurrentAndOriginalRows("PK_DATE", "2026/07/20 01:02:03", "2026/07/19 07:43:15.123456");

            var result = Build
            (
                DataSourceType.Oracle,
                rows.CurrentRow,
                rows.OriginalTable,
                true,
                Key("PK_DATE", 1, DateTimeColumn("PK_DATE"))
            );

            AssertSuccess(result, "PK_DATE = TIMESTAMP '2026-07-1907:43:15.123456'", DmlPreviewWhereConditionSource.PrimaryKey);
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("Oracle")]
        public void Build_OracleWithoutPrimaryKey_UsesRowId()
        {
            var rows = CreateCurrentAndOriginalRows("Value", "new", "old", "AAATUUAABAAAaefAAF");
            var result = Build(DataSourceType.Oracle, rows.CurrentRow, rows.OriginalTable, false);

            AssertSuccess(result, "ROWID = 'AAATUUAABAAAaefAAF'", DmlPreviewWhereConditionSource.PhysicalRowIdentifier);
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("PostgreSql")]
        public void Build_PostgreSqlWithoutPrimaryKey_UsesCtid()
        {
            var rows = CreateCurrentAndOriginalRows("Value", "new", "old", "(0,12)");
            var result = Build(DataSourceType.PostgreSql, rows.CurrentRow, rows.OriginalTable, false);

            AssertSuccess(result, "CTID = '(0,12)'", DmlPreviewWhereConditionSource.PhysicalRowIdentifier);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Build_PhysicalIdentifier_EscapesSingleQuote()
        {
            var rows = CreateCurrentAndOriginalRows("Value", "new", "old", "A'B");
            var result = Build(DataSourceType.Oracle, rows.CurrentRow, rows.OriginalTable, false);

            AssertSuccess(result, "ROWID = 'A''B'", DmlPreviewWhereConditionSource.PhysicalRowIdentifier);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Build_PhysicalIdentifierWithLowerCaseKeywords_UsesLowerCaseName()
        {
            var rows = CreateCurrentAndOriginalRows("Value", "new", "old", "(0,8)");
            var result = Build(DataSourceType.PostgreSql, rows.CurrentRow, rows.OriginalTable, false, false);

            AssertSuccess(result, "ctid = '(0,8)'", DmlPreviewWhereConditionSource.PhysicalRowIdentifier);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Build_SuccessClearsFailureKind()
        {
            var rows = CreateCurrentAndOriginalRows("PK", "new", "old");
            
            var result = Build
            (
                DataSourceType.Oracle,
                rows.CurrentRow,
                rows.OriginalTable,
                true,
                Key("PK", 1, StringColumn("PK"))
            );

            Assert.IsTrue(result.Success);
            Assert.AreEqual(DmlPreviewWhereConditionFailureKind.None, result.FailureKind);
        }

        [TestMethod]
        [TestCategory("Safety")]
        public void Build_FailureHasNoConditionAndNoSource()
        {
            var result = DmlPreviewWhereConditionBuilder.Build(null);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(string.Empty, result.Condition);
            Assert.AreEqual(DmlPreviewWhereConditionSource.None, result.Source);
        }

        private static DmlPreviewWhereConditionResult Build(DataSourceType dataSourceType, DataRow currentRow, DataTable originalTable,
                                                            bool hasDeclaredPrimaryKey, params DmlPreviewPrimaryKeyColumn[] primaryKeyColumns)
        {
            return Build
            (
                dataSourceType,
                currentRow,
                originalTable,
                hasDeclaredPrimaryKey,
                true,
                null, primaryKeyColumns
            );
        }

        private static DmlPreviewWhereConditionResult Build(DataSourceType dataSourceType, DataRow currentRow, DataTable originalTable, bool hasDeclaredPrimaryKey,
                                                            bool useUpperCaseKeywords, params DmlPreviewPrimaryKeyColumn[] primaryKeyColumns)
        {
            return Build
            (
                dataSourceType,
                currentRow,
                originalTable,
                hasDeclaredPrimaryKey,
                useUpperCaseKeywords,
                null,
                primaryKeyColumns
            );
        }

        private static DmlPreviewWhereConditionResult Build(DataSourceType dataSourceType, DataRow currentRow, DataTable originalTable, bool hasDeclaredPrimaryKey,
                                                            bool useUpperCaseKeywords, IEnumerable<string> nullValueIndicators, params DmlPreviewPrimaryKeyColumn[] primaryKeyColumns)
        {
            return DmlPreviewWhereConditionBuilder.Build
            (
                new DmlPreviewWhereConditionRequest
                {
                    DataSourceType = dataSourceType,
                    CurrentRow = currentRow,
                    OriginalTable = originalTable,
                    RowIdentityColumnName = RowIdentityColumn,
                    HasDeclaredPrimaryKey = hasDeclaredPrimaryKey,
                    PrimaryKeyColumns = primaryKeyColumns,
                    UseUpperCaseKeywords = useUpperCaseKeywords,
                    NullValueIndicators = nullValueIndicators
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

        private static ColumnInfo NStringColumn(string columnName)
        {
            return new ColumnInfo
            {
                ColumnName = columnName,
                CategoryDataTypeKind = CategoryDataTypeKind.String,
                SpecialDataTypeKind = SpecialDataTypeKind.NString,
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

        private static ColumnInfo DateTimeColumn(string columnName)
        {
            return new ColumnInfo
            {
                ColumnName = columnName,
                CategoryDataTypeKind = CategoryDataTypeKind.DateTime,
                IsNullable = true
            };
        }

        private static RowPair CreateCurrentAndOriginalRows(string valueColumnName, object currentValue, object originalValue, string rowIdentity = "001")
        {
            return CreateRows
            (
                new[] { valueColumnName },
                new[] { currentValue },
                new[] { originalValue },
                rowIdentity
            );
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

        private static DataTable CreateTable(params string[] valueColumnNames)
        {
            var table = new DataTable();

            table.Columns.Add(RowIdentityColumn, typeof(object));

            foreach (var columnName in valueColumnNames)
            {
                table.Columns.Add(columnName, typeof(object));
            }

            return table;
        }

        private static DataRow AddRow(DataTable table, object rowIdentity, string valueColumnName, object value)
        {
            return AddRow
            (
                table,
                rowIdentity,
                new[] { valueColumnName },
                new[] { value }
            );
        }

        private static DataRow AddRow(DataTable table, object rowIdentity, string[] columnNames, object[] values)
        {
            var row = table.NewRow();

            row[RowIdentityColumn] = rowIdentity ?? (object)DBNull.Value;

            for (var index = 0; index < columnNames.Length; index++)
            {
                row[columnNames[index]] = values[index] ?? (object)DBNull.Value;
            }

            table.Rows.Add(row);
            return row;
        }

        private static void AssertSuccess(DmlPreviewWhereConditionResult result, string expectedCondition, DmlPreviewWhereConditionSource expectedSource)
        {
            Assert.IsTrue(result.Success);
            Assert.AreEqual(expectedCondition, result.Condition);
            Assert.AreEqual(expectedSource, result.Source);
            Assert.AreEqual(DmlPreviewWhereConditionFailureKind.None, result.FailureKind);
        }

        private static void AssertFailure(DmlPreviewWhereConditionResult result, DmlPreviewWhereConditionFailureKind expectedFailure)
        {
            Assert.IsFalse(result.Success);
            Assert.AreEqual(expectedFailure, result.FailureKind);
            Assert.AreEqual(string.Empty, result.Condition);
            Assert.AreEqual(DmlPreviewWhereConditionSource.None, result.Source);
        }

        private sealed class RowPair
        {
            public DataRow CurrentRow { get; set; }

            public DataTable OriginalTable { get; set; }
        }
    }
}
