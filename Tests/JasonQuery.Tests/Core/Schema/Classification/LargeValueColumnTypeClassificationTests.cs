using JasonLibrary.Core.Schema.Enums;
using JasonLibrary.Providers.Oracle.Mapping;
using JasonLibrary.Providers.PostgreSql.Mapping;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonQuery.Tests.Core.Schema.Classification
{
    [TestClass]
    public sealed class LargeValueColumnTypeClassificationTests
    {
        [TestMethod]
        [TestCategory("Regression")]
        [DataRow("CLOB")]
        [DataRow("NCLOB")]
        [DataRow("LONG")]
        [DataRow("XMLTYPE")]
        [DataRow("JSON")]
        public void OracleResolver_LargeTextTypes_ReturnLargeText(string baseDataType)
        {
            var result = OracleColumnTypeResolver.Resolve(baseDataType, "System.String", 0, 0, 0, string.Empty);

            Assert.AreEqual(CategoryDataTypeKind.LargeText, result.CategoryDataTypeKind);
        }

        [TestMethod]
        [TestCategory("Regression")]
        [DataRow("RAW")]
        [DataRow("LONG RAW")]
        [DataRow("BLOB")]
        public void OracleResolver_BinaryTypes_ReturnLargeBinary(string baseDataType)
        {
            var result = OracleColumnTypeResolver.Resolve(baseDataType, "System.Byte[]", 0, 0, 0, string.Empty);

            Assert.AreEqual(CategoryDataTypeKind.LargeBinary, result.CategoryDataTypeKind);
        }

        [TestMethod]
        [TestCategory("Regression")]
        public void PostgreSqlResolver_Bytea_ReturnsLargeBinary()
        {
            var result = PostgreSqlColumnTypeResolver.Resolve(17, 0, 0, 0);

            Assert.AreEqual("bytea", result.BaseDataType);
            Assert.AreEqual(CategoryDataTypeKind.LargeBinary, result.CategoryDataTypeKind);
            Assert.IsFalse(result.IsArray);
        }

        [TestMethod]
        [TestCategory("Regression")]
        [DataRow(1001, "bytea[]")]
        [DataRow(1015, "character varying[]")]
        public void PostgreSqlResolver_ArrayTypes_ReturnLargeText(int providerType, string expectedBaseDataType)
        {
            var result = PostgreSqlColumnTypeResolver.Resolve(providerType, 0, 0, 0);

            Assert.AreEqual(expectedBaseDataType, result.BaseDataType);
            Assert.AreEqual(CategoryDataTypeKind.LargeText, result.CategoryDataTypeKind);
            Assert.IsTrue(result.IsArray);
        }

        [TestMethod]
        [TestCategory("Regression")]
        [DataRow(25, "text")]
        [DataRow(114, "json")]
        [DataRow(3802, "jsonb")]
        public void PostgreSqlResolver_LargeTextTypes_ReturnLargeText(int providerType, string expectedBaseDataType)
        {
            var result = PostgreSqlColumnTypeResolver.Resolve(providerType, 0, 0, 0);

            Assert.AreEqual(expectedBaseDataType, result.BaseDataType);
            Assert.AreEqual(CategoryDataTypeKind.LargeText, result.CategoryDataTypeKind);
        }

        [TestMethod]
        [TestCategory("Regression")]
        [DataRow("binary")]
        [DataRow("varbinary")]
        [DataRow("image")]
        public void SqlServerResolver_BinaryTypes_ReturnLargeBinary(string baseDataType)
        {
            var result = SqlServerColumnTypeResolver.Resolve(baseDataType, "System.Byte[]", 50, 0, 0, string.Empty);

            Assert.AreEqual(CategoryDataTypeKind.LargeBinary, result.CategoryDataTypeKind);
        }

        [TestMethod]
        [TestCategory("Regression")]
        [DataRow("text")]
        [DataRow("ntext")]
        [DataRow("xml")]
        [DataRow("json")]
        [DataRow("vector")]
        public void SqlServerResolver_LargeTextTypes_ReturnLargeText(string baseDataType)
        {
            var result = SqlServerColumnTypeResolver.Resolve(baseDataType, "System.String", 0, 0, 0, string.Empty);

            Assert.AreEqual(CategoryDataTypeKind.LargeText, result.CategoryDataTypeKind);
        }

        [TestMethod]
        [TestCategory("Regression")]
        [DataRow("timestamp")]
        [DataRow("rowversion")]
        public void SqlServerResolver_RowVersionTypes_RemainShortString(string baseDataType)
        {
            var result = SqlServerColumnTypeResolver.Resolve(baseDataType, "System.Byte[]", 8, 0, 0, string.Empty);

            Assert.AreEqual(SpecialDataTypeKind.RowVersion, result.SpecialDataTypeKind);
            Assert.AreEqual(CategoryDataTypeKind.String, result.CategoryDataTypeKind);
        }

        [TestMethod]
        [TestCategory("Regression")]
        [DataRow(4, 255, "tinyblob")]
        [DataRow(4, 65535, "blob")]
        [DataRow(4, 16777215, "mediumblob")]
        [DataRow(4, 2147483647, "longblob")]
        public void MySqlResolver_BlobTypes_ReturnLargeBinary(int providerType, int columnSize, string expectedBaseDataType)
        {
            var result = MySqlColumnTypeResolver.Resolve(string.Empty, "System.Byte[]", columnSize, 0, 0, providerType);

            Assert.AreEqual(expectedBaseDataType, result.BaseDataType);
            Assert.AreEqual(CategoryDataTypeKind.LargeBinary, result.CategoryDataTypeKind);
        }

        [TestMethod]
        [TestCategory("Regression")]
        [DataRow(13, 255, "tinytext")]
        [DataRow(13, 65535, "text")]
        [DataRow(13, 16777215, "mediumtext")]
        [DataRow(13, 2147483647, "longtext")]
        [DataRow(22, 0, "json")]
        public void MySqlResolver_TextTypes_ReturnLargeText(int providerType, int columnSize, string expectedBaseDataType)
        {
            var result = MySqlColumnTypeResolver.Resolve(string.Empty, "System.String", columnSize, 0, 0, providerType);

            Assert.AreEqual(expectedBaseDataType, result.BaseDataType);
            Assert.AreEqual(CategoryDataTypeKind.LargeText, result.CategoryDataTypeKind);
        }

        [TestMethod]
        [TestCategory("Regression")]
        public void MySqlResolver_GeometryProviderFallback_RemainsString()
        {
            var result = MySqlColumnTypeResolver.Resolve("geometry", "System.Object", 0, 0, 0, 21);

            Assert.AreEqual("geometry", result.BaseDataType);
            Assert.AreEqual(CategoryDataTypeKind.String, result.CategoryDataTypeKind);
            Assert.IsTrue(result.UsedProviderFallback);
        }
    }
}