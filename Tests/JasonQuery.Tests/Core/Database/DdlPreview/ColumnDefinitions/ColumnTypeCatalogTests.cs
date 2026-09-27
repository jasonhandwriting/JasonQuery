using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.DdlPreview.ColumnDefinitions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace JasonQuery.Tests.Core.Database.DdlPreview.ColumnDefinitions
{
    [TestClass]
    public sealed class ColumnTypeCatalogTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DdlPreview")]
        [DataRow(DataSourceType.Oracle)]
        [DataRow(DataSourceType.PostgreSql)]
        [DataRow(DataSourceType.SqlServer)]
        [DataRow(DataSourceType.MySql)]
        public void GetDefinitions_ForSupportedDatabase_ReturnsDefinitions(DataSourceType dataSourceType)
        {
            var definitions = ColumnTypeCatalog.GetDefinitions(dataSourceType);

            Assert.IsNotNull(definitions);
            Assert.IsGreaterThanOrEqualTo(15, definitions.Count);
            Assert.IsTrue(definitions.All(item => item.DataSourceType == dataSourceType));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DdlPreview")]
        public void GetDefinitions_ForNone_ReturnsEmptyCollection()
        {
            Assert.IsEmpty(ColumnTypeCatalog.GetDefinitions(DataSourceType.None));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DdlPreview")]
        [DataRow(DataSourceType.Oracle, "VARCHAR2")]
        [DataRow(DataSourceType.PostgreSql, "VARCHAR")]
        [DataRow(DataSourceType.SqlServer, "NVARCHAR")]
        [DataRow(DataSourceType.MySql, "VARCHAR")]
        public void GetDefault_ForEachDatabase_ReturnsExpectedType(DataSourceType dataSourceType, string expectedKey)
        {
            var definition = ColumnTypeCatalog.GetDefault(dataSourceType);

            Assert.IsNotNull(definition);
            Assert.AreEqual(expectedKey, definition.Key);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DdlPreview")]
        [DataRow(DataSourceType.Oracle)]
        [DataRow(DataSourceType.PostgreSql)]
        [DataRow(DataSourceType.SqlServer)]
        [DataRow(DataSourceType.MySql)]
        public void GetDefinitions_KeysAreUniqueIgnoringCase(DataSourceType dataSourceType)
        {
            var definitions = ColumnTypeCatalog.GetDefinitions(dataSourceType);

            var duplicate = definitions.GroupBy
                                        (
                                            item => item.Key,
                                            StringComparer.OrdinalIgnoreCase
                                        )
                                       .FirstOrDefault
                                        (
                                            group => group.Count() > 1
                                        );

            Assert.IsNull(duplicate);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DdlPreview")]
        [DataRow(DataSourceType.Oracle, "VARCHAR2")]
        [DataRow(DataSourceType.Oracle, "NUMBER")]
        [DataRow(DataSourceType.Oracle, "DATE")]
        [DataRow(DataSourceType.Oracle, "TIMESTAMP")]
        [DataRow(DataSourceType.Oracle, "CLOB")]
        [DataRow(DataSourceType.Oracle, "BLOB")]
        [DataRow(DataSourceType.PostgreSql, "VARCHAR")]
        [DataRow(DataSourceType.PostgreSql, "TEXT")]
        [DataRow(DataSourceType.PostgreSql, "INTEGER")]
        [DataRow(DataSourceType.PostgreSql, "NUMERIC")]
        [DataRow(DataSourceType.PostgreSql, "BOOLEAN")]
        [DataRow(DataSourceType.PostgreSql, "TIMESTAMP")]
        [DataRow(DataSourceType.PostgreSql, "UUID")]
        [DataRow(DataSourceType.PostgreSql, "JSONB")]
        [DataRow(DataSourceType.SqlServer, "VARCHAR")]
        [DataRow(DataSourceType.SqlServer, "NVARCHAR")]
        [DataRow(DataSourceType.SqlServer, "INT")]
        [DataRow(DataSourceType.SqlServer, "DECIMAL")]
        [DataRow(DataSourceType.SqlServer, "BIT")]
        [DataRow(DataSourceType.SqlServer, "DATETIME2")]
        [DataRow(DataSourceType.SqlServer, "UNIQUEIDENTIFIER")]
        [DataRow(DataSourceType.MySql, "VARCHAR")]
        [DataRow(DataSourceType.MySql, "INT")]
        [DataRow(DataSourceType.MySql, "DECIMAL")]
        [DataRow(DataSourceType.MySql, "BIT")]
        [DataRow(DataSourceType.MySql, "DATETIME")]
        [DataRow(DataSourceType.MySql, "TEXT")]
        [DataRow(DataSourceType.MySql, "BLOB")]
        [DataRow(DataSourceType.MySql, "JSON")]
        public void Find_CommonType_ReturnsDefinition(DataSourceType dataSourceType, string key)
        {
            var definition = ColumnTypeCatalog.Find(dataSourceType, key);

            Assert.IsNotNull(definition);
            Assert.AreEqual(key, definition.Key);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DdlPreview")]
        public void Find_IsCaseInsensitive()
        {
            var definition = ColumnTypeCatalog.Find(DataSourceType.Oracle, "varchar2");

            Assert.IsNotNull(definition);
            Assert.AreEqual("VARCHAR2", definition.Key);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DdlPreview")]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("missing")]
        public void Find_WithMissingKey_ReturnsNull(string key)
        {
            Assert.IsNull(ColumnTypeCatalog.Find(DataSourceType.Oracle, key));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DdlPreview")]
        public void OracleVarchar2_SupportsLengthSemantics()
        {
            var definition = ColumnTypeCatalog.Find(DataSourceType.Oracle, "VARCHAR2");

            Assert.IsTrue(definition.SupportsOracleLengthSemantics);
            Assert.AreEqual("50", definition.DefaultParameter1);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DdlPreview")]
        public void OracleNvarchar2_DoesNotExposeLengthSemantics()
        {
            var definition = ColumnTypeCatalog.Find(DataSourceType.Oracle, "NVARCHAR2");

            Assert.IsFalse(definition.SupportsOracleLengthSemantics);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DdlPreview")]
        [DataRow("VARCHAR")]
        [DataRow("NVARCHAR")]
        [DataRow("VARBINARY")]
        public void SqlServerVariableLengthType_AllowsMax(string key)
        {
            Assert.IsTrue(ColumnTypeCatalog.Find(DataSourceType.SqlServer, key).AllowsMax);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DdlPreview")]
        [DataRow(DataSourceType.Oracle, "BLOB")]
        [DataRow(DataSourceType.Oracle, "CLOB")]
        [DataRow(DataSourceType.Oracle, "RAW")]
        [DataRow(DataSourceType.SqlServer, "XML")]
        [DataRow(DataSourceType.SqlServer, "VARBINARY")]
        [DataRow(DataSourceType.MySql, "TEXT")]
        [DataRow(DataSourceType.MySql, "BLOB")]
        [DataRow(DataSourceType.MySql, "VARBINARY")]
        public void TypeWithVersionDependentOrUnsupportedDefault_DisablesDefault(DataSourceType dataSourceType, string key)
        {
            Assert.IsFalse(ColumnTypeCatalog.Find(dataSourceType, key).SupportsDefault);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DdlPreview")]
        public void CreateCustom_ReturnsCustomDefinition()
        {
            var definition = ColumnTypeCatalog.CreateCustom(DataSourceType.PostgreSql, "public.my_domain");

            Assert.IsTrue(definition.IsCustom);
            Assert.AreEqual("__CUSTOM__", definition.Key);
            Assert.AreEqual("public.my_domain", definition.SqlTypeName);
            Assert.IsTrue(definition.SupportsDefault);
        }
    }
}
