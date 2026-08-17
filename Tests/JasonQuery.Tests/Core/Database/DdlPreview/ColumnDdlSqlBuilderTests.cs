using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.DdlPreview;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonQuery.Tests.Core.Database.DdlPreview
{
    [TestClass]
    public sealed class ColumnDdlSqlBuilderTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DdlPreview")]
        public void Build_WithNullRequest_ReturnsEmptyString()
        {
            Assert.AreEqual(string.Empty, ColumnDdlSqlBuilder.Build(null));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DdlPreview")]
        public void Build_WithNoneOperation_ReturnsEmptyString()
        {
            var request = CreateRequest(DataSourceType.Oracle, ColumnDdlOperation.None);

            Assert.AreEqual(string.Empty, ColumnDdlSqlBuilder.Build(request));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DdlPreview")]
        public void Build_WithUnsupportedDataSource_ReturnsEmptyString()
        {
            var request = CreateRequest(DataSourceType.None, ColumnDdlOperation.Drop);

            Assert.AreEqual(string.Empty, ColumnDdlSqlBuilder.Build(request));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DdlPreview")]
        [DataRow(null, "c1")]
        [DataRow("", "c1")]
        [DataRow("a_test", null)]
        [DataRow("a_test", "")]
        public void Build_WithMissingRequiredColumnName_ReturnsEmptyString(string tableName, string columnName)
        {
            var request = CreateRequest(DataSourceType.PostgreSql, ColumnDdlOperation.Drop);

            request.TableName = tableName;
            request.ColumnName = columnName;

            Assert.AreEqual(string.Empty, ColumnDdlSqlBuilder.Build(request));
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("DdlPreview")]
        [DataRow(DataSourceType.Oracle,
                 "COMMENT ON COLUMN \"HR\".\"A_TEST\".\"C1\" IS 'column comment';")]
        [DataRow(DataSourceType.PostgreSql,
                 "COMMENT ON COLUMN \"public\".\"a_test\".\"c1\" IS 'column comment';")]
        [DataRow(DataSourceType.SqlServer,
                 "EXEC [MyDB].[sys].[sp_addextendedproperty] @name=N'MS_Description', @value=N'column comment', @level0type=N'SCHEMA', @level0name=N'public', @level1type=N'TABLE', @level1name=N'a_test', @level2type=N'COLUMN', @level2name=N'c1';")]
        [DataRow(DataSourceType.MySql,
                 "ALTER TABLE `MyDB`.`a_test` CHANGE COLUMN `c1` `c1` varchar(20) COMMENT 'column comment';")]
        public void BuildComment_ForEachDatabase_ReturnsExpectedSql(DataSourceType dataSourceType, string expected)
        {
            var request = CreateRequest(dataSourceType, ColumnDdlOperation.Comment);

            request.Comment = "column comment";

            Assert.AreEqual(expected, ColumnDdlSqlBuilder.Build(request));
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("DdlPreview")]
        [DataRow(DataSourceType.Oracle,
                 "COMMENT ON COLUMN \"HR\".\"A_TEST\".\"C1\" IS 'Jason''s column';")]
        [DataRow(DataSourceType.PostgreSql,
                 "COMMENT ON COLUMN \"public\".\"a_test\".\"c1\" IS 'Jason''s column';")]
        [DataRow(DataSourceType.SqlServer,
                 "EXEC [MyDB].[sys].[sp_addextendedproperty] @name=N'MS_Description', @value=N'Jason''s column', @level0type=N'SCHEMA', @level0name=N'public', @level1type=N'TABLE', @level1name=N'a_test', @level2type=N'COLUMN', @level2name=N'c1';")]
        [DataRow(DataSourceType.MySql,
                 "ALTER TABLE `MyDB`.`a_test` CHANGE COLUMN `c1` `c1` varchar(20) COMMENT 'Jason''s column';")]
        public void BuildComment_WithSingleQuote_EscapesComment(DataSourceType dataSourceType, string expected)
        {
            var request = CreateRequest(dataSourceType, ColumnDdlOperation.Comment);

            request.Comment = "Jason's column";

            Assert.AreEqual(expected, ColumnDdlSqlBuilder.Build(request));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DdlPreview")]
        [DataRow(false,
                 "EXEC [MyDB].[sys].[sp_addextendedproperty] @name=N'MS_Description', @value=N'comment', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'a_test', @level2type=N'COLUMN', @level2name=N'c1';")]
        [DataRow(true,
                 "EXEC [MyDB].[sys].[sp_updateextendedproperty] @name=N'MS_Description', @value=N'comment', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'a_test', @level2type=N'COLUMN', @level2name=N'c1';")]
        public void BuildComment_SqlServer_UsesAddOrUpdateMode(bool hasExistingComment, string expected)
        {
            var request = CreateRequest(DataSourceType.SqlServer, ColumnDdlOperation.Comment);

            request.SchemaName = "dbo";
            request.HasExistingSqlServerComment = hasExistingComment;

            Assert.AreEqual(expected, ColumnDdlSqlBuilder.Build(request));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DdlPreview")]
        public void BuildComment_SqlServer_EscapesObjectNamesInsideStringParameters()
        {
            var request = CreateRequest(DataSourceType.SqlServer, ColumnDdlOperation.Comment);

            request.SchemaName = "[d'bo]";
            request.TableName = "[a'test]";
            request.ColumnName = "[c'1]";

            Assert.AreEqual("EXEC [MyDB].[sys].[sp_addextendedproperty] @name=N'MS_Description', @value=N'comment', @level0type=N'SCHEMA', @level0name=N'd''bo', @level1type=N'TABLE', @level1name=N'a''test', @level2type=N'COLUMN', @level2name=N'c''1';",
                            ColumnDdlSqlBuilder.Build(request));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DdlPreview")]
        [DataRow("")]
        [DataRow(" ")]
        public void BuildComment_SqlServer_WithoutDatabaseName_ReturnsEmptyString(string databaseName)
        {
            var request = CreateRequest(DataSourceType.SqlServer, ColumnDdlOperation.Comment);

            request.SchemaDatabase = databaseName;

            Assert.AreEqual(string.Empty, ColumnDdlSqlBuilder.Build(request));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DdlPreview")]
        public void BuildComment_MySql_NormalizesColumnTypeSpacing()
        {
            var request = CreateRequest(DataSourceType.MySql, ColumnDdlOperation.Comment);

            request.ColumnType = "varchar (20)";

            Assert.AreEqual("ALTER TABLE `MyDB`.`a_test` CHANGE COLUMN `c1` `c1` varchar(20) COMMENT 'comment';",
                            ColumnDdlSqlBuilder.Build(request));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DdlPreview")]
        public void BuildComment_MySql_WithoutColumnType_ReturnsEmptyString()
        {
            var request = CreateRequest(DataSourceType.MySql, ColumnDdlOperation.Comment);

            request.ColumnType = string.Empty;

            Assert.AreEqual(string.Empty, ColumnDdlSqlBuilder.Build(request));
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("DdlPreview")]
        [DataRow(DataSourceType.Oracle,
                 "ALTER TABLE \"HR\".\"A_TEST\" DROP COLUMN \"C1\";")]
        [DataRow(DataSourceType.PostgreSql,
                 "ALTER TABLE \"public\".\"a_test\" DROP COLUMN \"c1\";")]
        [DataRow(DataSourceType.SqlServer,
                 "ALTER TABLE [MyDB].[public].[a_test] DROP COLUMN [c1];")]
        [DataRow(DataSourceType.MySql,
                 "ALTER TABLE `MyDB`.`a_test` DROP COLUMN `c1`;")]
        public void BuildDrop_ForEachDatabase_ReturnsExpectedSql(DataSourceType dataSourceType, string expected)
        {
            var request = CreateRequest(dataSourceType, ColumnDdlOperation.Drop);

            Assert.AreEqual(expected, ColumnDdlSqlBuilder.Build(request));
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("DdlPreview")]
        [DataRow(DataSourceType.Oracle,
                 "ALTER TABLE \"HR\".\"A_TEST\" RENAME COLUMN \"C1\" TO \"C1_NEW\";")]
        [DataRow(DataSourceType.PostgreSql,
                 "ALTER TABLE \"public\".\"a_test\" RENAME COLUMN \"c1\" TO \"c1_new\";")]
        [DataRow(DataSourceType.SqlServer,
                 "EXEC [MyDB].[sys].[sp_rename] @objname=N'[public].[a_test].[c1]', @newname=N'c1_new', @objtype=N'COLUMN';")]
        [DataRow(DataSourceType.MySql,
                 "ALTER TABLE `MyDB`.`a_test` CHANGE COLUMN `c1` `c1_new` varchar(20);")]
        public void BuildRename_ForEachDatabase_ReturnsExpectedSql(DataSourceType dataSourceType, string expected)
        {
            var request = CreateRequest(dataSourceType, ColumnDdlOperation.Rename);

            request.NewColumnName = dataSourceType == DataSourceType.Oracle ? "C1_NEW" : "c1_new";

            Assert.AreEqual(expected, ColumnDdlSqlBuilder.Build(request));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DdlPreview")]
        public void BuildRename_SqlServer_EscapesStringParameters()
        {
            var request = CreateRequest(DataSourceType.SqlServer, ColumnDdlOperation.Rename);

            request.SchemaName = "[d'bo]";
            request.TableName = "[a'test]";
            request.ColumnName = "[c'1]";
            request.NewColumnName = "[c'2]";

            Assert.AreEqual("EXEC [MyDB].[sys].[sp_rename] @objname=N'[d''bo].[a''test].[c''1]', @newname=N'c''2', @objtype=N'COLUMN';",
                            ColumnDdlSqlBuilder.Build(request));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DdlPreview")]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        [DataRow("c1")]
        public void BuildRename_WithInvalidNewName_ReturnsEmptyString(string newColumnName)
        {
            var request = CreateRequest(DataSourceType.PostgreSql, ColumnDdlOperation.Rename);

            request.NewColumnName = newColumnName;

            Assert.AreEqual(string.Empty, ColumnDdlSqlBuilder.Build(request));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DdlPreview")]
        public void BuildRename_MySql_WithoutColumnType_ReturnsEmptyString()
        {
            var request = CreateRequest(DataSourceType.MySql, ColumnDdlOperation.Rename);

            request.ColumnType = string.Empty;

            Assert.AreEqual(string.Empty, ColumnDdlSqlBuilder.Build(request));
        }

        [TestMethod]
        [TestCategory("Safety")]
        [TestCategory("DdlPreview")]
        [DataRow((int)ColumnDdlOperation.Comment)]
        [DataRow((int)ColumnDdlOperation.Drop)]
        [DataRow((int)ColumnDdlOperation.Rename)]
        public void Build_DoesNotAppendCommitOrRollback(int operationValue)
        {
            var operation = (ColumnDdlOperation)operationValue;
            var request = CreateRequest(DataSourceType.PostgreSql, operation);

            request.NewColumnName = "c1_new";
            request.Comment = "comment";

            var sql = ColumnDdlSqlBuilder.Build(request);

            Assert.IsFalse(sql.Contains("COMMIT"));
            Assert.IsFalse(sql.Contains("ROLLBACK"));
        }

        private static ColumnDdlRequest CreateRequest(DataSourceType dataSourceType, ColumnDdlOperation operation)
        {
            return new ColumnDdlRequest
            {
                DataSourceType = dataSourceType,
                Operation = operation,
                SchemaDatabase = "MyDB",
                SchemaName = dataSourceType == DataSourceType.Oracle ? "HR" : "public",
                TableName = dataSourceType == DataSourceType.Oracle ? "A_TEST" : "a_test",
                ColumnName = dataSourceType == DataSourceType.Oracle ? "C1" : "c1",
                NewColumnName = "c1_new",
                ColumnType = "varchar(20)",
                Comment = "comment"
            };
        }
    }
}