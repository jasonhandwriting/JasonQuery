using JasonQuery.Core.QueryEngine.Types;
using JasonQuery.UI.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;
using System.Globalization;
using System.Linq;

namespace JasonQuery.Tests.UI.Forms
{
    [TestClass]
    public sealed class QueryResultMultiFilterCorrectnessTests
    {
        private static DataTable Sample()
        {
            var table = new DataTable { Locale = CultureInfo.InvariantCulture };

            table.Columns.Add("id", typeof(int));
            table.Columns.Add("收入", typeof(string));
            table.Columns.Add("備註", typeof(string));

            table.Rows.Add(1, "<NULL>", "NULL value");
            table.Rows.Add(2, "125.50", "Winnie Chien");
            table.Rows.Add(3, "300", "Normal");
            table.Rows.Add(4, "-12.75", "Winnie's");
            table.Rows.Add(5, "30", "事業處");
            table.Rows.Add(6, "abc", "[N] literal");
            table.Rows.Add(7, "1000", "100% done");
            table.Rows.Add(8, "10", "literal *");
            table.Rows.Add(9, "99", "A_B");

            return table;
        }

        private static QueryForm.QueryResultFilterInput Filter(string field, string input, bool numeric = false)
        {
            return new QueryForm.QueryResultFilterInput(field, input, numeric);
        }

        private static string SelectIds(DataTable table, params QueryForm.QueryResultFilterInput[] conditions)
        {
            Assert.IsTrue(QueryForm.TryApplyQueryResultFilter(table, conditions, out _), "Failed to apply a supported filter condition.");
            return string.Join(",", table.DefaultView.Cast<DataRowView>().Select(row => (int)row.Row["id"]));
        }

        [TestMethod]
        [DataRow("Winnie", "2,4")]
        [DataRow("事業處", "5")]
        [DataRow("Winnie's", "4")]
        [DataRow("[N]", "6")]
        [DataRow("100%", "7")]
        [DataRow("*", "8")]
        [DataRow("A_B", "9")]
        [DataRow("", "1,2,3,4,5,6,7,8,9")]
        public void TextContains_LiteralInput_IsSafe(string text, string expected)
        {
            Assert.AreEqual(expected, SelectIds(Sample(), Filter("備註", text)));
        }

        [TestMethod]
        [DataRow(">100", "2,3,7")]
        [DataRow(">=100", "2,3,7")]
        [DataRow("<100", "4,5,8,9")]
        [DataRow("<=100", "4,5,8,9")]
        [DataRow("=125.50", "2")]
        [DataRow("!=125.50", "3,4,5,7,8,9")]
        [DataRow("<>125.50", "3,4,5,7,8,9")]
        [DataRow(">9", "2,3,5,7,8,9")]
        [DataRow(">=0", "2,3,5,7,8,9")]
        [DataRow(">1000000", "")]
        public void NumericOperators_UseDecimal_NotLexical(string input, string expected)
        {
            Assert.AreEqual(expected, SelectIds(Sample(), Filter("收入", input, numeric: true)));
        }

        [TestMethod]
        public void NumericBareInput_StillUsesSubstring()
        {
            Assert.AreEqual("7", SelectIds(Sample(), Filter("收入", "100", numeric: true)));
        }

        [TestMethod]
        public void MultipleConditions_AreAnded()
        {
            Assert.AreEqual("2", SelectIds(Sample(), Filter("收入", ">=100", numeric: true), Filter("備註", "Winnie")));
            Assert.AreEqual("", SelectIds(Sample(), Filter("收入", "<100", numeric: true), Filter("備註", "Normal")));
        }

        [TestMethod]
        [DataRow(">=abc")]
        [DataRow(">=")]
        [DataRow(">9999999999999999999999999999999999999")]
        [DataRow("BETWEEN 1 AND 9")]
        public void InvalidNumericInput_RejectsWithoutChangingPreviousView(string input)
        {
            var table = Sample();

            table.DefaultView.RowFilter = "[id] = 2";

            Assert.IsFalse(QueryForm.TryBuildQueryResultFilter(table, new[] { Filter("收入", input, numeric: true) }, out _));
            Assert.AreEqual("[id] = 2", table.DefaultView.RowFilter);
            Assert.AreEqual(1, table.DefaultView.Count);
        }

        [TestMethod]
        public void NumericNullMarkersAndNonNumericValues_DoNotThrowOrCompareAsZero()
        {
            var table = Sample();

            table.Rows[0]["收入"] = string.Empty;
            Assert.AreEqual("2,3,5,7,8,9", SelectIds(table, Filter("收入", ">=0", numeric: true)));
            table.Rows[0]["收入"] = "{NULL}";
            Assert.AreEqual("2,3,5,7,8,9", SelectIds(table, Filter("收入", ">=0", numeric: true)));
            table.Rows[0]["收入"] = "(NULL)";
            Assert.AreEqual("2,3,5,7,8,9", SelectIds(table, Filter("收入", ">=0", numeric: true)));
        }

        [TestMethod]
        public void NumericGuardCache_InvalidatesWhenRowsChange()
        {
            var table = Sample();

            Assert.AreEqual("2,3,7", SelectIds(table, Filter("收入", ">=100", numeric: true)));
            table.Rows[5]["收入"] = "200";
            Assert.AreEqual("2,3,6,7", SelectIds(table, Filter("收入", ">=100", numeric: true)));
            table.Rows[2]["收入"] = "bad data";
            Assert.AreEqual("2,6,7", SelectIds(table, Filter("收入", ">=100", numeric: true)));
        }

