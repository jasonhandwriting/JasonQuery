using JasonLibrary.Core.Schema;
using JasonLibrary.Core.Schema.Enums;
using JasonQuery.Core.Arrange;
using JasonQuery.Core.Data.Schema;
using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.QueryEngine.Types;
using JasonQuery.Core.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;
using System.Linq;

namespace JasonQuery.IntegrationTests.Infrastructure
{
    internal static class IntegrationTestSpecialTypeAssertions
    {
        public static IntegrationTestDatabaseSettings LoadRequiredSettings(TestContext testContext, DataSourceType sourceType)
        {
            var settings = IntegrationTestDatabaseSettings.Load(testContext, sourceType);

            settings.RequireConfigured();

            return settings;
        }

        public static void AssertPagedSucceeded(IntegrationTestPagedQueryResult result, DataSourceType sourceType)
        {
            Assert.IsNotNull(result);
            Assert.AreEqual(string.Empty, result.ErrorMessage, $"Paged query failed for {sourceType}. ErrorCode: {result.ErrorCode}\r\n{result.ErrorMessage}");
            Assert.IsNotNull(result.Data);
            Assert.IsNotNull(result.Schema);
            Assert.AreEqual(1, result.Data.Rows.Count);
        }

        public static ColumnInfoCollector BuildCollector(DataSourceType sourceType, IntegrationTestPagedQueryResult result)
        {
            AssertPagedSucceeded(result, sourceType);

            return SchemaColumnInfoBuilder.Build(sourceType, result.Schema);
        }

        public static ColumnInfo GetColumnInfo(ColumnInfoCollector collector, string columnName)
        {
            Assert.IsNotNull(collector);
            Assert.IsTrue(collector.TryGet(columnName, out var info), $"ColumnInfo was not found: {columnName}");
            Assert.IsNotNull(info);

            return info;
        }

        public static DataRow GetSchemaRow(DataTable schema, string columnName)
        {
            Assert.IsNotNull(schema);
            Assert.IsTrue(schema.Columns.Contains("ColumnName"));

            var row = schema.Rows.Cast<DataRow>()
                            .FirstOrDefault
                             (
                                 item => string.Equals
                                         (
                                             Convert.ToString(item["ColumnName"]), columnName, StringComparison.OrdinalIgnoreCase
                                         )
                             );

            Assert.IsNotNull(row, $"Schema row was not found: {columnName}");

            return row;
        }

        public static DataTable Arrange(DataSourceType sourceType, IntegrationTestPagedQueryResult result, ColumnInfoCollector collector)
        {
            AssertPagedSucceeded(result, sourceType);

            var context = new ArrangeContext
            {
                SchemaTable = result.Schema,
                SourceData = result.Data,
                columnInfoCollector = collector,
                NullDisplayText = "<NULL>",
                LargeTextPreviewLength = 50,
                DateFormat = "yyyy/MM/dd",
                DateTimeFormat = "yyyy/MM/dd HH:mm:ss",
                TruncatedText = "...(truncated)",
                ShowColumnType = false,
                ShowColumnComments = false,
                LoadColumnCommentsForCellTip = false,
                ShowColumnDefaultValue = false
            };

            ArrangeStrategyFactory.Create(sourceType).Execute(context);

            Assert.IsNotNull(context.SortedData);
            Assert.AreEqual(result.Data.Rows.Count, context.SortedData.Rows.Count);

            return context.SortedData;
        }

        public static void AssertColumn(ColumnInfoCollector collector, string columnName, string expectedBaseDataType, CategoryDataTypeKind expectedCategory,
                                        SpecialDataTypeKind? expectedSpecialKind = null, bool? expectedIsArray = null)
        {
            var info = GetColumnInfo(collector, columnName);

            Assert.AreEqual(expectedBaseDataType, info.BaseDataType, true, $"Unexpected BaseDataType for {columnName}.");
            Assert.AreEqual(expectedCategory, info.CategoryDataTypeKind, $"Unexpected CategoryDataTypeKind for {columnName}.");

            if (expectedSpecialKind.HasValue)
            {
                Assert.AreEqual(expectedSpecialKind.Value, info.SpecialDataTypeKind, $"Unexpected SpecialDataTypeKind for {columnName}.");
            }

            if (expectedIsArray.HasValue)
            {
                Assert.AreEqual(expectedIsArray.Value, info.IsArray, $"Unexpected IsArray for {columnName}.");
            }
        }

        public static LargeTextDataType GetLargeText(DataTable arranged, string columnName)
        {
            var value = arranged.Rows[0][columnName] as LargeTextDataType;

            Assert.IsNotNull(value, $"Expected LargeTextDataType: {columnName}");

            return value;
        }

        public static LargeBinaryDataType GetLargeBinary(DataTable arranged, string columnName)
        {
            var value = arranged.Rows[0][columnName] as LargeBinaryDataType;

            Assert.IsNotNull(value, $"Expected LargeBinaryDataType: {columnName}");

            return value;
        }

        public static void AssertContains(string expectedText, object value, string columnName)
        {
            var actual = Convert.ToString(value) ?? string.Empty;

            StringAssert.Contains(actual, expectedText, $"Unexpected value for {columnName}.");
        }
    }
}
