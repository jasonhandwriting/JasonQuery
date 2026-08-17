using Microsoft.VisualStudio.TestTools.UnitTesting;
using JasonQuery.Core.Database.DmlPreview;
using System;
using System.Data;

namespace JasonQuery.Tests.Core.Database.DmlPreview
{
    [TestClass]
    public sealed class DmlPreviewOriginalRowResolverTests
    {
        private const string RowIdentityColumn = "R_O_W_1_D_P_K_J_Q";

        [TestMethod]
        [TestCategory("Safety")]
        public void TryResolve_WithNullCurrentRow_ReturnsFalse()
        {
            var originalTable = CreateTable();
            var success = DmlPreviewOriginalRowResolver.TryResolve(null, originalTable, RowIdentityColumn, out DataRow originalRow);

            Assert.IsFalse(success);
            Assert.IsNull(originalRow);
        }

        [TestMethod]
        [TestCategory("Safety")]
        public void TryResolve_WithNullOriginalTable_ReturnsFalse()
        {
            var currentRow = CreateTable().NewRow();
            var success = DmlPreviewOriginalRowResolver.TryResolve(currentRow, null, RowIdentityColumn, out DataRow originalRow);

            Assert.IsFalse(success);
            Assert.IsNull(originalRow);
        }

        [TestMethod]
        [TestCategory("Safety")]
        public void TryResolve_WithEmptyOriginalTable_ReturnsFalse()
        {
            var currentTable = CreateTable();
            var currentRow = AddRow(currentTable, "001", "current");
            var success = DmlPreviewOriginalRowResolver.TryResolve(currentRow, CreateTable(), RowIdentityColumn, out DataRow originalRow);

            Assert.IsFalse(success);
            Assert.IsNull(originalRow);
        }

        [TestMethod]
        [TestCategory("Safety")]
        public void TryResolve_WithBlankIdentityColumnName_ReturnsFalse()
        {
            var currentTable = CreateTable();
            var originalTable = CreateTable();
            var currentRow = AddRow(currentTable, "001", "current");

            AddRow(originalTable, "001", "original");

            var success = DmlPreviewOriginalRowResolver.TryResolve(currentRow, originalTable, " ", out DataRow originalRow);

            Assert.IsFalse(success);
            Assert.IsNull(originalRow);
        }

        [TestMethod]
        [TestCategory("Safety")]
        public void TryResolve_WhenCurrentTableDoesNotContainIdentityColumn_ReturnsFalse()
        {
            var currentTable = new DataTable();

            currentTable.Columns.Add("Value", typeof(string));

            var currentRow = currentTable.Rows.Add("current");
            var originalTable = CreateTable();

            AddRow(originalTable, "001", "original");

            var success = DmlPreviewOriginalRowResolver.TryResolve(currentRow, originalTable, RowIdentityColumn, out DataRow originalRow);

            Assert.IsFalse(success);
            Assert.IsNull(originalRow);
        }

        [TestMethod]
        [TestCategory("Safety")]
        public void TryResolve_WhenOriginalTableDoesNotContainIdentityColumn_ReturnsFalse()
        {
            var currentTable = CreateTable();
            var currentRow = AddRow(currentTable, "001", "current");
            var originalTable = new DataTable();

            originalTable.Columns.Add("Value", typeof(string));
            originalTable.Rows.Add("original");

            var success = DmlPreviewOriginalRowResolver.TryResolve(currentRow, originalTable, RowIdentityColumn, out DataRow originalRow);

            Assert.IsFalse(success);
            Assert.IsNull(originalRow);
        }

        [TestMethod]
        [TestCategory("Safety")]
        public void TryResolve_WithDbNullIdentity_ReturnsFalse()
        {
            var currentTable = CreateTable();
            var currentRow = currentTable.NewRow();

            currentRow[RowIdentityColumn] = DBNull.Value;
            currentRow["Value"] = "current";
            currentTable.Rows.Add(currentRow);

            var originalTable = CreateTable();

            AddRow(originalTable, "001", "original");

            var success = DmlPreviewOriginalRowResolver.TryResolve(currentRow, originalTable, RowIdentityColumn, out DataRow originalRow);

            Assert.IsFalse(success);
            Assert.IsNull(originalRow);
        }