        [TestMethod]
        public void ActiveNumericFilter_ProtectsNewInvalidValueBeforeDataViewReevaluation()
        {
            var table = Sample();

            Assert.AreEqual("2,3,7", SelectIds(table, Filter("收入", ">=100", numeric: true)));

            //Do not explicitly regenerate the filter between edits. The safety guard must protect the DataView before DataRow.SetItem commits.
            table.Rows[2]["收入"] = "bad data";
            Assert.AreEqual("2,7", string.Join(",", table.DefaultView.Cast<DataRowView>().Select(row => (int)row.Row["id"])));

            table.Rows[2]["收入"] = "350";
            Assert.AreEqual("2,3,7", string.Join(",", table.DefaultView.Cast<DataRowView>().Select(row => (int)row.Row["id"])));
        }

        [TestMethod]
        public void DuplicateAndMissingFields_DoNotCreateBogusRowFilterColumns()
        {
            Assert.AreEqual
            (
                "2,3,7",
                SelectIds(Sample(),
                Filter("missing", "N"),
                Filter("收入", ">=100", numeric: true),
                Filter("收入", "<0", numeric: true))
            );
        }

        [TestMethod]
        public void DisplayCaptionAndColumnMove_DoNotAffectDataFieldMapping()
        {
            var table = Sample();

            table.Columns["收入"].SetOrdinal(2);
            Assert.AreEqual("2,3,7", SelectIds(table, Filter("收入", ">=100", numeric: true)));
        }

        [TestMethod]
        public void SortingAndClearing_PreserveRowsAndOrder()
        {
            var table = Sample();

            table.DefaultView.Sort = "[id] DESC";
            Assert.AreEqual("7,3,2", SelectIds(table, Filter("收入", ">=100", numeric: true)));
            Assert.AreEqual("9,8,7,6,5,4,3,2,1", SelectIds(table, Filter("收入", "", numeric: true)));
            Assert.AreEqual(9, table.Rows.Count);
        }

        [TestMethod]
        public void DataFieldWithBracket_IsSafelyQuoted()
        {
            var table = new DataTable { Locale = CultureInfo.InvariantCulture };

            table.Columns.Add("id", typeof(int));
            table.Columns.Add("a]b", typeof(string));
            table.Rows.Add(1, "99");
            table.Rows.Add(2, "101");
            Assert.AreEqual("2", SelectIds(table, Filter("a]b", ">=100", numeric: true)));
        }

        [TestMethod]
        public void NumericGuard_RejectsExcessiveInvalidValueCardinality()
        {
            var table = Sample();

            for (var i = 0; i < 140; i++)
            {
                table.Rows.Add(i + 10, "not-a-number-" + i.ToString(CultureInfo.InvariantCulture), "memo");
            }

            Assert.IsFalse(QueryForm.TryBuildQueryResultFilter(table, new[] { Filter("收入", ">=1", numeric: true) }, out _));
        }

        private static DataTable LargeTextSample()
        {
            var table = new DataTable { Locale = CultureInfo.InvariantCulture };

            table.Columns.Add("id", typeof(int));
            table.Columns.Add("amount", typeof(string));
            table.Columns.Add("memo", typeof(LargeTextDataType));
            table.Rows.Add(1, "<NULL>", LargeTextDataType.CreateNull("<NULL>"));
            table.Rows.Add(2, "125.5", new LargeTextDataType("", "Winnie Chien", 12, false, () => throw new InvalidOperationException("LOB content must not be loaded by a display filter.")));
            table.Rows.Add(3, "300", new LargeTextDataType("", "Normal value", 12, false, () => throw new InvalidOperationException("LOB content must not be loaded by a display filter.")));

            return table;
        }

        [TestMethod]
        [DataRow("Winnie", "2")]
        [DataRow("Normal", "3")]
        public void LargeTextWrapper_UsesDisplayPreviewForContains(string text, string expected)
        {
            var table = LargeTextSample();

            Assert.AreEqual(typeof(LargeTextDataType), table.Columns["memo"].DataType);
            Assert.AreEqual(expected, SelectIds(table, Filter("memo", text)));
        }

        [TestMethod]
        public void LargeTextWrapper_CombinesWithNumericFilter()
        {
            Assert.AreEqual("2", SelectIds(LargeTextSample(), Filter("amount", ">=125.5", numeric: true), Filter("memo", "Winnie")));
        }

        [TestMethod]
        public void LargeTextWrapper_ClearPreservesDefaultViewSort()
        {
            var table = LargeTextSample();

            table.DefaultView.Sort = "[id] DESC";
            Assert.AreEqual("2", SelectIds(table, Filter("memo", "Winnie")));
            Assert.AreEqual("3,2,1", SelectIds(table, Filter("memo", "")));
        }

        [TestMethod]
        public void LargeBinaryWrapper_UsesDisplayTextForContains()
        {
            var table = new DataTable { Locale = CultureInfo.InvariantCulture };

            table.Columns.Add("id", typeof(int));
            table.Columns.Add("blob", typeof(LargeBinaryDataType));
            table.Rows.Add(1, new LargeBinaryDataType("(BLOB)(100)", "", 100, true, () => throw new InvalidOperationException("Binary content must not be loaded by a display filter.")));
            table.Rows.Add(2, new LargeBinaryDataType("(BLOB)(200)", "", 200, true, () => throw new InvalidOperationException("Binary content must not be loaded by a display filter.")));
            Assert.AreEqual("2", SelectIds(table, Filter("blob", "200")));
        }

        [TestMethod]
        public void LargeDecimalOutsideRange_IsSafelyExcluded()
        {
            var table = Sample();

            table.Rows[5]["收入"] = "9999999999999999999999999999999999999999";
            Assert.AreEqual("2,3,7", SelectIds(table, Filter("收入", ">=100", numeric: true)));
        }
    }
}
