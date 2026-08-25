using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.QueryEngine.Editor.AutoComplete.Builders;
using JasonQuery.Core.QueryEngine.Editor.AutoComplete.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;
using System.Linq;

namespace JasonQuery.Tests.Core.QueryEngine.Editor.AutoComplete
{
    [TestClass]
    public sealed class QueryEditorAutoCompleteDataTableBuilderTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        public void BuildSpaceSourceRows_WithNullMetadata_ReturnsExpectedEmptyShape()
        {
            var result = QueryEditorAutoCompleteDataTableBuilder.BuildAutoCompleteSourceRowsForSpace
            (
                null,
                DataSourceType.Oracle,
                AutoCompleteObjectLookupMode.TableOrView,
                string.Empty
            );

            Assert.AreEqual(0, result.Rows.Count);

            CollectionAssert.AreEqual
            (
                new[]
                {
                    "SchemaName",
                    "SchemaType",
                    "AllowDBNull"
                },
                result.Columns.Cast<DataColumn>()
                              .Select(column => column.ColumnName)
                              .ToArray()
            );
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        public void BuildSpaceSourceRows_DatabaseOnlyFiltersDistinctAndSorts()
        {
            var metadata = CreateMetadataTable();

            AddMetadataRow(metadata, "db2", string.Empty, string.Empty, string.Empty, "db2", "DB");
            AddMetadataRow(metadata, "DB1", string.Empty, string.Empty, string.Empty, "DB1", "DB");
            AddMetadataRow(metadata, "db1", string.Empty, string.Empty, string.Empty, "db1", "DB");
            AddMetadataRow(metadata, "Table1", "[Table]", string.Empty, string.Empty, "DB1");

            var result = QueryEditorAutoCompleteDataTableBuilder.BuildAutoCompleteSourceRowsForSpace
            (
                metadata,
                DataSourceType.SqlServer,
                AutoCompleteObjectLookupMode.DatabaseOnly,
                "DB1"
            );

            CollectionAssert.AreEqual(new[] { "DB1", "db2" }, GetValues(result, "SchemaName"));
            CollectionAssert.AreEqual(new[] { "[Database]", "[Database]" }, GetValues(result, "SchemaType"));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        public void BuildSpaceSourceRows_TableOnlyExcludesViews()
        {
            var metadata = CreateMetadataTable();

            AddMetadataRow(metadata, "Table1", "[Table]", string.Empty, string.Empty, "DB1");
            AddMetadataRow(metadata, "View1", "[View]", string.Empty, string.Empty, "DB1");

            var result = QueryEditorAutoCompleteDataTableBuilder.BuildAutoCompleteSourceRowsForSpace
            (
                metadata,
                DataSourceType.SqlServer,
                AutoCompleteObjectLookupMode.TableOnly,
                "DB1"
            );

            CollectionAssert.AreEqual(new[] { "Table1" }, GetValues(result, "SchemaName"));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        public void BuildSpaceSourceRows_TableOrViewIncludesExtendedTypeNames()
        {
            var metadata = CreateMetadataTable();

            AddMetadataRow(metadata, "Table1", "[Table]", string.Empty, string.Empty, "DB1");
            AddMetadataRow(metadata, "View1", "[View]", string.Empty, string.Empty, "DB1");
            AddMetadataRow(metadata, "TablePartition", "[Table], Partition", string.Empty, string.Empty, "DB1");
            AddMetadataRow(metadata, "Sequence1", "[Sequence]", string.Empty, string.Empty, "DB1");

            var result = QueryEditorAutoCompleteDataTableBuilder.BuildAutoCompleteSourceRowsForSpace
            (
                metadata,
                DataSourceType.SqlServer,
                AutoCompleteObjectLookupMode.TableOrView,
                "DB1"
            );

            CollectionAssert.AreEqual
            (
                new[]
                {
                    "Table1",
                    "TablePartition",
                    "View1"
                },
                GetValues(result, "SchemaName")
            );
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        [DataRow(DataSourceType.SqlServer)]
        [DataRow(DataSourceType.MySql)]
        public void BuildSpaceSourceRows_CurrentDatabaseFiltersObjects(DataSourceType dataSourceType)
        {
            var metadata = CreateMetadataTable();

            AddMetadataRow(metadata, "Customer", "[Table]", string.Empty, string.Empty, "DB1");
            AddMetadataRow(metadata, "Order", "[Table]", string.Empty, string.Empty, "DB2");

            var result = QueryEditorAutoCompleteDataTableBuilder.BuildAutoCompleteSourceRowsForSpace
            (
                metadata,
                dataSourceType,
                AutoCompleteObjectLookupMode.TableOrView,
                "db1"
            );

            CollectionAssert.AreEqual(new[] { "Customer" }, GetValues(result, "SchemaName"));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        public void BuildSpaceSourceRows_OracleAppendsSchemaSuffix()
        {
            var metadata = CreateMetadataTable();

            AddMetadataRow(metadata, "A_TEST", "[Table]", "HR", string.Empty, string.Empty);

            var result = QueryEditorAutoCompleteDataTableBuilder.BuildAutoCompleteSourceRowsForSpace
            (
                metadata,
                DataSourceType.Oracle,
                AutoCompleteObjectLookupMode.TableOrView,
                string.Empty
            );

            Assert.AreEqual("[Table], [HR]", result.Rows[0]["SchemaType"]);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        public void BuildSpaceSourceRows_PostgreSqlAppendsSchemaNodeSuffix()
        {
            var metadata = CreateMetadataTable();

            AddMetadataRow(metadata, "a_test", "[Table]", string.Empty, "private", string.Empty);

            var result = QueryEditorAutoCompleteDataTableBuilder.BuildAutoCompleteSourceRowsForSpace
            (
                metadata,
                DataSourceType.PostgreSql,
                AutoCompleteObjectLookupMode.TableOrView,
                string.Empty
            );

            Assert.AreEqual("[Table], [private]", result.Rows[0]["SchemaType"]);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        public void BuildSpaceSourceRows_SqlServerDoesNotAppendSchemaSuffix()
        {
            var metadata = CreateMetadataTable();

            AddMetadataRow(metadata, "A_TEST", "[Table]", "dbo", "dbo", "DB1");

            var result = QueryEditorAutoCompleteDataTableBuilder.BuildAutoCompleteSourceRowsForSpace
            (
                metadata,
                DataSourceType.SqlServer,
                AutoCompleteObjectLookupMode.TableOrView,
                "DB1"
            );

            Assert.AreEqual("[Table]", result.Rows[0]["SchemaType"]);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        public void BuildSpaceSourceRows_SortsByNameThenType()
        {
            var metadata = CreateMetadataTable();

            AddMetadataRow(metadata, "B", "[View]", string.Empty, string.Empty, string.Empty);
            AddMetadataRow(metadata, "A", "[View]", string.Empty, string.Empty, string.Empty);
            AddMetadataRow(metadata, "A", "[Table]", string.Empty, string.Empty, string.Empty);

            var result = QueryEditorAutoCompleteDataTableBuilder.BuildAutoCompleteSourceRowsForSpace
            (
                metadata,
                DataSourceType.Oracle,
                AutoCompleteObjectLookupMode.TableOrView,
                string.Empty
            );

            CollectionAssert.AreEqual
            (
                new[] { "A", "A", "B" },
                GetValues(result, "SchemaName")
            );

            CollectionAssert.AreEqual
            (
                new[] { "[Table]", "[View]", "[View]" },
                GetValues(result, "SchemaType")
            );
        }


        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Regression")]
        [TestCategory("AutoComplete")]
        [TestCategory("Oracle")]
        public void BuildPeriodSourceRows_DuplicateColumnNamesPreserveEachSourceMetadata()
        {
            var schema = CreateOracleColumnSchemaTable();

            AddOracleColumnSchemaRow(schema, "NAME", "SYS", "JQDEMO_DEPARTMENT", "NVARCHAR2", 100, false);
            AddOracleColumnSchemaRow(schema, "NAME", "SYS", "JQDEMO_EMPLOYEE", "VARCHAR2", 150, true);
            AddOracleColumnSchemaRow(schema, "NAME", "SYS", "JQDEMO_PRODUCT", "NVARCHAR2", 120, false);

            var result = QueryEditorAutoCompleteDataTableBuilder.BuildAutoCompleteSourceRowsForPeriod
            (
                schema,
                DataSourceType.Oracle
            );

            CollectionAssert.AreEqual
            (
                new[]
                {
                    "[SYS.JQDEMO_DEPARTMENT]",
                    "[SYS.JQDEMO_EMPLOYEE]",
                    "[SYS.JQDEMO_PRODUCT]"
                },
                GetValues(result, "BaseTableName")
            );

            CollectionAssert.AreEqual
            (
                new[] { "NVARCHAR2(100)", "VARCHAR2(150)", "NVARCHAR2(120)" },
                GetValues(result, "DataType")
            );

            CollectionAssert.AreEqual
            (
                new[] { "N", string.Empty, "N" },
                GetValues(result, "AllowDBNull")
            );
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        public void BuildSpaceTable_WithNullSourceReturnsExpectedShape()
        {
            var result = QueryEditorAutoCompleteDataTableBuilder.BuildAutoCompleteSpaceTable(null);

            Assert.AreEqual(0, result.Rows.Count);

            CollectionAssert.AreEqual
            (
                new[]
                {
                    "ColumnName",
                    "DataType",
                    "AllowDBNull"
                },
                result.Columns.Cast<DataColumn>()
                              .Select(column => column.ColumnName)
                              .ToArray()
            );
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        public void BuildSpaceTable_MapsSourceColumns()
        {
            var source = new DataTable();

            source.Columns.Add("SchemaName");
            source.Columns.Add("SchemaType");
            source.Columns.Add("AllowDBNull");

            source.Rows.Add("CustomerID", "int", "P");
            source.Rows.Add("CustomerName", "nvarchar(50)", string.Empty);

            var result = QueryEditorAutoCompleteDataTableBuilder.BuildAutoCompleteSpaceTable(source);

            CollectionAssert.AreEqual
            (
                new[] { "CustomerID", "CustomerName" },
                GetValues(result, "ColumnName")
            );

            CollectionAssert.AreEqual
            (
                new[] { "int", "nvarchar(50)" },
                GetValues(result, "DataType")
            );

            CollectionAssert.AreEqual
            (
                new[] { "P", string.Empty },
                GetValues(result, "AllowDBNull")
            );
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        public void BuildSpaceTable_WhenAllowDBNullMissingUsesEmptyValue()
        {
            var source = new DataTable();

            source.Columns.Add("SchemaName");
            source.Columns.Add("SchemaType");
            source.Rows.Add("CustomerID", "int");

            var result = QueryEditorAutoCompleteDataTableBuilder.BuildAutoCompleteSpaceTable(source);

            Assert.AreEqual(string.Empty, result.Rows[0]["AllowDBNull"]);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        public void BuildSpaceSourceRows_DoesNotMutateMetadata()
        {
            var metadata = CreateMetadataTable();

            AddMetadataRow(metadata, "B", "[Table]", string.Empty, string.Empty, string.Empty);
            AddMetadataRow(metadata, "A", "[Table]", string.Empty, string.Empty, string.Empty);

            QueryEditorAutoCompleteDataTableBuilder.BuildAutoCompleteSourceRowsForSpace
            (
                metadata,
                DataSourceType.Oracle,
                AutoCompleteObjectLookupMode.TableOrView,
                string.Empty
            );

            CollectionAssert.AreEqual
            (
                new[] { "B", "A" },
                GetValues(metadata, "SchemaName")
            );
        }

        private static DataTable CreateOracleColumnSchemaTable()
        {
            var table = new DataTable();

            table.Columns.Add("ColumnName");
            table.Columns.Add("BaseSchemaName");
            table.Columns.Add("BaseTableName");
            table.Columns.Add("TypeName");
            table.Columns.Add("DataType");
            table.Columns.Add("ColumnSize", typeof(int));
            table.Columns.Add("NumericPrecision", typeof(int));
            table.Columns.Add("NumericScale", typeof(int));
            table.Columns.Add("ProviderSpecificDataType");
            table.Columns.Add("IsKey", typeof(bool));
            table.Columns.Add("AllowDBNull", typeof(bool));
            table.Columns.Add("Comment");

            return table;
        }

        private static void AddOracleColumnSchemaRow(DataTable table, string columnName, string baseSchemaName,
                                                     string baseTableName, string typeName, int columnSize, bool allowDBNull)
        {
            var row = table.NewRow();

            row["ColumnName"] = columnName;
            row["BaseSchemaName"] = baseSchemaName;
            row["BaseTableName"] = baseTableName;
            row["TypeName"] = typeName;
            row["DataType"] = "System.String";
            row["ColumnSize"] = columnSize;
            row["NumericPrecision"] = 0;
            row["NumericScale"] = 0;
            row["ProviderSpecificDataType"] = "System.String";
            row["IsKey"] = false;
            row["AllowDBNull"] = allowDBNull;
            row["Comment"] = string.Empty;

            table.Rows.Add(row);
        }

        private static DataTable CreateMetadataTable()
        {
            var table = new DataTable();

            table.Columns.Add("SchemaName");
            table.Columns.Add("SchemaType");
            table.Columns.Add("Schema");
            table.Columns.Add("SchemaNode");
            table.Columns.Add("Memo");
            table.Columns.Add("DB");

            return table;
        }

        private static void AddMetadataRow(DataTable table, string schemaName, string schemaType, string schema, string schemaNode,
                                           string databaseName, string memo = "")
        {
            var row = table.NewRow();

            row["SchemaName"] = schemaName;
            row["SchemaType"] = schemaType;
            row["Schema"] = schema;
            row["SchemaNode"] = schemaNode;
            row["Memo"] = memo ?? string.Empty;
            row["DB"] = databaseName;

            table.Rows.Add(row);
        }

        private static string[] GetValues(DataTable table, string columnName)
        {
            return table.Rows.Cast<DataRow>()
                             .Select
                              (
                                  row => Convert.ToString(row[columnName])
                              )
                             .ToArray();
        }
    }
}
