using JasonQuery.UI.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data;

namespace JasonQuery.Tests.UI.Forms
{
    [TestClass]
    public sealed class QueryResultExportCorrectnessTests
    {
        private static readonly string[] OriginalOrder = { "id", "收入", "備註" };
        private static readonly string[] Reordered = { "備註", "id", "收入" };

        private static DataTable Sample()
        {
            var table = new DataTable();
            table.Columns.Add("id", typeof(string));
            table.Columns.Add("收入", typeof(string));
            table.Columns.Add("備註", typeof(string));
            table.Rows.Add("1", "<NULL>", "NULL value");
            table.Rows.Add("2", "125.50", "Normal value");
            table.Rows.Add("3", "300.00", "Normal value");
            return table;
        }

        private static DataTable Export(DataTable source, IEnumerable<string> order, string nullMarker = "<NULL>")
        {
            return QueryForm.BuildExportDataTable(source, order, field => field == "id" || field == "收入", nullMarker);
        }

        [TestMethod]
        public void EX01_NoFilter_NormalOrder_NullNumericDoesNotThrow()
        {
            var data = Export(Sample(), OriginalOrder);

            Assert.AreEqual(3, data.Rows.Count);
            Assert.AreEqual(typeof(decimal), data.Columns["收入"].DataType);
            Assert.IsTrue(data.Rows[0].IsNull("收入"));
            Assert.AreEqual(125.50m, data.Rows[1]["收入"]);
            Assert.AreEqual(300.00m, data.Rows[2]["收入"]);
        }

        [TestMethod]
        public void EX02_DefaultViewFilter_NormalOrder_UsesFilteredRows()
        {
            var source = Sample();

            source.DefaultView.RowFilter = "[id] >= '2'";

            var data = Export(source, OriginalOrder);

            Assert.AreEqual(2, data.Rows.Count);
            Assert.AreEqual(2m, data.Rows[0]["id"]);
            Assert.AreEqual(3, source.Rows.Count);
        }

        [TestMethod]
        public void EX03_NoFilter_ReorderedColumns_NullNumericDoesNotThrow()
        {
            var data = Export(Sample(), Reordered);

            Assert.AreEqual("備註", data.Columns[0].ColumnName);
            Assert.AreEqual("id", data.Columns[1].ColumnName);
            Assert.AreEqual("收入", data.Columns[2].ColumnName);
            Assert.IsTrue(data.Rows[0].IsNull("收入"));
        }

        [TestMethod]
        public void EX04_DefaultViewFilter_ReorderedColumns_CombinesBothContracts()
        {
            var source = Sample();

            source.DefaultView.RowFilter = "[id] >= '2'";

            var data = Export(source, Reordered);

            Assert.AreEqual(2, data.Rows.Count);
            Assert.AreEqual("備註", data.Columns[0].ColumnName);
            Assert.AreEqual("Normal value", data.Rows[0]["備註"]);
            Assert.AreEqual(125.50m, data.Rows[0]["收入"]);
        }

        [TestMethod]
        public void NumericNullAsNone_EmptyDisplayBecomesDbNull()
        {
            var source = Sample();

            source.Rows[0]["收入"] = string.Empty;

            var data = Export(source, OriginalOrder, "NONE");

            Assert.IsTrue(data.Rows[0].IsNull("收入"));
        }

        [TestMethod]
        public void TextEmptyUnderNone_RemainsEmptyString()
        {
            var source = Sample();

            source.Rows[0]["備註"] = string.Empty;

            var data = Export(source, OriginalOrder, "NONE");

            Assert.AreEqual(string.Empty, data.Rows[0]["備註"]);
        }

        [TestMethod]
        public void NumericNullAsDbNull_RemainsDbNull()
        {
            var source = Sample();

            source.Rows[0]["收入"] = DBNull.Value;

            var data = Export(source, OriginalOrder);

            Assert.IsTrue(data.Rows[0].IsNull("收入"));
        }

        [TestMethod]
        public void LargeNumericOutsideDecimal_PreservedAsTextInsteadOfFailing()
        {
            var source = Sample();

            const string number = "9999999999999999999999999999999999999999";

            source.Rows[1]["收入"] = number;

            var data = Export(source, OriginalOrder);

            Assert.AreEqual(typeof(string), data.Columns["收入"].DataType);
            Assert.AreEqual(number, data.Rows[1]["收入"]);
            Assert.IsTrue(data.Rows[0].IsNull("收入"));
        }

        [TestMethod]
        public void UnparseableNumeric_IsPreservedWithoutDiscardingOtherRows()
        {
            var source = Sample();

            source.Rows[1]["收入"] = "not-a-number";

            var data = Export(source, OriginalOrder);

            Assert.AreEqual(typeof(string), data.Columns["收入"].DataType);
            Assert.AreEqual("not-a-number", data.Rows[1]["收入"]);
        }

        [TestMethod]
        public void DecimalPrecision_IsPreservedInTypedResult()
        {
            var source = Sample();

            source.Rows[1]["收入"] = "123456789.123456789";

            var data = Export(source, OriginalOrder);

            Assert.AreEqual(123456789.123456789m, data.Rows[1]["收入"]);
        }

        [TestMethod]
        public void DisplayColumnOrder_IgnoresDuplicatesAndUnavailableFields()
        {
            var data = Export(Sample(), new[] { "備註", "missing", "收入", "收入", "id" });

            Assert.AreEqual(3, data.Columns.Count);
            Assert.AreEqual("備註", data.Columns[0].ColumnName);
            Assert.AreEqual("收入", data.Columns[1].ColumnName);
        }

        [TestMethod]
        public void DefaultViewSort_PreservesDisplayedRowOrder()
        {
            var source = Sample();

            source.DefaultView.Sort = "[id] DESC";

            var data = Export(source, OriginalOrder);

            Assert.AreEqual(3m, data.Rows[0]["id"]);
            Assert.AreEqual(1m, data.Rows[2]["id"]);
        }

        [TestMethod]
        public void EmptyFilterResult_ReturnsSchemaWithoutRows()
        {
            var source = Sample();

            source.DefaultView.RowFilter = "[id] = '99'";

            var data = Export(source, OriginalOrder);

            Assert.AreEqual(0, data.Rows.Count);
            Assert.AreEqual(3, data.Columns.Count);
        }
    }
}
