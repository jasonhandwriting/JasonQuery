using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.DdlPreview.ColumnDefinitions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonQuery.Tests.Core.Database.DdlPreview.ColumnDefinitions
{
    [TestClass]
    public sealed class ColumnAddSqlBuilderTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DdlPreview")]
        public void Build_WithNullRequest_ReturnsFailure()
        {
            var result = ColumnAddSqlBuilder.Build(null);

            Assert.IsFalse(result.Succeeded);
            Assert.AreEqual(ColumnAddBuildFailureKind.MissingRequest, result.FailureKind);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DdlPreview")]
        public void Build_WithNoneDataSource_ReturnsFailure()
        {
            var request = CreateRequest(DataSourceType.None, "C1", "VARCHAR", "10", string.Empty);
            var result = ColumnAddSqlBuilder.Build(request);

            Assert.IsFalse(result.Succeeded);
            Assert.AreEqual(ColumnAddBuildFailureKind.UnsupportedDataSource, result.FailureKind);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DdlPreview")]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        public void Build_WithMissingColumnName_ReturnsFailure(string columnName)
        {
            var result = ColumnAddSqlBuilder.Build(CreateRequest(DataSourceType.PostgreSql, columnName, "VARCHAR", "10", string.Empty));

            Assert.IsFalse(result.Succeeded);
            Assert.AreEqual(ColumnAddBuildFailureKind.MissingColumnName, result.FailureKind);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DdlPreview")]
        [DataRow("schema.column")]
        [DataRow("C1; DROP TABLE A")]
        [DataRow("C1 -- comment")]
        public void Build_WithUnsafeColumnName_ReturnsFailure(string columnName)
        {
            var result = ColumnAddSqlBuilder.Build(CreateRequest(DataSourceType.PostgreSql, columnName, "VARCHAR", "10", string.Empty));

            Assert.IsFalse(result.Succeeded);
            Assert.AreEqual(ColumnAddBuildFailureKind.UnsafeColumnName, result.FailureKind);
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("DdlPreview")]
        [DataRow(DataSourceType.Oracle, "VARCHAR2", "15", "", 0, true, 0, "",
                 "ALTER TABLE \"SYS\".\"AABBCC\" ADD \"AA\" VARCHAR2(15) NULL;")]
        [DataRow(DataSourceType.Oracle, "VARCHAR2", "15", "", 2, false, 0, "",
                 "ALTER TABLE \"SYS\".\"AABBCC\" ADD \"AA\" VARCHAR2(15 CHAR) NOT NULL;")]
        [DataRow(DataSourceType.Oracle, "NUMBER", "25", "5", 0, true, 0, "",
                 "ALTER TABLE \"SYS\".\"AABBCC\" ADD \"AA\" NUMBER(25,5) NULL;")]
        [DataRow(DataSourceType.Oracle, "TIMESTAMP", "6", "", 0, true, 0, "",
                 "ALTER TABLE \"SYS\".\"AABBCC\" ADD \"AA\" TIMESTAMP(6) NULL;")]
        [DataRow(DataSourceType.PostgreSql, "VARCHAR", "50", "", 0, true, 0, "",
                 "ALTER TABLE \"public\".\"aabbcc\" ADD COLUMN \"aa\" varchar(50) NULL;")]
        [DataRow(DataSourceType.PostgreSql, "NUMERIC", "18", "2", 0, false, 0, "",
                 "ALTER TABLE \"public\".\"aabbcc\" ADD COLUMN \"aa\" numeric(18,2) NOT NULL;")]
        [DataRow(DataSourceType.PostgreSql, "BOOLEAN", "", "", 0, true, 0, "",
                 "ALTER TABLE \"public\".\"aabbcc\" ADD COLUMN \"aa\" boolean NULL;")]
        [DataRow(DataSourceType.SqlServer, "NVARCHAR", "50", "", 0, true, 0, "",
                 "ALTER TABLE [MyDB].[dbo].[AABBCC] ADD [AA] nvarchar(50) NULL;")]
        [DataRow(DataSourceType.SqlServer, "DECIMAL", "18", "2", 0, false, 0, "",
                 "ALTER TABLE [MyDB].[dbo].[AABBCC] ADD [AA] decimal(18,2) NOT NULL;")]
        [DataRow(DataSourceType.SqlServer, "VARCHAR", "MAX", "", 0, true, 0, "",
                 "ALTER TABLE [MyDB].[dbo].[AABBCC] ADD [AA] varchar(MAX) NULL;")]
        [DataRow(DataSourceType.MySql, "VARCHAR", "100", "", 0, true, 0, "",
                 "ALTER TABLE `MyDB`.`AABBCC` ADD COLUMN `AA` varchar(100) NULL;")]
        [DataRow(DataSourceType.MySql, "DECIMAL", "20", "6", 0, false, 0, "",
                 "ALTER TABLE `MyDB`.`AABBCC` ADD COLUMN `AA` decimal(20,6) NOT NULL;")]
        [DataRow(DataSourceType.MySql, "DATETIME", "6", "", 0, true, 0, "",
                 "ALTER TABLE `MyDB`.`AABBCC` ADD COLUMN `AA` datetime(6) NULL;")]
        public void Build_BasicColumn_ReturnsExpectedSql(DataSourceType dataSourceType, string typeKey, string parameter1, string parameter2, int semanticsValue,
                                                         bool nullAllowed, int defaultKindValue, string defaultValue, string expected)
        {
            var request = CreateRequest(dataSourceType, "AA", typeKey, parameter1, parameter2);

            request.Column.OracleLengthSemantics = (OracleLengthSemantics)semanticsValue;
            request.Column.NullAllowed = nullAllowed;
            request.Column.DefaultValueKind = (ColumnDefaultValueKind)defaultKindValue;
            request.Column.DefaultValue = defaultValue;

            var result = ColumnAddSqlBuilder.Build(request);

            Assert.IsTrue(result.Succeeded, result.ErrorMessage);
            Assert.AreEqual(expected, result.Sql);
            Assert.AreNotEqual(string.Empty, result.ResolvedDataType);
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("DdlPreview")]
        [DataRow(DataSourceType.Oracle, "VARCHAR2", "Jason's data", 1,
                 "ALTER TABLE \"SYS\".\"AABBCC\" ADD \"AA\" VARCHAR2(50) DEFAULT 'Jason''s data' NULL;")]
        [DataRow(DataSourceType.PostgreSql, "INTEGER", "123", 1,
                 "ALTER TABLE \"public\".\"aabbcc\" ADD COLUMN \"aa\" integer DEFAULT 123 NULL;")]
        [DataRow(DataSourceType.PostgreSql, "TIMESTAMP", "CURRENT_TIMESTAMP", 2,
                 "ALTER TABLE \"public\".\"aabbcc\" ADD COLUMN \"aa\" timestamp(6) DEFAULT CURRENT_TIMESTAMP NULL;")]
        [DataRow(DataSourceType.SqlServer, "NVARCHAR", "台北'101", 1,
                 "ALTER TABLE [MyDB].[dbo].[AABBCC] ADD [AA] nvarchar(50) NULL DEFAULT N'台北''101';")]
        [DataRow(DataSourceType.SqlServer, "UNIQUEIDENTIFIER", "NEWID()", 2,
                 "ALTER TABLE [MyDB].[dbo].[AABBCC] ADD [AA] uniqueidentifier NULL DEFAULT NEWID();")]
        [DataRow(DataSourceType.MySql, "DATETIME", "CURRENT_TIMESTAMP", 2,
                 "ALTER TABLE `MyDB`.`AABBCC` ADD COLUMN `AA` datetime(0) NULL DEFAULT CURRENT_TIMESTAMP;")]
        public void Build_WithDefault_ReturnsExpectedSql(DataSourceType dataSourceType, string typeKey, string defaultValue, int defaultKindValue, string expected)
        {
            var definition = ColumnTypeCatalog.Find(dataSourceType, typeKey);
            var request = CreateRequest(dataSourceType, "AA", typeKey, definition?.DefaultParameter1 ?? string.Empty, definition?.DefaultParameter2 ?? string.Empty);

            request.Column.DefaultValueKind = (ColumnDefaultValueKind)defaultKindValue;
            request.Column.DefaultValue = defaultValue;

            var result = ColumnAddSqlBuilder.Build(request);

            Assert.IsTrue(result.Succeeded, result.ErrorMessage);
            Assert.AreEqual(expected, result.Sql);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DdlPreview")]
        [DataRow(DataSourceType.Oracle, "\"Mixed Column\"")]
        [DataRow(DataSourceType.PostgreSql, "\"Mixed Column\"")]
        [DataRow(DataSourceType.SqlServer, "[Mixed Column]")]
        [DataRow(DataSourceType.MySql, "`Mixed Column`")]
        public void Build_WithDelimitedColumnName_PreservesIdentifier(DataSourceType dataSourceType, string columnName)
        {
            var result = ColumnAddSqlBuilder.Build
            (
                CreateRequest
                (
                    dataSourceType,
                    columnName,
                    GetDefaultTypeKey(dataSourceType),
                    GetDefaultLength(dataSourceType),
                    string.Empty
                )
            );

            Assert.IsTrue(result.Succeeded, result.ErrorMessage);
            StringAssert.Contains(result.Sql, columnName);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DdlPreview")]
        [DataRow(DataSourceType.Oracle, "MY_TYPE")]
        [DataRow(DataSourceType.PostgreSql, "public.my_domain")]
        [DataRow(DataSourceType.SqlServer, "[dbo].[MyType]")]
        [DataRow(DataSourceType.MySql, "VECTOR(3)")]
        public void Build_WithSafeCustomType_ReturnsSql(DataSourceType dataSourceType, string customType)
        {
            var request = CreateRequest(dataSourceType, "AA", string.Empty, string.Empty, string.Empty);

            request.Column.CustomTypeText = customType;

            var result = ColumnAddSqlBuilder.Build(request);

            Assert.IsTrue(result.Succeeded, result.ErrorMessage);
            StringAssert.Contains(result.Sql, customType);
            Assert.AreEqual(customType, result.ResolvedDataType);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DdlPreview")]
        [DataRow(DataSourceType.Oracle)]
        [DataRow(DataSourceType.PostgreSql)]
        [DataRow(DataSourceType.SqlServer)]
        [DataRow(DataSourceType.MySql)]
        public void Build_DoesNotAppendCommitOrRollback(DataSourceType dataSourceType)
        {
            var result = ColumnAddSqlBuilder.Build
            (
                CreateRequest
                (
                    dataSourceType,
                    "AA",
                    GetDefaultTypeKey(dataSourceType),
                    GetDefaultLength(dataSourceType),
                    string.Empty
                )
            );

            Assert.IsTrue(result.Succeeded);
            Assert.IsFalse(result.Sql.Contains("COMMIT"));
            Assert.IsFalse(result.Sql.Contains("ROLLBACK"));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DdlPreview")]
        public void Build_MissingSchemaForPostgreSql_ReturnsFailure()
        {
            var request = CreateRequest(DataSourceType.PostgreSql, "AA", "VARCHAR", "10", string.Empty);

            request.SchemaName = string.Empty;

            var result = ColumnAddSqlBuilder.Build(request);

            Assert.IsFalse(result.Succeeded);
            Assert.AreEqual(ColumnAddBuildFailureKind.MissingTableName, result.FailureKind);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DdlPreview")]
        public void Build_MissingDatabaseForSqlServer_ReturnsFailure()
        {
            var request = CreateRequest(DataSourceType.SqlServer, "AA", "NVARCHAR", "10", string.Empty);

            request.SchemaDatabase = string.Empty;

            Assert.IsFalse(ColumnAddSqlBuilder.Build(request).Succeeded);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DdlPreview")]
        public void Build_InvalidDefault_ReturnsFailureWithResolvedType()
        {
            var request = CreateRequest(DataSourceType.PostgreSql, "AA", "INTEGER", string.Empty, string.Empty);

            request.Column.DefaultValueKind = ColumnDefaultValueKind.Literal;
            request.Column.DefaultValue = "abc";

            var result = ColumnAddSqlBuilder.Build(request);

            Assert.IsFalse(result.Succeeded);
            Assert.AreEqual(ColumnAddBuildFailureKind.InvalidDefaultValue, result.FailureKind);
            Assert.AreEqual("integer", result.ResolvedDataType);
        }

        private static ColumnAddRequest CreateRequest(DataSourceType dataSourceType, string columnName, string typeKey, string parameter1, string parameter2)
        {
            return new ColumnAddRequest
            {
                DataSourceType = dataSourceType,
                SchemaDatabase = "MyDB",
                SchemaName = dataSourceType == DataSourceType.Oracle ? "SYS" : dataSourceType == DataSourceType.SqlServer ? "dbo" : "public",
                TableName = dataSourceType == DataSourceType.PostgreSql ? "aabbcc" : "AABBCC",
                Column = new ColumnDefinition
                {
                    ColumnName = columnName,
                    TypeKey = typeKey,
                    Parameter1 = parameter1,
                    Parameter2 = parameter2,
                    NullAllowed = true,
                    DefaultValueKind = ColumnDefaultValueKind.None
                }
            };
        }

        private static string GetDefaultTypeKey(DataSourceType dataSourceType)
        {
            return ColumnTypeCatalog.GetDefault(dataSourceType).Key;
        }

        private static string GetDefaultLength(DataSourceType dataSourceType)
        {
            return ColumnTypeCatalog.GetDefault(dataSourceType).DefaultParameter1;
        }
    }
}