        [TestMethod]
        [TestCategory("Safety")]
        public void TryResolve_WithEmptyIdentity_ReturnsFalse()
        {
            var currentTable = CreateTable();
            var currentRow = AddRow(currentTable, string.Empty, "current");
            var originalTable = CreateTable();

            AddRow(originalTable, string.Empty, "original");

            var success = DmlPreviewOriginalRowResolver.TryResolve(currentRow, originalTable, RowIdentityColumn, out DataRow originalRow);

            Assert.IsFalse(success);
            Assert.IsNull(originalRow);
        }

        [TestMethod]
        [TestCategory("Safety")]
        public void TryResolve_WhenIdentityDoesNotMatch_ReturnsFalse()
        {
            var currentTable = CreateTable();
            var currentRow = AddRow(currentTable, "001", "current");
            var originalTable = CreateTable();

            AddRow(originalTable, "002", "original");

            var success = DmlPreviewOriginalRowResolver.TryResolve(currentRow, originalTable, RowIdentityColumn, out DataRow originalRow);

            Assert.IsFalse(success);
            Assert.IsNull(originalRow);
        }

        [TestMethod]
        [TestCategory("Safety")]
        public void TryResolve_WhenIdentityDiffersOnlyByCase_ReturnsFalse()
        {
            var currentTable = CreateTable();
            var currentRow = AddRow(currentTable, "abc", "current");
            var originalTable = CreateTable();

            AddRow(originalTable, "ABC", "original");

            var success = DmlPreviewOriginalRowResolver.TryResolve(currentRow, originalTable, RowIdentityColumn, out DataRow originalRow);

            Assert.IsFalse(success);
            Assert.IsNull(originalRow);
        }

        [TestMethod]
        [TestCategory("Safety")]
        public void TryResolve_WithDuplicateOriginalIdentity_ReturnsFalse()
        {
            var currentTable = CreateTable();
            var currentRow = AddRow(currentTable, "001", "current");
            var originalTable = CreateTable();

            AddRow(originalTable, "001", "original-1");
            AddRow(originalTable, "001", "original-2");

            var success = DmlPreviewOriginalRowResolver.TryResolve(currentRow, originalTable, RowIdentityColumn, out DataRow originalRow);

            Assert.IsFalse(success);
            Assert.IsNull(originalRow);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void TryResolve_WithSingleMatch_ReturnsOriginalRow()
        {
            var currentTable = CreateTable();
            var currentRow = AddRow(currentTable, "001", "current");
            var originalTable = CreateTable();
            var expectedRow = AddRow(originalTable, "001", "original");
            var success = DmlPreviewOriginalRowResolver.TryResolve(currentRow, originalTable, RowIdentityColumn, out DataRow originalRow);

            Assert.IsTrue(success);
            Assert.AreSame(expectedRow, originalRow);
            Assert.AreEqual("original", originalRow["Value"]);
        }

        [TestMethod]
        [TestCategory("Regression")]
        public void TryResolve_WhenCurrentValueWasModified_ReturnsUnmodifiedOriginalRow()
        {
            var currentTable = CreateTable();
            var currentRow = AddRow(currentTable, "001", "new-key");
            var originalTable = CreateTable();

            AddRow(originalTable, "001", "old-key");

            var success = DmlPreviewOriginalRowResolver.TryResolve(currentRow, originalTable, RowIdentityColumn, out DataRow originalRow);

            Assert.IsTrue(success);
            Assert.AreEqual("old-key", originalRow["Value"]);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void TryResolve_WithNumericIdentity_UsesTextRepresentation()
        {
            var currentTable = CreateNumericIdentityTable();
            var currentRow = currentTable.Rows.Add(12, "current");
            var originalTable = CreateNumericIdentityTable();
            var expectedRow = originalTable.Rows.Add(12, "original");
            var success = DmlPreviewOriginalRowResolver.TryResolve(currentRow, originalTable, RowIdentityColumn, out DataRow originalRow);

            Assert.IsTrue(success);
            Assert.AreSame(expectedRow, originalRow);
        }

        private static DataTable CreateTable()
        {
            var table = new DataTable();

            table.Columns.Add(RowIdentityColumn, typeof(string));
            table.Columns.Add("Value", typeof(string));

            return table;
        }

        private static DataTable CreateNumericIdentityTable()
        {
            var table = new DataTable();

            table.Columns.Add(RowIdentityColumn, typeof(int));
            table.Columns.Add("Value", typeof(string));

            return table;
        }

        private static DataRow AddRow(DataTable table, string identity, string value)
        {
            var row = table.NewRow();

            row[RowIdentityColumn] = identity;
            row["Value"] = value;
            table.Rows.Add(row);

            return row;
        }
    }
}