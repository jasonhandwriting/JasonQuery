using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.DdlPreview.ColumnDefinitions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonQuery.Tests.Core.Database.DdlPreview.ColumnDefinitions
{
    [TestClass]
    public sealed class ColumnTypeSqlBuilderTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DdlPreview")]
        public void Build_WithNullDefinition_ReturnsFailure()
        {
            var result = ColumnTypeSqlBuilder.Build(null, new ColumnDefinition());

            Assert.IsFalse(result.Succeeded);
            Assert.AreEqual(string.Empty, result.ResolvedDataType);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DdlPreview")]
        public void Build_WithNullColumn_ReturnsFailure()
        {
            var result = ColumnTypeSqlBuilder.Build(ColumnTypeCatalog.GetDefault(DataSourceType.Oracle), null);

            Assert.IsFalse(result.Succeeded);
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("DdlPreview")]
        [DataRow(DataSourceType.Oracle, "VARCHAR2", "15", "", 0, "VARCHAR2(15)")]
        [DataRow(DataSourceType.Oracle, "VARCHAR2", "15", "", 1, "VARCHAR2(15 BYTE)")]
        [DataRow(DataSourceType.Oracle, "VARCHAR2", "15", "", 2, "VARCHAR2(15 CHAR)")]
        [DataRow(DataSourceType.Oracle, "NVARCHAR2", "30", "", 2, "NVARCHAR2(30)")]
        [DataRow(DataSourceType.Oracle, "CHAR", "5", "", 2, "CHAR(5 CHAR)")]
        [DataRow(DataSourceType.Oracle, "NUMBER", "25", "5", 0, "NUMBER(25,5)")]
        [DataRow(DataSourceType.Oracle, "NUMBER", "", "", 0, "NUMBER")]
        [DataRow(DataSourceType.Oracle, "FLOAT", "53", "", 0, "FLOAT(53)")]
        [DataRow(DataSourceType.Oracle, "DATE", "", "", 0, "DATE")]
        [DataRow(DataSourceType.Oracle, "TIMESTAMP", "6", "", 0, "TIMESTAMP(6)")]
        [DataRow(DataSourceType.Oracle, "TIMESTAMP_WITH_TIME_ZONE", "3", "", 0, "TIMESTAMP(3) WITH TIME ZONE")]
        [DataRow(DataSourceType.PostgreSql, "VARCHAR", "100", "", 0, "varchar(100)")]
        [DataRow(DataSourceType.PostgreSql, "TEXT", "", "", 0, "text")]
        [DataRow(DataSourceType.PostgreSql, "INTEGER", "", "", 0, "integer")]
        [DataRow(DataSourceType.PostgreSql, "NUMERIC", "18", "4", 0, "numeric(18,4)")]
        [DataRow(DataSourceType.PostgreSql, "NUMERIC", "", "", 0, "numeric")]
        [DataRow(DataSourceType.PostgreSql, "TIME", "", "", 0, "time")]
        [DataRow(DataSourceType.PostgreSql, "TIMESTAMP", "3", "", 0, "timestamp(3)")]
        [DataRow(DataSourceType.PostgreSql, "TIMESTAMP_WITH_TIME_ZONE", "6", "", 0, "timestamp(6) with time zone")]
        [DataRow(DataSourceType.SqlServer, "VARCHAR", "100", "", 0, "varchar(100)")]
        [DataRow(DataSourceType.SqlServer, "VARCHAR", "MAX", "", 0, "varchar(MAX)")]
        [DataRow(DataSourceType.SqlServer, "NVARCHAR", "max", "", 0, "nvarchar(MAX)")]
        [DataRow(DataSourceType.SqlServer, "INT", "", "", 0, "int")]
        [DataRow(DataSourceType.SqlServer, "DECIMAL", "18", "2", 0, "decimal(18,2)")]
        [DataRow(DataSourceType.SqlServer, "FLOAT", "53", "", 0, "float(53)")]
        [DataRow(DataSourceType.SqlServer, "DATETIME2", "7", "", 0, "datetime2(7)")]
        [DataRow(DataSourceType.MySql, "VARCHAR", "255", "", 0, "varchar(255)")]
        [DataRow(DataSourceType.MySql, "INT", "", "", 0, "int")]
        [DataRow(DataSourceType.MySql, "DECIMAL", "20", "6", 0, "decimal(20,6)")]
        [DataRow(DataSourceType.MySql, "BIT", "8", "", 0, "bit(8)")]
        [DataRow(DataSourceType.MySql, "DATETIME", "6", "", 0, "datetime(6)")]
        [DataRow(DataSourceType.MySql, "TEXT", "", "", 0, "text")]
        public void Build_CommonType_ReturnsExpectedSqlType(DataSourceType dataSourceType, string key, string parameter1, string parameter2, int semanticsValue, string expected)
        {
            var definition = ColumnTypeCatalog.Find(dataSourceType, key);

            var result = ColumnTypeSqlBuilder.Build
                         (
                             definition,
                             new ColumnDefinition
                             {
                                 Parameter1 = parameter1,
                                 Parameter2 = parameter2,
                                 OracleLengthSemantics = (OracleLengthSemantics)semanticsValue
                             }
                         );

            Assert.IsTrue(result.Succeeded, result.ErrorMessage);
            Assert.AreEqual(expected, result.ResolvedDataType);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DdlPreview")]
        [DataRow(DataSourceType.Oracle, "VARCHAR2", "", "")]
        [DataRow(DataSourceType.SqlServer, "NVARCHAR", "", "")]
        [DataRow(DataSourceType.MySql, "VARCHAR", "", "")]
        [DataRow(DataSourceType.Oracle, "VARCHAR2", "0", "")]
        [DataRow(DataSourceType.SqlServer, "NVARCHAR", "4001", "")]
        [DataRow(DataSourceType.MySql, "BIT", "65", "")]
        [DataRow(DataSourceType.SqlServer, "DECIMAL", "39", "2")]
        [DataRow(DataSourceType.SqlServer, "DECIMAL", "10", "11")]
        [DataRow(DataSourceType.PostgreSql, "NUMERIC", "", "2")]
        [DataRow(DataSourceType.MySql, "DATETIME", "7", "")]
        public void Build_InvalidParameters_ReturnsFailure(DataSourceType dataSourceType, string key, string parameter1, string parameter2)
        {
            var result = ColumnTypeSqlBuilder.Build
            (
                ColumnTypeCatalog.Find
                (
                    dataSourceType,
                    key
                ),
                new ColumnDefinition
                {
                    Parameter1 = parameter1,
                    Parameter2 = parameter2
                }
            );

            Assert.IsFalse(result.Succeeded);
            Assert.AreEqual(string.Empty, result.ResolvedDataType);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DdlPreview")]
        [DataRow("public.my_domain")]
        [DataRow("VECTOR(3)")]
        [DataRow("\"MyType\"")]
        [DataRow("timestamp with time zone")]
        public void Build_SafeCustomType_ReturnsCustomText(string customType)
        {
            var definition = ColumnTypeCatalog.CreateCustom(DataSourceType.PostgreSql, customType);

            var result = ColumnTypeSqlBuilder.Build
            (
                definition,
                new ColumnDefinition
                {
                    CustomTypeText = customType
                }
            );

            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(customType, result.ResolvedDataType);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DdlPreview")]
        [DataRow("varchar(10); drop table a")]
        [DataRow("varchar(10) -- comment")]
        [DataRow("varchar(10) NOT NULL")]
        [DataRow("varchar(10) DEFAULT 'x'")]
        [DataRow("vector(3")]
        public void Build_UnsafeCustomType_ReturnsFailure(string customType)
        {
            var result = ColumnTypeSqlBuilder.Build
            (
                ColumnTypeCatalog.CreateCustom
                (
                    DataSourceType.PostgreSql,
                    customType
                ),
                new ColumnDefinition
                {
                    CustomTypeText = customType
                }
            );

            Assert.IsFalse(result.Succeeded);
        }
    }
}
