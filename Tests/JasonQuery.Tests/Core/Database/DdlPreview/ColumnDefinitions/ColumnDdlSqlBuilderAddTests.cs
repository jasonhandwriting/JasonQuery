using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.DdlPreview;
using JasonQuery.Core.Database.DdlPreview.ColumnDefinitions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonQuery.Tests.Core.Database.DdlPreview.ColumnDefinitions
{
    [TestClass]
    public sealed class ColumnDdlSqlBuilderAddTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DdlPreview")]
        [DataRow(DataSourceType.Oracle, "VARCHAR2",
                 "ALTER TABLE \"SYS\".\"AABBCC\" ADD \"AA\" VARCHAR2(50) NULL;")]
        [DataRow(DataSourceType.PostgreSql, "VARCHAR",
                 "ALTER TABLE \"public\".\"AABBCC\" ADD COLUMN \"aa\" varchar(50) NULL;")]
        [DataRow(DataSourceType.SqlServer, "NVARCHAR",
                 "ALTER TABLE [MyDB].[dbo].[AABBCC] ADD [AA] nvarchar(50) NULL;")]
        [DataRow(DataSourceType.MySql, "VARCHAR",
                 "ALTER TABLE `MyDB`.`AABBCC` ADD COLUMN `AA` varchar(50) NULL;")]
        public void Build_WithAddOperation_DelegatesToColumnAddBuilder(DataSourceType dataSourceType, string typeKey, string expected)
        {
            var request = new ColumnDdlRequest
            {
                DataSourceType = dataSourceType,
                Operation = ColumnDdlOperation.Add,
                SchemaDatabase = "MyDB",
                SchemaName = dataSourceType == DataSourceType.Oracle ? "SYS" : dataSourceType == DataSourceType.SqlServer ? "dbo" : "public",
                TableName = "AABBCC",
                ColumnDefinition = new ColumnDefinition
                {
                    ColumnName = "AA",
                    TypeKey = typeKey,
                    Parameter1 = "50",
                    NullAllowed = true
                }
            };

            Assert.AreEqual(expected, ColumnDdlSqlBuilder.Build(request));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DdlPreview")]
        public void Build_AddWithoutColumnDefinition_ReturnsEmptyString()
        {
            Assert.AreEqual
            (
                string.Empty,
                ColumnDdlSqlBuilder.Build
                (
                    new ColumnDdlRequest
                    {
                        DataSourceType = DataSourceType.Oracle,
                        Operation = ColumnDdlOperation.Add,
                        SchemaName = "SYS",
                        TableName = "AABBCC"
                    }
                )
            );
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DdlPreview")]
        public void Build_AddWithInvalidColumn_ReturnsEmptyString()
        {
            Assert.AreEqual
            (
                string.Empty,
                ColumnDdlSqlBuilder.Build
                (
                    new ColumnDdlRequest
                    {
                        DataSourceType = DataSourceType.Oracle,
                        Operation = ColumnDdlOperation.Add,
                        SchemaName = "SYS",
                        TableName = "AABBCC",
                        ColumnDefinition = new ColumnDefinition
                        {
                            ColumnName = "AA; DROP TABLE A",
                            TypeKey = "VARCHAR2",
                            Parameter1 = "50",
                            NullAllowed = true
                        }
                    }
                )
            );
        }
    }
}
