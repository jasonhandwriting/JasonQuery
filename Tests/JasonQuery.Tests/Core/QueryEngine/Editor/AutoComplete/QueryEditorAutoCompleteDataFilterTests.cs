using JasonQuery.Core.QueryEngine.Editor.AutoComplete.Support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;
using System.Linq;

namespace JasonQuery.Tests.Core.QueryEngine.Editor.AutoComplete
{
    [TestClass]
    public sealed class QueryEditorAutoCompleteDataFilterTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        public void FilterByKeyword_WithNullSource_ReturnsEmptyTable()
        {
            var result = QueryEditorAutoCompleteDataFilter.FilterByKeyword(null, "abc", "ColumnName");

            Assert.IsNotNull(result);
            Assert.IsEmpty(result.Rows);
            Assert.IsEmpty(result.Columns);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        public void FilterByKeyword_WithMissingMatchColumn_ReturnsCopy()
        {
            var source = CreateTable("SchemaName", "A", "B");
            var result = QueryEditorAutoCompleteDataFilter.FilterByKeyword(source, "A", "ColumnName");

            CollectionAssert.AreEqual(new[] { "A", "B" }, GetValues(result, "SchemaName"));
            Assert.AreNotSame(source, result);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("   ")]
        public void FilterByKeyword_WithEmptyKeyword_ReturnsCopy(string keyword)
        {
            var source = CreateTable("ColumnName", "B", "A");
            var result = QueryEditorAutoCompleteDataFilter.FilterByKeyword(source, keyword, "ColumnName");

            CollectionAssert.AreEqual(new[] { "B", "A" }, GetValues(result, "ColumnName"));
            Assert.AreNotSame(source, result);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        public void FilterByKeyword_OrdersMatchBuckets()
        {
            var source = CreateTable("ColumnName", "supercustomer", "order_customer", "CustomerCode", "cust", "OrderCustomer", "customer");
            var result = QueryEditorAutoCompleteDataFilter.FilterByKeyword(source, "cust", "ColumnName");

            CollectionAssert.AreEqual
            (
                new[]
                {
                    "cust",
                    "customer",
                    "CustomerCode",
                    "OrderCustomer",
                    "order_customer",
                    "supercustomer"
                },
                GetValues(result, "ColumnName")
            );
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        public void FilterByKeyword_IsCaseInsensitive()
        {
            var source = CreateTable("ColumnName", "CustomerID", "ORDERID");
            var result = QueryEditorAutoCompleteDataFilter.FilterByKeyword(source, "customer", "ColumnName");

            CollectionAssert.AreEqual
            (
                new[] { "CustomerID" },
                GetValues(result, "ColumnName")
            );
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        public void FilterByKeyword_OneCharacterKeywordDoesNotUseBoundaryBucket()
        {
            var source = CreateTable("ColumnName", "A_B", "ZA", "BA");
            var result = QueryEditorAutoCompleteDataFilter.FilterByKeyword(source, "B", "ColumnName");

            CollectionAssert.AreEqual
            (
                new[] { "BA", "A_B" },
                GetValues(result, "ColumnName")
            );
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        public void FilterByKeyword_NoMatchAndFallbackDisabled_ReturnsEmptyClone()
        {
            var source = CreateTable("ColumnName", "A", "B");
            var result = QueryEditorAutoCompleteDataFilter.FilterByKeyword(source, "XYZ", "ColumnName", false);

            Assert.IsEmpty(result.Rows);
            Assert.HasCount(source.Columns.Count, result.Columns);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        public void FilterByKeyword_NoMatchAndFallbackEnabled_ReturnsCopy()
        {
            var source = CreateTable("ColumnName", "B", "A");
            var result = QueryEditorAutoCompleteDataFilter.FilterByKeyword(source, "XYZ", "ColumnName", true);

            CollectionAssert.AreEqual(new[] { "B", "A" }, GetValues(result, "ColumnName"));
            Assert.AreNotSame(source, result);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        public void FilterByKeyword_SameBucketOrdersByLengthThenAlphabetically()
        {
            var source = CreateTable("ColumnName", "CustomerLongName", "CustomerB", "CustomerA");
            var result = QueryEditorAutoCompleteDataFilter.FilterByKeyword(source, "Customer", "ColumnName");

            CollectionAssert.AreEqual
            (
                new[]
                {
                    "CustomerA",
                    "CustomerB",
                    "CustomerLongName"
                },
                GetValues(result, "ColumnName")
            );
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        public void FilterByKeyword_UsesOriginalOrderAsFinalTieBreaker()
        {
            var source = new DataTable();

            source.Columns.Add("ColumnName");
            source.Columns.Add("DataType");

            source.Rows.Add("Customer", "varchar");
            source.Rows.Add("customer", "varchar");

            var result = QueryEditorAutoCompleteDataFilter.FilterByKeyword
            (
                source,
                "customer",
                "ColumnName",
                false,
                new[] { "DataType" }
            );

            Assert.AreEqual("Customer", result.Rows[0]["ColumnName"]);
            Assert.AreEqual("customer", result.Rows[1]["ColumnName"]);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        public void FilterPeriodByKeyword_UsesColumnName()
        {
            var source = new DataTable();

            source.Columns.Add("ColumnName");
            source.Columns.Add("DataType");
            source.Columns.Add("BaseTableName");

            source.Rows.Add("CustomerID", "int", "[Customer]");
            source.Rows.Add("OrderID", "int", "[Order]");

            var result = QueryEditorAutoCompleteDataFilter.FilterPeriodByKeyword(source, "cust");

            CollectionAssert.AreEqual(new[] { "CustomerID" }, GetValues(result, "ColumnName"));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        public void FilterSpaceByKeyword_PrefersSchemaName()
        {
            var source = new DataTable();

            source.Columns.Add("SchemaName");
            source.Columns.Add("ColumnName");

            source.Rows.Add("Customer", "NoMatch");
            source.Rows.Add("Order", "Customer");

            var result = QueryEditorAutoCompleteDataFilter.FilterSpaceByKeyword(source, "Customer");

            Assert.HasCount(1, result.Rows);
            Assert.AreEqual("Customer", result.Rows[0]["SchemaName"]);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        public void FilterSpaceByKeyword_UsesColumnNameWhenSchemaNameMissing()
        {
            var source = CreateTable("ColumnName", "CustomerID", "OrderID");
            var result = QueryEditorAutoCompleteDataFilter.FilterSpaceByKeyword(source, "cust");

            CollectionAssert.AreEqual(new[] { "CustomerID" }, GetValues(result, "ColumnName"));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        public void FilterSpaceByKeyword_WithNoSupportedColumnReturnsCopy()
        {
            var source = CreateTable("Name", "A", "B");
            var result = QueryEditorAutoCompleteDataFilter.FilterSpaceByKeyword(source, "A");

            CollectionAssert.AreEqual(new[] { "A", "B" }, GetValues(result, "Name"));
            Assert.AreNotSame(source, result);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        public void FilterSpaceByKeyword_WithNullSourceReturnsEmptyTable()
        {
            var result = QueryEditorAutoCompleteDataFilter.FilterSpaceByKeyword(null, "A");

            Assert.IsNotNull(result);
            Assert.IsEmpty(result.Rows);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        public void FilterByKeyword_DoesNotMutateSource()
        {
            var source = CreateTable("ColumnName", "OrderID", "CustomerID");
            var result = QueryEditorAutoCompleteDataFilter.FilterByKeyword(source, "Customer", "ColumnName");

            CollectionAssert.AreEqual
            (
                new[] { "OrderID", "CustomerID" },
                GetValues(source, "ColumnName")
            );

            CollectionAssert.AreEqual
            (
                new[] { "CustomerID" },
                GetValues(result, "ColumnName")
            );
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("AutoComplete")]
        public void FilterByKeyword_PreservesAllColumnsAndValues()
        {
            var source = new DataTable();

            source.Columns.Add("ColumnName");
            source.Columns.Add("DataType");
            source.Columns.Add("AllowDBNull");

            source.Rows.Add("CustomerID", "int", "P");

            var result = QueryEditorAutoCompleteDataFilter.FilterByKeyword(source, "Customer", "ColumnName");

            Assert.HasCount(3, result.Columns);
            Assert.AreEqual("int", result.Rows[0]["DataType"]);
            Assert.AreEqual("P", result.Rows[0]["AllowDBNull"]);
        }

        private static DataTable CreateTable(string columnName, params string[] values)
        {
            var table = new DataTable();

            table.Columns.Add(columnName);

            foreach (var value in values)
            {
                table.Rows.Add(value);
            }

            return table;
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
