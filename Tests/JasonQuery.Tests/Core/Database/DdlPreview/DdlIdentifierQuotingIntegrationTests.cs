using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.DdlPreview;
using JasonQuery.Core.Database.DdlPreview.ColumnDefinitions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonQuery.Tests.Core.Database.DdlPreview
{
    [TestClass]
    public sealed class DdlIdentifierQuotingIntegrationTests
    {
        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("DdlPreview")]
        [DataRow(DataSourceType.Oracle, (int)TableDdlOperation.Comment,
                 "COMMENT ON TABLE \"HR\".\"A_TEST\" IS 'comment';")]
        [DataRow(DataSourceType.Oracle, (int)TableDdlOperation.Drop,
                 "DROP TABLE \"HR\".\"A_TEST\";")]
        [DataRow(DataSourceType.Oracle, (int)TableDdlOperation.Rename,
                 "ALTER TABLE \"HR\".\"A_TEST\" RENAME TO \"A_TEST_NEW\";")]
        [DataRow(DataSourceType.Oracle, (int)TableDdlOperation.Truncate,
                 "TRUNCATE TABLE \"HR\".\"A_TEST\";")]
        [DataRow(DataSourceType.PostgreSql, (int)TableDdlOperation.Comment,
                 "COMMENT ON TABLE \"public\".\"A_TEST\" IS 'comment';")]
        [DataRow(DataSourceType.PostgreSql, (int)TableDdlOperation.Drop,
                 "DROP TABLE \"public\".\"A_TEST\";")]
        [DataRow(DataSourceType.PostgreSql, (int)TableDdlOperation.Rename,
                 "ALTER TABLE \"public\".\"A_TEST\" RENAME TO \"A_TEST_NEW\";")]
        [DataRow(DataSourceType.PostgreSql, (int)TableDdlOperation.Truncate,
                 "TRUNCATE TABLE \"public\".\"A_TEST\";")]
        [DataRow(DataSourceType.SqlServer, (int)TableDdlOperation.Comment,
                 "EXEC [MyDB].[sys].[sp_addextendedproperty] @name=N'MS_Description', @value=N'comment', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'A_TEST';")]
        [DataRow(DataSourceType.SqlServer, (int)TableDdlOperation.Drop,
                 "DROP TABLE [MyDB].[dbo].[A_TEST];")]
        [DataRow(DataSourceType.SqlServer, (int)TableDdlOperation.Rename,
                 "EXEC [MyDB].[sys].[sp_rename] @objname=N'[dbo].[A_TEST]', @newname=N'A_TEST_NEW';")]
        [DataRow(DataSourceType.SqlServer, (int)TableDdlOperation.Truncate,
                 "TRUNCATE TABLE [MyDB].[dbo].[A_TEST];")]
        [DataRow(DataSourceType.MySql, (int)TableDdlOperation.Comment,
                 "ALTER TABLE `MyDB`.`A_TEST` COMMENT = 'comment';")]
        [DataRow(DataSourceType.MySql, (int)TableDdlOperation.Drop,
                 "DROP TABLE `MyDB`.`A_TEST`;")]
        [DataRow(DataSourceType.MySql, (int)TableDdlOperation.Rename,
                 "RENAME TABLE `MyDB`.`A_TEST` TO `MyDB`.`A_TEST_NEW`;")]
        [DataRow(DataSourceType.MySql, (int)TableDdlOperation.Truncate,
                 "TRUNCATE TABLE `MyDB`.`A_TEST`;")]
        public void TableBuilder_AllOperations_ApplyIdentifierPolicy(DataSourceType dataSourceType, int operationValue, string expected)
        {
            var request = CreateTableRequest(dataSourceType, (TableDdlOperation)operationValue);

            Assert.AreEqual(expected, TableDdlSqlBuilder.Build(request));
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("DdlPreview")]
        [DataRow(DataSourceType.Oracle, (int)ColumnDdlOperation.Comment,
                 "COMMENT ON COLUMN \"HR\".\"A_TEST\".\"C1\" IS 'comment';")]
        [DataRow(DataSourceType.Oracle, (int)ColumnDdlOperation.Drop,
                 "ALTER TABLE \"HR\".\"A_TEST\" DROP COLUMN \"C1\";")]
        [DataRow(DataSourceType.Oracle, (int)ColumnDdlOperation.Rename,
                 "ALTER TABLE \"HR\".\"A_TEST\" RENAME COLUMN \"C1\" TO \"C1_NEW\";")]
        [DataRow(DataSourceType.PostgreSql, (int)ColumnDdlOperation.Comment,
                 "COMMENT ON COLUMN \"public\".\"A_TEST\".\"C1\" IS 'comment';")]
        [DataRow(DataSourceType.PostgreSql, (int)ColumnDdlOperation.Drop,
                 "ALTER TABLE \"public\".\"A_TEST\" DROP COLUMN \"C1\";")]
        [DataRow(DataSourceType.PostgreSql, (int)ColumnDdlOperation.Rename,
                 "ALTER TABLE \"public\".\"A_TEST\" RENAME COLUMN \"C1\" TO \"c1_new\";")]
        [DataRow(DataSourceType.SqlServer, (int)ColumnDdlOperation.Comment,
                 "EXEC [MyDB].[sys].[sp_addextendedproperty] @name=N'MS_Description', @value=N'comment', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'A_TEST', @level2type=N'COLUMN', @level2name=N'C1';")]
        [DataRow(DataSourceType.SqlServer, (int)ColumnDdlOperation.Drop,
                 "ALTER TABLE [MyDB].[dbo].[A_TEST] DROP COLUMN [C1];")]
        [DataRow(DataSourceType.SqlServer, (int)ColumnDdlOperation.Rename,
                 "EXEC [MyDB].[sys].[sp_rename] @objname=N'[dbo].[A_TEST].[C1]', @newname=N'C1_NEW', @objtype=N'COLUMN';")]
        [DataRow(DataSourceType.MySql, (int)ColumnDdlOperation.Comment,
                 "ALTER TABLE `MyDB`.`A_TEST` CHANGE COLUMN `C1` `C1` varchar(20) COMMENT 'comment';")]
        [DataRow(DataSourceType.MySql, (int)ColumnDdlOperation.Drop,
                 "ALTER TABLE `MyDB`.`A_TEST` DROP COLUMN `C1`;")]
        [DataRow(DataSourceType.MySql, (int)ColumnDdlOperation.Rename,
                 "ALTER TABLE `MyDB`.`A_TEST` CHANGE COLUMN `C1` `C1_NEW` varchar(20);")]
        public void ColumnBuilder_ExistingOperations_ApplyIdentifierPolicy(DataSourceType dataSourceType, int operationValue, string expected)
        {
            var request = CreateColumnRequest(dataSourceType, (ColumnDdlOperation)operationValue);

            Assert.AreEqual(expected, ColumnDdlSqlBuilder.Build(request));
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("DdlPreview")]
        [DataRow(DataSourceType.Oracle,
                 "ALTER TABLE \"SYS\".\"AABBCC\" ADD \"NEW_COLUMN\" VARCHAR2(50) NULL;")]
        [DataRow(DataSourceType.PostgreSql,
                 "ALTER TABLE \"public\".\"AABBCC\" ADD COLUMN \"new_column\" varchar(50) NULL;")]
        [DataRow(DataSourceType.SqlServer,
                 "ALTER TABLE [MyDB].[dbo].[AABBCC] ADD [New_Column] nvarchar(50) NULL;")]
        [DataRow(DataSourceType.MySql,
                 "ALTER TABLE `MyDB`.`AABBCC` ADD COLUMN `New_Column` varchar(50) NULL;")]
        public void AddColumnBuilder_AppliesIdentifierPolicy(DataSourceType dataSourceType, string expected)
        {
            var request = CreateAddRequest(dataSourceType);
            var result = ColumnAddSqlBuilder.Build(request);

            Assert.IsTrue(result.Succeeded, result.ErrorMessage);
            Assert.AreEqual(expected, result.Sql);
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("DdlPreview")]
        [DataRow(DataSourceType.Oracle, "\"Mixed Schema\"", "\"Mixed Table\"", "\"Mixed Column\"",
                 "COMMENT ON COLUMN \"Mixed Schema\".\"Mixed Table\".\"Mixed Column\" IS 'comment';")]
        [DataRow(DataSourceType.PostgreSql, "\"Mixed Schema\"", "\"Mixed Table\"", "\"Mixed Column\"",
                 "COMMENT ON COLUMN \"Mixed Schema\".\"Mixed Table\".\"Mixed Column\" IS 'comment';")]
        [DataRow(DataSourceType.SqlServer, "[Mixed Schema]", "[Mixed Table]", "[Mixed Column]",
                 "EXEC [MyDB].[sys].[sp_addextendedproperty] @name=N'MS_Description', @value=N'comment', @level0type=N'SCHEMA', @level0name=N'Mixed Schema', @level1type=N'TABLE', @level1name=N'Mixed Table', @level2type=N'COLUMN', @level2name=N'Mixed Column';")]
        [DataRow(DataSourceType.MySql, "`Mixed Schema`", "`Mixed Table`", "`Mixed Column`",
                 "ALTER TABLE `Mixed Schema`.`Mixed Table` CHANGE COLUMN `Mixed Column` `Mixed Column` varchar(20) COMMENT 'comment';")]
        public void ColumnComment_WithExplicitDelimitedNames_PreservesCaseAndSpaces(DataSourceType dataSourceType, string schemaName, string tableName, string columnName, string expected)
        {
            var request = CreateColumnRequest(dataSourceType, ColumnDdlOperation.Comment);

            request.SchemaName = schemaName;
            request.SchemaDatabase = dataSourceType == DataSourceType.MySql ? schemaName : "MyDB";
            request.TableName = tableName;
            request.ColumnName = columnName;

            Assert.AreEqual(expected, ColumnDdlSqlBuilder.Build(request));
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("DdlPreview")]
        [DataRow(DataSourceType.Oracle, "MixedSchema", "MixedTable",
                 "DROP TABLE \"MixedSchema\".\"MixedTable\";")]
        [DataRow(DataSourceType.PostgreSql, "MixedSchema", "MixedTable",
                 "DROP TABLE \"MixedSchema\".\"MixedTable\";")]
        [DataRow(DataSourceType.SqlServer, "MixedSchema", "MixedTable",
                 "DROP TABLE [MyDB].[MixedSchema].[MixedTable];")]
        [DataRow(DataSourceType.MySql, "MixedDatabase", "MixedTable",
                 "DROP TABLE `MixedDatabase`.`MixedTable`;")]
        public void TableBuilder_WithRawMixedCaseMetadata_PreservesExactCase(DataSourceType dataSourceType, string schemaName, string tableName, string expected)
        {
            var request = CreateTableRequest(dataSourceType, TableDdlOperation.Drop);

            if (dataSourceType == DataSourceType.MySql)
            {
                request.SchemaDatabase = schemaName;
            }
            else
            {
                request.SchemaName = schemaName;
            }

            request.TableName = tableName;

            Assert.AreEqual(expected, TableDdlSqlBuilder.Build(request));
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("DdlPreview")]
        [DataRow(DataSourceType.Oracle, "MixedColumn",
                 "ALTER TABLE \"HR\".\"A_TEST\" DROP COLUMN \"MixedColumn\";")]
        [DataRow(DataSourceType.PostgreSql, "MixedColumn",
                 "ALTER TABLE \"public\".\"A_TEST\" DROP COLUMN \"MixedColumn\";")]
        [DataRow(DataSourceType.SqlServer, "MixedColumn",
                 "ALTER TABLE [MyDB].[dbo].[A_TEST] DROP COLUMN [MixedColumn];")]
        [DataRow(DataSourceType.MySql, "MixedColumn",
                 "ALTER TABLE `MyDB`.`A_TEST` DROP COLUMN `MixedColumn`;")]
        public void ColumnBuilder_WithRawMixedCaseMetadata_PreservesExactCase(DataSourceType dataSourceType, string columnName, string expected)
        {
            var request = CreateColumnRequest(dataSourceType, ColumnDdlOperation.Drop);

            request.ColumnName = columnName;

            Assert.AreEqual(expected, ColumnDdlSqlBuilder.Build(request));
        }

        [TestMethod]
        [TestCategory("Safety")]
        [TestCategory("DdlPreview")]
        [DataRow(DataSourceType.Oracle, "C1", "c1")]
        [DataRow(DataSourceType.PostgreSql, "c1", "C1")]
        [DataRow(DataSourceType.SqlServer, "C1", "[C1]")]
        [DataRow(DataSourceType.MySql, "C1", "`C1`")]
        public void ColumnRename_ToEquivalentName_ReturnsEmpty(DataSourceType dataSourceType, string currentName, string newName)
        {
            var request = CreateColumnRequest(dataSourceType, ColumnDdlOperation.Rename);

            request.ColumnName = currentName;
            request.NewColumnName = newName;

            Assert.AreEqual(string.Empty, ColumnDdlSqlBuilder.Build(request));
        }

        [TestMethod]
        [TestCategory("Safety")]
        [TestCategory("DdlPreview")]
        [DataRow(DataSourceType.Oracle, "A_TEST", "A_TEST")]
        [DataRow(DataSourceType.PostgreSql, "a_test", "a_test")]
        [DataRow(DataSourceType.SqlServer, "A_TEST", "[A_TEST]")]
        [DataRow(DataSourceType.MySql, "A_TEST", "`A_TEST`")]
        public void TableRename_ToEquivalentName_ReturnsEmpty(DataSourceType dataSourceType, string currentName, string newName)
        {
            var request = CreateTableRequest(dataSourceType, TableDdlOperation.Rename);

            request.TableName = currentName;
            request.NewTableName = newName;

            Assert.AreEqual(string.Empty, TableDdlSqlBuilder.Build(request));
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("DdlPreview")]
        public void TableRename_PostgreSql_CaseOnlyChange_PreservesInputCase()
        {
            var request = CreateTableRequest
            (
                DataSourceType.PostgreSql,
                TableDdlOperation.Rename
            );

            request.TableName = "a_test";
            request.NewTableName = "A_test";

            Assert.AreEqual
            (
                "ALTER TABLE \"public\".\"a_test\" RENAME TO \"A_test\";",
                TableDdlSqlBuilder.Build(request)
            );
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("DdlPreview")]
        public void TableRename_Oracle_CaseOnlyChange_PreservesInputCase()
        {
            var request = CreateTableRequest
            (
                DataSourceType.Oracle,
                TableDdlOperation.Rename
            );

            request.TableName = "A_TEST";
            request.NewTableName = "a_test";

            Assert.AreEqual
            (
                "ALTER TABLE \"HR\".\"A_TEST\" RENAME TO \"a_test\";",
                TableDdlSqlBuilder.Build(request)
            );
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("DdlPreview")]
        [DataRow(DataSourceType.Oracle, "MixedTable", "new_table",
                 "ALTER TABLE \"HR\".\"MixedTable\" RENAME TO \"new_table\";")]
        [DataRow(DataSourceType.PostgreSql, "MixedTable", "NEW_TABLE",
                 "ALTER TABLE \"public\".\"MixedTable\" RENAME TO \"NEW_TABLE\";")]
        [DataRow(DataSourceType.SqlServer, "MixedTable", "New_Table",
                 "EXEC [MyDB].[sys].[sp_rename] @objname=N'[dbo].[MixedTable]', @newname=N'New_Table';")]
        [DataRow(DataSourceType.MySql, "MixedTable", "New_Table",
                 "RENAME TABLE `MyDB`.`MixedTable` TO `MyDB`.`New_Table`;")]
        public void TableRename_ExistingAndNewNamesUseDifferentPolicies(DataSourceType dataSourceType, string existingName, string newName, string expected)
        {
            var request = CreateTableRequest(dataSourceType, TableDdlOperation.Rename);

            request.TableName = existingName;
            request.NewTableName = newName;

            Assert.AreEqual(expected, TableDdlSqlBuilder.Build(request));
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("DdlPreview")]
        [DataRow(DataSourceType.Oracle, "MixedColumn", "new_column",
                 "ALTER TABLE \"HR\".\"A_TEST\" RENAME COLUMN \"MixedColumn\" TO \"NEW_COLUMN\";")]
        [DataRow(DataSourceType.PostgreSql, "MixedColumn", "NEW_COLUMN",
                 "ALTER TABLE \"public\".\"A_TEST\" RENAME COLUMN \"MixedColumn\" TO \"new_column\";")]
        [DataRow(DataSourceType.SqlServer, "MixedColumn", "New_Column",
                 "EXEC [MyDB].[sys].[sp_rename] @objname=N'[dbo].[A_TEST].[MixedColumn]', @newname=N'New_Column', @objtype=N'COLUMN';")]
        [DataRow(DataSourceType.MySql, "MixedColumn", "New_Column",
                 "ALTER TABLE `MyDB`.`A_TEST` CHANGE COLUMN `MixedColumn` `New_Column` varchar(20);")]
        public void ColumnRename_ExistingAndNewNamesUseDifferentPolicies(DataSourceType dataSourceType, string existingName, string newName, string expected)
        {
            var request = CreateColumnRequest(dataSourceType, ColumnDdlOperation.Rename);

            request.ColumnName = existingName;
            request.NewColumnName = newName;

            Assert.AreEqual(expected, ColumnDdlSqlBuilder.Build(request));
        }

        private static TableDdlRequest CreateTableRequest(DataSourceType dataSourceType, TableDdlOperation operation)
        {
            return new TableDdlRequest
            {
                DataSourceType = dataSourceType,
                Operation = operation,
                SchemaDatabase = "MyDB",
                SchemaName = dataSourceType == DataSourceType.Oracle ? "HR" : dataSourceType == DataSourceType.SqlServer ? "dbo" : "public",
                TableName = "A_TEST",
                NewTableName = "A_TEST_NEW",
                Comment = "comment"
            };
        }

        private static ColumnDdlRequest CreateColumnRequest(DataSourceType dataSourceType, ColumnDdlOperation operation)
        {
            return new ColumnDdlRequest
            {
                DataSourceType = dataSourceType,
                Operation = operation,
                SchemaDatabase = "MyDB",
                SchemaName = dataSourceType == DataSourceType.Oracle ? "HR" : dataSourceType == DataSourceType.SqlServer ? "dbo" : "public",
                TableName = "A_TEST",
                ColumnName = "C1",
                NewColumnName = "C1_NEW",
                ColumnType = "varchar(20)",
                Comment = "comment"
            };
        }

        private static ColumnAddRequest CreateAddRequest(DataSourceType dataSourceType)
        {
            var typeDefinition = ColumnTypeCatalog.GetDefault(dataSourceType);

            return new ColumnAddRequest
            {
                DataSourceType = dataSourceType,
                SchemaDatabase = "MyDB",
                SchemaName = dataSourceType == DataSourceType.Oracle ? "SYS" : dataSourceType == DataSourceType.SqlServer ? "dbo" : "public",
                TableName = dataSourceType == DataSourceType.PostgreSql ? "AABBCC" : "AABBCC",
                Column = new ColumnDefinition
                {
                    ColumnName = dataSourceType == DataSourceType.Oracle ? "new_column" : dataSourceType == DataSourceType.PostgreSql ? "NEW_COLUMN" : "New_Column",
                    TypeKey = typeDefinition.Key,
                    Parameter1 = typeDefinition.DefaultParameter1,
                    Parameter2 = typeDefinition.DefaultParameter2,
                    NullAllowed = true,
                    DefaultValueKind = ColumnDefaultValueKind.None
                }
            };
        }
    }
}