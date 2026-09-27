using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.DdlPreview;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonQuery.Tests.Core.Database.DdlPreview
{
    [TestClass]
    public sealed class TableDdlSqlBuilderTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DdlPreview")]
        public void Build_WithNullRequest_ReturnsEmptyString()
        {
            Assert.AreEqual(string.Empty, TableDdlSqlBuilder.Build(null));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DdlPreview")]
        public void Build_WithNoneOperation_ReturnsEmptyString()
        {
            var request = CreateRequest(DataSourceType.Oracle, TableDdlOperation.None);

            Assert.AreEqual(string.Empty, TableDdlSqlBuilder.Build(request));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DdlPreview")]
        public void Build_WithUnsupportedDataSource_ReturnsEmptyString()
        {
            var request = CreateRequest(DataSourceType.None, TableDdlOperation.Drop);

            Assert.AreEqual(string.Empty, TableDdlSqlBuilder.Build(request));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DdlPreview")]
        [DataRow(null, "a_test")]
        [DataRow("", "a_test")]
        [DataRow("public", null)]
        [DataRow("public", "")]
        public void Build_WithMissingRequiredTableName_ReturnsEmptyString(string schemaName, string tableName)
        {
            var request = CreateRequest(DataSourceType.PostgreSql, TableDdlOperation.Drop);

            request.SchemaName = schemaName;
            request.TableName = tableName;

            Assert.AreEqual(string.Empty, TableDdlSqlBuilder.Build(request));
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("DdlPreview")]
        [DataRow(DataSourceType.Oracle,
                 "COMMENT ON TABLE \"HR\".\"A_TEST\" IS 'table comment';")]
        [DataRow(DataSourceType.PostgreSql,
                 "COMMENT ON TABLE \"public\".\"a_test\" IS 'table comment';")]
        [DataRow(DataSourceType.SqlServer,
                 "EXEC [MyDB].[sys].[sp_addextendedproperty] @name=N'MS_Description', @value=N'table comment', @level0type=N'SCHEMA', @level0name=N'public', @level1type=N'TABLE', @level1name=N'a_test';")]
        [DataRow(DataSourceType.MySql,
                 "ALTER TABLE `MyDB`.`a_test` COMMENT = 'table comment';")]
        public void BuildComment_ForEachDatabase_ReturnsExpectedSql(DataSourceType dataSourceType, string expected)
        {
            var request = CreateRequest(dataSourceType, TableDdlOperation.Comment);

            request.Comment = "table comment";

            Assert.AreEqual(expected, TableDdlSqlBuilder.Build(request));
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("DdlPreview")]
        [DataRow(DataSourceType.Oracle,
                 "COMMENT ON TABLE \"HR\".\"A_TEST\" IS 'Jason''s table';")]
        [DataRow(DataSourceType.PostgreSql,
                 "COMMENT ON TABLE \"public\".\"a_test\" IS 'Jason''s table';")]
        [DataRow(DataSourceType.SqlServer,
                 "EXEC [MyDB].[sys].[sp_addextendedproperty] @name=N'MS_Description', @value=N'Jason''s table', @level0type=N'SCHEMA', @level0name=N'public', @level1type=N'TABLE', @level1name=N'a_test';")]
        [DataRow(DataSourceType.MySql,
                 "ALTER TABLE `MyDB`.`a_test` COMMENT = 'Jason''s table';")]
        public void BuildComment_WithSingleQuote_EscapesComment(DataSourceType dataSourceType, string expected)
        {
            var request = CreateRequest(dataSourceType, TableDdlOperation.Comment);

            request.Comment = "Jason's table";

            Assert.AreEqual(expected, TableDdlSqlBuilder.Build(request));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DdlPreview")]
        [DataRow(false,
                 "EXEC [MyDB].[sys].[sp_addextendedproperty] @name=N'MS_Description', @value=N'comment', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'a_test';")]
        [DataRow(true,
                 "EXEC [MyDB].[sys].[sp_updateextendedproperty] @name=N'MS_Description', @value=N'comment', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'a_test';")]
        public void BuildComment_SqlServer_UsesAddOrUpdateMode(bool hasExistingComment, string expected)
        {
            var request = CreateRequest(DataSourceType.SqlServer, TableDdlOperation.Comment);

            request.SchemaName = "dbo";
            request.Comment = "comment";
            request.HasExistingSqlServerComment = hasExistingComment;

            Assert.AreEqual(expected, TableDdlSqlBuilder.Build(request));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DdlPreview")]
        public void BuildComment_SqlServer_EscapesObjectNamesInsideStringParameters()
        {
            var request = CreateRequest(DataSourceType.SqlServer, TableDdlOperation.Comment);

            request.SchemaName = "[d'bo]";
            request.TableName = "[a'test]";
            request.Comment = "comment";

            Assert.AreEqual("EXEC [MyDB].[sys].[sp_addextendedproperty] @name=N'MS_Description', @value=N'comment', @level0type=N'SCHEMA', @level0name=N'd''bo', @level1type=N'TABLE', @level1name=N'a''test';",
                            TableDdlSqlBuilder.Build(request));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DdlPreview")]
        public void BuildComment_SqlServer_WithoutDatabaseName_ReturnsEmptyString()
        {
            var request = CreateRequest(DataSourceType.SqlServer, TableDdlOperation.Comment);

            request.SchemaDatabase = string.Empty;

            Assert.AreEqual(string.Empty, TableDdlSqlBuilder.Build(request));
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("DdlPreview")]
        [DataRow(false, false,
                 "DROP TABLE \"HR\".\"A_TEST\";")]
        [DataRow(true, false,
                 "DROP TABLE \"HR\".\"A_TEST\" CASCADE CONSTRAINTS;")]
        [DataRow(false, true,
                 "DROP TABLE \"HR\".\"A_TEST\" PURGE;")]
        [DataRow(true, true,
                 "DROP TABLE \"HR\".\"A_TEST\" CASCADE CONSTRAINTS PURGE;")]
        public void BuildDrop_Oracle_ReturnsExpectedOptions(bool cascadeConstraints, bool purge, string expected)
        {
            var request = CreateRequest(DataSourceType.Oracle, TableDdlOperation.Drop);

            request.OracleCascadeConstraints = cascadeConstraints;
            request.OraclePurge = purge;

            Assert.AreEqual(expected, TableDdlSqlBuilder.Build(request));
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("DdlPreview")]
        [DataRow(false, "DROP TABLE \"public\".\"a_test\";")]
        [DataRow(true, "DROP TABLE \"public\".\"a_test\" CASCADE;")]
        public void BuildDrop_PostgreSql_ReturnsExpectedCascadeOption(bool cascade, string expected)
        {
            var request = CreateRequest(DataSourceType.PostgreSql, TableDdlOperation.Drop);

            request.PostgreSqlDropCascade = cascade;

            Assert.AreEqual(expected, TableDdlSqlBuilder.Build(request));
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("DdlPreview")]
        [DataRow(DataSourceType.SqlServer, "DROP TABLE [MyDB].[public].[a_test];")]
        [DataRow(DataSourceType.MySql, "DROP TABLE `MyDB`.`a_test`;")]
        public void BuildDrop_SqlServerAndMySql_ReturnExpectedSql(DataSourceType dataSourceType, string expected)
        {
            var request = CreateRequest(dataSourceType, TableDdlOperation.Drop);

            Assert.AreEqual(expected, TableDdlSqlBuilder.Build(request));
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("DdlPreview")]
        [DataRow(DataSourceType.Oracle, "ALTER TABLE \"HR\".\"A_TEST\" RENAME TO \"A_TEST_NEW\";")]
        [DataRow(DataSourceType.PostgreSql, "ALTER TABLE \"public\".\"a_test\" RENAME TO \"a_test_new\";")]
        [DataRow(DataSourceType.SqlServer, "EXEC [MyDB].[sys].[sp_rename] @objname=N'[public].[a_test]', @newname=N'a_test_new';")]
        [DataRow(DataSourceType.MySql, "RENAME TABLE `MyDB`.`a_test` TO `MyDB`.`a_test_new`;")]
        public void BuildRename_ForEachDatabase_ReturnsExpectedSql(DataSourceType dataSourceType, string expected)
        {
            var request = CreateRequest(dataSourceType, TableDdlOperation.Rename);

            request.NewTableName = dataSourceType == DataSourceType.Oracle ? "A_TEST_NEW" : "a_test_new";

            Assert.AreEqual(expected, TableDdlSqlBuilder.Build(request));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DdlPreview")]
        public void BuildRename_SqlServer_EscapesStringParameters()
        {
            var request = CreateRequest(DataSourceType.SqlServer, TableDdlOperation.Rename);

            request.SchemaName = "[d'bo]";
            request.TableName = "[a'test]";
            request.NewTableName = "[new'table]";

            Assert.AreEqual("EXEC [MyDB].[sys].[sp_rename] @objname=N'[d''bo].[a''test]', @newname=N'new''table';",
                            TableDdlSqlBuilder.Build(request));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("DdlPreview")]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        [DataRow("a_test")]
        public void BuildRename_WithInvalidNewName_ReturnsEmptyString(string newTableName)
        {
            var request = CreateRequest(DataSourceType.PostgreSql, TableDdlOperation.Rename);

            request.NewTableName = newTableName;

            Assert.AreEqual(string.Empty, TableDdlSqlBuilder.Build(request));
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("DdlPreview")]
        [DataRow(false, "TRUNCATE TABLE \"HR\".\"A_TEST\";")]
        [DataRow(true, "TRUNCATE TABLE \"HR\".\"A_TEST\" REUSE STORAGE;")]
        public void BuildTruncate_Oracle_ReturnsExpectedStorageOption(bool reuseStorage, string expected)
        {
            var request = CreateRequest(DataSourceType.Oracle, TableDdlOperation.Truncate);

            request.OracleReuseStorage = reuseStorage;

            Assert.AreEqual(expected, TableDdlSqlBuilder.Build(request));
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("DdlPreview")]
        [DataRow(false, false, false, "TRUNCATE TABLE \"public\".\"a_test\";")]
        [DataRow(true, false, false, "TRUNCATE TABLE ONLY \"public\".\"a_test\";")]
        [DataRow(false, true, false, "TRUNCATE TABLE \"public\".\"a_test\" RESTART IDENTITY;")]
        [DataRow(false, false, true, "TRUNCATE TABLE \"public\".\"a_test\" CASCADE;")]
        [DataRow(true, true, false, "TRUNCATE TABLE ONLY \"public\".\"a_test\" RESTART IDENTITY;")]
        [DataRow(true, false, true, "TRUNCATE TABLE ONLY \"public\".\"a_test\" CASCADE;")]
        [DataRow(false, true, true, "TRUNCATE TABLE \"public\".\"a_test\" RESTART IDENTITY CASCADE;")]
        [DataRow(true, true, true, "TRUNCATE TABLE ONLY \"public\".\"a_test\" RESTART IDENTITY CASCADE;")]
        public void BuildTruncate_PostgreSql_ReturnsExpectedOptions(bool only, bool restartIdentity, bool cascade, string expected)
        {
            var request = CreateRequest(DataSourceType.PostgreSql, TableDdlOperation.Truncate);

            request.PostgreSqlOnly = only;
            request.PostgreSqlRestartIdentity = restartIdentity;
            request.PostgreSqlTruncateCascade = cascade;

            Assert.AreEqual(expected, TableDdlSqlBuilder.Build(request));
        }

        [TestMethod]
        [TestCategory("Regression")]
        [TestCategory("DdlPreview")]
        [DataRow(DataSourceType.SqlServer, "TRUNCATE TABLE [MyDB].[public].[a_test];")]
        [DataRow(DataSourceType.MySql, "TRUNCATE TABLE `MyDB`.`a_test`;")]
        public void BuildTruncate_SqlServerAndMySql_ReturnExpectedSql(DataSourceType dataSourceType, string expected)
        {
            var request = CreateRequest(dataSourceType, TableDdlOperation.Truncate);

            Assert.AreEqual(expected, TableDdlSqlBuilder.Build(request));
        }

        [TestMethod]
        [TestCategory("Safety")]
        [TestCategory("DdlPreview")]
        [DataRow((int)TableDdlOperation.Comment)]
        [DataRow((int)TableDdlOperation.Drop)]
        [DataRow((int)TableDdlOperation.Rename)]
        [DataRow((int)TableDdlOperation.Truncate)]
        public void Build_DoesNotAppendCommitOrRollback(int operationValue)
        {
            var operation = (TableDdlOperation)operationValue;
            var request = CreateRequest(DataSourceType.PostgreSql, operation);

            request.NewTableName = "a_test_new";
            request.Comment = "comment";

            var sql = TableDdlSqlBuilder.Build(request);

            Assert.DoesNotContain("COMMIT", sql);
            Assert.DoesNotContain("ROLLBACK", sql);
        }

        private static TableDdlRequest CreateRequest(DataSourceType dataSourceType, TableDdlOperation operation)
        {
            return new TableDdlRequest
            {
                DataSourceType = dataSourceType,
                Operation = operation,
                SchemaDatabase = "MyDB",
                SchemaName = dataSourceType == DataSourceType.Oracle ? "HR" : "public",
                TableName = dataSourceType == DataSourceType.Oracle ? "A_TEST" : "a_test",
                NewTableName = "a_test_new",
                Comment = "comment"
            };
        }
    }
}
