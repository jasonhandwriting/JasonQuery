using JasonQuery.Core.Database.Diagnostics.PostgreSql;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;
using System.Linq;

namespace JasonQuery.Tests.Core.Database.Diagnostics.PostgreSql
{
    [TestClass]
    public sealed class PostgreSqlColumnLengthMetadataProviderTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void GetColumnLengthInfo_WithNullDataTable_ReturnsEmptyCollection()
        {
            var provider = new PostgreSqlColumnLengthMetadataProvider(null);
            var actual = provider.GetColumnLengthInfo("public", "a_test");

            Assert.AreEqual(0, actual.Count);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void GetColumnLengthInfo_WithMissingRequiredColumns_ReturnsEmptyCollection()
        {
            var table = new DataTable();

            table.Columns.Add("SchemaNode", typeof(string));

            var provider = new PostgreSqlColumnLengthMetadataProvider(table);
            var actual = provider.GetColumnLengthInfo("public", "a_test");

            Assert.AreEqual(0, actual.Count);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void GetColumnLengthInfo_WithBlankTableName_ReturnsEmptyCollection()
        {
            var provider = new PostgreSqlColumnLengthMetadataProvider(CreateSchemaTable());
            var actual = provider.GetColumnLengthInfo("public", " ");

            Assert.AreEqual(0, actual.Count);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void GetColumnLengthInfo_WithBlankSchema_UsesPublicSchema()
        {
            var table = CreateSchemaTable();

            AddRow(table, "public", "a_test (0)", "t1, character varying(31)");

            var provider = new PostgreSqlColumnLengthMetadataProvider(table);
            var actual = provider.GetColumnLengthInfo(string.Empty, "a_test");

            Assert.AreEqual(1, actual.Count);
            Assert.AreEqual("public.a_test.t1", actual[0].FullColumnName);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        [TestCategory("Regression")]
        public void GetColumnLengthInfo_RemovesRowCountSuffixFromSchemaNameColumn()
        {
            var table = CreateSchemaTable();

            AddRow(table, "public", "a_test (123)", "t1, character varying(31)");

            var provider = new PostgreSqlColumnLengthMetadataProvider(table);
            var actual = provider.GetColumnLengthInfo("public", "a_test");

            Assert.AreEqual(1, actual.Count);
            Assert.AreEqual("a_test", actual[0].TableName);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void GetColumnLengthInfo_WithVarchar_NormalizesDataType()
        {
            var table = CreateSchemaTable();

            AddRow(table, "public", "a_test (0)", "t1, varchar(31)");

            var provider = new PostgreSqlColumnLengthMetadataProvider(table);
            var actual = provider.GetColumnLengthInfo("public", "a_test");

            Assert.AreEqual(1, actual.Count);
            Assert.AreEqual("character varying", actual[0].DataType);
            Assert.AreEqual(31, actual[0].CharacterMaximumLength);
            Assert.AreEqual("character varying(31)", actual[0].DisplayDataType);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void GetColumnLengthInfo_WithChar_NormalizesDataType()
        {
            var table = CreateSchemaTable();

            AddRow(table, "public", "a_test (0)", "t1, char(8)");

            var provider = new PostgreSqlColumnLengthMetadataProvider(table);
            var actual = provider.GetColumnLengthInfo("public", "a_test");

            Assert.AreEqual(1, actual.Count);
            Assert.AreEqual("character", actual[0].DataType);
            Assert.AreEqual(8, actual[0].CharacterMaximumLength);
            Assert.AreEqual("character(8)", actual[0].DisplayDataType);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void GetColumnLengthInfo_WithQuotedColumn_PreservesColumnCase()
        {
            var table = CreateSchemaTable();

            AddRow(table, "public", "a_test (0)", "\"MixedCase\", character varying(10)");

            var provider = new PostgreSqlColumnLengthMetadataProvider(table);
            var actual = provider.GetColumnLengthInfo("public", "a_test");

            Assert.AreEqual(1, actual.Count);
            Assert.AreEqual("MixedCase", actual[0].ColumnName);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void GetColumnLengthInfo_WithArrayType_IgnoresColumn()
        {
            var table = CreateSchemaTable();

            AddRow(table, "public", "a_test (0)", "t1, character varying(31)[]");

            var provider = new PostgreSqlColumnLengthMetadataProvider(table);
            var actual = provider.GetColumnLengthInfo("public", "a_test");

            Assert.AreEqual(0, actual.Count);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void GetColumnLengthInfo_WithNonCharacterType_IgnoresColumn()
        {
            var table = CreateSchemaTable();

            AddRow(table, "public", "a_test (0)", "id, integer");

            var provider = new PostgreSqlColumnLengthMetadataProvider(table);
            var actual = provider.GetColumnLengthInfo("public", "a_test");

            Assert.AreEqual(0, actual.Count);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void GetColumnLengthInfo_WithOtherSchemaOrTable_IgnoresRows()
        {
            var table = CreateSchemaTable();

            AddRow(table, "other", "a_test (0)", "t1, character varying(31)");
            AddRow(table, "public", "other_table (0)", "t2, character varying(32)");
            AddRow(table, "public", "a_test (0)", "t4, character varying(34)");

            var provider = new PostgreSqlColumnLengthMetadataProvider(table);
            var actual = provider.GetColumnLengthInfo("public", "a_test");

            Assert.AreEqual(1, actual.Count);
            Assert.AreEqual("t4", actual[0].ColumnName);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void GetColumnLengthInfo_AssignsOrdinalOnlyToSupportedMatchingColumns()
        {
            var table = CreateSchemaTable();

            AddRow(table, "public", "a_test (0)", "id, integer");
            AddRow(table, "public", "a_test (0)", "t1, character varying(31)");
            AddRow(table, "public", "a_test (0)", "arr, character varying(10)[]");
            AddRow(table, "public", "a_test (0)", "t2, character varying(32)");

            var provider = new PostgreSqlColumnLengthMetadataProvider(table);
            var actual = provider.GetColumnLengthInfo("public", "a_test");

            Assert.AreEqual(2, actual.Count);
            Assert.AreEqual(1, actual[0].OrdinalPosition);
            Assert.AreEqual("t1", actual[0].ColumnName);
            Assert.AreEqual(2, actual[1].OrdinalPosition);
            Assert.AreEqual("t2", actual[1].ColumnName);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void TryGetColumnLengthInfo_WithExistingColumn_ReturnsTrue()
        {
            var table = CreateSchemaTable();

            AddRow(table, "public", "a_test (0)", "t2, character varying(32)");

            var provider = new PostgreSqlColumnLengthMetadataProvider(table);
            var success = provider.TryGetColumnLengthInfo("public", "a_test", "T2", out PostgreSqlColumnLengthInfo columnInfo);

            Assert.IsTrue(success);
            Assert.IsNotNull(columnInfo);
            Assert.AreEqual("t2", columnInfo.ColumnName);
            Assert.AreEqual(32, columnInfo.CharacterMaximumLength);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void TryGetColumnLengthInfo_WithMissingColumn_ReturnsFalse()
        {
            var table = CreateSchemaTable();

            AddRow(table, "public", "a_test (0)", "t2, character varying(32)");

            var provider = new PostgreSqlColumnLengthMetadataProvider(table);
            var success = provider.TryGetColumnLengthInfo("public", "a_test", "missing", out PostgreSqlColumnLengthInfo columnInfo);

            Assert.IsFalse(success);
            Assert.IsNull(columnInfo);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("PostgreSql")]
        public void TryGetColumnLengthInfo_WithBlankColumn_ReturnsFalse()
        {
            var provider = new PostgreSqlColumnLengthMetadataProvider(CreateSchemaTable());
            var success = provider.TryGetColumnLengthInfo("public", "a_test", " ", out PostgreSqlColumnLengthInfo columnInfo);

            Assert.IsFalse(success);
            Assert.IsNull(columnInfo);
        }

        private static DataTable CreateSchemaTable()
        {
            var table = new DataTable();

            table.Columns.Add("SchemaNode", typeof(string));
            table.Columns.Add("SchemaName", typeof(string));
            table.Columns.Add("Schema_Browser", typeof(string));

            return table;
        }

        private static void AddRow(DataTable table, string schemaNode, string schemaName, string schemaBrowser)
        {
            var row = table.NewRow();

            row["SchemaNode"] = schemaNode;
            row["SchemaName"] = schemaName;
            row["Schema_Browser"] = schemaBrowser;

            table.Rows.Add(row);
        }
    }
}
